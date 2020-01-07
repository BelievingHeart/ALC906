using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.ViewModels.Database.FaiCollection;
using Core.ViewModels.Login;
using LiveCharts;
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
        private Dictionary<string, double> _dictionaryLower;
        private Dictionary<string, double> _dictionaryUpper;

        #endregion

        #region prop

        public LoginViewModel LoginViewModel { get; set; }

        public LineChartData ProductionSeriesData { get; set; }
        public LineChartData YieldSeriesData { get; set; }

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


        public DatabaseBufferViewModel DatabaseBuffer { get; set; }

        public DateTimeViewModel DateTimeViewModelStart { get; set; }
        public DateTimeViewModel DateTimeViewModelEnd { get; set; }

        public bool IsBusyQuerying { get; set; }

        public ProductType ProductType
        {
            get { return _productType; }
            set
            {
                _productType = value;
                LoadFaiLimits(value);
                OnPropertyChanged(nameof(ProductType));
                DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>();
                DatabaseBuffer.NavigateToPage(0);
            }
        }

        public List<FaiLimitViewModel> FaiLimits { get; set; }


        public ICommand QueryByIntervalCommand { get; }

        /// <summary>
        /// Do temporary tests 
        /// </summary>
        public ICommand DoSimulationCommand { get; }

        public ICommand OpenDeleteDialogCommand { get; }

        public ICommand OpenSaveDialogCommand { get; }

        public DatabaseContentPageType CurrentDatabaseContentPage
        {
            get { return _currentDatabaseContentPage; }
            set
            {
                var previousPage = _currentDatabaseContentPage;
                _currentDatabaseContentPage = value;
                // Update table because
                // the user may changed setting before view switching to table view
                if (value == DatabaseContentPageType.TablePage && previousPage == DatabaseContentPageType.SettingPage && DatabaseBuffer.CollectionCount > 0)
                    GenTableToShow();
            }
        }

        public IList<FaiCollectionItemViewModel> FaiCollectionItemViewModels { get; set; }

        public ICommand SwitchTablesViewCommand { get; set; }

        public ICommand SwitchSettingViewCommand { get; set; }

        public ICommand GenPieChartCommand { get; }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

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

        public string CsvDir { get; set; }

        public ICommand SaveAllCommand { get; }

        public ICommand SaveSelectionCommand { get; }

        public ICommand SwitchLoginViewCommand { get; }

        #endregion


        #region ctor

        public DatabaseQueryViewModel()
        {
            QueryByIntervalCommand = new RelayCommand(() =>
                RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyQuerying, QueryByIntervalAsync));
            SwitchSettingViewCommand =
                new SimpleCommand(o => CurrentDatabaseContentPage = DatabaseContentPageType.SettingPage,
                    o => CurrentDatabaseContentPage != DatabaseContentPageType.SettingPage&& LoginViewModel.Authorized);
            SwitchTablesViewCommand =
                new SimpleCommand(o => CurrentDatabaseContentPage = DatabaseContentPageType.TablePage,
                    o => CurrentDatabaseContentPage != DatabaseContentPageType.TablePage);
            
            SwitchLoginViewCommand = new RelayCommand(()=>CurrentDialogType = DatabaseViewDialogType.LoginDialog);
            
            GenPieChartCommand = new SimpleCommand(
                o => RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyGeneratingPieChart, GenPieChartDataAsync),
                o => DatabaseBuffer.CollectionCount > 0);

            GenLineChartsCommand = new SimpleCommand(
                o => RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyGeneratingLineCharts,
                    GenLineChartsDataAsync),
                o => DatabaseBuffer.CollectionCount > 0);

            // Delete commands
            OpenDeleteDialogCommand = new SimpleCommand(OpenDeleteDialog, o => DatabaseBuffer.CollectionCount > 0 && LoginViewModel.Authorized);
            DeleteAllCommand = new SimpleCommand(
                o => RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyDeleting, DeleteAll),
                o => DatabaseBuffer.CollectionCount > 0);
            DeleteSelectionCommand = new SimpleCommand(
                o => RunOnlySingleFireIsAllowedEachTimeCommand(() => IsBusyDeleting, DeleteSelection),
                o => SelectedCollections != null && SelectedCollections.Count > 0);

            // Save as csv commands
            OpenSaveDialogCommand = new SimpleCommand(OpenSaveDialog, o => DatabaseBuffer.CollectionCount > 0);
            SaveAllCommand = new SimpleCommand(o => SaveCollectionsToCsv(DatabaseBuffer.FaiCollectionBuffers),
                o => DatabaseBuffer.CollectionCount > 0 && Directory.Exists(CsvDir));
            SaveSelectionCommand = new SimpleCommand(o => SaveCollectionsToCsv(SelectedCollections),
                o => SelectedCollections != null && SelectedCollections.Count > 0 && Directory.Exists(CsvDir));


            DatabaseBuffer = new DatabaseBufferViewModel();
            DatabaseBuffer.CollectionToShowChanged += GenTableToShow;

            DoSimulationCommand = new RelayCommand(DoSimulation);

            LoadFaiLimits(_productType);
            GenLimitDictionaries();

            SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            LoadPasswordModule(SnackbarMessageQueue);

            DateTimeViewModelStart = new DateTimeViewModel();
            DateTimeViewModelEnd = new DateTimeViewModel();
        }
        

        #endregion


        #region impl

        private void GenTableToShow()
        {
            GenLimitDictionaries();
            FaiCollectionItemViewModels = DatabaseBuffer.CollectionsToShow.Select(c => new FaiCollectionItemViewModel()
            {
                DictionaryLower = _dictionaryLower,
                DictionaryUpper = _dictionaryUpper,
                FaiCollection = c
            }).ToList();
        }

        private void GenLimitDictionaries()
        {
            _dictionaryLower = FaiLimits.ToDictionary(item => item.Name, item => item.Lower);
            _dictionaryUpper = FaiLimits.ToDictionary(item => item.Name, item => item.Upper);
        }

        private void LoadPasswordModule(ISnackbarMessageQueue snackbarMessageQueue)
        {
            LoginViewModel =
                AutoSerializableHelper.LoadAutoSerializable<LoginViewModel>(DirectoryHelper.ConfigDirectory, "PD");
            LoginViewModel.ShouldAutoSerialize = true;
            LoginViewModel.MessageQueue = snackbarMessageQueue;
        }

        private void SaveCollectionsToCsv(IList<IFaiCollection> faiCollections)
        {
            // Close dialog
            ShouldDisplayDialog = false;
            // Gen header
            var properties = faiCollections[0].GetType().GetProperties();
            var headerRow = new List<string>();
            foreach (var property in properties)
            {
                headerRow.Add(property.Name);
            }

            // Gen max and min row
            var maxRow = new List<string>();
            var minRow = new List<string>();
            foreach (var property in properties)
            {
                var propName = property.Name;
                var isFaiProp = propName.Contains("FAI");
                var max = isFaiProp ? _dictionaryUpper[propName].ToString("F3") : "";
                var min = isFaiProp ? _dictionaryLower[propName].ToString("F3") : "";
                maxRow.Add(max);
                minRow.Add(min);
            }
            
            // Ng count for each fai items
            var faiNgCounts = new Dictionary<string, int>();
            foreach (var key in _dictionaryLower.Keys)
            {
                faiNgCounts[key] = 0;
            }

            // Gen contents row
            var contentsRows = new List<List<string>>();
            foreach (var faiCollection in faiCollections)
            {
                var contentRow = new List<string>();
                foreach (var property in properties)
                {
                    var propValue = property.GetValue(faiCollection);
                    var isFaiProp = property.Name.Contains("FAI");
                    if (isFaiProp)
                    {
                        var value = (double) propValue;
                        if (value > _dictionaryUpper[property.Name] || value < _dictionaryLower[property.Name])
                            faiNgCounts[property.Name]++;
                    }
                    
                    var cellContent = property.PropertyType == typeof(DateTime)
                        ? ((DateTime) propValue).ToString(NameConstants.DateTimeFormat)
                        : propValue.ToString();
                    contentRow.Add(cellContent);
                }

                contentsRows.Add(contentRow);
            }


      
            
            // Create ng count row
            var ngCountRowContent = new List<string>();
            foreach (var property in properties)
            {
                var isFaiProp = property.Name.Contains("FAI");
                ngCountRowContent.Add(isFaiProp? faiNgCounts[property.Name].ToString() : "");
            }
            
            // Append ng count row
            contentsRows.Insert(0, ngCountRowContent);
            
            // combine header and contents
            contentsRows.Insert(0, minRow);
            contentsRows.Insert(0, maxRow);
            contentsRows.Insert(0, headerRow);

            var csvPath = Path.Combine(CsvDir, ProductType + $"-{faiCollections.Count}PCS.xlsx");
            ExcelHelper.CsvToExcel(csvPath, "ALC", contentsRows);
            ExcelHelper.FormatFaiExcel(csvPath, 1, 2, contentsRows.Count-4, minRow.Count-3, 4);
            Process.Start(CsvDir);

        }

        private void OpenSaveDialog(object obj)
        {
            SelectedCollections = (IList<IFaiCollection>) obj;
            CurrentDialogType = DatabaseViewDialogType.SaveDialog;
        }

        /// <summary>
        /// Delete all local buffer data from database
        /// and query again
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
            var lastIndexBeforeSelections =
                indexOfFirstSelectedCollections == 0 ? 0 : indexOfFirstSelectedCollections - 1;
            // Calculate the page index of lastIndexBeforeSelection
            var pageIndex = (int) Math.Floor(lastIndexBeforeSelections / (double) DatabaseBuffer.RowsPerPage);
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
            if (!LoginViewModel.Authorized)
            {
                CurrentDialogType = DatabaseViewDialogType.LoginDialog;
                return;
            }

            SelectedCollections = (IList<IFaiCollection>) o;
            CurrentDialogType = DatabaseViewDialogType.DeleteDialog;
        }

        private async Task GenLineChartsDataAsync()
        {
            await Task.Run(() =>
            {
                var lineChartUnitType = GenProductivityData();
                GenYieldData(lineChartUnitType);
            });
            CurrentDialogType = DatabaseViewDialogType.LineChartDialog;
        }

        private void GenYieldData(LineChartUnitType lineChartUnitType)
        {
            var okCounts = new Dictionary<string, int>();
            List<DateTimePair> dateTimePairs;
            var dateFormat = lineChartUnitType == LineChartUnitType.Day ? "yy-MM-dd" : "MM-dd-HH";
            // Gen line chart data measured by day
            if (lineChartUnitType == LineChartUnitType.Day)
            {
                // Partition timespan into bins by day
                var firstDay = DatabaseBuffer.MinDate.Date;
                var lastDay = (DatabaseBuffer.MaxDate.Date + TimeSpan.FromDays(1)).Date;
                var totalDays = (lastDay - firstDay).Days;
                dateTimePairs = DateTimeHelper.GetDateTimePairs(firstDay, lastDay, totalDays);
            }
            else
            {
                var firstHour = DatabaseBuffer.MinDate.Date + TimeSpan.FromHours(DatabaseBuffer.MinDate.Hour);
                var lastHour = DatabaseBuffer.MaxDate.Date + TimeSpan.FromHours(DatabaseBuffer.MaxDate.Hour + 1);
                var totalHours = (lastHour - firstHour).Hours;
                dateTimePairs = DateTimeHelper.GetDateTimePairs(firstHour, lastHour, totalHours);
            }

            foreach (var dateTimePair in dateTimePairs)
            {
                okCounts[dateTimePair.FromDateTime.ToString(dateFormat)] = 0;
            }

            foreach (var collection in DatabaseBuffer.FaiCollectionBuffers)
            {
                if (collection.Result == "OK") okCounts[collection.InspectionTime.ToString(dateFormat)]++;
            }

            var yieldData = new Dictionary<string, double>();
            foreach (var key in okCounts.Keys)
            {
                var yieldNumber = okCounts[key] / ProductionSeriesData.Data[key] * 100;
                yieldData[key] = yieldNumber;
            }

            YieldSeriesData = new LineChartData()
            {
                Data = yieldData,
                UnitType = lineChartUnitType
            };
        }

        private LineChartUnitType GenProductivityData()
        {
// Determine unit type of line chart
            var lineChartUnitType = DatabaseBuffer.TotalDays >= 1 ? LineChartUnitType.Day : LineChartUnitType.Hour;
            List<DateTimePair> dateTimePairs;
            var dateFormat = lineChartUnitType == LineChartUnitType.Day ? "yy-MM-dd" : "MM-dd-HH";
            var bins = new Dictionary<string, double>();


            // Gen line chart data measured by day
            if (lineChartUnitType == LineChartUnitType.Day)
            {
                // Partition timespan into bins by day
                var firstDay = DatabaseBuffer.MinDate.Date;
                var lastDay = (DatabaseBuffer.MaxDate.Date + TimeSpan.FromDays(1)).Date;
                var totalDays = (lastDay - firstDay).Days;
                dateTimePairs = DateTimeHelper.GetDateTimePairs(firstDay, lastDay, totalDays);
            }
            else
            {
                var firstHour = DatabaseBuffer.MinDate.Date + TimeSpan.FromHours(DatabaseBuffer.MinDate.Hour);
                var lastHour = DatabaseBuffer.MaxDate.Date + TimeSpan.FromHours(DatabaseBuffer.MaxDate.Hour + 1);
                var totalHours = (lastHour - firstHour).Hours;
                dateTimePairs = DateTimeHelper.GetDateTimePairs(firstHour, lastHour, totalHours);
            }

            foreach (var dateTimePair in dateTimePairs)
            {
                bins[dateTimePair.FromDateTime.ToString(dateFormat)] = 0;
            }

            foreach (var collection in DatabaseBuffer.FaiCollectionBuffers)
            {
                bins[collection.InspectionTime.ToString(dateFormat)]++;
            }

            ProductionSeriesData = new LineChartData()
            {
                Data = bins,
                UnitType = lineChartUnitType
            };

            return lineChartUnitType;
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
                foreach (var key in _dictionaryLower.Keys)
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
                        if (IsFaiPropNg(propValue, propName, _dictionaryLower, _dictionaryUpper))
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


        private void DoSimulation()
        {
            DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>()
            {
                new FaiCollectionTest()
                    {Cavity = 1, InspectionTime = DateTime.Now, Result = "SomeResult", Test = "Hello"}
            };
        }

        private async Task QueryByIntervalAsync()
        {
            var dateStart = DateTimeViewModelStart.ToDateTime();
            var dateEnd = DateTimeViewModelEnd.ToDateTime();

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
                var output = await FaiCollectionHelper.SelectByIntervalAsync<FaiCollectionMtm>(ProductType,
                    NameConstants.SqlConnectionString, dateStart.Value, dateEnd.Value);
                DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>(output);
            }
            else
            {
                var output = await FaiCollectionHelper.SelectByIntervalAsync<FaiCollectionAlps>(ProductType,
                    NameConstants.SqlConnectionString, dateStart.Value, dateEnd.Value);
                DatabaseBuffer.FaiCollectionBuffers = new List<IFaiCollection>(output);
            }

            DatabaseBuffer.NavigateToPage(0);
            // Activate related controls
            CommandManager.InvalidateRequerySuggested();
        }

        private void PromptUser(string message)
        {
            SnackbarMessageQueue.Enqueue(message);
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

        #endregion
    }
}