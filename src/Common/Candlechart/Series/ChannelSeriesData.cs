using System;
using Candlechart.ChartMath;

namespace Candlechart.Series
{
    public class ChannelSeriesData
    {
        // Fields
        private DataArray<double> _bottom = new DataArray<double>();
        private DataArray<double> _top = new DataArray<double>();

        // Methods

        // Properties
        public int BarCount
        {
            get { return _top.BarCount; }
        }

        public DataArray<double> Bottom
        {
            get { return _bottom; }
        }

        public int Count
        {
            get { return _top.Count; }
        }

        internal int LastIndex
        {
            get { return (Count - 1); }
        }

        internal int StartIndex
        {
            get { return _top.StartIndex; }
            set { _top.StartIndex = value; }
        }

        public DataArray<double> Top
        {
            get { return _top; }
        }

        public void Add(double top, double bottom)
        {
            _top.Add(top);
            _bottom.Add(bottom);
        }

        public void Clear()
        {
            _top.Clear();
            _bottom.Clear();
        }

        public void SetDataArray(DataArray<double> top, DataArray<double> bottom)
        {
            if (top == null)
            {
                throw new ArgumentNullException("top", "DataArray cannot be null.");
            }
            if (bottom == null)
            {
                throw new ArgumentNullException("bottom", "DataArray cannot be null.");
            }
            if ((top.Count != bottom.Count) || (top.StartIndex != bottom.StartIndex))
            {
                throw new ArgumentException("The top and bottom arrays do not match.");
            }
            _top = top;
            _bottom = bottom;
        }
    }
}
