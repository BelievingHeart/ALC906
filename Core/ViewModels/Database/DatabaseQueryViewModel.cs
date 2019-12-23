using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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
        private bool _shouldDisplayDialog;
        private IList<IFaiCollection> _selectedCollections;
        private DatabaseViewDialogType _currentDialogType;

        #endregion

        #region prop

        public int SelectionCount
        {
            get { return SelectedCollections.Count; }
        }

        public IList<IFaiCollection> SelectedCollections
        {
            get { return _selectedCollections; }
            set
            {
                _selectedCollections = value;
                OnPropertyChanged(nameof(SelectionCount));
            }
        }

        public bool IsBusyDeleting { get; set; }
        
        public LineChartUnitType LineChartUnitType { get; set; }

        public bool IsBusyGeneratingPieChart { get; set; }

        public Type CollectionType { get; set; }

        public Dictionary<string, int> PieChartData
        {
            get { return _pieChartData; }
            set
            {
                _pieChartData = value;
                if (_pieChartData == null) return;
                CurrentDialogType = DatabaseViewDialogType.PieChartDialog;
            }
        }

        public bool ShouldDisplayDialog
        {
            get { return _shouldDisplayDialog; }
            set
            {
                _shouldDisplayDialog = value;
                // Allow CurrentDialogType to notify next time
                if (!value) CurrentDialogType = DatabaseViewDialogType.None;
            }
        }

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
        
        public ICommand OpenDeleteDialogCommand { get; }

        public ICommand OpenSaveDialogCommand { get;  }

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

        public bool IsBusyGeneratingLineCharts { get; private set; }

        public ICommand GenLineChartsCommand { get; }

        public DatabaseViewDialogType CurrentDialogType
        {
            get { return _currentDialogType; }
            private set
            {
                _currentDialogType = value;
                if (value == DatabaseViewDialogType.None) return;
                ShouldDisplayDialog = true;
            }
        }

        public ICommand DeleteAllCommand { get; }

        public ICommand DeleteSelectionCommand { get; }

        #endregion


        #region ctor

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
            
            GenLineChartsCommand = new SimpleCommand(o=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyGeneratingLineCharts, GenLineChartsDataAsync),
                o => DatabaseBuffer.FaiCollectionBuffers != null && DatabaseBuffer.FaiCollectionBuffers.Count > 0);
            
            OpenDeleteDialogCommand = new SimpleCommand(OpenDeleteDialog, o=>CurrentDatabaseContentPage == DatabaseContentPageType.TablePage && DatabaseBuffer.CollectionCount > 0);

            //TODO: add delete ui elements logic after database deletions
            DeleteAllCommand = new SimpleCommand(o=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyDeleting,DeleteAll),
                o => DatabaseBuffer.FaiCollectionBuffers != null && DatabaseBuffer.FaiCollectionBuffers.Count > 0);
            
            DeleteSelectionCommand = new SimpleCommand(o=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyDeleting,DeleteSelection),
                o => SelectedCollections != null && SelectedCollections.Count > 0);
            
            DatabaseBuffer = new DatabaseBufferViewModel();

            DoSimulationCommand = new RelayCommand(DoSimulation);

            LoadFaiLimits(_productType);
            GenerateLimitDictionaries();
        }

        /// <summary>
        /// Delete all local buffer data from database
        /// and query agian
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAll()
        {
            CurrentDialogType = DatabaseViewDialogType.WaitingDialog;
            await FaiCollectionHelper.DeleteByDateTimeAsync(DatabaseBuffer.FaiCollectionBuffers,
                NameConstants.SqlConnectionString);
            await QueryByIntervalAsync();
            ShouldDisplayDialog = false;
        }

        private async Task DeleteSelection()
        {
            CurrentDialogType = DatabaseViewDialogType.WaitingDialog;
            
            // Delete in database
            await FaiCollectionHelper.DeleteByDateTimeAsync(SelectedCollections, NameConstants.SqlConnectionString);
            
            // Get last index in buffer before selections
            var indexOfFirstSelectedCollections = DatabaseBuffer.FaiCollectionBuffers.IndexOf(SelectedCollections[0]);
            var lastIndexBeforeSelections = indexOfFirstSelectedCollections == 0 ? 0 : indexOfFirstSelectedCollections - 1;
            // Calculate the page index of lastIndexBeforeSelection
            var pageIndex = (int)Math.Floor(lastIndexBeforeSelections / (double) DatabaseBuffer.RowsPerPage);
            // Remove selections from buffer
            DatabaseBuffer.Remove(SelectedCollections);
            // Navigate to page where the first selection is deleted
            DatabaseBuffer.NavigateToPage(pageIndex);
            
            ShouldDisplayDialog = false;
        }


        /// <summary>
        /// Open delete dialog to ask whether delete all or delete selection
        /// </summary>
        private void OpenDeleteDialog(object o)
        {
            var items = (IList) o;
            SelectedCollections = items.Cast<IFaiCollection>().ToList();
            CurrentDialogType = DatabaseViewDialogType.DeleteDialog;
        }

        private async Task GenLineChartsDataAsync()
        {
            // Determine unit type of line chart
            LineChartUnitType = DatabaseBuffer.TotalDays > 1 ? LineChartUnitType.Day : LineChartUnitType.Hour;
            
            // Gen line chart data measured by day
            if (LineChartUnitType == LineChartUnitType.Day)
            {
                var firstDay = DatabaseBuffer.MinDate.Date;
                var lastDay = (DatabaseBuffer.MaxDate + TimeSpan.FromDays(1)).Date;
                var dateList = new List<DateTime>();
                var countList = new List<int>();
          
                // TODO: finish this
            }
            else
            // Gen line chart data measured by hour
            {
                
            }
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
                new FaiCollectionTest() {Cavity = 1, InspectionTime = DateTime.Now, Result = "SomeResult", Test = "Hello"}
            };
        }

        private async Task QueryByIntervalAsync()
        {
            var dateStart = ParseDateTime(YearStart, MonthStart, DayStart, HourStart);
            var dateEnd = ParseDateTime(YearEnd, MonthEnd, DayEnd, HourEnd);

            if (!dateStart.HasValue || !dateEnd.HasValue)
            {
                PromptUser("Invalid date");
                return;
            }
            
            if (dateEnd.Value <= dateStart.Value)
            {
                PromptUser("Datetime end should greater than datetime start.");
                return;
            }

            if ((dateEnd.Value - dateStart.Value).TotalDays > 31)
            {
                PromptUser("Can not query more than 31 days");
                return;
            }

            if (ProductType == ProductType.Mtm)
            {
                var output = await  FaiCollectionHelper.SelectByIntervalAsync<FaiCollectionMtm>(ProductType,
                    NameConstants.SqlConnectionString, dateStart.Value, dateEnd.Value);
                DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>(output);
            }
            else
            {
                var output = await  FaiCollectionHelper.SelectByIntervalAsync<FaiCollectionAlps>(ProductType,
                    NameConstants.SqlConnectionString, dateStart.Value, dateEnd.Value);
                DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>(output);
            }
            
            DatabaseBuffer.NavigateToPage(0);
        }

        private void PromptUser(string message)
        {
            SnackbarMessageQueue.Enqueue(message);
        }

        private DateTime? ParseDateTime(int year, int month, int day, int hour)
        {
            var dateText = $"{year}-{month:D2}-{day:D2} {hour:D2}:00";
            var date = dateText.ToDate("yyyy-MM-dd HH:mm");
//            return DateTime.ParseExact("yyyy-MM-dd HH:mm", dateText, CultureInfo.InvariantCulture);
           return date;
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