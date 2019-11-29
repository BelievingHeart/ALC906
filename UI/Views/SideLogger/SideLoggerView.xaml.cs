using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Application;
using Core.ViewModels.Plc;

namespace UI.Views.SideLogger
{
    public partial class SideLoggerView : UserControl
    {
        public SideLoggerView()
        {
            InitializeComponent();
        }
        
        private void OnLoggerLoaded(object sender, RoutedEventArgs e)
        {
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
    }
}