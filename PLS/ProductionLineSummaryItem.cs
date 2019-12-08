using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using WPFCommon.ViewModels.Base;

namespace PLS
{
    [Serializable]
    public class ProductionLineSummaryItem : ViewModelBase
    {
        public event Action<Dictionary<string, int>> BinsIncremented;
        
        /// <summary>
        /// Time when the item is created
        /// </summary>
        [XmlAttribute] public DateTime TimeCreated { get; set; }
        /// <summary>
        /// The most recent time when the item get updated
        /// </summary>
        [XmlAttribute] public DateTime TimeUpdated { get; set; }

        /// <summary>
        /// Bin name and count of corresponding bin
        /// </summary>
        [XmlAttribute]
        public Dictionary<string, int> BinsAndCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Name of the summary,
        /// for example, 23 for summary from 23:00 to 24:00
        /// </summary>
        [XmlAttribute]
        public string SummaryName { get; set; }

        /// <summary>
        /// Total count of products from new hour start
        /// </summary>
        public int TotalCount => BinsAndCounts.Values.Sum();

        /// <summary>
        /// The name of which bin that will store the count of ok products
        /// </summary>
        [XmlAttribute]
        public string OkKey { get; set; } = "OK";


        public int UnitsPerHour
        {
            get
            {
                var workingSeconds = (TimeUpdated - TimeCreated).Seconds;
                var unitsPerSecond = TotalCount / (double)workingSeconds;
                return (int) unitsPerSecond * 3600;
            }
        }

        /// <summary>
        /// Count-of-OK / Count-of-total
        /// </summary>
        [XmlIgnore]
        public double Yield
        {
            get
            {
                if (BinsAndCounts == null || BinsAndCounts.Count == 0)
                {
                    return 0;
                }

                var notHaveOkKey = BinsAndCounts.Keys.FirstOrDefault(key => key == OkKey) == null;
                if (notHaveOkKey)
                    throw new KeyNotFoundException($"{nameof(BinsAndCounts)} does not contain an OkKey named {OkKey}");

                var countOfTotal = BinsAndCounts.Sum(pair => pair.Value);
                var countOfOk = (double) BinsAndCounts[OkKey];
                return countOfOk / countOfTotal;
            }
        }

        public ProductionLineSummaryItem()
        {
            TimeCreated = DateTime.Now;
        }

        /// <summary>
        /// Increment bin values
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="KeyNotFoundException">
        /// </exception>
        public void IncrementBins(Dictionary<string, int> values)
        {
            foreach (var key in values.Keys)
            {
                if (!BinsAndCounts.Keys.Contains(key))
                    throw new KeyNotFoundException($"Can not find a bin named {key}");
                BinsAndCounts[key] = values[key];
            }

            TimeUpdated = DateTime.Now;
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(UnitsPerHour));
            OnBinsIncremented(BinsAndCounts);
        }

        public void Serialize(string serializationDir)
        {
            using (var fs = new FileStream(Path.Combine(serializationDir, SummaryName + ".xml")
                , FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(ProductionLineSummaryItem));
                serializer.Serialize(fs, this);
            }
        }

        public static ProductionLineSummaryItem LoadFromDisk(string dir, string name, List<string> binNames,
            string okKey = "OK")
        {
            if (!binNames.Contains(okKey))
                throw new ArgumentException($"{okKey} can not be found in {nameof(binNames)}");

            var filePath = Path.Combine(dir, name + ".xml");
            ProductionLineSummaryItem output;
            // Load if exists
            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(ProductionLineSummaryItem));
                    output = (ProductionLineSummaryItem) serializer.Deserialize(fs);
                }
            }
            else // Create one
            {
                var binsAndCounts = new Dictionary<string, int>();
                foreach (var binName in binNames)
                {
                    binsAndCounts[binName] = 0;
                }

                output = new ProductionLineSummaryItem()
                    {SummaryName = name, BinsAndCounts = binsAndCounts, OkKey = okKey};
            }


            return output;
        }

        protected virtual void OnBinsIncremented(Dictionary<string, int> obj)
        {
            BinsIncremented?.Invoke(obj);
        }
    }
}