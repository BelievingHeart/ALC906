using System.Net.Sockets;
using System.Windows.Input;
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
                OkButtonText = "确定",
                CancelButtonText = "取消",
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
            
            var content = errorCode == 2088? "请清除所有产品后点继续，或者点退出取消自动模式" : "请清料后点击继续，或者点退出再清料";
            
            return  new PopupViewModel()
            {
                OkButtonText = "继续",
                CancelButtonText = "退出",
                OkCommand = new CloseDialogAttachedCommand(o => true, () =>
                    {
                        alcServer.SentToPlc(continueMessagePack, PlcMessageType.Request);
                    }),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () =>
                {
                    alcServer.SentToPlc(quitMessagePack, PlcMessageType.Request);
                    alcServer.IsAutoRunning = false;
                }),
                MessageItem = LoggingMessageItem.CreateMessage(message),
                Content = content,
                IsSpecialPopup = true
            };
        }

        public static PopupViewModel CreateSafeDoorPopup(string message, AlcServerViewModel alcServer)
        {
     

            var continueMessagePack = PlcMessagePackConstants.PromptPlcToCheckDoorStateMessagePack;
            var quitMessagePack = PlcMessagePack.AbortMessage;
            
            var content = "请关门后点击继续，或者点取消退出自动模式";
            
            return  new PopupViewModel()
            {
                OkButtonText = "继续",
                CancelButtonText = "取消",
                OkCommand = new CloseDialogAttachedCommand(o => true, () =>
                    {
                        alcServer.SentToPlc(continueMessagePack, PlcMessageType.Request);
                    }),
                CancelCommand = new CloseDialogAttachedCommand(o => true, () =>
                {
                    alcServer.SentToPlc(quitMessagePack, PlcMessageType.Request);
                    alcServer.IsAutoRunning = false;
                }),
                MessageItem = LoggingMessageItem.CreateMessage(message),
                Content = content,
                IsSpecialPopup = true
            };        }
    }
}