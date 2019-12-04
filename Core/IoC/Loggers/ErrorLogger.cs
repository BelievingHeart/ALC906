using System;
using System.IO;
using Core.ViewModels.Plc;

namespace Core.IoC.Loggers
{
    public class ErrorLogger
    {
        private string _logDir;
        private string _previousDate;
        private readonly string _logFileName = "PlcErrors.txt";
        private string LogFilePath => Path.Combine(_logDir, _logFileName);
        private string CurrentDate => DateTime.Today.ToShortDateString();

        
        private static ErrorLogger _instance = new ErrorLogger(Path.Combine(Directory.GetCurrentDirectory(), "Log"));
        public static ErrorLogger Instance => _instance;
        
        public void LogToFile(string message)
        {
            // Create new file if not exists or date changes
            if (!File.Exists(LogFilePath) || CurrentDate != _previousDate) File.Create(LogFilePath);

            var line = $"{DateTime.Now:h:mm:ss tt zz}> {message}";
            File.AppendAllLines(LogFilePath, new []{line});
        }

        public ErrorLogger(string logDir)
        {
            _logDir = logDir;
            Directory.CreateDirectory(_logDir);
            _previousDate = CurrentDate;
        }

    }
}