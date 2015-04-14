using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Service;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.SiteBridge
{
    [TestFixture]
    public class NuAccountEfficiencyCache
    {
        private class FakeCalculator : IEfficiencyCalculator
        {
            public bool Calculate(AccountEfficiency ef)
            {
                return true;
            }

            public void CalculateProfitCoeffs(AccountEfficiency ef)
            {
            }
        }

        #region Test Performers
        private readonly List<PerformerStat> testPerformers = new List<PerformerStat>
            {
                new PerformerStat
                    {
                        Account = 1,
                        DepoCurrency = "USD",
                        Email = "x1@y.z",
                        SubscriberCount = 2,
                        Service = 1,
                        UserId = 1,
                        // коэффициенты для формулы
                        Profit = 12.5f,
                        MaxRelDrawDown = 21.5f,
                        AvgYearProfit = 3.89f
                    },
                new PerformerStat
                    {
                        Account = 2,
                        DepoCurrency = "USD",
                        Email = "x2@y.z",
                        SubscriberCount = 1,
                        Service = 2,
                        UserId = 2,
                        // коэффициенты для формулы
                        Profit = 28.1f,
                        MaxRelDrawDown = 34.4f,
                        AvgYearProfit = 12f
                    },
                new PerformerStat
                    {
                        Account = 3,
                        DepoCurrency = "USD",
                        Email = "x3@y.z",
                        SubscriberCount = 0,
                        Service = 3,
                        UserId = 3,
                        // коэффициенты для формулы
                        Profit = -0.28f,
                        MaxRelDrawDown = 12.6f,
                        AvgYearProfit = -0.12f,
                        FullName = "сурков владислав юрьевич"
                    },
                new PerformerStat
                    {
                        Account = 4,
                        DepoCurrency = "USD",
                        Email = "x4@y.z",
                        SubscriberCount = 8,
                        Service = 4,
                        UserId = 4,
                        // коэффициенты для формулы
                        Profit = 10f,
                        MaxRelDrawDown = 30f,
                        AvgYearProfit = 16f,
                        FullName = " Сурков Владислав"
                    },
                // эталонные счета для портфелей
                new PerformerStat
                    {
                        Account = 100,
                        DepoCurrency = "USD",
                        Email = "x100@y.z",
                        UserId = 100,
                        FullName = " Дмитрий "
                    }
            };
        #endregion

        [TestFixtureSetUp]
        public void Setup()
        {
            // ITradeSharpServer
            var fakeServer = ProxyBuilder.Instance.MakeImplementer<ITradeSharpServer>(true);
            ((IMockableProxy)fakeServer).MockMethods.Add(
                StronglyName.GetMethodName<ITradeSharpServer>(s => s.GetAllManagers(null)),
                new Func<PaidServiceType?, List<PerformerStat>>(srvType => testPerformers));

            ((IMockableProxy)fakeServer).MockMethods.Add(
                StronglyName.GetMethodName<ITradeSharpServer>(s => s.GetCompanyTopPortfolioManagedAccounts()),
                new Func<List<PerformerStat>>(() => new List<PerformerStat>()));
            TradeSharpServer.Initialize(fakeServer);

            //// ITradeSharpAccount
            //private delegate RequestStatus GetMarketOrdersDel(int accountId, out List<MarketOrder> orders);
            //var fakeServerAccount = ProxyBuilder.Instance.MakeImplementer<ITradeSharpAccount>(true);
            //List<MarketOrder> orderList;
            //((IMockableProxy)fakeServerAccount).MockMethods.Add(
            //    StronglyName.GetMethodName<ITradeSharpAccount>(s => s.GetMarketOrders(0, out orderList)),
            //    new GetMarketOrdersDel((int id, out List<MarketOrder> orders) =>
            //        {
                        
            //        }));
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            TradeSharpServer.Initialize(null);
        }

        [Test]
        public void TestGetPerformersByCriteriaGivesCorrectCount()
        {
            var cache = new AccountEfficiencyCache(new FakeCalculator(), new MockDailyQuoteStorage());
            try
            {
                cache.UpdateCache();
            }
            catch (Exception ex)
            {
                Assert.Fail("Ошибка в AccountEfficiencyCache.TestGetPerformersByCriteria(): {0}", ex);
            }

            var performers = cache.GetAllPerformers(true);
            Assert.AreEqual(testPerformers.Count(p => p.Service > 0), performers.Count, 
                "GetAllPerformers(managers=true) возвращает правильное количество перформеров");
            performers = cache.GetAllPerformers(false);
            Assert.AreEqual(testPerformers.Count, performers.Count, 
                "GetAllPerformers(managers=false) возвращает правильное количество перформеров");
        }

        [Test]
        public void TestGetPerformersByCriteriaGivesCorrectMatches()
        {
            var cache = new AccountEfficiencyCache(new FakeCalculator(), new MockDailyQuoteStorage());
            cache.UpdateCache();
            const string criteria = "((AYP>0)&(DD<30))*P";
            var performerScore = testPerformers.ToDictionary(p => p, p => (p.AvgYearProfit <= 0 || p.MaxRelDrawDown >= 30) ? 0 : p.Profit);
            var bestScoredOne = performerScore.First(p => p.Value == performerScore.Values.Max()).Key;
            
            // по убыванию, без указания граничного критерия
            var performers = cache.GetAllPerformersWithCriteria(false, criteria, 100, false, null, 0);
            Assert.AreEqual(testPerformers.Count, performers.Count, "GetAllPerformersWithCriteria(формула) - все перформеры получены");
            Assert.AreEqual(bestScoredOne, performers[0], "GetAllPerformersWithCriteria(формула) - лучший вверху списка");
            
            performers = cache.GetAllPerformersWithCriteria(false, criteria, 100, false, 0, 0);
            Assert.AreEqual(performerScore.Count(s => s.Value > 0), performers.Count, "GetAllPerformersWithCriteria(формула) - все перформеры с критерием > 0 получены");
        }

        [Test]
        public void NuGetPerformersByFilter()
        {
            var cache = new AccountEfficiencyCache(new FakeCalculator(), new MockDailyQuoteStorage());
            cache.UpdateCache();

            var filters = new List<PerformerSearchCriteria>
                {
                    new PerformerSearchCriteria
                        {
                            propertyName = new PerformerStat().Property(p => p.Account),
                            checkWholeWord = true,
                            compradant = "1",
                            ignoreCase = false
                        }
                };
            var performers = cache.GetPerformersByFilter(false, filters, 100);
            Assert.AreEqual(1, performers.Count, "GetPerformersByFilter(Account=1) - должен вернуть 1 счет");

            filters[0].propertyName = new PerformerStat().Property(p => p.FullName);
            filters[0].compradant = "Владислав";
            filters[0].ignoreCase = false;
            filters[0].checkWholeWord = false;
                
            performers = cache.GetPerformersByFilter(false, filters, 100);
            Assert.AreEqual(1, performers.Count, "GetPerformersByFilter(FullName=Владислав) - должен вернуть 1 счет");
        }
    }
}
