using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Message
{
    public class FixedSizeMessageList : ViewModelBase
    {
        private readonly string _serializationDir;
        private readonly string _fileName;
        private List<LoggingMessageItem> _messageList = new List<LoggingMessageItem>();
        public event Action<LoggingMessageItem> NewMessagePushed;
        public event Action<int> MessageRemoved;
        public double PercentToRemove { get; set; } = 0.3;
        public bool ShouldSerialize { get; set; } = true;
        

        public void LogAsync(string message)
        {
            System.Windows.Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                var messageItem = LoggingMessageItem.CreateMessage(message);
                _messageList.Add(messageItem);
                OnNewMessagePushed(messageItem);
            
                if(_messageList.Count > MaxCount)
                {
                    var numToRemove = (int) (MaxCount * PercentToRemove);
                    _messageList = _messageList.Skip(numToRemove).ToList();
                    OnMessageRemoved(numToRemove);
                }
                OnPropertyChanged(nameof(Count));
                
               if(ShouldSerialize) messageItem.WriteLineToFile(_serializationDir, _fileName);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializationDir">Dir to log</param>
        /// <param name="fileName">File name of the log</param>
        public FixedSizeMessageList(string serializationDir, string fileName)
        {
            _serializationDir = serializationDir;
            _fileName = fileName;
            var path = Path.Combine(_serializationDir, _fileName);
            if(File.Exists(path)) File.Delete(path);
        }

        public int MaxCount { get; set; } = 100;

        public int Count
        {
            get { return _messageList.Count; }
        }

        protected virtual void OnNewMessagePushed(LoggingMessageItem obj)
        {
            NewMessagePushed?.Invoke(obj);
        }

        protected virtual void OnMessageRemoved(int obj)
        {
             MessageRemoved?.Invoke(obj);
        }
        
    }
}