using System;
using System.IO;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Message
{
    public class LoggingMessageItem : ViewModelBase
    {
        public string Message { get; set; }

        public string Time { get; set; }
        
        public void WriteLineToFile(string dir, string name)
        {
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, name);
            using (var fs = new StreamWriter(path, true))
            {
                var line = $"{Time}> {Message}";
                fs.WriteLine(line);
            }
        }

        public static LoggingMessageItem CreateMessage(string message)
        {
            return new LoggingMessageItem()
            {
                Time = DateTime.Now.ToString("T"),
                Message = message
            };
        }
    }
}