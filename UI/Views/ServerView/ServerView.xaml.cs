using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Plc;

namespace UI.Views.ServerView
{
    public partial class ServerView : UserControl
    {
        public ServerView()
        {
            InitializeComponent();
           
        }

        internal void MessageListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MessageList.Count <= MaxMessageCount) return;
            // Shrink size if message list overflows
            var numToSkip = (int) (MaxMessageCount * 0.3);
            MessageList = new ObservableCollection<LoggingMessageItem>(MessageList.Skip(numToSkip));
        }

        public static readonly DependencyProperty MessageListProperty = DependencyProperty.Register(
            "MessageList", typeof(ObservableCollection<LoggingMessageItem>), typeof(ServerView), new PropertyMetadata(default(ObservableCollection<LoggingMessageItem>), OnMessageListBindingChanged));

        private static void OnMessageListBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var serverView = (ServerView) d;
            var messageList = serverView.MessageList;
            messageList.CollectionChanged += serverView.MessageListOnCollectionChanged;
        }

        public ObservableCollection<LoggingMessageItem> MessageList
        {
            get { return (ObservableCollection<LoggingMessageItem>) GetValue(MessageListProperty); }
            set { SetValue(MessageListProperty, value); }
        }

        public static readonly DependencyProperty MaxMessageCountProperty = DependencyProperty.Register(
            "MaxMessageCount", typeof(int), typeof(ServerView), new PropertyMetadata(100));

        public int MaxMessageCount
        {
            get { return (int) GetValue(MaxMessageCountProperty); }
            set { SetValue(MaxMessageCountProperty, value); }
        }
        
    }
}