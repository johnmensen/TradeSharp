using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeSharp.Client;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuUserServiceRegistrator
    {
        private ITradeSharpServerTrade fakeTradeServer;

        private IWalletManager fakeWalletManager;

        private delegate AuthenticationResponse GetUserOwnedAccountsWithActualBalanceDel(
            string login, ProtectedOperationContext secCtx, bool realOnly,
            out List<Account> accounts);

        private delegate bool RegisterOrUpdateServiceDel(
            ProtectedOperationContext ctx, PaidService service, out WalletError error);

        [TestFixtureSetUp]
        public void Setup()
        {
            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            // заглушки для диалогов - выбора пользователей
            UserServiceRegistrator.DialogBoxProvider = new TestDialogBoxProvider();

            // серверный (торговый) прокси
            fakeTradeServer = ProxyBuilder.Instance.GetImplementer<ITradeSharpServerTrade>();
            List<Account> accounts;
            var getUserOwnedAccountsWithActualBalanceMethodName = 
                ProxyBuilder.GetMethodName<ITradeSharpServerTrade>(a => a.GetUserOwnedAccountsWithActualBalance("",
                null, false, out accounts));

            ((IMockableProxy)fakeTradeServer).MockMethods.Add(getUserOwnedAccountsWithActualBalanceMethodName,
                new GetUserOwnedAccountsWithActualBalanceDel((string login, ProtectedOperationContext secCtx, bool realOnly,
                    out List<Account> acts) =>
                    {
                        acts = new List<Account>
                            {
                                new Account
                                    {
                                        Balance = 12000,
                                        Currency = "USD",
                                        ID = 51,
                                        Group = "Real"
                                    },
                                new Account
                                    {
                                        Balance = 2000,
                                        Currency = "USD",
                                        ID = 52,
                                        Group = "Real"
                                    }
                            };
                        return AuthenticationResponse.OK;
                    }));
            MainForm.serverProxyTrade = new TradeSharpServerTrade(fakeTradeServer);

            // кошельковый прокси
            fakeWalletManager = ProxyBuilder.Instance.GetImplementer<IWalletManager>();
            ((IMockableProxy) fakeWalletManager).MockMethods.Add(StronglyName.GetMethodName<IWalletManager>(
                w => w.GetUserWallet(null, null)),
                                                                 new Func<ProtectedOperationContext, string, Wallet>(
                                                                     (context, s) => new Wallet
                                                                         {
                                                                             Balance = 1005,
                                                                             Currency = "USD",
                                                                             User = 50
                                                                         }));
            WalletError walletError;
            var registerOrUpdateServiceMethodName =
                ProxyBuilder.GetMethodName<IWalletManager>(w => w.RegisterOrUpdateService(
                    null, null, out walletError));
            ((IMockableProxy) fakeWalletManager).MockMethods.Add(registerOrUpdateServiceMethodName,
                                                                 new RegisterOrUpdateServiceDel(
                                                                     (ProtectedOperationContext ctx, PaidService service,
                                                                      out WalletError error) =>
                                                                         {
                                                                             error = WalletError.OK;
                                                                             return true;
                                                                         }));
            TradeSharpWalletManager.Initialize(fakeWalletManager);
        }

        [Test]
        public void TestSelectTerminalUserAccount()
        {
            var account = UserServiceRegistrator.SelectTerminalUserAccount(true);
            Assert.IsNotNull(account, "UserServiceRegistrator - счет должен быть получен");
        }

        [Test]
        public void TestRegisterOrUpdateService()
        {
            var service = new PaidService
                {
                    AccountId = 51,
                    Comment = "TornSphincter",
                    ServiceType = PaidServiceType.PAMM,
                    serviceRates = new List<PaidServiceRate>
                        {
                            new PaidServiceRate
                                {
                                    Amount = 20,
                                    RateType = PaidServiceRate.ServiceRateType.Percent
                                }
                        }
                };

            var isRegistred = UserServiceRegistrator.RegisterOrUpdateService(service);
            Assert.IsTrue(isRegistred, "UserServiceRegistrator - сервис должен быть зарегистрирован");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            UserServiceRegistrator.DialogBoxProvider = new DialogBoxProvider();
            TradeSharpWalletManager.Initialize(null);
        }
    }
}
