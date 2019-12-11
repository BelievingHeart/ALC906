using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WPFCommon.ViewModels.Base;

namespace PLS
{
    public class ProductionLineSummaryItem : ViewModelBase
    {
        public event Action<Dictionary<string, int>> BinsIncremented;

        /// <summary>
        /// Time when the item is created
        /// </summary>
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// The most recent time when the item get updated
        /// </summary>
        public DateTime TimeUpdated { get; set; }

        /// <summary>
        /// Bin name and count of corresponding bin
        /// </summary>

        public Dictionary<string, int> BinsAndCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Name of the summary,
        /// for example, 23 for summary from 23:00 to 24:00
        /// </summary>

        public string SummaryName { get; set; }

        /// <summary>
        /// Total count of products from new hour start
        /// </summary>
        public int TotalCount => BinsAndCounts.Values.Sum();

        /// <summary>
        /// The name of which bin that will store the count of ok products
        /// </summary>

        public string OkKey { get; set; } = "OK";


        public int UnitsPerHour
        {
            get
            {
                var workingSeconds = (TimeUpdated - TimeCreated).TotalSeconds;
                var unitsPerSecond = TotalCount / workingSeconds;
                return (int) unitsPerSecond * 3600;
            }
        }

        /// <summary>
        /// Count-of-OK / Count-of-total
        /// </summary>

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


        public int NgCount => TotalCount - BinsAndCounts[OkKey];

        public ProductionLineSummaryItem()
        {
            TimeCreated = DateTime.Now;
        }

        /// <summary>
        /// Increment bin values
        /// </summary>
        /// <param name="binName"></param>
        /// <exception cref="KeyNotFoundException">
        /// </exception>
        public void IncrementBins(string binName)
        {
            if (!BinsAndCounts.Keys.Contains(binName))
                throw new KeyNotFoundException($"Can not find a bin named {binName}");
            BinsAndCounts[binName] = BinsAndCounts[binName] + 1;


            TimeUpdated = DateTime.Now;
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(UnitsPerHour));
            OnPropertyChanged(nameof(NgCount));
            OnBinsIncremented(BinsAndCounts);
        }

        public void Serialize(string serializationDir)
        {
            using (var fs = new StreamWriter(Path.Combine(serializationDir, SummaryName + ".summary")))
            {
                foreach (var key in BinsAndCounts.Keys)
                {
                    fs.WriteLine($"{key}={BinsAndCounts[key]}");
                }

                fs.WriteLine($"TimeCreated<>{TimeCreated}");
                fs.WriteLine($"TimeUpdated<>{TimeUpdated}");
            }
        }

        public static ProductionLineSummaryItem LoadFromDisk(string dir, string name, List<string> binNames,
            string okKey)
        {
            if (!binNames.Contains(okKey))
                throw new ArgumentException($"{okKey} can not be found in {nameof(binNames)}");

            var filePath = Path.Combine(dir, name + ".summary");
            var binsAndCounts = new Dictionary<string, int>();
            foreach (var binName in binNames)
            {
                binsAndCounts[binName] = 0;
            }

            var output = new ProductionLineSummaryItem()
                {SummaryName = name, BinsAndCounts = binsAndCounts, OkKey = okKey};
            // Load if exists
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    // dictionary element
                    if (line.Contains("="))
                    {
                        var indexOfSeparator = line.IndexOf("=");
                        var binName = line.Substring(0, indexOfSeparator);
                        var binCountText = line.Substring(indexOfSeparator + 1, line.Length-indexOfSeparator-1);
                        var binCount = int.Parse(binCountText);
                        output.BinsAndCounts[binName] = binCount;
                    }
                    else if (line.Contains("<>")) // Property
                    {
                        var indexOfSeparator = line.IndexOf("<>");
                        var propName = line.Substring(0, indexOfSeparator);
                        var content = line.Substring(indexOfSeparator + 2, line.Length-indexOfSeparator-2);
                        if(propName == nameof(TimeCreated))
                        {
                            output.TimeCreated = DateTime.Parse(content);
                        }
                        
                        if(propName == nameof(TimeUpdated))
                        {
                            output.TimeUpdated = DateTime.Parse(content);
                        }
                    }
                }
            }


            return output;
        }

        protected virtual void OnBinsIncremented(Dictionary<string, int> obj)
        {
            BinsIncremented?.Invoke(obj);
        }
    }
}