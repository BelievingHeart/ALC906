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
using HKCameraDev.Core.IoC.Interface;
using HKCameraDev.Core.ViewModels.Camera;
using I40_3D_Test;
using LJX8000.Core.ViewModels.Controller;
using PLCCommunication.Core.Enums;
using PLCCommunication.Core.ViewModels;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;
using CameraTriggerSourceType = HKCameraDev.Core.Enums.CameraTriggerSourceType;
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

        public GraphicsPackViewModel Graphics2DLeft { get; set; } = new GraphicsPackViewModel();
        public GraphicsPackViewModel Graphics2DRight { get; set; } = new GraphicsPackViewModel();

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
            InitContainer();
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
            OpenImageDirCommand = new RelayCommand(() =>
            {
                Process.Start(Directory.GetCurrentDirectory());
            });
            
            
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
            lock (_lockerOfPlcMessageList)
            {
                ClearMessagesIfOverflows(PlcMessageList, 100);
            }

            lock (_lockerOfRoutineMessageList)
            {
                ClearMessagesIfOverflows(RoutineMessageList, 100);
            }

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

        private void InitContainer()
        {
            HKCameraDev.Core.IoC.IoC.Kernel.Bind<IHKCameraLibLogger>().ToConstant(new CameraMessageLogger());
        }


        private void SetupServer()
        {
            Server = new AlcServerViewModel
            {
                IpAddress = IPAddress.Parse("192.168.100.100"),
                Port = 4000
            };
            Server.AutoRunStartRequested += OnNewRoundStarted;
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
            WaringMessageHighLevel = new LoggingMessageItem(){Time = DateTime.Now.ToString("hh:mm:ss t z"), Message = s};
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
                var lastInLeft = _resultQueues2D[CavityType.Cavity1].LastOrDefault();
                var isFirstRoundAfterReset = lastInLeft == null;
                if (isFirstRoundAfterReset) return;

                lastInLeft = _resultQueues2D[CavityType.Cavity1].Dequeue();
                _resultQueues2D[CavityType.Cavity1].Clear();
                _resultQueues2D[CavityType.Cavity1].Enqueue(lastInLeft);

                var lastInRight = _resultQueues2D[CavityType.Cavity2].Dequeue();
                _resultQueues2D[CavityType.Cavity2].Clear();
                _resultQueues2D[CavityType.Cavity2].Enqueue(lastInRight);
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
                    CurrentArrivedSocket2D = CavityType.Cavity1;
                    LogPlcMessage("2D left socket arrived");
                    break;
                case PlcMessagePackConstants.CommandIdRightSocketArrived:
                    CurrentArrivedSocket2D = CavityType.Cavity2;
                    LogPlcMessage("2D right socket arrived");
                    break;
            }
        }

        private void SetupCameras()
        {
            HKCameraManager.ScannedForAttachedCameras();
            TopCamera = HKCameraManager.AttachedCameras.First(cam => cam.Name == NameConstants.TopCameraName);
            TopCamera.ImageBatchSize = 6;
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
                result3D =  _procedure3D.Execute(imagesForOneSocket, _shapeModel3D);
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
            SummaryToday.Update(Cavity1ProductLevel);
            SummaryToday.Update(Cavity2ProductLevel);

            SummaryCurrentHour.Update(Cavity1ProductLevel);
            SummaryCurrentHour.Update(Cavity2ProductLevel);
        }

        /// <summary>
        /// Send product level information to plc
        /// </summary>
        private void SubmitProductLevels()
        {
            Cavity1ProductLevel = GetProductLevel(Graphics3DLeft.ItemExists, FaiItemsLeft);
            Cavity2ProductLevel = GetProductLevel(Graphics3DRight.ItemExists, FaiItemsRight);
            Server.SendProductLevels(Cavity1ProductLevel, Cavity2ProductLevel);
        }


        private void SerializeCsv()
        {
            _serializerLeft.Serialize(FaiItemsLeft, DateTime.Now.ToString("T"));
            _serializerRight.Serialize(FaiItemsRight, DateTime.Now.ToString("T"));
            
            // Write right and left
            _serializerAll.Serialize(FaiItemsRight, DateTime.Now.ToString("T"));
            _serializerAll.Serialize(FaiItemsLeft, DateTime.Now.ToString("T"));
        }

        /// <summary>
        /// Convert dictionaries from image processing units to list of fai items
        /// and display them
        /// </summary>
        private void Combine2D3DResults()
        {
            // Update _results2D
            lock (_lockerOfResultQueues2D)
            {
                // Corresponding results are put to the head of queues
                // when new round starts
                // No worry about this
                Graphics2DLeft = _resultQueues2D[CavityType.Cavity1].Dequeue();
                Graphics2DRight = _resultQueues2D[CavityType.Cavity2].Dequeue();
            }

            Task.Run(()=> SerializeImages(Graphics2DLeft.Images, Graphics2DRight.Images, _imagesToSerialize3dLeft, _imagesToSerialize3dRight));

            var faiResultDictLeft = ConcatDictionaryNew(Graphics2DLeft.FaiResults,
                Graphics3DLeft.FaiResults);
            var faiResultDictRight = ConcatDictionaryNew(Graphics2DRight.FaiResults,
                Graphics3DRight.FaiResults);

            // To avoid frequent context switching
            // Wrap all the UI-updating code in single Invoke block
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                // Update fai item lists using dictionaries from image processing modules
                UpdateFaiItems(FaiItemsLeft, faiResultDictLeft, Graphics3DLeft.ItemExists);
                UpdateFaiItems(FaiItemsRight, faiResultDictRight, Graphics3DRight.ItemExists);
                
                // Notify Ui
                OnPropertyChanged(nameof(FaiItems2DLeft));
                OnPropertyChanged(nameof(FaiItems2DRight));
                OnPropertyChanged(nameof(FaiItems3DLeft));
                OnPropertyChanged(nameof(FaiItems3DRight));
                OnPropertyChanged(nameof(FaiItemsLeft));
                OnPropertyChanged(nameof(FaiItemsRight));
            });
        }

        private void SerializeImages(List<HImage> images2dCavity1, List<HImage> images2dCavity2, List<HImage> images3dCavity1, List<HImage> images3dCavity2)
        {
            SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity2, images3dCavity2, ShouldSave2DImagesRight, 
                ShouldSave3DImagesRight, DirectoryConstants.ImageDirRight);
            
            SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity1, images3dCavity1, ShouldSave2DImagesLeft, 
                ShouldSave3DImagesLeft, DirectoryConstants.ImageDirLeft);
        }


        private void LoadFaiItems()
        {
                // load all fai names 
            List<string> names2d = Get2DFaiNames();
               List<string> names3d = ParseFaiNames(DirectoryConstants.FaiNamesDir, NameConstants.FaiNamesFile3D);

               // Load fai items configs
            FaiItems2DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(names2d,
                    DirectoryConstants.FaiConfigDir2DLeft)
                .ToList();
            FaiItems2DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(names2d,
                    DirectoryConstants.FaiConfigDir2DRight)
                .ToList();
            FaiItems3DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(names3d,
                    DirectoryConstants.FaiConfigDir3DLeft)
                .ToList();
            FaiItems3DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(names3d,
                    DirectoryConstants.FaiConfigDir3DRight)
                .ToList();

            FaiItemsLeft = FaiItems2DLeft.ConcatNew(FaiItems3DLeft);
            FaiItemsRight = FaiItems2DRight.ConcatNew(FaiItems3DRight);
        }

        private List<string> Get2DFaiNames()
        {
            return  I40Check.OnGetResultDefNameStr().Take(I40Check.YouXiaoFAINum).ToList();
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
            if (!itemExists) return ProductLevel.Ng4;
            return faiItems.Any(item => item.Rejected) ? ProductLevel.Ng2 : ProductLevel.Ok;
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
                    item.ValueUnbiased = double.NaN;
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
                foreach (var result in  resultDict)
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




            Task.Run(() =>
            {
                LogRoutine($"2D processing starts for {currentArrivedSocket2D} socket");
                GraphicsPackViewModel result;
                try
                {
                    result = I40Check.Execute(currentArrivedSocket2D.ToChusIndex(), images);
                }
                catch
                {
                    result = GraphicsPackViewModel.Stub;
                    result.Images = images;
                    LogRoutine($"2D processing for {currentArrivedSocket2D} socket errored");
                }
                
                lock (_lockerOfResultQueues2D)
                {
                    _resultQueues2D[currentArrivedSocket2D].Enqueue(result);
                }

                LogRoutine($"2D processing ends for {currentArrivedSocket2D} socket");
            });
        }

        public void LogPlcMessage(string message)
        {
            PlcMessageList.LogMessageRetryIfFailed(
                new LoggingMessageItem {Message = message, Time = DateTime.Now.ToString("T")},
                _lockerOfPlcMessageList, 20);
        }

        public void LogRoutine(string message)
        {
            RoutineMessageList.LogMessageRetryIfFailed(
                new LoggingMessageItem {Message = message, Time = DateTime.Now.ToString("T")},
                _lockerOfRoutineMessageList, 20);
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

        private void Scan2DConfigFolders()
        {
            var dirNamesInConfigDir = Directory.GetDirectories(DirectoryConstants.Config2DDir).Select(Path.GetFileName).ToList();
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
            I40Check = new I40Check( i40ConfigDir2d, "I40");
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


        public List<FindLineParam> FindLineParams2D { get; set; }


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

        #endregion
    }
}