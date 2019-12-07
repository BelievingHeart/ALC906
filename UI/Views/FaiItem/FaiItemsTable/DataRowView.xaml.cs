using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Enums;
using Core.ViewModels.Fai;

namespace UI.Views.FaiItem.FaiItemsTable
{
    public partial class DataRowView : UserControl
    {
        public DataRowView()
        {
            InitializeComponent();
            DataContextChanged += OnViewModelChanged;
        }
        

        private void OnViewModelChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DataRowViewModel) e.NewValue;
            if (viewModel == null) return;
            var me = (DataRowView)sender;
            
            // Clear elements if the grid has elements before
            if (me.PART_ValueCells.Children.Count > 0)
            {
                me.PART_ValueCells.Children.Clear();
            }

            var rowName = viewModel.RowName;
            var items = viewModel.Values;

            // Add RowName
            me.PART_LevelIndicator.Background = GetIndicatorColor(viewModel.ProductLevel);
            me.PART_NameText.Text = rowName;
            
            // Add Values
            for (int i = 0; i < items.Count; i++)
            {
                var valueColumnDef = new ColumnDefinition{Width = new GridLength(me.DataCellWidth)};
                me.PART_ValueCells.ColumnDefinitions.Add(valueColumnDef);
                var faiItem = items[i];
                var valueBlock = new TextBlock{Text = faiItem.Value.ToString(me.ValueFormat), HorizontalAlignment = HorizontalAlignment.Center};
                valueBlock.Foreground = GetCellForeground(faiItem);
                Grid.SetColumn(valueBlock, i);
                me.PART_ValueCells.Children.Add(valueBlock);
            }

        }

        private static Brush GetCellForeground(Core.ViewModels.Fai.FaiItem faiItem)
        {
            if(faiItem.TooLarge) return new SolidColorBrush(Colors.Red);
            if(faiItem.TooSmall) return new SolidColorBrush(Colors.DodgerBlue);

            return new SolidColorBrush(Colors.Black);
        }

        private static SolidColorBrush GetIndicatorColor(ProductLevel productLevel)
        {
            switch (productLevel)
            {
                case ProductLevel.Ok:
                    return new SolidColorBrush(Colors.Chartreuse);
                case ProductLevel.Empty:
                    return new SolidColorBrush(Colors.Aqua);
            }
            
            return new SolidColorBrush(Colors.Red);
        }


        #region Value Format

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            "ValueFormat", typeof(string), typeof(DataRowView), new PropertyMetadata("N3"));

        public string ValueFormat
        {
            get { return (string) GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        #endregion


        #region Data Cell Width

        public static readonly DependencyProperty DataCellWidthProperty = DependencyProperty.Register(
            "DataCellWidth", typeof(double), typeof(DataRowView), new PropertyMetadata(default(double), OnDataCellWidthChanged));

        private static void OnDataCellWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (DataRowView) d;
            var width = (double) e.NewValue;
            foreach (var columnDefinition in sender.PART_ValueCells.ColumnDefinitions)
            {
                columnDefinition.Width = new GridLength(width);
            }
        }


        public double DataCellWidth
        {
            get { return (double) GetValue(DataCellWidthProperty); }
            set { SetValue(DataCellWidthProperty, value); }
        }
        
        #endregion


        #region RowNameWith

        public static readonly DependencyProperty RowNameWithProperty = DependencyProperty.Register(
            "RowNameWith", typeof(double), typeof(DataRowView), new PropertyMetadata(100.0, OnRowNameWithChanged));

        private static void OnRowNameWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (DataRowView) d;
            var width = (double) e.NewValue;
            sender.PART_NameCell.Width = width;
        }

        public double RowNameWith
        {
            get { return (double) GetValue(RowNameWithProperty); }
            set { SetValue(RowNameWithProperty, value); }
        }
        #endregion
    }
}