using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    /// <summary>
    /// хранит информацию о доступной на сервере истории котировок:
    /// - время первой записи по каждому тикеру,
    /// - дырки на сервере
    /// - время последнего обновления истории по тикеру
    /// 
    /// "помнит", когда информация обновлялась последний раз
    /// освежает данные
    /// </summary>
    public class ServerQuoteHistory
    {
        private readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private readonly IQuoteStorage quoteStorage;

        /// <summary>
        /// время последнего обновления истории по тикеру
        /// </summary>
        private readonly Dictionary<string, DateTime> tickerUpdateTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// время первой записи в БД котировок по тикеру
        /// </summary>
        private Dictionary<string, DateSpan> tickerFirstRecord;

        private const int MinutesToRefreshTickerHistory = 60 * 12;

        public ServerQuoteHistory(IQuoteStorage quoteStorage)
        {
            this.quoteStorage = quoteStorage;
        }

        public bool IsFileActual(string ticker)
        {
            DateTime lastTime;
            if (!tickerUpdateTime.TryGetValue(ticker, out lastTime))
                return false;
            return (DateTime.Now - lastTime).TotalMinutes < MinutesToRefreshTickerHistory;
        }

        public DateTime GetServerTickerHistoryStart(string ticker)
        {
            var span = GetServerTickerHistorySpan(ticker);
            return span == null
                ? DateTime.Now.Date.AddDays(-355) 
                : span.start;
        }

        public DateSpan GetServerTickerHistorySpan(string ticker)
        {
            var lastUpTime = lastUpdated.GetLastHitIfHitted();
            var needUpdate = !lastUpTime.HasValue || (DateTime.Now - lastUpTime.Value).TotalMinutes >= MinutesToRefreshTickerHistory;
            if (needUpdate)
            {
                try
                {
                    tickerFirstRecord = quoteStorage.GetTickersHistoryStarts();
                }
                catch (Exception ex)
                {
                    tickerFirstRecord = new Dictionary<string, DateSpan>();
                    Logger.Error("GetServerTickerHistoryStart() - ошибка получения истории", ex);
                }
            }
            DateSpan span;
            tickerFirstRecord.TryGetValue(ticker, out span);
            return span;
        }
    
        public void UpdateTickerTime(string ticker)
        {
            if (tickerUpdateTime.ContainsKey(ticker))
                tickerUpdateTime[ticker] = DateTime.Now;
            else
                tickerUpdateTime.Add(ticker, DateTime.Now);
        }
    }
}
