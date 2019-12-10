using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ImageProcessing;
using Core.IoC.Loggers;
using Core.IoC.PlcErrorParser;
using Core.ViewModels.Fai;
using Core.ViewModels.Plc;
using Core.ViewModels.Results;
using Core.ViewModels.Summary;
using CYG906ALC.ALG;
using HalconDotNet;
using HKCamera;
using I40_3D_Test;
using LJX8000.Core.ViewModels.Controller;
using PLCCommunication.Core.Enums;
using PLCCommunication.Core.ViewModels;
using PLS;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;
using CavityType = Core.Enums.SocketType;

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

        public GraphicsPackViewModel Graphics2DLeft
        {
            get { return _graphics2DLeft; }
            set { _graphics2DLeft = value; }
        }

        public GraphicsPackViewModel Graphics2DRight
        {
            get { return _graphics2DRight; }
            set { _graphics2DRight = value; }
        }

        public GraphicsPackViewModel Graphics3DLeft { get; set; } = new GraphicsPackViewModel();
        public GraphicsPackViewModel Graphics3DRight { get; set; } = new GraphicsPackViewModel();


        private HTuple _shapeModel2D, _shapeModel3D;

        private readonly CsvSerializer.CsvSerializer _serializerAll =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "All"));

        private readonly CsvSerializer.CsvSerializer _serializerLeft =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity1"));


        private readonly CsvSerializer.CsvSerializer _serializerRight =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity2"));

        private readonly IMeasurementProcedure3D _procedure3D = new I40_3D_Output();

        private readonly object _lockerOfRoutineMessageList = new object();

        private readonly Dictionary<CavityType, Queue<GraphicsPackViewModel>> _resultQueues2D =
            new Dictionary<CavityType, Queue<GraphicsPackViewModel>>
            {
                [CavityType.Cavity1] = new Queue<GraphicsPackViewModel>(),
                [CavityType.Cavity2] = new Queue<GraphicsPackViewModel>()
            };

        private readonly object _lockerOfResultQueues2D = new object();
        private bool _isAllControllersHighSpeedConnected;
        private int _rightSocketIndexSinceReset2D;
        private readonly object _lockerOfRightSocketIndexSinceReset2D = new object();
        private readonly object _lockerOfCurrentArrivedSocket2D = new object();
        private readonly DispatcherTimer _lazyTimer = new DispatcherTimer(DispatcherPriority.Background);
        private readonly object _lockerOfPlcMessageList = new object();
        private LoggingMessageItem _waringMessageHighLevel;

        protected ApplicationViewModel()
        {
            CurrentApplicationPage = ApplicationPageType.Home;
            PlcMessageList = new ObservableCollection<LoggingMessageItem>();
            EnableSynchronization();

            InitTimers();

            InitCommands();

            ClearLaserImagesForNewRound();
        }

        private void InitCommands()
        {
            OpenCSVDirCommand = new RelayCommand(() =>
            {
                Directory.CreateDirectory(DirectoryConstants.CsvOutputDir);
                Process.Start(DirectoryConstants.CsvOutputDir);
            });
            OpenImageDirCommand = new RelayCommand(() => { Process.Start(Directory.GetCurrentDirectory()); });
            
            SimulateCommand = new RelayCommand(DoSimulation);
        }

        /// <summary>
        /// Do temporary simulations
        /// </summary>
        private void DoSimulation()
        {
            Summary.UpdateCurrentSummary("Ng4");
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
//            lock (_lockerOfPlcMessageList)
//            {
//                ClearMessagesIfOverflows(PlcMessageList, 100);
//            }
//
//            lock (_lockerOfRoutineMessageList)
//            {
//                ClearMessagesIfOverflows(RoutineMessageList, 100);
//            }
        }


        private void ClearMessagesIfOverflows(ObservableCollection<LoggingMessageItem> messageList, int maxCount)
        {
            if (messageList.Count < maxCount) return;
            try
            {
                int numToRemove = (int) (maxCount * 0.3);
                for (int i = 0; i < numToRemove; i++)
                {
                    messageList.RemoveAt(i);
                }
            }
            catch
            {
                // it's fine to remove it later
            }
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
                    _laserImageBuffers[name] = new List<HImage>
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


        private void SetupServer()
        {
            Server = new AlcServerViewModel
            {
                IpAddress = IPAddress.Parse("192.168.100.100"),
                Port = 4000
            };
            Server.NewRunStarted += OnNewRunStarted;
            Server.AutoRunStopRequested += () => LogPlcMessage("Auto-mode-stop requested from plc");
            Server.InitRequested += OnPlcInitRequested;
            Server.ClientHooked += OnPlcHooked;
            Server.CustomCommandReceived += PlcCustomCommandHandler;
            Server.PlcResetFinished += OnPlcResetFinished;
            Server.PlcStopFinished += OnPlcStopFinished;
            Server.IsAutoRunningChanged += isAutoRunning =>
            {
                if (isAutoRunning) ResetStates();
            };

            var errorParser = new PlcErrorParser(Path.Combine(DirectoryHelper.ConfigDirectory, "ErrorSheet.csv"));
            errorParser.WarningL1Emit += OnWarningL1Received;
            errorParser.WarningL2Emit += OnWarningL2Received;
            errorParser.WarningL3Emit += OnWarningL3Received;
            errorParser.WarningL4Emit += OnWarningL4Received;
            Server.ErrorParser = errorParser;
        }

        private void OnPlcStopFinished()
        {
            ResetStates();
        }

        private void OnPlcInitRequested()
        {
            LogPlcMessage("Init requested from plc");
        }


        /// <summary>
        /// Enable plc init command
        /// </summary>
        private void EnablePlcInit()
        {
            Server.IsBusyResetting = false;
            Server.IsAutoRunning = false;
            Server.CurrentMachineState = MachineState.Idle;
        }

        private void LogHighLevelWarning(string s)
        {
            WaringMessageHighLevel = new LoggingMessageItem()
                {Time = DateTime.Now.ToString("hh:mm:ss t z"), Message = s};
        }

        private void OnWarningL4Received(string message)
        {
            Logger.Instance.LogErrorToFile(message);
            LogHighLevelWarning(message);
            //  Init must be able to execute after L4 warning received
            EnablePlcInit();
        }

        private void OnWarningL3Received(string message)
        {
            Logger.Instance.LogErrorToFile(message);
            LogHighLevelWarning(message);
        }

        private void OnWarningL2Received(string message)
        {
            Logger.Instance.LogErrorToFile(message);
            LogHighLevelWarning(message);
        }

        private void OnWarningL1Received(string message)
        {
            Logger.Instance.LogErrorToFile(message);
        }

        private void OnPlcHooked(Socket socket)
        {
            LogPlcMessage(socket.RemoteEndPoint + " is hooked");
        }

        private void OnNewRunStarted()
        {
            LogPlcMessage("New run starts");
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
                var lastInLeft = _resultQueues2D[CavityType.Cavity1].LastOrDefault();
                var lastInRight = _resultQueues2D[CavityType.Cavity2].LastOrDefault();
                if (lastInLeft == null && lastInRight != null || lastInLeft != null && lastInRight == null)
                    throw new InvalidOperationException("Result of one cavity is missing");
                var isFirstRoundAfterReset = lastInLeft == null;
                if (isFirstRoundAfterReset)
                {
                    return;
                }

                lock (_lockerOfResultQueues2D)
                {
                    _graphics2DLeft = _resultQueues2D[CavityType.Cavity1].Dequeue();
                    Trace.Assert(_resultQueues2D[CavityType.Cavity1].Count == 0);

                    _graphics2DRight = _resultQueues2D[CavityType.Cavity2].Dequeue();
                    Trace.Assert(_resultQueues2D[CavityType.Cavity2].Count == 0);
                }
            }
        }

        private void OnPlcResetFinished()
        {
            LogPlcMessage("Plc init done");
            ResetStates();
        }

        private void ResetStates()
        {
            lock (_lockerOfResultQueues2D)
            {
                _resultQueues2D[CavityType.Cavity1].Clear();
                _resultQueues2D[CavityType.Cavity2].Clear();
            }

            RoundCountSinceReset = 0;
        }

        private void PlcCustomCommandHandler(int commandId)
        {
            switch (commandId)
            {
                case PlcMessagePackConstants.CommandIdLeftSocketArrived:
                    OnCavity1Arrived2D();
                    break;
                case PlcMessagePackConstants.CommandIdRightSocketArrived:
                    OnCavity2Arrived2D();
                    break;
                default:
                    LogPlcMessage($"Command ID {commandId} received");
                    break;
            }
        }

        private void OnCavity1Arrived2D()
        {
            lock (_lockerOfCurrentArrivedSocket2D)
            {
                CurrentArrivedSocket2D = CavityType.Cavity1;
            }

            LogPlcMessage("2D left socket arrived");
        }

        private void OnCavity2Arrived2D()
        {
            lock (_lockerOfCurrentArrivedSocket2D)
            {
                CurrentArrivedSocket2D = CavityType.Cavity2;
            }

            LogPlcMessage("2D right socket arrived");
            ResultReady2D = ResultStatus.Waiting;
        }

        private void SetupCameras()
        {
            TopCamera = new HKCameraViewModel {ImageBatchSize = 6};
            TopCamera.ImageBatchCollected += ProcessImages2D;
            TopCamera.Open();
            TopCamera.SetTriggerToLine0();
            TopCamera.StartGrabbing();
        }

        private void SetupLaserControllers()
        {
            ControllerManager.AttachedControllers = new List<ControllerViewModel>(NameConstants.ControllerNames
                .Select(name =>
                    new ControllerViewModel
                    {
                        Name = name,
                        // 每800行发一张图回来
                        RowsPerRun = 800,
                        NumImagesPerRun = 1,
                        ProfileCountEachFetch = 100
                    }).OrderBy(c => c.Name));
            ControllerManager.Init();
            IsAllControllersHighSpeedConnected = true;

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.RunFinished += (heightImages, luminanceImages) =>
                    OnSingleControllerFinishedScanningSingleSocket(heightImages, controller);
            }
        }

        /// <summary>
        /// When a controller finished scanning one socket ...
        /// </summary>
        /// <param name="heightImages"></param>
        /// <param name="controller"></param>
        private void OnSingleControllerFinishedScanningSingleSocket(IReadOnlyList<HImage> heightImages,
            ControllerViewModel controller)
        {
            if (!Server.IsAutoRunning) return;
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
            var enumValue = (CavityType) socketIndex;


            LogRoutine($"3D processing starts for {enumValue} socket");
            // Do processing for one socket and get its result
            var imagesForOneSocket =
                _laserImageBuffers.Values.Select(list => list[socketIndex]).ToList();

            // Save images for later serialization when 2d and 3d combine
            if (enumValue == CavityType.Cavity1) _imagesToSerialize3dLeft = imagesForOneSocket;
            else _imagesToSerialize3dRight = imagesForOneSocket;

            MeasurementResult3D result3D = null;
            try
            {
                result3D = _procedure3D.Execute(imagesForOneSocket, _shapeModel3D);
            }
            catch
            {
                result3D = new MeasurementResult3D()
                {
                    FaiResults = new Dictionary<string, double>(),
                    CompositeImage = imagesForOneSocket,
                };
            }

            if (socketIndex == (int) CavityType.Cavity1)
            {
                Graphics3DLeft = result3D.GetGraphics();
            }
            else
            {
                Graphics3DRight = result3D.GetGraphics();
            }

            LogRoutine($"3D processing ends for {enumValue} socket");


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

        private List<HImage> _imagesToSerialize3dRight;

        private List<HImage> _imagesToSerialize3dLeft;
        private int _config2dDirIndex;
        private GraphicsPackViewModel _graphics2DLeft = new GraphicsPackViewModel();
        private GraphicsPackViewModel _graphics2DRight = new GraphicsPackViewModel();
        private List<string> _names2d;
        private List<string> _names3d;

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

                UiDispatcher.Invoke(UpdateSummaries);
                Task.Run(() => SerializeImagesAndCsv(Graphics2DLeft.Images, Graphics2DRight.Images,
                    _imagesToSerialize3dLeft,
                    _imagesToSerialize3dRight, FaiItemsCavity1, FaiItemsCavity2));
            }

            // Round finished, increment round count
            RoundCountSinceReset++;
        }

        private void UpdateSummaries()
        {
            if (Cavity1ProductLevel != ProductLevel.Empty) Summary.UpdateCurrentSummary(Cavity1ProductLevel.ToString());
            if (Cavity2ProductLevel != ProductLevel.Empty) Summary.UpdateCurrentSummary(Cavity2ProductLevel.ToString());
        }

        /// <summary>
        /// Send product level information to plc
        /// </summary>
        private void SubmitProductLevels()
        {
            Cavity1ProductLevel = GetProductLevel(Graphics3DLeft.ItemExists, FaiItemsCavity1);
            Cavity2ProductLevel = GetProductLevel(Graphics3DRight.ItemExists, FaiItemsCavity2);
            Server.SendProductLevels(Cavity1ProductLevel, Cavity2ProductLevel);
        }


        /// <summary>
        /// Convert dictionaries from image processing units to list of fai items
        /// and display them
        /// </summary>
        private void Combine2D3DResults()
        {
            var faiResultDictLeft = ConcatDictionaryNew(Graphics2DLeft.FaiResults,
                Graphics3DLeft.FaiResults);
            var faiResultDictRight = ConcatDictionaryNew(Graphics2DRight.FaiResults,
                Graphics3DRight.FaiResults);

            // To avoid frequent context switching
            // Wrap all the UI-updating code in single Invoke block
            UiDispatcher.Invoke(() =>
            {
                // Update fai item lists using dictionaries from image processing modules
                UpdateFaiItems(FaiItemsCavity1, faiResultDictLeft, Graphics3DLeft.ItemExists);
                UpdateFaiItems(FaiItemsCavity2, faiResultDictRight, Graphics3DRight.ItemExists);

                // Notify Ui
                OnPropertyChanged(nameof(FaiItems2DLeft));
                OnPropertyChanged(nameof(FaiItems2DRight));
                OnPropertyChanged(nameof(FaiItems3DLeft));
                OnPropertyChanged(nameof(FaiItems3DRight));
                OnPropertyChanged(nameof(FaiItemsCavity1));
                OnPropertyChanged(nameof(FaiItemsCavity2));
                
                // Ensure 2d and 3d graphics are updated with approximate time
                OnPropertyChanged(nameof(Graphics2DLeft));
                OnPropertyChanged(nameof(Graphics2DRight));
            });
        }

        private void SerializeImagesAndCsv(List<HImage> images2dCavity1, List<HImage> images2dCavity2,
            List<HImage> images3dCavity1, List<HImage> images3dCavity2, List<FaiItem> faiItemsCavity1,
            List<FaiItem> faiItemsCavity2)
        {
            // Cavity 2
            var timestampCavity2 = SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity2, images3dCavity2,
                ShouldSave2DImagesRight,
                ShouldSave3DImagesRight, DirectoryConstants.ImageDirRight);
            _serializerRight.Serialize(faiItemsCavity2, timestampCavity2, Cavity2ProductLevel.GetResultText());
            _serializerAll.Serialize(faiItemsCavity2, timestampCavity2, Cavity2ProductLevel.GetResultText());


            // Cavity 1
            var timestampCavity1 = SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity1, images3dCavity1,
                ShouldSave2DImagesLeft,
                ShouldSave3DImagesLeft, DirectoryConstants.ImageDirLeft);
            _serializerLeft.Serialize(faiItemsCavity1, timestampCavity1, Cavity1ProductLevel.GetResultText());
            _serializerAll.Serialize(faiItemsCavity1, timestampCavity1, Cavity1ProductLevel.GetResultText());

            // Update tables
            UiDispatcher.InvokeAsync(() => UpdateTables(timestampCavity1, timestampCavity2));
        }

        private void UpdateTables(string timestampCavity1, string timestampCavity2)
        {
            // Init header if necessary
            if (Table == null)
            {
                Table = new FaiTableStackViewModel()
                {
                    Header = FaiItemsCavity1.Select(item => item.Name).ToList(),
                    MaxRows = 50,
                };
            }

            // Add rows
            Table.AddRow(FaiItemsCavity2, Cavity2ProductLevel, timestampCavity2);
            Table.AddRow(FaiItemsCavity1, Cavity1ProductLevel, timestampCavity1);
        }


        private void LoadFaiItems()
        {
            // load all fai names 
            _names2d = Get2DFaiNames();
            _names3d = ParseFaiNames(DirectoryConstants.FaiNamesDir, NameConstants.FaiNamesFile3D);

            // Load fai items configs
            FaiItems2DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_names2d,
                    DirectoryConstants.FaiConfigDir2DLeft)
                .ToList();
            FaiItems2DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_names2d,
                    DirectoryConstants.FaiConfigDir2DRight)
                .ToList();
            FaiItems3DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_names3d,
                    DirectoryConstants.FaiConfigDir3DLeft)
                .ToList();
            FaiItems3DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(_names3d,
                    DirectoryConstants.FaiConfigDir3DRight)
                .ToList();

            FaiItemsCavity1 = FaiItems2DLeft.ConcatNew(FaiItems3DLeft);
            FaiItemsCavity2 = FaiItems2DRight.ConcatNew(FaiItems3DRight);
        }

        private List<string> Get2DFaiNames()
        {
            return I40Check.OnGetResultDefNameStr().Take(I40Check.YouXiaoFAINum).ToList();
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
        private ProductLevel GetProductLevel(bool itemExists, IEnumerable<FaiItem> faiItems)
        {
            if (!itemExists) return ProductLevel.Empty;
            return faiItems.Any(item => item.Rejected) ? ProductLevel.Ng2 : ProductLevel.OK;
        }

        /// <summary>
        /// Update ValueUnbiased of fai-items from dictionaries
        /// </summary>
        /// <param name="faiItems"></param>
        /// <param name="faiResultDict"></param>
        /// <param name="itemExists"></param>
        private void UpdateFaiItems(List<FaiItem> faiItems, Dictionary<string, double> faiResultDict, bool itemExists)
        {
            if (faiItems == null) throw new ArgumentNullException(nameof(faiItems));
            if (itemExists)
            {
                var faiNames = faiResultDict.Keys;
                foreach (var name in faiNames)
                {
                    faiItems.ByName(name).ValueUnbiased = faiResultDict[name];
                }
            }
            else
            {
                foreach (var item in faiItems)
                {
                    item.ValueUnbiased = 999;
                }
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
        private void ProcessImages2D(List<HImage> images)
        {
            LogRoutine($"Received {images.Count} 2d images");
            if (!Server.IsAutoRunning) return;
            CavityType currentArrivedSocket2D;
            lock (_lockerOfCurrentArrivedSocket2D)
            {
                currentArrivedSocket2D = CurrentArrivedSocket2D;
            }


            int itemIndexSinceReset;
            lock (_lockerOfRightSocketIndexSinceReset2D)
            {
                itemIndexSinceReset = currentArrivedSocket2D == CavityType.Cavity2
                    ? _rightSocketIndexSinceReset2D
                    : _rightSocketIndexSinceReset2D + 1;
            }


            LogRoutine($"2D processing starts for {currentArrivedSocket2D}");
            GraphicsPackViewModel result;
            
            try
            {
                result = I40Check.Execute(currentArrivedSocket2D.ToChusIndex(), images);
            }
            catch
            {
                result = new GraphicsPackViewModel {Images = images, FaiResults = GenErrorFaiResults(_names2d)};
                LogRoutine($"2D processing for {currentArrivedSocket2D} errored");
            }

            LogRoutine($"2D processing ends for {currentArrivedSocket2D}");

            lock (_lockerOfResultQueues2D)
            {
                _resultQueues2D[currentArrivedSocket2D].Enqueue(result);
                var lastInCavity1 = _resultQueues2D[CavityType.Cavity1].LastOrDefault();
                var lastInCavity2 = _resultQueues2D[CavityType.Cavity2].LastOrDefault();
                var all2DProcessingForThisRunIsDone = lastInCavity1 != null && lastInCavity2 != null;
                if (all2DProcessingForThisRunIsDone)
                {
                    LogRoutine("All 2D images have processed");
                    ResultReady2D = ResultStatus.Ready;
                    Server.NotifyPlcReadyToGoNextLoop();
                }
            }
        }

        public void LogPlcMessage(string message)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                PlcMessageList.Add(
                    new LoggingMessageItem {Message = message, Time = DateTime.Now.ToString("T")});
            });
        }

        public void LogRoutine(string message)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                RoutineMessageList.Add(
                    new LoggingMessageItem {Message = message, Time = DateTime.Now.ToString("T")});
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

        public void EnableSynchronization()
        {
            BindingOperations.EnableCollectionSynchronization(RoutineMessageList, _lockerOfRoutineMessageList);

            BindingOperations.EnableCollectionSynchronization(PlcMessageList, _lockerOfPlcMessageList);
        }

        public void LoadFiles()
        {
            Scan2DConfigFolders();

            LoadI40CheckConfigs();

            LoadShapeModels();

            LoadFaiItems();

            LoadProductionLineSummaries();
        }

        private void LoadProductionLineSummaries()
        {
            var binNames = new List<string>
            {
                ProductLevel.OK.ToString(), ProductLevel.Ng2.ToString(), ProductLevel.Ng3.ToString(),
                ProductLevel.Ng4.ToString(), ProductLevel.Ng5.ToString()
            };
            Summary = new ProductionLineSummary(DirectoryConstants.ProductionLineRecordDir, binNames);
            OnPropertyChanged(nameof(Summary));
        }

        private void Scan2DConfigFolders()
        {
            var dirNamesInConfigDir = Directory.GetDirectories(DirectoryConstants.Config2DDir).Select(Path.GetFileName)
                .ToList();
            Config2dDirList = dirNamesInConfigDir;
        }


        private List<string> ParseFaiNames(string dir, string fileName)
        {
            var filePath = Path.Combine(dir, fileName);
            var names = File.ReadAllText(filePath).Split(',').Select(ele => ele.Trim());
            return names.ToList();
        }

        private void LoadI40CheckConfigs()
        {
            var i40ConfigDir2d = Path.Combine(DirectoryConstants.Config2DDir, Config2dDirList[Config2dDirIndex]);
            I40Check = new I40Check(i40ConfigDir2d, "I40");
        }

        private Dictionary<string, double> GenErrorFaiResults(List<string> faiNames)
        {
            var output = new Dictionary<string, double>();
            foreach (var name in faiNames)
            {
                output[name] = 999;
            }

            return output;
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
        public void Cleanup()
        {
            TopCamera.StopGrabbing();
            TopCamera.Close();

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.IsConnectedHighSpeed = false;
            }

            Server.Disconnect();
            Summary.SerializeCurrentSummary();
        }


        #region Properties

        public int RoundCountSinceReset { get; set; }

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

        /// <summary>
        /// 2D fai items from left or right socket
        /// </summary>
        public List<FaiItem> FaiItems2D { get; set; }


        public CavityType CurrentArrivedSocket2D { get; set; }
        public ProductLevel Cavity1ProductLevel { get; set; }
        public ProductLevel Cavity2ProductLevel { get; set; }


        /// <summary>
        /// Current application page
        /// </summary>
        public ApplicationPageType CurrentApplicationPage { get; set; }


        /// <summary>
        /// 2D camera object
        /// </summary>
        public HKCameraViewModel TopCamera { get; set; }


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

        public List<FaiItem> FaiItemsCavity1 { get; set; }
        public List<FaiItem> FaiItemsCavity2 { get; set; }

        public List<FaiItem> FaiItems2DLeft { get; set; }
        public List<FaiItem> FaiItems2DRight { get; set; }
        public List<FaiItem> FaiItems3DLeft { get; set; }
        public List<FaiItem> FaiItems3DRight { get; set; }


        public I40Check I40Check { get; set; }


        public bool ShouldMessageBoxPopup { get; set; }

        public LoggingMessageItem WaringMessageHighLevel
        {
            get { return _waringMessageHighLevel; }
            set
            {
                _waringMessageHighLevel = value;
                ShouldMessageBoxPopup = true;
            }
        }

        public ICommand OpenCSVDirCommand { get; set; }
        public ICommand OpenImageDirCommand { get; set; }

        public bool ShouldSave2DImagesLeft { get; set; }
        public bool ShouldSave2DImagesRight { get; set; }
        public bool ShouldSave3DImagesLeft { get; set; }
        public bool ShouldSave3DImagesRight { get; set; }

        public List<string> Config2dDirList { get; set; }

        public int Config2dDirIndex
        {
            get { return _config2dDirIndex; }
            set
            {
                _config2dDirIndex = value;
                LoadI40CheckConfigs();
            }
        }

        public FaiTableStackViewModel Table { get; set; }

        public static Dispatcher UiDispatcher => System.Windows.Application.Current.Dispatcher;

        public ProductionLineSummary Summary { get; set; }

        public ResultStatus ResultReady2D { get; set; }

        public ICommand SimulateCommand { get; set; }

        #endregion
    }
}