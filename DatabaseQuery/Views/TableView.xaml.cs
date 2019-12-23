using System.Windows;
using System.Windows.Controls;

namespace DatabaseQuery.Views
{
    public partial class TableView : UserControl
    {
        public TableView()
        {
            InitializeComponent();
        }

        private void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_SelectedRowsTextBlock.Text = PART_DataGrid.SelectedItems.Count.ToString();
        }
        
    }
}