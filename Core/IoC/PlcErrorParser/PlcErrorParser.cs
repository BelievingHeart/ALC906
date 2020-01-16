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
        
        private static readonly List<long> ClearProductCode = new List<long>(){2080, 2081, 2082, 2083, 2084, 2085, 2086, 2087, 2088, 2090, 2091, 2092, 2093, 2094, 2095, 2096, 2097, 2098, 2099};
        private static readonly List<long> SafeDoorOpenCode = new List<long>(){2062, 2063, 2064, 2065};

        public static bool IsClearProductWarningCode(long errorCode)
        {
            return ClearProductCode.Contains(errorCode);
        }

        public static bool IsSafeDoorOpenWarningCode(long errorCode)
        {
            return SafeDoorOpenCode.Contains(errorCode);
        }
    }
}