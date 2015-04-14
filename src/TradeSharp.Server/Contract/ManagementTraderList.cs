using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    /// <summary>
    /// список акаунтов, с которых рассылаются сигналы для автоматической торговли
    /// актуализируется раз в N секунд
    /// </summary>
    class ManagementTraderList
    {
        private static ManagementTraderList instance;

        public static ManagementTraderList Instance
        {
            get { return instance ?? (instance = new ManagementTraderList()); }
        }

        /// <summary>
        /// account id - signal cat id
        /// </summary>
        private readonly Dictionary<int, int> traderAccounts = new Dictionary<int, int>();

        private readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private const int MilsToUpdateTraders = 1000 * 4;

        private readonly ReaderWriterLock locker = new ReaderWriterLock();

        private const int LockTimeout = 2000;

        private bool firstUpdate = true;

        private ManagementTraderList()
        {
        }

        /// <summary>
        /// возвращает Id категории сигналов или null
        /// </summary>        
        public int? IsSignaller(int acId)
        {
            var lastReq = lastUpdated.GetLastHitIfHitted();
            var milsSinceRequest = lastReq.HasValue
                                       ? (DateTime.Now - lastReq.Value).TotalMilliseconds
                                       : int.MaxValue;
            if (milsSinceRequest > MilsToUpdateTraders)
                UpdateSignallers();
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return null;
            }
            try
            {
                int catId;
                return (traderAccounts.TryGetValue(acId, out catId)) ? catId : (int?)null;                
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
        }

        private void UpdateSignallers()
        {
            var dicSignaller = new Dictionary<int, int>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var accounts = ctx.SERVICE.Where(a => a.AccountId.HasValue).ToList();
                    foreach (var ac in accounts)
                    {
                        // ReSharper disable PossibleInvalidOperationException
                        if (!dicSignaller.ContainsKey(ac.AccountId.Value))                        
                        // ReSharper restore PossibleInvalidOperationException
                            dicSignaller.Add(ac.AccountId.Value, ac.ID);                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ManagementTraderList - UpdateSignallers - ошибка DAL", ex);
                return;
            }

            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return;
            }
            try
            {
                traderAccounts.Clear();
                foreach (var ac in dicSignaller)
                    traderAccounts.Add(ac.Key, ac.Value);                                
                lastUpdated.Touch();
            }
            catch (Exception ex)
            {
                Logger.Error("ManagementTraderList - ошибка DAL", ex);
            }
            finally
            {
                locker.ReleaseWriterLock();
            }

            if (firstUpdate)
            {
                firstUpdate = false;
                Logger.Info("Signallers are users: " + string.Join(", ", dicSignaller.Keys));
            }
        }
    }
}
