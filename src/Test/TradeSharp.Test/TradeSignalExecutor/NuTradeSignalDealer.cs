using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.TradeSignalExecutor.BL;
using TradeSharp.Util;

namespace TradeSharp.Test.TradeSignalExecutor
{
    [TestFixture]
    public class TradeSignalDealer
    {
        private TradeSharpConnectionPersistent connection;

        private int testAccountId;

        private int testUserId;

        private int serviceId = 150001;

        private const string AccountCurrency = "USD";

        private List<MarketOrder> ordersForTest;

        private readonly Dictionary<int, MarketOrder> marketRequests = new Dictionary<int, MarketOrder>();
        
        [TestFixtureSetUp]
        public void Setup()
        {
            // подыграть за торговый контракт
            SetupFakeServer();

            // инициализировать словари (прежде всего - словарь тикеров)
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            connection = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();

            // пользователь - владелец тестового сервиса
            var ownerUser = new PLATFORM_USER
                {
                    Email = "yatester111@test.com",
                    Login = "yatester111",
                    Title = "Vafel",
                    RoleMask = 0,
                    Password = "yasxucotester",
                    RegistrationDate = DateTime.Now
                };
            connection.PLATFORM_USER.Add(ownerUser);
            connection.SaveChanges();
            
            // добавить категорию сигналов
            var srv = new SERVICE
                {
                    FixedPrice = 0,
                    Currency = "USD",
                    User = ownerUser.ID
                };
            connection.SERVICE.Add(srv);
            connection.SaveChanges();
            serviceId = srv.ID;
            
            // добавить пользователя
            var user = new PLATFORM_USER
                {
                    Email = "testusertest@t.test",
                    Login = "TestUserTest",
                    Password = "TestUserTest",
                    Title = "test",
                    RegistrationDate = DateTime.Now
                };
            connection.PLATFORM_USER.Add(user);
            connection.SaveChanges();
            testUserId = user.ID;

            // добавить счет и сделок
            var group = connection.ACCOUNT_GROUP.First(g => !g.IsReal);
            var account = new ACCOUNT
                {
                    AccountGroup = group.Code,
                    Currency = AccountCurrency,
                    Balance = 30000
                };
            connection.ACCOUNT.Add(account);
            connection.SaveChanges();
            testAccountId = account.ID;

            // назначить пользователя владельцем счета
            connection.PLATFORM_USER_ACCOUNT.Add(new PLATFORM_USER_ACCOUNT
                {
                    Account = testAccountId,
                    PlatformUser = testUserId,
                    RightsMask = (int) AccountRights.Управление
                });

            // подписать счет на сигнал
            connection.SUBSCRIPTION.Add(new SUBSCRIPTION
                {
                    User = testUserId,
                    Service = serviceId,
                    RenewAuto = true,
                    TimeEnd = DateTime.Now.Date.AddDays(1)
                });
            connection.SUBSCRIPTION_SIGNAL.Add(new SUBSCRIPTION_SIGNAL
            {
                User = testUserId,
                Service = serviceId,
                AutoTrade = true,
                PercentLeverage = 120,
                MinVolume = 10000,
                TargetAccount = testAccountId
            });
            connection.SaveChanges();

            // позиции
            MakeOrdersForTest();
            foreach (var order in ordersForTest)
            {
                var pos = LinqToEntity.UndecorateOpenedPosition(order);
                connection.POSITION.Add(pos);
            }
            connection.SaveChanges();

            // прописать срез котировок
            var nowTime = DateTime.Now;
            Contract.Util.BL.QuoteStorage.Instance.UpdateValues(new []
                {
                    "EURUSD", "GBPUSD", "USDJPY", "EURGBP"
                }, 
                new []
                    {
                        new QuoteData(1.3820f, 1.3822f, nowTime), 
                        new QuoteData(1.5350f, 1.5354f, nowTime), 
                        new QuoteData(90.81f, 90.83f, nowTime),
                        new QuoteData(1.1107f, 1.1112f, nowTime)
                    }
                );
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            connection.Cleanup();
            Dealer.fakeProxyTrade = null;
        }

        [Test]
        public void TestCalcVolume()
        {
            var action = new TradeSignalActionTrade
                {
                    Leverage = 1,
                    OrderId = 9999999,
                    Price = 1.3820f,
                    Side = -1,
                    ServiceId = serviceId,
                    Ticker = "EURUSD"
                };
            var tradeSets = new SUBSCRIPTION_SIGNAL
                {
                    TargetAccount = testAccountId,
                    AutoTrade = true,
                    MaxLeverage = 10,
                    MaxVolume = 100000,
                    PercentLeverage = 120,
                    HedgingOrdersEnabled = true,
                    User = 0
                };

            // расчет объема обычного ордера
            var volume = Dealer.GetNewOrderVolume(action, connection, tradeSets);
            Assert.Greater(volume, 0, "TradeSignalDealer - объем больше 0");

            // превышение плеча
            action.Leverage = 4.1M;
            tradeSets.MaxLeverage = 5;
            volume = Dealer.GetNewOrderVolume(action, connection, tradeSets);
            Assert.AreEqual(volume, 0, "TradeSignalDealer - объем 0 (превышение плеча)");

            // просто сделка по йене
            action.Ticker = "USDJPY";
            action.Price = 92.1f;
            action.Leverage = 2;
            tradeSets.MaxLeverage = 7.6f;
            volume = Dealer.GetNewOrderVolume(action, connection, tradeSets);
            Assert.AreEqual(70000, volume, "TradeSignalDealer - объем USDJPY больше 0");
        }

        [Test]
        public void TestMakeTrade()
        {
            marketRequests.Clear();
            Dealer.ProcessSignal(new TradeSignalActionTrade
                {
                    Leverage = 0.75M,
                    OrderId = 550001,
                    Side = 1,
                    Ticker = "EURUSD",
                    Price = 1.3824f,
                    ServiceId = serviceId,
                    StopLoss = 1.35M,
                });
            Assert.AreEqual(0, marketRequests.Count, "Отправка торгового сигнала не прошла - есть встречные ордера");

            Dealer.ProcessSignal(new TradeSignalActionTrade
            {
                Leverage = 0.68M,
                OrderId = 550002,
                Side = -1,
                Ticker = "EURGBP",
                Price = 1.1109f,
                ServiceId = serviceId,
                StopLoss = 1.21M
            });
            Assert.AreEqual(1, marketRequests.Count, "Отправка торгового сигнала (EURGBP) прошла");
        }

        private void MakeOrdersForTest()
        {
            ordersForTest = new List<MarketOrder>
                {
                    new MarketOrder
                        {
                            Symbol = "EURUSD",
                            Volume = 10000,
                            Side = 1,
                            PriceEnter = 1.3290f,
                            TimeEnter = DateTime.Now.AddMinutes(-60*24*3),
                            State = PositionState.Opened,
                            ExpertComment = MarketOrder.MakeSignalComment(serviceId),
                            MasterOrder = 10001
                        },
                    new MarketOrder
                        {
                            Symbol = "EURUSD",
                            Volume = 40000,
                            Side = -1,
                            PriceEnter = 1.3240f,
                            TimeEnter = DateTime.Now.AddMinutes(-60*100),
                            State = PositionState.Opened,
                            ExpertComment = MarketOrder.MakeSignalComment(serviceId),
                            //MasterOrder = 10002
                        },
                    new MarketOrder
                        {
                            Symbol = "USDJPY",
                            Volume = 20000,
                            Side = 1,
                            PriceEnter = 90.67f,
                            TimeEnter = DateTime.Now.AddMinutes(-23*100),
                            State = PositionState.Opened,
                            ExpertComment = MarketOrder.MakeSignalComment(serviceId),
                            MasterOrder = 10003
                        },
                    new MarketOrder
                        {
                            Symbol = "EURGBP",
                            Volume = 20000,
                            Side = -1,
                            PriceEnter = 1.2000f,
                            TimeEnter = DateTime.Now.AddMinutes(-117*100),
                            State = PositionState.Opened,
                            ExpertComment = MarketOrder.MakeSignalComment(serviceId),
                            MasterOrder = 10004
                        }
                };
            ordersForTest.ForEach(o => o.AccountID = testAccountId);
        }
    
        private void SetupFakeServer()
        {
            var fakeProxy = ProxyBuilder.Instance.MakeImplementer<ITradeSharpServerTrade>(true);
            Dealer.fakeProxyTrade = fakeProxy;

            // "замочить" нужные методы
            // ReSharper disable SuspiciousTypeConversion.Global
            ((IMockableProxy) fakeProxy).MockMethods.Add(
            // ReSharper restore SuspiciousTypeConversion.Global
                StronglyName.GetMethodName<ITradeSharpServerTrade>(
                    f => f.SendNewOrderRequest(null, 0, null, OrderType.Market, 0, 0)), // название метода - SendNewOrderRequest
                // тело мок-метода
                new Func<ProtectedOperationContext, int, MarketOrder, OrderType, decimal, decimal, RequestStatus>((
                    ctx, requestUniqueId, order, orderType, requestedPrice, slippagePoints) =>
                    {
                        marketRequests.Add(requestUniqueId, order);
                        return RequestStatus.OK;
                    }));
        }
    }
}
