using System;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.TradeSignalExecutor.BL;
using TradeSharp.Util;

namespace TradeSharp.Test.TradeSignalExecutor
{
    [TestFixture]
    public class PortfolioBalancer
    {
        private TradeSharpConnectionPersistent conn;
        private TOP_PORTFOLIO[] portfolios;
        private PLATFORM_USER_ACCOUNT[] portOwners;
        private PLATFORM_USER_ACCOUNT[] portSubs;

        [TestFixtureSetUp]
        public void SetupTest()
        {
            MakeFakeTradeProxy();

            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();

            // очистить подписки на портфели
            foreach (var sub in conn.USER_TOP_PORTFOLIO.ToList())
                conn.USER_TOP_PORTFOLIO.Remove(sub);
            foreach (var portf in conn.TOP_PORTFOLIO.ToList())
                conn.TOP_PORTFOLIO.Remove(portf);
            conn.SaveChanges();

            // определить владельцев портфелей и подписантов
            portOwners =
                conn.PLATFORM_USER_ACCOUNT.Where(a => a.RightsMask == (int) AccountRights.Управление).Take(2).ToArray();
            var lastOwnerId = portOwners[portOwners.Length - 1].PlatformUser;
            portSubs =
                conn.PLATFORM_USER_ACCOUNT.Where(a => a.RightsMask == (int)AccountRights.Управление).Where(a => a.PlatformUser > lastOwnerId).Take(2).ToArray();

            // создать портфели
            portfolios = new []
                {
                    new TOP_PORTFOLIO
                        {
                            Criteria = "AYP",
                            ParticipantCount = 3,
                            OwnerUser = portOwners[0].PlatformUser,
                            Name = "P1"
                        },
                    new TOP_PORTFOLIO
                        {
                            Criteria = "DD",
                            ParticipantCount = 2,
                            OwnerUser = portOwners[1].PlatformUser,
                            Name = "P1"
                        }
                };
            foreach (var pf in portfolios)
                conn.TOP_PORTFOLIO.Add(pf);
            conn.SaveChanges();

            // подписать пользователей на портфели
            var curPortfolioIndex = 0;
            foreach (var usr in portSubs)
            {
                var port = portfolios[curPortfolioIndex++];
                if (curPortfolioIndex == portfolios.Length) curPortfolioIndex = 0;
                conn.USER_TOP_PORTFOLIO.Add(new USER_TOP_PORTFOLIO
                    {
                        User = usr.PlatformUser,
                        Portfolio = port.Id
                    });
            }
            conn.SaveChanges();
        }

        [Test]
        public void TestUpdatePortfolio()
        {
            var firstSubId = portSubs[0].PlatformUser;
            var subUser = conn.PLATFORM_USER.First(u => u.ID == firstSubId);
            var usrPort = conn.USER_TOP_PORTFOLIO.First(p => p.User == subUser.ID);
            var status = TradeSharp.TradeSignalExecutor.BL.PortfolioBalancer.Instance.UpdateTop(subUser.Login, usrPort.Portfolio);
            Assert.AreEqual(RequestStatus.OK, status, "Портфель обновлен без ошибок");
        }

        [Test]
        public void TestUpdateAllPortfolios()
        {
            var rst = TradeSharp.TradeSignalExecutor.BL.PortfolioBalancer.Instance.UpdatePortfolios();
            Assert.IsTrue(rst, "Метод UpdatePortfolios отработал без ошибок");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            Dealer.fakeProxyTrade = null;
        }

        private void MakeFakeTradeProxy()
        {
            Dealer.fakeProxyTrade = ProxyBuilder.Instance.MakeImplementer<ITradeSharpServerTrade>(true);
            ((IMockableProxy)Dealer.fakeProxyTrade).MockMethods.Add(
                StronglyName.GetMethodName<ITradeSharpServerTrade>(s => s.SubscribeOnPortfolio(null, "", null, null, null)),
                new Func<ProtectedOperationContext, string, TopPortfolio, decimal?, AutoTradeSettings, RequestStatus>((
                    secCtx, subscriberLogin, portfolio, maxFee, tradeSets) =>
                    {
                        if (secCtx == null) throw new Exception("SubscribeOnPortfolio - sec ctx is null");
                        if (string.IsNullOrEmpty(subscriberLogin)) throw new Exception("SubscribeOnPortfolio - portfolio is null");
                        if (portfolio == null) throw new Exception("SubscribeOnPortfolio - portfolio is null");
                        if (portfolio.Id == 0) throw new Exception("SubscribeOnPortfolio - portfolio Id is 0");
                        //if (tradeSets == null) throw new Exception("SubscribeOnPortfolio - tradeSets is null");
                        return RequestStatus.OK;
                    })
                );
        }
    }
}
