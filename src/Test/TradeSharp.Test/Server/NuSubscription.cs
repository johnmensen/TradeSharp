using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Test.Moq;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuSubscription
    {
        private TradeSharpConnectionPersistent conn;
        private PLATFORM_USER subscriber;

        private PLATFORM_USER serviceOwnerUsd, serviceOwnerRub;
        private SERVICE serviceUsd, serviceRub;
        private const string SubscriberCurx = "USD";

        [TestFixtureSetUp]
        public void Setup()
        {
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            ManagerTrade.MakeTestInstance();
            subscriber = (from usr in conn.PLATFORM_USER 
                          join w in conn.WALLET on usr.ID equals w.User
                          join pa in conn.PLATFORM_USER_ACCOUNT on usr.ID equals pa.PlatformUser
                          where pa.RightsMask == (int)AccountRights.Управление && w.Currency == SubscriberCurx && w.Balance > 40
                          select usr).First();
            
            CreateServiceWithOwner("USD", 5, out serviceOwnerUsd, out serviceUsd);
            CreateServiceWithOwner("RUB", 25, out serviceOwnerRub, out serviceRub);
            QuoteStorage.Instance.UpdateValues("USDRUB", new QuoteData(35.20f, 35.20f, DateTime.Now));
            
            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
        }

        private void CreateServiceWithOwner(string currency, decimal fee, out PLATFORM_USER serviceOwner, out SERVICE service)
        {
            // создать для будущего подписчика сервиса
            // для начала - создать владельца сервиса
            serviceOwner = new PLATFORM_USER
                {
                    Email = "testuser" + currency + "@t.test",
                    Login = "TestUserTest" + currency,
                    Password = "TestUserTest",
                    Title = "test",
                    RegistrationDate = DateTime.Now
                };
            conn.PLATFORM_USER.Add(serviceOwner);
            conn.SaveChanges();
            conn.WALLET.Add(new WALLET
                {
                    Currency = currency,
                    User = serviceOwner.ID,
                    Password = "SO1200000012",
                    Balance = 500
                });

            // и, собственно, сервис
            service = new SERVICE
                {
                    User = serviceOwner.ID,
                    ServiceType = (int) PaidServiceType.Signals,
                    Comment = "test_" + currency,
                    Currency = currency,
                    FixedPrice = fee
                };
            conn.SERVICE.Add(service);
            conn.SaveChanges();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
        }

        [Test]
        public void TestSubscribeOnServiceChargesRightMoney()
        {
            var tradeManager = ManagerTrade.Instance;
            TestSubscriptionFeeCharged(tradeManager, serviceUsd, serviceOwnerUsd);
        }

        [Test]
        public void TestSubscribeOnServiceChargesRightMoneyWhenCurrenciesAreDifferent()
        {
            var tradeManager = ManagerTrade.Instance;
            TestSubscriptionFeeCharged(tradeManager, serviceRub, serviceOwnerRub);
        }

        [Test]
        public void TestSubscribeOnHimself()
        {
            var tradeManager = ManagerTrade.Instance;
            WalletError error;
            tradeManager.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                            serviceOwnerUsd.Login, serviceUsd.ID, true, false, new AutoTradeSettings(),
                                            out error);
            Assert.AreEqual(WalletError.InvalidData, error, "Нельзя подписываться на себя, любимого");
        }

        [Test]
        public void TestSubscribeThenUpdateSettingsThenUnsubscribe()
        {
            // проверить наличие отсутствия подписки
            RemoveSubscriptionIfAny(serviceUsd.ID);
            var tradeManager = ManagerTrade.Instance;
            
            WalletError error;
            var tradeSets = AutoTradeSettingsSampler.MakeSampleTradeSettings();
            tradeSets.TargetAccount = conn.PLATFORM_USER_ACCOUNT.First(pa => pa.PlatformUser == subscriber.ID).Account;

            tradeManager.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                            subscriber.Login, serviceUsd.ID, true, false, tradeSets,
                                            out error);
            Assert.AreEqual(WalletError.OK, error, "Тест подписки/модификации/отписки: подписка прошла");
            var signalSubSets = conn.SUBSCRIPTION_SIGNAL.FirstOrDefault(v => v.User == subscriber.ID);
            Assert.IsNotNull(signalSubSets, "Тест подписки/модификации/отписки: настройки созданы");
            var wrongFields = AutoTradeSettingsSampler.TradeSignalSetsAreCorrect(tradeSets, signalSubSets);
            if (!string.IsNullOrEmpty(wrongFields))
                Assert.Fail("Тест подписки/модификации/отписки: настройки не сохранены: " + wrongFields);

            tradeSets.MinVolume = 10000;
            tradeSets.MaxLeverage = 8;
            tradeSets.StepVolume = 10000;
            // модифицировать настройки авто-торговли
            tradeManager.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                            subscriber.Login, serviceUsd.ID, true, false, tradeSets,
                                            out error);
            Assert.AreEqual(WalletError.OK, error, "Тест подписки/модификации/отписки: обновление прошло");
            
            signalSubSets = conn.SUBSCRIPTION_SIGNAL.FirstOrDefault(v => v.User == subscriber.ID);
            wrongFields = AutoTradeSettingsSampler.TradeSignalSetsAreCorrect(tradeSets, signalSubSets);
            if (!string.IsNullOrEmpty(wrongFields))
                Assert.Fail("Тест подписки/модификации/отписки: настройки не обновлены: " + wrongFields);

            // отписаться
            tradeManager.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                            subscriber.Login, serviceUsd.ID, true, true, tradeSets,
                                            out error);
            Assert.AreEqual(WalletError.OK, error, "Тест подписки/модификации/отписки: удаление подписки прошло");
            Assert.IsFalse(conn.SUBSCRIPTION_SIGNAL.Any(ss => ss.User == subscriber.ID && ss.Service == serviceUsd.ID), "Тест отписки: SUBSCRIPTION_SIGNAL удалена");
            Assert.IsFalse(conn.SUBSCRIPTION.Any(ss => ss.User == subscriber.ID && ss.Service == serviceUsd.ID), "Тест отписки: SUBSCRIPTION удалена");
        }

        private void TestSubscriptionFeeCharged(ManagerTrade tradeManager, SERVICE service, PLATFORM_USER serviceOwner)
        {
            RemoveSubscriptionIfAny(service.ID);

            var cacheInWallet = GetUserCacheAmount(subscriber.ID);
            var ownerCache = GetUserCacheAmount(serviceOwner.ID);
            WalletError error;
            tradeManager.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                            subscriber.Login, service.ID, true, false, new AutoTradeSettings(),
                                            out error);
            Assert.AreEqual(WalletError.OK, error, "Тестовая подписка должна пройти успешно");
            var cacheInWalletAfter = GetUserCacheAmount(subscriber.ID);
            var ownerCacheAfter = GetUserCacheAmount(serviceOwner.ID);

            var curxRate = service.Currency == SubscriberCurx
                               ? new QuoteData(1, 1, DateTime.Now)
                               : QuoteStorage.Instance.ReceiveValue("USDRUB");
            var userLost = service.FixedPrice / (decimal)curxRate.bid;
            var ownerGot = service.FixedPrice;

            Assert.IsTrue((cacheInWallet - userLost).SameMoney(cacheInWalletAfter),
                          "Подписка сняла оговоренную сумму (валюта одна)");
            Assert.IsTrue((ownerCache + ownerGot).SameMoney(ownerCacheAfter),
                          "Подписка принесла владельцу оговоренную сумму (валюта одна)");
        }

        private decimal GetUserCacheAmount(int userId)
        {
            return conn.WALLET.First(w => w.User == userId).Balance;
        }
    
        private void RemoveSubscriptionIfAny(int serviceId)
        {
            var oldSubs = conn.SUBSCRIPTION.FirstOrDefault(s => s.User == subscriber.ID && s.Service == serviceId);
            if (oldSubs != null)
            {
                conn.SUBSCRIPTION.Remove(oldSubs);
                conn.SaveChanges();
            }
        }           
    }
}
