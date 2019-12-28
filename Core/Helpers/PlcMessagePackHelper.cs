using Core.Enums;
using Core.IoC.Loggers;
using Core.ViewModels.Application;
using PLCCommunication.Core;
using PLCCommunication.Core.Enums;
using PLCCommunication.Core.ViewModels;

namespace Core.Helpers
{
    public static class PlcMessagePackHelper
    {
        public static void SendProductLevels(this AlcServerViewModel server, ProductLevel leftSocketLevel,
            ProductLevel rightSocketLevel)
        {
            server.SentToPlc(new PlcMessagePack
            {
                ChannelId = 0,
                MsgType = 0,
                CommandId = 16,
                Param1 = leftSocketLevel.GetProductLevelValue(),
                Param2 = rightSocketLevel.GetProductLevelValue()
            }, PlcMessageType.Request);
            
           Logger.LogPlcMessage($"Sent {leftSocketLevel} and {rightSocketLevel} to plc");

        }

        private static float GetProductLevelValue(this ProductLevel productLevel)
        {
            if (productLevel == ProductLevel.Empty) return 0;
            return (float) productLevel;
        }
    }
}