using System;
using System.Linq;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuCandlePacker
    {
        private const string Symbol = "INDUSD";
        private CandlePacker packer;
        private List<CandleData> candles;
        private List<QuoteData> quotes;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            ExecutablePath.InitializeFake(string.Empty);
            AtomCandleStorage.Instance.LoadFromFile(ExecutablePath.ExecPath + "\\quotes", Symbol);
            candles = AtomCandleStorage.Instance.GetAllMinuteCandles(Symbol);
            quotes = candles.Select(q => new QuoteData(q.close, DalSpot.Instance.GetAskPriceWithDefaultSpread(Symbol, q.close), q.timeOpen)).ToList();
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            ExecutablePath.Unitialize();
        }

        [SetUp]
        public void Setup()
        {
            var barSettings = new BarSettings { Intervals = new List<int> { 60 }, Title = "H1" };
            packer = new CandlePacker(barSettings);
        }

        [TearDown]
        public void Teardown()
        {

        }

        #region UpdateCandle
        [Test]
        public void UpdateCandleNull()
        {
            var candleFirst = candles.First(x => x.timeOpen.Minute == 2);
            var updateCandleFirst = packer.UpdateCandle(candleFirst);

            Assert.IsNull(updateCandleFirst);
        }

        [Test]
        public void UpdateCandleHour()
        {
            var candleStart = candles.First(x => x.timeOpen.Minute == 0);
            var candleEnd = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.Hour != candleStart.timeOpen.Hour);

            var indexStart = candles.IndexOf(candleStart);
            var inedxEnd = candles.IndexOf(candleEnd);

            for (var i = indexStart; i < inedxEnd; i++) Assert.IsNull(packer.UpdateCandle(candles[i]));
            var updateCandleHour = packer.UpdateCandle(candles[inedxEnd + 1]);

            Assert.IsNotNull(updateCandleHour);
            Assert.AreEqual(0, updateCandleHour.timeOpen.Minute);
            Assert.AreEqual(0, updateCandleHour.timeClose.Minute);
            Assert.AreEqual(candleStart.timeOpen.Hour, updateCandleHour.timeOpen.Hour);
            Assert.AreEqual(candleStart.timeOpen.Hour + 1, updateCandleHour.timeClose.Hour);
        }

        [Test]
        public void UpdateCandle4Hour()
        {
            packer.BarSettings = new BarSettings {Intervals = new List<int> {240}, Title = "H4"};

            var candleStart = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.Hour == 4);
            var candleEnd = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.Hour == 8);

            var indexStart = candles.IndexOf(candleStart);
            var inedxEnd = candles.IndexOf(candleEnd);

            for (var i = indexStart; i < inedxEnd; i++) Assert.IsNull(packer.UpdateCandle(candles[i]));
            var updateCandle4Hour = packer.UpdateCandle(candles[inedxEnd + 1]);

            Assert.IsNotNull(updateCandle4Hour);
            Assert.AreEqual(0, updateCandle4Hour.timeOpen.Minute);
            Assert.AreEqual(0, updateCandle4Hour.timeClose.Minute);
        }

        [Test]
        public void UpdateCandleOnPrice()
        {
            var quoteFirst = quotes.First();
            var updateCandleFirst = packer.UpdateCandle(quoteFirst.bid, quoteFirst.time);
            Assert.IsNull(updateCandleFirst);
        }

        [Test]
        public void UpdateCandleOnPriceHour()
        {
            var quoteStart = quotes.First(x => x.time.Minute == 0);
            var quoteEnd = quotes.First(x => x.time.Minute == 0 && x.time.Hour != quoteStart.time.Hour);

            var indexStart = quotes.IndexOf(quoteStart);
            var inedxEnd = quotes.IndexOf(quoteEnd);

            for (var i = indexStart; i < inedxEnd; i++)
                Assert.IsNull(packer.UpdateCandle(quotes[i].bid, quotes[i].time));
            var updateCandleHour = packer.UpdateCandle(quotes[inedxEnd + 1].bid, quotes[inedxEnd + 1].time);

            Assert.IsNotNull(updateCandleHour);
            Assert.AreEqual(0, updateCandleHour.timeOpen.Minute);
            Assert.AreEqual(0, updateCandleHour.timeClose.Minute);
            Assert.AreEqual(quoteStart.time.Hour, updateCandleHour.timeOpen.Hour);
            Assert.AreEqual(quoteStart.time.Hour + 1, updateCandleHour.timeClose.Hour);
        }
        #endregion

        [Test]
        public void GetLastCandleClose()
        {
            var candleStart = candles.First(x => x.timeOpen.Minute == 0);
            var candleEnd = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.Hour != candleStart.timeOpen.Hour);

            var indexStart = candles.IndexOf(candleStart);
            var inedxEnd = candles.IndexOf(candleEnd);

            for (var i = indexStart; i < inedxEnd; i++) Assert.IsNull(packer.UpdateCandle(candles[i]));
            var updateCandleHour = packer.UpdateCandle(candles[inedxEnd + 1]);

            Assert.IsNotNull(updateCandleHour);
            Assert.AreEqual(0, updateCandleHour.timeOpen.Minute);

            DateTime candleOpenTime;
            var lastCandleClose = packer.GetLastCandleClose(candles[indexStart].timeOpen, out candleOpenTime);

            Assert.IsNotNull(lastCandleClose);
            Assert.AreEqual(candleEnd.timeClose.AddMinutes(-1), lastCandleClose);
        }

        [Test]
        public void GetLastCandleCloseWeek()
        {
            packer.BarSettings = new BarSettings { Intervals = new List<int> { 1440 * 5 }, Title = "W1", StartMinute = 0};

            var candleStart = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.DayOfWeek == DayOfWeek.Monday);
            var candleEnd = candles.First(x => x.timeOpen.DayOfWeek == DayOfWeek.Monday && x.timeOpen.Day != candleStart.timeOpen.Day);

            var indexStart = candles.IndexOf(candleStart);
            var inedxEnd = candles.IndexOf(candleEnd);

            for (var i = indexStart; i < inedxEnd; i++) Assert.IsNull(packer.UpdateCandle(candles[i]));
            var updateCandleHour = packer.UpdateCandle(candles[inedxEnd + 1]);

            Assert.IsNotNull(updateCandleHour);
            Assert.AreEqual(0, updateCandleHour.timeOpen.Minute);

            DateTime candleOpenTime;
            var lastCandleClose = packer.GetLastCandleClose(candles[indexStart].timeOpen, out candleOpenTime);

            Assert.IsNotNull(lastCandleClose);
            Assert.AreEqual(candleEnd.timeClose.Day, lastCandleClose.Day);
        }

        [Test]
        public void GetLastCandleCloseWeekTuesday()
        {
            packer.BarSettings = new BarSettings { Intervals = new List<int> { 1440 * 5 }, Title = "W1", StartMinute = 0 };
            var candleStart = candles.First(x => x.timeOpen.Minute == 0 && x.timeOpen.DayOfWeek == DayOfWeek.Tuesday);  
            var indexStart = candles.IndexOf(candleStart);

            CandleData updateCandleHour;
            do
            {
                updateCandleHour = packer.UpdateCandle(candles[indexStart]);
                indexStart++;

            } while (updateCandleHour == null);
          
            
            Assert.IsNotNull(updateCandleHour);
            //Assert.AreEqual(DayOfWeek.Sunday, updateCandleHour.timeOpen.DayOfWeek);

            DateTime candleOpenTime;
            var lastCandleClose = packer.GetLastCandleClose(candles[indexStart].timeOpen, out candleOpenTime);

            Assert.IsNotNull(lastCandleClose);
            //Assert.AreEqual(candleStart.timeOpen.AddDays(7).Day, updateCandleHour.timeOpen.Day);
        }

        [Test]
        public void TestGetLastCandleClose()
        {
            packer = new CandlePacker(new BarSettings() { Intervals = new EditableList<int> { 5 } });
            
            DateTime candleOpenTime;
            int nextCandlePeriod;
            var time = new DateTime(2014, 6, 18, 9, 5, 0);
            var timeClose = packer.GetLastCandleClose(time, out candleOpenTime, out nextCandlePeriod);
            Assert.AreEqual(time, candleOpenTime, "GetLastCandleClose() - время начала свечи должно быть равно текущему");
            Assert.AreEqual(time.AddMinutes(5), timeClose, "GetLastCandleClose() - время закрытия свечи должно быть равно текущему + 5 минут");

            var timeShifted = new DateTime(2014, 6, 18, 9, 5, 0, 10, DateTimeKind.Unspecified);
            timeClose = packer.GetLastCandleClose(timeShifted, out candleOpenTime, out nextCandlePeriod);
            Assert.AreEqual(time, candleOpenTime, "GetLastCandleClose(+10ms) - время начала свечи должно быть равно текущему");
            Assert.AreEqual(time.AddMinutes(5), timeClose, "GetLastCandleClose(+10ms) - время закрытия свечи должно быть равно текущему + 5 минут");
        }
    }
}
