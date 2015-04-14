using NUnit.Framework;
using TradeSharp.SiteAdmin.Models.CommonClass;

namespace TradeSharp.AdminSite.Test.VoidTests
{
    [TestFixture]
    class NuVoidTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void Teardown()
        {

        }

        #region Utils
        [Test]
        public void NuGetMetadataTypeTitle()
        {
            var result = Utils.GetMetadataTypeTitle();

            Assert.NotNull(result);
            Assert.GreaterOrEqual(result.Count, 1);
        }

        [Test]
        public void NuGetDefaultPortfolioTradeSettings()
        {
            var result = Utils.GetDefaultPortfolioTradeSettings();
            Assert.NotNull(result);
            Assert.IsTrue(result.TradeAuto);
            Assert.AreEqual(100, result.PercentLeverage);
            Assert.AreEqual(15, result.MaxLeverage);
            Assert.AreEqual(10000, result.MinVolume);
            Assert.AreEqual(10000, result.StepVolume );
        }
        #endregion
        
    }
}
