using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ImageProcessing;
using Core.Models;
using Core.ViewModels.Fai;
using HalconDotNet;
using HKCameraDev.Core.ViewModels.Camera;
using LJX8000.Core.ViewModels.Controller;
using MaterialDesignThemes.Wpf;
using PLCCommunication.Core.ViewModels;
using PropertyChanged;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Application
{
    /// <summary>
    /// Holds the global states of the application
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        /// <summary>
        /// Static instance for xaml to bind to 
        /// </summary>
        public static ApplicationViewModel Instance => _instance;

        private static ApplicationViewModel _instance = new ApplicationViewModel();

        private int _maxRoutineLogs = 100;

        private HTuple _shapeModel2D, _shapeModel3D;

    

        private IMeasurementProcedure2D _procedure2D;

        private IMeasurementProcedure3D _procedure3D;
        
        private object _lockerOfRoutineMessageList = new object();
        private SocketType _socketToDisplay2D;
        private SocketType _socketToDisplay3D;
        private List<string> _findLineParam2DNames;


        public Dictionary<string, ThreadSafeFixedSizeQueue<MeasurementResult2D>> ResultQueues2D { get; set; } =
            new Dictionary<string, ThreadSafeFixedSizeQueue<MeasurementResult2D>>()
            {
                ["Left"] = new ThreadSafeFixedSizeQueue<MeasurementResult2D>(2),
                ["Right"] = new ThreadSafeFixedSizeQueue<MeasurementResult2D>(2)
            };

        private ApplicationViewModel()
        {
            CurrentApplicationPage = ApplicationPageType.Home;
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
            BindingOperations.EnableCollectionSynchronization(RoutineMessageList, _lockerOfRoutineMessageList);

            SetupServer();
            
            SetupCameras();
            
            SetupLaserControllers();

            LoadShapeModels();

            LoadFaiItems();

            LoadFindLineParams2D();

            InitCommands();
        }

        private void LoadFindLineParams2D()
        {
            FindLineParams2D = AutoSerializableHelper.LoadAutoSerializables<FindLineParam>(_findLineParam2DNames, DirectoryConstants.FindLineParamsConfigDir).ToList();
        }

        private void InitCommands()
        {
            SwitchSocketView2DCommand = new RelayCommand(SwitchSocketView2D);
            ManualTest2DCommand = new SimpleCommand(o=> RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyRunningManualTest2D, async ()=> await RunManualTest2D()), o=>!Server.IsAutoRunning);
        }

        private async Task RunManualTest2D()
        {
            // Preserve images for next manual run
            var images = ResultToDisplay2D.Images;
            var result2D =
                await Task.Run(() => _procedure2D.Execute(images, FindLineParams2D.ToDict()));
            result2D.Images = images;
            
            ResultToDisplay2D = result2D;
        }

        private void SwitchSocketView2D()
        {
            SocketToDisplay2D = _socketToDisplay2D == SocketType.Left ? SocketType.Right : SocketType.Left;
        }

        private void SetupServer()
        {
            Server = new AlcServerViewModel();
            Server.AutoRunStartRequested += () => LogRoutine("Auto-mode-start requested from plc");
            Server.AutoRunStopRequested += () => LogRoutine("Auto-mode-stop requested from plc");
            Server.InitRequested += () => LogRoutine("Init requested from plc");
            Server.ClientHooked += socket => LogRoutine(socket.RemoteEndPoint + " is hooked");
            Server.CustomCommandReceived += PlcCustomCommandHandler;
        }

        private void PlcCustomCommandHandler(int commandId)
        {
            switch (commandId)
            {
                case PlcMessagePackConstants.CommandIdLeftSocketArrived:
                    CurrentArrivedSocket = SocketType.Left;
                    break;
                case PlcMessagePackConstants.CommandIdRightSocketArrived:
                    CurrentArrivedSocket = SocketType.Right;
                    break;
            }
      }

        private void SetupCameras()
        {
            HKCameraManager.ScannedForAttachedCameras();
            Camera = HKCameraManager.AttachedCameras[0];
            Camera.ImageBatchSize = 5;
            Camera.BatchImageReceived += ProcessImages2DFireForget;
        }
        
        private void SetupLaserControllers()
        {
            ControllerManager.AttachedControllers = new List<ControllerViewModel>(NameConstants.ControllerNames.Select(name =>
                new ControllerViewModel()
                {
                    Name = name,
                    RowsPerRun = 1600,
                    ProfileCountEachFetch = 100,
                    NumImagesPerRun = 2
                }).OrderBy(c => c.Name));
            ControllerManager.Init();

            for (int i = 0; i < NameConstants.ControllerNames.Count; i++)
            {
                var index = i;
                ControllerManager.AttachedControllers[i].RunFinished +=
                    (heightImages, intensityImages) =>
                    {
                        ImageInputs3DLeft[index] = heightImages[0];
                        ImageInputs3DRight[index] = heightImages[1];
                    };
            }

            ControllerManager.AttachedControllers[NameConstants.ControllerNames.Count - 1].RunFinished +=
                (heightImages, intensityImages) => OnLastLaserFinished();
        }

        private void OnLastLaserFinished()
        {
            Summarize();
            UpdateResultsToDisplay();
        }

        private void UpdateResultsToDisplay()
        {
            // 2D
            ResultToDisplay2D = SocketToDisplay2D == SocketType.Left ? Result2DLeft : Result2DRight;
            
            // 3D
            ResultToDisplay3D = SocketToDisplay3D == SocketType.Left ? Result3DLeft : Result3DRight;
        }

        private void LoadFaiItems()
        {
            FaiItemsLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames, DirectoryConstants.LeftFaiConfigDir)
                .ToList();
            FaiItemsRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames, DirectoryConstants.RightFaiConfigDir)
                .ToList();
        }

        private void LoadShapeModels()
        {
            HOperatorSet.ReadShapeModel(PathConstants.ShapeModelPath2D, out _shapeModel2D);
            HOperatorSet.ReadShapeModel(PathConstants.ShapeModelPath3D, out _shapeModel3D);
        }

       

        /// <summary>
        /// 统计一次测量结果
        /// </summary>
        private void Summarize()
        {
            Result3DLeft = MeasureImages3D(ImageInputs3DLeft, _shapeModel3D);
            Result3DRight = MeasureImages3D(ImageInputs3DRight, _shapeModel3D);
            Result2DLeft = ResultQueues2D["Left"].DequeueThreadSafe();
            Result2DRight = ResultQueues2D["Right"].DequeueThreadSafe();

            var faiResultDictLeft = MergeResultDicts(Result3DLeft.FaiResults, Result2DLeft.FaiResults);
            var faiResultDictRight = MergeResultDicts(Result3DRight.FaiResults, Result2DRight.FaiResults);

            UpdateFaiItems(FaiItemsLeft, faiResultDictLeft);
            UpdateFaiItems(FaiItemsRight, faiResultDictRight);

            LeftDecision = GetDecision(Result3DLeft.ItemExists, FaiItemsLeft);
            RightDecision = GetDecision(Result3DRight.ItemExists, FaiItemsRight);
        }



        /// <summary>
        /// Decide whether the item is missing, passed or rejected
        /// </summary>
        /// <param name="itemExists"></param>
        /// <param name="faiItems"></param>
        /// <returns></returns>
        private MeasurementDecision GetDecision(bool itemExists, List<FaiItem> faiItems)
        {
            if (!itemExists) return MeasurementDecision.Empty;
            return faiItems.All(item => item.Passed) ? MeasurementDecision.Passed : MeasurementDecision.Rejected;
        }

        /// <summary>
        /// Update ValueUnbiased of fai-items from dictionaries
        /// </summary>
        /// <param name="faiItems"></param>
        /// <param name="faiResultDict"></param>
        /// <exception cref="ArgumentException"></exception>
        private void UpdateFaiItems(List<FaiItem> faiItems, Dictionary<string, double> faiResultDict)
        {
            if (faiItems.Count != faiResultDict.Count)
                throw new ArgumentException("faiItems.Count!=faiResultDict.Count");
            foreach (var faiResult in faiResultDict)
            {
                faiItems.ByName(faiResult.Key).ValueUnbiased = faiResult.Value;
            }
        }

        /// <summary>
        /// Concat 2 dictionaries to the first one
        /// </summary>
        /// <param name="resultsA"></param>
        /// <param name="resultsB"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Dictionary<string, double> MergeResultDicts(Dictionary<string, double> resultsA,
            Dictionary<string, double> resultsB)
        {
            foreach (var result in resultsB)
            {
                resultsA[result.Key] = result.Value;
            }

            return resultsA;
        }

        private MeasurementResult3D MeasureImages3D(List<HImage> imageInputs, HTuple shapeModel3D)
        {
            throw new NotImplementedException();
        }


        private void ProcessImages2DFireForget(List<HImage> images)
        {
            //TODO: replace dummy find-line-params
            Task.Run(() =>
            {
                if (CurrentArrivedSocket == SocketType.Left)
                {
                    var result = _procedure2D.Execute(images, FindLineParams2D.ToDict());
                    result.Images = images;
                    ResultQueues2D["Left"].EnqueueThreadSafe(result);
                }
                else
                {
                    var result = _procedure2D.Execute(images, FindLineParams2D.ToDict());
                    result.Images = images;
                    ResultQueues2D["Right"].EnqueueThreadSafe(result);
                }
            });
        }
        
        public void LogRoutine(string message)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                lock (_lockerOfRoutineMessageList)
                {
                    RoutineMessageList.Add(DateTime.Now.ToString("T") + " > " + message);
                    // Remove some messages if overflows
                    if (RoutineMessageList.Count > _maxRoutineLogs)
                    {
                        RoutineMessageList =
                            new ObservableCollection<string>(RoutineMessageList.Skip(_maxRoutineLogs / 3));
                    }
                }
            });
        }

        /// <summary>
        /// Current image to display in Vision2DView
        /// </summary>

        #region Properties

        public List<FindLineParam> FindLineParams2D { get; set; }

        public bool IsBusyRunningManualTest2D { get; set; }

        public HImage ImageToDisplay2D => ResultToDisplay2D.Images?[ImageIndexToDisplay2D];

        /// <summary>
        /// Index of current displayed image in Vision2DView
        /// </summary>
       
        [AlsoNotifyFor(nameof(ImageToDisplay2D))]
        public int ImageIndexToDisplay2D { get; set; } = 0;

        /// <summary>
        /// 2D image result from left or right socket
        /// </summary>
        [AlsoNotifyFor(nameof(ImageToDisplay2D))]
        public MeasurementResult2D ResultToDisplay2D { get; set; }

        /// <summary>
        /// Current 
        /// </summary>
        public SocketType SocketToDisplay2D
        {
            get { return _socketToDisplay2D; }
            set
            {
                _socketToDisplay2D = value;
                ResultToDisplay2D = value == SocketType.Left ? Result2DLeft : Result2DRight;
            }
        }


        public MeasurementResult2D Result2DRight { get; set; }

        public MeasurementResult2D Result2DLeft { get; set; }


        public SocketType SocketToDisplay3D
        {
            get { return _socketToDisplay3D; }
            set
            {
                _socketToDisplay3D = value;
                ResultToDisplay3D = value == SocketType.Left ? Result3DLeft : Result3DRight;
            }
        }

        public MeasurementResult3D ResultToDisplay3D { get; set; }

        public MeasurementResult3D Result3DRight { get; set; }

        public MeasurementResult3D Result3DLeft { get; set; }
        
        public List<FaiItem> FaiItemsLeft { get; set; }

        public SocketType CurrentArrivedSocket { get; set; }
        public MeasurementDecision LeftDecision { get; set; }
        public MeasurementDecision RightDecision { get; set; }

        public List<FaiItem> FaiItemsRight { get; set; }

        public List<HImage> ImageInputs3DLeft { get; set; } = new List<HImage>() {null, null, null};
        public List<HImage> ImageInputs3DRight { get; set; } = new List<HImage>() {null, null, null};

        /// <summary>
        /// Switch 2D view between left and right socket
        /// </summary>
        public ICommand SwitchSocketView2DCommand { get; set; }

        /// <summary>
        /// Manually run test on the last 2D images
        /// </summary>
        public ICommand ManualTest2DCommand { get; set; }

        /// <summary>
        /// Current application page
        /// </summary>
        public ApplicationPageType CurrentApplicationPage { get; set; }

        /// <summary>
        /// Message queue for global UI logging
        /// </summary>
        public ISnackbarMessageQueue MessageQueue { get; set; }


        /// <summary>
        /// 2D camera object
        /// </summary>
        public CameraViewModel Camera { get; set; }


        /// <summary>
        /// Message list for routine logging
        /// </summary>
        public ObservableCollection<string> RoutineMessageList { get; set; } = new ObservableCollection<string>();
        
        public AlcServerViewModel Server { get; set; }


        #endregion

    



     
    }
}