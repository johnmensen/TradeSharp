using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.QuoteStorage
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class QuoteStorageManager : IQuoteStorage
    {
        private static string connectionString;
        private static int minCountToPack;
        private static int maxCountToPack;

        private readonly Dictionary<string, int> currencyIDs = new Dictionary<string, int>();

        /// <summary>
        /// время последнего сохранения котировки по валютной паре
        /// для прореживания
        /// </summary>
        private static readonly Dictionary<string, QuoteValue> lastSavedQuotes = new Dictionary<string, QuoteValue>();
        
        /// <summary>
        /// даты самых старых котировок по конкретным тикерам в БД
        /// </summary>
        private readonly Dictionary<string, DateSpan>
            tickerHistoryStartEnd = new Dictionary<string, DateSpan>();
        
        private static QuoteStorageManager instance;
        public static QuoteStorageManager Instance
        {
            get
            {
                try
                {
                    return instance ?? (instance = new QuoteStorageManager());
                }
                catch (Exception ex)
                {
                    Logger.Error("QuoteStorageManager ctor(out)", ex);
                    throw;
                }                
            }
        }

        private QuoteStorageManager()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
                minCountToPack = AppConfig.GetIntParam("Packing.MinCount", 300);
                maxCountToPack = AppConfig.GetIntParam("Packing.MaxCount", int.MaxValue);
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteStorageManager ctor()", ex);
                throw;
            }

            try
            {
                var curNames = DalSpot.Instance.GetTickerNames();
                foreach (var name in curNames)
                    currencyIDs.Add(name, DalSpot.Instance.GetFXICodeBySymbol(name));
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteStorageManager ctor", ex);
                currencyIDs = new Dictionary<string, int> { {"EURUSD", 1}, {"GBPUSD", 2} };
            }
            try
            {
                FillTickerHistoryStart();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в FillTickerHistoryStart()", ex);
            }
        }

        #region IQuoteStorage

        public PackedCandleStream GetMinuteCandlesPacked(string symbol, DateTime start, DateTime end)
        {
            var candles = GetMinuteCandlesUnpacked(symbol, start, end);
            if (candles == null || candles.Count == 0) return null;
            return new PackedCandleStream(candles, minCountToPack, maxCountToPack);
        }

        public Dictionary<string, DateSpan> GetTickersHistoryStarts()
        {
            var dic = tickerHistoryStartEnd.ToDictionary(pair => pair.Key, pair => new DateSpan(pair.Value));
            // обновить
            foreach (var quote in lastSavedQuotes)
            {
                if (dic.ContainsKey(quote.Key))
                    dic[quote.Key].end = quote.Value.time;                
            }
            return dic;
        }

        public Dictionary<string, QuoteData> GetQuoteData()
        {
            return SafeQuoteStorage.Instance.GetQuotes();
        }

        public PackedCandleStream GetMinuteCandlesPackedFast(string symbol, List<Cortege2<DateTime, DateTime>> intervals)
        {
            var result = new List<CandleDataPacked>();
            foreach (var interval in intervals)
            {
                var candles = GetMinuteCandlesUnpacked(symbol, interval.a, interval.b);
                if (candles == null)
                    continue;
                result.AddRange(candles);
            }
            return new PackedCandleStream(result, minCountToPack, maxCountToPack);
        }

        #endregion

        public Dictionary<int, float> GetLastPriceByTicker()
        {
            var prices = new Dictionary<int, float>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("select q1.ticker, q1.[open] from QUOTE q1 " +
                        "join (select ticker, max(date) as maxDate from QUOTE group by ticker) as q2 " +
                        "on q1.ticker = q2.ticker and q1.date = q2.maxDate order by q1.ticker",
                                             connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ticker = (short)reader[0];
                            var price = (float)(double)reader[1];

                            prices.Add(ticker, price);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetLastPriceByTicker()", ex);
                throw;
            }

            return prices;
        }

        private static List<CandleDataPacked> GetMinuteCandlesUnpacked(string symbol,
            DateTime start, DateTime end)
        {
            var tickerCode = DalSpot.Instance.GetFXICodeBySymbol(symbol);
            if (tickerCode == 0)
            {
                Logger.ErrorFormat("Ошибка в GetMinuteCandlesUnpacked: инструмент не определен: {0}", symbol);
                return null;
            }
            
            var values = new object[3];
            var candles = new List<CandleDataPacked>();

            try
            {            
                using (var connection = new SqlConnection(connectionString))
                {
                    var commandSql = string.Format("SELECT date, [open], HLC FROM QUOTE " +
                        "WHERE ticker={0} AND date BETWEEN '{1:yyMMdd HH:mm}' " +
                        "AND '{2:yyMMdd HH:mm}'", tickerCode, start, end);
                    var command = new SqlCommand(commandSql) { Connection = connection };
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        CandleDataPacked previousCandle = null;
                        while (reader.Read())
                        {
                            if (reader.GetValues(values) != values.Length) continue;
                            var candle = new CandleDataPacked
                                {
                                    timeOpen = (DateTime) values[0],
                                    open = (float) (double) values[1],
                                    HLC = (int) values[2]
                                };
                            candle.close = candle.open; // default
                            if (previousCandle != null)
                                previousCandle.close = candle.open;
                            previousCandle = candle;
                            candles.Add(candle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetMinuteCandlesUnpacked({0}, {1:dd.MM.yyyy HH:mm}, " +
                    "{2:dd.MM.yyyy HH:mm}) : {3}", symbol, start, end, ex);
                return null;
            }
            return candles;
        }

        /// <summary>
        /// залезть в БД и выдрать самую старую котиру по каждому тикеру
        /// </summary>
        private void FillTickerHistoryStart()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("select ticker, min(date), max(date) from QUOTE group by ticker",
                                         connection);

                var tickerRange = new Dictionary<int, DateSpan>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var ticker = (short) reader[0];
                        if (tickerRange.ContainsKey(ticker)) continue;
                        tickerRange.Add(ticker, new DateSpan((DateTime)reader[1], (DateTime)reader[2]));
                    }
                }

                foreach (var pair in tickerRange)
                {
                    var symbol = DalSpot.Instance.GetSymbolByFXICode(pair.Key);
                    if (string.IsNullOrEmpty(symbol)) continue;
                    if (tickerHistoryStartEnd.ContainsKey(symbol)) continue;
                    tickerHistoryStartEnd.Add(symbol, pair.Value);
                }
            }
        }
    }

    class QuoteValue
    {
        public DateTime time;
        private float sumQuote;
        private int quoteCount = 1;
        public float LastQuote
        {
            set
            {
                sumQuote += value;
                quoteCount++;
            }
        }
        public float AvgQuote
        {
            get
            {
                return sumQuote / quoteCount;
            }
        }
        public QuoteValue(DateTime time, float quote)
        {
            this.time = time;
            sumQuote = quote;
        }
    }
}
