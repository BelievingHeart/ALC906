using Core.Enums;
using Core.ViewModels.Application;
using PLCCommunication.Core;
using PLCCommunication.Core.ViewModels;

namespace Core.Helpers
{
    public static class PlcMessagePackHelper
    {
        public static void SendProductLevels(this AlcServerViewModel server, ProductLevel leftSocketLevel,
            ProductLevel rightSocketLevel)
        {
            server.SendMessagePack(new PlcMessagePack()
            {
                ChannelId = 0,
                MsgType = 0,
                CommandId = 16,
                Param1 = leftSocketLevel.GetProductLevelValue(),
                Param2 = rightSocketLevel.GetProductLevelValue(),
            });
            
           ApplicationViewModel.Instance.LogPlcMessage($"Sent {leftSocketLevel} and {rightSocketLevel} to plc");

        }

        private static float GetProductLevelValue(this ProductLevel productLevel) => (float) productLevel;

    }
}