using System;
using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.LotRecord
{
    public class LotRecordItemViewModel : ViewModelBase
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public ProductType ProductType { get; set; }
        public string Description { get; set; }
    }
}