using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.CodeScan
{
    public class CodeItemViewModel : ViewModelBase
    {
        public CavityType Cavity { get;  set; }

        public string Code { get; set; }

        public bool Scanned => !string.IsNullOrEmpty(Code);

    }
}