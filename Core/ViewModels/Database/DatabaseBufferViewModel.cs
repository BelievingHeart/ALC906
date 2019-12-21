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

        #endregion



        #region prop

        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        
        public IList<IFaiCollection> FaiCollectionBuffers
        {
            get { return _faiCollectionBuffers; }
            set
            {
                _faiCollectionBuffers = value;
                CalculateTotalPages();
                CurrentPageIndex = 0;
                NavigateToPage(CurrentPageIndex);
            }
        }


        public int RowsPerPage
        {
            get { return _rowsPerPage; }
            set
            {
                _rowsPerPage = value;
                CalculateTotalPages();
                CurrentPageIndex = 0;
                NavigateToPage(CurrentPageIndex);
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

        public IList<IFaiCollection> TableToShow
        {
            get { return _tableToShow; }
            set { _tableToShow = value as FaiCollectionList ?? new FaiCollectionList(value); }
        }
        #endregion


        #region impl

        private void NavigateToPage(int pageIndex)
        {
            
            if (FaiCollectionBuffers == null || FaiCollectionBuffers.Count == 0) return;
            // If it is the first page ...
            if (pageIndex == 0)
                TableToShow = FaiCollectionBuffers
                    .Take(RowsPerPage > FaiCollectionBuffers.Count ? FaiCollectionBuffers.Count : RowsPerPage)
                    .ToList();
            // If it is the last page ...
            else if (pageIndex == TotalPages - 1)
            {
                var numRowsToShow = FaiCollectionBuffers.Count % RowsPerPage == 0
                    ? RowsPerPage
                    : FaiCollectionBuffers.Count % RowsPerPage;
                TableToShow = FaiCollectionBuffers.Skip(pageIndex * RowsPerPage).Take(numRowsToShow).ToList();
            }
            else
            {
                TableToShow = FaiCollectionBuffers.Skip(pageIndex * RowsPerPage).Take(RowsPerPage).ToList();
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
            NextPageCommand = new SimpleCommand(o=> CurrentPageIndex++, o=>TotalPages>0&&CurrentPageIndex<TotalPages-1);
            PreviousPageCommand = new SimpleCommand(o=> CurrentPageIndex--, o=>CurrentPageIndex>0);
        }

        #endregion
    }
}