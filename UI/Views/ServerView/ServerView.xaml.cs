using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Plc;

namespace UI.Views.ServerView
{
    public partial class AutoScrollMessageBox : UserControl
    {
        public AutoScrollMessageBox()
        {
            InitializeComponent();
           
        }

        private void MessageListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Shrink size if message list overflows
            if (MessageList.Count > MaxMessageCount)
            {
                var numToSkip = (int) (MaxMessageCount * 0.3);
                MessageList = new ObservableCollection<LoggingMessageItem>(MessageList.Skip(numToSkip));
            }

            if (MessageList.Count == 0) return;
            // Scroll to the last item
            MessageListBox.SelectedIndex = MessageList.Count - 1;
            MessageListBox.ScrollIntoView(MessageListBox.SelectedItem);
        }

        public static readonly DependencyProperty MessageListProperty = DependencyProperty.Register(
            "MessageList", typeof(ObservableCollection<LoggingMessageItem>), typeof(AutoScrollMessageBox), new PropertyMetadata(default(ObservableCollection<LoggingMessageItem>), OnMessageListBindingChanged));

        private static void OnMessageListBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var serverView = (AutoScrollMessageBox) d;
            var newList = (ObservableCollection<LoggingMessageItem>)e.NewValue;
            var oldList = (ObservableCollection<LoggingMessageItem>)e.OldValue;
            
            if (newList != null)
            {
                newList.CollectionChanged +=serverView.MessageListOnCollectionChanged;
            }

            if (oldList != null)
            {
                oldList.CollectionChanged -= serverView.MessageListOnCollectionChanged;
            }
        }

        public ObservableCollection<LoggingMessageItem> MessageList
        {
            get { return (ObservableCollection<LoggingMessageItem>) GetValue(MessageListProperty); }
            set { SetValue(MessageListProperty, value); }
        }

        public static readonly DependencyProperty MaxMessageCountProperty = DependencyProperty.Register(
            "MaxMessageCount", typeof(int), typeof(AutoScrollMessageBox), new PropertyMetadata(100));

        public int MaxMessageCount
        {
            get { return (int) GetValue(MaxMessageCountProperty); }
            set { SetValue(MaxMessageCountProperty, value); }
        }
        
    }
}