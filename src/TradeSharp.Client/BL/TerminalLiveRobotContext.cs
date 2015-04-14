using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// наследник обычного real-time контекста роботов
    /// адаптирован для терминала:
    /// - открытые сделки получает из кэша открытых ордеров
    /// </summary>
    class TerminalLiveRobotContext : RobotContextLive
    {
        public TerminalLiveRobotContext(TradeSharpServerTrade proxyTrade, Account accountInfo, Func<string> getUserLogin) 
            : base(proxyTrade, accountInfo, getUserLogin)
        {
        }

        public override RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> ordlist)
        {
            ordlist = MarketOrdersStorage.Instance.MarketOrders;
            return RequestStatus.OK;
        }
    }
}
