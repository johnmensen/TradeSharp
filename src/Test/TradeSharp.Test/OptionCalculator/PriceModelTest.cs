using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;

namespace TradeSharp.Test.OptionCalculator
{
    [TestFixture]
    public class PriceModelTest
    {
        private readonly Random rand = new Random();

        [Test]
        public void TestDeltaGeneration()
        {
            var candles = new List<CandleData>();
            for (var i = 0; i < 10000; i++)
            {
                var delta = Gaussian.BoxMuller(0, 1);
                candles.Add(new CandleData(100, 100,
                    99, 100 + (float)delta, new DateTime(), new DateTime()));
            }
            // небольшой процент больших смещений
            for (var i = 0; i < 10; i++)
            {
                var delta = 5 * (i % 2 == 0 ? 1 : -1);
                candles.Add(new CandleData(100, 100,
                    99, 100 + 1f * (float)delta, new DateTime(), new DateTime()));
            }
            var listOc = candles.Select(c => (double)(c.close - c.open)).OrderBy(c => c).ToList();
            var model = new PriceModel(listOc);

            const int checksCount = 5000;
            var deltas = new double[checksCount];
            for (var i = 0; i < checksCount; i++)
                deltas[i] = model.GetRandomDelta();

            var meanDelta = deltas.Average();
            var maxDelta = deltas.Max(d => Math.Abs(d));
            var vl = deltas.Sum(d => (d - meanDelta)*(d - meanDelta));
            vl = Math.Sqrt(vl / checksCount);

            Assert.Less(Math.Abs(meanDelta), 0.3, "Mean value should be around 0");
            Assert.Less(Math.Abs(vl - 1), 0.3, "AD should be around 1");
            Assert.Greater(maxDelta, 1, "max delta should be greater than 1");
        }
    }
}
