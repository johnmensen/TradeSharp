using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// используя TcpReceiverClient, подключается к серверу, и,
    /// получив котировки, обновляет хеш
    /// 
    /// порождает дополнительный поток, в котором вызываются обработчики
    /// на получение котировок и новостей, чтобы не тормозить прием котировок
    /// </summary>
    public class TcpQuoteReceiver
    {
        private String tail = String.Empty;
        private TcpReceiverClient client;

        private readonly Thread processQuotesThread;
        private readonly Thread processNewsThread;
        private Dictionary<string, QuoteData> tickersToDeliver = new Dictionary<string, QuoteData>();
        private List<News> newsToDeliver = new List<News>();
        private ReaderWriterLock 
            lockerTickers = new ReaderWriterLock(), 
            lockerNews = new ReaderWriterLock();
        private volatile bool isStopping;
        /// <summary>
        /// таймаут проверяется в ProcessQuotesLoop
        /// </summary>
        private bool reconnectOnTimeout = AppConfig.GetBooleanParam("QuoteStream.ShouldReconnect", true);
        private int timeoutIntervalMils = AppConfig.GetIntParam("QuoteStream.ReconnectTimoutMils", 10 * 1000);
        private readonly ThreadSafeTimeStamp lastMessageReceived = new ThreadSafeTimeStamp();

        public TcpQuoteReceiver()
        {
            client = new TcpReceiverClient(AppConfig.GetStringParam("Quote.ServerHost", "70.38.11.49"),
                AppConfig.GetIntParam("Quote.ServerPort", 55056));
            client.OnTcpPacketReceived += OnTcpPacketReceived;
            client.Connect();
            processQuotesThread = new Thread(ProcessQuotesLoop);
            processNewsThread = new Thread(ProcessNewsLoop);
            lastMessageReceived.Touch();
            processQuotesThread.Start();
            processNewsThread.Start();
        }

        private QuotesReceivedDel onQuotesReceived;
        public event QuotesReceivedDel OnQuotesReceived
        {
            add { onQuotesReceived += value; }
            remove { onQuotesReceived -= value; }
        }

        private NewsReceivedDel onNewsReceived;
        public event NewsReceivedDel OnNewsReceived
        {
            add { onNewsReceived += value; }
            remove { onNewsReceived -= value; }
        }

        private void OnTcpPacketReceived(string data)
        {
            lastMessageReceived.Touch();
            var str = tail + data;
            // смотрим - завершена ли строка разделителем?
            var indexSt = str.LastIndexOf(BaseNewsParser.MessagesSeparator[0]);
            if (indexSt < 0)
            {
                tail = str;
                return;
            }
            var isTerminated = str.EndsWith(BaseNewsParser.MessagesSeparator[0]);

            // прочитать все новости, оставшийся кусок новости записать в tail
            var parts = str.Split(BaseNewsParser.MessagesSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                tail = String.Empty;
                return;
            }

            var lastIndex = isTerminated ? parts.Length - 1 : parts.Length - 2;
            
            // разбираем список на котировки и новости и складываем их
            var news = new List<News>();
            var quotes = new List<TickerQuoteData>();
            for (var i = 0; i <= lastIndex; i++)
            {
                var item = BaseNewsParser.ParseItem(parts[i]);
                
                if (item != null)
                {
                    if (item is News)
                        news.Add((News)item);
                    else
                        if (item is TickerQuoteData)
                            quotes.Add((TickerQuoteData)item);
                }
            }
            AddNewsToDeliver(news);

            var quoteNames = quotes.Select(q => q.Ticker).ToArray();
            var quoteArray = quotes.ToArray();
            // обновить в хранилище котировок
            QuoteStorage.Instance.UpdateValues(quoteNames, quoteArray);

            AddTickersToDeliver(quotes);
            
            // сформировать хвост
            tail = isTerminated ? "" : parts[parts.Length - 1];
        }

        public void Stop()
        {
            client.Stop();
            isStopping = true;
            if (processQuotesThread != null) processQuotesThread.Join();
            if (processNewsThread != null) processNewsThread.Join();
        }

        private void ProcessQuotesLoop()
        {
            while (!isStopping)
            {                
                Thread.Sleep(200);
                if (reconnectOnTimeout) CheckTimeout();
                if (onQuotesReceived == null) continue;

                Dictionary<string, QuoteData> tickers;
                try
                {
                    lockerTickers.AcquireReaderLock(200);
                }
                catch
                {
                    continue;
                }
                try
                {
                    var countQuotes = tickersToDeliver.Count;
                    if (countQuotes == 0) continue;
                    try
                    {
                        lockerTickers.UpgradeToWriterLock(200);
                        tickers = tickersToDeliver;
                        tickersToDeliver = new Dictionary<string, QuoteData>();
                    }
                    catch
                    {
                        continue;
                    }
                }
                finally
                {
                    lockerTickers.ReleaseLock();
                }

                if (tickers == null || tickers.Count == 0) continue;
                // раздать котировки получателям
                onQuotesReceived(tickers.Keys.ToArray(), tickers.Values.ToArray());
            }
        }

        private void ProcessNewsLoop()
        {
            while (!isStopping)
            {
                Thread.Sleep(200);
                if (onNewsReceived == null) continue;

                List<News> news;
                try
                {
                    lockerNews.AcquireReaderLock(200);
                }
                catch
                {
                    continue;
                }
                try
                {
                    var countNews = newsToDeliver.Count;
                    if (countNews == 0) continue;
                    try
                    {
                        lockerNews.UpgradeToWriterLock(200);
                        news = newsToDeliver;
                        newsToDeliver = new List<News>();
                    }
                    catch
                    {
                        continue;
                    }
                }
                finally
                {
                    lockerNews.ReleaseLock();
                }

                if (news == null) continue;
                // раздать котировки получателям
                onNewsReceived(news.ToArray());
            }
        }

        private void AddTickersToDeliver(List<TickerQuoteData> tickers)
        {
            try
            {
                lockerTickers.AcquireWriterLock(200);
            }
            catch
            {
                return;
            }
            try
            {
                foreach (var ticker in tickers)
                {
                    if (tickersToDeliver.ContainsKey(ticker.Ticker))
                        tickersToDeliver[ticker.Ticker] = ticker;
                    else                   
                        tickersToDeliver.Add(ticker.Ticker, ticker);                    
                }
            }
            finally
            {
                lockerTickers.ReleaseWriterLock();
            }            
        }

        private void AddNewsToDeliver(List<News> news)
        {
            try
            {
                lockerNews.AcquireWriterLock(200);
            }
            catch
            {
                return;
            }
            try
            {                
                newsToDeliver.AddRange(news);
            }
            finally
            {
                lockerNews.ReleaseWriterLock();
            }
        }
    
        private void CheckTimeout()
        {
            // поток котировок может быть не включен в рабочий день
            if (!WorkingDay.Instance.IsWorkingDay(DateTime.Now)) return;
            var delta = (DateTime.Now - lastMessageReceived.GetLastHit()).TotalMilliseconds;
            if (delta < timeoutIntervalMils) return;
            return;
            
            // инициировать таймаут
            //client.Stop();
            //lastMessageReceived.Touch();
            //Logger.InfoFormat("TcpQuoteReceiver - таймаут, {0} секунд", (int)(delta / 1000));
            //client = new TcpReceiverClient(AppConfig.GetStringParam("Quote.ServerHost", "70.38.11.49"),
            //    AppConfig.GetIntParam("Quote.ServerPort", 55056));
            //client.OnTcpPacketReceived += OnTcpPacketReceived;
            //client.Connect();
        }
    }

    public delegate void QuotesReceivedDel(string[] names, QuoteData[] quotes);
    public delegate void NewsReceivedDel(News[] news);
}
