using System.Collections.Generic;
using Entity;
using TradeSharp.Util;

namespace Candlechart.ChartMath
{
    public enum ZigZagSource { HighLow = 0, OpenClose = 1}

    public static class ZigZag
    {
        public static List<Cortege2<int, float>> GetPivots(IList<CandleData> candles,
            float thresholdPercent, ZigZagSource srcType)
        {
            return GetPivots(candles, thresholdPercent, candles.Count - 1, srcType);
        }

        public static List<Cortege2<int, float>> GetPivots(IList<CandleData> candles,
            float thresholdPercent, int endIndex, ZigZagSource srcType)
        {
            var pivots = new List<Cortege2<int, float>>();
            if (candles.Count == 0) return pivots;
            var lastPivot = new Cortege2<int, float> { a = 0, b = candles[0].open };
            int lastSign = 0;

            // занести вершины
            int i = 1;
            for (; i <= endIndex; i++)
            {
                var candle = candles[i];
                var high = srcType == ZigZagSource.HighLow ? candle.high : System.Math.Max(candle.open, candle.close);
                var low = srcType == ZigZagSource.HighLow ? candle.low : System.Math.Min(candle.open, candle.close);

                var deltaPlus = high - lastPivot.b;
                var deltaMinus = lastPivot.b - low;
                deltaPlus = deltaPlus > 0 ? 100 * deltaPlus / lastPivot.b : 0;
                deltaMinus = deltaMinus > 0 ? 100 * deltaMinus / lastPivot.b : 0;
                if (deltaPlus > thresholdPercent &&
                    ((deltaPlus > deltaMinus && lastSign == 0) || lastSign < 0))
                {
                    pivots.Add(lastPivot);
                    lastPivot = new Cortege2<int, float> { a = i, b = high };
                    lastSign = 1;
                    continue;
                }
                if (deltaMinus > thresholdPercent &&
                    ((deltaPlus <= deltaMinus && lastSign == 0) || lastSign > 0))
                {
                    pivots.Add(lastPivot);
                    lastPivot = new Cortege2<int, float> { a = i, b = low };
                    lastSign = -1;
                    continue;
                }
                if (lastSign > 0 && high > lastPivot.b)
                {
                    lastPivot.b = high;
                    lastPivot.a = i;
                    continue;
                }
                if (lastSign < 0 && low < lastPivot.b)
                {
                    lastPivot.b = low;
                    lastPivot.a = i;
                    continue;
                }
            }
            // последняя вершина
            if (lastSign != 0)
            {
                //var candleLast = candles[endIndex];
                //var lastHigh = srcType == ZigZagSource.HighLow ? candleLast.high : 
                //    System.Math.Max(candleLast.open, candleLast.close);
                //var lastLow = srcType == ZigZagSource.HighLow ? candleLast.low :
                //    System.Math.Min(candleLast.open, candleLast.close);

                //pivots.Add(new Cortege2<int, float>
                //{
                //    a = endIndex,
                //    b = lastSign > 0 ? lastHigh : lastLow
                //});
                pivots.Add(lastPivot);
            }
            return pivots;
        }
    }
}
