using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.TradeSignalExecutor.BL;

namespace TradeSharp.Test.TradeSignalExecutor
{
    [TestFixture]
    public class NuPAMMFeeManager
    {
        private TradeSharpConnectionPersistent conn;
        private ACCOUNT testAccount;
        private PLATFORM_USER shareOwner;
        
        [TestFixtureSetUp]
        public void SetupTest()
        {
            // подготовить свежие котировки
            QuoteMaker.FillQuoteStorageWithDefaultValues();
            conn = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            ACCOUNT eurAccount;
            AccountShareTestDataMaker.MakePammData(conn, out testAccount, out shareOwner, out eurAccount);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            conn.Cleanup();
        }

        [Test]
        public void TestCalculateAccountShare()
        {
            var mgr = PAMMFeeManager.Instance;

            var investorsShare =
                conn.ACCOUNT_SHARE.First(s => s.Account == testAccount.ID && s.ShareOwner != shareOwner.ID);
            var ownersShare =
                conn.ACCOUNT_SHARE.First(s => s.Account == testAccount.ID && s.ShareOwner == shareOwner.ID);


            var testService = new PaidService
                {
                    Currency = testAccount.Currency,
                    serviceRates =
                        new List<PaidServiceRate>
                            {
                                new PaidServiceRate
                                    {
                                        Amount = 20,
                                        RateType = PaidServiceRate.ServiceRateType.Percent,
                                        UserBalance = 0
                                    }
                            }
                };
            decimal ownersMoney = 0;
            bool feeWasTaken;
            var record = mgr.CalculateAccountShare(conn, investorsShare, ownersShare, 
                ref ownersMoney, 10000, testService, out feeWasTaken);
            Assert.IsFalse(feeWasTaken, "CalculateAccountShare(==HWM) - комиссия не взымается");
            Assert.IsNotNull(record, "CalculateAccountShare(==HWM) - запись д.б. создана");
            Assert.IsNotNull(record.NewHWM, "CalculateAccountShare(==HWM) - запись д.б. создана c HWM != null");
            Assert.IsNotNull(investorsShare.HWM, "CalculateAccountShare(==HWM) - HWM д.б. модифицирована");

            // "добавить" денег - HWM должен обновиться
            const int aboveEquity = 500;
            record = mgr.CalculateAccountShare(conn, investorsShare, ownersShare,
                ref ownersMoney, 10000 + aboveEquity, testService, out feeWasTaken);
            Assert.IsTrue(feeWasTaken, "CalculateAccountShare(>HWM) - комиссия таки взымается");
            Assert.IsNotNull(record.OldHWM, "CalculateAccountShare(==HWM) - запись д.б. создана c OldHWM != null");
        }
    }
}
