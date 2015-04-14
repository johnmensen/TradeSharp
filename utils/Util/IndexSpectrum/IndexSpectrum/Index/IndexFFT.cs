using System;
using System.Collections.Generic;
using System.Linq;
using FXI.Client.MtsBase.Common;

namespace IndexSpectrum.Index
{
    public class IndexFFT
    {
        public enum TrendReducion
        {
            Linear, MovingAverage
        }

        public enum CurrencyIndexType
        {
            AdditiveWeightedToOne = 0,
            AdditiveMomentumAccumulated
        }

        /// <param name="indexStr">формула индекса</param>
        /// <param name="path">каталог с котировками</param>
        /// <param name="intervalMinutes">период дискретизации</param>
        /// <param name="dataLength">количество отсчетов для выведения характеристик</param>
        /// <param name="trendReducion">тип удаления тренда</param>
        /// <param name="trendMAPeriod">период СС для удаления тренда</param>
        /// <returns></returns>
        public static double[] GetIndexData(string indexStr, string path, int intervalMinutes, int dataLength,
            TrendReducion trendReducion, int trendMAPeriod, DateTime? startDate, CurrencyIndexType curIndexType)
        {
            var ci = new CurrencyIndexInfo(indexStr) {MomentumPeriod = 1};
            // аддитивный индекс без приращений,
            // веса уравнивают слагаемые в точке отсчета
            if (curIndexType == CurrencyIndexType.AdditiveWeightedToOne)
            {
                for (var i = 0; i < ci.weights.Length; i++)
                    ci.weights[i] = 0; // вес посчитается автоматически            
            }
            var currencyStream = new Dictionary<string, CurrencyStream>();
            // открыть потоки чтения
            foreach (var curName in ci.pairs)
            {
                currencyStream.Add(curName, new CurrencyStream(curName,
                    path));
            }

            // посчитать данные индекса (ряд)
            var indexData = new List<double>();
            decimal index = 0;
            try
            {
                if (!startDate.HasValue)
                    startDate = GetFirstDateFromCurStreams(currencyStream, false);
                if (!startDate.HasValue) return new double[0];
                DateTime date = startDate.Value;

                int dataCountMax = dataLength + (trendReducion == TrendReducion.MovingAverage ? trendMAPeriod : 0);
                for (var i = 0; i < dataCountMax; i++)
                {
                    index = (decimal)ci.CalculateIndexMultiplicative(date, currencyStream);                    
                    indexData.Add((double)index);
                    date = date.AddMinutes(intervalMinutes);
                }

                
            }
            finally
            {
                // закрыть потоки чтения
                foreach (var cs in currencyStream)
                    cs.Value.CloseStream();
            }

            if (indexData.Count < 10) return new double[0];
            return indexData.ToArray();            
        }

        public static double[] RemoveTrend(double[] data,
            TrendReducion trendReducion, int trendMAPeriod)
        {
            if (trendReducion == TrendReducion.Linear)
                ReduceTrendLinear(data);
            if (trendReducion == TrendReducion.MovingAverage)
                data = ReduceTrendMA(data, trendMAPeriod);
            return data;
        }

        public static double[] CalcFFT(double[] dIn, int dataLength)
        {
            int i, j, n, m, mmax, istep;
            double tempr, tempi, wtemp, theta, wpr, wpi, wr, wi;

            int isign = -1;
            var data = new double[dataLength * 2 + 1];

            for (i = 0; i < dataLength; i++)
            {
                data[i * 2] = 0;
                data[i * 2 + 1] = dIn[i];
            }

            n = dataLength << 1;
            j = 1;
            i = 1;
            while (i < n)
            {
                if (j > i)
                {
                    tempr = data[i]; data[i] = data[j]; data[j] = tempr;
                    tempr = data[i + 1]; data[i + 1] = data[j + 1]; data[j + 1] = tempr;
                }
                m = n >> 1;
                while ((m >= 2) && (j > m))
                {
                    j = j - m;
                    m = m >> 1;
                }
                j = j + m;
                i = i + 2;
            }
            mmax = 2;
            while (n > mmax)
            {
                istep = 2 * mmax;
                theta = 2.0 * Math.PI / (isign * mmax);
                wtemp = Math.Sin(0.5 * theta);
                wpr = -2.0 * wtemp * wtemp;
                wpi = Math.Sin(theta);
                wr = 1.0;
                wi = 0.0;
                m = 1;
                while (m < mmax)
                {
                    i = m;
                    while (i < n)
                    {
                        j = i + mmax;
                        tempr = wr * data[j] - wi * data[j + 1];
                        tempi = wr * data[j + 1] + wi * data[j];
                        data[j] = data[i] - tempr;
                        data[j + 1] = data[i + 1] - tempi;
                        data[i] = data[i] + tempr;
                        data[i + 1] = data[i + 1] + tempi;
                        i = i + istep;
                    }
                    wtemp = wr;
                    wr = wtemp * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                    m = m + 2;
                }
                mmax = istep;
            }
            var dOut = new double[dataLength / 2];

            for (i = 0; i < (dataLength / 2); i++)
            {
                dOut[i] = Math.Sqrt(data[i * 2] * data[i * 2] + data[i * 2 + 1] * data[i * 2 + 1]);
            }

            return dOut;
        }

        private static void ReduceTrendLinear(double []data)
        {
            var delta = data[data.Length - 1] - data[0];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = data[i] - i*delta/data.Length;
            }
        }

        private static double [] ReduceTrendMA(double []data, int maPeriod)
        {
            var maData = new RestrictedQueue<double>(maPeriod);
            var outData = new List<double>();
            for (var i = 0; i < data.Length; i++)
            {
                maData.Add(data[i]);
                if (maData.Length < maPeriod) continue;
                var ma = maData.Average();
                outData.Add(data[i] / ma);
            }
            return outData.ToArray();
        }
        
        public static DateTime? GetFirstDateFromCurStreams(Dictionary<string, CurrencyStream> curStreams,
            bool useFirst)
        {
            DateTime? minTime = null;
            DateTime? maxTime = null;

            foreach (var cs in curStreams)
            {
                var time = cs.Value.GetFirstQuoteDate();
                if (!minTime.HasValue && time.HasValue)
                {
                    minTime = time;
                    maxTime = time;
                    continue;
                }

                if (time.HasValue)
                {
                    if (time.Value < minTime.Value)
                        minTime = time;
                    if (time.Value > maxTime.Value)
                        maxTime = time;
                }
            }
            return useFirst ? minTime : maxTime;
        }
    }
}
