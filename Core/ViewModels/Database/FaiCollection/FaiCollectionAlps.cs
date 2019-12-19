using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database.FaiCollection
{
    public class FaiCollectionAlps : ViewModelBase, IFaiCollection
    {
        public string InspectionTime { get; set; }
        public int Cavity { get; set; }
        public string Result { get; set; }
    }
}    