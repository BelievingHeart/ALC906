using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<FaiCollectionMtm> TableToShow { get; set; }

        public int Year
        {
            get => _year;
            set => _year = value;
        }

        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }

        public ProductType ProductType
        {
            get => _productType;
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

        private void ParseDate(DateTime date)
        {

            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }

        public DatabaseQueryViewModel()
        {
            QueryByHourCommand = new RelayCommand(QueryByHour);
        }

        private void QueryByHour()
        {
            ParseDate(Date);
            var output = FaiCollectionHelper.SelectByHour(ProductType, NameConstants.SqlConnectionString, Year, Month,
                Day, Hour);
            TableToShow = new List<FaiCollectionMtm>(output.Cast<FaiCollectionMtm>());

        }
    }
}