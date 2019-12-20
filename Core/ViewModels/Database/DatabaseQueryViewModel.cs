using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Constants;
using Core.Enums;
using Core.Helpers;
using Core.ViewModels.Database.FaiCollection;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database
{
    public class DatabaseQueryViewModel : ViewModelBase
    {
        private DateTime _date = DateTime.Today;
        private int _year;
        private ProductType _productType;
        private IList<IFaiCollection> _tableToShow;

        public IList<IFaiCollection> TableToShow
        {
            get { return _tableToShow; }
            set { _tableToShow = value as FaiCollectionList ?? new FaiCollectionList(value); }
        }

        public int Year
        {
            get { return _year; }
            set { _year = value; }
        }

        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }

        public bool IsBusyQuerying { get; set; }

        public ProductType ProductType
        {
            get { return _productType; }
            set
            {
                _productType = value;
                OnPropertyChanged(nameof(ProductType));
            }
        }

        public DateTime Date
        {   
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public ICommand QueryByHourCommand { get; }
        /// <summary>
        /// Do temporary tests 
        /// </summary>
        public ICommand DoSimulationCommand { get; }

        private void ParseDate(DateTime date)
        {

            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }

        public DatabaseQueryViewModel()
        {
            QueryByHourCommand = new RelayCommand(()=>RunOnlySingleFireIsAllowedEachTimeCommand(()=>IsBusyQuerying, QueryByHourAsync));
            DoSimulationCommand = new RelayCommand(DoSimulation);
        }

        private void DoSimulation()
        {
            TableToShow = new List<IFaiCollection>()
            {
                new FaiCollectionTest(){Cavity = 1, InspectionTime = "SomeTime", Result = "SomeResult", Test = "Hello"}
            };
        }

        private  async Task QueryByHourAsync()
        {
            ParseDate(Date);

            TableToShow = await Task.Run(() => FaiCollectionHelper.SelectByHour(ProductType,
                NameConstants.SqlConnectionString, Year, Month,
                Day, Hour));
        }
        
        
    }
}