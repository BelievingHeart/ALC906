using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ImageProcessing;
using Core.IoC.Loggers;
using Core.Models;
using Core.Stubs;
using Core.ViewModels.Bin;
using Core.ViewModels.Fai;
using Core.ViewModels.Plc;
using HalconDotNet;
using HKCameraDev.Core.IoC.Interface;
using HKCameraDev.Core.ViewModels.Camera;
using LJX8000.Core.ViewModels.Controller;
using MaterialDesignThemes.Wpf;
using PLCCommunication.Core.ViewModels;
using PropertyChanged;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;
using CameraTriggerSourceType = HKCameraDev.Core.Enums.CameraTriggerSourceType;
using SocketType = Core.Enums.SocketType;

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
        public static ApplicationViewModel Instance
        {
            get { return _instance; }
        }

        private static ApplicationViewModel _instance;

        private readonly Object _lockerOfLaserImageBuffers = new Object();

        /// <summary>
        /// Key=ControllerName, Value=CurrentIndexOfSocket
        /// CurrentIndexOfSocket: 0 for right, 1 for left
        /// </summary>
        private readonly Dictionary<string, int> _laserRunIndex = new Dictionary<string, int>();

        /// <summary>
        /// Key=ControllerName, Value=ImagesInOneRound
        /// FirstImage=HeightImageFromRightSocket, SecondImage=HeightImageFromLeftSocket
        /// </summary>
        private readonly Dictionary<string, List<HImage>> _laserImageBuffers = new Dictionary<string, List<HImage>>();

        /// <summary>
        /// Keep results of left and right socket in memory
        /// for displaying purpose when switching between socket views
        /// </summary>
        private readonly List<MeasurementResult2D> _results2D = new List<MeasurementResult2D>()
        {
            null, null
        };

        /// <summary>
        /// Keep results of left and right socket in memory
        /// for displaying purpose when switching between socket views
        /// </summary>
        private readonly List<MeasurementResult3D> _results3D = new List<MeasurementResult3D>() {null, null};

        private int _maxRoutineLogs = 100;

        private HTuple _shapeModel2D, _shapeModel3D;

        private readonly CsvSerializer.CsvSerializer _serializerLeft =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Left"));

        private readonly CsvSerializer.CsvSerializer _serializerRight =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Right"));

        private readonly IMeasurementProcedure2D _procedure2D = new MeasurementProcedure2DStub();

        private readonly IMeasurementProcedure3D _procedure3D = new MeasurementProcedure3DStub();

        private readonly object _lockerOfRoutineMessageList = new object();
        private readonly object _lockerOfPlcMessageList = new object();
        private SocketType _socketToDisplay2D;
        private SocketType _socketToDisplay3D;

        private readonly List<string> _findLineParam2DNames = new List<string>()
        {
            //TODO: make this meaningful
            "test1", "test2"
        };


        private List<FaiItem> _faiItems2DLeft;
        private List<FaiItem> _faiItems2DRight;
        private List<FaiItem> _faiItems3DLeft;
        private List<FaiItem> _faiItems3DRight;


        private readonly Dictionary<SocketType, Queue<MeasurementResult2D>> _resultQueues2D =
            new Dictionary<SocketType, Queue<MeasurementResult2D>>()
            {
                [SocketType.Left] = new Queue<MeasurementResult2D>(2),
                [SocketType.Right] = new Queue<MeasurementResult2D>(2)
            };

        private readonly Object _lockerOfResultQueues2D = new Object();
        private bool _isAllControllersHighSpeedConnected;

        protected ApplicationViewModel()
        {
            InitContainer();
            AutoResetInterval = 10000;
            CurrentApplicationPage = ApplicationPageType.Home;
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
            PlcMessageList = new ObservableCollection<LoggingMessageItem>();
            BindingOperations.EnableCollectionSynchronization(RoutineMessageList, _lockerOfRoutineMessageList);
            BindingOperations.EnableCollectionSynchronization(PlcMessageList, _lockerOfPlcMessageList);


            LoadShapeModels();

            LoadFaiItems();

            LoadFindLineParams2D();

            LoadProductionLineRecords();

            ClearLaserImagesForNewRound();

            InitCommands();
        }

        /// <summary>
        /// Load records of the production line
        /// </summary>
        private void LoadProductionLineRecords()
        {
            Bins = AutoSerializableHelper.LoadAutoSerializable<BinListViewModel>("Bins",
                DirectoryConstants.ProductionLineRecordDir);
        }

        private void ClearLaserImagesForNewRound()
        {
            lock (_lockerOfLaserImageBuffers)
            {
                // Reset run index
                foreach (var name in NameConstants.ControllerNames)
                {
                    _laserRunIndex[name] = 0;
                }

                // Reset image buffers
                foreach (var name in NameConstants.ControllerNames)
                {
                    _laserImageBuffers[name] = new List<HImage>()
                    {
                        null, null
                    };
                }

                // Clear profile buffers
                // 机台RESET回程时会迷之扫描
                foreach (var controller in ControllerManager.AttachedControllers)
                {
                    controller.ClearBuffer();
                }
            }
        }

        private void InitContainer()
        {
            HKCameraDev.Core.IoC.IoC.Kernel.Bind<IHKCameraLibLogger>().ToConstant(new CameraMessageLogger());
        }

        private void LoadFindLineParams2D()
        {
            FindLineParams2D = AutoSerializableHelper
                .LoadAutoSerializables<FindLineParam>(_findLineParam2DNames, DirectoryConstants.FindLineParamsConfigDir)
                .ToList();
        }

        private void InitCommands()
        {
            SwitchSocketView2DCommand = new RelayCommand(SwitchSocketView2D);
            SwitchSocketView3DCommand = new RelayCommand(SwitchSocketView3D);
            ManualTest2DCommand =
                new SimpleCommand(
                    o => RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyRunningManualTest2D,
                        async () => await RunManualTest2D()), o => !Server.IsAutoRunning);
        }

        private void SwitchSocketView3D()
        {
            SocketToDisplay3D = _socketToDisplay3D == SocketType.Left ? SocketType.Right : SocketType.Left;
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
            Server = new AlcServerViewModel()
            {
                IpAddress = IPAddress.Parse("192.168.100.100"),
                Port = 4000
            };
            Server.AutoRunStartRequested += () => LogPlcMessage("Auto-mode-start requested from plc");
            Server.AutoRunStopRequested += () => LogPlcMessage("Auto-mode-stop requested from plc");
            Server.InitRequested += () => LogPlcMessage("Init requested from plc");
            Server.ClientHooked += OnPlcHooked;
            Server.CustomCommandReceived += PlcCustomCommandHandler;
            Server.PlcInitFinished += OnPlcInitFinished;
            Server.NewRoundStarted += OnNewRoundStarted;
        }

        private void OnPlcHooked(Socket socket)
        {
            LogPlcMessage(socket.RemoteEndPoint + " is hooked");
        }

        private void OnNewRoundStarted()
        {
            LogPlcMessage("Received start permission from ALC");
            ClearLaserImagesForNewRound();
            Rearrange2DImageQueues();
        }

        /// <summary>
        /// When starting a new round
        /// the last one in the queue must be the one
        /// that will be concluded with 3D results in the new round
        /// </summary>
        private void Rearrange2DImageQueues()
        {
            lock (_lockerOfResultQueues2D)
            {
                var lastInLeft = _resultQueues2D[SocketType.Left].LastOrDefault();
                var isFirstRoundAfterReset = lastInLeft == null;
                if (isFirstRoundAfterReset) return;
            
                _resultQueues2D[SocketType.Left].Clear();
                _resultQueues2D[SocketType.Left].Enqueue(lastInLeft);
            
                var lastInRight = _resultQueues2D[SocketType.Right].LastOrDefault();
                _resultQueues2D[SocketType.Right].Clear();
                _resultQueues2D[SocketType.Right].Enqueue(lastInRight);
            }
        }

        private void OnPlcInitFinished()
        {
            LogPlcMessage("Plc init done");
            ResetStates();
        }

        private void ResetStates()
        {
            lock (_lockerOfResultQueues2D)
            {
                _resultQueues2D[SocketType.Left].Clear();
                _resultQueues2D[SocketType.Right].Clear();
            }

            RoundCountSinceReset = 0;
        }

        private void PlcCustomCommandHandler(int commandId)
        {
            switch (commandId)
            {
                case PlcMessagePackConstants.CommandIdLeftSocketArrived:
                    CurrentArrivedSocket = SocketType.Left;
                    LogPlcMessage("2D left socket arrived");
                    break;
                case PlcMessagePackConstants.CommandIdRightSocketArrived:
                    CurrentArrivedSocket = SocketType.Right;
                    LogPlcMessage("2D right socket arrived");
                    break;
            }
        }

        private void SetupCameras()
        {
            HKCameraManager.ScannedForAttachedCameras();
            TopCamera = HKCameraManager.AttachedCameras.First(cam => cam.Name == NameConstants.TopCameraName);
            TopCamera.ImageBatchSize = 5;
            TopCamera.BatchImageReceived += ProcessImages2DFireForget;


            // TODO: remove the following lines
            TopCamera.IsOpened = true;
            TopCamera.IsGrabbing = true;
            TopCamera.TriggerSourceType = CameraTriggerSourceType.Line0;
        }

        private void SetupLaserControllers()
        {
            ControllerManager.AttachedControllers = new List<ControllerViewModel>(NameConstants.ControllerNames
                .Select(name =>
                    new ControllerViewModel()
                    {
                        Name = name,
                        // 每800行发一张图回来
                        RowsPerRun = 800,
                        NumImagesPerRun = 1,
                        ProfileCountEachFetch = 100,
                    }).OrderBy(c => c.Name));
            ControllerManager.Init();
            IsAllControllersHighSpeedConnected = true;

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.RunFinished += (heightImages, luminanceImages) =>
                    OnSingleControllerFinishedScanningSingleSocket(heightImages, luminanceImages, controller);
            }
        }

        /// <summary>
        /// When a controller finished scanning one socket ...
        /// </summary>
        /// <param name="heightImages"></param>
        /// <param name="luminanceImages"></param>
        /// <param name="controller"></param>
        private void OnSingleControllerFinishedScanningSingleSocket(List<HImage> heightImages, List<HImage> luminanceImages,
            ControllerViewModel controller)
        {
            Trace.Assert(heightImages.Count == 1);
            // 绕开机台RESET时的蜜汁采图触发
            if (Server.IsBusyResetting) return;

            lock (_lockerOfLaserImageBuffers)
            {
                var socketIndex = _laserRunIndex[controller.Name];
                // Assign new image to corresponding image list
                var imageList = _laserImageBuffers[controller.Name];
                imageList[socketIndex] = heightImages[0];

                // If all controllers finish scanning the socket indexed _laserRunIndex[controller.Name]
                // begin processing one set of images
                if (_laserImageBuffers.Values.All(list => list[socketIndex] != null))
                {
                    On3DImagesOfOneSocketFinishedCollecting(socketIndex);
                }

                _laserRunIndex[controller.Name]++;
            }
        }

        /// <summary>
        /// When 3D images of one socket is finished collecting ...
        /// </summary>
        /// <param name="socketIndex"></param>
        private void On3DImagesOfOneSocketFinishedCollecting(int socketIndex)
        {
            var enumValue = (SocketType) socketIndex;
            LogRoutine($"{enumValue} socket finished 3D image collected");
            
            // Do processing for one socket and get its result
            var imagesForOneSocket =
                _laserImageBuffers.Values.Select(list => list[socketIndex]).ToList();
            _results3D[socketIndex] = _procedure3D.Execute(imagesForOneSocket, _shapeModel3D);

            // If all reserved places for 3D image buffers are filled,
            // 3D image collection of one round is done
            // and product level can be submit to plc
            var all3DImagesCollected =
                _laserImageBuffers.Values.All(list => list.All(image => image != null));
            if (all3DImagesCollected)
            {
                OnAllSocketsFinished3DImageCollection();
            }
        }

        /// <summary>
        /// Left socket finishing scanning indicates
        /// collection of images for one round is finished
        /// </summary>
        private void OnAllSocketsFinished3DImageCollection()
        {
            var isFirstRoundSinceReset = RoundCountSinceReset == 0;
            if (isFirstRoundSinceReset)
            {
                // Send dummy product level, Ng5 for now
                Server.SendProductLevels(ProductLevel.Ng5, ProductLevel.Ng5);
                return;
            }
            
            //NOTE: don't need to wait for new results of 2d,
            // since they are only required in the next round
            // and the corresponding 2d results that are needed
            // in this round is already available at the start of this round
            Combine2D3DResults();
            SubmitProductLevels();
            SerializeCsv();
            
            // Round finished, increment round count
            RoundCountSinceReset++;
        }

        /// <summary>
        /// Send product level information to plc
        /// </summary>
        private void SubmitProductLevels()
        {
            LeftProductLevel = GetProductLevel(_results3D[(int) SocketType.Left].ItemExists, FaiItemsLeft);
            RightProductLevel = GetProductLevel(_results3D[(int) SocketType.Right].ItemExists, FaiItemsRight);
            Server.SendProductLevels(LeftProductLevel, RightProductLevel);
        }
        

        private void SerializeCsv()
        {
            _serializerLeft.Serialize(FaiItemsLeft, DateTime.Now.ToString("T"));
            _serializerRight.Serialize(FaiItemsRight, DateTime.Now.ToString("T"));
        }

        /// <summary>
        /// Convert dictionaries from image processing units to list of fai items
        /// and display them
        /// </summary>
        private void Combine2D3DResults()
        {
            var leftSocketIndex = (int) SocketType.Left;
            var rightSocketIndex = (int) SocketType.Right;

            // Update _results2D
            lock (_lockerOfResultQueues2D)
            {
                // Corresponding results are put to the head of queues
                // when new round starts
                // No worry about this
                _results2D[leftSocketIndex] = _resultQueues2D[SocketType.Left].Dequeue();
                _results2D[rightSocketIndex] = _resultQueues2D[SocketType.Right].Dequeue();
            }

            var faiResultDictLeft = ConcatDictionaryNew(_results2D[leftSocketIndex].FaiResults,
                _results3D[leftSocketIndex].FaiResults);
            var faiResultDictRight = ConcatDictionaryNew(_results2D[rightSocketIndex].FaiResults,
                _results3D[rightSocketIndex].FaiResults);

            // To avoid frequent context switching
            // Wrap all the UI-updating code in single Invoke block
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                // Update fai item lists using dictionaries from image processing modules
                UpdateFaiItems(_faiItems2DLeft, _results2D[leftSocketIndex].FaiResults);
                UpdateFaiItems(_faiItems2DRight, _results2D[rightSocketIndex].FaiResults);
                UpdateFaiItems(_faiItems3DLeft, _results3D[leftSocketIndex].FaiResults);
                UpdateFaiItems(_faiItems3DRight, _results3D[rightSocketIndex].FaiResults);
                UpdateFaiItems(FaiItemsLeft, faiResultDictLeft);
                UpdateFaiItems(FaiItemsRight, faiResultDictRight);

                // Optionally display results and fai items based on current displaying page
                // 2D
                ResultToDisplay2D = _results2D[(int) SocketToDisplay2D];
                FaiItems2D = SocketToDisplay2D == SocketType.Left ? _faiItems2DLeft : _faiItems2DRight;

                // 3D
                ResultToDisplay3D = _results3D[(int) SocketToDisplay3D];
                FaiItems3D = SocketToDisplay3D == SocketType.Left ? _faiItems3DLeft : _faiItems3DRight;
            });
        }


        private void LoadFaiItems()
        {
            _faiItems2DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames2D,
                    DirectoryConstants.FaiConfigDir2DLeft)
                .ToList();
            _faiItems2DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames2D,
                    DirectoryConstants.FaiConfigDir2DRight)
                .ToList();
            _faiItems3DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames3D,
                    DirectoryConstants.FaiConfigDir3DLeft)
                .ToList();
            _faiItems3DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames3D,
                    DirectoryConstants.FaiConfigDir3DRight)
                .ToList();

            FaiItemsLeft = _faiItems2DLeft.ConcatNew(_faiItems3DLeft);
            FaiItemsRight = _faiItems2DRight.ConcatNew(_faiItems3DRight);
        }

        private void LoadShapeModels()
        {
            HOperatorSet.ReadShapeModel(PathConstants.ShapeModelPath2D, out _shapeModel2D);
            HOperatorSet.ReadShapeModel(PathConstants.ShapeModelPath3D, out _shapeModel3D);
        }


        /// <summary>
        /// Decide whether the item is missing, passed or rejected
        /// </summary>
        /// <param name="itemExists"></param>
        /// <param name="faiItems"></param>
        /// <returns></returns>
        private ProductLevel GetProductLevel(bool itemExists, List<FaiItem> faiItems)
        {
            if (!itemExists) return ProductLevel.Ng5;
            return faiItems.Any(item => item.Rejected) ? ProductLevel.Ng2 : ProductLevel.Ok;
        }

        /// <summary>
        /// Update ValueUnbiased of fai-items from dictionaries
        /// </summary>
        /// <param name="faiItems"></param>
        /// <param name="faiResultDict"></param>
        private void UpdateFaiItems(List<FaiItem> faiItems, Dictionary<string, double> faiResultDict)
        {
            foreach (var faiResult in faiResultDict)
            {
                faiItems.ByName(faiResult.Key).ValueUnbiased = faiResult.Value;
            }
        }

        /// <summary>
        /// Create a new dictionary from the give dictionaries
        /// </summary>
        /// <param name="resultDicts"></param>
        /// <returns></returns>
        private Dictionary<string, double> ConcatDictionaryNew(params Dictionary<string, double>[] resultDicts)
        {
            var output = new Dictionary<string, double>();

            foreach (var resultDict in resultDicts)
            {
                foreach (var result in resultDict)
                {
                    output[result.Key] = result.Value;
                }
            }

            return output;
        }


        /// <summary>
        /// Process 2d images from one socket and queue up the results
        /// </summary>
        /// <param name="images"></param>
        private void ProcessImages2DFireForget(List<HImage> images)
        {
            LogRoutine($"{CurrentArrivedSocket} socket start processing {images.Count} 2D images");
            Task.Run(() =>
            {
                if (CurrentArrivedSocket == SocketType.Left)
                {
                    var result = _procedure2D.Execute(images, FindLineParams2D.ToDict());
                    result.Images = images;
                    lock (_lockerOfResultQueues2D)
                    {
                        _resultQueues2D[SocketType.Left].Enqueue(result);
                    }
                }
                else
                {
                    var result = _procedure2D.Execute(images, FindLineParams2D.ToDict());
                    result.Images = images;
                    lock (_lockerOfResultQueues2D)
                    {
                        _resultQueues2D[SocketType.Right].Enqueue(result);
                    }
                }
            });
        }

        public void LogPlcMessage(string message)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                lock (_lockerOfPlcMessageList)
                {
                    PlcMessageList.Add(new LoggingMessageItem()
                    {
                        Time = DateTime.Now.ToString("T"),
                        Message = message
                    });

                    // Remove some messages if overflows
                    if (PlcMessageList.Count > _maxRoutineLogs)
                    {
                        PlcMessageList =
                            new ObservableCollection<LoggingMessageItem>(PlcMessageList.Skip(_maxRoutineLogs / 3));
                    }
                }
            });
        }

        public void LogRoutine(string message)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                lock (_lockerOfRoutineMessageList)
                {
                    RoutineMessageList.Add(new LoggingMessageItem()
                    {
                        Time = DateTime.Now.ToString("T"),
                        Message = message
                    });
                    // Remove some messages if overflows
                    if (RoutineMessageList.Count > _maxRoutineLogs)
                    {
                        RoutineMessageList =
                            new ObservableCollection<LoggingMessageItem>(RoutineMessageList.Skip(_maxRoutineLogs / 3));
                    }

                    // Expand logging box
                    AutoResetFlag = true;
                }
            });
        }

        /// <summary>
        /// This should be executed only after the construction of the instance,
        /// so the logging system can be used to debug
        /// </summary>
        public void InitHardWares()
        {
            SetupServer();

            SetupCameras();

            SetupLaserControllers();
        }

        /// <summary>
        /// Init before any binding taking place can avoid bazaar <see cref="TypeInitializationException"/> exceptions
        /// </summary>
        public static void Init()
        {
            _instance = new ApplicationViewModel();
        }

        /// <summary>
        /// Disconnect cameras, lasers and plc
        /// </summary>
        public static void Cleanup()
        {
            foreach (var camera in HKCameraManager.AttachedCameras)
            {
                camera.IsOpened = false;
            }

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.IsConnectedHighSpeed = false;
            }

            Instance.Server.Disconnect();
        }


        #region Properties
        
        public int RoundCountSinceReset { get; set; }

        public BinListViewModel Bins { get; set; }

        public bool IsAllControllersHighSpeedConnected
        {
            get { return _isAllControllersHighSpeedConnected; }
            set
            {
                _isAllControllersHighSpeedConnected = value;
                foreach (var controller in ControllerManager.AttachedControllers)
                {
                    controller.IsConnectedHighSpeed = value;
                }
            }
        }

        public bool IsBusyWaitingFor2DToFinished { get; set; }

        /// <summary>
        /// 2D fai items from left or right socket
        /// </summary>
        public List<FaiItem> FaiItems2D { get; set; }

        /// <summary>
        /// 3D fai items from left or right socket
        /// </summary>
        public List<FaiItem> FaiItems3D { get; set; }

        public List<FindLineParam> FindLineParams2D { get; set; }

        public bool IsBusyRunningManualTest2D { get; set; }

        /// <summary>
        /// Current image to display in Vision2DView
        /// </summary>
        public HImage ImageToDisplay2D
        {
            get { return ResultToDisplay2D?.Images?[ImageIndexToDisplay2D]; }
        }

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
                ResultToDisplay2D = _results2D[(int) value];
                FaiItems2D = value == SocketType.Left ? _faiItems2DLeft : _faiItems2DRight;
            }
        }


        public SocketType SocketToDisplay3D
        {
            get { return _socketToDisplay3D; }
            set
            {
                _socketToDisplay3D = value;
                ResultToDisplay3D = _results3D[(int) value];
                FaiItems3D = value == SocketType.Left ? _faiItems3DLeft : _faiItems3DRight;
            }
        }

        public MeasurementResult3D ResultToDisplay3D { get; set; }

        public SocketType CurrentArrivedSocket { get; set; }
        public ProductLevel LeftProductLevel { get; set; }
        public ProductLevel RightProductLevel { get; set; }


        /// <summary>
        /// Switch 2D view between left and right socket
        /// </summary>
        public ICommand SwitchSocketView2DCommand { get; set; }

        public ICommand SwitchSocketView3DCommand { get; set; }

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
        public CameraViewModel TopCamera { get; set; }


        /// <summary>
        /// Message list for routine logging
        /// </summary>
        public ObservableCollection<LoggingMessageItem> RoutineMessageList { get; set; } =
            new ObservableCollection<LoggingMessageItem>();

        /// <summary>
        /// Message list for plc message logging
        /// </summary>
        public ObservableCollection<LoggingMessageItem> PlcMessageList { get; set; }

        public AlcServerViewModel Server { get; set; }

        public List<FaiItem> FaiItemsLeft { get; set; }
        public List<FaiItem> FaiItemsRight { get; set; }

        #endregion
    }
}