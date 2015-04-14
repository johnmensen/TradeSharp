using System;
using System.Collections.Generic;
using Entity;

namespace Candlechart.ChartMath
{
    public class StockSeriesData
    {
        private readonly List<CandleData> candles = new List<CandleData>();
        public List<CandleData> Candles { get { return candles; } }        
        public int Count
        {
            get { return candles.Count; }
        }
        internal int LastIndex
        {
            get { return (Count - 1); }
        }
        private int _startIndex;
        internal int StartIndex
        {
            get { return _startIndex; }
            set
            {
                if ((_startIndex < 0) || (_startIndex > (Count - 1)))
                {
                    throw new ArgumentOutOfRangeException("StartIndex", "Index out of bounds.");
                }
                _startIndex = value;
            }
        }

        public void Add(float open, float high, float low, float close, DateTime date)
        {
            candles.Add(new CandleData
            {
                open = open,
                high = high,
                low = low,
                close = close,
                timeOpen = date
            });
        }

        public void Clear()
        {
            candles.Clear();
        }
        public CandleData this[int index]
        {
            get { return candles[index]; }
        }
    }
}