using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Plc;

namespace UI.Views.Server
{
    public partial class AutoScrollMessageBox : UserControl
    {
        public AutoScrollMessageBox()
        {
            InitializeComponent();
           
        }

        private void MessageListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (MessageList.Count == 0) return;
                // Scroll to the last item
                MessageListBox.SelectedIndex = MessageList.Count - 1;
                MessageListBox.ScrollIntoView(MessageListBox.SelectedItem);
            });
        }


        public static readonly DependencyProperty MessageListProperty = DependencyProperty.Register(
            "MessageList", typeof(ObservableCollection<LoggingMessageItem>), typeof(AutoScrollMessageBox), new PropertyMetadata(default(ObservableCollection<LoggingMessageItem>), OnMessageListBindingChanged));

        public ObservableCollection<LoggingMessageItem> MessageList
        {
            get
            {
                return  Dispatcher.Invoke(() => (ObservableCollection<LoggingMessageItem>) GetValue(MessageListProperty));
            }
            set { Dispatcher.Invoke(() => SetValue(MessageListProperty, value)); }
        }
        
        private static void OnMessageListBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var serverView = (AutoScrollMessageBox) d;
            serverView.Dispatcher.Invoke(() =>
            {
                var newList = (ObservableCollection<LoggingMessageItem>) e.NewValue;
                var oldList = (ObservableCollection<LoggingMessageItem>) e.OldValue;

                if (newList != null)
                {
                    newList.CollectionChanged += serverView.MessageListOnCollectionChanged;
                }

                if (oldList != null)
                {
                    oldList.CollectionChanged -= serverView.MessageListOnCollectionChanged;
                }
            });
        }

  

  
 

        private void OnMessageBoxLoaded(object sender, RoutedEventArgs e)
        {
            var messages = MessageListBox.ItemsSource as ObservableCollection<LoggingMessageItem>;
            messages.CollectionChanged += MessageListOnCollectionChanged;
        }
        
    }
}