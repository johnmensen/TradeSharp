using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Effort.DataLoaders;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.Util;


namespace TradeSharp.Test.Processing
{
    //[TestFixture]
    public class NuPaymentProcessor
    {
        private TradeSharpConnectionPersistent connectionPersistent;

        [TestFixtureSetUp]
        public void InitTest()
        {
            connectionPersistent = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
        }

        [SetUp]
        public void Init()
        {
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            connectionPersistent.Cleanup();
        }      

        /*
        [Test]
        public void XmlPropertiesTest()
        {

        }
        */
    }
}
