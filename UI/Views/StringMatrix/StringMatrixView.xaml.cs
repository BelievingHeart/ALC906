using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.StringMatrix
{
    public partial class StringMatrixView : UserControl
    {
        public StringMatrixView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StringMatrixProperty = DependencyProperty.Register(
            "StringMatrix", typeof(List<List<string>>), typeof(StringMatrixView),
            new PropertyMetadata(default(List<List<string>>), OnStringMatrixChanged));

        public List<List<string>> StringMatrix
        {
            get { return (List<List<string>>) GetValue(StringMatrixProperty); }
            set { SetValue(StringMatrixProperty, value); }
        }

        private static void OnStringMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (StringMatrixView) d;
            var newValue = (List<List<string>>) e.NewValue;
            if (e.NewValue == null) return;

            sender.UpdateGridContents(newValue);
        }

        private void UpdateGridContents(List<List<string>> stringMatrix)
        {
            // Check for alignment
            var countOfFirstList = stringMatrix[0].Count;
            Trace.Assert(stringMatrix.All(list => list.Count == countOfFirstList));
            var gridHeight = stringMatrix.Count + 1;


            if (!GridHasContent)
            {
                // Create new grid
                for (int i = 0; i < gridHeight; i++)
                {
                    StringGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
                }

                for (int i = 0; i < countOfFirstList; i++)
                {
                    StringGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = CellWidth});
                }
            }


            for (int row = 1; row < gridHeight; row++)
            {
                for (int col = 0; col < countOfFirstList; col++)
                {
                    var text = stringMatrix[row - 1][col];
                    var textBox = new TextBox() {Text = text, HorizontalContentAlignment = HorizontalAlignment.Center};
                    Grid.SetRow(textBox, row);
                    Grid.SetColumn(textBox, col);
                    StringGrid.Children.Add(textBox);
                }
            }

            // Add header if header exists
            if (Header == null) return;
            Trace.Assert(Header.Count == countOfFirstList);
            for (int col = 0; col < countOfFirstList; col++)
            {
                var text = Header[col];
                var textBlock = new Button() {Content = text, HorizontalContentAlignment = HorizontalAlignment.Center};
                Grid.SetRow(textBlock, 0);
                Grid.SetColumn(textBlock, col);
                StringGrid.Children.Add(textBlock);

            }
        }

        public bool GridHasContent => StringGrid.RowDefinitions.Count > 0;

        
        public static readonly DependencyProperty CellWidthProperty = DependencyProperty.Register(
            "CellWidth", typeof(GridLength), typeof(StringMatrixView), new PropertyMetadata(new GridLength(100)));

        public GridLength CellWidth
        {
            get { return (GridLength) GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        
        

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(List<string>), typeof(StringMatrixView), new PropertyMetadata(default(List<string>), OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (StringMatrixView) d;
            var newValue = (List<string>) e.NewValue;
            if (newValue == null) return;
            
            if(sender.StringMatrix != null)
            {
                Trace.Assert(newValue.Count == sender.StringMatrix[0].Count);
                for (int col = 0; col < newValue.Count; col++)
                {
                    var text = newValue[col];
                    var textBlock = new Button() {Content = text, HorizontalContentAlignment = HorizontalAlignment.Center};
                    Grid.SetRow(textBlock, 0);
                    Grid.SetColumn(textBlock, col);
                    sender.StringGrid.Children.Add(textBlock);
                }
                
            }
            
            
        }

        public List<string> Header
        {
            get { return (List<string>) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        
        private List<List<string>> ParseContent()
        {
            if (StringMatrix == null) return null;
            
            var numColumns = StringGrid.ColumnDefinitions.Count;
            var numRows = StringGrid.RowDefinitions.Count;
            if (numColumns == 0 || numRows == 0) return null;

            var output = new List<List<string>>();
            for (int row = 0; row < numRows; row++)
            {
                var elements = new List<UIElement>();
                for (int i = row*numColumns; i < row*numColumns + numColumns; i++)
                {
                    elements.Add(StringGrid.Children[i]);
                }

                var isHeaderRow = elements.All(ele => ele is Button);
                if(!isHeaderRow) output.Add(elements.Cast<TextBox>().Select(box=>box.Text).ToList());
            }

            return output;
        }
    }
}