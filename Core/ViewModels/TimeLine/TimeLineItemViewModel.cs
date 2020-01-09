using System;
using System.Xml.Serialization;
using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.TimeLine
{
    public class TimeLineItemViewModel : AutoSerializableBase<TimeLineItemViewModel>
    {
        [XmlAttribute]
        public DateTime StartTime { get; set; }
        
        [XmlAttribute]
        public ProductType ProductType { get; set; }

        [XmlAttribute]
        public string Comment { get; set; }
        
        [XmlIgnore]
         public override string Name => StartTime.ToString("yy-MMdd-HHmmss");
    }
}