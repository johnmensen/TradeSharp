using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuProfitByTickerForm
    {
        private delegate RequestStatus GetMarketOrdersDel(int accountId, out List<MarketOrder> opOrders);

        private readonly List<MarketOrder> testOrders = new List<MarketOrder>();

        [TestFixtureSetUp]
        public void Setup()
        {
            // забить котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();
            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            // ордера
            MakeTestOrders();
            // прокси
            var proxyAccount = ProxyBuilder.Instance.MakeImplementer<ITradeSharpAccount>(true);
            ((IMockableProxy)proxyAccount).MockMethods.Add(
                StronglyName.GetMethodName<ITradeSharpAccount>(a => a.GetClosedOrders(0, "", 0, 0)),
                new Func<int, string, int, int, List<MarketOrder>>((accountId, ticker, startId, maxCount) => 
                    testOrders.Where(o => o.Symbol == ticker && o.ID > startId && o.State == PositionState.Closed).Take(maxCount).ToList())
                );
            List<MarketOrder> opOrders;
            ((IMockableProxy)proxyAccount).MockMethods.Add(
                StronglyName.GetMethodName<ITradeSharpAccount>(a => a.GetMarketOrders(0, out opOrders)),
                new GetMarketOrdersDel((int accountId, out List<MarketOrder> resOrders) =>
                    {
                        resOrders = testOrders.Where(o => o.State == PositionState.Opened).ToList();
                        return RequestStatus.OK;
                    })
                );
            
            TradeSharpAccount.Initialize(proxyAccount);
            // тестовый счет
            AccountStatus.Instance.AccountData = new Account
            {
                Currency = "USD"
            };
        }

        [Test]
        public void TestGetChartData()
        {
            var worker = new BackgroundWorker();
            var args = new DoWorkEventArgs("EURUSD");
            ProfitByTickerForm.OrderCalcWorkerOnDoWork(worker, args);
            var balanceOnDate = (ProfitByTickerForm.BalanceAndEquitySeriesData)args.Result;
            Assert.Greater(balanceOnDate.lstBalance.Count, 5, "GetChartData() - история насчитывает достаточное количество дней");
        }

        private void MakeTestOrders()
        {
            var tickers = new[] {"EURUSD", "USDJPY", "USDCAD"};
            var rnd = new Random();
            
            for (var i = 0; i < 600; i++)
            {
                var ticker = tickers[rnd.Next(tickers.Length)];
                var side = rnd.Next(2) * 2 - 1;
                var isClosed = rnd.Next(100) < 75;
                var hoursOfEnter = -rnd.Next(24*200);
                var timeEnter = DateTime.Now.AddHours(hoursOfEnter);
                
                var order = new MarketOrder
                    {
                        ID = i + 1,
                        Volume = 50000,
                        Symbol = ticker,
                        State = isClosed ? PositionState.Closed : PositionState.Opened,
                        Side = side,
                        TimeEnter = timeEnter,                        
                    };

                if (!isClosed)
                {
                    var quote = Contract.Util.BL.QuoteStorage.Instance.ReceiveValue(ticker);
                    order.PriceEnter = quote.bid + DalSpot.Instance.GetAbsValue(ticker, (float)(rnd.Next(400) - 200));
                }
                else
                {
                    var hoursWhenClosed = -rnd.Next(-hoursOfEnter);
                    order.TimeExit = DateTime.Now.AddHours(hoursWhenClosed);
                    order.ResultDepo = (float) (500d * rnd.NextDouble() - 250d);
                }

                testOrders.Add(order);
            }

            var minStartTime = DateTime.Now.AddDays(-25);
            if (testOrders[0].TimeEnter > minStartTime)
                testOrders[0].TimeEnter = minStartTime;
        }
    }
}
