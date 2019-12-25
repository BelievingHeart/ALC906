using System.Collections.Generic;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database.FaiCollection
{
    public class FaiCollectionItemViewModel
    {
        public IFaiCollection FaiCollection { get; set; }

        public Dictionary<string, double> DictionaryUpper { get; set; }
        public Dictionary<string, double> DictionaryLower { get; set; }
    }
}