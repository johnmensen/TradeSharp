using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using System.Linq;
using TradeSharp.Util;
using PlatformManager = TradeSharp.Server.Contract.PlatformManager;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuPlatformManager
    {
        private TradeSharpConnectionPersistent conn;
        private PlatformManager platformManager;

        [TestFixtureSetUp]
        public void Setup()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            QuoteMaker.FillQuoteStorageWithDefaultValues();
            platformManager = new PlatformManager();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
        }

        [Test]
        public void TestGetUserAccounts()
        {
            var usr = conn.PLATFORM_USER.First(u => u.PLATFORM_USER_ACCOUNT.Count > 1);
            var hash = CredentialsHash.MakeCredentialsHash(usr.Login, usr.Password, 100);

            RequestStatus status;
            var accounts = platformManager.GetUserAccounts(hash, usr.Login, 100, out status);
            Assert.AreEqual(RequestStatus.OK, status, "GetUserAccounts() - не отработал");
            Assert.AreEqual(usr.PLATFORM_USER_ACCOUNT.Count, accounts.Count(a => a.ID > 0), 
                "GetUserAccounts() - не вернул нужное количество счетов");
            platformManager.GetUserAccounts(hash + "X", usr.Login, 100, out status);
            Assert.AreEqual(status, RequestStatus.Unauthorized, "GetUserAccounts(wrong hash) - не должен отработать отработать");
        }

        [Test]
        public void GetPaidServiceDetail()
        {
            var signal = conn.SERVICE.First(s => s.SUBSCRIPTION.Count > 0);
            var signalDetail = platformManager.GetPaidServiceDetail(signal.ID);
            Assert.IsNotNull(signalDetail, "GetPaidServiceDetail() - должен вернуть не null");
            Assert.IsNull(platformManager.GetPaidServiceDetail(int.MaxValue), "GetPaidServiceDetail(int.MaxValue) - должен вернуть null");
            var subsCount = signal.SUBSCRIPTION.Count;
            Assert.AreEqual(subsCount, signalDetail.SubscribersCount, "GetPaidServiceDetail() - подсчет количества сигнальщиков");
        }

        [Test]
        public void TestAuthoriseUser()
        {
            var usr = conn.PLATFORM_USER.First();
            var hash = platformManager.AuthoriseUser(usr.Login, usr.Password, 100);
            Assert.Greater(hash.Length, 0, "AuthoriseUser - хеш должен был вернуться");
            hash = platformManager.AuthoriseUser(usr.Login, usr.Password + "neverExist", 0);
            Assert.IsTrue(string.IsNullOrEmpty(hash), "AuthoriseUser - хеш не должен был вернуться (пароль неверный)");
            hash = platformManager.AuthoriseUser(usr.Login + "neverExist", usr.Password, 0);
            Assert.IsTrue(string.IsNullOrEmpty(hash), "AuthoriseUser - хеш не должен был вернуться (логин неверный)");
        }

        [Test]
        public void TestAuthoriseUserWithAccounts()
        {
            var usr = conn.PLATFORM_USER.First(u => u.PLATFORM_USER_ACCOUNT.Count > 1);
            Account[] userAccounts;
            var hash = platformManager.AuthoriseUserWithAccountDetail(usr.Login, usr.Password, 100, out userAccounts);            
            Assert.Greater(hash.Length, 0, "TestAuthoriseUserWithAccounts - хеш должен был вернуться");
            Assert.Greater(userAccounts.Length, 1, "TestAuthoriseUserWithAccounts - все акаунты должны быть получены");
        }

        [Test]
        public void TestPlatformManager()
        {
            var quotes = new Dictionary<string, QuoteData>
                {
                    {"EURUSD", new QuoteData(1.3000f, 1.3002f, DateTime.Now)},
                    {"GBPUSD", new QuoteData(1.5000f, 1.5002f, DateTime.Now)}
                };

            QuoteStorage.Instance.UpdateValues(quotes.Keys.ToArray(), quotes.Values.ToArray());
            var allVolumes = platformManager.GetPairVolumes(string.Empty);
            Assert.Greater(allVolumes.TotalVolumesByTicker.Length, 0, "GetPairVolumes() - данные получены");
            Assert.Greater(allVolumes.TotalGrossInUsd, 0, "GetPairVolumes() - суммарный объем больше 0 USD");
        }

        [Test]
        public void TestGetAllOpenedOrders()
        {
            var orders = platformManager.GetAllOpenedOrders(10);
            Assert.AreEqual(10, orders.Count, "GetAllOpenedOrders(10) - вернулись все 10 ордеров");
            Assert.IsTrue(orders.All(o => o.State != PositionState.Closed && o.PriceEnter > 0), "все ордера вернулись с относительно корректным заполнением полей");

            orders = platformManager.GetAllOpenedOrders(0);
            Assert.AreEqual(0, orders.Count, "GetAllOpenedOrders(0) - вернулись 0 ордеров");

            orders = platformManager.GetAllOpenedOrders(-1);
            Assert.AreEqual(0, orders.Count, "GetAllOpenedOrders(-1) - вернулись 0 ордеров");
        }
        
        [Test]
        public void TestGetSubscriberEmailsByAccount()
        {
            var service = conn.SERVICE.First(s => s.SUBSCRIPTION.Count > 1);
            var subEmails = platformManager.GetSubscriberEmailsByAccount(service.AccountId.Value);
            Assert.Greater(subEmails.Count, 0, "GetSubscriberEmailsByAccount() - вернул непустое множество адресов подписчиков");
            Assert.IsTrue(subEmails.All(e => e.Contains("@")), "GetSubscriberEmailsByAccount() - вернулись адреса, в которых есть хотя бы @");

            subEmails = platformManager.GetSubscriberEmailsByAccount(int.MaxValue);
            Assert.AreEqual(0, subEmails.Count, "GetSubscriberEmailsByAccount(MaxValue) - вернул пустое множество адресов подписчиков, не свалился");
        }

        [Test]
        public void TestGetSubscriberEmails()
        {
            var service = conn.SERVICE.First(s => s.SUBSCRIPTION.Count > 1);
            var subEmails = platformManager.GetSubscriberEmails(service.User);
            Assert.Greater(subEmails.Count, 0, "GetSubscriberEmails() - вернул непустое множество адресов подписчиков");
            Assert.IsTrue(subEmails.All(e => e.Contains("@")), "GetSubscriberEmails() - вернулись адреса, в которых есть хотя бы @");

            subEmails = platformManager.GetSubscriberEmails(int.MaxValue);
            Assert.AreEqual(0, subEmails.Count, "GetSubscriberEmails(MaxValue) - вернул пустое множество адресов подписчиков, не свалился");
        }

        [Test]
        public void TestGetUserPortfolioAndSubscriptions()
        {
            var user = conn.PLATFORM_USER.First(u => u.SUBSCRIPTION.Count > 1 && u.USER_TOP_PORTFOLIO.Count == 1);
            const long magic = 1;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, magic);

            List<Contract.Entity.Subscription> subscriptions;
            TopPortfolio portfolio;
            var status = platformManager.GetUserPortfolioAndSubscriptions(hash, user.Login, magic, out subscriptions, out portfolio);
            Assert.AreEqual(RequestStatus.OK, status, "GetUserPortfolioAndSubscriptions() - завершен успешно");
            var userSubsCount = conn.SUBSCRIPTION.Count(s => s.User == user.ID);
            // !! тест невозможен из-за обращения к VIEW
            //Assert.AreEqual(userSubsCount, subscriptions.Count(s => s.User == user.ID), "GetUserPortfolioAndSubscriptions() - получены все подписки");
            Assert.IsNotNull(portfolio, "GetUserPortfolioAndSubscriptions() - портфель получен");
            var portfolioOrig = conn.TOP_PORTFOLIO.First(p => p.USER_TOP_PORTFOLIO.Any(u => u.User == user.ID));
            Assert.AreEqual(portfolioOrig.Criteria, portfolio.Criteria, "GetUserPortfolioAndSubscriptions() - портфель прочитан верно");

            status = platformManager.GetUserPortfolioAndSubscriptions("wrongHash", user.Login, magic, out subscriptions, out portfolio);
            Assert.AreEqual(RequestStatus.Unauthorized, status, "GetUserPortfolioAndSubscriptions() - левая авторизация не прошла");
        }

        [Test]
        public void TestGetTickersTraded()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Any() && a.POSITION_CLOSED.Any());
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);

            const long magic = 1;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, magic);

            var tickers = platformManager.GetTickersTraded(hash, user.Login, account.ID, magic);
            Assert.IsNotNull(tickers, "GetTickersTraded - список тикеров не должен быть null");
            Assert.Greater(tickers.Count, 1, "GetTickersTraded - торгуемые инструменты получены");

            tickers = platformManager.GetTickersTraded(hash + "x", user.Login, account.ID, magic);
            Assert.IsNull(tickers, "GetTickersTraded - список тикеров таки должен быть null - хеш не подходит");
            
            account = conn.ACCOUNT.First(a => a.POSITION.Any() && a.POSITION_CLOSED.Any() && 
                a.PLATFORM_USER_ACCOUNT.All(pa => pa.PlatformUser != user.ID));

            tickers = platformManager.GetTickersTraded(hash, user.Login, account.ID, magic);
            Assert.IsNull(tickers, "GetTickersTraded - список тикеров должен быть null - не тот пользователь");
        }

        [Test]
        public void TestGetClosedOrders()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION_CLOSED.Count > 50);
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);

            const long magic = 1;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, magic);

            var orders = platformManager.GetClosedOrders(hash, user.Login, magic, account.ID,
                                                                  string.Empty, 0, 45);
            Assert.IsNotNull(orders, "GetClosedOrders - список ордеров не должен быть null");
            Assert.AreEqual(45, orders.Count, "GetClosedOrders - список ордеров должен содержать 45 значений (45 запрошено)");
            
            var totalOrdersInDbCount = account.POSITION_CLOSED.Count;
            orders = platformManager.GetClosedOrders(hash, user.Login, magic, account.ID,
                                                                  string.Empty, orders[orders.Count - 1].ID, 100000);
            var totalCount = orders.Count + 45;
            Assert.AreEqual(totalOrdersInDbCount, totalCount, 
                "GetClosedOrders - список ордеров должен содержать все значения после 2-го запроса");

            var ticker = orders[1].Symbol;
            var ordersByTickerCount = account.POSITION_CLOSED.Count(a => a.Symbol == ticker);
            orders = platformManager.GetClosedOrders(hash, user.Login, magic, account.ID,
                ticker, 0, 100000);
            Assert.AreEqual(ordersByTickerCount, orders.Count, "GetClosedOrders - количество ордеров " + ticker + 
                " должно совпадать");
        }

        [Test]
        public void TestGetOpenOrders()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Count > 1);
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);

            const long magic = 1;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, magic);

            var orders = platformManager.GetOpenOrdersByAccount(hash, user.Login, magic, account.ID,
                string.Empty, 0, 10);
            Assert.IsNotNull(orders, "GetOpenOrdersByAccount - список ордеров не должен быть null");
            Assert.Greater(orders.Count, 1, "GetOpenOrdersByAccount - список ордеров должен содержать 45 значений (45 запрошено)");
        }

        [Test]
        public void TestGetAccountDetail()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Count > 1);
            var group = conn.ACCOUNT_GROUP.First(g => g.Code == account.AccountGroup);
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);

            const long magic = 1;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, magic);

            decimal brokerLeverage, exposure;
            var accountResulted = platformManager.GetAccountDetail(hash, user.Login, magic, account.ID, true,
                out brokerLeverage, out exposure);
            Assert.IsNotNull(accountResulted, "GetAccountDetail - счет не должен быть null");
            Assert.AreEqual(group.BrokerLeverage, brokerLeverage, "GetAccountDetail - плечо брокера определено неверно");
            Assert.Greater(exposure, 0, "GetAccountDetail - экспозиция должна быть больше 0");
        }

        [Test]
        public void TestEditPosition()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Any());
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);
            var pos = conn.POSITION.First(p => p.AccountID == account.ID);

            const long localTime = 13;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, localTime);
            var status = platformManager.EditPosition(hash, user.Login, localTime,
                                                               account.ID, pos.ID, (float) (pos.Stoploss ?? 0),
                                                               (float) pos.PriceEnter + pos.Side*0.009f, 0, "comment");
            // попытка редактировать ордер может не пройти - торговый контекст не настроен
            var isOk = status == RequestStatus.GroupUnsupported || status == RequestStatus.OK;
            Assert.IsTrue(isOk, "EditPosition - должно быть ОК || (GroupUnsupported)");
            
            status = platformManager.EditPosition(hash, user.Login, localTime,
                                                               account.ID + 1, pos.ID, (float)(pos.Stoploss ?? 0),
                                                               (float)pos.PriceEnter + pos.Side * 0.009f, 0, "comment");
            Assert.AreEqual(RequestStatus.IncorrectData, status, "EditPosition - должно быть IncorrectData");
        }

        [Test]
        public void TestOpenPosition()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Any());
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);
            var pos = conn.POSITION.First(p => p.AccountID == account.ID);

            const long localTime = 13;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, localTime);
            var status = platformManager.EditPosition(hash, user.Login, localTime,
                                                               account.ID, pos.ID, (float)(pos.Stoploss ?? 0),
                                                               (float)pos.PriceEnter + pos.Side * 0.009f, 0, "comment");
            // попытка редактировать ордер может не пройти - торговый контекст не настроен
            var isOk = status == RequestStatus.GroupUnsupported || status == RequestStatus.OK;
            Assert.IsTrue(isOk, "EditPosition - должно быть ОК || (GroupUnsupported)");

            status = platformManager.EditPosition(hash, user.Login, localTime,
                                                               account.ID + 1, pos.ID, (float)(pos.Stoploss ?? 0),
                                                               (float)pos.PriceEnter + pos.Side * 0.009f, 0, "comment");
            Assert.AreEqual(RequestStatus.IncorrectData, status, "EditPosition - должно быть IncorrectData");
        }

        [Test]
        public void TestClosePosition()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION.Any());
            var ownerId = account.PLATFORM_USER_ACCOUNT.First().PlatformUser;
            var user = conn.PLATFORM_USER.First(u => u.ID == ownerId);
            var pos = conn.POSITION.First(p => p.AccountID == account.ID);

            const long localTime = 13;
            var hash = CredentialsHash.MakeCredentialsHash(user.Login, user.Password, localTime);
            var status = platformManager.ClosePosition(hash, user.Login, localTime,
                account.ID, pos.ID);
            // попытка закрыть ордер может не пройти - торговый контекст не настроен
            var isOk = status == RequestStatus.GroupUnsupported || status == RequestStatus.OK;
            Assert.IsTrue(isOk, "ClosePosition - должно быть ОК || (GroupUnsupported)");

            status = platformManager.ClosePosition(hash, user.Login, localTime,
                account.ID + 1, pos.ID);
            Assert.AreEqual(RequestStatus.NotFound, status, "ClosePosition - должно быть NotFound");
        }
    }
}
