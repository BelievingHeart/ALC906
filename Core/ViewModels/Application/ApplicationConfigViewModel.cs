using System;
using System.Xml.Serialization;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Application
{
    public class ApplicationConfigViewModel : AutoSerializableBase<ApplicationConfigViewModel>
    {
        [XmlAttribute] public DateTime LastBackupDate3D { get; set; }
        [XmlAttribute] public DateTime LastRecoverDate3D { get; set; }
    }
}