using System;
using System.Xml.Serialization;
using Core.CsvSerializer;
using PropertyChanged;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Fai
{
    public sealed partial class FaiItem : AutoSerializableBase<FaiItem>, ICsvColumnElement
    {

        public string CsvName
        {
            get { return Name; }
        }

        /// <summary>
        /// Max boundary of the fai item
        /// </summary>
        [XmlAttribute]public double MaxBoundary { get; set; }

        /// <summary>
        /// Min boundary of the fai item
        /// </summary>
        [XmlAttribute]public double MinBoundary { get; set; }

        /// <summary>
        /// Measured value
        /// </summary>
        [XmlIgnore] [AlsoNotifyFor(nameof(Value))]
        public double ValueUnbiased { get; set; }

        /// <summary>
        /// Measured value plus bias
        /// </summary>
        [XmlIgnore]
         public double Value
        {
            get { return Math.Abs(ValueUnbiased * Weight + Bias); }
            set { throw new NotImplementedException(); }
        }

        [XmlAttribute][AlsoNotifyFor(nameof(Value))] public double Weight { get; set; } = 1;

        /// <summary>
        /// Bias 
        /// </summary>
        [XmlAttribute][AlsoNotifyFor(nameof(Value))]public double Bias { get; set; }


        /// <summary>
        /// Measure result
        /// </summary>
        [XmlIgnore]
        public bool Rejected
        {
            get { return Value < MinBoundary || Value > MaxBoundary; }
        }

        
        [XmlIgnore]
        public bool Passed => !Rejected;
    

        public FaiItem(string name)
        {
            Name = name;
        }

        public FaiItem()
        {

        }

    }
}