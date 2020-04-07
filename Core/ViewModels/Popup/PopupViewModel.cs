using System.Windows.Input;
using Core.Commands;
using WPFCommon.Logger.Message;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Popup
{
    public class PopupViewModel : ViewModelBase
    {
        public CloseDialogAttachedCommand OkCommand { get; set; }
        public CloseDialogAttachedCommand CancelCommand { get; set; }
        public string OkButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public LoggingMessageItem MessageItem { get; set; }
        public string Content { get; set; }

        public bool IsSpecialPopup { get; set; }
    }
}