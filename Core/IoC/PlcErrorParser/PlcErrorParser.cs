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
        public event Action<string> WarningL1Emit;
        public event Action<string> WarningL2Emit;
        public event Action<string> WarningL3Emit;
        public event Action<string> WarningL4Emit;

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
                    OnWarningL1Emit(errorMessageItem.Message);
                    break;
                case 2:
                    OnWarningL2Emit(errorMessageItem.Message);
                    break;
                case 3:
                    OnWarningL3Emit(errorMessageItem.Message);
                    break;
                case 4:
                    OnWarningL4Emit(errorMessageItem.Message);
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

        protected virtual void OnWarningL1Emit(string obj)
        {
            WarningL1Emit?.Invoke(obj);
        }

        protected virtual void OnWarningL2Emit(string obj)
        {
            WarningL2Emit?.Invoke(obj);
        }

        protected virtual void OnWarningL3Emit(string obj)
        {
            WarningL3Emit?.Invoke(obj);
        }

        protected virtual void OnWarningL4Emit(string obj)
        {
            WarningL4Emit?.Invoke(obj);
        }
    }
}