using TradeSharp.Contract.Entity;

namespace TradeSharp.DealerInterface
{
    public class SimpleDealer
    {
        public IOrderManager ServerInterface { get; set; }

        public RequestStatus SendNewPendingOrderRequest(Account account, PendingOrder order)
        {
            // цена не проверяется (возможность заранее разместить ордер, когда, например, нет котировки)
            order.AccountID = account.ID;
            // сохранить ордер (и уведомить клиента)
            int orderId;
            var result = ServerInterface.SavePendingOrder(order, out orderId);
            return result ? RequestStatus.OK : RequestStatus.SerializationError;
        }

        public RequestStatus DeletePendingOrderRequest(PendingOrder order, PendingOrderStatus status, int? positionId,
            string closeReason)
        {
            // снять отложенный ордер 
            return ServerInterface.DeletePendingOrder(order, status, positionId, closeReason)
                ? RequestStatus.OK : RequestStatus.ServerError;
        }

        public virtual RequestStatus ModifyMarketOrderRequest(MarketOrder order)
        {
            // изменить маркет ордер
            return ServerInterface.ModifyMarketOrder(order)
                ? RequestStatus.OK : RequestStatus.ServerError;
        }

        public RequestStatus ModifyPendingOrderRequest(PendingOrder order)
        {
            // изменить отложенный ордер
            return ServerInterface.ModifyPendingOrder(order)
                ? RequestStatus.OK : RequestStatus.ServerError;
        }
    }
}
