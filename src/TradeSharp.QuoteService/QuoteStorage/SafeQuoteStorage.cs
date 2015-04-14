using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Contract.Entity;

namespace TradeSharp.QuoteService.QuoteStorage
{
    class SafeQuoteStorage
    {
        private static SafeQuoteStorage instance;

        public static SafeQuoteStorage Instance
        {
            get { return instance ?? (instance = new SafeQuoteStorage()); }
        }

        private readonly Dictionary<string, QuoteData> quotes = new Dictionary<string, QuoteData>(); 

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        public void UpdateQuotes(List<TickerQuoteData> newQuotes)
        {
            if (!locker.TryEnterWriteLock(LockTimeout)) return;
            try
            {
                foreach (var q in newQuotes)
                {
                    if (quotes.ContainsKey(q.Ticker))
                        quotes[q.Ticker] = new QuoteData(q.bid, q.ask, q.time);
                    else                    
                        quotes.Add(q.Ticker, new QuoteData(q.bid, q.ask, q.time));
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public Dictionary<string, QuoteData> GetQuotes()
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return new Dictionary<string, QuoteData>();
            try
            {
                return quotes.ToDictionary(q => q.Key, q => q.Value);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}
