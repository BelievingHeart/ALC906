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
            get { return DateTime.Now.ToString("hh"); }
        }

        /// <summary>
        /// Get current date by string
        /// </summary>
        public string CurrentDate
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
        }


        public void UpdateCurrentSummary(Dictionary<string, int> summaryDict)
        {
            // If hour changed
            if (CurrentHourChanged)
            {
                UpdatePreviousHour();
                // Save current summary
                CurrentSummaryItem.Serialize(SerializationDirToday);

                // Create new summary for new hour
                CurrentSummaryItem = CreateNewSummaryItem();
                AllSummaryItems.Add(CurrentSummaryItem);

                OnHourChanged();
                if (CurrentDateChanged)
                {
                    UpdatePreviousDate();
                    CreateAllSummaries();
                }
            }

            CurrentSummaryItem.IncrementBins(summaryDict);
            TodaySummaryItem.IncrementBins(summaryDict);

            OnCurrentSummaryItemUpdated(CurrentSummaryItem);
        }

        private void UpdatePreviousDate()
        {
            _previousDate = CurrentDate;
        }

        public bool CurrentDateChanged => _previousDate != CurrentDate;

        private void UpdatePreviousHour()
        {
            _previousHour = CurrentHour;
        }

        public bool CurrentHourChanged => CurrentHour != _previousHour;


        private ProductionLineSummaryItem CreateNewSummaryItem()
        {
            var dict = new Dictionary<string, int>();
            foreach (var name in BinNames)
            {
                dict[name] = 0;
            }

            return new ProductionLineSummaryItem() {BinsAndCounts = dict, OkKey = _okKey, SummaryName = CurrentHour};
        }


        public ProductionLineSummary(string serializationBaseDir, List<string> binNames, string okKey = "OK")
        {
            SerializationBaseDir = serializationBaseDir;
            BinNames = binNames;
            _okKey = okKey;

            CreateAllSummaries();
        }

        private void CreateAllSummaries()
        {
            CreateDirToday();
            CurrentSummaryItem = LoadCurrentSummaryItem();

            // Init today's summary items list
            var pastSummaryItems = LoadPastSummaryItems();
            pastSummaryItems.Add(CurrentSummaryItem);
            AllSummaryItems = pastSummaryItems;

            CreateSummaryItemToday();
        }

        private void CreateSummaryItemToday()
        {
            TodaySummaryItem = new ProductionLineSummaryItem() {SummaryName = "Today"};
            foreach (var binName in BinNames)
            {
                TodaySummaryItem.BinsAndCounts[binName] =
                    AllSummaryItems.Select(pair => pair.BinsAndCounts[binName]).Sum();
            }

            TodaySummaryItem.TimeCreated = AllSummaryItems[0].TimeCreated;
        }


        /// <summary>
        /// Load summary items in the past of today
        /// </summary>
        /// <returns></returns>
        private List<ProductionLineSummaryItem> LoadPastSummaryItems()
        {
            var currentFileName = CurrentHour + ".xml";
            var output = new List<ProductionLineSummaryItem>();
            var xmlsInTodaysDir = Directory.GetFiles(SerializationDirToday).Where(file => file.EndsWith("xml"));
            foreach (var xml in xmlsInTodaysDir)
            {
                if (xml.Contains(currentFileName)) continue;

                ProductionLineSummaryItem item;
                using (var fs = new FileStream(xml, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(ProductionLineSummaryItem));
                    item = (ProductionLineSummaryItem) serializer.Deserialize(fs);
                }

                output.Add(item);
            }

            return output.OrderBy(item => item.SummaryName).ToList();
        }

        /// <summary>
        /// Create today's summary dir if not exists
        /// </summary>
        /// <returns>directory string</returns>
        private void CreateDirToday()
        {
            Directory.CreateDirectory(SerializationDirToday);
        }

        /// <summary>
        /// Load summary item of current hour if exists
        /// otherwise create new one
        /// </summary>
        /// <returns></returns>
        private ProductionLineSummaryItem LoadCurrentSummaryItem()
        {
            return ProductionLineSummaryItem.LoadFromDisk(SerializationDirToday, CurrentHour, BinNames, _okKey);
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