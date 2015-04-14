using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.WebContract;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuWebContract
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
        public void Serialization()
        {
            var time = DateTime.Now;
            var objects = new List<HttpParameter>
                {
                    new TradeSharpServiceProcess
                        {
                            FileName = "TradeSharp.SiteBridge",
                            Name = "SiteBridge",
                            Title = "Мост Web-сервиса"
                        },
                    new TradeSharpServiceProcess
                        {
                            FileName = "TradeSharp.Quote",
                            Name = "Quote",
                            Title = "Котировки"
                        },
                    new TradeSharpServiceStartStop
                        {
                            ShouldStart = true,
                            SrvName = "TradeSharp.SiteBridge"
                        },
                    new ExecutionReport
                        {
                            Comment = "not OK",
                            IsOk = false
                        },
                    new TerminalUser
                        {
                            Account = 100,
                            IP = "198.15.12.45",
                            Login = "Вася"
                        },
                    new ChangeAccountBalanceQuery
                        {
                            AccountId = 3,
                            ChangeType = BalanceChangeType.Deposit,
                            Amount = 0.15M,
                            ValueDate = time
                        }
                };
            var str = HttpParameter.SerializeInJSon(objects);
            var objs = HttpParameter.DeserializeFromJSon(str);
            Assert.IsNotNull(objs);
            Assert.AreEqual(objs.Count, objects.Count);
            Assert.IsFalse(objects.Select(o => o.ToString()).Except(objs.Select(o => o.ToString())).Any());
        }
    }
}
