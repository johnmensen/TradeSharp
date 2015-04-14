using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.BacktestServerProxy;

namespace TradeSharp.Test.Robot
{
    /// <summary>
    /// Вспомогательный класс для частых действий при тестиорвании роботов.
    /// Например, при инициализации тестового класса нужно создать RobotContextBacktest и т.п.
    /// </summary>
    public static class TestRobotUtil
    {
        public static RobotContextBacktest GetRobotContextBacktest(DateTime timeFrom, DateTime timeTo, decimal initialBalance = 100000, 
            string currency = "USD", string group = "Demo", float maxLeverage = 50, float brokerLeverage = 100)
        {
            return  new RobotContextBacktest((tickers, end) => { })
            {
                AccountInfo = new Account
                {
                    Balance = initialBalance,
                    Equity = initialBalance,
                    Currency = currency,
                    Group = group,
                    ID = 1,
                    MaxLeverage = maxLeverage,
                    UsedMargin = 0
                },
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                UpdateTickerCache = false,
                groupDefault = new AccountGroup
                {
                    Code = group,
                    IsReal = false,
                    BrokerLeverage = brokerLeverage,
                    MarginCallPercentLevel = 90,
                    StopoutPercentLevel = 95
                }
            };
        }

        public static int InitialBalance { get { return 100000; } }
    }
}
