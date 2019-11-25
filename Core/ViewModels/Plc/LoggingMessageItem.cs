using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Plc
{
    public class LoggingMessageItem : ViewModelBase
    {
        public string Message { get; set; }

        public string Time { get; set; }
    }
}