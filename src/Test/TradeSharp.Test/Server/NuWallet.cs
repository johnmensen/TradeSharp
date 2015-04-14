using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Server
{
    /// <summary>
    /// Этот класс является "дополнением" к классу NuWalletManager
    /// </summary>
    [TestFixture]
    public class NuWallet
    {
        const decimal Deposit = 3m;
        private TradeSharpConnectionPersistent connectionPersistent;
        private WalletManager walletManager;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            connectionPersistent = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            walletManager = new WalletManager();

            DatabaseContext.InitializeFake(connectionPersistent);
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            connectionPersistent.Cleanup();
        }

        [SetUp]
        public void Setup()
        {
            QuoteMaker.FillQuoteStorageWithDefaultValues();
        }

        [TearDown]
        public void Teardown()
        {
        }

        #region WalletDb
        [Test]
        public void UpdateBalanceDeposit()
        {
            var ctx = connectionPersistent;
            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0);
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var balanceBefore = user.WALLET.Balance;
            var transfersBefore = user.TRANSFER.Count();
            WalletError error;
            var wallet = walletManager.UpdateBalance(user.ID, Deposit, true, out error);
            Assert.IsNotNull(wallet);
            Assert.AreEqual(WalletError.OK, error);
            Assert.AreEqual(transfersBefore + 1, user.TRANSFER.Count());
            Assert.AreEqual(transfersBefore + 1, user.TRANSFER.Count());
            Assert.IsTrue(user.WALLET.Balance.RoughCompares(balanceBefore + Deposit, 0.0001m),
                            string.Format("Должно быть: {0}. На самом деле: {1}", balanceBefore + 3,
                                        user.WALLET.Balance));


            var lastTransfer = user.TRANSFER.Last();
            Assert.AreEqual(3, lastTransfer.Amount);
            Assert.IsNull(lastTransfer.BalanceChange);
            Assert.IsNull(lastTransfer.RefWallet);
            Assert.AreEqual(user.ID, lastTransfer.User);

        }

        [Test]
        public void UpdateBalanceWithdraw()
        {
            var ctx = connectionPersistent;
            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0);
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var balanceBefore = user.WALLET.Balance;
            var transfersBefore = user.TRANSFER.Count();

            WalletError error;
            var wallet = walletManager.UpdateBalance(user.ID, Deposit, false, out error);
            Assert.IsNotNull(wallet);
            Assert.AreEqual(WalletError.OK, error);
            Assert.AreEqual(transfersBefore + 1, user.TRANSFER.Count());
            Assert.IsTrue(user.WALLET.Balance.RoughCompares(balanceBefore - 3, 0.0001m),
                            string.Format("Должно быть: {0}. На самом деле: {1}", balanceBefore - 3,
                                        user.WALLET.Balance));

            var lastTransfer = user.TRANSFER.Last();
            Assert.AreEqual(-3, lastTransfer.Amount);
            Assert.IsNull(lastTransfer.BalanceChange);
            Assert.IsNull(lastTransfer.RefWallet);
            Assert.AreEqual(user.ID, lastTransfer.User);

        }

        [Test]
        public void UpdateBalanceFail()
        {
            var ctx = connectionPersistent;
            WalletError error;
            var wallet = walletManager.UpdateBalance(-1, Deposit, false, out error);
            Assert.IsNull(wallet);
            Assert.AreEqual(WalletError.InvalidData, error);

            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0);
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var balanceBefore = user.WALLET.Balance;
            var transfersBefore = user.TRANSFER.Count();

            wallet = walletManager.UpdateBalance(user.ID, 0, false, out error);
            Assert.IsNotNull(wallet);
            Assert.AreEqual(WalletError.InvalidData, error);
            wallet = walletManager.UpdateBalance(user.ID, -1, false, out error);
            Assert.IsNotNull(wallet);
            Assert.AreEqual(WalletError.InvalidData, error);

            Assert.AreEqual(transfersBefore, user.TRANSFER.Count());
            Assert.IsTrue(user.WALLET.Balance.RoughCompares(balanceBefore, 0.0001m));
            Assert.IsTrue(wallet.Balance.RoughCompares(balanceBefore, 0.0001m));
        }

        [Test]
        public void TransferToTradingAccount()
        {
            var ctx = connectionPersistent;
            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0 && x.PLATFORM_USER_ACCOUNT.Count > 0 && x.WALLET.Currency == "USD");
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var accountFirst = user.PLATFORM_USER_ACCOUNT.First().ACCOUNT1;

            var walletBalanceBefore = user.WALLET.Balance;
            var walletTransfersBefore = user.TRANSFER.Count();
            var walletBalanceChengeBefore = ctx.BALANCE_CHANGE.Count();
            var accountBalanceBefore = accountFirst.Balance;

            WalletError error;
            walletManager.TransferToTradingAccount(
                ProtectedOperationContext.MakeServerSideContext(),
                    user.Login, accountFirst.ID, Deposit, out error);

            Assert.AreEqual(walletTransfersBefore + 1, user.TRANSFER.Count());
            Assert.AreEqual(walletBalanceChengeBefore + 1, ctx.BALANCE_CHANGE.Count());
            Assert.IsTrue(user.WALLET.Balance.RoughCompares(walletBalanceBefore - Deposit, 0.0001m));
            Assert.IsTrue(accountFirst.Balance.RoughCompares(accountBalanceBefore + Deposit, 0.0001m));
        }

        [Test]
        public void ChangeCurrency()
        {
            var ctx = connectionPersistent;
            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0 && x.PLATFORM_USER_ACCOUNT.Count > 0);
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var walletBalanceBefore = user.WALLET.Balance;
            var walletCurrencyBefore = user.WALLET.Currency;
            var walletCurrencyNew = ctx.SPOT.First(x => x.ComBase != walletCurrencyBefore).ComBase;

            WalletError error;
            var wallet = walletManager.ChangeCurrency(user.ID, walletCurrencyNew, false, out error);

            Assert.AreEqual(WalletError.OK, error);

            Assert.AreNotEqual(walletCurrencyBefore, wallet.Currency);
            Assert.AreEqual(walletCurrencyNew, user.WALLET.Currency);
            Assert.AreEqual(walletCurrencyNew, wallet.Currency);

            Assert.IsTrue(walletBalanceBefore.RoughCompares(user.WALLET.Balance, 0.0001m));

            wallet = walletManager.ChangeCurrency(user.ID, walletCurrencyBefore, false, out error);
        }

        [Test]
        public void ChangeCurrencyRecalculationBalance()
        {
            var ctx = connectionPersistent;
            WalletError error;
            var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.TRANSFER.Count > 0 && x.PLATFORM_USER_ACCOUNT.Count > 0);
            if (user == null) Assert.Fail("В БД нет необходимых для теста данных");

            var walletBalanceBefore = user.WALLET.Balance;
            var walletCurrencyBefore = user.WALLET.Currency;
            var walletCurrencyNew = ctx.SPOT.First(x => x.ComBase != walletCurrencyBefore).ComBase;

            var wallet = walletManager.ChangeCurrency(user.ID, walletCurrencyNew, true, out error);

            var quote = QuoteMaker.GetQuoteByKey(walletCurrencyNew + walletCurrencyBefore);
            var price = 1 / quote.ask;
            var calcNewBalance = (decimal)price * walletBalanceBefore;

            Assert.AreEqual(WalletError.OK, error);

            Assert.AreNotEqual(walletCurrencyBefore, wallet.Currency);
            Assert.AreEqual(walletCurrencyNew, user.WALLET.Currency);
            Assert.AreEqual(walletCurrencyNew, wallet.Currency);

            Assert.IsTrue(user.WALLET.Balance - calcNewBalance < 0.011m);
        }
        #endregion
    }
}
