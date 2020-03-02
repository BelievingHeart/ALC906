using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.CodeScan
{
    public class CodeListViewModel : ViewModelBase
    {
        #region props

        public IList<CodeItemViewModel> CodeItems { get; set; } = new List<CodeItemViewModel>();

        public bool AllCodesReady => CodeItems.All(item => !string.IsNullOrEmpty(item.Code));
        #endregion

        #region ctor

        public CodeListViewModel()
        {
            var cavityTypes = new List<CavityType>();
            foreach (var cavityType in EnumHelper.GetValues<CavityType>())
            {
                cavityTypes.Add(cavityType);
            }

            cavityTypes.Reverse();
            foreach (var cavityType in cavityTypes)
            {
                CodeItems.Add(new CodeItemViewModel()
                {
                    Code = string.Empty,
                    Cavity = cavityType
                });
            }
        }

        #endregion

        #region api

        public void ClearCodes()
        {
            foreach (var state in CodeItems)
            {
                state.Code = string.Empty;
            }
        }

        public void TransferCodes(IDictionary<CavityType, string> codeDict)
        {
            foreach (var codeItem in CodeItems)
            {
                codeDict[codeItem.Cavity] = codeItem.Code;
                codeItem.Code = string.Empty;
            }
        }

        public CodeItemViewModel this[CavityType index] => CodeItems.First(item => item.Cavity == index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns>True for can set, false for can not set</returns>
        public bool AddCode(string code)
        {
            var itemCodeNotSet = CodeItems.FirstOrDefault(item => string.IsNullOrEmpty(item.Code));
            if (itemCodeNotSet == null) return false;

            itemCodeNotSet.Code = code;
            return true;
        }

        #endregion
    }
}