using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.DealerInterface
{
    public interface IDealer
    {
        string DealerCode { get; }
        
        List<string> GroupCodes { get; }

        void Initialize();

        void Deinitialize();

        string GetErrorString();

        void ClearError();

        IOrderManager ServerInterface { get; set; }

        RequestStatus SendNewOrderRequest(Account account, MarketOrder order, OrderType ordType, 
                                          decimal requestedPrice, decimal slippagePoints);

        RequestStatus SendNewPendingOrderRequest(Account account, PendingOrder order);

        RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason);

        RequestStatus DeletePendingOrderRequest(PendingOrder order, PendingOrderStatus status, int? positionId,
            string closeReason);

        RequestStatus ModifyMarketOrderRequest(MarketOrder order);

        RequestStatus ModifyPendingOrderRequest(PendingOrder order);
    }
}
