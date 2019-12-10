using Core.Enums;
using Core.IoC.Loggers;
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
            server.SendMessagePack(new PlcMessagePack
            {
                ChannelId = 0,
                MsgType = 0,
                CommandId = 16,
                Param1 = leftSocketLevel.GetProductLevelValue(),
                Param2 = rightSocketLevel.GetProductLevelValue()
            });
            
           Logger.LogPlcMessage($"Sent {leftSocketLevel} and {rightSocketLevel} to plc");

        }

        private static float GetProductLevelValue(this ProductLevel productLevel)
        {
            if (productLevel == ProductLevel.Empty) return 3;
            return (float) productLevel;
        }
    }
}