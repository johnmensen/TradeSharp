using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL
{
    class TradeServerCallbackProcessor : ITradeSharpServerCallback
    {
        public TradeSignalProcessor SignalProcessor { set; private get; }

        public TradeServerCallbackProcessor()
        {
            LogMessage += RobotFarm.Instance.AppendLogMessage;
        }

        private Action<string> logMessage;
        public event Action<string> LogMessage
        {
            add { logMessage += value; }
            remove { logMessage -= value; }
        }

        public void Ping()
        {
        }

        public void NewOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            if (status == RequestStatus.OK)
                logMessage(string.Format("Счет {0}: ордер ({1} {2} {3}) успешно размещен",
                                         order.AccountID, order.Side > 0 ? "BUY" : "SELL",
                                         order.Volume.ToStringUniformMoneyFormat(), order.Symbol));
            else
                logMessage(string.Format("Счет {0}: ошибка размещения ордера ({1} {2} {3}): {4}",
                                         order.AccountID, order.Side > 0 ? "BUY" : "SELL",
                                         order.Volume.ToStringUniformMoneyFormat(), order.Symbol,
                                         status));
        }

        public void NewPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            if (status == RequestStatus.OK)
                logMessage(string.Format("Счет {0}: ордер {1} успешно размещен",
                                         order.AccountID, order));
            else
                logMessage(string.Format("Счет {0}: ошибка размещения ордера {1}: {2}",
                                         order.AccountID, order, status));
        }

        public void NewCloseOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            if (status == RequestStatus.OK)
                logMessage(string.Format("Счет {0}: ордер {1} закрыт",
                                         order.AccountID, order));
            else
                logMessage(string.Format("Счет {0}: ошибка закрытия ордера {1}: {2}",
                                         order.AccountID, order, status));
        }

        public void EditOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            if (status == RequestStatus.OK)
                logMessage(string.Format("Счет {0}: ордер {1} успешно изменен",
                                         order.AccountID, order));
            else
                logMessage(string.Format("Счет {0}: ошибка редактирования ордера {1}: {2}",
                                         order.AccountID, order, status));
        }

        public void EditPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            if (status == RequestStatus.OK)
                logMessage(string.Format("Счет {0}: ордер {1} успешно изменен",
                                         order.AccountID, order));
            else
                logMessage(string.Format("Счет {0}: ошибка редактирования ордера {1}: {2}",
                                         order.AccountID, order, status));
        }

        public void ProcessServiceMessage(ServiceMessageCategory cat, string message)
        {
        }

        public void ProcessTradeSignalAction(TradeSignalAction action)
        {
            Logger.InfoFormat("ProcessTradeSignalAction(order: {0}, signal: {1})", action.OrderId, action.ServiceId);
            SignalProcessor.ProcessTradeSignalAction(action);
        }

        public void AccountDataUpdated(Account account)
        {
        }
    }
}
