using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuSubscriptionOnPortfolio
    {
        private List<PerformerStat> allPerformers;
        private TradeSharpConnectionPersistent conn;
        private PLATFORM_USER subscriber;
        private const string SubscriberCurx = "USD";

        [TestFixtureSetUp]
        public void Setup()
        {
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            ManagerTrade.MakeTestInstance();
            SetupFakeStatistics();
            FindSubscriber();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
        }

        [Test]
        public void TestSubscribeOnCustomPortfolio()
        {
            var managerTrade = ManagerTrade.Instance;
            var portfolio = new TopPortfolio
                {
                    Criteria = "P",
                    DescendingOrder = true,
                    ParticipantCount = 6,
                    Name = "UserGeniusExclusiveTop",
                };
            var tradeSets = AutoTradeSettingsSampler.MakeSampleTradeSettings();
            tradeSets.TargetAccount = conn.PLATFORM_USER_ACCOUNT.First(pa => pa.PlatformUser == subscriber.ID).Account;
            var status = managerTrade.SubscribeOnPortfolio(ProtectedOperationContext.MakeServerSideContext(),
                                              subscriber.Login, portfolio, null, tradeSets);
            
            Assert.AreEqual(RequestStatus.OK, status, "SubscribeOnPortfolio - удалось подписаться");
            // проверить количество подписок
            var userSubsCount = conn.SUBSCRIPTION.Count(s => s.User == subscriber.ID);
            Assert.AreEqual(portfolio.ParticipantCount, userSubsCount, "SubscribeOnPortfolio - подписался на всех в портфеле");
            
            // проверить настройки авто-торговли
            var subSettings = conn.SUBSCRIPTION_SIGNAL.First(ss => ss.User == subscriber.ID);
            var wrongFields = AutoTradeSettingsSampler.TradeSignalSetsAreCorrect(tradeSets, subSettings);
            if (!string.IsNullOrEmpty(wrongFields))
                Assert.Fail("SubscribeOnPortfolio - настройки авто-торговли не сохранены в подписке: " + wrongFields);
            
            // настройки авто-торговли для самого портфеля
            var portfolioTradeSets = (from upf in conn.USER_TOP_PORTFOLIO
                                      join pf in conn.TOP_PORTFOLIO on upf.Portfolio equals pf.Id
                                      where upf.User == subscriber.ID
                                      select upf).FirstOrDefault();
            Assert.IsNotNull(portfolioTradeSets, "SubscribeOnPortfolio - портфель создан");
            var portfolioSignalSets = new SUBSCRIPTION_SIGNAL
                                          {
                                              AutoTrade = portfolioTradeSets.AutoTrade,
                                              MaxLeverage = portfolioTradeSets.MaxLeverage,
                                              PercentLeverage = portfolioTradeSets.PercentLeverage,
                                              HedgingOrdersEnabled = portfolioTradeSets.HedgingOrdersEnabled,
                                              FixedVolume = portfolioTradeSets.FixedVolume,
                                              MinVolume = portfolioTradeSets.MinVolume,
                                              MaxVolume = portfolioTradeSets.MaxVolume,
                                              VolumeRound = portfolioTradeSets.VolumeRound,
                                              StepVolume = portfolioTradeSets.StepVolume,
                                              TargetAccount = portfolioTradeSets.TargetAccount
                                          };
            wrongFields = AutoTradeSettingsSampler.TradeSignalSetsAreCorrect(tradeSets, portfolioSignalSets);
            if (!string.IsNullOrEmpty(wrongFields))
                Assert.Fail("SubscribeOnPortfolio - настройки авто-торговли не сохранены в портфеле: " + wrongFields);

            // отписаться от портфеля
            status = managerTrade.UnsubscribePortfolio(ProtectedOperationContext.MakeServerSideContext(),
                                                       subscriber.Login, true, true);
            Assert.AreEqual(RequestStatus.OK, status, "SubscribeOnPortfolio - удалось отписаться");
            userSubsCount = conn.SUBSCRIPTION.Count(s => s.User == subscriber.ID);
            Assert.AreEqual(0, userSubsCount, "SubscribeOnPortfolio - отписался ото всех в портфеле");
        }

        private void SetupFakeStatistics()
        {
            allPerformers = (from account in conn.ACCOUNT
                             join ua in conn.PLATFORM_USER_ACCOUNT on account.ID equals ua.Account
                             join srv in conn.SERVICE on ua.PlatformUser equals srv.User
                             where account.Balance > 0
                             select new PerformerStat
                                 {
                                     Account = account.ID,
                                     DepoCurrency = account.Currency,
                                     Service = srv.ID
                                 }).ToList();
            for (var i = 0; i < allPerformers.Count; i++)
                allPerformers[i].Profit = (i - 10) * 5;

            var accountStatistics = ProxyBuilder.Instance.MakeImplementer<IAccountStatistics>(true);
            ((IMockableProxy)accountStatistics).MockMethods.Add(StronglyName.GetMethodName<IAccountStatistics>(s => 
                s.GetAllPerformersWithCriteria(false, "", 0, true, null, 0)),
                new Func<bool, string, int, bool, float?, int, List<PerformerStat>>(
                    (managersOnly, criteria, count, asc, filterValue, serviceTypeMask) =>
                    {
                        return (asc 
                            ? allPerformers.OrderBy(p => p.Profit) 
                            : allPerformers.OrderByDescending(p => p.Profit)).Take(count).ToList();
                    })
                );

            TradeSharpAccountStatistics.Initialize(accountStatistics);
        }
    
        private void FindSubscriber()
        {
            subscriber = (from usr in conn.PLATFORM_USER
                          join w in conn.WALLET on usr.ID equals w.User
                          join pa in conn.PLATFORM_USER_ACCOUNT on usr.ID equals pa.PlatformUser
                          where pa.RightsMask == (int)AccountRights.Управление && w.Currency == SubscriberCurx && w.Balance > 1000
                          select usr).First();
            // отписать будущего подписчика от всех служб
            var ownedSubs = conn.SUBSCRIPTION.Where(s => s.User == subscriber.ID).ToList();
            foreach (var sub in ownedSubs)
            {
                conn.SUBSCRIPTION.Remove(sub);
            }
            conn.SaveChanges();
        }
    }
}
