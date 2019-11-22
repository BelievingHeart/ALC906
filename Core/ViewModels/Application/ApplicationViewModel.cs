using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using Core.Enums;
using Core.ImageProcessing;
using Core.Models;
using Core.ViewModels.Fai;
using HalconDotNet;
using HKCameraDev.Core.ViewModels.Camera;
using LJX8000.Core.ViewModels.Controller;
using MaterialDesignThemes.Wpf;
using PLCCommunication.Core.ViewModels;
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

        private List<string> _faiItemNames = new List<string>()
        {
            "16.1", "16.2", 
            "17.1", "17.2", "17.3", "17.4",
            "18.E", "18.M",
            "19.1", "19.2", "19.3", "19.4", "19.5", "19.6", "19.7", "19.8", 
            "20.1", "20.2", "20.3", "20.4",  
            "21", "22"
        };

        /// <summary>
        /// Names of all LJX-8000A controllers
        /// </summary>
        private List<string> _controllerNames = new List<string>()
        {
            "192.168.0.1@24691", "192.168.0.2@24691", "192.168.0.3@24691"
        };

        private IMeasurementProcedure2D _procedure2D;

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
        }
        
        private void SetupServer()
        {
            Server = new AlcServerViewModel();
            Server.AutoRunStartRequested += () => LogRoutine("Auto-mode-start requested from plc");
            Server.AutoRunStopRequested += () => LogRoutine("Auto-mode-stop requested from plc");
            Server.InitRequested += () => LogRoutine("Init requested from plc");
            Server.ClientHooked += socket => LogRoutine(socket.RemoteEndPoint + " is hooked");
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
            ControllerManager.AttachedControllers = new List<ControllerViewModel>(_controllerNames.Select(name =>
                new ControllerViewModel()
                {
                    Name = name,
                    RowsPerRun = 1600,
                    ProfileCountEachFetch = 100,
                    NumImagesPerRun = 2
                }).OrderBy(c => c.Name));
            ControllerManager.Init();

            for (int i = 0; i < _controllerNames.Count; i++)
            {
                var index = i;
                ControllerManager.AttachedControllers[i].RunFinished +=
                    (heightImages, intensityImages) =>
                    {
                        ImageInputs3DLeft[index] = heightImages[0];
                        ImageInputs3DRight[index] = heightImages[1];
                    };
            }

            ControllerManager.AttachedControllers[_controllerNames.Count - 1].RunFinished +=
                (heightImages, intensityImages) => Summarize();
        }

        private void LoadFaiItems()
        {
            FaiItemsLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_controllerNames, LeftFaiConfigDir)
                .ToList();
            FaiItemsRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_controllerNames, RightFaiConfigDir)
                .ToList();
        }

        private void LoadShapeModels()
        {
            HOperatorSet.ReadShapeModel(_shapeModelPath2D, out _shapeModel2D);
            HOperatorSet.ReadShapeModel(_shapeModelPath3D, out _shapeModel3D);
        }

       

        /// <summary>
        /// 统计一次测量结果
        /// </summary>
        private void Summarize()
        {
            var result3DLeft = MeasureImages3D(ImageInputs3DLeft, _shapeModel3D);
            var result3DRight = MeasureImages3D(ImageInputs3DRight, _shapeModel3D);
            var result2DLeft = ResultQueues2D["Left"].DequeueThreadSafe();
            var result2DRight = ResultQueues2D["Right"].DequeueThreadSafe();

            var faiResultDictLeft = MergeResultDicts(result3DLeft.FaiResults, result2DLeft.FaiResults);
            var faiResultDictRight = MergeResultDicts(result3DRight.FaiResults, result2DRight.FaiResults);

            UpdateFaiItems(FaiItemsLeft, faiResultDictLeft);
            UpdateFaiItems(FaiItemsRight, faiResultDictRight);

            LeftDecision = GetDecision(result3DLeft.ItemExists, FaiItemsLeft);
            RightDecision = GetDecision(result3DRight.ItemExists, FaiItemsRight);
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
                var result = _procedure2D.Execute(images, new Dictionary<string, FindLineParam>());
                if (result.IsLeft)
                {
                    ResultQueues2D["Left"].EnqueueThreadSafe(result);
                }
                else
                {
                    ResultQueues2D["Right"].EnqueueThreadSafe(result);
                }
            });
        }

        public List<FaiItem> FaiItemsLeft { get; set; }

        public MeasurementDecision LeftDecision { get; set; }
        public MeasurementDecision RightDecision { get; set; }

        public List<FaiItem> FaiItemsRight { get; set; }

        public List<HImage> ImageInputs3DLeft { get; set; } = new List<HImage>() {null, null, null};
        public List<HImage> ImageInputs3DRight { get; set; } = new List<HImage>() {null, null, null};

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

        private object _lockerOfRoutineMessageList = new object();
        private readonly string _shapeModelPath2D, _shapeModelPath3D;

        private static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        private static string LeftFaiConfigDir => Path.Combine(FaiConfigDir, "Left");
        private static string RightFaiConfigDir => Path.Combine(FaiConfigDir, "Right");

        public AlcServerViewModel Server { get; set; }


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
    }
}