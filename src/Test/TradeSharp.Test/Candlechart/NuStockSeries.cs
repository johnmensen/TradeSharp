using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Candlechart;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using NUnit.Framework;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Candlechart
{
    [TestFixture]
    public class NuStockSeries
    {
        class StockSeriesDebug : StockSeries
        {
            public StockSeriesDebug(string name) : base(name)
            {
            }
        }

        private StockSeriesDebug stockSeries;

        private const string Symbol = "INDUSD";
        private List<CandleData> candles;

        readonly Dictionary<int, DateTime> trueDate = new Dictionary<int, DateTime>
                {
                    {3, new DateTime(2013, 09, 30, 1, 8, 0)},
                    {100, new DateTime(2013, 09, 30, 2, 45, 0)},
                    {30174, new DateTime(2013, 10, 28, 23, 50, 0)},
                    {51366, new DateTime(2013, 11, 19, 21, 0, 0)},
                    {67460, new DateTime(2013, 12, 5, 7, 12, 0)}
                };

        readonly Dictionary<int, DateTime> fallDate = new Dictionary<int, DateTime>
                {
                    {3, new DateTime(2013, 08, 30, 3, 8, 0)},
                    {100, new DateTime(2013, 08, 30, 2, 49, 0)},
                    {30174, new DateTime(2013, 10, 28, 20, 50, 0)},
                    {51366, new DateTime(2013, 11, 11, 21, 0, 0)},
                    {67460, new DateTime(2013, 12, 15, 8, 12, 0)}
                };

        [TestFixtureSetUp]
        public void TestSetup()
        {
            stockSeries = new StockSeriesDebug("StockSeriesDebug");

            ExecutablePath.InitializeFake(string.Empty);
            AtomCandleStorage.Instance.LoadFromFile(ExecutablePath.ExecPath + "\\quotes", Symbol);
            candles = AtomCandleStorage.Instance.GetAllMinuteCandles(Symbol);

            Localizer.ResourceResolver = new MockResourceResolver();
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            ExecutablePath.Unitialize();
            Localizer.ResourceResolver = null;
        }

        [SetUp]
        public void Setup()
        {
            stockSeries.Data.Clear();
            foreach (var candleData in candles)
            {
                stockSeries.Data.Add(candleData.open, candleData.high, candleData.low, candleData.close, candleData.timeOpen);
                stockSeries.Data.Candles.Last().timeClose = candleData.timeClose;
            }
        }

        [Test]
        public void GetYExtent()
        {
            var top = 1.3495d;
            var bottom = 1.3495d;

            var resultTrue = stockSeries.GetYExtent(-15.5, 15.5, ref top, ref bottom);

            Assert.IsTrue(resultTrue, "must be True");
            Assert.IsTrue(top.RoughCompares(1.3499, 0.0001), "must be 1.3499");
            Assert.IsTrue(bottom.RoughCompares(1.34909, 0.0001), "must be 1.34909");

            stockSeries.Data.Clear();
            var resultFalse = stockSeries.GetYExtent(-15.5, 15.5, ref top, ref bottom);
            Assert.IsFalse(resultFalse, "must be False");
        }

        [Test]
        public void GetIndexByCandleOpen()
        {
            var candleChartControl = new CandleChartControl
                {
                    Timeframe = new BarSettings {Intervals = new List<int> { 1 }, Title = "M1"}
                };
            var chartControl = new ChartControl
                {
                    Owner = candleChartControl,
                    Timeframe = new BarSettings { Intervals = new List<int> { 1 }, Title = "M1" }
                };
            stockSeries.Owner = new Pane("Pane", chartControl);

            foreach (var date in trueDate)
                Assert.AreEqual(date.Key, stockSeries.GetIndexByCandleOpen(date.Value), "not a correct value for " + date.Value);

            foreach (var date in fallDate)
                Assert.AreNotEqual(date.Key, stockSeries.GetIndexByCandleOpen(date.Value), "not a correct value for " + date.Value);

            stockSeries.Data.Clear();
            Assert.AreEqual(0, stockSeries.GetIndexByCandleOpen(new DateTime(2013, 9, 30, 1, 8, 0)), "must be 0 for 2013/9/30  01:08:00  after 'Clear()'");

            stockSeries.Data.Add(0, 0, 0, 0, new DateTime(2013, 10, 30, 1, 8, 0));
            Assert.AreEqual(0, stockSeries.GetIndexByCandleOpen(new DateTime(2013, 9, 30, 1, 8, 0)), "must be 0 for 2013/9/30  01:08:00");

            Assert.AreNotEqual(0, stockSeries.GetIndexByCandleOpen(new DateTime(2014, 11, 30, 1, 8, 0)), "must be !0 for 2013/11/30  01:08:00");

            // У первой свечи время закрытия сейчас не задано.
            stockSeries.Data.Add(0, 0, 0, 0, new DateTime(2013, 10, 30, 1, 9, 0));
            stockSeries.Data.Candles.Last().timeClose = new DateTime(2013, 10, 30, 1, 10, 0);
            stockSeries.Data.Add(0, 0, 0, 0, new DateTime(2013, 10, 30, 1, 10, 0));
            stockSeries.Data.Candles.Last().timeClose = new DateTime(2013, 10, 30, 1, 11, 0);
            Assert.AreEqual(1, stockSeries.GetIndexByCandleOpen(new DateTime(2013, 10, 30, 1, 9, 0)));
            Assert.AreEqual(0, stockSeries.GetIndexByCandleOpen(new DateTime(2013, 10, 30, 1, 8, 30)));


            stockSeries.Owner.Owner.Owner = null;
            Assert.AreEqual(0, stockSeries.GetIndexByCandleOpen(new DateTime(2013, 11, 30, 1, 8, 0)));
            
        }

        [Test]
        public void GetDoubleIndexByTime()
        {
            foreach (var date in trueDate)
                Assert.AreEqual(date.Key, stockSeries.GetDoubleIndexByTime(date.Value), "not a correct value for " + date.Value);

            foreach (var date in fallDate)
                Assert.AreNotEqual(date.Key, stockSeries.GetDoubleIndexByTime(date.Value), "not a correct value for " + date.Value);

            stockSeries.Data.Clear();
            Assert.AreEqual(0, stockSeries.GetDoubleIndexByTime(new DateTime(2013, 9, 30, 1, 8, 0)), "must be 0 for 2013/9/30  01:08:00  after 'Clear()'");


            stockSeries.Data.Add(0, 0, 0, 0, new DateTime(2013, 10, 30, 1, 8, 0));
            Assert.AreEqual(0, stockSeries.GetDoubleIndexByTime(new DateTime(2013, 9, 30, 1, 8, 0)), "must be 0 for 2013/9/30  01:08:00");

            Assert.AreEqual(-1d, stockSeries.GetDoubleIndexByTime(new DateTime(2013, 9, 30, 1, 8, 0), true), "must be -1 for 2013/9/30  01:08:00");

            stockSeries.Data.Candles.Last().timeClose = new DateTime(2013, 10, 30, 1, 9, 0);
            Assert.AreEqual(0, stockSeries.GetDoubleIndexByTime(new DateTime(2013, 10, 30, 1, 9, 0)));
        }
     
        [Test]
        public void GetCandleOpenTimeByIndex()
        {
            Assert.AreEqual(new DateTime(), stockSeries.GetCandleOpenTimeByIndex(-1), "must be 'new DateTime()' for -1");


            foreach (var date in trueDate)
                Assert.AreEqual(date.Value, stockSeries.GetCandleOpenTimeByIndex(date.Key), "not a correct value for " + date.Value);


            stockSeries.Data.Clear();
            Assert.AreEqual(new DateTime(), stockSeries.GetCandleOpenTimeByIndex(1), "must be 'new DateTime()' for 1 after 'Clear()'");

            var candleChartControl = new CandleChartControl
            {
                Timeframe = new BarSettings { Intervals = new List<int> { 1 }, Title = "M1" }
            };
            var chartControl = new ChartControl
            {
                Owner = candleChartControl,
                Timeframe = new BarSettings { Intervals = new List<int> { 1 }, Title = "M1" }
            };
            stockSeries.Owner = new Pane("Pane", chartControl);
            stockSeries.Data.Add(0, 0, 0, 0, new DateTime(2013, 10, 30, 1, 8, 0));
            Assert.AreEqual(new DateTime(2013, 10, 30, 1, 10, 0), stockSeries.GetCandleOpenTimeByIndex(2));
        }

        [Test]
        public void GetCandleOpenTimeByIndexManyIntervals()
        {
            var candleChartControl = new CandleChartControl
            {
                Timeframe = new BarSettings { Intervals = new List<int> { 1, 30, 60 }, Title = "M1 M30 H1" }
            };
            var chartControl = new ChartControl
            {
                Owner = candleChartControl,
                Timeframe = new BarSettings { Intervals = new List<int> { 1, 30, 60 }, Title = "M1 M30 H1" }
            };
            stockSeries.Owner = new Pane("Pane", chartControl);
            Assert.AreEqual(new DateTime(2014, 1, 30, 5, 4, 0), stockSeries.GetCandleOpenTimeByIndex(119843));
        }

        [Test]
        public void GetPrice()
        {
            Assert.IsNull(stockSeries.GetPrice(-1), "index '-1' must get null");
            Assert.IsNull(stockSeries.GetPrice(stockSeries.DataCount + 1), "index '-1' must get null");
            Assert.AreEqual(stockSeries.Data.Candles.Last().close, stockSeries.GetPrice(stockSeries.Data.LastIndex), "index '-1' must get null");
        }

        /// <summary>
        /// для большего процента покрытия
        /// </summary>
        [Test]
        public void CheckProperty()
        {
            stockSeries.UpFillColor = Color.AliceBlue;
            Assert.AreEqual(Color.AliceBlue, stockSeries.UpFillColor);

            stockSeries.UpLineColor = Color.AliceBlue;
            Assert.AreEqual(Color.AliceBlue, stockSeries.UpLineColor);

            stockSeries.DownFillColor = Color.AliceBlue;
            Assert.AreEqual(Color.AliceBlue, stockSeries.DownFillColor);

            stockSeries.DownLineColor = Color.AliceBlue;
            Assert.AreEqual(Color.AliceBlue, stockSeries.DownLineColor);

            stockSeries.BarNeutralColor = Color.AliceBlue;
            Assert.AreEqual(Color.AliceBlue, stockSeries.BarNeutralColor);

            stockSeries.BarLineWidth = 0;
            Assert.AreEqual(0, stockSeries.BarLineWidth);
        }
    }
}
