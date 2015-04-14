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
    }
}
