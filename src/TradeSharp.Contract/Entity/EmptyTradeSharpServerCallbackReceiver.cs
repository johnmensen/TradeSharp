using TradeSharp.Contract.Contract;

namespace TradeSharp.Contract.Entity
{    
    public class EmptyTradeSharpCallbackReceiver : ITradeSharpServerCallback
    {
        private static EmptyTradeSharpCallbackReceiver instance;

        public static EmptyTradeSharpCallbackReceiver Instance
        {
            get { return instance ?? (instance = new EmptyTradeSharpCallbackReceiver()); }
        }

        public void Ping()
        {
        }

        public void NewOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
        }

        public void NewPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
        }

        public void NewCloseOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
        }

        public void EditOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
        }

        public void EditPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
        }

        public void ProcessServiceMessage(ServiceMessageCategory cat, string message)
        {
        }

        public void ProcessTradeSignalAction(TradeSignalAction action)
        {
        }

        public void AccountDataUpdated(Account ac)
        {            
        }
    }
}
