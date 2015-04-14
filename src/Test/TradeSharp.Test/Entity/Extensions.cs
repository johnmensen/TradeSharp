using System;
using TradeSharp.Util;
using NUnit.Framework;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuExtensions
    {
        private readonly double[] coll = new double[] // len = 10
                {
                    2, 4, 6, 2, 0, 2, 1, 1, 8, 2
                };

        [Test]
        public void TestResizeResample()
        {
            var expected = new double[]
                {
                    3, 4, 1, 1, 5
                };

            var resulted = coll.ResizeAndResample(coll.Length, coll.Length / 2);
            Assert.AreEqual(expected.Length, resulted.Count, "resulted is of the right size");
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.LessOrEqual(Math.Abs(expected[i] - resulted[i]), 0.00001, "counted OK");
            }

            var len = coll.Length/3;
            resulted = coll.ResizeAndResample(coll.Length, len);
            Assert.AreEqual(len, resulted.Count, "shrunk coll has the right size");
        }

        [Test]
        public void TestResizeEnlarge()
        {
            var len = coll.Length * 2;
            var resulted = coll.ResizeAndResample(coll.Length, coll.Length * 2);
            Assert.AreEqual(len, resulted.Count, "enlarged is of the right size");
            for (var i = 0; i < coll.Length; i++)
            {
                Assert.LessOrEqual(Math.Abs(coll[i] - resulted[i]), 0.00001, "enlarged counted OK");
            }
            for (var i = coll.Length; i < len; i++)
            {
                Assert.LessOrEqual(Math.Abs(coll[coll.Length - 1] - resulted[i]), 0.00001, "enlarged tail is OK");
            }
        }
    }
}
