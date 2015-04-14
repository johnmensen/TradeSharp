using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.TradeSignalExecutor.BL;

namespace TradeSharp.Test.TradeSignalExecutor
{
    [TestFixture]
    public class StaleOrdersChecker
    {
        private TradeSharpConnectionPersistent conn;
        private POSITION[] allPositions;

        [TestFixtureSetUp]
        public void InitTest()
        {
            // временно удалить файл начального наполнения (открытые ордера)
            TradeSharpConnectionPersistent.RenameCsvFilesInMoqDbFolder(typeof(POSITION));
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            MakeTestOrders();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
            TradeSharpConnectionPersistent.RestoreCsvFilesInMoqDbFolder();
        }

        [Test]
        public void TestCleanupPositionQuery()
        {
            var stalePositions = SignalExecutor.GetStaleOrders();
            Assert.AreEqual(0, stalePositions.Count, "Устаревших сигнальных сделок быть не должно");

            allPositions[1].MasterOrder = allPositions[0].ID + 50;
            conn.SaveChanges();

            stalePositions = SignalExecutor.GetStaleOrders();
            Assert.AreEqual(1, stalePositions.Count, "Один сигнал должен числиться устаревшим");
        }

        private void MakeTestOrders()
        {
            // очистить сделки
            var allpos = conn.POSITION.ToList();
            foreach (var pos in allpos)
                conn.POSITION.Remove(pos);
            conn.SaveChanges();

            // добавить тестовые сделки
            var accountA = conn.ACCOUNT.First().ID;
            var accountB = conn.ACCOUNT.First(a => a.ID != accountA).ID;

            allPositions = new[]
                {
                    new POSITION
                        {
                            Symbol = "EURUSD",
                            Volume = 200000,
                            Side = -1,
                            TimeEnter = DateTime.Now.AddMinutes(-1012),
                            PriceEnter = 1.3224M,
                            AccountID = accountA,
                        },
                    new POSITION
                        {
                            Symbol = "EURUSD",
                            Volume = 20000,
                            Side = -1,
                            TimeEnter = DateTime.Now.AddMinutes(-1012),
                            PriceEnter = 1.3224M,
                            AccountID = accountB
                        }
                };
            foreach (var pos in allPositions)
            {
                if (pos.AccountID == 0)
                    pos.AccountID = accountA;
                pos.State = (int) PositionState.Opened;
                conn.POSITION.Add(pos);
            }
            conn.SaveChanges();

            // связать сделки
            allPositions[1].MasterOrder = allPositions[0].ID;
            conn.SaveChanges();
        }
    }
}
