using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class ThreadSafeFixedSizeQueue<T> : Queue<T>
    {
        private object _syncObject = new object();
        private int _maxSize;

        public ThreadSafeFixedSizeQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void EnqueueThreadSafe(T ele)
        {
            lock (_syncObject)
            {
                Enqueue(ele);
                if (Count > _maxSize)
                {
                    Dequeue();
                }
            }
        }

        public T DequeueThreadSafe()
        {
            T output;
            lock (_syncObject)
            {
                output = Dequeue();
            }

            return output;
        }
        
    }
}