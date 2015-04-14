using System;
using TradeSharp.Server.Repository;
using TradeSharp.Util;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuWalletManager
    {
        private TradeSharpConnectionPersistent conn;
        private PLATFORM_USER srvOwner, srvSubscriber;
        private SERVICE paidService;
        private ACCOUNT accountShared;
        private PLATFORM_USER accountSharedOwner;
        private WalletManager walletManager = new WalletManager();

        [TestFixtureSetUp]
        public void InitTest()
        {
            // забить котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            // временно удалить файл начального наполнения (открытые ордера)
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
            //TradeSharpConnectionPersistent.RenameCsvFilesContainingAccountDataInMoqDbFolder();
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            MakeTestData();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            //TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
        }

        [Test]
        public void TestDoubleFeeWasNotCharged()
        {
            var repo = new WalletRepository();
            var hasPaid = repo.HasUserPaidTheService(conn, paidService, srvSubscriber.ID);
            Assert.IsFalse(hasPaid, "HasUserPaidTheService - пользователь еще не оплатил услугу");
            // сбацать платеж
            var error = repo.ChargeFeeOnSubscription(conn, paidService, srvSubscriber.ID, false);
            Assert.AreEqual(WalletError.OK, error, "Платеж сформирован");
            hasPaid = repo.HasUserPaidTheService(conn, paidService, srvSubscriber.ID);
            Assert.IsTrue(hasPaid, "HasUserPaidTheService - пользователь таки уже оплатил услугу");

            var userMoneyBefore = conn.WALLET.First(w => w.User == srvSubscriber.ID);
            error = repo.ChargeFeeOnSubscription(conn, paidService, srvSubscriber.ID, false);
            Assert.AreEqual(WalletError.OK, error, "Повторный платеж сформирован");
            var userMoneyAfter = conn.WALLET.First(w => w.User == srvSubscriber.ID);
            Assert.AreEqual(userMoneyBefore, userMoneyAfter, "Лишних денег не списано за повторный платеж");
        }

        [Test]
        public void TestCalculateAccountEquity()
        {
            List<AccountShare> shares;
            bool noQuoteError;
            var equity = WalletManager.CalculateAccountEquityWithShares(conn, accountShared, accountSharedOwner.ID,
                                                 Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(),
                                                 out shares, out noQuoteError);
            Assert.Greater(equity, 0, "CalculateAccountEquityWithShares() - equity должна быть больше 0");
            Assert.AreEqual(2, shares.Count, "CalculateAccountEquityWithShares() - должно быть два владельца");
            var sumShare = shares.Sum(s => s.ShareMoney);
            Assert.IsTrue(sumShare.RoughCompares(equity, 0.01M), "CalculateAccountEquityWithShares() - сумма средств {0} должна равняться балансу {1}",
                sumShare, equity);
        }

        [Test]
        public void TestInvestInPAMM()
        {
            // PAMM-инвестор
            var pammInvestor = new PLATFORM_USER
                {
                    Email = "pamminvestor0001@pamm.com",
                    Login = "PammInvestore",
                    Password = "PammInvestore",
                    RegistrationDate = DateTime.Now,
                    RoleMask = (int) UserRole.Trader,
                    Title = "trader"
                };
            conn.PLATFORM_USER.Add(pammInvestor);
            conn.SaveChanges();
            var wallet = new WALLET
                {
                    User = pammInvestor.ID,
                    Balance = 160000,
                    Currency = "USD",
                    Password = "WalletPwrd"
                };
            conn.WALLET.Add(wallet);
            conn.SaveChanges();

            // посчитать, что было ДО инвестирования в ПАММ
            List<AccountShare> shares;
            bool noQuoteError;
            WalletManager.CalculateAccountEquityWithShares(conn, accountShared, accountSharedOwner.ID,
                                                 Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(),
                                                 out shares, out noQuoteError);

            // инвестировать в ПАММ
            const decimal amountToInvest = 10000;
            var oldBalance = accountShared.Balance;
            var status = walletManager.InvestInPAMM(ProtectedOperationContext.MakeServerSideContext(),
                                                pammInvestor.Login, accountShared.ID, amountToInvest);
            Assert.AreEqual(RequestStatus.OK, status, "InvestInPAMM - должно быть ОК");
            accountShared = conn.ACCOUNT.First(a => a.ID == accountShared.ID);
            
            var shouldBeBalance = oldBalance + amountToInvest;
            Assert.IsTrue(shouldBeBalance.RoughCompares(accountShared.Balance, 0.1M), 
                "InvestInPAMM - баланс счета должен увеличиться на сумму вложения");

            var newShares = conn.ACCOUNT_SHARE.Where(s => s.Account == accountShared.ID).ToList();
            Assert.AreEqual(shares.Count + 1, newShares.Count, "InvestInPAMM - добавился один пай");
            var sumSharesPercent = newShares.Sum(s => s.Share);
            Assert.IsTrue(100M.RoughCompares(sumSharesPercent, 0.000001M),
                "InvestInPAMM - сумма паев осталась 100%");
            Assert.IsTrue(newShares.Any(s => s.ShareOwner == pammInvestor.ID), 
                "InvestInPAMM - новый владелец должен быть в списке пайщиков");
            Assert.IsTrue(conn.SUBSCRIPTION.Any(s => s.User == pammInvestor.ID),
                "InvestInPAMM - должна появиться подписка");

            // инвестировать еще денег
            const decimal moreMoney = 145201.55M;
            status = walletManager.InvestInPAMM(ProtectedOperationContext.MakeServerSideContext(),
                                                pammInvestor.Login, accountShared.ID, moreMoney);
            Assert.AreEqual(RequestStatus.OK, status, "InvestInPAMM (повторно) - должно быть ОК");
            var updatedNewShares = conn.ACCOUNT_SHARE.Where(s => s.Account == accountShared.ID).ToList();
            Assert.AreEqual(newShares.Count, updatedNewShares.Count, "InvestInPAMM (повторно) - количество паев не должно меняться");
            sumSharesPercent = updatedNewShares.Sum(s => s.Share);
            Assert.IsTrue(100M.RoughCompares(sumSharesPercent, 0.000001M),
                "InvestInPAMM (повторно) - сумма паев осталась 100%");

            status = walletManager.InvestInPAMM(ProtectedOperationContext.MakeServerSideContext(),
                pammInvestor.Login, accountShared.ID, moreMoney);
            Assert.AreEqual(RequestStatus.MarginOrLeverageExceeded, status, 
                "InvestInPAMM (превышение баланса) - должно быть MarginOrLeverageExceeded");

            // вывести из ПАММа кусочек
            const decimal withdrawnMoney = 50.25M;
            var balanceBeforeWth = conn.WALLET.First(w => w.User == pammInvestor.ID).Balance;
            status = walletManager.WithdrawFromPAMM(ProtectedOperationContext.MakeServerSideContext(),
                                                pammInvestor.Login, accountShared.ID, withdrawnMoney, false);
            Assert.AreEqual(RequestStatus.OK, status, "InvestInPAMM (вывод средств) - должно быть OK");
            var sharesAfterWithdraw = conn.ACCOUNT_SHARE.Where(s => s.Account == accountShared.ID).ToList();
            var balanceAfterWth = conn.WALLET.First(w => w.User == pammInvestor.ID).Balance;
            Assert.IsTrue(balanceBeforeWth.RoughCompares(balanceAfterWth - withdrawnMoney, 0.005M), 
                "баланс должен увеличиться на сумму вывода");
            Assert.AreEqual(updatedNewShares.Count, sharesAfterWithdraw.Count, 
                "InvestInPAMM (вывод средств) - количество паёв не должно измениться");
            Assert.IsTrue(conn.SUBSCRIPTION.Any(s => s.User == pammInvestor.ID),
                "InvestInPAMM (вывод средств) - должна остаться подписка");

            // попробовать вывести слишком много
            status = walletManager.WithdrawFromPAMM(ProtectedOperationContext.MakeServerSideContext(),
                                                pammInvestor.Login, accountShared.ID, 1000000, false);
            Assert.AreEqual(RequestStatus.BadRequest, status,
                "InvestInPAMM (вывод средств) - должно быть BadRequest");

            // вывести остаток
            status = walletManager.WithdrawFromPAMM(ProtectedOperationContext.MakeServerSideContext(),
                                                pammInvestor.Login, accountShared.ID, 0, true);
            Assert.AreEqual(RequestStatus.OK, status, "InvestInPAMM (вывод всех средств) - должно быть OK");
            Assert.IsFalse(conn.SUBSCRIPTION.Any(s => s.User == pammInvestor.ID),
                "InvestInPAMM (вывод всех средств) - должна удалиться подписка");
            Assert.IsFalse(conn.ACCOUNT_SHARE.Any(s => s.ShareOwner == pammInvestor.ID),
                "InvestInPAMM (вывод всех средств) - должна удалиться долька нашего пайщика");
        }

        public void TestTransferToTradingAccountWhichHappensToBePAMM()
        {
            var someInvestor = 
                (from us in conn.PLATFORM_USER
                 join pa in conn.PLATFORM_USER_ACCOUNT on us.ID equals pa.PlatformUser
                 join ac in conn.ACCOUNT on pa.Account equals ac.ID
                 join gr in conn.ACCOUNT_GROUP on ac.AccountGroup equals gr.Code
                 where us.ID != srvOwner.ID
                 select us).First();
                                
            const decimal amountToInvest = 10000;
            var oldBalance = accountShared.Balance;

            // завести немного денег на счет - на деле - инвестировать в ПАММ
            WalletError error;
            var wallet = walletManager.TransferToTradingAccount(ProtectedOperationContext.MakeServerSideContext(),
                someInvestor.Login, accountShared.ID, amountToInvest, out error);

            Assert.AreEqual(WalletError.OK, error, "TransferToTradingAccount (shared) - должно быть ОК");
            Assert.IsNotNull(wallet, "TransferToTradingAccount (shared) - должны быть данные кошелька");
            var newBalance = conn.ACCOUNT.First(a => a.ID == accountShared.ID).Balance;
            Assert.IsTrue(newBalance.RoughCompares(oldBalance + amountToInvest, 0.005M), 
                "TransferToTradingAccount (shared) - баланс счета должен вырасти");

            var shareNew =
                conn.ACCOUNT_SHARE.FirstOrDefault(s => s.ShareOwner == someInvestor.ID && s.Account == accountShared.ID);
            Assert.IsNotNull(shareNew, "TransferToTradingAccount (shared) - пай должен быть создан");

            const decimal amountToWithdraw = 2000;
            // немножко вывести
            walletManager.TransferToWallet(ProtectedOperationContext.MakeServerSideContext(),
                someInvestor.Login, accountShared.ID, amountToWithdraw, out error);

            Assert.AreEqual(WalletError.OK, error, "TransferToWallet (shared) - должно быть ОК");
            oldBalance = newBalance;            
            newBalance = conn.ACCOUNT.First(a => a.ID == accountShared.ID).Balance;
            Assert.IsTrue(newBalance.RoughCompares(oldBalance - amountToWithdraw, 0.005M),
                "TransferToWallet (shared) - баланс счета должен уменьшиться");
        }

        [Test]
        public void TestGetAccountSharePercent()
        {
            var account = conn.ACCOUNT.First(a => a.ACCOUNT_SHARE.Any());
            var ownerUserId = account.PLATFORM_USER_ACCOUNT.First(pa => pa.RightsMask == (int) AccountRights.Управление).PlatformUser;
            var share = conn.ACCOUNT_SHARE.First(s => s.Account == account.ID && s.ShareOwner == ownerUserId);
            var percent = walletManager.GetAccountSharePercent(conn, account.ID, ownerUserId);
            Assert.AreEqual(share.Share, percent, "GetAccountSharePercent - должен вернуть верную цифру");

            account = conn.ACCOUNT.First(a => !a.ACCOUNT_SHARE.Any());
            ownerUserId = account.PLATFORM_USER_ACCOUNT.First(pa => pa.RightsMask == (int)AccountRights.Управление).PlatformUser;
            percent = walletManager.GetAccountSharePercent(conn, account.ID, ownerUserId);
            Assert.AreEqual(100M, percent, "GetAccountSharePercent - должен вернуть 100%");
        }

        [Test]
        public void TestGetUserWalletSubscriptionAndLastPaymentsInner()
        {
            var user = conn.PLATFORM_USER.First(u => u.SUBSCRIPTION.Count > 2 && u.TRANSFER.Count > 5);
            var subsCount = user.SUBSCRIPTION.Count;

            int paymentsTotal;
            List<Contract.Entity.Subscription> subscriptions;
            List<Transfer> transfers;
            WalletError error;

            var repo = new WalletRepository();
            var wallet = repo.GetUserWalletSubscriptionAndLastPaymentsInner(user.Login,
                100, out paymentsTotal, out subscriptions, out transfers, out error);

            Assert.AreEqual(WalletError.OK, error, "GetUserWalletSubscriptionAndLastPaymentsInner - должно быть ОК");
            Assert.IsNotNull(wallet, "GetUserWalletSubscriptionAndLastPaymentsInner - кошелек должен быть получен");
            Assert.Greater(paymentsTotal, 5, "GetUserWalletSubscriptionAndLastPaymentsInner - платежей должно быть больше N");
            Assert.Greater(transfers.Count, 0, "GetUserWalletSubscriptionAndLastPaymentsInner - платежи должны быть получены");
            Assert.AreEqual(subscriptions.Count, subsCount, "GetUserWalletSubscriptionAndLastPaymentsInner - подписки должны быть получены");
            var distinctSubscriptionServiceAccounts =
                subscriptions.Select(s => s.PaidService.AccountId).Distinct().Count();
            Assert.AreEqual(distinctSubscriptionServiceAccounts, subscriptions.Count,
                "GetUserWalletSubscriptionAndLastPaymentsInner - инф. о подписке должна быть заполнена");
        }

        private void MakeTestData()
        {
            paidService = conn.SERVICE.First(s => s.Currency == "USD");
            srvOwner = paidService.PLATFORM_USER;
            srvSubscriber = conn.PLATFORM_USER.First(u => u.WALLET.Currency == "USD" && u.ID != srvOwner.ID);
            // аккаунт с дольками и открытыми позами
            accountShared = conn.ACCOUNT.First(a => a.POSITION.Count > 1 && a.POSITION.Count < 200);
            accountSharedOwner = (from pa in conn.PLATFORM_USER_ACCOUNT
                                  join u in conn.PLATFORM_USER on pa.PlatformUser equals u.ID
                                  where pa.Account == accountShared.ID
                                  select u).First();
            // добавить ПАММ-сервис
            conn.SERVICE.Add(new SERVICE
                {
                    Currency = "USD",
                    ServiceType = (int) PaidServiceType.PAMM,
                    AccountId = accountShared.ID,
                    User = accountSharedOwner.ID,
                    Comment = "PAMMMmm"
                });
            
            // добавить дольки
            var oneMoreUser = conn.PLATFORM_USER.First(u => u.ID != accountSharedOwner.ID);
            conn.ACCOUNT_SHARE.Add(new ACCOUNT_SHARE
                {
                    Account = accountShared.ID,
                    Share = 55,
                    ShareOwner = accountSharedOwner.ID
                });
            conn.ACCOUNT_SHARE.Add(new ACCOUNT_SHARE
            {
                Account = accountShared.ID,
                Share = 45,
                ShareOwner = oneMoreUser.ID
            });
            conn.SaveChanges();
        }
    }
}
