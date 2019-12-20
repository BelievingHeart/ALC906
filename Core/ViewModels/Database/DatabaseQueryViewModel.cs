using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ViewModels.Database.FaiCollection;
using MaterialDesignThemes.Wpf;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database
{
    public class DatabaseQueryViewModel : ViewModelBase
    {
        #region private fields

        private ProductType _productType;
        private DatabaseContentPageType _currentDatabaseContentPage;
        private Dictionary<string, int> _pieChartData;

        #endregion

        #region prop

        public bool IsBusyGeneratingPieChart { get; set; }

        public Type CollectionType { get; set; }

        public Dictionary<string, int> PieChartData
        {
            get => _pieChartData;
            set
            {
                _pieChartData = value;
                if (value != null) ShouldDisplayPieChart = true;
            }
        }

        public bool ShouldDisplayPieChart { get; set; }

        public Dictionary<string, double> DictionaryUpper { get; set; }
        public Dictionary<string, double> DictionaryLower { get; set; }

        public DatabaseBufferViewModel DatabaseBuffer { get; set; }

        public int YearStart { get; set; } = int.Parse(DateTime.Now.ToString("yyyy"));

        public int MonthStart { get; set; } = int.Parse(DateTime.Now.ToString("MM"));
        public int DayStart { get; set; } = int.Parse(DateTime.Now.ToString("dd"));
        public int HourStart { get; set; } = int.Parse(DateTime.Now.ToString("HH"));
        
        public int YearEnd { get; set; } = int.Parse(DateTime.Now.ToString("yyyy"));

        public int MonthEnd { get; set; } = int.Parse(DateTime.Now.ToString("MM"));
        public int DayEnd { get; set; } = int.Parse(DateTime.Now.ToString("dd"));
        public int HourEnd { get; set; } = int.Parse(DateTime.Now.ToString("HH"));

        public bool IsBusyQuerying { get; set; }

        public ProductType ProductType
        {
            get { return _productType; }
            set
            {
                _productType = value;
                LoadFaiLimits(value);
                OnPropertyChanged(nameof(ProductType));
            }
        }

        public List<FaiLimitViewModel> FaiLimits { get; set; }


        public ICommand QueryByIntervalCommand { get; }

        /// <summary>
        /// Do temporary tests 
        /// </summary>
        public ICommand DoSimulationCommand { get; }

        public DatabaseContentPageType CurrentDatabaseContentPage
        {
            get { return _currentDatabaseContentPage; }
            set
            {
                _currentDatabaseContentPage = value;
                if (value == DatabaseContentPageType.TablePage) GenerateLimitDictionaries();
            }
        }

        private void GenerateLimitDictionaries()
        {
            DictionaryLower = FaiLimits.ToDictionary(item => item.Name, item => item.Lower);
            DictionaryUpper = FaiLimits.ToDictionary(item => item.Name, item => item.Upper);
        }

        public ICommand SwitchTablesViewCommand { get; set; }

        public ICommand SwitchSettingViewCommand { get; set; }

        public ICommand GenPieChartCommand { get; }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        #endregion


        #region Constructor

        public DatabaseQueryViewModel()
        {
            QueryByIntervalCommand = new RelayCommand(() =>
                RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyQuerying, QueryByIntervalAsync));
            SwitchSettingViewCommand =
                new SimpleCommand(o => CurrentDatabaseContentPage = DatabaseContentPageType.SettingPage,
                    o => CurrentDatabaseContentPage != DatabaseContentPageType.SettingPage);
            SwitchTablesViewCommand =
                new SimpleCommand(o => CurrentDatabaseContentPage = DatabaseContentPageType.TablePage,
                    o => CurrentDatabaseContentPage != DatabaseContentPageType.TablePage);
            GenPieChartCommand = new SimpleCommand(o=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyGeneratingPieChart, GenPieChartDataAsync),
                o => DatabaseBuffer.FaiCollectionBuffers != null && DatabaseBuffer.FaiCollectionBuffers.Count > 0);

            DatabaseBuffer = new DatabaseBufferViewModel();

            DoSimulationCommand = new RelayCommand(DoSimulation);

            LoadFaiLimits(_productType);
            GenerateLimitDictionaries();
        }

        /// <summary>
        /// Loop through all the fai properties in FaiCollectionBuffer
        /// and count for the Ng occurence for each fai property
        /// remove those with 0 count
        /// </summary>
        private async Task GenPieChartDataAsync()
        {
            var output = new Dictionary<string, int>();

            await Task.Run(() =>
            {
                // Init PieChartData with all 0s
                foreach (var key in DictionaryLower.Keys)
                {
                    output[key] = 0;
                }

                // Accumulate counts for NG fai items
                foreach (var collection in DatabaseBuffer.FaiCollectionBuffers)
                {
                    foreach (var property in CollectionType.GetProperties())
                    {
                        if (!IsFaiProp(property)) continue;
                        var propName = property.Name;
                        var propValue = (double) property.GetValue(collection);
                        if (IsFaiPropNg(propValue, propName, DictionaryLower, DictionaryUpper))
                            output[propName]++;
                    }
                }

                // Prune fai items with 0 Ng count
                foreach (var item in output.Where(kvp => kvp.Value == 0).ToList())
                {
                    output.Remove(item.Key);
                }
            });
            
            PieChartData = output;
        }


        private bool IsFaiPropNg(double propValue, string propName, Dictionary<string, double> dictionaryLower,
            Dictionary<string, double> dictionaryUpper)
        {
            return propValue < dictionaryLower[propName] || propValue > dictionaryUpper[propName];
        }

        #endregion

        private void DoSimulation()
        {
            DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>()
            {
                new FaiCollectionTest() {Cavity = 1, InspectionTime = "SomeTime", Result = "SomeResult", Test = "Hello"}
            };
        }

        private async Task QueryByIntervalAsync()
        {
            var dateStart = ParseDateTime(YearStart, MonthStart, DayStart, HourStart);
            var dateEnd = ParseDateTime(YearEnd, MonthEnd, DayEnd, HourEnd);
            if (dateEnd <= dateStart)
            {
                PromptUser("Datetime end should greater than datetime start.");
                return;
            }
            
            DatabaseBuffer.FaiCollectionBuffers = await Task.Run(() => FaiCollectionHelper.SelectByInterval(ProductType,
                NameConstants.SqlConnectionString, dateStart, dateEnd));
        }

        private void PromptUser(string message)
        {
            SnackbarMessageQueue.Enqueue(message);
        }

        private DateTime ParseDateTime(int year, int month, int day, int hour)
        {
            var dateText = $"{year}-{month:D2}-{day:D2} {hour:D2}:00";
            var date = dateText.ToDate("yyyy-MM-dd HH:mm");
//            return DateTime.ParseExact("yyyy-MM-dd HH:mm", dateText, CultureInfo.InvariantCulture);
           return (DateTime) date;
        }

        private void LoadFaiLimits(ProductType productType)
        {
            CollectionType = productType == ProductType.Mtm ? typeof(FaiCollectionMtm) : typeof(FaiCollectionAlps);
            var propNames = GetFaiPropNames(CollectionType);
            FaiLimits = AutoSerializableHelper
                .LoadAutoSerializables<FaiLimitViewModel>(propNames, DirectoryConstants.DatabaseLimitsDirs[productType])
                .ToList();
        }


        private IEnumerable<string> GetFaiPropNames(Type collectionType)
        {
            return collectionType.GetProperties().Where(IsFaiProp).Select(info => info.Name);
        }

        private bool IsFaiProp(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name.Contains("FAI");
        }
    }
}