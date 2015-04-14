using System;
using System.Linq;
using System.Collections.Generic;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using TradeSharp.TradeLib;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuTradeManager
    {
        private TradeManager tradeManager;

        private delegate RequestStatus GetAccountInfoDel(int accountId, bool needEquityInfo,
                                                         out Account account);

        private delegate RequestStatus GetMarketOrdersDel(int accountId, out List<MarketOrder> orders);

        private readonly AccountGroup testGroup = new AccountGroup
            {
                Code = "Demo",
                IsReal = false,
                BrokerLeverage = 50,
                StopoutPercentLevel = 90,
                MarginCallPercentLevel = 80
            };

        private readonly Account testAccount = new Account
                            {
                                Currency = "USD",
                                Balance = 19999M,
                                ID = 1
                            };

        private ITradeSharpAccount fakeAccountManager;

        private ITradeSharpServerTrade fakeServerTrade;

        [TestFixtureSetUp]
        public void Setup()
        {
            // подготовить свежие котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            // замочить прокси
            MakeTradeSharpAccountProxy();
            MakeTradeSharpServerTradeProxy();

            tradeManager = new TradeManager(
                fakeServerTrade,
                fakeAccountManager,
                QuoteStorage.Instance, accountId => testGroup);
        }

        [Test]
        public void TestIsEnterEnabled()
        {
            const int volm = 50000;
            testAccount.MaxLeverage = 0;
            decimal equity;
            //int accountId, string symbol, int side, int volume, out decimal equity)
            var enabld = tradeManager.IsEnterEnabled(testAccount.ID, "EURUSD", (int) DealType.Buy, volm, out equity);
            Assert.IsTrue(enabld, "TradeManager.IsEnterEnabled() - должен быть True");

            enabld = tradeManager.IsEnterEnabled(testAccount.ID, "EURUSD", (int)DealType.Sell, 2500000, out equity);
            Assert.IsFalse(enabld, "TradeManager.IsEnterEnabled() - должен быть False (морж)");
        }

        [Test]
        public void TestIsEnterEnabledFailedMaxLeverage()
        {
            const int volm = 40000;
            testAccount.MaxLeverage = 5;
            decimal equity;
            
            var enabld = tradeManager.IsEnterEnabled(testAccount.ID, "EURUSD", (int)DealType.Buy, volm, out equity);
            Assert.IsFalse(enabld, "TradeManager.IsEnterEnabled() - должен быть False (макс. плечо)");
        }

        [Test]
        public void TestCalculateAccountExposure()
        {
            decimal equity;
            decimal reservedMargin;
            decimal exposure;

            List<MarketOrder> orders;
            var profitCalculator = ProfitCalculator.Instance;
            fakeAccountManager.GetMarketOrders(testAccount.ID, out orders);

            var dicExp = profitCalculator.CalculateAccountExposure(testAccount.ID,
                out equity, out reservedMargin, out exposure, QuoteStorage.Instance.ReceiveAllData(),
                fakeAccountManager, i => testGroup);
            Assert.AreEqual(orders.Select(o => o.Symbol).Distinct().Count(),
                dicExp.Count, "CalculateAccountExposure() - вернула нужное количество записей");

            // проверить общую экспозицию
            var dealByTicker = orders.GroupBy(o => o.Symbol).ToDictionary(o => o.Key,
                o => Math.Abs(o.Sum(ord => ord.Side * ord.Volume)));
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            var mustBeExposure = dealByTicker.Sum(deal =>
                {
                    string errorStr;
                    return DalSpot.Instance.ConvertToTargetCurrency(deal.Key, true, testAccount.Currency,
                                                                    deal.Value, quotes, out errorStr, true) ?? 0;
                });
            var deltaExp = Math.Abs(mustBeExposure - exposure);
            Assert.Less(deltaExp, 1, "CalculateAccountExposure() - неверный расчет экспозиции");
        }

        [Test]
        public void TestCheckOrderTrailing()
        {
            var order = new MarketOrder
                {
                    PriceEnter = 1.3500f,
                    Symbol = "USDCAD",
                    Side = -1,
                    TrailLevel1 = 72,
                    TrailTarget1 = 20
                };
            tradeManager.CheckOrderTrailing(order, new QuoteData(1.3420f, 1.3423f, DateTime.Now));
            Assert.Less(Math.Abs(1.3480f - order.StopLoss.Value), 
                0.0001f, "CheckOrderTrailing - StopLoss не переместился, а должен был");

            order.trailingLevels[0] = 80;
            order.StopLoss = null;
            tradeManager.CheckOrderTrailing(order, new QuoteData(1.3420f, 1.3423f, DateTime.Now));
            Assert.IsNull(order.StopLoss, "CheckOrderTrailing - StopLoss переместился, а не должен был");
            
            order.trailingLevels[1] = 120;
            order.trailingTargets[1] = 100;
            tradeManager.CheckOrderTrailing(order, new QuoteData(1.3320f, 1.3323f, DateTime.Now));
            Assert.Less(Math.Abs(1.3400f - order.StopLoss.Value),
                0.0001f, "CheckOrderTrailing - StopLoss не переместился на второй уровень трейлинга, а должен был");
        }

        [Test]
        public void TestCheckStopOut()
        {
            var oldBalance = testAccount.Balance;
            testAccount.Balance = 5000;
            var oldSoLever = testGroup.StopoutPercentLevel;
            try
            {
                var isStop = tradeManager.CheckStopOut(testAccount.ID);
                Assert.IsTrue(isStop, "CheckStopOut() - должен быть с/о (90%)");
                testGroup.StopoutPercentLevel = 50;
                isStop = tradeManager.CheckStopOut(testAccount.ID);
                Assert.IsFalse(isStop, "CheckStopOut() - не должен быть с/о (50%)");
            }
            finally
            {
                testAccount.Balance = oldBalance;
                testGroup.StopoutPercentLevel = oldSoLever;
            }
        }

        private void MakeTradeSharpAccountProxy()
        {
            fakeAccountManager = ProxyBuilder.Instance.MakeImplementer<ITradeSharpAccount>(true);
            // "замочить" нужные методы
            // ReSharper disable SuspiciousTypeConversion.Global
            Account account;
            ((IMockableProxy)fakeAccountManager).MockMethods.Add(
                // ReSharper restore SuspiciousTypeConversion.Global
                StronglyName.GetMethodName<ITradeSharpAccount>(
                    f => f.GetAccountInfo(0, false, out account)),
                new GetAccountInfoDel((int accountId, bool needEquityInfo, out Account act) =>
                    {
                        act = testAccount;
                        act.ID = accountId;

                        if (needEquityInfo)
                        {
                            List<MarketOrder> ordList;
                            fakeAccountManager.GetMarketOrders(accountId, out ordList);
                            var profit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(ordList,
                                QuoteStorage.Instance.ReceiveAllData(), act.Currency);
                            act.Equity = act.Balance + (decimal)profit;
                        }
                        return RequestStatus.OK;
                    }));
            List<MarketOrder> orders;
            // ReSharper disable SuspiciousTypeConversion.Global
            ((IMockableProxy)fakeAccountManager).MockMethods.Add(
                // ReSharper restore SuspiciousTypeConversion.Global
                StronglyName.GetMethodName<ITradeSharpAccount>(
                    f => f.GetMarketOrders(0, out orders)),
                new GetMarketOrdersDel((int accountId, out List<MarketOrder> ordList) =>
                    {
                        ordList = new List<MarketOrder>
                            {
                                new MarketOrder
                                    {
                                        AccountID = accountId,
                                        Symbol = "EURUSD",
                                        Side = (int)DealType.Buy,
                                        Volume = 100000,
                                        PriceEnter = 1.3140f,
                                        State = PositionState.Opened
                                    },
                                new MarketOrder
                                    {
                                        AccountID = accountId,
                                        Symbol = "EURUSD",
                                        Side = (int)DealType.Sell,
                                        Volume = 20000,
                                        PriceEnter = 1.3180f,
                                        State = PositionState.Opened
                                    },
                                new MarketOrder
                                    {
                                        AccountID = accountId,
                                        Symbol = "USDCAD",
                                        Side = (int)DealType.Sell,
                                        Volume = 80000,
                                        PriceEnter = 1.1130f,
                                        State = PositionState.Opened
                                    }
                            };
                        return RequestStatus.OK;
                    }));
        }
    
        private void MakeTradeSharpServerTradeProxy()
        {
            fakeServerTrade = ProxyBuilder.Instance.MakeImplementer<ITradeSharpServerTrade>(true);

            // ReSharper disable SuspiciousTypeConversion.Global
            ((IMockableProxy)fakeServerTrade).MockMethods.Add(
            // ReSharper restore SuspiciousTypeConversion.Global
                StronglyName.GetMethodName<ITradeSharpServerTrade>(
                    f => f.SendEditMarketRequest(null, null)),
                new Func<ProtectedOperationContext, MarketOrder, RequestStatus>((ctx, order) => RequestStatus.OK));
        }
    }
}
