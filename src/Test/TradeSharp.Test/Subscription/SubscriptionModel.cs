using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeSharp.Client.Util;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;

namespace TradeSharp.Test.Subscription
{
    [TestFixture]
    public class NuSubscriptionModel
    {
        delegate bool GetAccountInfoDel(ProtectedOperationContext secCtx,
            string login, int serviceId, bool renewAuto, bool unsubscribe,
            AutoTradeSettings tradeSets, out WalletError error);
        private List<Contract.Entity.Subscription> serverCats;
        private ITradeSharpAccount fakeTradeAccount;
        private ITradeSharpServerTrade fakeTradeServer;

        [SetUp]
        public void SetupTest()
        {
            MakeMoq();
            AccountModel.Instance.Initialize(() => new Account
                {
                    Balance = 10000,
                    Currency = "USD",
                    Equity = 10000,
                    Group = "Demo",
                    ID = 3,
                    Status = Account.AccountStatus.Created
                },
                () => "Demo",
                stat => { },
                fakeTradeServer, // server proxy
                null);// chat proxy
            AccountStatus.Instance.Login = "Demo";
            SubscriptionModel.Instance.ServerProxy = fakeTradeServer;
            TradeSharpAccount.Initialize(fakeTradeAccount);
        }

        [Test]
        public void TestModifyingSubscriptions()
        {
            var modelIsLoadedCalledCount = 0;
            SubscriptionModel.Instance.ModelIsLoaded += list =>
            {
                modelIsLoadedCalledCount++;
            };
            var modelStateChangedCalledCount = 0;
            SubscriptionModel.Instance.ModelStateChanged += b =>
            {
                modelStateChangedCalledCount++;
            };

            // прочитать подписку
            try
            {
                SubscriptionModel.Instance.LoadSubscribedCategories();
            }
            catch (Exception ex)
            {
                Assert.Fail("LoadSubscribedCategories() failed: " + ex.Message);
            }

            var cats = SubscriptionModel.Instance.SubscribedCategories;
            Assert.IsNotNull(cats, "cats from server are not null");
            Assert.AreEqual(1, cats.Count, "cats from server are not empty");
            Assert.AreEqual(1, modelIsLoadedCalledCount, "model loaded just once");
            Assert.AreEqual(0, modelStateChangedCalledCount, "model state is not changed");

            // обновить подписку
            var newCat = new Contract.Entity.Subscription
            {
                Service = 2,
                PaidService = new PaidService
                    {
                        Id = 2,
                        Comment = "+100500 profit"
                    }
            };
            SubscriptionModel.Instance.AddSubscription(newCat);
            var firstCat = SubscriptionModel.Instance.SubscribedCategories[0];
            firstCat.AutoTradeSettings.PercentLeverage = 66;
            SubscriptionModel.Instance.ModifySubscription(firstCat);
            Assert.AreEqual(SubscriptionModel.Instance.WasModified, true, "state changed to Modified");
            // сохранить изменения
            SubscriptionModel.Instance.SaveSubscribedCategories();

            // проверить, сохранились ли изменения
            var updatedSubs = SubscriptionModel.Instance.SubscribedCategories;
            Assert.AreEqual(2, updatedSubs.Count, "2 subscriptions after update");
            var updatedFirstCat = updatedSubs[0];
            var updatedSecondCat = updatedSubs[1];
            Assert.AreEqual(firstCat.AutoTradeSettings.PercentLeverage, updatedFirstCat.AutoTradeSettings.PercentLeverage, "modifications are saved");
            Assert.AreEqual(newCat.Title, updatedSecondCat.Title, "new sub is unchanged");
            Assert.AreEqual(SubscriptionModel.Instance.WasModified, false, "state changed to Unmodified");
        }




        private void MakeMoq()
        {
            serverCats = new List<Contract.Entity.Subscription>
                {
                    new Contract.Entity.Subscription
                        {
                            Service = 1,
                            PaidService = new PaidService
                                {
                                    Comment = "Test signals",
                                },
                            AutoTradeSettings = new AutoTradeSettings
                            {
                                PercentLeverage = 50,
                                MaxLeverage = 10,
                                MinVolume = 10000,
                                StepVolume = 5000,
                                TradeAuto = true,                            
                            }
                        }
                };

            fakeTradeServer = ProxyBuilder.Instance.GetImplementer<ITradeSharpServerTrade>();
            fakeTradeAccount = ProxyBuilder.Instance.GetImplementer<ITradeSharpAccount>();


            WalletError walletError;
            var bindToTradeSignal = ProxyBuilder.GetMethodName<ITradeSharpServerTrade>(a => a.SubscribeOnService(
                null, "", 0, false, false, null, out walletError));

            ((IMockableProxy)fakeTradeServer).MockMethods.Add(bindToTradeSignal,
                new GetAccountInfoDel((ProtectedOperationContext secCtx, string userLogin, int serviceId, bool renewAuto,
                    bool unsb, AutoTradeSettings sets, out WalletError error) =>
                {
                    error = WalletError.OK;
                    // модифицировать подписки
                    var indexExists = serverCats.FindIndex(c => c.Service == serviceId);
                    if (indexExists >= 0)
                    {
                        if (unsb)
                            serverCats.RemoveAt(indexExists);
                        else
                        {
                            var subs = new Contract.Entity.Subscription
                                {
                                    Service = serviceId,
                                    AutoTradeSettings = sets,
                                    RenewAuto = renewAuto,
                                    PaidService = new PaidService
                                        {
                                            Id = serviceId
                                        }
                                };
                            serverCats[indexExists] = subs;
                            serverCats.Add(subs);
                        }
                        return true;
                    }
                    
                    return true;
                }));


            var getTradeSignalsSubscribed = ProxyBuilder.GetMethodName<ITradeSharpAccount>(a => a.GetSubscriptions(""));
            ((IMockableProxy)fakeTradeAccount).MockMethods.Add(getTradeSignalsSubscribed, new Func<string, List<Contract.Entity.Subscription>>(userLogin =>
            {
                return userLogin == "Demo"
                   ? serverCats
                   : new List<Contract.Entity.Subscription>();
            }));
        }
    }
}
