using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Core.Constants;
using Core.Helpers;
using Core.ViewModels.Message;
using Core.ViewModels.Popup;
using HalconDotNet;
using MaterialDesignThemes.Wpf;
using WPFCommon.ViewModels.Base;

namespace Core.IoC.Loggers
{
    public class Logger : ViewModelBase
    {
        #region private members

        private string _previousDate;
        private PopupQueue _popupQueue;
        private int _errorCount;
        private PopupViewModel _popupViewModel;
        private bool _shouldMessageBoxPopup;

        private static Logger _instance = new Logger
        {
            PlcMessageList = new FixedSizeMessageList(DirectoryConstants.ErrorLogDir, "PLC.txt"),
            UnhandledPlcMessageList = new FixedSizeMessageList(DirectoryConstants.ErrorLogDir, "PLC-Unhandled.txt"),
            RoutineMessageList = new FixedSizeMessageList(DirectoryConstants.ErrorLogDir, "Routine.txt"),
        };

        #endregion

        public event Action<bool> ShouldMessageBoxPopupChanged; 

        #region prop

        public static bool HasPopupsUnhandled => Instance._popupQueue.PopupCount > 0;
        
        public static string HighLevelWarningFilePath => Path.Combine(DirectoryConstants.ErrorLogDir, "PlcErrors.txt");
        public static string AllCommandIdsFromPlcFilePath => Path.Combine(DirectoryConstants.ErrorLogDir, "AllCommandIdsFromPlc.txt");
        public static string AllCommandIdsToPlcFilePath => Path.Combine(DirectoryConstants.ErrorLogDir, "AllCommandIdsToPlc.txt");

             
        /// <summary>
        /// Used to prompt user when the machine state is changed
        /// </summary>
        public ISnackbarMessageQueue StateChangedMessageQueue { get; set; }


        public bool ShouldMessageBoxPopup
        {
            get { return _shouldMessageBoxPopup; }
            set
            {
                _shouldMessageBoxPopup = value;
                OnShouldMessageBoxPopupChanged(_shouldMessageBoxPopup);
            }
        }

        public PopupViewModel PopupViewModel
        {
            get { return _popupViewModel; }
            set
            {
                _popupViewModel = value;
                if(_popupViewModel!=null)ShouldMessageBoxPopup = true;
            }
        }

        private string CurrentDate
        {
            get { return DateTime.Today.ToShortDateString(); }
        }



        
     

        public static Logger Instance
        {
            get { return _instance; }
        }

        public FixedSizeMessageList PlcMessageList { get; set; }
        public FixedSizeMessageList UnhandledPlcMessageList { get; set; }
    
        public FixedSizeMessageList RoutineMessageList { get; set; }
        
        #endregion

        #region ctor

        public Logger()
        {
            _previousDate = CurrentDate;
            // State changed logging
            StateChangedMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        }
        

        #endregion



        #region api

        public static void LogHighLevelWarningSpecial(PopupViewModel popupViewModel)
        {
            EnqueuePopup(popupViewModel);
        }

        public static void EnqueuePopup(PopupViewModel popupViewModel)
        {
            Instance._popupQueue.EnqueuePopupThreadSafe(popupViewModel);
        }

        public static void LogHighLevelWarningNormal(string s)
        {
            var popupViewModel = PopupHelper.CreateNormalPopup(s);
            Instance._popupQueue.EnqueuePopupThreadSafe(popupViewModel);
        }
        
        public static void LogPlcMessage(string message)
        {
            Instance?.PlcMessageList?.LogAsync(message);
        }
        public static void LogUnhandledPlcMessage(string message)
        {
            Instance?.UnhandledPlcMessageList?.LogAsync(message);
        }
        public static void LogRoutineMessageAsync(string message)
        {
            Instance?.RoutineMessageList?.LogAsync(message);
        }

        /// <summary>
        /// Start queuing up popup windows
        /// </summary>
        public void StartPopupQueue()
        {
            _popupQueue = new PopupQueue(o=>!ShouldMessageBoxPopup);
            _popupQueue.NewPopupDequeued += UpdatePopupViewModel;
        }

        public void RecordErrorImages(List<HImage> images, string message, string recordDir, string format="tiff")
        {
            Directory.CreateDirectory(recordDir);
            // Record images
            for (int i = 0; i < images.Count; i++)
            {
                var path = Path.Combine(recordDir, i.ToString("D2") + "." + format);
                images[i].WriteImage(format, 0, path);
            }
            // Record error
            var errorTextPath = Path.Combine(recordDir, "error.txt");
            File.WriteAllLines(errorTextPath, new []{message});
        }

        private void UpdatePopupViewModel(PopupViewModel obj)
        {
            PopupViewModel = obj;
        }
        
        public void LogMessageToFile(string message, string path)
        {
            try
            {
                // Create new file if not exists or date changes
                if (!File.Exists(path) || CurrentDate != _previousDate) File.Create(path);

                var line = $"{DateTime.Now:h:mm:ss tt zz}> {message}";
                File.AppendAllLines(path, new []{line});
            }
            catch (Exception e)
            {
                // Don't care
            }
        }
        
        public static void LogStateChanged(string message)
        {
            Instance.StateChangedMessageQueue.Enqueue(message);
        }
        

        #endregion

        protected virtual void OnShouldMessageBoxPopupChanged(bool obj)
        {
            ShouldMessageBoxPopupChanged?.Invoke(obj);
        }
    }
}