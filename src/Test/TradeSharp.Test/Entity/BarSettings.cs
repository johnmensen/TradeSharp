using System.Collections.Generic;
using Entity;
using NUnit.Framework;

namespace TradeSharp.Test.Entity
{
    /// <summary>
    /// BarSettings, BarSettingsStorage
    /// </summary>
    [TestFixture]
    public class NuBarSettings
    {
        [TestFixtureSetUp]
        public void SetupMethods()
        {
        }

        [TestFixtureTearDown]
        public void TearDownMethods()
        {
        }


        [SetUp]
        public void SetupTest()
        {
        }

        [TearDown]
        public void TearDownTest()
        {
        }

        [Test]
        public void BarSettingsParse()
        {
            Assert.AreEqual(BarSettings.TryParseString("30;#60"), new BarSettings
                                                                      {
                                                                          Intervals = new List<int> {60},
                                                                          StartMinute = 30
                                                                      });
            Assert.IsNull(BarSettings.TryParseString("-30#60"));
            Assert.IsNotNull(BarSettings.TryParseString("5#180;240;180"));
        }

        [Test]
        public void BarSettingsEqualityTest()
        {
            Assert.IsTrue(new BarSettings("30;#60") == new BarSettings("30#60"));
            Assert.IsFalse(new BarSettings("30;#60") == null);
        }
    }
}