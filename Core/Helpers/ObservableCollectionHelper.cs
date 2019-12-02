using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Core.ViewModels.Plc;

namespace Core.Helpers
{
    public static class ObservableCollectionHelper
    {
        /// <summary>
        /// If log failed, for example when CollectionChanged event is firing
        /// an <see cref="InvalidOperationException"/> is thrown and the message
        /// will be delay but not cancel
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="message"></param>
        /// <param name="locker"></param>
        /// <param name="timeRetry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void LogMessageRetryIfFailed(this ObservableCollection<LoggingMessageItem> collection, LoggingMessageItem message, object locker, int timeRetry)
        {
            string resendMarker = "--resend";
                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        lock (locker)
                        {
                            collection.Add(message);
                        }
                    });
                }
                catch
                {
                    Task.Run(()=>
                    {
                        Thread.Sleep(timeRetry);
                        // Mark it resent if it is resent
                        if (!message.Message.Contains(resendMarker)) message.Message += resendMarker;
                        
                        collection.LogMessageRetryIfFailed(message, locker, timeRetry);
                    });
                }
            
        }
    }
}