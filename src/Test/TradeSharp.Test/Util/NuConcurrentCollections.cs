using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Util
{
    class NuConcurrentCollections
    {
        private int workingThreadCount;
        private int result, threadSafeStorageResult, concurrentDictionaryResult;

        private ThreadSafeStorage<int, int> threadSafeStorage = new ThreadSafeStorage<int, int>();
        private ConcurrentDictionary<int, int> concurrentDictionary = new ConcurrentDictionary<int, int>();

        private readonly List<BackgroundWorker> threadSafeStorageWorkers = new List<BackgroundWorker>();
        private readonly List<BackgroundWorker> concurrentDictionaryWorkers = new List<BackgroundWorker>();

        //[TestFixtureSetUp]
        public void Setup()
        {
            var generator = new Random();
            for (var i = 0; i < 10000; i++ )
            {
                threadSafeStorage.UpdateValues(i, generator.Next());
            }

            for (var i = 0; i < 5; i++)
            {
                var worker = new BackgroundWorker();
                worker.RunWorkerCompleted += WorkCompleted;
                threadSafeStorageWorkers.Add(worker);
            }

            for (var i = 0; i < 5; i++)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += ConcurrentDictionaryGetAndUpdate;
                worker.RunWorkerCompleted += WorkCompleted;
                concurrentDictionaryWorkers.Add(worker);
            }
        }

        //[TestFixtureTearDown]
        public void Teardown()
        {
            Console.WriteLine("TreadSafeStorage count = {0}", threadSafeStorageResult);
            Console.WriteLine("ConcurrentDictionary count = {0}", concurrentDictionaryResult);
        }

        //[Test]
        public void TestGetAndUpdate()
        {
            // prep
            workingThreadCount = threadSafeStorageWorkers.Count;
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
            {
                threadSafeStorageWorkers[i].DoWork += ThreadSafeStorageGetAndUpdate;
                threadSafeStorageWorkers[i].RunWorkerAsync(2 * 1000 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
                threadSafeStorageWorkers[i].DoWork -= ThreadSafeStorageGetAndUpdate;
            threadSafeStorageResult = result;
            Assert.True(result != 0, "threadSafeStorageWorkers error");

            // prep
            result = 0;
            workingThreadCount = concurrentDictionaryWorkers.Count;
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
            {
                concurrentDictionaryWorkers[i].DoWork += ConcurrentDictionaryGetAndUpdate;
                concurrentDictionaryWorkers[i].RunWorkerAsync(2 * 1000 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
                concurrentDictionaryWorkers[i].DoWork -= ConcurrentDictionaryGetAndUpdate;
            concurrentDictionaryResult = result;
            Assert.True(result != 0, "concurrentDictionaryWorkers error");
        }

        //[Test]
        public void TestCopy()
        {
            // prep
            workingThreadCount = threadSafeStorageWorkers.Count;
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
            {
                threadSafeStorageWorkers[i].DoWork += ThreadSafeStorageCopy;
                threadSafeStorageWorkers[i].RunWorkerAsync(10 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
                threadSafeStorageWorkers[i].DoWork -= ThreadSafeStorageCopy;
            threadSafeStorageResult = result;
            Assert.True(result != 0, "threadSafeStorageWorkers error");

            // prep
            result = 0;
            workingThreadCount = concurrentDictionaryWorkers.Count;
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
            {
                concurrentDictionaryWorkers[i].DoWork += ConcurrentDictionaryCopy;
                concurrentDictionaryWorkers[i].RunWorkerAsync(10 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
                concurrentDictionaryWorkers[i].DoWork -= ConcurrentDictionaryCopy;
            concurrentDictionaryResult = result;
            Assert.True(result != 0, "concurrentDictionaryWorkers error");
        }

        //[Test]
        public void TestTerminalAction()
        {
            // prep
            workingThreadCount = threadSafeStorageWorkers.Count;
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
            {
                threadSafeStorageWorkers[i].DoWork += ThreadSafeStorageTerminalAction;
                threadSafeStorageWorkers[i].RunWorkerAsync(1 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < threadSafeStorageWorkers.Count; i++)
                threadSafeStorageWorkers[i].DoWork -= ThreadSafeStorageTerminalAction;
            threadSafeStorageResult = result;
            Assert.True(result != 0, "threadSafeStorageWorkers error");

            // prep
            result = 0;
            workingThreadCount = concurrentDictionaryWorkers.Count;
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
            {
                concurrentDictionaryWorkers[i].DoWork += ConcurrentDictionaryTerminalAction;
                concurrentDictionaryWorkers[i].RunWorkerAsync(1 * 1000);
            }
            // work
            while (workingThreadCount != 0) { }
            //end
            for (var i = 0; i < concurrentDictionaryWorkers.Count; i++)
                concurrentDictionaryWorkers[i].DoWork -= ConcurrentDictionaryTerminalAction;
            concurrentDictionaryResult = result;
            Assert.True(result != 0, "concurrentDictionaryWorkers error");
        }

        // doWorkEventArgs.Argument = operations count
        private void ThreadSafeStorageGetAndUpdate(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            var generator = new Random();
            while (count > 0)
            {
                var key = generator.Next(1000);
                var data = threadSafeStorage.ReceiveValue(key);
                threadSafeStorage.UpdateValues(key, generator.Next());
                count--;
            }
            doWorkEventArgs.Result = (int) DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void ConcurrentDictionaryGetAndUpdate(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            var generator = new Random();
            while (count > 0)
            {
                var key = generator.Next(1000);
                int value;
                concurrentDictionary.TryGetValue(key, out value);
                value = generator.Next();
                concurrentDictionary.AddOrUpdate(key, value, (k, v) => value);
                count--;
            }
            doWorkEventArgs.Result = (int)DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void ThreadSafeStorageCopy(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            while (count > 0)
            {
                var data = threadSafeStorage.ReceiveAllData();
                count--;
            }
            doWorkEventArgs.Result = (int)DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void ConcurrentDictionaryCopy(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            while (count > 0)
            {
                //var data = concurrentDictionary.ToArray().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var data = concurrentDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                count--;
            }
            doWorkEventArgs.Result = (int)DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void ThreadSafeStorageTerminalAction(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            var generator = new Random();
            while (count > 0)
            {
                var data = threadSafeStorage.ReceiveAllData();
                if (count % 1000 == 0)
                    for (var i = 0; i < 10000; i++)
                        threadSafeStorage.UpdateValues(i, generator.Next());
                count--;
            }
            doWorkEventArgs.Result = (int)DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void ConcurrentDictionaryTerminalAction(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var count = (int) doWorkEventArgs.Argument;
            var beginTime = DateTime.Now;
            var generator = new Random();
            while (count > 0)
            {
                var data = concurrentDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (count % 1000 == 0)
                    for (var i = 0; i < 10000; i++)
                    {
                        var value = generator.Next();
                        concurrentDictionary.AddOrUpdate(i, value, (k, v) => value);
                    }
                count--;
            }
            doWorkEventArgs.Result = (int)DateTime.Now.Subtract(beginTime).TotalMilliseconds;
        }

        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
                result = (int) e.Result;
            workingThreadCount--;
        }
    }
}
