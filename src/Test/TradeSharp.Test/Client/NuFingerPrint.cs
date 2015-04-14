using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeSharp.Client.BL;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuFingerPrint
    {
        [TestFixtureSetUp]
        public void TestSetup()
        {  
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FingerPrintTest()
        {

            var fingerPrintHash = FingerPrint.Instance.FingerPrintHash;
            var defaultRegex = new Regex(@"[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}-[a-z0-9A-Z]{4}");
            var matches = defaultRegex.Matches(fingerPrintHash);
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(fingerPrintHash, matches[0].ToString());

            Assert.IsNotNull(FingerPrint.Instance.fingerPrintLong);
            Assert.IsNotNullOrEmpty(FingerPrint.Instance.fingerPrintString);
            Assert.AreEqual(fingerPrintHash, FingerPrint.Instance.FingerPrintHash);
        }
    }
}
