using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// потокобезопасное хранилище рыночных ордеров с авто-обновлением, запускаемом
    /// в отдельном потоке
    /// 
    /// хранит флаг завершения последней операции чтения ордеров
    /// </summary>
    class MarketOrdersStorage
    {
        private static MarketOrdersStorage instance;
        public static MarketOrdersStorage Instance
        {
            get
            {
                return instance ?? (instance = new MarketOrdersStorage());
            }
        }

        private readonly int updateOrdersInterval;
        private readonly int sleepInterval;

        private readonly ReaderWriterLock lockOrders = new ReaderWriterLock();
        private readonly ReaderWriterLock lockPending = new ReaderWriterLock();
        
        private const int LockTimeout = 1000;
        private volatile bool isStopping;
        private readonly Thread pollOrdersThread;
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);
        private const int LogMsgRetrieveOrderError = 1;
        
        private int hashCodeOrders;
        private int hashCodePositions;

        private OrdersUpdatedDel marketOrdersUpdated;
        public event OrdersUpdatedDel MarketOrdersUpdated
        {
            add { marketOrdersUpdated += value; }
            remove { marketOrdersUpdated -= value; }
        }

        private OrdersUpdatedDel pendingOrdersUpdated;

        public event OrdersUpdatedDel PendingOrdersUpdated
        {
            add { pendingOrdersUpdated += value; }
            remove { pendingOrdersUpdated -= value; }
        }

        private volatile RequestStatus requestOrdersStatus = RequestStatus.NoConnection;
        public RequestStatus RequestOrderStatus
        {
            get { return requestOrdersStatus; }
            private set { requestOrdersStatus = value; }
        }

        private volatile RequestStatus requestPendingOrdersStatus = RequestStatus.NoConnection;
        public RequestStatus RequestPendingOrderStatus
        {
            get { return requestPendingOrdersStatus; }
            private set { requestPendingOrdersStatus = value; }
        }

        private List<MarketOrder> orders;
        public List<MarketOrder> MarketOrders
        {
            get
            {
                try
                {
                    lockOrders.AcquireReaderLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("MarketOrdersStorage - get orders timeout");
                    return null;
                }
                try
                {
                    return orders == null ? null : orders.ToList();
                }
                finally
                {
                    lockOrders.ReleaseReaderLock();
                }
            }
            private set
            {
                try
                {
                    lockOrders.AcquireWriterLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("MarketOrdersStorage - set orders timeout");
                    return;
                }
                try
                {
                    orders = value;                    
                }
                finally
                {
                    lockOrders.ReleaseWriterLock();
                }
            }
        }

        private List<PendingOrder> pendingOrders;
        public List<PendingOrder> PendingOrders
        {
            get
            {
                try
                {
                    lockPending.AcquireReaderLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("MarketOrdersStorage - get pendingOrders timeout");
                    return null;
                }
                try
                {
                    return pendingOrders == null ? null : pendingOrders.ToList();
                }
                finally
                {
                    lockPending.ReleaseReaderLock();
                }
            }
            private set
            {
                try
                {
                    lockPending.AcquireWriterLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("MarketOrdersStorage - set pendingOrders timeout");
                    return;
                }
                try
                {
                    pendingOrders = value;
                }
                finally
                {
                    lockPending.ReleaseWriterLock();
                }
            }
        }

        private MarketOrdersStorage()
        {
            updateOrdersInterval = AppConfig.GetIntParam("OrderStorage.UpdateInterval", 1000);
            if (updateOrdersInterval < 500)
                updateOrdersInterval = 500;
            pollOrdersThread = new Thread(UpdateRoutine);
            sleepInterval = updateOrdersInterval <= 100 ? updateOrdersInterval : 100;
        }

        public void Start()
        {
            isStopping = false;
            if (pollOrdersThread.IsAlive) return;
            pollOrdersThread.Start();
        }

        public void Stop()
        {
            const int wcfTimeout = 5000;

            if (!pollOrdersThread.IsAlive) return;
            isStopping = true;
            if (!pollOrdersThread.Join(wcfTimeout))
            {
                try
                {
                    MainForm.serverProxyTrade.AbortWCFCall();
                    pollOrdersThread.Join();
                }
                catch
                {
                }
            }
        }

        private void UpdateRoutine()
        {
            var milsSincePoll = 0;
            while (!isStopping)
            {
                Thread.Sleep(sleepInterval);
                milsSincePoll += sleepInterval;
                if (milsSincePoll < updateOrdersInterval) continue;
                milsSincePoll = 0;
                
                // обновить
                try
                {
                    if (!AccountStatus.Instance.isAccountSelected)
                        continue;
                    List<MarketOrder> marketOrders;
                    var reqStatus = TradeSharpAccount.Instance.proxy.GetMarketOrders(AccountStatus.Instance.accountID, out marketOrders);
                    
                    RequestOrderStatus = reqStatus;
                    if (marketOrders != null)
                    {
                        RecalcOpenResult(marketOrders);
                        MarketOrders = marketOrders;
                        var hashCode = (int)marketOrders.Sum(order => order.PriceEnter * 10003 + (order.TakeProfit ?? 0) * 5001 + 
                            (order.StopLoss ?? 0) * 5006 + (order.Swap ?? 0) * 3007);
                        if (hashCodePositions != hashCode)
                        {
                            hashCodePositions = hashCode;
                            if (marketOrdersUpdated != null)
                                marketOrdersUpdated();
                        }
                    }

                    List<PendingOrder> pOrders;
                    reqStatus = TradeSharpAccount.Instance.proxy.GetPendingOrders(AccountStatus.Instance.accountID, out pOrders);
                    
                    RequestPendingOrderStatus = reqStatus;
                    if (pOrders != null)
                    {
                        PendingOrders = pOrders;
                        var hashCode = (int)pOrders.Sum(order => order.PriceFrom * 15001 + (order.TakeProfit ?? 0) * 7503 + 
                            (order.StopLoss ?? 0) * 5007);
                        if (hashCode != hashCodeOrders)
                        {
                            hashCodeOrders = hashCode;
                            if (pendingOrdersUpdated != null) pendingOrdersUpdated();
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgRetrieveOrderError,
                                                          1000 * 60 * 5, 
                                                          "MarketOrdersStorage - ошибка получения ордеров {0}", ex.Message);
                    RequestOrderStatus = RequestStatus.NoConnection;
                    RequestPendingOrderStatus = RequestStatus.NoConnection;
                }
            }
        }

        /// <summary>
        /// пересчитать результат открытых позиций,
        /// затем скорректировать баланс в AccountStatus
        /// </summary>
        private static void RecalcOpenResult(List<MarketOrder> marketOrders)
        {
            var account = AccountStatus.Instance.AccountData;
            if (account == null) return;
            var quotes = Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData();
            var accountCurrency = account.Currency;

            var totalPosProfit = 0f;
            //var noQuoteForProfitCalculation = false;
            foreach (var order in marketOrders)
            {
                if (order.State != PositionState.Opened) continue;
                var positionProfit = DalSpot.Instance.CalculateProfitInDepoCurrency(order, quotes, accountCurrency);
                //if (!positionProfit.HasValue) noQuoteForProfitCalculation = true;
                order.ResultDepo = positionProfit ?? 0;
                totalPosProfit += order.ResultDepo;
            }

            // обновить средства на счете
            var newEquity = account.Balance + (decimal)totalPosProfit;
            if (!newEquity.SameMoney(account.Equity))
            {
                account.Equity = newEquity;
                if (AccountStatus.Instance.isAuthorized)
                    AccountStatus.Instance.AccountData = account;
            }            
        }
    }
}
