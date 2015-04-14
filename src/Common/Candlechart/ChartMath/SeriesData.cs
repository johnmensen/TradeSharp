using System;

namespace Candlechart.ChartMath
{
    public class SeriesData
    {
        // Fields
        private DataArray<double> _data = new DataArray<double>();

        // Methods

        // Properties
        public int BarCount
        {
            get { return _data.BarCount; }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public DataArray<double> DataArray
        {
            get { return _data; }
        }

        public double this[int index]
        {
            get { return _data[index]; }
        }

        internal int LastIndex
        {
            get { return (Count - 1); }
        }

        internal int StartIndex
        {
            get { return _data.StartIndex; }
            set { _data.StartIndex = value; }
        }

        public void Add(double value)
        {
            _data.Add(value);
        }

        public void Remove(int index)
        {
            _data.RemoveAt(index);
        }

        public void RemoveLast()
        {
            if (_data.Count > 0) _data.RemoveAt(_data.Count - 1);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public void SetDataArray(DataArray<double> array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "DataArray<double> cannot be null.");
            }
            _data = array;
        }
    }    
}
