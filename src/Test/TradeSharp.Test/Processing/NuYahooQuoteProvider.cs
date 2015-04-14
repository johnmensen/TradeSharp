using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeSharp.Linq;
using TradeSharp.Processing.Lib;

namespace TradeSharp.Test.Processing
{
    //[TestFixture]
    public class NuYahooQuoteProvider
    {
        [TestFixtureSetUp]
        public void InitTest()
        {
        }

        [SetUp]
        public void Init()
        {
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }
        /*
        [Test]
        public void XmlPropertiesTest()
        {
            var usdRub = YahooQuoteProvider.GetQuoteByKey("USDRUB");


            var walletCurrency ="RUB" ;
            var srcCurrency = "USD";
            double amount = 100;


            var r = PaymentProcessor.ConvertPaySysCurrencyToWalletCurrency(walletCurrency, srcCurrency, amount);
        }*/
    }
}
