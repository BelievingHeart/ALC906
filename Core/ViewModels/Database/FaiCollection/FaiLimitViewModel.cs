using System.Xml.Serialization;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database.FaiCollection
{
    public class FaiLimitViewModel : AutoSerializableBase<FaiLimitViewModel>
    {
        [XmlAttribute]
        public double Upper { get; set; }
        [XmlAttribute]
        public double Lower { get; set; }
    }
}