using System;
using Candlechart.Series;

namespace Candlechart.Core
{
    /// <summary>
    /// Описывает временную шкалу свечей: время первой - время последней свечиЮ индексы
    /// </summary>
    public class CandleRange
    {
        private StockSeries stockSeries;
        public CandleRange(StockSeries _stockSeries)
        {
            stockSeries = _stockSeries;
        }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double StartIndex { get; set; }
        public double EndIndex { get; set; }
        public double GetXCoord(DateTime t)
        {
            if (stockSeries.Data.Count < 2) return 0;
            DateTime timeA = stockSeries.Data[0].timeOpen;
            if (t <= timeA) return 0;
            for (int i = 1; i < stockSeries.Data.Count; i++)
            {
                DateTime timeB = stockSeries.Data[i].timeOpen;
                if (t >= timeA && t < timeB)
                {
                    return i + (t.Ticks - timeA.Ticks) / (timeB.Ticks - timeA.Ticks) - 1;
                }
                timeA = timeB;
            }
            return stockSeries.Data.Count;
        }
    }
}
