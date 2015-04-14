using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Contract;
using TradeSharp.Test.Moq;
using TradeSharp.TradeLib;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuAccountCheckStream
    {
        private TradeSharpConnectionPersistent conn;
        private List<POSITION> allPositions;
        private readonly Dictionary<POSITION, decimal> swapByPos = new Dictionary<POSITION, decimal>();

        private ACCOUNT account;
        private TradeManager tradeManager;
        private const decimal SwapBuy = 0.4M, SwapSell = 0.35M;

        [TestFixtureSetUp]
        public void InitTest()
        {
            // забить котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();

            // словари
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            DalSpot.Instantiate(TradeSharpDictionary.Instance.proxy);

            tradeManager = new TradeManager(
                null,
                null,
                QuoteStorage.Instance, accountId =>
                    {
                        // ReSharper disable ConvertToLambdaExpression
                        return LinqToEntity.DecorateAccountGroup(conn.ACCOUNT.First(a => a.ID == accountId).ACCOUNT_GROUP);
                        // ReSharper restore ConvertToLambdaExpression
                    });
            ManagerTrade.MakeTestInstance();

            // временно удалить файл начального наполнения (открытые ордера)
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
            TradeSharpConnectionPersistent.RenameCsvFilesContainingAccountDataInMoqDbFolder();
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            MakeTestContents();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
        }

        [Test]
        public void TestDoMakeSwap()
        {
            var initialBalance = account.Balance;
            var stream = AccountCheckStream.Instance; // MakeTestInstance(tradeManager);
            stream.DoMakeSwap();
            // проверить - по каждой позе создан трансфер
            var resultedTransfers = conn.BALANCE_CHANGE.Where(bc => bc.AccountID == account.ID).ToList();
            Assert.AreEqual(allPositions.Count, resultedTransfers.Count, 
                "DoMakeSwap - по каждой сделке должно быть создано по трансферу");
            // проверить размер каждого трансфера
            foreach (var trans in resultedTransfers)
            {
                var posId = trans.Position.Value;
                var pos = allPositions.First(p => p.ID == posId);
                var swap = swapByPos[pos];
                var delta = Math.Abs(swap - trans.Amount);
                Assert.Less(delta, 0.01M, "DoMakeSwap - своп по ордеру " + pos.Symbol + " [" + 
                    pos.Side + "] посчитан неверно");
            }

            var shouldBeBalance = initialBalance + resultedTransfers.Sum(b => b.Amount);
            var resultedBalance = conn.ACCOUNT.First(a => a.ID == account.ID).Balance;
            var deltaBal = Math.Abs(resultedBalance - shouldBeBalance);
            Assert.Less(deltaBal, 0.01M, "DoMakeSwap - баланс счета должен измениться на сумму списаний");
        }

        private void MakeTestContents()
        {
            // добавить тестовый счет
            var group = conn.ACCOUNT_GROUP.First(g => !g.IsReal);
            group.SwapFree = false;
            account = new ACCOUNT
                {
                    AccountGroup = group.Code,
                    Currency = "USD",
                    Balance = 10000,
                    Description = "test",
                    Status = (int) Account.AccountStatus.Created
                };
            conn.ACCOUNT.Add(account);
            conn.SaveChanges();
            
            // задать всем дефолтовый своп
            foreach (var spot in conn.SPOT)
            {
                spot.SwapBuy = SwapBuy;
                spot.SwapSell = SwapSell;
            }

            // создать сделки по счету
            allPositions = PositionMaker.MakePositions(conn, account.ID);
            conn.SaveChanges();

            foreach (var pos in allPositions)
            {
                // расчетное значение своп-а
                var swap = DalSpot.Instance.GetAbsValue(pos.Symbol, pos.Side > 0 ? SwapBuy : SwapSell);
                swap = swap * pos.Volume;
                // перевести своп в валюту счета
                string errorStr;
                swap = DalSpot.Instance.ConvertToTargetCurrency(pos.Symbol, false, account.Currency,
                                                         (double)swap, QuoteStorage.Instance.ReceiveAllData(),
                                                         out errorStr) ?? 0;
                swapByPos.Add(pos, swap);
            }
        }
    }
}
