using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuCandlePackerPool
    {
        [TestFixtureSetUp]
        public void SetupMethods()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
        }

        [Test]
        public void TestMakeCandles()
        {
            var pool = new CandlePackerPool();

            var quoteNames = new[] {"EURUSD", "GBPUSD"};

            CandleDataBidAsk[] candles = null;
            for (var date = DateTime.Now.Date; date < DateTime.Now.Date.AddDays(1).AddSeconds(1); date = date.AddSeconds(22))
            {
                candles = pool.MakeCandles(new[]
                    {
                        new QuoteData(1, 1.0001f, date),
                        new QuoteData(2, 2.0001f, date),
                    }, ref quoteNames);
                Assert.IsNotNull(candles, "CandlePackerPool.MakeCandles - должен таки вернуть свечи");
                Assert.AreEqual(candles.Length, 2, "CandlePackerPool.MakeCandles - должен таки вернуть 2 свечи");
            }
            var candle = candles[0];
            Assert.Less((DateTime.Now.Date.AddDays(1) - candle.timeClose).TotalSeconds, 30,
                "CandlePackerPool.MakeCandles - должен закончиться на последней свече за день");

            // добавить левый тикер
            quoteNames = new[] { "EURUSD", "IDDQD" };
            candles = pool.MakeCandles(new[]
                    {
                        new QuoteData(1, 1.0001f, DateTime.Now),
                        new QuoteData(2, 2.0001f, DateTime.Now),
                    }, ref quoteNames);
            Assert.AreEqual(1, candles.Length, "CandlePackerPool.MakeCandles - должен пропустить левый тикер (свечу)");
            Assert.AreEqual(1, quoteNames.Length, "CandlePackerPool.MakeCandles - должен пропустить левый тикер");
            Assert.IsNotNull(candles[0], "CandlePackerPool.MakeCandles - оставшаяся свеча не должна пропасть");
        }
    }
}
