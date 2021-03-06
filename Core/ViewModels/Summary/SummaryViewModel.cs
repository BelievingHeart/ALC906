﻿using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;
using System.Xml.Serialization;
using Core.ViewModels.Fai.FaiYieldCollection;
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
        private bool _shouldRefreshStartTime = true;

        public SummaryViewModel()
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(TotalCount)) return;
                // Update Start time if clear command has just called
                if (!_shouldRefreshStartTime) return;
                StartTime = DateTime.Now - TimeSpan.FromSeconds(10);
                _shouldRefreshStartTime = false;
            };
        }

        public void ClearSummary()
        {
            OkCount = 0;
            Ng2Count = 0;
            Ng3Count = 0;
            Ng4Count = 0;
            Ng5Count = 0;
            _shouldRefreshStartTime = true;
            
            // Clear yield collection
            FaiYieldCollectionViewModel?.Clear();
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
            get { return Ng2Count + Ng3Count + Ng4Count; }
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

        public FaiYieldCollectionViewModel FaiYieldCollectionViewModel { get; set; }




    }
}