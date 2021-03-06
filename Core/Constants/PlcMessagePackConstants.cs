﻿using System.Collections.Generic;
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
        
        public static readonly PlcMessagePack PromptPlcToCheckDoorStateMessagePack = new PlcMessagePack()
        {
            CommandId = 33,
            MsgType = PlcMessagePack.RequestIndicator
        };


        public static readonly PlcMessagePack ClearProductErrorStateMessagePack = new PlcMessagePack()
        {
            CommandId = CommandIdClearProductErrorState,
            MsgType = PlcMessagePack.RequestIndicator
        };

        #region 2080

        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2080Continue = new PlcMessagePack()
        {
            CommandId = 27,
            Param1 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2080Quit = new PlcMessagePack()
        {
            CommandId = 27,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2081Continue = new PlcMessagePack()
        {
            CommandId = 27,
            Param1 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2081Quit = new PlcMessagePack()
        {
            CommandId = 27,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2082Continue = new PlcMessagePack()
        {
            CommandId = 29,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2082Quit = new PlcMessagePack()
        {
            CommandId = 29,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2083Continue = new PlcMessagePack()
        {
            CommandId = 29,
            Param1 = 0,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2083Quit = new PlcMessagePack()
        {
            CommandId = 29,
            Param1 = 1,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2084Continue = new PlcMessagePack()
        {
            CommandId = 28,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2084Quit = new PlcMessagePack()
        {
            CommandId = 28,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2085Continue = new PlcMessagePack()
        {
            CommandId = 28,
            Param1 = 0,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2085Quit = new PlcMessagePack()
        {
            CommandId = 28,
            Param1 = 1,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2086Continue = new PlcMessagePack()
        {
            CommandId = 30,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2086Quit = new PlcMessagePack()
        {
            CommandId = 30,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2087Continue = new PlcMessagePack()
        {
            CommandId = 30,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2087Quit = new PlcMessagePack()
        {
            CommandId = 30,
            Param1 = 1,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2088Continue = new PlcMessagePack()
        {
            CommandId = 24,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2088Quit = new PlcMessagePack()
        {
            CommandId = 24,
            Param1 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };

        #endregion

        #region 2090

        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2090Continue = new PlcMessagePack()
        {
            CommandId = 34,
            Param2 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
    
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2091Continue = new PlcMessagePack()
        {
            CommandId = 34,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
      
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2092Continue = new PlcMessagePack()
        {
            CommandId = 36,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
     
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2093Continue = new PlcMessagePack()
        {
            CommandId = 36,
            Param1 = 0,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
      
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2094Continue = new PlcMessagePack()
        {
            CommandId = 35,
            Param1 = 0,
            Param2 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2095Continue = new PlcMessagePack()
        {
            CommandId = 35,
            Param1 = 0,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
      
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2096Continue = new PlcMessagePack()
        {
            CommandId = 37,
            Param2 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
     
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2097Continue = new PlcMessagePack()
        {
            CommandId = 37,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2098Continue = new PlcMessagePack()
        {
            CommandId = 38,
            Param2 = 0,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        private static readonly PlcMessagePack MessagePack2099Continue = new PlcMessagePack()
        {
            CommandId = 38,
            Param2 = 1,
            MsgType = PlcMessagePack.RequestIndicator
        };
        
        

        #endregion
        
        
        
        
        /// <summary>
        /// See communication table for more details
        /// </summary>
        public static Dictionary<long, List<PlcMessagePack>> PlcWarningHandler8090Series { get; } = new Dictionary<long, List<PlcMessagePack>>()
        {
            [2080] = new List<PlcMessagePack>(){MessagePack2080Continue, MessagePack2080Quit},
            [2081] = new List<PlcMessagePack>(){MessagePack2081Continue, MessagePack2081Quit},
            [2082] = new List<PlcMessagePack>(){MessagePack2082Continue, MessagePack2082Quit},
            [2083] = new List<PlcMessagePack>(){MessagePack2083Continue, MessagePack2083Quit},
            [2084] = new List<PlcMessagePack>(){MessagePack2084Continue, MessagePack2084Quit},
            [2085] = new List<PlcMessagePack>(){MessagePack2085Continue, MessagePack2085Quit},
            [2086] = new List<PlcMessagePack>(){MessagePack2086Continue, MessagePack2086Quit},
            [2087] = new List<PlcMessagePack>(){MessagePack2087Continue, MessagePack2087Quit},
            [2088] = new List<PlcMessagePack>(){MessagePack2088Continue, MessagePack2088Quit},
            
            [2090] = new List<PlcMessagePack>(){MessagePack2090Continue, PlcMessagePack.AbortMessage},
            [2091] = new List<PlcMessagePack>(){MessagePack2091Continue, PlcMessagePack.AbortMessage},
            [2092] = new List<PlcMessagePack>(){MessagePack2092Continue, PlcMessagePack.AbortMessage},
            [2093] = new List<PlcMessagePack>(){MessagePack2093Continue, PlcMessagePack.AbortMessage},
            [2094] = new List<PlcMessagePack>(){MessagePack2094Continue, PlcMessagePack.AbortMessage},
            [2095] = new List<PlcMessagePack>(){MessagePack2095Continue, PlcMessagePack.AbortMessage},
            [2096] = new List<PlcMessagePack>(){MessagePack2096Continue, PlcMessagePack.AbortMessage},
            [2097] = new List<PlcMessagePack>(){MessagePack2097Continue, PlcMessagePack.AbortMessage},
            [2098] = new List<PlcMessagePack>(){MessagePack2098Continue, PlcMessagePack.AbortMessage},
            [2099] = new List<PlcMessagePack>(){MessagePack2099Continue, PlcMessagePack.AbortMessage},
        };
    }
}