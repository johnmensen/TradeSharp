using System;
using System.Collections.Generic;
using System.Drawing;

namespace Entity
{
    public class DiverSpan
    {
        public int start, end;
        public int sign;

        public DiverSpan(int start, int end, int sign)
        {
            this.start = start;
            this.end = end;
            this.sign = sign;
        }

        public bool Includes(DiverSpan a)
        {
            if (a.sign != sign) return false;
            return
                (start >= a.start && start <= a.end) || (end >= a.start && end <= a.end);
        }
    }

    public static class Divergency
    {
        public delegate double GetPrice(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDataCount"></param>
        /// <param name="periodExtremum"></param>
        /// <param name="maxPastExtremum"></param>
        /// <param name="getSourcePrice"></param>
        /// <param name="getIndexPrice"></param>
        /// <param name="waitOneBar">ждать одного бара перед отрисовкой стрелки, закрывающегося ниже (для макс) или выше (для мин)</param>
        /// <returns></returns>
        public static List<DiverSpan> FindDivergencePointsClassic(
            int sourceDataCount,
            int periodExtremum,
            int maxPastExtremum,
            GetPrice getSourcePrice,
            GetPrice getIndexPrice,
            bool waitOneBar)
        {
            var diverPairs = new List<DiverSpan>();
            var srcCount = sourceDataCount;

            // найти экстремумы            
            var extremums = new List<Point>(); // index - sign
            for (var i = periodExtremum; i < srcCount - periodExtremum - 1; i++)
            {
                bool isPeak = true, isLow = true;
                var price = getSourcePrice(i);
                for (var j = i - periodExtremum; j <= i + periodExtremum; j++)
                {
                    if (j == i) continue;
                    var priceNear = getSourcePrice(j);
                    if (priceNear > price) isPeak = false;
                    if (priceNear < price) isLow = false;
                    if (!isPeak && isLow == false) break;
                }
                if (isPeak) extremums.Add(new Point(i, 1));
                if (isLow) extremums.Add(new Point(i, -1));
            }

            // от каждого экстремума поиск вперед до M баров - поиск экстремума на интервале [i-N...i+1]
            foreach (var extr in extremums)
            {
                var signExtr = extr.Y;
                var indexExtr = extr.X;
                var priceExtr = getSourcePrice(indexExtr);

                //for (var i = extr.X + 1; i <= (extr.X + MaxPastExtremum); i++)
                for (var i = extr.X + periodExtremum; i <= (extr.X + maxPastExtremum); i++)
                {
                    // не выйти за пределы интервала, если нужен бар подтверждения - остановиться за бар до конца интервала
                    if (i > (srcCount - 1) || (waitOneBar && i == (srcCount - 1))) break;
                    // новый "экстремум" должен быть выше старого
                    var price = getSourcePrice(i);
                    if ((signExtr > 0 && price <= priceExtr) ||
                        (signExtr < 0 && price >= priceExtr)) continue;
                    // за новым "экстремумом" должна следовать точка ниже (выше для минимума)
                    if (waitOneBar)
                    {
                        var nextPrice = getSourcePrice(i + 1);
                        if ((signExtr > 0 && price <= nextPrice) ||
                            (signExtr < 0 && price >= nextPrice)) continue;
                    }
                    // сравнение с дельтой индекса
                    // !! смещение индекса
                    var indexPriceI = getIndexPrice(i);
                    var indexPriceEx = getIndexPrice(indexExtr);
                    var deltaIndex = (double.IsNaN(indexPriceI) || double.IsNaN(indexPriceEx)) 
                        ? 0 
                        : indexPriceI - indexPriceEx;
                    var signDelta = double.IsNaN(deltaIndex) ? 0 : Math.Sign(deltaIndex);
                    if (signExtr == signDelta) continue;
                    // экстремум найден
                    priceExtr = price;
                    diverPairs.Add(new DiverSpan(indexExtr, i, -signExtr));
                }
            }

            return diverPairs;
        }

        public static List<DiverSpan> FindDivergencePointsQuasi(
            int sourceDataCount,
            double marginUpper,
            double marginLower,
            GetPrice getSourcePrice, 
            GetPrice getIndexPrice)
        {
            var divers = new List<DiverSpan>();
            var srcCount = sourceDataCount;

            double? lastPeak = null;
            var peakPrice = 0.0;
            var extremumSign = 0;
            var startIndex = 0;

            for (var i = 0; i < srcCount; i++)
            {
                var index = getIndexPrice(i);
                var indexNext = i < srcCount ? getIndexPrice(i + 1) : index;
                // если находимся в зоне перекупленности / перепроданности,
                // если след. точка ниже (выше для перепроданности),
                // если текущая точка выше последнего максимума (ниже минимума...)

                var margUp = index >= marginUpper;
                var margDn = index <= marginLower;

                if ((margUp && indexNext < index &&
                    index > (lastPeak.HasValue ? lastPeak.Value : double.MinValue)) ||
                    (margDn && indexNext > index && index < (lastPeak.HasValue ? lastPeak.Value : double.MaxValue)))
                {// + или - экстремум 
                    peakPrice = getSourcePrice(i);
                    extremumSign = margUp ? 1 : -1;
                    startIndex = i;
                    lastPeak = index;
                    continue;
                }
                if (!margUp && margDn == false)
                {// конец отсчета экстремума
                    extremumSign = 0;
                    lastPeak = null;
                    continue;
                }
                // отсчет экстремума - сравнить дельту цены и дельту индекса
                var deltaPrice = getSourcePrice(i) - peakPrice;
                var deltaIndex = index - lastPeak;

                if (extremumSign > 0)
                    if (deltaPrice > 0 && deltaIndex < 0)
                    // медвежий дивер
                    {
                        divers.Add(new DiverSpan(startIndex, i, -1));
                        peakPrice = getSourcePrice(i);
                    }
                if (extremumSign < 0)
                    if (deltaPrice < 0 && deltaIndex > 0)
                    // бычий дивер
                    {
                        divers.Add(new DiverSpan(startIndex, i, 1));
                        peakPrice = getSourcePrice(i);
                    }
            }
            return divers;
        }
    }
}
