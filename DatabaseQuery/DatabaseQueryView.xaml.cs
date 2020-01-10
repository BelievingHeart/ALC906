using System;
using Core.ViewModels.Database;

namespace DatabaseQuery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DatabaseQueryView
    {
        public DatabaseQueryView()
        {
            InitializeComponent();
            Closed += Cleanup;
        }

        private void Cleanup(object sender, EventArgs e)
        {
            var viewModel = (DatabaseQueryViewModel) DataContext;
            viewModel.DeleteOutdatedCollections();
        }
    }
}