using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Robot.BacktestServerProxy
{
    /// <summary>
    /// реализует необходимые роботам интерфейсы
    /// работает в реальном времени
    /// </summary>
    public class RobotContextLive : RobotContext
    {
        public override Account AccountInfo { get; set; }
        
        private readonly TradeSharpServerTrade proxyTrade;
        
        private Func<string> getUserLogin;

        public RobotContextLive(TradeSharpServerTrade proxyTrade, Account accountInfo, Func<string> getUserLogin)
        {
            this.proxyTrade = proxyTrade;
            this.AccountInfo = accountInfo;
            this.getUserLogin = getUserLogin;
            quotesStorage = Contract.Util.BL.QuoteStorage.Instance;
        }

        #region RobotContext overrides
        public override RequestStatus SendNewOrderRequest(
            ProtectedOperationContext secCtx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            return proxyTrade.proxy.SendNewOrderRequest(
                secCtx,
                requestUniqueId, order, orderType, requestedPrice, slippagePoints);
        }

        public override RequestStatus SendNewPendingOrderRequest(
            ProtectedOperationContext ctx,
            int requestUniqueId,
            PendingOrder order)
        {
            return proxyTrade.proxy.SendNewPendingOrderRequest(ctx, requestUniqueId, order);
        }

        public override RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId, PositionExitReason reason)
        {
            return proxyTrade.proxy.SendCloseRequest(ctx, accountId, orderId, reason);
        }

        public override RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId,
                                                               string ticker, PositionExitReason reason)
        {
            return proxyTrade.proxy.SendCloseByTickerRequest(ctx, accountId, ticker, reason);
        }

        public override RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord)
        {
            return proxyTrade.proxy.SendEditPendingRequest(secCtx, ord);
        }

        public override Account GetAccountInfo(bool needEquity)
        {
            return AccountInfo;
        }

        public override RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> ordlist)
        {
            return TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out ordlist);
        }

        public override RequestStatus GetHistoryOrdersCompressed(int? accountId, DateTime? startDate, out byte[] buffer)
        {
            return TradeSharpAccount.Instance.proxy.GetHistoryOrdersCompressed(accountId, startDate, out buffer);
        }

        public override RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate, out List<MarketOrder> ordlist)
        {
            return TradeSharpAccount.Instance.proxy.GetHistoryOrders(accountId, startDate, out ordlist);
        }

        public override RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate, out List<MarketOrder> ordlist)
        {
            return TradeSharpAccount.Instance.proxy.GetHistoryOrdersByCloseDate(accountId, startDate, out ordlist);
        }

        public override RequestStatus GetOrdersByFilter(int accountId, bool getClosedOrders, OrderFilterAndSortOrder filter, out List<MarketOrder> orders)
        {
            return TradeSharpAccount.Instance.proxy.GetOrdersByFilter(accountId, getClosedOrders, filter, out orders);
        }

        public override RequestStatus GetPendingOrders(int accountId, out List<PendingOrder> ordlist)
        {
            return TradeSharpAccount.Instance.proxy.GetPendingOrders(accountId, out ordlist);
        }

        public override RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext ctx,
            PendingOrder order, PendingOrderStatus status, int? positionId, string closeReason)
        {
            return proxyTrade.proxy.SendDeletePendingOrderRequest(ctx, order, status, positionId, closeReason);
        }

        public override RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder pos)
        {
            return proxyTrade.proxy.SendEditMarketRequest(secCtx, pos);
        }

        public override void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId,
            int tradeSignalCategory, UserEvent acEvent)
        {
            try
            {
                proxyTrade.proxy.SendTradeSignalEvent(ctx, accountId, tradeSignalCategory, acEvent);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в SendTradeSignalEvent(acc: {0}, cat: {1}): {2}",
                    accountId, tradeSignalCategory, ex);
            }
        }

        public List<PaidService> GetAuthoredTradeSignals()
        {
            try
            {
                var userLogin = getUserLogin();
                return TradeSharpAccount.Instance.proxy.GetPaidServices(userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAuthoredTradeSignals()", ex);
                return new List<PaidService>();
            }
        }
        #endregion
    }
}
