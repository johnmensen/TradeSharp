using System;
using System.Collections;
using System.Collections.Generic;

namespace Entity
{
    /// <summary>
    /// очередь ограниченной длины с вытеснением
    /// при добавлении 
    /// </summary>    
    public class RestrictedQueue<T> : IEnumerator<T>, IEnumerable<T>
    {
        private int maxQueueLength = 10;
        public int MaxQueueLength
        {
            get { return maxQueueLength; }
            set { maxQueueLength = value; }
        }

        private int length;
        public int Length
        {
            get { return length; }
        }

        private RestrictedQueueContainer<T> first, last;
        private RestrictedQueueContainer<T> current;

        public T First
        {
            get
            {
                return first == null ? default(T) : first.item;
            }
        }
        public T Last
        {
            get
            {
                return last == null ? default(T) : last.item;
            }
        }

        public RestrictedQueueContainer<T> LastItem
        {
            get { return last; }
        }

        public RestrictedQueue(int maxLength)
        {
            maxQueueLength = maxLength;
        }

        public void Add(T item)
        {
            var newIt = new RestrictedQueueContainer<T>(item) { prev = null, next = null };
            if (length == 0 || maxQueueLength == 1)
            {
                first = newIt;
                last = newIt;
                if (length < maxQueueLength) length++;
                return;
            }

            if (length == 1)
            {
                first.next = newIt;
                newIt.prev = first;
                last = newIt;
                length++;
                return;
            }

            if (length == maxQueueLength)
            {
                first.next.prev = null;
                first = first.next;
            }
            else length++;

            last.next = newIt;
            newIt.prev = last;
            last = newIt;
        }

        public void Clear()
        {
            length = 0;
            first = null;
            last = null;
        }

        public T DequeueLast()
        {            
            if (last == null) return default(T);
            if (length == 1)
            {
                var item = last.item;
                last = null;
                first = null;
                current = null;
                length = 0;
                return item;
            }

            var lastValue = last.item;
            var prevLast = last.prev;
            
            prevLast.next = null;
            last = prevLast;
            length--;
            return lastValue;
        }

        /// <summary>
        /// отрицательный индекс отсчитывается назад от last
        /// </summary>        
        public T GetItemByIndex(int i, bool fromBegin)
        {
            if (i > length) throw new IndexOutOfRangeException(string.Format("index ({0}) is gt than length ({1})", i, length));
            var cur = last;

            if (!fromBegin)
            {
                if (last == null) throw new IndexOutOfRangeException(string.Format("index is {0}, last is null", i));                                
                for (var iter = 0; iter < i; iter++, cur = cur.prev) { }
                return cur.item;
            }
            if (first == null) throw new IndexOutOfRangeException(string.Format("index is {0}, first is null", i));            
            cur = first;
            for (var iter = 0; iter < i; iter++, cur = cur.next) { }
            return cur.item;
        }

        #region IEnumerator
        public void Dispose()
        {
            length = 0;
            first = null;
            last = null;
        }

        public bool MoveNext()
        {
            if (current == null || first == null)
            {
                current = first;
                return current != null;
            }
            current = current.next;
            return current != null;
        }

        public void Reset()
        {
            current = null;
        }

        public T Current
        {
            get { return current == null ? default(T) : current.item; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return new RestrictedQueueEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RestrictedQueueEnumerator<T>(this);
        }
        #endregion
    }

    public class RestrictedQueueContainer<T>
    {
        public T item;
        public RestrictedQueueContainer<T> prev;
        public RestrictedQueueContainer<T> next;

        public RestrictedQueueContainer() { }
        public RestrictedQueueContainer(T itm)
        {
            item = itm;
        }
    }

    public class RestrictedQueueEnumerator<T> : IEnumerator<T>
    {
        private readonly RestrictedQueue<T> queue;

        public RestrictedQueueEnumerator(RestrictedQueue<T> queue)
        {
            this.queue = queue;
            queue.Reset();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return queue.MoveNext();
        }

        public void Reset()
        {
            queue.Reset();
        }

        public T Current
        {
            get { return queue.Current; }
        }

        object IEnumerator.Current
        {
            get { return queue.Current; }
        }
    }
}
