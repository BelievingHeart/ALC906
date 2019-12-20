using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion

        #region prop

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
            get
            {
                return IsDayEnable && _isHourEnable;
            }
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




        #endregion



        #region Constructor

        public DatabaseQueryViewModel()
        {
            QueryByHourCommand = new RelayCommand(()=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyQuerying, QueryByHourAsync));
            SwitchSettingViewCommand = new SimpleCommand(o=>CurrentDatabaseContentPage = DatabaseContentPageType.SettingPage, o=>CurrentDatabaseContentPage!=DatabaseContentPageType.SettingPage);
            SwitchTablesViewCommand = new SimpleCommand(o=>CurrentDatabaseContentPage = DatabaseContentPageType.TablePage, o=>CurrentDatabaseContentPage!=DatabaseContentPageType.TablePage);
            
            DatabaseBuffer = new DatabaseBufferViewModel();
            
            DoSimulationCommand = new RelayCommand(DoSimulation);
            
            LoadFaiLimits(_productType);
            GenerateLimitDictionaries();
        }

        #endregion

        private void DoSimulation()
        {
           DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>()
            {
                new FaiCollectionTest(){Cavity = 1, InspectionTime = "SomeTime", Result = "SomeResult", Test = "Hello"}
            };
        }

        private  async Task QueryByHourAsync()
        {

            DatabaseBuffer.FaiCollectionBuffers = await Task.Run(() => FaiCollectionHelper.SelectByHour(ProductType,
                NameConstants.SqlConnectionString, Year, Month,
                Day, Hour));
        }
        
        private void LoadFaiLimits(ProductType productType)
        {
            var collectionType = productType == ProductType.Mtm ? typeof(FaiCollectionMtm) : typeof(FaiCollectionAlps);
            var propNames = GetFaiPropNames(collectionType);
            FaiLimits = AutoSerializableHelper
                .LoadAutoSerializables<FaiLimitViewModel>(propNames, DirectoryConstants.DatabaseLimitsDirs[productType])
                .ToList();
        }

        private IEnumerable<string> GetFaiPropNames(Type collectionType)
        {
            return collectionType.GetProperties().Where(prop => prop.Name.Contains("FAI")).Select(info => info.Name);
        }
        
    }
}