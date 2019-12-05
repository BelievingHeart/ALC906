using PLCCommunication.Core;

namespace Core.Constants
{
    public static class PlcMessagePackConstants
    {
        /// <summary>
        /// 2D拍照左槽到位
        /// </summary>
        public const int CommandIdLeftSocketArrived = 22;

        /// <summary>
        /// 2D拍照右槽到位
        /// </summary>
        public const int CommandIdRightSocketArrived = 21;

        /// <summary>
        /// 产品状态异常，比如plc逻辑认为夹具有料， 但料已经被取走
        /// 由PLC发起，收到后弹窗
        /// 处理完毕后处理弹窗，并回复PLC
        /// </summary>
        public const int CommandIdProductStateError = 24;
        
        /// <summary>
        /// 清除产品异常状态
        /// 只允许在！IsAutoRunning状态下执行 
        /// </summary>
        private const int CommandIdClearProductErrorState = 25;
        
        
        public static readonly PlcMessagePack ClearProductErrorStateMessagePack = new PlcMessagePack()
        {
            CommandId = CommandIdClearProductErrorState,
            MsgType = PlcMessagePack.RequestIndicator
        };
    }
}