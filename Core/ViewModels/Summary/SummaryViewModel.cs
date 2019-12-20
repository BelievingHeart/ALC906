using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Xml.Serialization;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Summary
{
    public class SummaryViewModel : ViewModelBase
    {
        private int _ng2Count;
        private int _ng3Count;
        private int _ng4Count;
        private int _ng5Count;
        private int _okCount;

        public SummaryViewModel()
        {
            ClearCommand = new RelayCommand(ClearSummary);
        }

        private void ClearSummary()
        {
            OkCount = 0;
            Ng2Count = 0;
            Ng3Count = 0;
            Ng4Count = 0;
            Ng5Count = 0;
        }

        public int OkCount
        {
            get { return _okCount; }
            set
            {
                _okCount = value; 
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(YieldText));
                OnPropertyChanged(nameof(Uph));
            }
        }

        public int Ng2Count
        {
            get { return _ng2Count; }
            set
            {
                _ng2Count = value;
                OnPropertyChanged(nameof(NgCount));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(YieldText));
                OnPropertyChanged(nameof(Uph));
            }
        }

        public int Ng3Count
        {
            get { return _ng3Count; }
            set
            {
                _ng3Count = value;
                OnPropertyChanged(nameof(NgCount));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(YieldText));
                OnPropertyChanged(nameof(Uph));
            }
        }

        public int Ng4Count
        {
            get { return _ng4Count; }
            set
            {
                _ng4Count = value;
                OnPropertyChanged(nameof(NgCount));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(YieldText));
                OnPropertyChanged(nameof(Uph));
            }
        }

        public int Ng5Count
        {
            get { return _ng5Count; }
            set
            {
                _ng5Count = value;
                OnPropertyChanged(nameof(NgCount));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(YieldText));
                OnPropertyChanged(nameof(Uph));
            }
        }

        public int TotalCount
        {
            get { return NgCount + OkCount; }
        }

        public int EmptyCount { get; set; }
        public int NgCount
        {
            get { return Ng2Count + Ng3Count + Ng4Count + Ng5Count; }
        }

        public string YieldText
        {
            get { return (OkCount / (double) TotalCount * 100).ToString("N1") + "%"; }
        }


        public DateTime StartTime { get; set; }
        public double Uph
        {
            get { return TotalCount / (DateTime.Now - StartTime).TotalHours; }
        }

        public ICommand ClearCommand { get; set; }

    }
}