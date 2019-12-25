using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModels.Database;
using Core.ViewModels.Database.FaiCollection;

namespace DatabaseQuery.Views.Table
{
    public partial class FaiCollectionListView : UserControl
    {
        public FaiCollectionListView()
        {
            InitializeComponent();
        }



        #region FaiCollectionProperty

        public static readonly DependencyProperty FaiCollectionItemListProperty = DependencyProperty.Register(
            "FaiCollectionItemList", typeof(IList<FaiCollectionItemViewModel>), typeof(FaiCollectionListView), new PropertyMetadata(default(IList<FaiCollectionItemViewModel>), OnFaiCollectionChanged));

        private static void OnFaiCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FaiCollectionListView) d;
            var collectionList = (IList<FaiCollectionItemViewModel>) e.NewValue;
            if (e.NewValue == null || collectionList.Count == 0) return;

            // Define header row
            view.PART_HeaderRow.CollectionType = collectionList[0].FaiCollection.GetType();
            view.PART_HeaderRow.DateBlockWidth = view.DateBlockWidth;
            view.PART_HeaderRow.ValueBlockWidth = view.ValueBlockWidth;

            // Add collections
            view.PART_ListBox.ItemsSource = collectionList;
        }

        public IList<FaiCollectionItemViewModel> FaiCollectionItemList
        {
            get { return (IList<FaiCollectionItemViewModel>) GetValue(FaiCollectionItemListProperty); }
            set { SetValue(FaiCollectionItemListProperty, value); }
        }

        #endregion
        
        #region DateBlockWidthProperty

        public static readonly DependencyProperty DateBlockWidthProperty = DependencyProperty.Register(
            "DateBlockWidth", typeof(double), typeof(FaiCollectionListView), new PropertyMetadata(200.0));

        public double DateBlockWidth
        {
            get { return (double) GetValue(DateBlockWidthProperty); }
            set { SetValue(DateBlockWidthProperty, value); }
        }

        #endregion

        #region ValueBlockWidthProperty

        public static readonly DependencyProperty ValueBlockWidthProperty = DependencyProperty.Register(
            "ValueBlockWidth", typeof(double), typeof(FaiCollectionListView), new PropertyMetadata(100.0));

        public double ValueBlockWidth
        {
            get { return (double) GetValue(ValueBlockWidthProperty); }
            set { SetValue(ValueBlockWidthProperty, value); }
        }

        #endregion

        private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;        }
        
        

        #region SelectionCountProperty

        public static readonly DependencyProperty SelectionCountProperty = DependencyProperty.Register(
            "SelectionCount", typeof(int), typeof(FaiCollectionListView), new PropertyMetadata(default(int)));

        public int SelectionCount
        {
            get { return (int) GetValue(SelectionCountProperty); }
            set { SetValue(SelectionCountProperty, value); }
        }

        #endregion

        #region SelectedCollectionsProperty

        public static readonly DependencyProperty SelectedCollectionsProperty = DependencyProperty.Register(
            "SelectedCollections", typeof(IList<IFaiCollection>), typeof(FaiCollectionListView), new PropertyMetadata(default(IList<IFaiCollection>)));

        public IList<IFaiCollection> SelectedCollections
        {
            get { return (IList<IFaiCollection>) GetValue(SelectedCollectionsProperty); }
            set { SetValue(SelectedCollectionsProperty, value); }
        }

        #endregion

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCollections = PART_ListBox.SelectedItems.Cast<FaiCollectionItemViewModel>()
                .Select(ele => ele.FaiCollection).ToList();
            
            SelectionCount = PART_ListBox.SelectedItems.Count;
        }
    }
}