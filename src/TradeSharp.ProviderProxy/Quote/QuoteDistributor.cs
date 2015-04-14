using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TradeSharp.ProviderProxy.BL;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.Quote
{
    /// <summary>
    /// Сохраняет котировки для последующей раздачи
    /// Немедленной или с минимальным указанным интервалом
    /// Потокобезопасный
    /// Выдает подписантам строки вида EURUSD,1.3205,1.3206;GBPUSD,1.4750,1.4752;
    /// </summary>
    class QuoteDistributor
    {
        private static QuoteDistributor instance;
        public static QuoteDistributor Instance
        {
            get
            {
                if (instance == null) instance = new QuoteDistributor();
                return instance;
            }
        }
        private readonly TcpDistributor tcpDistributor;
        private readonly int minDistributeIntervalMil = 200;
        private readonly Dictionary<string, PartialQuote> quotes = new Dictionary<string, PartialQuote>();
        private readonly ReaderWriterLock lockerQuotes = new ReaderWriterLock();
        private const int lockTimeout = 200;
        private volatile bool isStopping;
        private Thread threadDistr;
        private readonly bool shouldDistribute = AppConfig.GetBooleanParam("Quote.ShouldDistribute", false);
        private readonly float maxQuoteDeltaPercent = AppConfig.GetStringParam("Quote.MaxDeltaPercent", "8").ToFloatUniform();
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);
        private const int LogMsgWrongQuote = 1;
        private const int LogMsgQuotesDistributed = 2;


        private QuoteDistributor()
        {
            if (!shouldDistribute) return;
            try
            {
                tcpDistributor = new TcpDistributor(AppConfig.GetIntParam("Quote.DistributionOwnPort", 2780));
                tcpDistributor.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в QuoteDistributor() ctor", ex);
                throw;
            }
            
            minDistributeIntervalMil = AppConfig.GetIntParam("Quote.MinDistributeIntervalMil", 200);
        }

        public void UpdateQuote(string symbol, float? bid, float? ask)
        {
            if (!shouldDistribute) return;
            if (!bid.HasValue && ask.HasValue == false) return;

            symbol = TickerCodeDictionary.Instance.GetClearTickerName(symbol);
            try
            {
                lockerQuotes.AcquireWriterLock(lockTimeout);
            }
            catch (ApplicationException)
            {
                return;
            }
            try
            {
                FilterAndUpdateQuotes(bid, ask, symbol);
            }
            finally
            {
                lockerQuotes.ReleaseWriterLock();
            }
        }

        private void FilterAndUpdateQuotes(float? bid, float? ask, string symbol)
        {
            PartialQuote quote;
            quotes.TryGetValue(symbol, out quote);
            if (quote != null)
            {
                if (ask.HasValue)
                {
                    var delta = quote.Ask.HasValue
                                    ? Math.Abs(100*(ask.Value - quote.Ask.Value)/quote.Ask.Value)
                                    : 0;
                    if (delta > maxQuoteDeltaPercent)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogMsgWrongQuote, 1000 * 60 * 15,
                            "Неверная котировка {0}: ask={1:f4}, старое значение {2:f4}",
                            symbol, ask.Value, quote.Ask ?? 0);
                    }
                    else
                        quote.Ask = ask;
                }
                if (bid.HasValue)
                {
                    var delta = quote.Bid.HasValue
                                    ? Math.Abs(100 * (bid.Value - quote.Bid.Value) / quote.Bid.Value)
                                    : 0;
                    if (delta > maxQuoteDeltaPercent)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogMsgWrongQuote, 1000 * 60 * 15,
                            "Неверная котировка {0}: bid={1:f4}, старое значение {2:f4}",
                            symbol, bid.Value, quote.Bid ?? 0);
                    }
                    else
                        quote.Bid = bid;
                }
                return;
            }
            quote = new PartialQuote(ask, bid);
            quotes.Add(symbol, quote);
        }

        public void StartDistribution()
        {
            if (!shouldDistribute) return;
            threadDistr = new Thread(DistributeLoop);
            threadDistr.Start();
        }
        
        public void StopDistribution()
        {
            if (!shouldDistribute) return;
            isStopping = true;
            if (threadDistr != null)
                threadDistr.Join();
        }

        private void DistributeLoop()
        {
            while (!isStopping)
            {
                var sb = new StringBuilder();
                try
                {
                    lockerQuotes.AcquireReaderLock(lockTimeout);
                }
                catch (ApplicationException)
                {
                    continue;
                }
                try
                {
                    foreach (var pair in quotes)
                    {
                        if (!pair.Value.IsComplete || pair.Value.isUpdated == false) continue;
                        sb.AppendFormat("{0},{1},{2};", pair.Key,
                                        pair.Value.Bid.Value.ToStringUniform(),
                                        pair.Value.Ask.Value.ToStringUniform());
                        pair.Value.isUpdated = false;
                    }
                }
                finally
                {
                    lockerQuotes.ReleaseReaderLock();
                }

                if (sb.Length > 0)
                {
                    var quoteStr = sb.ToString();
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgQuotesDistributed, 1000 * 60 * 60,
                        "Раздаются котировки: " + quoteStr);
                    //Logger.InfoFormat("QuoteDistributor: distributing {0} bytes quotes", sb.Length);
                    tcpDistributor.DistributeStringData(quoteStr);                    
                }

                Thread.Sleep(minDistributeIntervalMil);
            }
        }
    }

    class PartialQuote
    {
        private float? ask, bid;
        public bool isUpdated;

        public float? Ask
        {
            get { return ask; }
            set
            {
                if (!value.HasValue) return;
                ask = value;
                isUpdated = true;
            }
        }

        public float? Bid
        {
            get { return bid; }
            set
            {
                if (!value.HasValue) return;
                bid = value;
                isUpdated = true;
            }
        }

        public bool IsComplete
        {
            get { return ask.HasValue && bid.HasValue; }
        }

        public PartialQuote(float? _ask, float? _bid)
        {
            ask = _ask;
            bid = _bid;
            isUpdated = ask.HasValue || bid.HasValue;
        }
        
    }
}
