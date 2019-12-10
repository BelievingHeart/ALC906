﻿using System;
using System.IO;
using Core.Constants;
using Core.ViewModels.Message;
using MaterialDesignThemes.Wpf;
using WPFCommon.ViewModels.Base;

namespace Core.IoC.Loggers
{
    public class Logger : ViewModelBase
    {
        private string _logDir;
        private string _previousDate;
        private readonly string _logFileName = "PlcErrors.txt";
        private string LogFilePath => Path.Combine(_logDir, _logFileName);
        private string CurrentDate => DateTime.Today.ToShortDateString();

        
        private static Logger _instance = new Logger(DirectoryConstants.ErrorLogDir)
        {
            PlcMessageList = new FixedSizeMessageList(DirectoryConstants.ErrorLogDir, "PLC.txt"),
            RoutineMessageList = new FixedSizeMessageList(DirectoryConstants.ErrorLogDir, "Routine.txt")
        };
        public static Logger Instance => _instance;

        public FixedSizeMessageList PlcMessageList { get; set; }
    
        public FixedSizeMessageList RoutineMessageList { get; set; }
        
        public void LogErrorToFile(string message)
        {
            try
            {
                // Create new file if not exists or date changes
                if (!File.Exists(LogFilePath) || CurrentDate != _previousDate) File.Create(LogFilePath);

                var line = $"{DateTime.Now:h:mm:ss tt zz}> {message}";
                File.AppendAllLines(LogFilePath, new []{line});
            }
            catch (Exception e)
            {
              // TODO: add later execution logic
            }
        }
        
        public void LogStateChanged(string message)
        {
            StateChangedMessageQueue.Enqueue(message);
        }

        public Logger(string logDir)
        {
            // Error logging
            _logDir = logDir;
            Directory.CreateDirectory(_logDir);
            _previousDate = CurrentDate;
            
            // State changed logging
            StateChangedMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        }

        /// <summary>
        /// Used to prompt user when the machine state is changed
        /// </summary>
        public ISnackbarMessageQueue StateChangedMessageQueue { get; set; }

        public static void LogPlcMessage(string message)
        {
            Instance?.PlcMessageList?.LogAsync(message);
        }

        public static void LogRoutineMessage(string message)
        {
            Instance?.RoutineMessageList?.LogAsync(message);
        }
    }
}