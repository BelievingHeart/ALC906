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
                MessageItem = LoggingMessageItem.CreateMessage(message)
            };
        }

        public static PopupViewModel CreateClearProductPopup(string message, long errorCode, AlcServerViewModel alcServer)
        {
            var continueMessagePack = PlcMessagePackConstants.PlcWarningHandler2080Series[errorCode][0];
            var quitMessagePack = PlcMessagePackConstants.PlcWarningHandler2080Series[errorCode][1];
            
            return  new PopupViewModel()
            {
                OkButtonText = "Continue",
                CancelButtonText = "Exit",
                OkCommand = new CloseDialogAttachedCommand(o => true, () => {alcServer.SendMessagePack(continueMessagePack);}),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () => {alcServer.SendMessagePack(quitMessagePack);}),
                MessageItem = LoggingMessageItem.CreateMessage(message)
            };
        }
    }
}