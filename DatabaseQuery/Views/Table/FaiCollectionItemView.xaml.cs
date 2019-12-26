using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Constants;
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
           
            var viewModel = (FaiCollectionItemViewModel) e.NewValue;
            var collection = viewModel.FaiCollection;
            if (collection == null) return;

            // Add date block
            PART_Grid.ColumnDefinitions.Add(new ColumnDefinition(){Width = new GridLength(DateBlockWidth)});
            var inspectionTimeBlock = new TextBlock()
            {
                Width = DateBlockWidth, Text = collection.InspectionTime.ToString(NameConstants.DateTimeFormat),
                TextAlignment = TextAlignment.Center
                , HorizontalAlignment = HorizontalAlignment.Center};
            Grid.SetColumn(inspectionTimeBlock, 0);
            PART_Grid.Children.Add(inspectionTimeBlock);
            

            // Add result block
            PART_Grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(ValueBlockWidth)});
            var resultBlock = new TextBlock()
                {Width = ValueBlockWidth, Text = collection.Result, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
            // Highlight ng row
            if(collection.Result == "NG")
            {
                resultBlock.Foreground = Brushes.White;
                resultBlock.Background = Brushes.Red;
            }
            Grid.SetColumn(resultBlock, 1);
            PART_Grid.Children.Add(resultBlock);
            
            // Add cavity block
            PART_Grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(ValueBlockWidth)});
            var cavityBlock = new TextBlock()
            {
                Width = ValueBlockWidth, Text = collection.Cavity.ToString(), TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(cavityBlock, 2);
            PART_Grid.Children.Add(cavityBlock);

            // Add other blocks
            var collectionType = collection.GetType();
            var props = collectionType.GetProperties();
            var resultTooltipText = string.Empty;
            for (var index = 0; index < props.Length; index++)
            {
                var property = props[index];
                var propName = property.Name;
                if (!propName.Contains("FAI")) continue;
                var value = (double) property.GetValue(collection);
                var max = viewModel.DictionaryUpper[propName];
                var min = viewModel.DictionaryLower[propName];
                var faiBlock = new TextBlock()
                {
                    Width = ValueBlockWidth, Text = value.ToString("F3"), TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center, ToolTip = $"({min:F3}-{max:F3})"
                };
                // Colorize ng cells
                if(value > max) faiBlock.Foreground = new SolidColorBrush(ColorConstants.ExceedUpperColor); 
                if(value < min) faiBlock.Foreground = new SolidColorBrush(ColorConstants.ExceedLowerColor);
                if (value < min || value > max) resultTooltipText += $"{propName}, ";
                PART_Grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(ValueBlockWidth)});
                Grid.SetColumn(faiBlock, index+3);
                PART_Grid.Children.Add(faiBlock);
            }
            resultBlock.ToolTip = resultTooltipText;
        }
        

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