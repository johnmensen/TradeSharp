using System;
using System.Collections.Generic;
using System.Threading;
using BrokerService.Contract.Entity;
using TradeSharp.Util;

namespace Mt4Dealer
{
    public class RequestStorage
    {
        private readonly List<RequestedOrder> tradeRequestList = new List<RequestedOrder>();

        private readonly ReaderWriterLockSlim requestLocker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        #region Singleton
        private static readonly Lazy<RequestStorage> instance = new Lazy<RequestStorage>(() => new RequestStorage());

        public static RequestStorage Instance
        {
            get { return instance.Value; }
        }

        private RequestStorage()
        {
        }
        #endregion

        public void StoreRequest(RequestedOrder request)
        {
            if (!requestLocker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("Таймаут сохранения запроса (RequestStorage)");
                return;
            }

            try
            {
                tradeRequestList.Add(request);
            }
            finally
            {
                requestLocker.ExitWriteLock();
            }
        }

        public RequestedOrder FindRequest(int reqId)
        {
            if (!requestLocker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("Таймаут извлечения запроса (RequestStorage)");
                return null;
            }

            try
            {
                var index = tradeRequestList.FindIndex(r => r.request.Id == reqId);
                if (index < 0) return null;
                var req = tradeRequestList[index];
                tradeRequestList.RemoveAt(index);
                return req;
            }
            finally
            {
                requestLocker.ExitWriteLock();
            }
        }
    }

    public class RequestedOrder
    {
        public TradeTransactionRequest request;

        public TradeSharp.Contract.Entity.MarketOrder requestedOrder;

        public RequestedOrder() {}

        public RequestedOrder(TradeTransactionRequest request, TradeSharp.Contract.Entity.MarketOrder requestedOrder)
        {
            this.request = request;
            this.requestedOrder = requestedOrder;
        }
    }
}
