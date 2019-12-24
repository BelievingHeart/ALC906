using System;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseQuery.Views.Table
{
    public partial class FaiCollectionHeaderRowView : UserControl
    {
        public FaiCollectionHeaderRowView()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty CollectionTypeProperty = DependencyProperty.Register(
            "CollectionType", typeof(Type), typeof(FaiCollectionHeaderRowView), new PropertyMetadata(default(Type), OnCollectionTypeChanged));

        private static void OnCollectionTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FaiCollectionHeaderRowView) d;
            var collectionType = (Type)e.NewValue;
            if (collectionType == null) return;
            
            // Add date block
            view.PART_StackPanel.Children.Add(new TextBlock()
                {Width = view.DateBlockWidth, Text = "Inspection time",TextAlignment = TextAlignment.Center});
            
            // Add result block
            view.PART_StackPanel.Children.Add(new TextBlock()
                {Width = view.ValueBlockWidth, Text = "Result", TextAlignment = TextAlignment.Center});
            
            // Add cavity block
            view.PART_StackPanel.Children.Add(new TextBlock()
                {Width = view.ValueBlockWidth, Text = "Cavity", TextAlignment = TextAlignment.Center});

            // Add other blocks
            foreach (var property in collectionType.GetProperties())
            {
                if(!property.Name.Contains("FAI")) continue;
                view.PART_StackPanel.Children.Add(new TextBlock()
                    {Width = view.ValueBlockWidth, Text = property.Name, TextAlignment = TextAlignment.Center});
            }
        }

        public Type CollectionType
        {
            get { return (Type) GetValue(CollectionTypeProperty); }
            set { SetValue(CollectionTypeProperty, value); }
        }
        
        #region DateBlockWidthProperty

        public static readonly DependencyProperty DateBlockWidthProperty = DependencyProperty.Register(
            "DateBlockWidth", typeof(double), typeof(FaiCollectionHeaderRowView), new PropertyMetadata(200.0));

        public double DateBlockWidth
        {
            get { return (double) GetValue(DateBlockWidthProperty); }
            set { SetValue(DateBlockWidthProperty, value); }
        }

        #endregion

        #region ValueBlockWidthProperty

        public static readonly DependencyProperty ValueBlockWidthProperty = DependencyProperty.Register(
            "ValueBlockWidth", typeof(double), typeof(FaiCollectionHeaderRowView), new PropertyMetadata(100.0));

        public double ValueBlockWidth
        {
            get { return (double) GetValue(ValueBlockWidthProperty); }
            set { SetValue(ValueBlockWidthProperty, value); }
        }

        #endregion
    }
}