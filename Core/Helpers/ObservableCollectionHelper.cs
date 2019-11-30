using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

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
        public static void LogMessageRetryIfFailedAsync<T>(this ObservableCollection<T> collection, T message, object locker, int timeRetry)
        {
           
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
                        collection.LogMessageRetryIfFailedAsync(message, locker, timeRetry);
                    });
                }
            
        }
    }
}