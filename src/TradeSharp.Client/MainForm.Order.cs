using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        #region Константы
        private const int ShowLastOrdersIntervalHours = 24;
        #endregion

        public RequestStatus SendNewOrderRequestSafe(
            int requestUniqueId,
            int accountId, 
            MarketOrder order,
            decimal requestedPrice, decimal slippagePoints,
            OrderType ordType)
        {
            order.AccountID = accountId;

            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<int, MarketOrder, decimal, decimal, OrderType, RequestStatus>(SendNewOrderRequestUnsafe),
                    requestUniqueId, order, requestedPrice, slippagePoints, ordType);
            return SendNewOrderRequestUnsafe(requestUniqueId, order, requestedPrice, slippagePoints, ordType);
        }

        private static RequestStatus SendNewOrderRequestUnsafe(int requestUniqueId,
            MarketOrder order,
            decimal requestedPrice, decimal slippagePoints,
            OrderType ordType)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendNewOrderRequest(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    requestUniqueId,
                    order,
                    ordType, requestedPrice, slippagePoints);
                if (res != RequestStatus.OK)
                {
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageUnableToDeliverRequest"),
                        string.Format(
                            Localizer.GetString("MessageRequestRejectedFmt"),
                            order.Side > 1 ? "BUY" : "SELL",
                            order.Volume.ToStringUniformMoneyFormat(), order.Symbol), AccountEventCode.ServerMessage)); 
                    return res;
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendNewOrderRequest", ex);
                return RequestStatus.NoConnection;
            }            
        }

        public RequestStatus SendNewPendingOrderRequestSafe(int requestUniqueId, PendingOrder order)
        {
            if (requestUniqueId == 0) requestUniqueId = RequestUniqueId.Next();
            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<PendingOrder, int, RequestStatus>(SendNewPendingOrderRequestUnsafe), 
                    order, requestUniqueId);
            return SendNewPendingOrderRequestUnsafe(order, requestUniqueId);
        }

        private static RequestStatus SendNewPendingOrderRequestUnsafe(PendingOrder order, int requestUniqueId)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendNewPendingOrderRequest(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    requestUniqueId,
                    order);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageErrorExecutingOrder"), 
                        EnumFriendlyName<RequestStatus>.GetString(res), 
                        AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendNewPendingOrderRequest", ex);
                return RequestStatus.NoConnection;
            }
        }

        public RequestStatus SendCloseRequestSafe(int accountId, int orderId, PositionExitReason reason)
        {
            if (InvokeRequired)
                return (RequestStatus) Invoke(new Func<int, int, PositionExitReason, RequestStatus>(SendCloseRequestUnsafe),
                           accountId, orderId, reason);

            return SendCloseRequestUnsafe(accountId, orderId, reason);
        }

        private static RequestStatus SendCloseRequestUnsafe(int accountId, int orderId, PositionExitReason reason)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendCloseRequest(CurrentProtectedContext.Instance.MakeProtectedContext(),
                    accountId, orderId, reason);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageErrorExecutingOrder"),
                        EnumFriendlyName<RequestStatus>.GetString(res),  
                        AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendCloseRequest", ex);
                return RequestStatus.NoConnection;
            }
        }

        public RequestStatus SendCloseByTickerRequestSafe(int accountId, string ticker,
                                                                  PositionExitReason reason)
        {
            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<int, string, PositionExitReason, RequestStatus>(SendCloseByTickerRequestUnsafe),
                              accountId, ticker, reason);

            return SendCloseByTickerRequestUnsafe(accountId, ticker, reason);
        }

        private static RequestStatus SendCloseByTickerRequestUnsafe(int accountId, string ticker, PositionExitReason reason)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendCloseByTickerRequest(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    accountId, ticker, reason);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageErrorExecutingOrder"),
                        EnumFriendlyName<RequestStatus>.GetString(res), 
                        AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendCloseByTickerRequest", ex);
                return RequestStatus.NoConnection;
            }
        }

        public RequestStatus SendDeletePendingOrderRequestSafe(PendingOrder order,
            PendingOrderStatus status, int? positionId, string closeReason)
        {
            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<PendingOrder, PendingOrderStatus, int?, string, RequestStatus>(SendDeletePendingOrderRequestUnsafe),
                    status, positionId, closeReason);
            return SendDeletePendingOrderRequestUnsafe(order, status, positionId, closeReason);
        }

        private static RequestStatus SendDeletePendingOrderRequestUnsafe(PendingOrder order,
            PendingOrderStatus status, int? positionId, string closeReason)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendDeletePendingOrderRequest(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    order, status, positionId, closeReason);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageErrorExecutingOrder"),
                        EnumFriendlyName<RequestStatus>.GetString(res), 
                        AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendDeletePendingOrderRequest", ex);
                return RequestStatus.NoConnection;
            }
        }
    
        public RequestStatus SendEditMarketRequestSafe(MarketOrder ord)
        {
            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<MarketOrder, RequestStatus>(SendEditMarketRequestUnsafe), ord);
            return SendEditMarketRequestUnsafe(ord);
        }

        private static RequestStatus SendEditMarketRequestUnsafe(MarketOrder ord)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), ord);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(
                        new AccountEvent(
                            Localizer.GetString("MessageErrorExecutingOrder"),
                            EnumFriendlyName<RequestStatus>.GetString(res),  
                            AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendEditMarketRequest ошибка выполнения: ", ex);
                return RequestStatus.NoConnection;
            }
        }

        public RequestStatus SendEditPendingRequestSafe(PendingOrder ord)
        {
            if (InvokeRequired)
                return (RequestStatus)Invoke(new Func<PendingOrder, RequestStatus>(SendEditPendingRequestUnsafe), ord);
            return SendEditPendingRequestUnsafe(ord);
        }

        private static RequestStatus SendEditPendingRequestUnsafe(PendingOrder ord)
        {
            try
            {
                var res = serverProxyTrade.proxy.SendEditPendingRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), ord);
                if (res != RequestStatus.OK)
                    Instance.ShowMsgWindowSafe(new AccountEvent(
                        Localizer.GetString("MessageErrorExecutingOrder"),
                        EnumFriendlyName<RequestStatus>.GetString(res), 
                        AccountEventCode.ServerMessage));
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("SendEditPendingRequest ошибка выполнения: ", ex);
                return RequestStatus.NoConnection;
            }
        }

        /// <summary>
        /// Запускается при старте терминала и показывает, какие сделки были проделаны за последние 24 часа
        /// </summary>
        private void ShowLast24HrOrders()
        {
            var account = AccountStatus.Instance.AccountData;
            if (account == null) return;
            var accountId = AccountStatus.Instance.accountID;

            var lastTime = UserSettings.Instance.TimeClosed;
            var timeMargin = DateTime.Now.AddHours(-ShowLastOrdersIntervalHours);
            if (lastTime > timeMargin) timeMargin = lastTime;

            lock (this)
            {
                // открытые ордера за указанный интервал
                List<MarketOrder> orders;
                try
                {
                    TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out orders);
                }
                catch (Exception ex)
                {
                    Logger.Error("ShowLast24HrOrders() - GetMarketOrders() error", ex);
                    orders = new List<MarketOrder>();
                }
                orders = orders.Where(o => o.TimeEnter > timeMargin).ToList();

                // закрытые ордера
                List<MarketOrder> ordersClosed = null;
                try
                {
                    TradeSharpAccount.Instance.proxy.GetHistoryOrdersByCloseDate(accountId, timeMargin, out ordersClosed);
                }
                catch (Exception ex)
                {
                    Logger.Error("ShowLast24HrOrders() - GetHistoryOrders() error", ex);
                }
                if (ordersClosed == null)
                    ordersClosed = new List<MarketOrder>();
                ordersClosed = ordersClosed.Where(o => !o.TimeExit.HasValue || o.TimeExit.Value > timeMargin).ToList();

                // по всем ордерам сформировать сообщение
                var events = new List<AccountEvent>();
                foreach (var order in orders)
                {
                    events.Add(new AccountEvent(
                                   string.Format("Новый ордер №{0}", order.ID),
                                   string.Format("{0} {1} {2}, {3:dd MMM HH:mm}",
                                                 order.Side > 0 ? "BUY" : "SELL",
                                                 order.Volume.ToStringUniformMoneyFormat(),
                                                 order.Symbol, order.TimeEnter), AccountEventCode.DealOpened));
                }

                // по закрытым ордерам
                foreach (var order in ordersClosed)
                {
                    if (!order.TimeExit.HasValue)
                        events.Add(new AccountEvent(
                                       string.Format("Отмена ордера №{0}", order.ID),
                                       string.Format("{0} {1} {2}",
                                                     order.Side > 0 ? "BUY" : "SELL",
                                                     order.Volume.ToStringUniformMoneyFormat(), order.Symbol),
                                       AccountEventCode.DealCanceled));
                    else
                        events.Add(new AccountEvent(
                                       string.Format("Закрытие ордера №{0}", order.ID),
                                       string.Format("{0} {1} {2} по {3}, выход {4:dd MMM HH:mm} ({5} {6})",
                                                     order.Side > 0 ? "BUY" : "SELL",
                                                     order.Volume.ToStringUniformMoneyFormat(),
                                                     order.Symbol,
                                                     order.PriceEnter.ToStringUniformPriceFormat(),
                                                     order.TimeExit,
                                                     order.ResultDepo.ToStringUniformMoneyFormat(),
                                                     account.Currency), AccountEventCode.DealClosed));
                }

                if (events.Count == 0) return;

                // таки показать окно
                ShowMsgWindowSafe(events);
                UserSettings.Instance.TimeClosed = DateTime.Now;
                UserSettings.Instance.SaveSettings();
            }
        }
    }
}
