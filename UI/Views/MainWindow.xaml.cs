using System;
using System.ComponentModel;
using System.Windows;
using Core.ViewModels.Application;
using Core.ViewModels.MainWindow;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void CleanupApplication(object sender, EventArgs eventArgs)
        {
            //TODO: uncomment this line
            ApplicationViewModel.Cleanup();
        }


        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.InitHardWares();
        }
    }
}