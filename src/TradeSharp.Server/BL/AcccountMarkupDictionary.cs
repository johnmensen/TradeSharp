using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Server.Contract;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// хранит записи GroupMarkup, следит за их актуальностью,
    /// обновляет (потокобезопасно)
    /// </summary>
    static class AcccountMarkupDictionary
    {
        private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        private static readonly ThreadSafeTimeStamp lastTimeDicIsUpdated = new ThreadSafeTimeStamp();

        private const int MaxMilsBetweenUpdate = 1000 * 20;

        private static Dictionary<string, GroupMarkup> markups = new Dictionary<string, GroupMarkup>();

        public static float GetMarkupAbs(string groupCode, string ticker, out AccountGroup.MarkupType markType)
        {
            //Logger.InfoFormat("Markup: GetMarkupAbs({0}, {1})", groupCode, ticker);
            markType = AccountGroup.MarkupType.NoMarkup;
            // пришло время обновить словарь?
            var lastTimeUpdate = lastTimeDicIsUpdated.GetLastHitIfHitted();
            var milsSinceUpdate = lastTimeUpdate == null
                                      ? int.MaxValue
                                      : (DateTime.Now - lastTimeUpdate.Value).TotalMilliseconds;
            if (milsSinceUpdate > MaxMilsBetweenUpdate)
            {
                //Logger.InfoFormat("Markup: Updating dic");
                // таки обновить словарь
                var dicMarkup = DictionaryManager.Instance.GetMarkupByGroup();
                //Logger.InfoFormat("Markup: Dic is updated - {0} records", dicMarkup.Count);
                if (!locker.TryEnterWriteLock(LockTimeout))
                {
                    Logger.ErrorFormat("AcccountMarkupDictionary - write timeout ({0} ms)", LockTimeout);
                }
                else
                {
                    markups = dicMarkup.ToDictionary(m => m.GroupCode, m => m);
                    locker.ExitWriteLock();
                }                
            }

            GroupMarkup mark;
            // безопасно прочитать данные
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.ErrorFormat("AcccountMarkupDictionary - read timeout ({0} ms)", LockTimeout);
                return 0;
            }
            try
            {
                markups.TryGetValue(groupCode, out mark);
            }
            finally
            {
                locker.ExitReadLock();
            }

            if (mark == null) return 0;
            //Logger.InfoFormat("Markup: got value ({0}, {1})", mark.DefaultSpread, mark.MarkupType);
            if (mark.MarkupType == AccountGroup.MarkupType.NoMarkup) return 0;
            markType = mark.MarkupType;

            float spread;
            if (mark.spreadByTicker.TryGetValue(ticker, out spread)) 
                return spread;

            // посчитать спред из пунктов
            return mark.DefaultSpread == 0
                       ? 0
                       : DalSpot.Instance.GetAbsValue(ticker, mark.DefaultSpread);
        }
    }
}
