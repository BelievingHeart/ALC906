using System.Net.Sockets;
using Core.Commands;
using Core.Constants;
using Core.ViewModels.Message;
using Core.ViewModels.Popup;
using PLCCommunication.Core;
using PLCCommunication.Core.Enums;
using PLCCommunication.Core.ViewModels;

namespace Core.Helpers
{
    public static class PopupHelper
    {
        public static PopupViewModel CreateNormalPopup(string message)
        {
          return  new PopupViewModel()
            {
                OkButtonText = "OK",
                CancelButtonText = "Cancel",
                OkCommand = new CloseDialogAttachedCommand(o => true, () => {}),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () => {}),
                MessageItem = LoggingMessageItem.CreateMessage(message),
                IsSpecialPopup = false
            };
        }

        public static PopupViewModel CreateClearProductPopup(string message, long errorCode, AlcServerViewModel alcServer)
        {
            var continueMessagePack = PlcMessagePackConstants.PlcWarningHandler2080Series[errorCode][0];
            var quitMessagePack = PlcMessagePackConstants.PlcWarningHandler2080Series[errorCode][1];
            
            var content = errorCode == 2088? "请清除所有产品后点Continue，或者点Quit退出自动" : "请清料后点击Continue，或者点Quit再清料";
            
            return  new PopupViewModel()
            {
                OkButtonText = "Continue",
                CancelButtonText = "Quit",
                OkCommand = new CloseDialogAttachedCommand(o => true, execution: () => alcServer.SentToPlc(continueMessagePack,PlcMessageType.Request)),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () => {alcServer.SentToPlc(quitMessagePack, PlcMessageType.Request);}),
                MessageItem = LoggingMessageItem.CreateMessage(message),
                Content = content,
                IsSpecialPopup = true
            };
        }
    }
}