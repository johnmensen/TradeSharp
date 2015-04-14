using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteService.ModuleControl;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.QuoteStorage
{
    /// <summary>
    /// потокобезопасный - принимает котировки, кладет их в список,
    /// из списка по таймеру выхватывает и сохраняет в БД
    /// </summary>
    class CandleStoreStream
    {
        #region Класс свечки m1
        class CandleForDb
        {
            public int ticker;
            public DateTime time;
            public float open;
            public int HLC;

            public CandleForDb() {}

            public CandleForDb(int ticker, int pointCost, CandleData candle)
            {
                this.ticker = ticker;
                time = candle.timeOpen;
                open = candle.open;
                HLC = candle.GetHlcOffset16(pointCost);
            }
        }
        #endregion

        #region Singleton

        private static readonly Lazy<CandleStoreStream> instance =
            new Lazy<CandleStoreStream>(() => new CandleStoreStream());
        
        public static CandleStoreStream Instance
        {
            get { return instance.Value; }
        }
        
        private CandleStoreStream()
        {
            foreach (var tickerName in DalSpot.Instance.GetTickerNames())
            {
                var code = DalSpot.Instance.GetFXICodeBySymbol(tickerName);
                if (code == 0) continue;
                var cost = DalSpot.Instance.GetPrecision10(tickerName);
                pointCostByTicker.Add(code, cost);
            }
            
            storeInterval = AppConfig.GetIntParam("DB.StoreInterval", 300);
            connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            lastQuoteTime.Touch();
        }

        #endregion

        #region Данные
        private readonly string connectionString;
        
        private readonly ThreadSafeQueue<CandleForDb> queueCandles =
            new ThreadSafeQueue<CandleForDb>();

        private const int QueueTimeout = 500;
        private volatile int queueLength;
        private const int MaxQueueLength = 1000;

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);
        private const int LogMagicQueueFull = 1;
        private const int LogMagicQueueTimeout = 2;
        private const int LogMagicErrorOpenCon = 3;
        private const int LogMagicErrorInsert = 4;
        private const int LogMagicErrorStore = 5;
        private const int LogMagicCantGetPointsCost = 6;
            
        private volatile bool isStopping;
        private Thread threadStoreQuotes;
        private readonly int storeInterval;
        private readonly ThreadSafeTimeStamp lastQuoteTime = new ThreadSafeTimeStamp();

        private readonly Dictionary<int, int> pointCostByTicker = new Dictionary<int, int>();

        private const string TableNameCandles = "QUOTE";
        #endregion

        #region Открытые методы
        
        /// <summary>
        /// thread-safe
        /// </summary>
        public void EnqueueCandles(List<Cortege2<int, CandleData>> quotes)
        {
            if (queueLength > MaxQueueLength)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMagicQueueFull, 60 * 1000 * 10,
                    "CandleStoreStream: очередь котировок на запись переполнена ({0})",
                    queueLength);
                return;
            }

            var candles = new List<CandleForDb>();
            foreach (var quote in quotes)
            {
                int pointCost;
                if (!pointCostByTicker.TryGetValue(quote.a, out pointCost))
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        LogMagicCantGetPointsCost, 1000 * 30,
                        "CandleStoreStream: cannot get point's cost ({0})", quote.a);
                    continue;
                }
                candles.Add(new CandleForDb(quote.a, pointCost, quote.b));
            }

            if (queueCandles.InQueue(candles, QueueTimeout))
                queueLength += quotes.Count;
        }

        public void Start()
        {
            isStopping = false;
            threadStoreQuotes = new Thread(StoreRoutine);
            threadStoreQuotes.Start();
        }

        public void Stop()
        {
            if (threadStoreQuotes == null) return;
            isStopping = true;
            threadStoreQuotes.Join();
        }

        #endregion

        #region Закрытые методы
        private void StoreRoutine()
        {
            const int iterationLen = 100;
            int nIters = storeInterval / iterationLen;
            int curIter = 0;

            while (!isStopping)
            {
                curIter++;
                if (curIter <= nIters)
                {
                    Thread.Sleep(iterationLen);
                    continue;
                }
                curIter = 0;
                
                // сохранить котировки из списка
                bool isTimeout;
                var candles = queueCandles.ExtractAll(QueueTimeout, out isTimeout);
                if (isTimeout)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        LogMagicQueueTimeout, 60 * 1000 * 10,
                        "QuoteStoreStream: таймаут чтения очереди ({0}), длина очереди {1}",
                        QueueTimeout, queueLength);
                    continue;
                }
                if (candles.Count == 0)
                {
                    var lastTime = lastQuoteTime.GetLastHit();
                    if ((DateTime.Now - lastTime).TotalMilliseconds > ModuleStatusController.MaxMillsBetweenQuotes)
                        ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasCriticalErrors,
                            "quote stream", DateTime.Now, "QUOTE");
                    continue;
                }
                lastQuoteTime.Touch();
                ModuleStatusController.Instance.Status.RemoveError(ServiceProcessState.HasCriticalErrors,
                                                                                 "QUOTE");
                queueLength = 0;

                // собственно сохранить
                DoSaveCandles(candles);
            }
        }
        
        private void DoSaveCandles(List<CandleForDb> candles)
        {
            if (candles.Count == 0) return;
            const int maxCommands = 50;

            // установить соединение с БД и сформировать строку - SQL с выражениями "insert... TableNameCandles
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMagicErrorOpenCon,
                            1000 * 60 * 30, "CandleStoreStream::DoSaveCandles() error opening connection: {0}", ex);
                        return;
                    }
                
                    for (var i = 0; i < candles.Count; i += maxCommands)
                    {
                        var endIndex = i + maxCommands;
                        if (endIndex >= candles.Count)
                            endIndex = candles.Count - 1;


                        var sqlInsert = new StringBuilder();
                        for (var j = i; j <= endIndex; j++)
                        {
                            var candle = candles[j];
                            sqlInsert.AppendLine(string.Format("INSERT INTO {0} (date, ticker, [open], HLC) VALUES (" +
                                                               "'{1:yyyyMMdd HH:mm}', {2}, {3}, {4})",
                                                               TableNameCandles,
                                                               candle.time, candle.ticker,
                                                               candle.open.ToStringUniformPriceFormat(true), candle.HLC));
                            var cmd = new SqlCommand(sqlInsert.ToString()) { Connection = connection };
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMagicErrorInsert,
                                    1000 * 60 * 30, "CandleStoreStream::DoSaveCandles() error inserting: {0}", ex);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMagicErrorStore,
                                    1000 * 60 * 30, "CandleStoreStream::DoSaveCandles() error storeing: {0}", ex);
            }
        }
        #endregion
    }
}
