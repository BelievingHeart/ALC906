using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
            Dispatcher.Invoke(() =>
            {
                var sideBarMessages = SideBarLoggingBox.ItemsSource as ObservableCollection<LoggingMessageItem>;

                if (sideBarMessages.Count == 0) return;
                // Scroll to the last item
                SideBarLoggingBox.SelectedIndex = sideBarMessages.Count - 1;
                SideBarLoggingBox.ScrollIntoView(SideBarLoggingBox.SelectedItem);
            });
        }

        public int MaxMessageCount { get; set; } = 10;
    }
}