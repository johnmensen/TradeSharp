using System;
using System.Collections;
using System.Collections.Generic;
using IEnumerator=System.Collections.IEnumerator;

namespace Candlechart.ChartMath
{
    public class DataArray<T> : IList<T>
    {
        #region Внутренний список
        private readonly List<T> innerList;

        private int _startIndex;

        public List<T> InnerList
        {
            get { return innerList; }
        }
        #endregion

        #region Конструкторы
        public DataArray()
        {
            innerList = new List<T>();
        }

        internal DataArray(int capacity)
        {
            innerList = new List<T>(capacity);
        }
        #endregion

        #region Собственные свойства - методы
        public int Count
        {
            get
            {
                return innerList.Count;
            }
        }

        internal int BarCount
        {
            get
            {
                if (((LastIndex - StartIndex) + 1) < 0)
                {
                    return 0;
                }
                return ((LastIndex - StartIndex) + 1);
            }
        }

        public T this[int index]
        {
            get { return innerList[index]; }
            set { innerList[index] = value; }
        }

        internal int LastIndex
        {
            get { return (innerList.Count - 1); }
        }

        internal int StartIndex
        {
            get { return _startIndex; }
            set
            {
                if ((_startIndex < 0) || (_startIndex > (innerList.Count - 1)))
                    throw new ArgumentOutOfRangeException("StartIndex", "Index out of bounds.");
                _startIndex = value;
            }
        }

        public void Add(T value)
        {
            innerList.Add(value);
        }

        public void Clear()
        {
            innerList.Clear();
        }
        #endregion

        #region IEnumerable<T>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
        }        

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public bool Remove(T item)
        {
            return innerList.Remove(item);
        }

        public void CopyTo(T[] arr, int index)
        {
            innerList.CopyTo(arr, index);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}
