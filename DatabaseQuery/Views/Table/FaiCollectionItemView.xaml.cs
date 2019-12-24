using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Database.FaiCollection;

namespace DatabaseQuery.Views.Table
{
    public partial class FaiCollectionItemView : UserControl
    {
        public FaiCollectionItemView()
        {
            InitializeComponent();
            DataContextChanged += OnFaiCollectionChanged;
        }

        private void OnFaiCollectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
            var collection = (IFaiCollection) e.NewValue;
            if (collection == null) return;

            // Add date block
            PART_StackPanel.Children.Add(new TextBlock()
                {Width = DateBlockWidth, Text = collection.InspectionTime.ToString("G"),TextAlignment = TextAlignment.Center});
            
            // Add result block
            PART_StackPanel.Children.Add(new TextBlock()
                {Width = ValueBlockWidth, Text = collection.Result, TextAlignment = TextAlignment.Center});
            
            // Add cavity block
            PART_StackPanel.Children.Add(new TextBlock()
                {Width = ValueBlockWidth, Text = collection.Cavity.ToString(), TextAlignment = TextAlignment.Center});

            // Add other blocks
            var collectionType = collection.GetType();
            foreach (var property in collectionType.GetProperties())
            {
                if(!property.Name.Contains("FAI")) continue;
                var value = (double)property.GetValue(collection);
                PART_StackPanel.Children.Add(new TextBlock()
                    {Width = ValueBlockWidth, Text = value.ToString("F4"), TextAlignment = TextAlignment.Center});
            }
        }

        #region FaiCollectionProperty

        public static readonly DependencyProperty FaiCollectionProperty = DependencyProperty.Register(
            "FaiCollection", typeof(IFaiCollection), typeof(FaiCollectionItemView), new PropertyMetadata(default(IFaiCollection), OnFaiCollectionChanged));

        private static void OnFaiCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
          // TODO delete this

        }

        public IFaiCollection FaiCollection
        {
            get { return (IFaiCollection) GetValue(FaiCollectionProperty); }
            set { SetValue(FaiCollectionProperty, value); }
        }

        #endregion

        #region DateBlockWidthProperty

        public static readonly DependencyProperty DateBlockWidthProperty = DependencyProperty.Register(
            "DateBlockWidth", typeof(double), typeof(FaiCollectionItemView), new PropertyMetadata(200.0));

        public double DateBlockWidth
        {
            get { return (double) GetValue(DateBlockWidthProperty); }
            set { SetValue(DateBlockWidthProperty, value); }
        }

        #endregion

        #region ValueBlockWidthProperty

        public static readonly DependencyProperty ValueBlockWidthProperty = DependencyProperty.Register(
            "ValueBlockWidth", typeof(double), typeof(FaiCollectionItemView), new PropertyMetadata(100.0));

        public double ValueBlockWidth
        {
            get { return (double) GetValue(ValueBlockWidthProperty); }
            set { SetValue(ValueBlockWidthProperty, value); }
        }

        #endregion
    }
}