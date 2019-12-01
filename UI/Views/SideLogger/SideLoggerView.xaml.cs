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



        public static readonly DependencyProperty MessageListProperty = DependencyProperty.Register(
            "MessageList", typeof(ObservableCollection<LoggingMessageItem>), typeof(SideLoggerView), new PropertyMetadata(default(ObservableCollection<LoggingMessageItem>), OnMessageListBindingChanged));

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
            var sender = (SideLoggerView) d;
            var newValue = (ObservableCollection<LoggingMessageItem>)e.NewValue;
            if (newValue == null) return;
            sender.MessageListBox.ItemsSource = newValue;
            
            sender.Dispatcher.Invoke(() =>
            {
                var oldList = (ObservableCollection<LoggingMessageItem>) e.OldValue;

                newValue.CollectionChanged += sender.ScrollToBottom;
                
                if (oldList != null)
                {
                    oldList.CollectionChanged -= sender.ScrollToBottom;
                }
            });
        }
        
        private void ScrollToBottom(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (MessageList.Count == 0) return;
                // Scroll to the last item
                MessageListBox.SelectedIndex = MessageList.Count - 1;
                MessageListBox.ScrollIntoView(MessageListBox.SelectedItem);
            });
        }

    }
}