using System;
using System.Collections.Generic;
using TradeSharp.Client.Util.Storage;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;

namespace TradeSharp.RobotFarm.BL
{
    /// <summary>
    /// контекст роботов специально для фермы (по заказу)
    /// 
    /// переопределены методы получения информации по счету / ордерам
    /// </summary>
    class RobotContextLiveFarm : RobotContextLive
    {
        private readonly ActualAccountData accountData;

        public RobotContextLiveFarm(TradeSharpServerTrade proxyTrade, Account accountInfo, Func<string> getUserLogin,
            ActualAccountData accountData) : 
            base(proxyTrade, accountInfo, getUserLogin)
        {
            this.accountData = accountData;
        }

        public override RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> ordlist)
        {
            ordlist = accountData.GetActualOrderList() ?? new List<MarketOrder>();
            return RequestStatus.OK;
        }

        public override Account GetAccountInfo(bool needEquity)
        {
            return accountData.GetActualAccount(needEquity);
        }

        public override RequestStatus SendNewOrderRequest(ProtectedOperationContext secCtx, int requestUniqueId, MarketOrder order,
            OrderType orderType, decimal requestedPrice, decimal slippagePoints)
        {
            if (IsDayOff()) return RequestStatus.WrongTime;
            return base.SendNewOrderRequest(secCtx, requestUniqueId, order, orderType, requestedPrice, slippagePoints);
        }

        public override RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder pos)
        {
            if (IsDayOff()) return RequestStatus.WrongTime;
            return base.SendEditMarketRequest(secCtx, pos);
        }

        public override RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId, PositionExitReason reason)
        {
            if (IsDayOff()) return RequestStatus.WrongTime;
            return base.SendCloseRequest(ctx, accountId, orderId, reason);
        }

        public override RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker, PositionExitReason reason)
        {
            if (IsDayOff()) return RequestStatus.WrongTime;
            return base.SendCloseByTickerRequest(ctx, accountId, ticker, reason);
        }

        private static bool IsDayOff()
        {
            return FarmDaysOff.IsDayOff(DateTime.UtcNow);
        }
    }
}
