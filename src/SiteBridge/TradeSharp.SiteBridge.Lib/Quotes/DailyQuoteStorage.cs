using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Quotes
{
    public class DailyQuoteStorage : IDailyQuoteStorage
    {
        private readonly ThreadSafeStorage<string, List<Cortege2<DateTime, float>>> quotes = 
            new ThreadSafeStorage<string, List<Cortege2<DateTime, float>>>();

        private readonly string quoteFolder = AppDomain.CurrentDomain.BaseDirectory + "\\quotes";

        private readonly ServerQuoteHistory quoteHistoryOnServer;

        private readonly IQuoteStorage quoteStorage;

        public DailyQuoteStorage()
        {
            try
            {
                quoteStorage = TradeSharp.Contract.Util.Proxy.QuoteStorage.Instance.proxy;
            }
            catch (Exception ex)
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена", ex);
                throw;
            }
            quoteHistoryOnServer = new ServerQuoteHistory(quoteStorage);
            try
            {
                if (!Directory.Exists(quoteFolder))
                    Directory.CreateDirectory(quoteFolder);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка создания каталога " + quoteFolder, ex);
                throw;
            }
        }

        public void InitializeFake(Dictionary<string, List<Cortege2<DateTime, float>>> qts)
        {
            quotes.Clear();        
            foreach (var pair in qts)
                quotes.UpdateValues(pair.Key, pair.Value);
        }

        public List<Cortege2<DateTime, float>> GetQuotes(string ticker)
        {
            return quotes.ReceiveValue(ticker) ?? new List<Cortege2<DateTime, float>>();
        }

        public void UpdateStorageSync()
        {
            foreach (var ticker in DalSpot.Instance.GetTickerNames())
            {
                UpdateTicker(ticker);
            }
        }

        private void UpdateTicker(string ticker)
        {
            // прочитать содержимое файла
            var tickerQuotes = LoadTickerQuotesFromFile(ticker);
            
            // интервал истории, хранимой на сервере
            var serverRange = quoteHistoryOnServer.GetServerTickerHistorySpan(ticker);
            if (serverRange == null && tickerQuotes.Count == 0) return;
            if (serverRange != null && serverRange.TotalDays < 1)
                serverRange = null;
            if (serverRange == null) // записать, что есть
            {
                var oldQuotes = quotes.ReceiveValue(ticker);
                if (oldQuotes == null || oldQuotes.Count < tickerQuotes.Count)
                {
                    quotes.UpdateValues(ticker, tickerQuotes);
                    SaveUpdatedQuotesInFile(ticker, tickerQuotes);
                }
                return;
            }
            
            // уточнить, за какой период загружать данные
            var intervalsToUpdate = new List<DateSpan>();
            if (tickerQuotes.Count == 0)
                intervalsToUpdate.Add(serverRange);
            else
            {
                if (serverRange.start.Date < tickerQuotes[0].a)
                    intervalsToUpdate.Add(new DateSpan(serverRange.start.Date, tickerQuotes[0].a));
                var lastDate = tickerQuotes[tickerQuotes.Count - 1].a;
                if ((DateTime.Now.Date - lastDate).TotalDays < 2)
                    lastDate = DateTime.Now.Date.AddDays(-2);
                if (intervalsToUpdate.Count > 0 && lastDate <= intervalsToUpdate[0].end)
                    intervalsToUpdate[0] = new DateSpan(intervalsToUpdate[0].start, DateTime.Now.Date);
                else
                    intervalsToUpdate.Add(new DateSpan(lastDate, DateTime.Now.Date));
            }

            // таки загрузить данные
            tickerQuotes = UpdateTickerQuotesFromServer(ticker, intervalsToUpdate, tickerQuotes);

            // и сохранить их в файле
            quotes.UpdateValues(ticker, tickerQuotes);
            if (tickerQuotes.Count > 0)
                SaveUpdatedQuotesInFile(ticker, tickerQuotes);
        }

        private void SaveUpdatedQuotesInFile(string ticker, List<Cortege2<DateTime, float>> tickerQuotes)
        {
            try
            {
                using (var sw = new StreamWriter(quoteFolder + "\\" + ticker + ".txt", false, Encoding.UTF8))
                {
                    foreach (var q in tickerQuotes)
                    {
                        sw.WriteLine("{0:ddMMyyyy} {1}", q.a, q.b.ToStringUniformPriceFormat(true));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в SaveUpdatedQuotesInFile({0}): {1}", ticker, ex);
            }
        }

        private List<Cortege2<DateTime, float>> UpdateTickerQuotesFromServer(string ticker, 
            List<DateSpan> intervalsToUpdate, List<Cortege2<DateTime, float>> tickerQuotes)
        {
            foreach (var interval in intervalsToUpdate)
            {
                var candles = LoadCandlesFromServer(ticker, interval);
                if (candles.Count == 0) continue;
                // склеить котировки
                tickerQuotes.AddRange(candles);
            }
            tickerQuotes.Sort((a, b) => a.a.CompareTo(b.a));
            
            // исключить дублирующиеся котировки
            var resultedList = new List<Cortege2<DateTime, float>>();
            foreach (var quote in tickerQuotes)
            {
                if (resultedList.Count == 0)
                {
                    resultedList.Add(quote);
                    continue;
                }
                var lastQuote = resultedList[resultedList.Count - 1];
                if (lastQuote.a == quote.a) continue;
                if (lastQuote.a.Date == quote.a.Date)
                {
                    if (quote.a < lastQuote.a)
                        resultedList[resultedList.Count - 1] = quote;
                    continue;
                }
                resultedList.Add(quote);
            }

            // оставить только даты
            return resultedList.Select(q => new Cortege2<DateTime, float>(q.a.Date, q.b)).ToList();
        }

        private List<Cortege2<DateTime, float>> LoadCandlesFromServer(string ticker, DateSpan span)
        {
            var candles = new List<CandleData>();
            const int stepDays = 7;
            for (var start = span.start; start < span.end; start = start.AddDays(stepDays))
            {
                var end = start.AddDays(stepDays);
                if (end > span.end) end = span.end;
                end = end.AddMinutes(1);

                bool error;
                var candlesM1 = FileGapActualizator.LoadQuotesFromDbSynch(ticker, start, end, out error);
                if (candlesM1 == null || candlesM1.Count == 0) continue;

                candles.AddRange(candlesM1);
            }

            // проредить котировки до дневных
            var dailyCandles = new List<Cortege2<DateTime, float>>();
            DateTime? lastTime = null;
            foreach (var quote in candles)
            {
                if (lastTime == null || lastTime.Value.Date != quote.timeOpen.Date)
                {
                    dailyCandles.Add(new Cortege2<DateTime, float>(quote.timeOpen, quote.open));
                    lastTime = quote.timeOpen;
                }
            }

            return dailyCandles;
        }

        private List<Cortege2<DateTime, float>> LoadTickerQuotesFromFile(string ticker)
        {
            var tickerQuotes = new List<Cortege2<DateTime, float>>();
            var fileName = quoteFolder + "\\" + ticker + ".txt";
            if (!File.Exists(fileName))
                return tickerQuotes;

            try
            {
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    var separators = new [] {' '};
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        // 29112013 1.3657
                        var parts = line.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2) continue;
                        var price = parts[1].ToFloatUniformSafe();
                        if (!price.HasValue) continue;

                        DateTime time;
                        if (!DateTime.TryParseExact(parts[0], "ddMMyyyy", CultureProvider.Common, DateTimeStyles.None,
                                                    out time))
                            continue;
                        tickerQuotes.Add(new Cortege2<DateTime, float>(time, price.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in LoadTickerQuotesFromFile()", ex);
            }
            return tickerQuotes;
        }
    }
}
