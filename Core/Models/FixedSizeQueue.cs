using System.Collections.Generic;

namespace Core.Models
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        private int _maxSize;

        public FixedSizeQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public new void Enqueue(T ele)
        {
          
                base.Enqueue(ele);
                if (Count > _maxSize)
                {
                    Dequeue();
                }
        }

    }
}