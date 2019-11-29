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
using Core.ViewModels.Fai;
using Core.ViewModels.Plc;
using Core.ViewModels.Summary;
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

        public MeasurementResult2D Result2DLeft { get; set; } = new MeasurementResult2D();
        public MeasurementResult2D Result2DRight { get; set; } = new MeasurementResult2D();

        public MeasurementResult3D Result3DLeft { get; set; } = new MeasurementResult3D();
        public MeasurementResult3D Result3DRight { get; set; } = new MeasurementResult3D();

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


        private readonly Dictionary<SocketType, Queue<MeasurementResult2D>> _resultQueues2D =
            new Dictionary<SocketType, Queue<MeasurementResult2D>>()
            {
                [SocketType.Left] = new Queue<MeasurementResult2D>(2),
                [SocketType.Right] = new Queue<MeasurementResult2D>(2)
            };

        private readonly Object _lockerOfResultQueues2D = new Object();
        private bool _isAllControllersHighSpeedConnected;
        private int _rightSocketIndexSinceReset2D;
        private readonly object _lockerOfRightSocketIndexSinceReset2D = new object();
        private readonly object _lockerOfCurrentArrivedSocket2D = new object();
        private readonly DispatcherTimer _lazyTimer = new DispatcherTimer(DispatcherPriority.Background);

        protected ApplicationViewModel()
        {
            InitContainer();
            AutoResetInterval = 10000;
            CurrentApplicationPage = ApplicationPageType.Home;
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
            PlcMessageList = new ObservableCollection<LoggingMessageItem>();
            BindingOperations.EnableCollectionSynchronization(RoutineMessageList, _lockerOfRoutineMessageList);
            BindingOperations.EnableCollectionSynchronization(PlcMessageList, _lockerOfPlcMessageList);

            InitTimers();

            InitCommands();

            ClearLaserImagesForNewRound();
        }

        private void InitCommands()
        {
        }

        private void InitTimers()
        {
            _lazyTimer.Interval = TimeSpan.FromSeconds(5);
            _lazyTimer.Tick += OnLazyTimerClicked;

            _lazyTimer.Start();
        }

        private void OnLazyTimerClicked(object sender, EventArgs e)
        {
            // Clear messages if overflows
            ClearMessagesIfOverflows(PlcMessageList, 100);
            ClearMessagesIfOverflows(RoutineMessageList, 100);

            GenerateSummaryNameList();
            ReadSelectedSummary();
        }

        public void ReadSelectedSummary()
        {
            if (string.IsNullOrEmpty(SelectedSummaryName))
            {
                SelectedSummary = SummaryCurrentHour;
            }
            else
            {
                SelectedSummary = AutoSerializableHelper.LoadAutoSerializable<SummaryViewModel>(
                    DirectoryConstants.SummaryDirToday,
                    SelectedSummaryName);
            }
        }

        /// <summary>
        /// Scan disk and add the existing hour summary of today to list
        /// </summary>
        private void GenerateSummaryNameList()
        {
            Directory.CreateDirectory(DirectoryConstants.SummaryDirToday);
            var xmls = Directory.GetFiles(DirectoryConstants.SummaryDirToday);
            SummaryNames = xmls.Select(Path.GetFileNameWithoutExtension).ToList();
        }


        private void ClearMessagesIfOverflows(ObservableCollection<LoggingMessageItem> messageList, int maxCount)
        {
            if (messageList.Count < maxCount) return;
            int numToRemove = (int) (maxCount * 0.3);
            for (int i = 0; i < numToRemove; i++)
            {
                messageList.RemoveAt(i);
            }
        }

        /// <summary>
        /// Load summaries of the production line
        /// </summary>
        private void LoadProductionLineSummaries()
        {
            // Today
            SummaryToday =
                AutoSerializableHelper.LoadAutoSerializable<SummaryViewModel>(DirectoryConstants.SummaryDirToday,
                    "Today");
            SummaryToday.ShouldAutoSerialize = true;

            // Current hour
            SummaryCurrentHour = AutoSerializableHelper.LoadAutoSerializable<SummaryViewModel>(
                DirectoryConstants.SummaryDirToday,
                TimeHelper.CurrentHour);
            SummaryCurrentHour.ShouldAutoSerialize = true;
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


        private void SetupServer()
        {
            Server = new AlcServerViewModel()
            {
                IpAddress = IPAddress.Parse("192.168.100.100"),
                Port = 4000
            };
            Server.AutoRunStartRequested += OnNewRoundStarted;
            Server.AutoRunStopRequested += () => LogPlcMessage("Auto-mode-stop requested from plc");
            Server.InitRequested += () => LogPlcMessage("Init requested from plc");
            Server.ClientHooked += OnPlcHooked;
            Server.CustomCommandReceived += PlcCustomCommandHandler;
            Server.PlcInitFinished += OnPlcInitFinished;
        }

        private void OnPlcHooked(Socket socket)
        {
            LogPlcMessage(socket.RemoteEndPoint + " is hooked");
        }

        private void OnNewRoundStarted()
        {
            LogPlcMessage("Auto-mode-start requested from plc");
            ClearLaserImagesForNewRound();
            Enqueue2DImagesFromPreviousRound();
            lock (_lockerOfRightSocketIndexSinceReset2D)
            {
                // Note: this must be updated at the time of take-off
                _rightSocketIndexSinceReset2D = RoundCountSinceReset * 2 + 1;
            }
        }

        /// <summary>
        /// When starting a new round
        /// the last one in the queue must be the one
        /// that will be concluded with 3D results in the new round
        /// </summary>
        private void Enqueue2DImagesFromPreviousRound()
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
                    CurrentArrivedSocket2D = SocketType.Left;
                    LogPlcMessage("2D left socket arrived");
                    break;
                case PlcMessagePackConstants.CommandIdRightSocketArrived:
                    CurrentArrivedSocket2D = SocketType.Right;
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
        private void OnSingleControllerFinishedScanningSingleSocket(List<HImage> heightImages,
            List<HImage> luminanceImages,
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
            var result3D = _procedure3D.Execute(imagesForOneSocket, _shapeModel3D);
            if (socketIndex == (int) SocketType.Left)
            {
                Result3DLeft = result3D;
            }
            else
            {
                Result3DRight = result3D;
            }


            #region ImageSerialization

            int itemIndexSinceReset;
            lock (_lockerOfRightSocketIndexSinceReset2D)
            {
                itemIndexSinceReset = socketIndex == (int) SocketType.Right
                    ? _rightSocketIndexSinceReset2D - 2
                    : _rightSocketIndexSinceReset2D - 1;
            }

            // TODO: remove the following image serialization logic
            if (itemIndexSinceReset > 0 && socketIndex == (int) SocketType.Left)
            {
                Task.Run(() =>
                {
                    Directory.CreateDirectory(DirectoryConstants.ImageDir3D);
                    for (int i = 0; i < imagesForOneSocket.Count; i++)
                    {
                        var imageName = $"{itemIndexSinceReset}-{i}.tif";
                        imagesForOneSocket[i].WriteImage("tiff", 0,
                            Path.Combine(DirectoryConstants.ImageDir3D, imageName));
                    }
                });
            }

            #endregion


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
            }

            else
            {
                //NOTE: don't need to wait for new results of 2d,
                // since they are only required in the next round
                // and the corresponding 2d results that are needed
                // in this round is already available at the start of this round
                Combine2D3DResults();
                SubmitProductLevels();
                UpdateSummaries();
                SerializeCsv();
            }

            // Round finished, increment round count
            RoundCountSinceReset++;
        }

        private void UpdateSummaries()
        {
            SummaryToday.Update(LeftProductLevel);
            SummaryToday.Update(RightProductLevel);

            SummaryCurrentHour.Update(LeftProductLevel);
            SummaryCurrentHour.Update(RightProductLevel);
        }

        /// <summary>
        /// Send product level information to plc
        /// </summary>
        private void SubmitProductLevels()
        {
            LeftProductLevel = GetProductLevel(Result3DLeft.ItemExists, FaiItemsLeft);
            RightProductLevel = GetProductLevel(Result3DRight.ItemExists, FaiItemsRight);
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
                Result2DLeft = _resultQueues2D[SocketType.Left].Dequeue();
                Result2DRight = _resultQueues2D[SocketType.Right].Dequeue();
            }

            var faiResultDictLeft = ConcatDictionaryNew(Result2DLeft.FaiResults,
                Result3DLeft.FaiResults);
            var faiResultDictRight = ConcatDictionaryNew(Result2DRight.FaiResults,
                Result3DRight.FaiResults);

            // To avoid frequent context switching
            // Wrap all the UI-updating code in single Invoke block
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                // Update fai item lists using dictionaries from image processing modules
                UpdateFaiItems(FaiItems2DLeft, Result2DLeft.FaiResults);
                UpdateFaiItems(FaiItems2DRight, Result2DRight.FaiResults);
                UpdateFaiItems(FaiItems3DLeft, Result3DLeft.FaiResults);
                UpdateFaiItems(FaiItems3DRight, Result3DRight.FaiResults);
                UpdateFaiItems(FaiItemsLeft, faiResultDictLeft);
                UpdateFaiItems(FaiItemsRight, faiResultDictRight);
            });
        }


        private void LoadFaiItems()
        {
            FaiItems2DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames2D,
                    DirectoryConstants.FaiConfigDir2DLeft)
                .ToList();
            FaiItems2DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames2D,
                    DirectoryConstants.FaiConfigDir2DRight)
                .ToList();
            FaiItems3DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames3D,
                    DirectoryConstants.FaiConfigDir3DLeft)
                .ToList();
            FaiItems3DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(NameConstants.FaiItemNames3D,
                    DirectoryConstants.FaiConfigDir3DRight)
                .ToList();

            FaiItemsLeft = FaiItems2DLeft.ConcatNew(FaiItems3DLeft);
            FaiItemsRight = FaiItems2DRight.ConcatNew(FaiItems3DRight);
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
            SocketType currentArrivedSocket2D;
            lock (_lockerOfCurrentArrivedSocket2D)
            {
                currentArrivedSocket2D = CurrentArrivedSocket2D;
            }

            LogRoutine($"{currentArrivedSocket2D} socket start processing {images.Count} 2D images");

            int itemIndexSinceReset;
            lock (_lockerOfRightSocketIndexSinceReset2D)
            {
                itemIndexSinceReset = currentArrivedSocket2D == SocketType.Right
                    ? _rightSocketIndexSinceReset2D
                    : _rightSocketIndexSinceReset2D + 1;
            }

            // TODO: remove the following image serialization logic
            if (currentArrivedSocket2D == SocketType.Left)
            {
                Task.Run(() =>
                {
                    Directory.CreateDirectory(DirectoryConstants.ImageDir2D);
                    for (int i = 0; i < images.Count; i++)
                    {
                        var imageName = $"{itemIndexSinceReset}-{i}.bmp";
                        images[i].WriteImage("bmp", 0, Path.Combine(DirectoryConstants.ImageDir2D, imageName));
                    }
                });
            }


            Task.Run(() =>
            {
                if (CurrentArrivedSocket2D == SocketType.Left)
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
                PlcMessageList.Add(new LoggingMessageItem()
                {
                    Time = DateTime.Now.ToString("T"),
                    Message = message
                });
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

                    var tempList = RoutineMessageList;
                    RoutineMessageList = null;
                    RoutineMessageList = tempList;

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

        
        public void LoadFiles()
        {
            LoadShapeModels();

            LoadFaiItems();

            LoadFindLineParams2D();

            LoadProductionLineSummaries();
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


        public SummaryViewModel Bins { get; set; }

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


        public List<FindLineParam> FindLineParams2D { get; set; }


        public SocketType CurrentArrivedSocket2D { get; set; }
        public ProductLevel LeftProductLevel { get; set; }
        public ProductLevel RightProductLevel { get; set; }


        public ICommand SwitchSocketView3DCommand { get; set; }

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

        public List<FaiItem> FaiItems2DLeft { get; set; }
        public List<FaiItem> FaiItems2DRight { get; set; }
        public List<FaiItem> FaiItems3DLeft { get; set; }
        public List<FaiItem> FaiItems3DRight { get; set; }

        public SummaryViewModel SummaryCurrentHour { get; set; }
        public SummaryViewModel SelectedSummary { get; set; }
        public SummaryViewModel SummaryToday { get; set; }
        public List<string> SummaryNames { get; set; }

        public string SelectedSummaryName { get; set; }

        #endregion
    }
}