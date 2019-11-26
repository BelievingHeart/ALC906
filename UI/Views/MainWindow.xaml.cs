using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Core.ViewModels.Application;
using Core.ViewModels.MainWindow;
using Core.ViewModels.Plc;

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

            var sideBarMessages = SideBarLoggingBox.ItemsSource as ObservableCollection<LoggingMessageItem>;
            sideBarMessages.CollectionChanged += ScrollToBottom;
        }

        private void ScrollToBottom(object sender, NotifyCollectionChangedEventArgs e)
        {
            var me = sender as ObservableCollection<LoggingMessageItem>;
            if (me.Count == 0) return;
            Dispatcher.Invoke(() =>
            {
                SideBarLoggingBox.SelectedIndex = me.Count - 1;
                SideBarLoggingBox.ScrollIntoView(SideBarLoggingBox.SelectedItem);
            });
        }
    }
}