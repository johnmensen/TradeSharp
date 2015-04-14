using System;
using System.Collections.Generic;
using System.Threading;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Util.Storage
{
    public class ActualAccountData
    {
        public int accountId;
        //private readonly ProtectedOperationContext context;
        private const int OrdersActualInterval = 1000 * 5;
        private const int AccountActualInterval = 1000 * 5;
        private const int DefaultTimeout = 1000;

        #region Logger
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);
        private const int LogMsgGetOrdersError = 1;
        private const int LogMsgGetAccountError = 2;
        #endregion

        public ActualAccountData(int accountId)
        {
            this.accountId = accountId;
        }

        #region Thread Safe Account
        private Account account;
        private readonly ReaderWriterLockSlim accountLocker = new ReaderWriterLockSlim();
        private const int AccountLockerTimeout = 1500;

        private Account Account
        {
            get
            {
                if (!accountLocker.TryEnterReadLock(AccountLockerTimeout))
                    return null;
                try
                {
                    return account;
                }
                finally 
                {
                    accountLocker.ExitReadLock();
                }
            }
            set
            {
                if (!accountLocker.TryEnterWriteLock(AccountLockerTimeout))
                    return;
                try
                {
                    account = value;
                }
                finally 
                {
                    accountLocker.ExitWriteLock();
                }
            }
        }
        #endregion

        private readonly ThreadSafeTimeStamp lastTimeAccountReceived = new ThreadSafeTimeStamp();

        private readonly ThreadSafeTimeStamp lastTimeOrdersReceived = new ThreadSafeTimeStamp();

        private readonly ThreadSafeList<MarketOrder> orders = new ThreadSafeList<MarketOrder>();
    
        /// <summary>
        /// возвращает список ордеров - как есть или обновив (прочитав) с сервера
        /// считает прибыль по ордерам
        /// </summary>
        public List<MarketOrder> GetActualOrderList()
        {
            var lastHit = lastTimeOrdersReceived.GetLastHitIfHitted();
            var deltaTime = lastHit.HasValue ? (DateTime.Now - lastHit.Value).TotalMilliseconds : int.MaxValue;
            var isActual = deltaTime < OrdersActualInterval;
            bool timeoutFlag;
            if (isActual)
                return orders.GetAll(DefaultTimeout, out timeoutFlag) ?? new List<MarketOrder>();

            // обновить и вернуть список ордеров
            ActualizeOrders();
            return orders.GetAll(DefaultTimeout, out timeoutFlag) ?? new List<MarketOrder>();
        }

        public Account GetActualAccount(bool updateOrders = false)
        {
            var lastHit = lastTimeAccountReceived.GetLastHitIfHitted();
            var deltaTime = lastHit.HasValue ? (DateTime.Now - lastHit.Value).TotalMilliseconds : int.MaxValue;
            var isActual = deltaTime < AccountActualInterval;
            if (isActual)
                return Account;

            // обновить информацию по счету
            ActualizeAccount();

            // опционально - обновить сделки и посчитать Equity
            if (updateOrders)
                ActualizeOrders();
            
            // вернуть акаунт
            return Account;
        }

        private void ActualizeAccount()
        {
            Account acc;
            try
            {
                TradeSharpAccount.Instance.proxy.GetAccountInfo(accountId, false, out acc);
                if (acc != null && acc.Equity == 0)
                    acc.Equity = acc.Balance;
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgGetAccountError, 1000 * 60 * 15,
                    "GetAccountInfo() error: ", ex);
                return;
            }

            if (acc != null)
                Account = acc;
        }

        private void ActualizeOrders()
        {
            List<MarketOrder> marketOrders;
            try
            {
                TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out marketOrders);
                if (marketOrders == null) return;
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgGetOrdersError, 1000 * 60 * 15,
                    "GetMarketOrders() error: {0}", ex);
                return;
            }
            
            // посчитать открытый профит
            var accountData = GetActualAccount();
            if (accountData == null) return;
            var sumProfit = 0f;
            
            if (marketOrders.Count > 0)
            {
                var quotes = Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData();                
                foreach (var order in marketOrders)
                {
                    var profit = DalSpot.Instance.CalculateProfitInDepoCurrency(order, quotes, accountData.Currency) ?? 0;
                    order.ResultDepo = profit;
                    sumProfit += profit;
                }
            }

            if (!orders.ReplaceRange(marketOrders, DefaultTimeout))
                Logger.InfoFormat("Таймаут обновления ордеров по счету {0} в ActualizeOrders()", accountId);

            // обновить Equity для счета
            account.Equity = account.Balance + (decimal)sumProfit;
            Account = account;
        }
    }
}
