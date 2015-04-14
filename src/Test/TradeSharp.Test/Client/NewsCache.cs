using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuNewsCache
    {
        delegate RequestStatus GetAccountInfoDel(int accountId, bool needEquityInfo, out Account account);
        private ITradeSharpAccount fakeTradeAccount;
        private ITradeSharpServerTrade  fakeTradeServer;

        private readonly List<Account> accounts = new List<Account>
            {
                new Account
                    {
                        ID = 1,
                        Balance = 152300,
                        Currency = "USD",
                        Equity = 151080,
                        Group = "Demo",
                        MaxLeverage = 5,
                        Status = Account.AccountStatus.Created                        
                    },
                new Account
                    {
                        ID = 2,
                        Balance = 10000,
                        Currency = "USD",
                        Equity = 10000,
                        Group = "Demo",
                        Status = Account.AccountStatus.Created                        
                    }
            };

        [SetUp]
        public void Setup()
        {
            NewsMaker.ClearNewsFolder();

            fakeTradeServer = ProxyBuilder.Instance.GetImplementer<ITradeSharpServerTrade>();
            fakeTradeAccount = ProxyBuilder.Instance.GetImplementer<ITradeSharpAccount>();

            Account ac;
            var methodName = ProxyBuilder.GetMethodName<ITradeSharpAccount>(a => a.GetAccountInfo(1, false, out ac));
            ((IMockableProxy)fakeTradeAccount).MockMethods.Add(methodName,
                new GetAccountInfoDel((int accountId, bool needEquityInfo, out Account account) =>
                {
                    account = accounts.FirstOrDefault(a => a.ID == accountId);
                    return RequestStatus.OK;
                }));

            TradeSharpServerTradeProxy.fakeProxy = fakeTradeServer;
        }

        [TearDown]
        public void TearDown()
        {
            TradeSharpServerTradeProxy.fakeProxy = null;
        }

        [Test]
        public void TestLoad()
        {
            // очистить локальный кеш
            NewsLocalStorage.ReInstantiate();
            
            var fakeNewsServer = MakeFakeNewsServer();
            // подготовить тестовые новости
            NewsStorageProxy.fakeChannel = fakeNewsServer;

            // "залогиниться"
            var firstAccountId = fakeNewsServer.channelsByAccount.Keys.First();
            Account account;
            fakeTradeAccount.GetAccountInfo(firstAccountId, true, out account);
            AccountStatus.Instance.AccountData = account;

            // запустить актуализацию
            NewsCache.Instance.ActualizeAsync();

            // дождаться завершения...
            NewsCache.Instance.syncCompletedEvent.Wait();

            // содержимое кеша должно соответствовать исходному наполнению
            CompareStoredAndServerNews(fakeNewsServer, firstAccountId, "NewsStorageProxy first load: all news are loaded");

            // дополнить сервер несколькими новостями
            fakeNewsServer.AddNews(1, 
                new List<News> 
                {
                    NewsMaker.MakeSingleNews(1, DateTime.Now.AddMinutes(-30)),
                    NewsMaker.MakeSingleNews(1, DateTime.Now.AddMinutes(-28)),
                });
            fakeNewsServer.AddNews(3,
                new List<News> { NewsMaker.MakeSingleNews(3, DateTime.Now.AddMinutes(-21)) });

            // запустить актуализацию
            NewsCache.Instance.ActualizeAsync();

            // дождаться завершения...
            NewsCache.Instance.syncCompletedEvent.Wait();

            // содержимое кеша снова должно соответствовать исходному наполнению
            CompareStoredAndServerNews(fakeNewsServer, firstAccountId, "NewsStorageProxy second load: all news are loaded");
        }

        private static void CompareStoredAndServerNews(FakeNewsServer fakeNewsServer, int accountId, string msg)
        {
            foreach (var channel in fakeNewsServer.channelsByAccount[accountId])
            {
                var cachedNews = NewsLocalStorage.Instance.GetNews(channel);
                var serverNews = fakeNewsServer.newsByChannel[channel];
                Assert.IsTrue(cachedNews.SequenceEqual(serverNews), msg);
            }
        }

        private static FakeNewsServer MakeFakeNewsServer()
        {
            return new FakeNewsServer(new Dictionary<int, int[]>
                {
                    { 1, new[] { 1, 2, 3 } },
                    { 2, new[] { 1 } }
                });
        }
    }
}
