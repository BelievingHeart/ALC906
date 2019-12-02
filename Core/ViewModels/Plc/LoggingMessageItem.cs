using System;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Plc
{
    public class LoggingMessageItem : ViewModelBase
    {
        public string Message { get; set; }

        public string Time { get; set; }

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