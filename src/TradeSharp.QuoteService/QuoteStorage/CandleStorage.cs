using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.QuoteStorage
{
    /// <summary>
    /// сохраняет котировки, формируя свечи m1 в таблицу QUOTE
    /// </summary>
    class CandleStorage
    {
        #region Singleton

        private static readonly Lazy<CandleStorage> instance = new Lazy<CandleStorage>(() => new CandleStorage());

        public static CandleStorage Instance
        {
            get { return instance.Value; }
        }

        private CandleStorage()
        {
            FillTickerNameDictionary();
        }

        #endregion

        #region Данные

        private readonly Dictionary<string, int> tickerCodeBySymbol = new Dictionary<string, int>();

        private readonly Dictionary<int, CandleData> lastCandles = new Dictionary<int, CandleData>();

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 10);

        private const int LogMsgTickerCodeMissed = 1;

        private const int LogMsgTickerBidWrong = 2;

        #endregion

        #region Открытые методы
        public void UpdateQuotes(List<TickerQuoteData> quotes)
        {
            var candlesToSave = new List<Cortege2<int, CandleData>>();
            var nowTime = GetDateTimeWoSeconds(DateTime.Now);

            foreach (var q in quotes)
            {
                int ticker;
                if (!tickerCodeBySymbol.TryGetValue(q.Ticker, out ticker))
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgTickerCodeMissed,
                        1000 * 30, "[{0}] code is missed - can not store", q.Ticker);
                    continue;
                }
                if (q.bid <= 0)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgTickerBidWrong,
                        1000 * 30, "[{0}] bid is not valid - can not store", q.Ticker);
                    continue;
                }

                // обновить свечу
                CandleData candle;
                if (!lastCandles.TryGetValue(ticker, out candle)) continue;

                if (candle == null)
                {
                    candle = new CandleData(q.bid, q.bid, q.bid, q.bid, nowTime, nowTime);
                    candlesToSave.Add(new Cortege2<int, CandleData>(ticker, candle));
                    lastCandles[ticker] = candle;
                    continue;
                }

                if (candle.high < q.bid) candle.high = q.bid;
                if (candle.low > q.bid) candle.low = q.bid;
                candle.close = q.bid;
                if (candle.timeOpen != nowTime)
                {
                    candlesToSave.Add(new Cortege2<int, CandleData>(ticker, candle));
                    candle = new CandleData(q.bid, q.bid, q.bid, q.bid, nowTime, nowTime);
                    lastCandles[ticker] = candle;
                }
            }

            // сохранить новые свечки
            if (candlesToSave.Count > 0)
                CandleStoreStream.Instance.EnqueueCandles(candlesToSave);
        }
        #endregion

        #region Закрытые методы

        private void FillTickerNameDictionary()
        {
            var aliases = ReadTickerAliasFile();

            // заполнить словарь: тикер - код
            // заполнить словарь свечек
            foreach (var name in DalSpot.Instance.GetTickerNames())
            {
                var ticker = DalSpot.Instance.GetFXICodeBySymbol(name);
                if (ticker == 0) continue;

                var tickerName = name;
                string alias;
                if (aliases.TryGetValue(name, out alias))
                    tickerName = alias;

                tickerCodeBySymbol.Add(tickerName, ticker);
                lastCandles.Add(ticker, null);
            }

            Logger.Info(string.Join(", ", tickerCodeBySymbol.Select(p => string.Format("[{0}:{1}]", p.Key, p.Value))));
        }

        private Dictionary<string, string> ReadTickerAliasFile()
        {
            var fileName = ExecutablePath.ExecPath + "\\alias.txt";
            if (!File.Exists(fileName)) return new Dictionary<string, string>();
            string fileData;

            using (var sr = new StreamReader(fileName, Encoding.UTF8))
                fileData = sr.ReadToEnd();
            if (string.IsNullOrEmpty(fileData))
                return new Dictionary<string, string>();

            var parts = fileData.Split(new[] {Environment.NewLine, ";", ","}, StringSplitOptions.RemoveEmptyEntries);
            var aliases = new Dictionary<string, string>();

            foreach (var part in parts)
            {
                var keyVal = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyVal.Length != 2) continue;
                if (!aliases.ContainsKey(keyVal[0]))
                    aliases.Add(keyVal[0], keyVal[1]);
            }

            return aliases;
        }

        private DateTime GetDateTimeWoSeconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }

        #endregion
    }    
}
