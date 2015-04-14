using System;
using Entity;
using NUnit.Framework;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Linq
{
    //[TestFixture]
    class DatabaseQuoteEnquirerTest
    {
        private readonly string[] spotList = { "AUDCAD", "AUDCHF", "AUDJPY", "AUDNZD", "AUDUSD", "CADJPY", "CHFJPY", "CHFX", "EURAUD", "EURCAD", "EURCHF", "EURDKK", 
                                    "EURGBP", "EURJPY", "EURNOK", "EURNZD", "EURSEK", "EURUSD", "EURX", "GBPAUD", "GBPCAD", "GBPCHF", "GBPJPY", "GBPNZD", 
                                    "GBPUSD", "GBPX", "JPYX", "NZDJPY", "NZDUSD", "USDCAD", "USDCHF" , "USDDKK", "USDHKD", "USDJPY", "USDNOK", "USDRUB", 
                                    "USDSEK", "USDSGD", "USDX", "USDZAR" };

        private DatabaseQuoteEnquirer adoHalper;



        [TestFixtureSetUp]
        public void SetupMethods()
        {
            adoHalper = new DatabaseQuoteEnquirer();
            DalSpot.Instantiate(MoqTradeSharpDictionary.Mock);
        }

        [TestFixtureTearDown]
        public void TearDownMethods()
        {
            DalSpot.Instantiate(null);
        }

        [SetUp]
        public void SetupTest()
        {            
        }

        [TearDown]
        public void TearDownTest()
        {
        }

        //[Test]
        public void GetLastQuoteStoredProcTest()
        {
            foreach (var spot in spotList)
            {
                var quoteLast1 = adoHalper.GetQuoteStoredProc(spot);
                Assert.IsNotNull(quoteLast1);
                Assert.IsTrue(quoteLast1.ask > 0);
                Assert.IsTrue(quoteLast1.bid > 0);
                Assert.IsTrue(quoteLast1.bid < quoteLast1.ask);

                var quoteLast2 = adoHalper.GetQuoteStoredProc(spot, new DateTime(2014, 1, 1));
                Assert.IsNotNull(quoteLast2);
            }


            Assert.IsNull(adoHalper.GetQuoteStoredProc(String.Empty));
            Assert.IsNull(adoHalper.GetQuoteStoredProc(null));

            Assert.IsNotNull(adoHalper.GetQuoteStoredProc("df"));
        }

    }
}
