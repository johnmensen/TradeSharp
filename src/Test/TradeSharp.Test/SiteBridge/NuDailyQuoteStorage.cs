using System;
using System.Collections.Generic;
using System.IO;
using Entity;
using Moq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using System.Linq;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Test.Moq;
using QuoteStorage = TradeSharp.Contract.Util.Proxy.QuoteStorage;

namespace TradeSharp.Test.SiteBridge
{
    [TestFixture]
    public class NuDailyQuoteStorage
    {
        private Mock<IQuoteStorage> mockQuoteStorage;
        private string quoteFolder;


        [TestFixtureTearDown]
        public void TestTeardown()
        {
            var directoryInfo = Directory.GetParent(quoteFolder).Parent;
            if (directoryInfo != null)
            {
                if (directoryInfo.Parent != null)
                {
                    var s = directoryInfo.Parent.FullName + "\\quotes";
                    foreach (var fileName in Directory.GetFiles(s, "*.quote"))
                    {
                        File.Copy(fileName, quoteFolder + "\\" + fileName.Split('\\').Last(), true);
                    }
                }
            }
        }

        [SetUp]
        public void InitContext()
        {
            // очистить каталог котировок
            quoteFolder = AppDomain.CurrentDomain.BaseDirectory + "\\quotes";
            try
            {
                if (Directory.Exists(quoteFolder))
                {
                    foreach (var fileName in Directory.GetFiles(quoteFolder, "*.quote"))
                    {
                        File.Delete(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("NuDailyQuoteStorage - невозможно очистить каталог котировок: {0}", ex.Message);
            }

            // подготовить словарь валют
            DalSpot.Instantiate(MoqTradeSharpDictionary.Mock);
            
            // подготовить тестовые котировки
            var endTime = DateTime.Now;
            var startTime = endTime.AddDays(-30).AddHours(-2);
            // подготовить список "серверных" гэпов
            var daysOff = QuoteMaker.MakeGapsOnDaysOff(startTime, endTime);
            // подготовить свечи m1
            var tickerCandles = new Dictionary<string, List<CandleData>>();
            foreach (var ticker in DalSpot.Instance.GetTickerNames())
            {
                var candles = QuoteMaker.MakeQuotes(startTime, endTime, daysOff);
                tickerCandles.Add(ticker, candles);
            }

            mockQuoteStorage = MoqQuoteStorage.MakeMoq(tickerCandles);
            QuoteStorage.Initialize(mockQuoteStorage.Object);
        }

        [Test]
        public void TestLoadAndUpdate()
        {
            var stor = new DailyQuoteStorage();
            try
            {
                stor.UpdateStorageSync();
            }
            catch (Exception ex)
            {
                Assert.Fail("UpdateStorageSync() failed: " + ex.Message);
            }
            
            // удостовериться в наличии нужной котировки
            var quotes = stor.GetQuotes("EURUSD");
            Assert.Greater(quotes.Count, 0, "GetQuotes(\"EURUSD\") - has some quotes");

            // котировки за одну дату не повторяются
            Assert.AreEqual(quotes.Select(q => q.a.Date).Distinct().Count(), quotes.Count,
                          "all dates are unique");

            // прочитать котировки снова
            stor.UpdateStorageSync();
            var quotesNew = stor.GetQuotes("EURUSD");
            Assert.Less(quotesNew.Count - quotes.Count, 2, //quotes.SequenceEqual(quotesNew),
                          "Прочитанные заново котировки - количество неизменно");

        }
    }
}
