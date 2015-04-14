using System;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Test.Moq;
using TradeSharp.TradeLib;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuBillingManager
    {
        private TradeSharpConnectionPersistent conn;
        private TradeManager tradeManager;
        private ACCOUNT account;
        
        [TestFixtureSetUp]
        public void InitTest()
        {
            // забить котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            tradeManager = new TradeManager(
                null,
                null,
                QuoteStorage.Instance, accountId =>
                {
                    // ReSharper disable ConvertToLambdaExpression
                    return LinqToEntity.DecorateAccountGroup(conn.ACCOUNT.First(a => a.ID == accountId).ACCOUNT_GROUP);
                    // ReSharper restore ConvertToLambdaExpression
                });

            // временно удалить файл начального наполнения (открытые ордера)
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
            TradeSharpConnectionPersistent.RenameCsvFilesContainingAccountDataInMoqDbFolder();
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();

            MakeTestData();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
        }    

        [Test]
        public void TestProcessOrderOpening()
        {
            var order = MakeTestOrder();
            var bill = BillingManager.ProcessOrderOpening(order, LinqToEntity.DecorateAccount(account));
            Assert.IsNotNull(bill, "ProcessOrderOpening - bill не должен быть null");

            var pointCost = DalSpot.Instance.GetAbsValue(order.Symbol, 1f);
            var amountCounter = pointCost * 1; // order.Volume;
            var delta = Math.Abs(amountCounter - bill.MarkupEnter);
            Assert.Less(delta, 0.01, "ProcessOrderOpening - при открытии должна быть списана корректная сумма");
        }

        [Test]
        public void TestProcessOrderClosing()
        {
            var order = MakeTestOrder();
            var bill = BillingManager.ProcessOrderOpening(order, LinqToEntity.DecorateAccount(account));
            Assert.IsNotNull(bill, "ProcessPriceForOrderClosing - открытие не обработано");
            BillingManager.SaveNewOrderBill(bill, order.ID, conn);
            conn.SaveChanges();
            
            var billClose = BillingManager.ProcessPriceForOrderClosing(order, 
                LinqToEntity.DecorateAccount(account), conn);
            Assert.IsNotNull(billClose, "ProcessPriceForOrderClosing - bill не должен быть null");
        }

        private void MakeTestData()
        {
            var group = conn.ACCOUNT_GROUP.First(g => !g.MARKUP_BY_GROUP.Any());
            group.MarkupType = (int) AccountGroup.MarkupType.Markup;
            // прописать маркап для группы
            foreach (var ticker in DalSpot.Instance.GetTickerNames())
                conn.MARKUP_BY_GROUP.Add(new MARKUP_BY_GROUP
                    {
                        Group = group.Code,
                        Spot = ticker,
                        MarkupAbs = DalSpot.Instance.GetAbsValue(ticker, 1.0f)
                    });
            // создать счет
            account = new ACCOUNT
                {
                    Balance = 50000,
                    Currency = "USD",
                    AccountGroup = group.Code,
                    Status = (int) Account.AccountStatus.Created,
                    Description = "test"
                };
            conn.SaveChanges();
        }
    
        private MarketOrder MakeTestOrder()
        {
            return new MarketOrder
                {
                    ID = 50000,
                    AccountID = account.ID,
                    Symbol = "EURUSD",
                    Volume = 150000,
                    Side = -1,
                    TimeEnter = DateTime.Now.AddMinutes(-3010),
                    PriceEnter = 1.3600f,
                    PriceExit = 1.3640f,
                    TimeExit = DateTime.Now,
                    State = PositionState.Closed
                };
        }
    }
}
