using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.AdminSite.Test.DifficultTests
{
    /// <summary>
    /// тесты для DealerDb
    /// Логика методов проста - тесты, в основном, регрессионные
    /// </summary>
    partial class NuDbAccess
    {
        [Test]
        public void GetAllDealerDescription()
        {
            var dealersDescriprion = dealerRepository.GetAllDealerDescription().ToArray();
            Assert.AreEqual(3, dealersDescriprion.Length);           

            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            Assert.AreEqual(new List<DealerDescription>(), dealerRepository.GetAllDealerDescription());
        }

        [Test]
        public void SaveChangesDealerFealdInAccountGroup()
        {
            var accGroup = new AccountGroup
            {
                SessionName = "TestSessionName",
                SenderCompId = "TestSenderCompId",
                HedgingAccount = "TestHedgingAccount",
                MessageQueue = "TestMessageQueue"
            };

            using (var ctx = DatabaseContext.Instance.Make())
            {accGroup.Code = ctx.DEALER_GROUP.First().ACCOUNT_GROUP.Code;}

            var resultUpdate = dealerRepository.SaveChangesDealerFealdInAccountGroup(accGroup);
            Assert.IsTrue(resultUpdate);
            
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var actualDealer = ctx.DEALER_GROUP.First();
                Assert.AreEqual("TestSessionName", actualDealer.SessionName);
                Assert.AreEqual("TestSenderCompId", actualDealer.SenderCompId);
                Assert.AreEqual("TestHedgingAccount", actualDealer.HedgingAccount);
                Assert.AreEqual("TestMessageQueue", actualDealer.MessageQueue); 
            }

            accGroup.SessionName = "FaileSessionName";
            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            Assert.IsFalse(dealerRepository.SaveChangesDealerFealdInAccountGroup(accGroup));
        }

        [Test]
        public void SaveDealerChanges()
        {
            DealerDescription dealerDescription;  

            using (var ctx = DatabaseContext.Instance.Make())
            { dealerDescription = LinqToEntity.DecorateDealerDescription(ctx.DEALER.First()); }

            dealerDescription.DealerEnabled = !dealerDescription.DealerEnabled;
            dealerDescription.FileName = "TestFileName";

            var resultUpdate = dealerRepository.SaveDealerChanges(dealerDescription);
            Assert.IsTrue(resultUpdate);

            using (var ctx = DatabaseContext.Instance.Make())
            {
                var actualDealerDescription = LinqToEntity.DecorateDealerDescription(ctx.DEALER.Single(x => x.Code == dealerDescription.Code));
                Assert.AreEqual(actualDealerDescription.FileName, "TestFileName");
            }

            dealerDescription.FileName = "FaileFileName";
            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            Assert.IsFalse(dealerRepository.SaveDealerChanges(dealerDescription));
        }

        [Test]
        public void UpdateDealerForAccountGroup()
        {
            const string accountGroupCode = "CFHABD";
            const string dealerGroupCodeOld = "Demo";
            const string dealerGroupCodeNew = "SiteSignalDealer";

            using (var ctx = DatabaseContext.Instance.Make())
            {
                Assert.IsNotNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeOld));
                Assert.IsNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeNew));
            }
            Assert.IsTrue(dealerRepository.UpdateDealerForAccountGroup(accountGroupCode, dealerGroupCodeNew));
            using (var ctx = DatabaseContext.Instance.Make())
            {
                Assert.IsNotNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeNew));
                Assert.IsNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeOld));
            }


            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            Assert.IsFalse(dealerRepository.UpdateDealerForAccountGroup(accountGroupCode, dealerGroupCodeNew));
        }

        [Test]
        public void UpdateDealerForAccountGroupAdd()
        {
            const string accountGroupCode = "TestCode";
            const string dealerGroupCodeOld = "Demo";
            const string dealerGroupCodeNew = "FIX";

// ReSharper disable RedundantAssignment
            var countDealerGroup = -1;
// ReSharper restore RedundantAssignment

            using (var ctx = DatabaseContext.Instance.Make())
            {
                ctx.ACCOUNT_GROUP.Add(new ACCOUNT_GROUP
                    {
                        Code = accountGroupCode,
                        Name = "CFH Olg Bez Test",
                        IsReal = true,
                        DefaultVirtualDepo = 10000,
                        BrokerLeverage = 500.00m,
                        MarginCallPercentLevel = 50.00m,
                        StopoutPercentLevel = 90.00m,
                        MarkupType = 0,
                        DefaultMarkupPoints = 0,
                        SwapFree = true
                    });

                Assert.IsNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeOld));
                Assert.IsNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeNew));

                countDealerGroup = ctx.DEALER_GROUP.Count();
                Assert.IsTrue(countDealerGroup >= 0);
            }
            Assert.IsTrue(dealerRepository.UpdateDealerForAccountGroup(accountGroupCode, dealerGroupCodeNew));
            using (var ctx = DatabaseContext.Instance.Make())
            {
                Assert.IsNotNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeNew));
                Assert.IsNull(ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode && x.Dealer == dealerGroupCodeOld));
                Assert.AreEqual(countDealerGroup + 1, ctx.DEALER_GROUP.Count());
            }
        }
    }
}