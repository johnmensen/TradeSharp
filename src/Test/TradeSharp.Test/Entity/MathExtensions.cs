using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuMathExtensions
    {
        [Test]
        public void TestBetween()
        {
            Assert.IsTrue(5f.Between(4f, 6f), "5 btw 4, 6");
            Assert.IsTrue(1.Between(1, 1), "5 btw 4, 6");
            Assert.IsFalse((-5f).Between(-4f, 6f), "-5 not btw -4, 6");
            Assert.IsTrue(5f.Between(4, 6), "5.0 btw 4, 6");
        }

        [Test]
        public void HasSameContentsAs()
        {
            var listA = new List<string> { "a", "z", "alpha" };
            var listB = new List<string> { "alpha", "a", "z",  };
            Assert.IsTrue(listA.HasSameContentsAs(listB), "same content, diff order");
            listA.RemoveAt(0);            
            Assert.IsFalse(listA.HasSameContentsAs(listB), "not the same content");
            listA.Add("b");
            Assert.IsFalse(listA.HasSameContentsAs(listB), "not the same content");
        }

        [Test]
        public void IndexOfMin()
        {
            var numbers = new List<Point>
                {
                    new Point(0, 1),    // 0
                    new Point(0, -1),   // 1
                    new Point(0, 2),    // 2
                    new Point(0, 3),    // 3
                    new Point(0, -2),   // 4
                    new Point(0, 4),    // 5
                };
            Assert.IsTrue(numbers.IndexOfMin(p => p.Y) == 4);
            numbers.Add(new Point(0, -2));
            var minIndex = numbers.IndexOfMin(p => p.Y);
            Assert.IsTrue(minIndex == 4 || minIndex == 6);
            numbers.Clear();
            numbers.IndexOfMin(p => p.Y);
        }

        [Test]
        public void FirstOrNull()
        {
            var dates = new[]
                {
                    new DateTime(2001, 9, 11, 14, 55, 20),
                    new DateTime(2002, 9, 11, 13, 20, 18),
                    new DateTime(2003, 9, 12, 16, 55, 20)
                };
            Assert.IsNotNull(dates.FirstOrNull(d => d.Year == 2002), "FirstOrNull should not be null");
            Assert.IsNull(dates.FirstOrNull(d => d.Year > 2104), "FirstOrNull should be null");
        }

        [Test]
        public void TestIndexOfMax()
        {
            var transfers = new []
                {
                    new Transfer {Id = 1, ValueDate = new DateTime(2014, 2, 9, 16, 11, 13)},
                    new Transfer {Id = 2, ValueDate = new DateTime(2014, 2, 9, 16, 11, 14)},
                    new Transfer {Id = 3, ValueDate = new DateTime(2014, 2, 9, 16, 11, 19)},
                    new Transfer {Id = 4, ValueDate = new DateTime(2014, 2, 9, 16, 11, 18)},
                };
            Assert.AreEqual(3, transfers[transfers.IndexOfMax(t => t.ValueDate)].Id);
        }

    }
}
