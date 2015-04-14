using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.Test.Moq;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuAccountManager
    {
        private TradeSharpConnectionPersistent conn;
        private ManagerAccount testManager;
        private ACCOUNT testAccount;
        private ACCOUNT eurAccount;
        private PLATFORM_USER shareOwner;

        [TestFixtureSetUp]
        public void Setup()
        {
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            // подготовить свежие котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            ManagerTrade.MakeTestInstance();
            testManager = ManagerAccount.Instance;
            
            // тестовые данные
            MakeTestData();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
        }

        [Test]
        public void TestGetAccountTransfersPartByPart()
        {
            var user = conn.PLATFORM_USER.First(u => u.TRANSFER.Count > 30);
            var userTransfersCount = user.TRANSFER.Count;

            var startId = 0;
            var totalCount = 0;
            while (true)
            {
                var transfers = testManager.GetAccountTransfersPartByPart(
                    ProtectedOperationContext.MakeServerSideContext(), user.Login, startId, 20);
                if (transfers.Count == 0) break;

                totalCount += transfers.Count;
                startId = transfers.Max(t => t.Id);
                Assert.IsTrue(transfers.All(t => t != null),
                              "GetAccountTransfersPartByPart - не жолжно быть пустых (null) трансферов");
            }
            Assert.AreEqual(userTransfersCount, totalCount,
                            "GetAccountTransfersPartByPart - неверное количество трансферов");
        }

        [Test]
        public void TestGetAccountShares()
        {
            var shares = testManager.GetAccountShares(
                ProtectedOperationContext.MakeServerSideContext(), testAccount.ID, false);
            Assert.AreEqual(3, shares.Count, "GetAccountShares - должно вернуться 3 записи");
            Assert.AreEqual(100, shares.Sum(s => s.SharePercent), "GetAccountShares - сумма частей должна дать 100%");
            Assert.AreEqual(1, shares.Count(s => s.UserLogin == shareOwner.Login),
                            "GetAccountShares - (не) содержит логин владельца");

            // теперь - то же самое, но еще и посчитать в деньгах
            var quotes = Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData();
            var allOpenedOrders = conn.POSITION.Where(p => p.AccountID == testAccount.ID &&
                                                           p.State == (int) PositionState.Opened)
                                      .ToList()
                                      .Select(LinqToEntity.DecorateOrder)
                                      .ToList();
            var currentProfit =
                allOpenedOrders.Sum(o => DalSpot.Instance.CalculateProfitInDepoCurrency(o, quotes, testAccount.Currency));
            var equity = testAccount.Balance + (decimal) currentProfit.Value;

            shares = testManager.GetAccountShares(
                ProtectedOperationContext.MakeServerSideContext(), testAccount.ID, true);
            Assert.AreEqual(3, shares.Count, "GetAccountShares (equity) - должно вернуться 3 записи");
            var equitySum = shares.Sum(s => s.ShareMoney);
            var delta = Math.Abs(equity - equitySum);
            Assert.Less(delta, 1, "GetAccountShares - сумма в деньгах должна совпадать, около "
                                  + equity.ToStringUniformMoneyFormat());
        }

        [Test]
        public void TestGetAccountShareHistory()
        {
            conn.ACCOUNT_SHARE_HISTORY.Add(new ACCOUNT_SHARE_HISTORY
                {
                    Account = testAccount.ID,
                    ShareOwner = shareOwner.ID,
                    NewShare = 50,
                    NewHWM = 25400,
                    ShareAmount = 25300,
                    Date = DateTime.Now.Date.AddDays(-5)
                });
            conn.ACCOUNT_SHARE_HISTORY.Add(new ACCOUNT_SHARE_HISTORY
            {
                Account = testAccount.ID,
                ShareOwner = shareOwner.ID,
                NewShare = 49.95M,
                NewHWM = 25800,
                ShareAmount = 25800,
                Date = DateTime.Now.Date.AddDays(-4)
            });
            conn.ACCOUNT_SHARE_HISTORY.Add(new ACCOUNT_SHARE_HISTORY
            {
                Account = testAccount.ID,
                ShareOwner = conn.PLATFORM_USER.First(u => u.ID != shareOwner.ID).ID,
                NewShare = 50.05M,
                NewHWM = 25810,
                ShareAmount = 25810,
                Date = DateTime.Now.Date.AddDays(-4)
            });
            conn.SaveChanges();

            var records = testManager.GetAccountShareHistory(testAccount.ID, shareOwner.Login);
            Assert.AreEqual(2, records.Count, "GetAccountShareHistory() - должен вернуть 2 записи");
        }

        [Test]
        public void TestGetUserOwnAndSharedAccounts()
        {
            List<AccountShared> accounts;
            var status = testManager.GetUserOwnAndSharedAccounts(shareOwner.Login,
                                                                 ProtectedOperationContext.MakeServerSideContext(),
                                                                 out accounts);
            Assert.AreEqual(RequestStatus.OK, status, "GetUserOwnAndSharedAccounts() - должна завершиться успешно");
            Assert.IsNotNull(accounts, "GetUserOwnAndSharedAccounts() - список счетов не должен быть NULL");
            Assert.AreEqual(1, accounts.Count,
                            "GetUserOwnAndSharedAccounts() - должен вернуться ровно один счет (владельца)");
            Assert.IsTrue(accounts[0].IsOwnAccount, "GetUserOwnAndSharedAccounts() - счетом владеет управляющий");
            Assert.AreEqual(50, accounts[0].SharePercent,
                            "GetUserOwnAndSharedAccounts() - доля управляющего должна быть 50%");
            Assert.Greater(accounts[0].ShareMoneyWallet, 0,
                           "GetUserOwnAndSharedAccounts() - доля управляющего в валюте кошелька должна быть расчитана");

            var ownerNewShare = new ACCOUNT_SHARE
                {
                    Account = eurAccount.ID,
                    Share = 11,
                    ShareOwner = shareOwner.ID
                };
            conn.ACCOUNT_SHARE.Add(ownerNewShare);
            conn.SaveChanges();

            status = testManager.GetUserOwnAndSharedAccounts(shareOwner.Login,
                                                             ProtectedOperationContext.MakeServerSideContext(),
                                                             out accounts);
            Assert.AreEqual(RequestStatus.OK, status, "GetUserOwnAndSharedAccounts(2) - должна завершиться успешно");
            Assert.AreEqual(2, accounts.Count,
                            "GetUserOwnAndSharedAccounts() - должно вернуться два счета - владельца и вкладчика");
            Assert.AreEqual(ownerNewShare.Share, accounts[1].SharePercent,
                            "GetUserOwnAndSharedAccounts() - вкладчику одлжно принадлежать N% счета");
            Assert.Greater(accounts[1].ShareMoneyWallet, 0,
                           "GetUserOwnAndSharedAccounts() - доля вкладчика в валюте кошелька должна быть расчитана");
        }

        [Test]
        public void TestGetTransfersSummary()
        {
            const string login = "Andrey123";
            var summPos = testManager.GetTransfersSummary(ProtectedOperationContext.MakeServerSideContext(), login);
            Assert.IsNotNull(summPos, "GetTransfersSummary - должен вернуться не null");
            Assert.Greater(summPos.TransfersByType.Count, 1, "GetTransfersSummary - вернулись все значения");
            Assert.AreEqual(login, summPos.Login, "GetTransfersSummary - вернулся верный акаунт");
        }

        [Test]
        public void TestGetHistoryOrdersCompressed()
        {
            var account = conn.ACCOUNT.First(a => a.POSITION_CLOSED.Count > 100 && a.POSITION_CLOSED.Count < 1000);
            byte[] buffer;
            var status = testManager.GetHistoryOrdersCompressed(account.ID, null, out buffer);
            Assert.AreEqual(RequestStatus.OK, status, "GetHistoryOrdersCompressed - должно быть ОК");

            object[] array;
            using (var reader = new SerializationReader(buffer))
            {
                array = reader.ReadObjectArray(typeof (MarketOrder));
            }
            Assert.Greater(array.Length, 0, "GetHistoryOrdersCompressed - ордера должны быть распакованы");
            var orders = array.Cast<MarketOrder>().ToArray();
            var realCount = account.POSITION_CLOSED.Count;
            Assert.AreEqual(realCount, orders.Length, "GetHistoryOrdersCompressed - все ордера должны быть прочитаны");
        }

        [Test]
        public void TestGetSignallersOpenTrades()
        {
            var account = (from ac in conn.ACCOUNT
                           join pa in conn.PLATFORM_USER_ACCOUNT on ac.ID equals pa.Account
                           join sub in conn.SUBSCRIPTION on pa.PlatformUser equals sub.User
                           where ac.POSITION.Count(p => p.MasterOrder.HasValue) > 1
                           select ac).First();

            var trades = testManager.GetSignallersOpenTrades(account.ID);
            Assert.Greater(trades.Count, 1, "GetSignallersOpenTrades - трейды должны быть получены");
            var totalPositions = trades.Values.Sum(v => v.Count);

            var signallers = (from ac in conn.ACCOUNT
                              join pa in conn.PLATFORM_USER_ACCOUNT on ac.ID equals pa.Account
                              join sub in conn.SUBSCRIPTION on pa.PlatformUser equals sub.User
                              join srv in conn.SERVICE on sub.Service equals srv.ID
                              where ac.ID == account.ID && srv.AccountId.HasValue
                              select srv.AccountId).ToList();
            var totalTrades = conn.POSITION.Count(p => signallers.Contains(p.AccountID));

            Assert.AreEqual(totalTrades, totalPositions, "GetSignallersOpenTrades - все трейды должны быть получены");
        }

        [Test]
        public void TestChangeBalance()
        {
            var accountRepo = AccountRepository.Instance;

            var account = conn.ACCOUNT.First(a => a.Currency == "USD");
            var oldBalance = account.Balance;
            const decimal amount = 100.51M;
            var status = accountRepo.ChangeBalance(account.ID, amount, "test transfer", DateTime.Now,
                                                   BalanceChangeType.Deposit);
            Assert.AreEqual(RequestStatus.OK, status, "ChangeBalance - должен отработать на пополнение");
            Assert.AreEqual(oldBalance + amount, conn.ACCOUNT.First(a => a.ID == account.ID).Balance, 
                "ChangeBalance - баланс должен увеличиться");

            status = accountRepo.ChangeBalance(account.ID, amount, "test transfer", DateTime.Now,
                                                   BalanceChangeType.Withdrawal);
            Assert.AreEqual(RequestStatus.OK, status, "ChangeBalance - должен отработать на списание");
            Assert.AreEqual(oldBalance, conn.ACCOUNT.First(a => a.ID == account.ID).Balance,
                "ChangeBalance - баланс должен снизиться до исходного значения");
        }

        [Test]
        public void TestGetFreeMagicsPool()
        {
            // сбацать пару позиций для счета
            var account = conn.ACCOUNT.First(a => a.POSITION.Count == 0 && a.POSITION_CLOSED.Count == 0);
            var startId = conn.POSITION_CLOSED.Max(p => p.ID);

            var magics = new int?[] {1, 2, 3, 4, null, null, 7, 8, 100};
            foreach (var magic in magics)
            {
                if (magic % 2 == 0)
                    conn.POSITION.Add(new POSITION
                        {
                            AccountID = account.ID,
                            Magic = magic,
                            PriceEnter = 1.2150M,
                            TimeEnter = DateTime.Now,
                            Volume = 100000,
                            Symbol = "EURUSD",
                            Side = 1
                        });
                else
                    conn.POSITION_CLOSED.Add(new POSITION_CLOSED
                        {
                            AccountID = account.ID,
                            Magic = magic,
                            PriceEnter = 1.2150M,
                            TimeEnter = DateTime.Now.AddMinutes(-1),
                            Volume = 100000,
                            Symbol = "EURUSD",
                            Side = 1,
                            PriceExit = 1.2140M,
                            TimeExit = DateTime.Now,
                            ResultBase = -10,
                            ResultDepo = -10,
                            ResultPoints = -1,
                            ID = ++startId
                        });
            }
            conn.SaveChanges();

            var pool = testManager.GetFreeMagicsPool(account.ID, 15);
            Assert.AreEqual(15, pool.Count, "GetFreeMagicsPool - должен вернуть строго 15 записей");
            Assert.AreEqual(5, pool[0], "GetFreeMagicsPool - промахнулся мимо первого свободного номера");
            Assert.AreEqual(6, pool[1], "GetFreeMagicsPool - промахнулся мимо второго свободного номера");
            Assert.AreEqual(9, pool[2], "GetFreeMagicsPool - промахнулся мимо третьего свободного номера");
            Assert.AreEqual(10, pool[3], "GetFreeMagicsPool - промахнулся мимо четвертого свободного номера");
        }

        private void MakeTestData()
        {
            AccountShareTestDataMaker.MakePammData(conn, out testAccount, out shareOwner, out eurAccount);
            // сделать все счета реальными - ПАММы работают с реальными счетами
            foreach (var gr in conn.ACCOUNT_GROUP)
            {
                gr.IsReal = true;
            }
            conn.SaveChanges();
        }
    }
}
