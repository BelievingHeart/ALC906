using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ViewModels.Database.FaiCollection;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database
{
    public class DatabaseQueryViewModel : ViewModelBase
    {
        #region private fields

        private ProductType _productType;
        private bool _isHourEnable;
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

        public int Year { get; set; } = int.Parse(DateTime.Now.ToString("yyyy"));

        public int Month { get; set; } = int.Parse(DateTime.Now.ToString("MM"));
        public int Day { get; set; } = int.Parse(DateTime.Now.ToString("dd"));
        public int Hour { get; set; } = int.Parse(DateTime.Now.ToString("HH"));

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


        public ICommand QueryByHourCommand { get; }

        /// <summary>
        /// Do temporary tests 
        /// </summary>
        public ICommand DoSimulationCommand { get; }

        public bool IsDayEnable { get; set; }

        public bool IsHourEnable
        {
            get { return IsDayEnable && _isHourEnable; }
            set { _isHourEnable = value; }
        }

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

        #endregion


        #region Constructor

        public DatabaseQueryViewModel()
        {
            QueryByHourCommand = new RelayCommand(() =>
                RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyQuerying, QueryByHourAsync));
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

        private async Task QueryByHourAsync()
        {
            DatabaseBuffer.FaiCollectionBuffers = await Task.Run(() => FaiCollectionHelper.SelectByHour(ProductType,
                NameConstants.SqlConnectionString, Year, Month,
                Day, Hour));
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