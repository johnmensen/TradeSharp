using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin.BL
{
    /// <summary>
    /// котировки заталкиваются в общий список, из него партиями
    /// пишутся в DB
    /// </summary>
    class DBStoreThread
    {
        private List<Dictionary<int, List<CandleData>>> quoteQueue;

        private readonly ReaderWriterLock lockQueue = new ReaderWriterLock();

        private const int LockTimeout = 1000;

        private readonly Thread storeThread;

        private volatile bool isStopping;

        private long candlesLeftInQueue;

        public long CandlesLeftInQueue
        {
            get { return Interlocked.Read(ref candlesLeftInQueue); }
            set { Interlocked.Exchange(ref candlesLeftInQueue, value); }
        }

        readonly private bool rewriteQuotes;

        public DBStoreThread(bool rewriteQuotes)
        {
            this.rewriteQuotes = rewriteQuotes;
            storeThread = new Thread(ThreadRoutine);
            storeThread.Start();
        }        

        public void Stop()
        {
            isStopping = true;
            storeThread.Join();
        }

        public void PushQuotes(Dictionary<int, List<CandleData>> quotes)
        {
            try
            {
                lockQueue.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("PushQuotes - write timeout");
                return;
            }

            try
            {
                if (quoteQueue == null) quoteQueue = new List<Dictionary<int, List<CandleData>>>();
                quoteQueue.Add(quotes);
            }
            finally
            {
                lockQueue.ReleaseWriterLock();
            }
        }
    
        private void ThreadRoutine()
        {
            while (!isStopping)
            {
                // получить котировки
                var quotes = DequeueAllQuotes();
                if (quotes == null || quotes.Count == 0) continue;

                // организовать котировки в один словарь
                var candlesByTicker = new Dictionary<int, List<CandleData>>();
                foreach (var dic in quotes)
                {
                    foreach (var pair in dic)
                    {
                        List<CandleData> targetList;
                        if (!candlesByTicker.TryGetValue(pair.Key, out targetList))
                        {
                            targetList = new List<CandleData>();
                            candlesByTicker.Add(pair.Key, targetList);
                        }
                        targetList.AddRange(pair.Value);
                    }
                    
                }

                CandlesLeftInQueue = candlesByTicker.Sum(p => p.Value.Count);

                // сохранить котировки по каждому тикеру
                foreach (var pair in candlesByTicker)
                {
                    var ticker = pair.Key;
                    var candles = pair.Value;
                    if (candles.Count == 0) continue;
                    QuoteDataBase.Instance.StoreQuotesByMerge(candles, ticker);
                    CandlesLeftInQueue -= candles.Count;
                }                
            }
        }

        private List<Dictionary<int, List<CandleData>>> DequeueAllQuotes()
        {
            try
            {
                lockQueue.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return null;
            }
            try
            {
                if (quoteQueue == null || quoteQueue.Count == 0) return null;
                lockQueue.UpgradeToWriterLock(LockTimeout);
                var quotes = quoteQueue;
                quoteQueue = null;
                return quotes;
            }
            finally
            {
                lockQueue.ReleaseLock();
            }
            return null;
        }        
    }
}
