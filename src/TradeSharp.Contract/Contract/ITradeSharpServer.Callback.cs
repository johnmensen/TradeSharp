using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    /// <summary>
    /// категория сообщений, отправляемых пользователю (терминал) от сервера
    /// </summary>
    public enum ServiceMessageCategory
    {
        LogMessage = 0,
        ServiceMessage = 1,
        DialogBox = 2
    }

    [ServiceContract]
    [ServiceKnownType(typeof(TradeSignalActionTrade))]
    [ServiceKnownType(typeof(TradeSignalActionClose))]
    [ServiceKnownType(typeof(TradeSignalActionMoveStopTake))]
    public interface ITradeSharpServerCallback
    {
        /// <summary>
        /// опрос клиентов на предмет отвала
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Ping();

        /// <summary>
        /// квитанция на запрос - создать ордер (рыночный или инстантный)
        /// </summary>
        /// <param name="order">созданный ордер или null</param>
        /// <param name="status">статус выполнения</param>
        /// <param name="detailMessage">подробности ошибки или null</param>
        [OperationContract(IsOneWay = true)]
        void NewOrderResponse(MarketOrder order, RequestStatus status, string detailMessage);

        /// <summary>
        /// квитанция на запрос - создать отложенный ордер
        /// </summary>
        /// <param name="order">созданный ордер или null</param>
        /// <param name="status">статус выполнения</param>
        /// <param name="detailMessage">подробности ошибки или null</param>
        [OperationContract(IsOneWay = true)]
        void NewPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage);

        /// <summary>
        /// в позиции заполняются такие поля, как:
        /// цена выхода, своп, результат в контрвалюте, результат в валюте депо
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NewCloseOrderResponse(MarketOrder order, RequestStatus status, string detailMessage);

        /// <summary>
        /// квитанция на запрос редактирования маркет ордера
        /// </summary>
        /// <param name="order">отредактированный ордер</param>
        /// <param name="status"></param>
        /// <param name="detailMessage"></param>
        [OperationContract(IsOneWay = true)]
        void EditOrderResponse(MarketOrder order, RequestStatus status, string detailMessage);

        /// <summary>
        /// квитанция на запрос редактирования отложенного ордера
        /// </summary>
        /// <param name="order">отредактированный ордер</param>
        /// <param name="status"></param>
        /// <param name="detailMessage"></param>
        [OperationContract(IsOneWay = true)]
        void EditPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage);

        [OperationContract(IsOneWay = true)]
        void ProcessServiceMessage(ServiceMessageCategory cat, string message);

        [OperationContract(IsOneWay = true)]
        void ProcessTradeSignalAction(TradeSignalAction action);

        /// <summary>
        /// периодически выдается клиенту
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void AccountDataUpdated(Account account);
    }
}