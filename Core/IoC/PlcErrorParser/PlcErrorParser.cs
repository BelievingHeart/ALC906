using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PLCCommunication.Core.Interfaces;

namespace Core.IoC.PlcErrorParser
{
    public class PlcErrorParser : IPlcErrorParser
    {
        public event Action<string,long> WarningL1Emit;
        public event Action<string,long> WarningL2Emit;
        public event Action<string,long> WarningL3Emit;
        public event Action<string,long> WarningL4Emit;

        /// <summary>
        /// Key = ErrorCode, Value = Message + Level
        /// </summary>
        private Dictionary<long, ErrorMessageItem> _errorLookUpTable = new Dictionary<long, ErrorMessageItem>();
        public void ParseErrorCode(long errorCode)
        {
       

            var errorMessageItem = _errorLookUpTable[errorCode];
            switch (errorMessageItem.Level)
            {
                case 1:
                    OnWarningL1Emit(errorMessageItem.Message, errorCode);
                    break;
                case 2:
                    OnWarningL2Emit(errorMessageItem.Message, errorCode);
                    break;
                case 3:
                    OnWarningL3Emit(errorMessageItem.Message, errorCode);
                    break;
                case 4:
                    OnWarningL4Emit(errorMessageItem.Message, errorCode);
                    break;
            }
        }

        public PlcErrorParser(string errorSheetPath)
        {
            var errorLines = File.ReadAllLines(errorSheetPath, Encoding.GetEncoding("gb2312"));
            var errorLinesSplit = errorLines.Select(line => line.Split(','));

            foreach (var errorCodeAndDescription in errorLinesSplit)
            {
                var errorCode = long.Parse(errorCodeAndDescription[1]);
                var errorLevel = int.Parse(errorCodeAndDescription[2]);
                var errorDescription = errorCodeAndDescription[0];
                _errorLookUpTable[errorCode] = new ErrorMessageItem {Level = errorLevel, Message = errorDescription};
            }

        }

        protected virtual void OnWarningL1Emit(string obj, long errorCode)
        {
            WarningL1Emit?.Invoke(obj, errorCode);
        }

        protected virtual void OnWarningL2Emit(string obj, long errorCode)
        {
            WarningL2Emit?.Invoke(obj, errorCode);
        }

        protected virtual void OnWarningL3Emit(string obj, long errorCode)
        {
            WarningL3Emit?.Invoke(obj, errorCode);
        }

        protected virtual void OnWarningL4Emit(string obj, long errorCode)
        {
            WarningL4Emit?.Invoke(obj, errorCode);
        }
        
        private static List<long> _specialCode = new List<long>(){2080, 2081, 2082, 2083, 2084, 2085, 2086, 2087, 2088};

        public static bool IsSpecialErrorCode(long errorCode)
        {
            return _specialCode.Contains(errorCode);
        }
    }
}