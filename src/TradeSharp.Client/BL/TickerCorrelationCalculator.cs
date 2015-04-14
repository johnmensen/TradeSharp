using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public class PortfolioActive
    {
        public string Ticker { get; set; }
        public int Side { get; set; }
        public float Leverage { get; set; }
        public float Price { get; set; }

        public static float CalculateProfitPercent(List<PortfolioActive> actives,
            List<float> newPrices)
        {
            var profit = 0f;
            for (var i = 0; i < actives.Count; i++)
            {
                var dp = 100 * actives[i].Side * (newPrices[i] - actives[i].Price) / actives[i].Price * actives[i].Leverage;
                profit += dp;
            }
            return profit;
        }
    }

    /// <summary>
    /// считает корреляцию пар и определяет вероятность смещения цен в портфеле
    /// </summary>
    static class TickerCorrelationCalculator
    {
        private static readonly Random rand = new Random();

        public delegate void ReportProgressDel(string progressStr);

        public static volatile bool stoppingCalculation;

        public static bool CalculateCorrelation(List<string> tickers, 
            int intervalMinutes, 
            int intervalsCount,
            int minIntervals,
            bool uploadQuotesFromServer,
            out List<string> errors,
            out double[][] cor,
            out List<double> sigmas)
        {
            sigmas = null;
            errors = new List<string>();
            if (tickers.Count == 0)
            {
                cor = new [] { new double[] { 1 }};
                return true;
            }

            var count = tickers.Count;
            cor = new double[count][];
            for (var i = 0; i < count; i++)
            {
                cor[i] = new double[count];
                cor[i][i] = 1;
            }

            // обновить котировки по тикерам
            var startTime = EnsureQuotes(tickers, intervalMinutes, intervalsCount, uploadQuotesFromServer);

            // заполнить массивы приращений вал. пар
            var rates = GetTickersRates(tickers, startTime, intervalMinutes, errors);
            if (rates == null || rates.Count == 0 || rates[tickers[0]].Count == 0) return false;

            // считать СКО
            sigmas = rates.Select(r => CalculateSKO(r.Value)).ToList();
            
            // считать корреляцию
            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    var tickA = tickers[i];
                    var tickB = tickers[j];
                    var r = CalculateCorrelation(rates[tickA], rates[tickB]);
                    cor[i][j] = r;
                    cor[j][i] = r;
                }
            }
            return true;
        }

        private static DateTime EnsureQuotes(List<string> tickers, 
            int timeframeMinutes, int intervalsCount, bool uploadQuotesFromServer)
        {
            var totalMinutes = timeframeMinutes * intervalsCount;
            var totalDays = totalMinutes / 1440;
            if (totalDays < 4) totalDays = 4;
            if (totalDays > 6) totalDays = 7 * totalDays / 5;
            var timeStart = DateTime.Now.AddMinutes(-totalDays*1440);
            if (!uploadQuotesFromServer) return timeStart;
            var dicUpload = tickers.ToDictionary(t => t, t => timeStart);
            if (dicUpload.Count == 0) return timeStart;
            MainForm.Instance.UpdateTickersCacheForRobots(dicUpload, 60);
            return timeStart;
        }

        private static Dictionary<string, List<float>> GetTickersRates(List<string> tickers, DateTime dateStart,
            int intervalMinutes, List<string> errors)
        {
            var curs = new BacktestTickerCursor();
            if (!curs.SetupCursor(string.Format("{0}{1}", ExecutablePath.ExecPath, TerminalEnvironment.QuoteCacheFolder),
                             tickers, dateStart))
            {
                errors.Add(Localizer.GetString("MessageErrorGettingQuotesFromFiles"));
                return null;
            }
            var rates = tickers.ToDictionary(t => t, t => new List<float>());                
            try
            {
                curs.MoveToTime(dateStart);
                // выйти на некоторую начальную точку, в которой представлены все валютные пары
                var lastTime = DateTime.MinValue;
                var quotes = new List<Cortege2<string, CandleData>>();
                while (true)
                {
                    if (!curs.MoveNext()) break;
                    quotes = curs.GetCurrentQuotes();
                    if (quotes.Count < tickers.Count) continue; // !!
                    lastTime = quotes[0].b.timeOpen;                    
                    break;
                }
                if (quotes.Count < tickers.Count)
                {
                    var nullTickers = tickers.Where(t => !quotes.Any(q => q.a == t));
                    errors.Add(string.Format(Localizer.GetString("MessageNoQuotesForTickers") + ": {0}",
                        string.Join(", ", nullTickers)));
                    return null;
                }
                
                // считать приращения
                while (curs.MoveNext())
                {
                    var newQuotes = curs.GetCurrentQuotes();
                    var time = newQuotes[0].b.timeOpen;
                    if ((time - lastTime).TotalMinutes < intervalMinutes) continue;
                    lastTime = time;
                    for (var i = 0; i < quotes.Count; i++)
                    {
                        rates[quotes[i].a].Add((newQuotes[i].b.open - quotes[i].b.open)/quotes[i].b.open);
                    }
                    quotes = newQuotes;
                }
            }
            catch (Exception ex)
            {
                errors.Add(Localizer.GetString("MessageErrorReadingQuotes"));
                Logger.Error("GetTickersRates error", ex);
            }
            finally
            {                
                curs.Close();                
            }
            if (rates.Count == 0 || rates[tickers[0]].Count == 0)
                errors.Add(Localizer.GetString("MessageErrorCalcDeltas"));
            return rates;
        }

        private static double CalculateSKO(List<float> a)
        {
            var avg = a.Average();
            return Math.Sqrt(a.Sum(x => (x - avg)*(x - avg)) / a.Count);
        }

        private static double CalculateCorrelation(List<float> a, List<float> b)
        {
            var ma = a.Average();
            var mb = b.Average();
            double numer = 0;
            double denA = 0, denB = 0;
            for (var i = 0; i < a.Count; i++)
            {
                var devA = (a[i] - ma);
                var devB = (b[i] - mb); 
                numer += devA * devB;
                denA += devA * devA;
                denB += devB * devB;
            }
            var den2 = denA * denB;
            return den2 == 0 ? 0 : numer / Math.Sqrt(den2);
        }
        
        /// <summary>
        /// rowDim - row dimension
        /// </summary>        
        public static double[][] CholeskyTransform(double[][] m, int rowDim, int colDim)
        {
            // Initialize.
			var l = new double[rowDim][];
			for (int i = 0; i < rowDim; i++)
			{
				l[i] = new double[rowDim];
			}
			var isspd = colDim == rowDim;
			// Main loop.
			for (int j = 0; j < rowDim; j++)
			{
				double[] lrowj = l[j];
				double d = 0.0;
				for (int k = 0; k < j; k++)
				{
					double[] lrowk = l[k];
					double s = 0.0;
					for (int i = 0; i < k; i++)
					{
						s += lrowk[i] * lrowj[i];
					}
					lrowj[k] = s = (m[j][k] - s) / l[k][k];
					d = d + s * s;
					isspd = isspd & (m[k][j] == m[j][k]);
				}
				d = m[j][j] - d;
				isspd = isspd & (d > 0.0);
				l[j][j] = Math.Sqrt(Math.Max(d, 0.0));
				for (int k = j + 1; k < rowDim; k++)
				{
					l[k][j] = 0.0;
				}
			}

            return TransposeMatrix(l);
        }

        private static double[][] TransposeMatrix(double[][] m)
        {
            if (m.Length == m[0].Length)
            {
                for (var i = 0; i < m.Length; i++)
                    for (var j = 0; j < m.Length; j++)
                    {
                        if (i < j)
                        {
                            var buf = m[i][j];
                            m[i][j] = m[j][i];
                            m[j][i] = buf;
                        }
                    }
                return m;
            }
            var rows = m.Length;
            var cols = m[0].Length;
            var n = new double[cols][];
            for (var i = 0; i < cols; i++)
            {
                n[i] = new double[rows];
                for (var j = 0; j < rows; j++)
                    n[i][j] = m[j][i];
            }
            return n;
        }

        /// <summary>
        /// моделировать поведение портфеля методом монте-карло
        /// возвращает массив: кол-во строк равно количеству percentiles
        /// в каждой строке intervalsCount значений по каждому перцентилю
        /// </summary>
        public static double[][] CalculateRisks(List<PortfolioActive> actives, double [][] corrM, List<double> sigmas,
            int intervalMinutes, int intervalsCount, 
            int iterationsCount,
            List<double> percentiles,
            ReportProgressDel reportProgress)
        {
            // преобразование Холецкого
            var chM = CholeskyTransform(corrM, corrM.Length, corrM.Length);
            // массив: одна строка соотв. одному испытанию
            // ячейка i строки - накопленнный за i-интервалов профит, %
            var profitByDay = new float[intervalsCount][];
            for (var i = 0; i < intervalsCount; i++)
            {
                profitByDay[i] = new float[iterationsCount];
            }

            var lastPercent = 0;
            for (var i = 0; i < iterationsCount; i++)
            {
                if (stoppingCalculation)
                {
                    stoppingCalculation = false;
                    return null;
                }
                var startPrices = actives.Select(a => a.Price).ToList();
                for (var j = 0; j < intervalsCount; j++)
                {
                    ShiftPrices(startPrices, chM, sigmas);
                    // посчитать отклонение портфеля
                    var profit = PortfolioActive.CalculateProfitPercent(actives, startPrices);
                    profitByDay[j][i] = profit;
                }
                var percent = 100*i/iterationsCount;
                if (percent - lastPercent >= 5)
                {
                    reportProgress(string.Format(Localizer.GetString("MessagePortfolioSimulationCompletedPercentFmt"), percent));
                    lastPercent = percent;
                }
            }

            // посчитать перцентили
            var percents = new double[percentiles.Count][];
            for (var i = 0; i < percentiles.Count; i++)
                percents[i] = new double[intervalsCount];

            lastPercent = 0;
            for (var j = 0; j < intervalsCount; j++)
            {
                if (stoppingCalculation)
                {
                    stoppingCalculation = false;
                    return null;
                }
                // упорядочить массив профитов на день (ну, вообще-то - на данный интервал времени)
                var intervalProfit = new EverSortedList<float>();
                for (var i = 0; i < iterationsCount; i++)
                {
                    intervalProfit.Add(profitByDay[j][i]);
                }
                
                // записать перцентили на интервал
                for (var i = 0; i < percentiles.Count; i++)
                {
                    var index = (int)(iterationsCount * percentiles[i] / 100);
                    percents[i][j] = intervalProfit[index];
                    if (j > 0) if (Math.Abs(percents[i][j]) < Math.Abs(percents[i][j - 1]))
                        percents[i][j] = percents[i][j - 1];
                }

                var percent = 100 * j / intervalsCount;
                if (percent - lastPercent >= 5)
                {
                    reportProgress(string.Format(Localizer.GetString("MessageResultProcessingCompletedPercentFmt"), percent));
                    lastPercent = percent;
                }
            }
            return percents;
        }

        private static void ShiftPrices(List<float> prices, double [][] chM, List<double> sigmas)
        {
            // получить случайный вектор
            var v = new double[prices.Count];
            for (var i = 0; i < prices.Count; i++)
                v[i] = NextGauss(0, sigmas[i]);
            
            // умножить на матрицу Холецкого
            var corDeltas = MultipleByMatrix(v, chM);
            for (var i = 0; i < prices.Count; i++)
            {
                prices[i] = (float) (prices[i] + prices[i]*corDeltas[i]);
            }            
        }

        public static double[] MultipleByMatrix(double[] a, double[][]b)
        {
            var result = new double[a.Length];
            for (int j = 0; j < a.Length; j++)
                    for (int k = 0; k < a.Length; k++)
                        result[j] += a[k] * b[k][j];
            return result;
        }

        private static double NextGauss(double mean, double stdDev)
        {
            var u1 = rand.NextDouble();
            var u2 = rand.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}
