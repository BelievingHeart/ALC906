using System.Windows.Controls;
using Core.ViewModels.Application;

namespace UI.Views.Bins
{
    public partial class SummaryView : UserControl
    {
        public SummaryView()
        {
            InitializeComponent();
        }


        private void OnSummarySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationViewModel.Instance.ReadSelectedSummary();
        }
    }
}