using System.Collections.Generic;
using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Fai
{
    public class DataRowViewModel : ViewModelBase
    {
        private List<FaiItem> _values;
        private string _rowName;
        private ProductLevel _productLevel;

        public List<FaiItem> Values
        {
            get { return _values; }
            set
            {
                _values = value;
                OnPropertyChanged();
            }
        }

        public string RowName
        {
            get { return _rowName; }
            set
            {
                _rowName = value;
                OnPropertyChanged();
            }
        }

        public ProductLevel ProductLevel
        {
            get { return _productLevel; }
            set
            {
                _productLevel = value;
                OnPropertyChanged();
            }
        }
    }
}