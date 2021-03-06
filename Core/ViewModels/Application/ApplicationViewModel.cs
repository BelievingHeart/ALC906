﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using _2DI40Check;
using Core.Commands;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ImageProcessing._2D;
using Core.IoC.Loggers;
using Core.IoC.PlcErrorParser;
using Core.ViewModels.Database.FaiCollection;
using Core.ViewModels.Fai;
using Core.ViewModels.Fai.FaiYieldCollection;
using Core.ViewModels.Login;
using Core.ViewModels.Popup;
using Core.ViewModels.Results;
using Core.ViewModels.Summary;
using Core.ViewModels.TimeLine;
using CYG906ALC.ALG;
using HalconDotNet;
using HKCamera;
using I40_3D_Test;
using LJX8000.Core.ViewModels.Controller;
using PLCCommunication.Core;
using PLCCommunication.Core.Enums;
using PLCCommunication.Core.ViewModels;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.Logger.Message;
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
        public static ApplicationViewModel Instance
        {
            get { return _instance; }
        }

        private static ApplicationViewModel _instance;

        private readonly Object _lockerOfLaserImageBuffers = new Object();

        private bool _readyToEnterNextRun = true;

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


        private HTuple _shapeModel2D, _shapeModel3D;

        private CsvSerializer.CsvSerializer _serializerAll;

        private CsvSerializer.CsvSerializer _serializerLeft =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity1"));


        private CsvSerializer.CsvSerializer _serializerRight =
            new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity2"));

        private IMeasurementProcedure3D _procedure3D;


        private readonly Dictionary<CavityType, Queue<GraphicsPackViewModel>> _resultQueues2D =
            new Dictionary<CavityType, Queue<GraphicsPackViewModel>>
            {
                [CavityType.Cavity1] = new Queue<GraphicsPackViewModel>(),
                [CavityType.Cavity2] = new Queue<GraphicsPackViewModel>()
            };

        private readonly object _lockerOfResultQueues2D = new object();
        private bool _isAllControllersHighSpeedConnected;

        #region ctor

        protected ApplicationViewModel()
        {
            // Init logger
            Logger.Init();
            
            CurrentApplicationPage = ApplicationPageType.Home;
            Server = new AlcServerViewModel();

            Logger.Instance.StartPopupQueue();
            Logger.Instance.ShouldMessageBoxPopupChanged += OnShouldWarningMessageBoxPopupChanged;

            InitCommands();

            
            // Summary and time line
            TimeLineManager = new TimeLineManagerViewModel(DirectoryConstants.TimeLineSerializationDir);
            Summary = new SummaryViewModel() {StartTime = DateTime.Now};
        }

        private void OnShouldWarningMessageBoxPopupChanged(bool shouldPopup)
        {
            if (shouldPopup) Server.IsContinueAllowed = false;
            if (!shouldPopup && !Logger.HasPopupsUnhandled) Server.IsContinueAllowed = true;
        }

        #endregion

        private void InitCommands()
        {

            SimulateCommand = new RelayCommand(DoSimulation);

            SwitchMtmCommand = new SimpleCommand(o => { CurrentProductType = ProductType.Mtm; },
                o => CurrentProductType != ProductType.Mtm);
            SwitchAlpsCommand = new SimpleCommand(o => { CurrentProductType = ProductType.Alps; },
                o => CurrentProductType != ProductType.Alps);

            ClearProductErrorStatesCommand = new SimpleCommand(
                o => Server.SentToPlc(PlcMessagePackConstants.ClearProductErrorStateMessagePack,
                    PlcMessageType.Request),
                o => !Server.IsAutoRunning);

            CloseMainWindowCommand = new RelayCommand(AskIfCloseIntentionally);

            Backup3DConfigsCommand = new RelayCommand(Backup3DConfigs);
            Recover3DConfigsCommand = new RelayCommand(Recover3DConfigs);
            
            OpenTimelineDialogCommand = new RelayCommand(()=>TimeLineManager.ShouldDialogOpen = true);
            
            InsertNewTimelineCommand = new ParameterizedCommand(obj =>
            {
                var comment = (string) obj;
                TimeLineManager.AddTimeLineItem(comment);
                Summary.ClearSummary();
            });
        }

        private void Recover3DConfigs()
        {
            Directory.CreateDirectory(DirectoryConstants.BackupDir3DConfigs);
            Directory.CreateDirectory(DirectoryConstants.FaiConfigDir3D);
//Copy all the files & Replaces any files with the same name
            foreach (string oldPath in Directory.GetFiles(DirectoryConstants.BackupDir3DConfigs, "*.*",
                SearchOption.AllDirectories))
            {
                var dstPath = oldPath.Replace(DirectoryConstants.BackupDir3DConfigs, DirectoryConstants.FaiConfigDir3D);
                var closestDir = Directory.GetParent(dstPath).FullName;
                Directory.CreateDirectory(closestDir);
                File.Copy(oldPath, dstPath
                    , true);
            }

            ApplicationConfigs.LastRecoverDate3D = DateTime.Now;
            Logger.LogStateChanged("还原成功");
        }

        private void Backup3DConfigs()
        {
            Directory.CreateDirectory(DirectoryConstants.BackupDir3DConfigs);
            Directory.CreateDirectory(DirectoryConstants.FaiConfigDir3D);

            foreach (string oldPath in Directory.GetFiles(DirectoryConstants.FaiConfigDir3D, "*.*",
                SearchOption.AllDirectories))
            {
                var dstPath = oldPath.Replace(DirectoryConstants.FaiConfigDir3D, DirectoryConstants.BackupDir3DConfigs);
                var closestDir = Directory.GetParent(dstPath).FullName;
                Directory.CreateDirectory(closestDir);
                File.Copy(oldPath, dstPath, true);
            }
            
            ApplicationConfigs.LastBackupDate3D = DateTime.Now;
            Logger.LogStateChanged("备份成功");
        }

        private void AskIfCloseIntentionally()
        {
            var popup = new PopupViewModel
            {
                OkCommand = new CloseDialogAttachedCommand(o => true, CloseMainWindow),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () => { }),
                IsSpecialPopup = true,
                MessageItem = new LoggingMessageItem("是否退出ALC?"),
                OkButtonText = "是",
                CancelButtonText = "否"
            };
            Logger.EnqueuePopup(popup);
        }

        private void CloseMainWindow()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                System.Windows.Application.Current.MainWindow.Close();
        }

        /// <summary>
        /// Do temporary simulations
        /// </summary>
        private void DoSimulation()
        {
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
            Server.IpAddress = IPAddress.Parse("192.168.100.100");
            Server.Port = 4000;

            Server.PlcRequestToEnterNewRun += OnPlcRequestToEnterNewRun;
            Server.PlcStopRequested += () => Logger.LogPlcMessage("收到停止请求");
            Server.PlcResetRequested += OnPlcInitRequested;
            Server.ClientHooked += OnPlcHooked;
            Server.CustomCommandReceived += PlcCustomCommandHandler;
            Server.PlcResetFinished += OnPlcResetFinished;
            Server.IsAutoRunningChanged += isAutoRunning =>
            {
                if (isAutoRunning)
                {
                    ResetStates();
                    Logger.LogPlcMessage("进入自动模式");
                }
            };
            Server.MessageSendFailed += () => { Logger.LogHighLevelWarningNormal("发送消息失败"); };
            Server.MessagePackReceived += messagePack =>
            {
                Task.Run(() =>
                    Logger.Instance.LogMessageToFile($"收到代号{messagePack.CommandId}",
                        Logger.AllCommandIdsFromPlcFilePath));
            };

            Server.MessagePackSent += messagePack =>
            {
                Task.Run(() =>
                    Logger.Instance.LogMessageToFile($"发送代号:{messagePack.CommandId}.",
                        Logger.AllCommandIdsToPlcFilePath));
            };


            var errorParser = new PlcErrorParser(Path.Combine(DirectoryConstants.ConfigDirectory, "ErrorSheet.csv"));
            errorParser.WarningL1Emit += OnWarningL1Received;
            errorParser.WarningL2Emit += OnWarningL2Received;
            errorParser.WarningL3Emit += OnWarningL3Received;
            errorParser.WarningL4Emit += OnWarningL4Received;
            Server.ErrorParser = errorParser;
        }


        private void OnPlcInitRequested()
        {
            Logger.LogPlcMessage("收到复位请求");
        }


        private void OnWarningL4Received(string message, long l)
        {
            //  Init must be able to execute after L4 warning received
            Server.IsAutoRunning = false;
            Server.IsBusyResetting = false;
            Server.IsStopping = false;
            Server.IsPausing = false;
            Server.CurrentMachineState = MachineState.Idle;

            Logger.Instance.LogMessageToFile(message, Logger.HighLevelWarningFilePath);
            Logger.LogHighLevelWarningNormal(message);
        }

        private void OnWarningL3Received(string message, long errorCode)
        {
            if (Server.IsAutoRunning) Server.PauseCommand.Execute(null);
            Logger.Instance.LogMessageToFile(message, Logger.HighLevelWarningFilePath);

            if (PlcErrorParser.IsClearProductWarningCode(errorCode))
            {
                Logger.LogUnhandledPlcMessage($"收到特殊代号{errorCode}");
                var popupViewModel = PopupHelper.CreateClearProductPopup(message, errorCode, Server);
                Logger.LogHighLevelWarningSpecial(popupViewModel);
            }
            else if (PlcErrorParser.IsSafeDoorOpenWarningCode(errorCode))
            {
                Logger.LogUnhandledPlcMessage($"收到特殊代号{errorCode}");
                var popupViewModel = PopupHelper.CreateSafeDoorPopup(message, Server);
                Logger.LogHighLevelWarningSpecial(popupViewModel);
            }
            else
            {
                Logger.LogHighLevelWarningNormal(message);
            }
        }

        private void OnWarningL2Received(string message, long l)
        {
            Logger.Instance.LogMessageToFile(message, Logger.HighLevelWarningFilePath);
            Logger.LogHighLevelWarningNormal(message);
        }

        private void OnWarningL1Received(string message, long l)
        {
            Logger.Instance.LogMessageToFile(message, Logger.HighLevelWarningFilePath);
        }

        private void OnPlcHooked(Socket socket)
        {
            Logger.LogPlcMessage(socket.RemoteEndPoint + "已连接");
        }

        private void OnPlcRequestToEnterNewRun()
        {
            Logger.LogPlcMessage("新一轮开始");

            // Reply plc
            Server.SentToPlc(new PlcMessagePack()
            {
                CommandId = 23, MsgType = PlcMessagePack.RespondIndicator, Param1 = _readyToEnterNextRun ? 0 : 1,
                Param2 = ShotsPerExecution2D == 2 ? 0 : 1
            });

            if (!_readyToEnterNextRun)
            {
                Show2DAcquisitionErrorPopup();
                return;
            }

            // Clear run related states
            ClearLaserImagesForNewRound();
            Dequeue2DImagesFromPreviousRound();

             _readyToEnterNextRun = false;
             ResultReady2D = ResultStatus.Waiting;
                 CurrentArrivedSocket2D = CavityType.Cavity2;
        }

        /// <summary>
        /// When starting a new round
        /// the last one in the queue must be the one
        /// that will be concluded with 3D results in the new round
        /// </summary>
        private void Dequeue2DImagesFromPreviousRound()
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
                    _graphics2DCavity1 = _resultQueues2D[CavityType.Cavity1].Dequeue();
                    _graphics2DCavity2 = _resultQueues2D[CavityType.Cavity2].Dequeue();

                    if (_resultQueues2D[CavityType.Cavity1].Count != 0 ||
                        _resultQueues2D[CavityType.Cavity2].Count != 0)
                    {
                        Show2DAcquisitionErrorPopup();
                    }
                }
            }
        }

        private void Show2DAcquisitionErrorPopup([CallerLineNumber]int lineNumber= 0)
        {
            var popup = new PopupViewModel
            {
                OkCommand = new CloseDialogAttachedCommand(o => true,
                    () => { Server.SentToPlc(PlcMessagePack.AbortMessage); }),
                IsSpecialPopup = false,
                Content = $"2D相机不能正常取像{lineNumber}，请复位",
                OkButtonText = "确定"
            };
            Server.IsAutoRunning = false;
            Logger.EnqueuePopup(popup);
        }

        private void OnPlcResetFinished()
        {
            Logger.LogStateChanged("PLC复位完成");
            UiDispatcher.InvokeAsync(CommandManager.InvalidateRequerySuggested);

            // Reopen camera
            try
            {
                SetupCameras();
            }
            catch
            {
                var popup = new PopupViewModel
                {
                    OkCommand = new CloseDialogAttachedCommand(o => true, CloseMainWindow),
                    IsSpecialPopup = false,
                    Content = "2D相机不能正常取像，请重启ALC",
                    OkButtonText = "确定"
                };
                Logger.EnqueuePopup(popup);
            }
        }

        private void ResetStates()
        {
            lock (_lockerOfResultQueues2D)
            {
                _resultQueues2D[CavityType.Cavity1].Clear();
                _resultQueues2D[CavityType.Cavity2].Clear();
            }

            RoundCountSinceReset = 0;

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.ClearBuffer();
            }

            _readyToEnterNextRun = true;
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
                    Logger.LogUnhandledPlcMessage($"未处理代号{commandId}");
                    break;
            }
        }

        private void OnCavity1Arrived2D()
        {
            Logger.LogPlcMessage("2D cavity1到达");
        }

        private void OnCavity2Arrived2D()
        {
            Logger.LogPlcMessage("2D cavity2到达");
        }

        /// <summary>
        /// Open or reopen camera
        /// </summary>
        private void SetupCameras()
        {
            if (TopCamera != null)
            {
                TopCamera.ImageBatchCollected -= ProcessImages2D;
                TopCamera.ImageAcquisitionEnd -= OnTopCameraOnImageAcquisitionEnd;
                try
                {
                    TopCamera.Close();
                }
                catch
                {
                    //Don't care
                }
            }


            TopCamera = new HKCameraViewModel {ImageBatchSize = 2};
            TopCamera.ImageBatchCollected += ProcessImages2D;
            TopCamera.ImageAcquisitionEnd += OnTopCameraOnImageAcquisitionEnd;
            TopCamera.Open();
            TopCamera.SetTriggerToLine0();
            TopCamera.StartGrabbing();
        }

        private void OnTopCameraOnImageAcquisitionEnd()
        {
            if (!Server.IsConnected) return;
            // Else image acquisition failed
            Server.Disconnect();
            Logger.LogHighLevelWarningNormal("2D相机不能正常取像，请重启ALC");
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


            Logger.LogRoutineMessageAsync($"3D处理开始-{enumValue}");
            // Do processing for one socket and get its result
            var imagesForOneSocket =
                _laserImageBuffers.Values.Select(list => list[socketIndex]).ToList();

            // Save images for later serialization when 2d and 3d combine
            if (enumValue == CavityType.Cavity1) _imagesToSerialize3dCavity1 = imagesForOneSocket;
            else _imagesToSerialize3dCavity2 = imagesForOneSocket;
            
            MeasurementResult3D result3D = null;
            lock (_lockerOf3DProc)
            {
                try
                {
                    result3D = _procedure3D.Execute(imagesForOneSocket, _shapeModel3D,
                        enumValue == CavityType.Cavity1 ? 1 : 2);
                }
                catch (Exception e)
                {
                    result3D = new MeasurementResult3D()
                    {
                        FaiResults = GenErrorFaiResults(_procedure3D.FaiNames),
                        CompositeImage = imagesForOneSocket,
                        ErrorMessage = e.Message
                    };
                    // Log error images
                    var logDir = Path.Combine(DirectoryConstants.ImageDir3D,
                        "Error/" + DateTime.Now.ToString(NameConstants.DateTimeFormat));
                    Task.Run(() => { Logger.Instance.RecordErrorImages(imagesForOneSocket, e.Message, logDir); });
                }
            }

            if (OutputRawData3D && result3D.ItemExists && string.IsNullOrEmpty(result3D.ErrorMessage)) 
            {
                var rawFilePath = Path.Combine(DirectoryConstants.ErrorLogDir, $"{enumValue}-3D-raw.csv");
                Task.Run(() => SerializationHelper.LogDataDict(result3D.FaiResults, rawFilePath));
            }

            if (socketIndex == (int) CavityType.Cavity1)
            {
                Graphics3DCavity1 = result3D.GetGraphics();
            }
            else
            {
                Graphics3DCavity2 = result3D.GetGraphics();
            }

            Logger.LogRoutineMessageAsync($"3D处理完成-{enumValue}");


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

        private List<HImage> _imagesToSerialize3dCavity2;

        private List<HImage> _imagesToSerialize3dCavity1;
        private GraphicsPackViewModel _graphics2DCavity1 = new GraphicsPackViewModel();
        private GraphicsPackViewModel _graphics2DCavity2 = new GraphicsPackViewModel();
        private ProductType _currentProductType;
        private Dictionary<string, double> _faiResultDictCavity1;
        private Dictionary<string, double> _faiResultDictCavity2;
        private int _shotsPerExecution2D;
        private readonly object _lockerOf2DProc = new object();
        private readonly object _lockerOf3DProc = new object();

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
                Task.Run(() => SerializeImagesAndData(Graphics2DCavity1.Images, Graphics2DCavity2.Images,
                    _imagesToSerialize3dCavity1, _imagesToSerialize3dCavity2,
                    FaiItemsCavity1, FaiItemsCavity2));
            }

            // Round finished, increment round count
            RoundCountSinceReset++;
        }

        private void UpdateSummaries()
        {
            Summary.UpdateSummary(ProductLevelCavity1, ProductLevelCavity2);
            Summary.UpdateYieldCollection(
                ProductLevelCavity2 == ProductLevel.Empty || ProductLevelCavity2 == ProductLevel.Ng5 ||
                ProductLevelCavity2 == ProductLevel.Ng3
                    ? null
                    : FaiItemsCavity2,
                ProductLevelCavity1 == ProductLevel.Empty || ProductLevelCavity1 == ProductLevel.Ng5 ||
                ProductLevelCavity1 == ProductLevel.Ng3
                    ? null
                    : FaiItemsCavity1);
        }

        /// <summary>
        /// Send product level information to plc
        /// </summary>
        private void SubmitProductLevels()
        {
            var cavity1Errored = Graphics2DCavity1.ErrorMessage != null || Graphics3DCavity1.ErrorMessage != null;
            ProductLevelCavity1 = GetProductLevel(Graphics3DCavity1.ItemExists, cavity1Errored, FaiItemsCavity1);

            var cavity2Errored = Graphics2DCavity2.ErrorMessage != null || Graphics3DCavity2.ErrorMessage != null;
            ProductLevelCavity2 = GetProductLevel(Graphics3DCavity2.ItemExists, cavity2Errored, FaiItemsCavity2);

            Server.SendProductLevels(ProductLevelCavity1, ProductLevelCavity2);
        }


        /// <summary>
        /// Convert dictionaries from image processing units to list of fai items
        /// and display them
        /// </summary>
        private void Combine2D3DResults()
        {
            _faiResultDictCavity1 = ConcatDictionaryNew(Graphics2DCavity1.FaiResults,
                Graphics3DCavity1.FaiResults);
            _faiResultDictCavity2 = ConcatDictionaryNew(Graphics2DCavity2.FaiResults,
                Graphics3DCavity2.FaiResults);

            // To avoid frequent context switching
            // Wrap all the UI-updating code in single Invoke block
            UiDispatcher.Invoke(() =>
            {
                // Update fai item lists using dictionaries from image processing modules
                UpdateFaiItems(FaiItemsCavity1, _faiResultDictCavity1, Graphics3DCavity1.ItemExists);
                UpdateFaiItems(FaiItemsCavity2, _faiResultDictCavity2, Graphics3DCavity2.ItemExists);

                // Notify Ui
                OnPropertyChanged(nameof(FaiItems2DLeft));
                OnPropertyChanged(nameof(FaiItems2DRight));
                OnPropertyChanged(nameof(FaiItems3DLeft));
                OnPropertyChanged(nameof(FaiItems3DRight));
                OnPropertyChanged(nameof(FaiItemsCavity1));
                OnPropertyChanged(nameof(FaiItemsCavity2));

                // Ensure 2d and 3d graphics are updated with approximate time
                OnPropertyChanged(nameof(Graphics2DCavity1));
                OnPropertyChanged(nameof(Graphics2DCavity2));
            });
        }

        private void SerializeImagesAndData(List<HImage> images2dCavity1, List<HImage> images2dCavity2,
            List<HImage> images3dCavity1, List<HImage> images3dCavity2, List<FaiItem> faiItemsCavity1,
            List<FaiItem> faiItemsCavity2)
        {
            // Cavity 2
            if (ProductLevelCavity2 != ProductLevel.Empty)
            {
                TimestampCavity2 = SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity2, images3dCavity2,
                    ShouldSave2DImagesRight,
                    ShouldSave3DImagesRight, CavityType.Cavity2, SaveNgImagesOnly, ProductLevelCavity2);
                _serializerRight.Serialize(faiItemsCavity2, TimestampCavity2.ToString(NameConstants.DateTimeFormat),
                    ProductLevelCavity2.GetResultText());
                _serializerAll.Serialize(faiItemsCavity2, TimestampCavity2.ToString(NameConstants.DateTimeFormat),
                    ProductLevelCavity2.GetResultText());
            }


            // Cavity 1
            if (ProductLevelCavity1 != ProductLevel.Empty)
            {
                TimestampCavity1 = SerializationHelper.SerializeImagesWith2D3DMatched(images2dCavity1, images3dCavity1,
                    ShouldSave2DImagesLeft,
                    ShouldSave3DImagesLeft, CavityType.Cavity1, SaveNgImagesOnly, ProductLevelCavity1);
                _serializerLeft.Serialize(faiItemsCavity1, TimestampCavity1.ToString(NameConstants.DateTimeFormat),
                    ProductLevelCavity1.GetResultText());
                _serializerAll.Serialize(faiItemsCavity1, TimestampCavity1.ToString(NameConstants.DateTimeFormat),
                    ProductLevelCavity1.GetResultText());
            }

            // Assign database inputs
            var faiCollectionCavity2 = CurrentProductType == ProductType.Mtm
                ? new FaiCollectionMtm()
                : new FaiCollectionAlps() as IFaiCollection;
            faiCollectionCavity2.SetFaiValues(FaiItemsCavity2, TimestampCavity2, 2,
                ProductLevelCavity2.GetResultText());

            var faiCollectionCavity1 = CurrentProductType == ProductType.Mtm
                ? new FaiCollectionMtm()
                : new FaiCollectionAlps() as IFaiCollection;
            faiCollectionCavity1.SetFaiValues(FaiItemsCavity1, TimestampCavity1, 1,
                ProductLevelCavity1.GetResultText());
            // Insert into local database
            FaiCollectionHelper.Insert(NameConstants.SqlConnectionString, faiCollectionCavity2, faiCollectionCavity1);


            // Update tables
            UiDispatcher.InvokeAsync(() =>
                UpdateTables(TimestampCavity1.ToString(NameConstants.DateTimeFormat),
                    TimestampCavity2.ToString(NameConstants.DateTimeFormat)));
        }

        public DateTime TimestampCavity1 { get; set; }

        public DateTime TimestampCavity2 { get; set; }

        private void UpdateTables(string timestampCavity1, string timestampCavity2)
        {
            // Init header if necessary
            if (Table == null)
            {
                Table = new FaiTableStackViewModel()
                {
                    Header = FaiItemsCavity1.Select(item => item.Name).ToList(),
                    Max = FaiItemsCavity1.Select(item => item.MaxBoundary).ToList(),
                    Min = FaiItemsCavity1.Select(item => item.MinBoundary).ToList(),
                    MaxRows = 50,
                    PortionToRemoveWhenOverflows = 0.3
                };
            }

            // Add rows
            var shouldAddRowCavity2 = ProductLevelCavity2 != ProductLevel.Empty;
            if (shouldAddRowCavity2) Table.AddRow(FaiItemsCavity2, ProductLevelCavity2, timestampCavity2);

            var shouldAddRowCavity1 = ProductLevelCavity1 != ProductLevel.Empty;
            if (shouldAddRowCavity1) Table.AddRow(FaiItemsCavity1, ProductLevelCavity1, timestampCavity1);
        }


        private void LoadShapeModels()
        {
            HOperatorSet.ReadShapeModel(DirectoryConstants.ShapeModelPath2D, out _shapeModel2D);
            HOperatorSet.ReadShapeModel(DirectoryConstants.ShapeModelPath3D, out _shapeModel3D);
        }


        /// <summary>
        /// Decide whether the item is missing, passed or rejected
        /// </summary>
        /// <param name="itemExists"></param>
        /// <param name="errored"></param>
        /// <param name="faiItems"></param>
        /// <returns></returns>
        private ProductLevel GetProductLevel(bool itemExists, bool errored, IEnumerable<FaiItem> faiItems)
        {
            if (!itemExists) return ProductLevel.Empty;
            if (errored) return ProductLevel.Ng5;

            var value_16_1 = faiItems.First(item => item.Name == "FAI16_1").Value;
            var wrongProduct = value_16_1 > 0.15 && value_16_1 < 100;
            if (wrongProduct) return ProductLevel.Ng3;

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
            Logger.LogRoutineMessageAsync($"收到2D图像{images.Count}张");
            if (!Server.IsAutoRunning) return;

            var currentArrivedSocket2D = CurrentArrivedSocket2D;

            GraphicsPackViewModel result;

            lock (_lockerOf2DProc)
            {
                try
                {
                    result = I40Check.Execute(currentArrivedSocket2D.ToChusIndex(), images);
                    Logger.LogRoutineMessageAsync($"2D处理完成-{currentArrivedSocket2D}");
                }
                catch (Exception e)
                {
                    result = new GraphicsPackViewModel
                    {
                        Images = images, FaiResults = I40Check.GetFaiDict(currentArrivedSocket2D.ToChusIndex()),
                        ErrorMessage = e.Message
                    };
                    Logger.LogRoutineMessageAsync($"2D处理错误-{currentArrivedSocket2D}");
                    // If error is unexpected
                    if (!e.Message.Contains("[2D Vision]"))
                    {
                        var logDir = Path.Combine(DirectoryConstants.ImageDir2D,
                            "Error/" + DateTime.Now.ToString(NameConstants.DateTimeFormat));
                        Task.Run(() => { Logger.Instance.RecordErrorImages(images, e.Message, logDir); });
                    }
                }
            }


            if (currentArrivedSocket2D == CavityType.Cavity2)
            {
                CurrentArrivedSocket2D = CavityType.Cavity1;
            }
            

            bool all2DProcessingForThisRunIsDone;
            lock (_lockerOfResultQueues2D)
            {
                _resultQueues2D[currentArrivedSocket2D].Enqueue(result);

                // Check if both cavities have their results returned
                var lastInCavity1 = _resultQueues2D[CavityType.Cavity1].LastOrDefault();
                var lastInCavity2 = _resultQueues2D[CavityType.Cavity2].LastOrDefault();
                all2DProcessingForThisRunIsDone = lastInCavity1 != null && lastInCavity2 != null;
            }

            if (!all2DProcessingForThisRunIsDone) return;
        
            ResultReady2D = ResultStatus.Ready;
            _readyToEnterNextRun = true;
            
            // If results of both cavities are ready
            Logger.LogRoutineMessageAsync("本轮2D处理完成");
        }


        /// <summary>
        /// Open connections to hardware as well as load on-site-only files
        /// This should be executed only after the construction of the instance,
        /// so the logging system can be used to debug
        /// </summary>
        public void InitHardWares()
        {
            SetupServer();

            try
            {
                SetupCameras();
                ShotsPerExecution2D = 2;
            }
            catch
            {
                var popup = new PopupViewModel
                {
                    OkCommand = new CloseDialogAttachedCommand(o => true, CloseMainWindow),
                    IsSpecialPopup = false,
                    Content = "相机连接失败，请解除相机占用稍候并重启ALC",
                    OkButtonText = "确定"
                };
                Logger.EnqueuePopup(popup);
            }

            SetupLaserControllers();
        }


        public void LoadConfigs()
        {
            // Load product-type-specific configs by switching product type
            CurrentProductType = ProductType.Alps;

            LoadPasswordModule();

            LoadShapeModels();

            ApplicationConfigs =
                AutoSerializableHelper.LoadAutoSerializable<ApplicationConfigViewModel>(DirectoryConstants.ConfigDirectory,
                    "ApplicationConfigs");            
            
            ApplicationConfigs.ShouldAutoSerialize = true;
        }

        private void LoadPasswordModule()
        {
            LoginViewModel =
                AutoSerializableHelper.LoadAutoSerializable<LoginViewModel>(DirectoryConstants.ConfigDirectory, "PD");
            LoginViewModel.ShouldAutoSerialize = true;
            LoginViewModel.MessageQueue = Logger.Instance.StateChangedMessageQueue;
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
            _instance = new ApplicationViewModel()
            {
                ShouldSave2DImagesLeft = false,
                ShouldSave2DImagesRight = false,
                ShouldSave3DImagesLeft = false,
                ShouldSave3DImagesRight = false,
                SaveNgImagesOnly = false,

            };
        }

        /// <summary>
        /// Disconnect cameras, lasers and plc
        /// </summary>
        public void Cleanup()
        {
            Server?.Disconnect();

            try
            {
                TopCamera.StopGrabbing();
                TopCamera.Close();
            }
            catch
            {
                // I don't care
            }

            foreach (var controller in ControllerManager.AttachedControllers)
            {
                controller.IsConnectedHighSpeed = false;
            }
            
            Logger.CleanUp();

            // Remove outdated files if any
            SerializationHelper.RemoveOutdatedFiles(DirectoryConstants.ErrorLogDir, 1);
            SerializationHelper.RemoveOutdatedFiles(DirectoryConstants.ImageDir2D, 5);
            SerializationHelper.RemoveOutdatedFiles(DirectoryConstants.ImageDir3D, 5);
            SerializationHelper.RemoveOutdatedFiles(DirectoryConstants.CsvOutputDir, 30);
        }


        #region prop

        public bool OutputRawData3D { get; set; } = false;

        public TimeLineManagerViewModel TimeLineManager { get; set; }

        public GraphicsPackViewModel Graphics2DCavity1
        {
            get { return _graphics2DCavity1; }
            set { _graphics2DCavity1 = value; }
        }

        public GraphicsPackViewModel Graphics2DCavity2
        {
            get { return _graphics2DCavity2; }
            set { _graphics2DCavity2 = value; }
        }

        public GraphicsPackViewModel Graphics3DCavity1 { get; set; } = new GraphicsPackViewModel();
        public GraphicsPackViewModel Graphics3DCavity2 { get; set; } = new GraphicsPackViewModel();

        public bool SaveNgImagesOnly { get; set; }

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


        public CavityType CurrentArrivedSocket2D { get; set; }
        public ProductLevel ProductLevelCavity1 { get; set; }
        public ProductLevel ProductLevelCavity2 { get; set; }


        /// <summary>
        /// Current application page
        /// </summary>
        public ApplicationPageType CurrentApplicationPage { get; set; }


        /// <summary>
        /// 2D camera object
        /// </summary>
        public HKCameraViewModel TopCamera { get; set; }

        public AlcServerViewModel Server { get; set; }

        public List<FaiItem> FaiItemsCavity1 { get; set; }
        public List<FaiItem> FaiItemsCavity2 { get; set; }

        public List<FaiItem> FaiItems2DLeft { get; set; }
        public List<FaiItem> FaiItems2DRight { get; set; }
        public List<FaiItem> FaiItems3DLeft { get; set; }
        public List<FaiItem> FaiItems3DRight { get; set; }


        public I40Check I40Check { get; set; }

        
        public ICommand OpenTimelineDialogCommand { get; set; }
        public ICommand InsertNewTimelineCommand { get; set; }


        public ICommand CloseMainWindowCommand { get; set; }

        public bool ShouldSave2DImagesLeft { get; set; }
        public bool ShouldSave2DImagesRight { get; set; }
        public bool ShouldSave3DImagesLeft { get; set; }
        public bool ShouldSave3DImagesRight { get; set; }


        public FaiTableStackViewModel Table { get; set; }

        public static Dispatcher UiDispatcher
        {
            get { return System.Windows.Application.Current.Dispatcher; }
        }

        public SummaryViewModel Summary { get; set; } 

        public ResultStatus ResultReady2D { get; set; }

        public ICommand SimulateCommand { get; set; }
        
        public ICommand SwitchMtmCommand { get; private set; }
        public ICommand SwitchAlpsCommand { get; private set; }

        public ProductType CurrentProductType
        {
            get { return _currentProductType; }
            set
            {
                _currentProductType = value;
                SwitchProductType(_currentProductType);
            }
        }

        public ICommand ClearProductErrorStatesCommand { get; private set; }

        public LoginViewModel LoginViewModel { get; set; }
        public ApplicationConfigViewModel ApplicationConfigs { get; set; }

        /// <summary>
        /// Count of 2D input images per execution
        /// </summary>
        public int ShotsPerExecution2D
        {
            get { return _shotsPerExecution2D; }
            set
            {
                if (value <= 0) return;
                _shotsPerExecution2D = value;
                TopCamera.ImageBatchSize = value;
            }
        }

        public ICommand Backup3DConfigsCommand { get; private set; }

        public ICommand Recover3DConfigsCommand { get; private set; }


        private void SwitchProductType(ProductType currentProductType)
        {
            SwitchProductType2D(currentProductType);
            SwitchProductType3D(currentProductType);

            FaiItemsCavity1 = FaiItems2DLeft.ConcatNew(FaiItems3DLeft);
            FaiItemsCavity2 = FaiItems2DRight.ConcatNew(FaiItems3DRight);

            // Init yield collection
            Summary.ClearSummary();
            Summary.FaiYieldCollectionViewModel =
                new FaiYieldCollectionViewModel(FaiItemsCavity1.Select(item => item.Name));

            InitSerializer();
            InitTables();
        }

        private void InitTables()
        {
            Table = null;
        }


        private void InitSerializer()
        {
            _serializerAll = new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "All"));

            _serializerLeft =
                new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity1"));

            _serializerRight =
                new CsvSerializer.CsvSerializer(Path.Combine(DirectoryConstants.CsvOutputDir, "Cavity2"));
        }

        private void SwitchProductType3D(ProductType currentProductType)
        {
            if (currentProductType == ProductType.Mtm)
            {
                _procedure3D = new Procedure_MTM3D();
            }
            else
            {
                _procedure3D = new Procedure_ALPS3D();
            }

            var faiNames = _procedure3D.FaiNames;

            // Load fai items configs
            FaiItems3DLeft = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(faiNames,
                    DirectoryConstants.FaiConfigDirs3DCavity1[currentProductType])
                .ToList();
            FaiItems3DRight = AutoSerializableHelper.LoadAutoSerializables<FaiItem>(faiNames,
                    DirectoryConstants.FaiConfigDirs3DCavity2[currentProductType])
                .ToList();
        }

        private void SwitchProductType2D(ProductType currentProductType)
        {
            var i40ConfigDir2d = DirectoryConstants.ConfigDirs2D[currentProductType];
            I40Check = new I40Check(i40ConfigDir2d, "I40");

            FaiItems2DLeft = I40Check.GetFaiBoundaries();
            FaiItems2DRight = I40Check.GetFaiBoundaries();
        }


        public StringMatrixData LoadData2D(StringMatrixType dataType)
        {
            return I40Check.GetData(dataType);
        }

        public void SaveData2D(List<List<string>> data, StringMatrixType dataType)
        {
            switch (dataType)
            {
                case StringMatrixType.Results:
                    I40Check.ResultDictionary1 = data.ToDict();
                    I40Check.SaveResultLimitParam();
                    // Update 2d boundaries
                    FaiItems2DLeft = I40Check.GetFaiBoundaries();
                    FaiItems2DRight = I40Check.GetFaiBoundaries();
                    FaiItemsCavity1 = FaiItems2DLeft.ConcatNew(FaiItems3DLeft);
                    FaiItemsCavity2 = FaiItems2DRight.ConcatNew(FaiItems3DRight);

                    Logger.LogStateChanged("保存结果数据成功");
                    break;

                case StringMatrixType.Misc:
                    I40Check.AlgDictionary = data.ToDict();
                    I40Check.SaveAlgParam();
                    Logger.LogStateChanged("保存其他数据成功");
                    break;

                case StringMatrixType.FindLine:
                    I40Check.SearchLineDictionary = data.ToDict();
                    I40Check.SaveSearchLineParam();
                    Logger.LogStateChanged("保存找边数据成功");
                    break;
                default:
                    throw new KeyNotFoundException($"Can not find such StringMatrixType {dataType}");
            }
        }

        #endregion
    }
}