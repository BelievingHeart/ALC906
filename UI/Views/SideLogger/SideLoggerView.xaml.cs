using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Core.Constants;
using Core.ViewModels.Application;
using Core.ViewModels.Message;

namespace UI.Views.SideLogger
{
    public partial class SideLoggerView : UserControl
    {
        public SideLoggerView()
        {
            InitializeComponent();
        }

  public static readonly DependencyProperty MessageListProperty = DependencyProperty.Register(
            "MessageList", typeof(FixedSizeMessageList), typeof(SideLoggerView),
            new PropertyMetadata(default(FixedSizeMessageList), OnMessageListChanged));

        private static void OnMessageListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (SideLoggerView) d;
            var newList = (FixedSizeMessageList) e.NewValue;

            if (newList == null) return;
            // Hook event handlers to new list
            sender.Hook(newList);

            // Unhook event handlers to new list
            var oldList = (FixedSizeMessageList) e.OldValue;
            if (oldList == null) return;
            sender.Unhook(oldList);
        }

        private void Hook(FixedSizeMessageList newList)
        {
            newList.NewMessagePushed += AddMessageItemView;
            newList.NewMessagePushed += ScrollToBottom;
            newList.MessageRemoved += RemoveViews;
        }

        private void Unhook(FixedSizeMessageList oldList)
        {
            oldList.NewMessagePushed -= AddMessageItemView;
            oldList.NewMessagePushed -= ScrollToBottom;
            oldList.MessageRemoved -= RemoveViews;
        }

        private void RemoveViews(int removeCount)
        {
            for (int i = 0; i < removeCount; i++)
            {
                PART_MessageListBox.Items.RemoveAt(i);
            }
        }

        private void AddMessageItemView(LoggingMessageItem messageItem)
        {
            PART_MessageListBox.Items.Add(messageItem);
        }

        public FixedSizeMessageList MessageList
        {
            get { return (FixedSizeMessageList) GetValue(MessageListProperty); }
            set { SetValue(MessageListProperty, value); }
        }

        private void ScrollToBottom(LoggingMessageItem loggingMessageItem)
        {
            if (MessageList.Count == 0) return;
            // Scroll to the last item
            PART_MessageListBox.SelectedIndex = MessageList.Count - 1;
            PART_MessageListBox.ScrollIntoView(PART_MessageListBox.SelectedItem);
        }
    }
}