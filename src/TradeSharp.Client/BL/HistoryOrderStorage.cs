using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// потокобезопасное хранилище отложенных ордеров
    /// само следит за актуальностью ордеров
    /// </summary>
    class HistoryOrderStorage
    {
        #region Singletone

        private static readonly Lazy<HistoryOrderStorage> instance =
            new Lazy<HistoryOrderStorage>(() => new HistoryOrderStorage());

        public static HistoryOrderStorage Instance
        {
            get { return instance.Value; }
        }

        private HistoryOrderStorage()
        {
            nextUpdateTime.SetTime(DateTime.Now.AddYears(1));
            updateThread = new Thread(UpdateLoop);
            updateThread.Start();
        }

        #endregion

        #region Данные
        private readonly ThreadSafeTimeStamp nextUpdateTime = new ThreadSafeTimeStamp();

        private const int DefaultUpdateIntervalSec = 60 * 60 * 24;
        
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        private List<MarketOrder> orders = new List<MarketOrder>();

        private Action<List<MarketOrder>> ordersUpdated;

        public event Action<List<MarketOrder>> OrdersUpdated
        {
            add { ordersUpdated += value; }
            remove { ordersUpdated -= value; }
        }

        private const int ThreadTimeout = 250;

        private readonly Thread updateThread;

        private volatile bool isStopping;
        #endregion

        public void StopUpdating()
        {
            isStopping = true;
            if (!updateThread.Join(2000))
                updateThread.Abort();
        }

        public List<MarketOrder> GetOrders()
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return new List<MarketOrder>();
            try
            {
                return orders.ToList();
            }
            finally
            {                
                locker.ExitReadLock();
            }
        }
    
        /// <summary>
        /// подвинуть nextUpdateTime чуть назад, так, чтобы след. обновление произошло не позднее,
        /// чем через maxMillisecondsDelay милисекунд 
        /// </summary>
        public void HurryUpUpdateMaxDelay(int maxMillisecondsDelay)
        {
            var delay = (nextUpdateTime.GetLastHit() - DateTime.Now).TotalMilliseconds;
            if (delay > maxMillisecondsDelay)
                nextUpdateTime.SetTime(DateTime.Now.AddMilliseconds(maxMillisecondsDelay));
        }

        /// <summary>
        /// перегрузка HurryUpUpdateMaxDelay
        /// </summary>
        public void HurryUpUpdate()
        {
            HurryUpUpdateMaxDelay(2000);
        }

        /// <summary>
        /// строго установить nextUpdateTime на n милисекунд вперед
        /// </summary>
        public void PlanUpdate(int maxMillisecondsDelay = 2000)
        {
            nextUpdateTime.SetTime(DateTime.Now.AddMilliseconds(maxMillisecondsDelay));
        }

        /// <summary>
        /// прочитать ордера на сервере и раздать получателям этого события
        /// </summary>
        private void UpdateOrders()
        {
            if (!AccountStatus.Instance.isAccountSelected) return;

            List<MarketOrder> serverOrders;
            try
            {
                var status = TradeSharpAccount.Instance.GetHistoryOrdersUncompressed(AccountStatus.Instance.accountID, null, out serverOrders);
                if (status != RequestStatus.OK) return;
            }
            catch (Exception ex)
            {
                Logger.Error("HistoryOrderStorage - ошибка получения ордеров", ex);
                return;
            }
            if (orders == null) return;

            if (!locker.TryEnterWriteLock(LockTimeout)) return;
            try
            {
                orders = serverOrders == null ? new List<MarketOrder>() : serverOrders.ToList();
            }
            finally
            {
                locker.ExitWriteLock();
            }

            try
            {
                if (ordersUpdated != null) ordersUpdated(serverOrders);
            }
            catch (Exception ex)
            {
                // Logger. 
            }
        }
    
        private void UpdateLoop()
        {
            try
            {
                while (!isStopping)
                {
                    var upTime = nextUpdateTime.GetLastHit();
                    if (DateTime.Now >= upTime)
                    {
                        UpdateOrders();
                        if (nextUpdateTime.GetLastHit() <= DateTime.Now)
                            nextUpdateTime.SetTime(DateTime.Now.AddSeconds(DefaultUpdateIntervalSec));
                    }
                    Thread.Sleep(ThreadTimeout);
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }
    }
}
