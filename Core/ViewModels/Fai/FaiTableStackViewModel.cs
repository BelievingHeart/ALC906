using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Fai
{
    public class FaiTableStackViewModel : ViewModelBase
    {
        public event Action RowsRemoved;
        public event Action<DataRowViewModel> NewRowAdded;
        
        
        private List<string> _header;
        private List<DataRowViewModel> _valueMatrix = new List<DataRowViewModel>();
        private int _maxRows;
        private double _portionToRemoveWhenOverflows = 0.3;
        private List<double> _max;
        private List<double> _min;

        public List<string> Header
        {
            get { return _header; }
            set
            {
                _header = value;
            }
        }

        public List<double> Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public List<double> Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public List<DataRowViewModel> ValueMatrix
        {
            get { return _valueMatrix; }
           private set
            {
                _valueMatrix = value;
                OnPropertyChanged(nameof(ValueMatrix));
            }
        }

        public int MaxRows
        {
            get { return _maxRows; }
            set
            {
                _maxRows = value;
                OnPropertyChanged(nameof(MaxRows));
            }
        }

        public double PortionToRemoveWhenOverflows
        {
            get { return _portionToRemoveWhenOverflows; }
            set
            {
                _portionToRemoveWhenOverflows = value; 
                OnPropertyChanged(nameof(PortionToRemoveWhenOverflows));
                
            }
        }

        public int Rows
        {
            get { return ValueMatrix.Count; }
        }

        public void AddRow(List<FaiItem> faiItems, ProductLevel productLevel, string rowName)
        {
            // Sort fai items by header order
            var values = new List<FaiItem>();
           
            foreach (var faiName in _header)
            {
                values.Add(faiItems.First(item=>item.Name == faiName).Clone());
            }

            var row = new DataRowViewModel() {Values = values, RowName = rowName, ProductLevel = productLevel};
            ValueMatrix.Add(row);
            OnPropertyChanged(nameof(Rows));
            OnNewRowAdded(row);
            
            if (Rows > MaxRows) RemoveMultipleRows();
        }

        private void RemoveMultipleRows()
        {
            var rowsToRemove = (int) (MaxRows * PortionToRemoveWhenOverflows);
            ValueMatrix = ValueMatrix.Skip(rowsToRemove).ToList();
            OnPropertyChanged(nameof(Rows));
            OnRowsRemoved();
        }

        protected virtual void OnRowsRemoved()
        {
            RowsRemoved?.Invoke();
        }

        protected virtual void OnNewRowAdded(DataRowViewModel newRow)
        {
            NewRowAdded?.Invoke(newRow);
        }
    }
}