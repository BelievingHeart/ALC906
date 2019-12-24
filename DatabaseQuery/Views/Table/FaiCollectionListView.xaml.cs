using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

        public static readonly DependencyProperty FaiCollectionProperty = DependencyProperty.Register(
            "FaiCollection", typeof(IList<IFaiCollection>), typeof(FaiCollectionListView), new PropertyMetadata(default(IList<IFaiCollection>), OnFaiCollectionChanged));

        private static void OnFaiCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FaiCollectionListView) d;
            var collectionList = (IList<IFaiCollection>) e.NewValue;
            if (e.NewValue == null || collectionList.Count == 0) return;
            
            // Define header row
            view.PART_HeaderRow.CollectionType = collectionList[0].GetType();
            view.PART_HeaderRow.DateBlockWidth = view.DateBlockWidth;
            view.PART_HeaderRow.ValueBlockWidth = view.ValueBlockWidth;

            // Add collections
            view.PART_ListBox.ItemsSource = collectionList;
        }

        public IList<IFaiCollection> FaiCollection
        {
            get { return (IList<IFaiCollection>) GetValue(FaiCollectionProperty); }
            set { SetValue(FaiCollectionProperty, value); }
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
    }
}