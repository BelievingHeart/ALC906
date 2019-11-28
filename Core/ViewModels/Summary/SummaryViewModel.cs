using System.Xml.Serialization;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Summary
{
    public class SummaryViewModel : AutoSerializableBase<SummaryViewModel>
    {
        [XmlAttribute] public int OkCount { get; set; }
        [XmlAttribute] public int Ng2Count { get; set; }
        [XmlAttribute] public int Ng3Count { get; set; }
        [XmlAttribute] public int Ng4Count { get; set; }
        [XmlAttribute] public int Ng5Count { get; set; }
        [XmlAttribute] public int TotalCount { get; set; }
        [XmlAttribute] public int EmptyCount { get; set; }
        [XmlAttribute] public int NgCount { get; set; }
    }
}