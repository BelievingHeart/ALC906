using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.FaiItem.FaiItemsTable
{
    public partial class HeaderRowView : UserControl
    {
        public HeaderRowView()
        {
            InitializeComponent();
        }


        #region TextData

        public static readonly DependencyProperty TextDataProperty = DependencyProperty.Register(
            "TextData", typeof(List<string>), typeof(HeaderRowView), new PropertyMetadata(default(List<string>), OnTextDataChanged));
        
        public List<string> TextData
        {
            get { return (List<string>) GetValue(TextDataProperty); }
            set { SetValue(TextDataProperty, value); }
        }
        
        private static void OnTextDataChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var me = (HeaderRowView) sender;
            
            var textData = (List<string>) e.NewValue;
            if (textData == null) return;
            
            // Clear elements if the grid has elements before
            if (me.PART_HeaderRow.Children.Count > 0)
            {
                me.PART_HeaderRow.ColumnDefinitions.Clear();
                me.PART_HeaderRow.Children.Clear();
            }

            // Add RowName
            me.PART_NameTextBlock.Text = me.RowName;
            
            // Add header items
            for (int i = 0; i < textData.Count; i++)
            {
                var valueColumnDef = new ColumnDefinition{Width = me.CellWidth};
                me.PART_HeaderRow.ColumnDefinitions.Add(valueColumnDef);
                var textBlock = new TextBlock{Text = textData[i], HorizontalAlignment = HorizontalAlignment.Center};
                Grid.SetColumn(textBlock, i);
                me.PART_HeaderRow.Children.Add(textBlock);
            }

        }


        #endregion



        #region Row Name

        public static readonly DependencyProperty RowNameProperty = DependencyProperty.Register(
            "RowName", typeof(string), typeof(HeaderRowView), new PropertyMetadata("Fai Items", OnRowNameChanged));

        private static void OnRowNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (HeaderRowView) d;
            var name = (string) e.NewValue;
            sender.PART_NameTextBlock.Text = name;
        }

        public string RowName
        {
            get { return (string) GetValue(RowNameProperty); }
            set { SetValue(RowNameProperty, value); }
        }

        #endregion


        #region Cell Width

        public static readonly DependencyProperty CellWidthProperty = DependencyProperty.Register(
            "CellWidth", typeof(GridLength), typeof(HeaderRowView), new PropertyMetadata(new GridLength(100)));

        public GridLength CellWidth
        {
            get { return (GridLength) GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        #endregion

        public static readonly DependencyProperty NameCellWidthProperty = DependencyProperty.Register(
            "NameCellWidth", typeof(double), typeof(HeaderRowView), new PropertyMetadata(100.0, OnNameCellWidthChanged));

        private static void OnNameCellWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (HeaderRowView) d;
            var width = (double) e.NewValue;
            sender.PART_NameCell.Width = width;
        }


        public double NameCellWidth
        {
            get { return (double) GetValue(NameCellWidthProperty); }
            set { SetValue(NameCellWidthProperty, value); }
        }
    }
}