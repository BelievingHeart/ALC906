using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WPFCommon.ViewModels.Base;

namespace PLS
{
    public class ProductionLineSummary : ViewModelBase
    {
        public event Action<ProductionLineSummaryItem> CurrentSummaryItemUpdated;
        public event Action HourChanged;

        /// <summary>
        /// Names of all the bins
        /// </summary>
        public List<string> BinNames { get; }

        /// <summary>
        /// Name of ok bin
        /// </summary>
        private string _okKey;

        private string _previousHour;

        /// <summary>
        /// Summary items of today
        /// </summary>
        public List<ProductionLineSummaryItem> AllSummaryItems { get; set; }

        private ProductionLineSummaryItem _currentSummaryItem;
        private string _previousDate;

        /// <summary>
        /// The directory that will hold summaries of every day
        /// </summary>
        public string SerializationBaseDir { get; set; }

        /// <summary>
        /// The directory that will hold today's summary
        /// </summary>
        private string SerializationDirToday => Path.Combine(SerializationBaseDir, CurrentDate);

        /// <summary>
        /// Summary item of current hour
        /// </summary>
        public ProductionLineSummaryItem CurrentSummaryItem
        {
            get { return _currentSummaryItem; }
            set
            {
                _currentSummaryItem = value;
                OnPropertyChanged();
            }
        }

        public ProductionLineSummaryItem TodaySummaryItem { get; set; }

        /// <summary>
        /// Get current hour by string
        /// </summary>
        public string CurrentHour
        {
            get { return DateTime.Now.ToString("HH"); }
        }

        /// <summary>
        /// Get current date by string
        /// </summary>
        public string CurrentDate
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
        }


        public void UpdateCurrentSummary(string binName)
        {
            // If hour changed
            if (CurrentHourChanged)
            {
                _previousHour = CurrentHour;
                // Save current summary
                SerializeCurrentSummary();

                // Create new summary for new hour
                CurrentSummaryItem = LoadCurrentSummaryItem(SerializationDirToday, CurrentHour, BinNames, _okKey);
                AllSummaryItems.Add(CurrentSummaryItem);

                OnHourChanged();
                if (CurrentDateChanged)
                {
                    _previousDate = CurrentDate;
                    CreateSummariesForNewDay(BinNames);
                }
            }

            CurrentSummaryItem.IncrementBins(binName);
            TodaySummaryItem.IncrementBins(binName);

            OnCurrentSummaryItemUpdated(CurrentSummaryItem);
        }

        public void SerializeCurrentSummary()
        {
            CurrentSummaryItem.Serialize(SerializationDirToday);
        }

        public bool CurrentDateChanged => _previousDate != CurrentDate;

        public bool CurrentHourChanged => CurrentHour != _previousHour;

        


        public ProductionLineSummary(string serializationBaseDir, List<string> binNames, string okKey = "OK")
        {
            SerializationBaseDir = serializationBaseDir;
            BinNames = binNames;
            _okKey = okKey;
            _previousDate = CurrentDate;
            _previousHour = CurrentHour;

            CreateSummariesForNewDay(binNames);
        }

        private void CreateSummariesForNewDay(List<string> binNames)
        {
            Directory.CreateDirectory(SerializationDirToday);
            CurrentSummaryItem = LoadCurrentSummaryItem(SerializationDirToday, CurrentHour, binNames, _okKey);

            // Init today's summary items list
            var pastSummaryItems = LoadPastSummaryItems(SerializationDirToday, binNames, _okKey, CurrentHour);
            pastSummaryItems.Add(CurrentSummaryItem);
            AllSummaryItems = pastSummaryItems;

           TodaySummaryItem = CreateSummaryItemToday(AllSummaryItems, binNames);
        }

        private static ProductionLineSummaryItem CreateSummaryItemToday(List<ProductionLineSummaryItem> summaryItems, List<string> binNames)
        {
            var output = new ProductionLineSummaryItem() {SummaryName = "Today"};
            foreach (var binName in binNames)
            {
                output.BinsAndCounts[binName] =
                    summaryItems.Select(pair => pair.BinsAndCounts[binName]).Sum();
            }
            output.TimeCreated = summaryItems[0].TimeCreated;
            output.TimeUpdated = summaryItems.Last().TimeUpdated;
            return output;
        }


        /// <summary>
        /// Load summary items in the past of today
        /// </summary>
        /// <returns></returns>
        private static List<ProductionLineSummaryItem> LoadPastSummaryItems(string serializationDir, List<string> binNames, string okKey, string currentItemName)
        {
            var output = new List<ProductionLineSummaryItem>();
            var summaryFileNames = Directory.GetFiles(serializationDir).Where(file => file.EndsWith("summary")).Select(Path.GetFileName);
            var fileNamesWithoutExt = summaryFileNames.Select(name => name.Substring(0, name.IndexOf(".")));
            foreach (var fileName in fileNamesWithoutExt)
            {
                if (fileName.Contains(currentItemName)) continue;

                var item = ProductionLineSummaryItem.LoadFromDisk(serializationDir, fileName, binNames, okKey);

                output.Add(item);
            }

            return output.OrderBy(item => item.SummaryName).ToList();
        }

        /// <summary>
        /// Load summary item of current hour if exists
        /// otherwise create new one
        /// </summary>
        /// <returns></returns>
        private ProductionLineSummaryItem LoadCurrentSummaryItem(string serializationDirToday, string itemName, List<string> binNames, string okKey)
        {
            return ProductionLineSummaryItem.LoadFromDisk(serializationDirToday, itemName, binNames, okKey);
        }

        protected virtual void OnCurrentSummaryItemUpdated(ProductionLineSummaryItem obj)
        {
            CurrentSummaryItemUpdated?.Invoke(obj);
        }

        protected virtual void OnHourChanged()
        {
            HourChanged?.Invoke();
        }
    }
}