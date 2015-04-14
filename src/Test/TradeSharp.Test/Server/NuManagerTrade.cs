using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.DealerInterface;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuManagerTrade
    {
        private TradeSharpConnectionPersistent conn;
        public ManagerTrade managerTrade;
        public ACCOUNT testAccount;
        public List<POSITION> testOrders;

        [TestFixtureSetUp]
        public void Setup()
        {
            // подготовить свежие котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // соединение и тестовые данные
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            MakeTestData();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            // тестируемый менеджер
            ManagerTrade.MakeTestInstance(
                new Dictionary<string, IDealer>
                    {
                        { testAccount.AccountGroup, 
                            new DemoDealer.DemoDealer(new DealerDescription(), conn.ACCOUNT_GROUP.Select(g => g.Code).ToList())}
                    });
            managerTrade = ManagerTrade.Instance;
            var mockAccountRepository = new Mock<IAccountRepository>();
            mockAccountRepository.Setup(s => s.GetAccount(It.IsAny<int>())).Returns(
                (int id) => LinqToEntity.DecorateAccount(testAccount));
            managerTrade.accountRepository = mockAccountRepository.Object;

            managerTrade.orderRepository = OrderRepository.Instance;
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            ManagerTrade.MakeTestInstance();
        }  
    
        [Test]
        public void TestSendCloseByTickerRequest()
        {
            var ticker = testOrders[0].Symbol;
            var status = managerTrade.SendCloseByTickerRequest(ProtectedOperationContext.MakeServerSideContext(),
                                                               testAccount.ID, ticker, PositionExitReason.ClosedFromUI);
            Assert.AreEqual(RequestStatus.OK, status, "SendCloseByTickerRequest (" + ticker + ") - позиции должны быть закрыты");
        }

        private void MakeTestData()
        {
            testAccount = conn.ACCOUNT.First(a => !a.POSITION.Any() && a.Balance >= 10000 && a.Currency == "USD");
            // создать тестовые ордера
            // создать сделки по счету
            testOrders = PositionMaker.MakePositions(conn, testAccount.ID);
            conn.SaveChanges();
        }
    }
}
