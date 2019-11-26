using Core.Enums;
using PLCCommunication.Core;
using PLCCommunication.Core.ViewModels;

namespace Core.Helpers
{
    public static class PlcMessagePackHelper
    {
        public static void SendProductLevels(this AlcServerViewModel server, ProductLevel leftSocketLevel,
            ProductLevel rightSocketLevel)
        {
            server.SetMessagePack(new PlcMessagePack()
            {
                ChannelId = 0,
                MsgType = 0,
                CommandId = 16,
                Param1 = leftSocketLevel.GetProductLevelValue(),
                Param2 = rightSocketLevel.GetProductLevelValue(),
            });
        }

        private static float GetProductLevelValue(this ProductLevel productLevel) => (float) productLevel;

    }
}