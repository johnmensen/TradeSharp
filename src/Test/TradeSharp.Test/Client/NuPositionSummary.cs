using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Effort.DataLoaders;
using NUnit.Framework;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    //[TestFixture]
    class NuPositionSummary
    {
        private TradeSharpConnectionPersistent connectionPersistent;
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
            connectionPersistent = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            DatabaseContext.InitializeFake(connectionPersistent);
        }

        [TearDown]
        public void Teardown()
        {
            connectionPersistent.Cleanup();
        }

        [Test]
        public void TestConvertExpositionToDepo()
        {
            /*
            var symbol = "";
            var accountCurrency = "";

            
            var sum = new PositionSummary { Symbol = symbol, orders = new List<MarketOrder>() };


            var exposition = (int)sum.ConvertExpositionToDepo(sum.Symbol, accountCurrency, quotes,
                                                             sum.Volume > 0 ? QuoteType.Bid : QuoteType.Ask,
                                                             sum.Volume);*/
        }
    }
}
