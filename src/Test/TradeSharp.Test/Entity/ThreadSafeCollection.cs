using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    /// <summary>
    /// ThreadSafeTimeStamp, ThreadSafeList, ThreadSafeQueue, ThreadSafeStorage, ThreadSafeUpdatingList
    /// </summary>
    [TestFixture]
    public class NuThreadSafeCollection
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
        public void ThreadSafeTimeStampTest()
        {
            var timeSt = new ThreadSafeTimeStamp();

            var method = new WaitCallback(s =>
                {
                    for (var i = 0; i < 4; i++)
                    {
                        timeSt.Touch();
                        Thread.Sleep(200);
                        var delta = (DateTime.Now - timeSt.GetLastHit()).TotalMilliseconds;
                    }
                });

            ThreadPool.QueueUserWorkItem(method);
            ThreadPool.QueueUserWorkItem(method);
        }

        [Test]
        public void ThreadSafeQueueTest()
        {
            var q = new ThreadSafeQueue<string>();

            var methodFill = new WaitCallback(s =>
            {
                for (var i = 0; i < 33; i++)
                {
                    Assert.IsTrue(q.InQueue("iter" + i, 1000));
                    Thread.Sleep(20);                    
                }
            });
            var methodFlush = new WaitCallback(s =>
                {
                    var countRead = 0;
                    for (var i = 0; i < 40; i++)
                    {
                        bool timeout;
                        var items = q.ExtractAll(1000, out timeout);
                        countRead += items.Count;
                        Thread.Sleep(20);
                    }
                    Assert.IsTrue(countRead > 0);
                });
            ThreadPool.QueueUserWorkItem(methodFill);
            ThreadPool.QueueUserWorkItem(methodFlush);
        }

        [Test]
        public void ThreadSafeListCopyTest()
        {
            var list1 = new ThreadSafeList<QuoteData>();
            var list2 = new ThreadSafeList<int>();

            list1.AddRange(new[] {new QuoteData(1, 2, DateTime.Now), null}, 1000);
            list2.AddRange(new[] {1, 2, 3}, 1000);

            var found1 = list1.Find(x => true, x => new QuoteData(x), 1000);
            Assert.IsTrue(found1.ask == 2 && found1.bid == 1);
            var found2 = list1.Find(x => x == null, x => new QuoteData(x), 1000);
            Assert.IsNull(found2);
            found2 = list1.Find(x => x.ask == 100, x => new QuoteData(x), 1000);
            Assert.IsNull(found2);

            var found3 = list2.Find(x => x == 2, x => x, 1000);
            Assert.IsTrue(found3 == 2);
            found3 = list2.Find(x => x == 100, x => x, 1000);
            Assert.AreEqual(found3, default(int));
        }

        [Test]
        public void ThreadSafeStorageExtractTest()
        {
            var stor = new ThreadSafeStorage<int, int>();
            stor.UpdateValues(1, 10);
            stor.UpdateValues(2, 20);
            stor.UpdateValues(3, 30);

            Assert.AreEqual(10, stor.ExtractData(1));
            Assert.AreEqual(20, stor.ExtractData(2));
            Assert.AreEqual(default(int), stor.ExtractData(1));
            Assert.AreEqual(default(int), stor.ExtractData(4));
            Assert.AreEqual(30, stor.ExtractData(3));
        }
    }
}
