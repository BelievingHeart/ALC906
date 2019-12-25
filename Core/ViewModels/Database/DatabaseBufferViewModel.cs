using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Core.ViewModels.Database.FaiCollection;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database
{
    /// <summary>
    /// Represent a buffer that pull from database for local calculations
    /// </summary>
    public class DatabaseBufferViewModel : ViewModelBase
    {
        #region private memebers

        private IList<IFaiCollection> _faiCollectionBuffers;
        private int _currentPageIndex;
        private IList<IFaiCollection> _tableToShow;
        private int _rowsPerPage = 20;
        private IList<IFaiCollection> _collectionsToShow;

        #endregion

        public event Action CollectionToShowChanged;


        #region prop

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        /// <summary>
        /// The max timespan among all FaiCollectionBuffers in days
        /// </summary>
        public double TotalDays { get; set; }
        /// <summary>
        /// The max timespan among all FaiCollectionBuffers in hours
        /// </summary>
        public double TotalHours { get; set; }

        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        
        public IList<IFaiCollection> FaiCollectionBuffers
        {
            get { return _faiCollectionBuffers; }
            set
            {
                _faiCollectionBuffers = value;
                OnPropertyChanged(nameof(CollectionCount));
                GenerateTotalYield();
                GenerateTotalDayAndTotalHours();
                CalculateTotalPages();
            }
        }

  


        public int RowsPerPage
        {
            get { return _rowsPerPage; }
            set
            {
                _rowsPerPage = value;
                CalculateTotalPages();
                NavigateToPage(0);
            }
        }

        public int TotalPages { get; set; }

        public int CurrentPageIndex
        {
            get { return _currentPageIndex; }
            set
            {
                _currentPageIndex = value;
                NavigateToPage(value);
            }
        }

        public IList<IFaiCollection> CollectionsToShow
        {
            get { return _collectionsToShow; }
            set
            {
                _collectionsToShow = value;
                OnCollectionToShowChanged();
            }
        }

        public int CollectionCount
        {
            get { return FaiCollectionBuffers?.Count ?? 0; }
        }

        public string TotalYield { get; private set; }


        #endregion


        #region impl
        
        private void GenerateTotalDayAndTotalHours()
        {
            if (FaiCollectionBuffers == null || FaiCollectionBuffers.Count == 0)
            {
                TotalDays = 0;
                TotalHours = 0;
                return;
            }
            
            var dates = FaiCollectionBuffers.Select(c => c.InspectionTime).ToArray();
             MaxDate = dates.Max();
             MinDate = dates.Min();
            var timeSpan = MaxDate - MinDate;
            TotalDays = timeSpan.TotalDays;
            TotalHours = timeSpan.TotalHours;
        }

    

        private void GenerateTotalYield()
        {
            var yield = FaiCollectionBuffers.Count(collection => collection.Result == "OK") / (double) CollectionCount;
            TotalYield = $"{yield * 100 :F1}%";
        }


        public void NavigateToPage(int pageIndex)
        {
            CurrentPageIndex = pageIndex;
            if (FaiCollectionBuffers == null || FaiCollectionBuffers.Count == 0)
            {
                CollectionsToShow = new List<IFaiCollection>();
                return;
            }
            // If it is the first page ...
            if (pageIndex == 0)
                CollectionsToShow = FaiCollectionBuffers
                    .Take(RowsPerPage > FaiCollectionBuffers.Count ? FaiCollectionBuffers.Count : RowsPerPage)
                    .ToList();
            // If it is the last page ...
            else if (pageIndex == TotalPages - 1)
            {
                var numRowsToShow = FaiCollectionBuffers.Count % RowsPerPage == 0
                    ? RowsPerPage
                    : FaiCollectionBuffers.Count % RowsPerPage;
                CollectionsToShow = FaiCollectionBuffers.Skip(pageIndex * RowsPerPage).Take(numRowsToShow).ToList();
            }
            else
            {
                CollectionsToShow = FaiCollectionBuffers.Skip(pageIndex * RowsPerPage).Take(RowsPerPage).ToList();
            }
        }

        private void CalculateTotalPages()
        {
            if (FaiCollectionBuffers == null)
            {
                TotalPages = 0;
                return;
            }
            
            if (FaiCollectionBuffers.Count < RowsPerPage) {
                TotalPages = 1;
                return;
            }

            TotalPages = (int) Math.Ceiling(FaiCollectionBuffers.Count / (decimal) RowsPerPage);
        }

        #endregion

        #region ctor

        public DatabaseBufferViewModel()
        {
            NextPageCommand = new SimpleCommand(o=> NavigateToPage(CurrentPageIndex+1), o=>TotalPages>0&&CurrentPageIndex<TotalPages-1);
            PreviousPageCommand = new SimpleCommand(o=> NavigateToPage(CurrentPageIndex-1), o=>CurrentPageIndex>0);
        }

        #endregion

        /// <summary>
        /// Remove a list of collection from FaiCollectionBuffers
        /// </summary>
        /// <param name="selectedCollections"></param>
        public void Remove(IList<IFaiCollection> selectedCollections)
        {
            foreach (var collection in selectedCollections)
            {
                FaiCollectionBuffers.Remove(collection);
            }
            OnPropertyChanged(nameof(CollectionCount));
            GenerateTotalYield();
            GenerateTotalDayAndTotalHours();
            CalculateTotalPages();
        }

        protected virtual void OnCollectionToShowChanged()
        {
            CollectionToShowChanged?.Invoke();
        }
    }
}