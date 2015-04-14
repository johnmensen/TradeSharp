using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Test.Entity
{
    /// <summary>
    /// CandleData, CandlePacker, CandleStorage, AtomCandleStorage
    /// </summary>
    [TestFixture]
    public class NuCandle
    {
        const int VersePointValue = 10000;

        private List<CandleData> srcCandles;

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
            srcCandles = new List<CandleData>
                {
                    new CandleData
                        {
                            open = (float) 1.2345,
                            high = (float) 1.2355,
                            low = (float) 1.2335,
                            close = (float) 1.2345,
                            timeOpen = new DateTime(2013, 1, 1, 0, 0, 0)
                        },
                    new CandleData
                        {
                            open = (float) 1.2345,
                            high = (float) 1.2344,
                            low = (float) 1.2324,
                            close = (float) 1.2335,
                            timeOpen = new DateTime(2013, 1, 1, 0, 1, 0)
                        },
                    new CandleData
                        {
                            open = (float) 1.2335,
                            high = (float) 1.2345,
                            low = (float) 1.2335,
                            close = (float) 1.2335, // last candle, close must have default value = open
                            timeOpen = new DateTime(2013, 1, 1, 0, 2, 0)
                        }
                };
        }

        [TearDown]
        public void TearDownTest()
        {
        }

        [Test]
        public void TestHlcFormat()
        {
            var candleData = new CandleData
                {
                    open = (float) 1.2345,
                    high = (float) 1.2355,
                    low = (float) 1.2335,
                    close = (float) 1.2344
                };

            var offset = candleData.GetHlcOffset(VersePointValue);
            Assert.AreEqual(0x7E7589, offset, "TestHlcFormat: GetHlcOffset error");
            var line = candleData.GetHlcOffsetHEX(VersePointValue);
            Assert.AreEqual("7E7589", line, "TestHlcFormat: GetHlcOffsetHEX error");

            var offset16 = candleData.GetHlcOffset16(VersePointValue);
            Assert.AreEqual(0x7F9B8063, offset16, "TestHlcFormat: GetHlcOffset16 error");
            var line16 = candleData.GetHlcOffsetHEX16(VersePointValue);
            Assert.AreEqual("7F9B8063", line16, "TestHlcFormat: GetHlcOffsetHEX16 error");

            var candle = new CandleData {open = candleData.open};
            candle.MakeHlcFromOffset(offset, VersePointValue);
            Assert.AreEqual(candleData.high, candle.high, 0.00001, "TestHlcFormat: MakeHlcFromOffset error (high)");
            Assert.AreEqual(candleData.low, candle.low, 0.00001, "TestHlcFormat: MakeHlcFromOffset error (low)");
            Assert.AreEqual(candleData.close, candle.close, 0.00001, "TestHlcFormat: MakeHlcFromOffset error (close)");

            candle.MakeHlcFromOffsetHEX(line, VersePointValue);
            Assert.AreEqual(candleData.high, candle.high, 0.00001, "TestHlcFormat: MakeHlcFromOffsetHEX error (high)");
            Assert.AreEqual(candleData.low, candle.low, 0.00001, "TestHlcFormat: MakeHlcFromOffsetHEX error (low)");
            Assert.AreEqual(candleData.close, candle.close, 0.00001, "TestHlcFormat: MakeHlcFromOffsetHEX error (close)");

            candle.MakeHlcFromOffset16(offset16, VersePointValue);
            Assert.AreEqual(candleData.high, candle.high, 0.00001, "TestHlcFormat: MakeHlcFromOffset16 error (high)");
            Assert.AreEqual(candleData.low, candle.low, 0.00001, "TestHlcFormat: MakeHlcFromOffset16 error (low)");

            candle.MakeHlcFromOffsetHEX16(line16, VersePointValue);
            Assert.AreEqual(candleData.high, candle.high, 0.00001, "TestHlcFormat: MakeHlcFromOffset16 error (high)");
            Assert.AreEqual(candleData.low, candle.low, 0.00001, "TestHlcFormat: MakeHlcFromOffset16 error (low)");
        }

        [Test]
        public void TestParseLine()
        {
            var parsedCandles = new List<CandleData>();
            CandleData previousCandle = null;
            DateTime? date = srcCandles[0].timeOpen;
            foreach (var candle in srcCandles)
            {
                var line = string.Format("{0:HHmm} {1} {2}",
                                         candle.timeOpen, candle.open.ToString("f4", CultureInfo.InvariantCulture),
                                         candle.GetHlcOffsetHEX(VersePointValue));
                parsedCandles.Add(CandleData.ParseLine(line, ref date, VersePointValue, ref previousCandle));
            }
            for (var candleIndex = 0; candleIndex < srcCandles.Count;candleIndex++)
            {
                var candle = srcCandles[candleIndex];
                var parsedCandle = parsedCandles[candleIndex];
                Assert.AreEqual(candle.high, parsedCandle.high, 0.000001, "ParseTest: ParseLine error (high)");
                Assert.AreEqual(candle.low, parsedCandle.low, 0.000001, "ParseTest: ParseLine error (low)");
                Assert.AreEqual(candle.close, parsedCandle.close, 0.000001, "ParseTest: ParseLine error (close)");
            }

            parsedCandles.Clear();
            previousCandle = null;
            foreach (var candle in srcCandles)
            {
                var line = string.Format("{0:HHmm} {1} {2}",
                                         candle.timeOpen, candle.open.ToString("f5", CultureInfo.InvariantCulture),
                                         candle.GetHlcOffsetHEX16(VersePointValue));
                parsedCandles.Add(CandleData.ParseLine(line, ref date, VersePointValue, ref previousCandle));
            }
            for (var candleIndex = 0; candleIndex < srcCandles.Count; candleIndex++)
            {
                var candle = srcCandles[candleIndex];
                var parsedCandle = parsedCandles[candleIndex];
                Assert.AreEqual(candle.high, parsedCandle.high, 0.000001, "TestParseLine: ParseLine error (high)");
                Assert.AreEqual(candle.low, parsedCandle.low, 0.000001, "TestParseLine: ParseLine error (low)");
                Assert.AreEqual(candle.close, parsedCandle.close, 0.000001, "TestParseLine: ParseLine error (close)");
            }
        }

        [Test]
        public void TestPackedCandleStream()
        {
            var stream =
                new PackedCandleStream(
                    srcCandles.Select(
                        c =>
                        new CandleDataPacked
                            {
                                timeOpen = c.timeOpen,
                                open = c.open,
                                HLC = c.GetHlcOffset16(VersePointValue),
                                close = c.close
                            }).ToList(), true);
            var parsedCandles = stream.GetCandles().Select(c =>
                {
                    var candle = new CandleData {timeOpen = c.timeOpen, open = c.open, close =  c.close};
                    candle.MakeHlcFromOffset16(c.HLC, VersePointValue);
                    return candle;
                }).ToList();
            for (var candleIndex = 0; candleIndex < srcCandles.Count; candleIndex++)
            {
                var candle = srcCandles[candleIndex];
                var parsedCandle = parsedCandles[candleIndex];
                Assert.AreEqual(candle.high, parsedCandle.high, 0.000001, "TestPackedCandleStream: ParseLine error (high)");
                Assert.AreEqual(candle.low, parsedCandle.low, 0.000001, "TestPackedCandleStream: ParseLine error (low)");
                Assert.AreEqual(candle.close, parsedCandle.close, 0.000001, "TestPackedCandleStream: ParseLine error (close)");
            }
        }
    }
}
