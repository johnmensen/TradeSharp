using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using MarketOrder = TradeSharp.Contract.Entity.MarketOrder;

namespace TradeSharp.DealerInterface
{
    /// <summary>
    /// интерфейс реализуют серверный класс (TradeSharpTradeManager)
    /// пользуются им дилеры
    /// </summary>
    public interface IOrderManager
    {
        bool SavePendingOrder(PendingOrder order, out int orderID);

        bool DeletePendingOrder(PendingOrder orderDecorated,
                                PendingOrderStatus status,
                                int? positionID,
                                string closeReason);

        bool ModifyMarketOrder(MarketOrder order);

        bool ModifyPendingOrder(PendingOrder modified);

        bool SaveOrderAndNotifyClient(MarketOrder order, out int posID);

        bool CloseOrder(MarketOrder order,
                        decimal price,
                        PositionExitReason exitReason);

        bool CloseOrder(int orderId,
                        decimal price,
                        PositionExitReason exitReason);

        Account GetAccount(int accountId);

        bool GetMarketOrder(int orderId, out MarketOrder order);

        void SaveProviderMessage(BrokerOrder msg);

        /// <summary>
        /// возвращает словарь response - request
        /// значения м.б. пустыми
        /// </summary>        
        /// <returns>response - request</returns>
        Dictionary<BrokerResponse, BrokerOrder>
            FindRequestsForExecutionReports(List<BrokerResponse> reports);

        void NotifyClientOnOrderRejected(MarketOrder order, string rejectReason);
    }
}
