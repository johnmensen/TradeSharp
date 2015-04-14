using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.Report
{
    /// <summary>
    /// возвращает все записи, получая их от TradeSharp.SiteBridge,
    /// обновляет при устаревании
    /// </summary>
    public static class PerformerStatRecordStorage
    {
        private static readonly ThreadSafeList<PerformerStatRecord> recordsSafe = new ThreadSafeList<PerformerStatRecord>();

        private static readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private const int RecordsUpdateIntervalSeconds = 60 * 60;

        private const int TimeoutIntervalMils = 1000;

        public static List<PerformerStatRecord> GetRecords()
        {
            var timeSinceUpdate = lastUpdated.GetLastHitIfHitted();
            if (timeSinceUpdate.HasValue)
            {
                var deltaSec = (DateTime.Now - timeSinceUpdate.Value).TotalSeconds;
                bool isTimeout;
                if (deltaSec < RecordsUpdateIntervalSeconds)
                    return recordsSafe.GetAll(TimeoutIntervalMils, out isTimeout);
            }

            var records = new List<PerformerStatRecord>();

            // обратиться к сервису TradeSharp.SiteBridge
            try
            {
                try
                {
                    var performers = TradeSharpAccountStatistics.Instance.proxy.GetAllPerformers(false) ?? new List<PerformerStat>();
                    var recs = performers.Select(p => new PerformerStatRecord(p)).ToList();
                    recordsSafe.ReplaceRange(recs, TimeoutIntervalMils);
                    return recs;
                }
                catch (Exception ex)
                {
                    //Logger.Error("Невозможно получить информацию о подписчиках - необходимо обновить файл конфигурации сайта", ex);
                    Logger.Error("GetPerformers() error", ex);
                    return records;
                }            
            }
            finally
            {
                lastUpdated.Touch();                
            }
        }
    }
}