using System;
using System.Collections.Generic;
using Entity;
using Moq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.QuoteHistory;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuGap
    {
        [TestFixtureSetUp]
        public void SetupMethods()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
        }

        [Test]
        public void WeekendIsNotGap()
        {
            // Выходные дни: май 2013, 4,5    9   11,12
            // выходной
            var gap = new DateSpan(new DateTime(2013, 5, 4, 3, 51, 12),
                                                       new DateTime(2013, 5, 5, 1, 19, 0));

            var miniHoles = DaysOff.Instance.GetIntersected(gap);
            var gaps = QuoteCacheManager.SubtractPeriods(gap, miniHoles);
            Assert.AreEqual(gaps.Count, 0);

            // праздник
            gap = new DateSpan(new DateTime(2013, 5, 9, 3, 21, 0),
                                                       new DateTime(2013, 5, 9, 11, 19, 0));
            miniHoles = DaysOff.Instance.GetIntersected(gap);
            gaps = QuoteCacheManager.SubtractPeriods(gap, miniHoles);
            Assert.AreEqual(gaps.Count, 0);

            // гэп присутствует
            gap = new DateSpan(new DateTime(2013, 5, 5, 16, 30, 0),
                                                       new DateTime(2013, 5, 6, 8, 30, 0));
            miniHoles = DaysOff.Instance.GetIntersected(gap);
            Assert.AreNotEqual(miniHoles.Count, 0, "DaysOff.Instance.GetIntersected");
            gaps = QuoteCacheManager.SubtractPeriods(gap, miniHoles);
            
            Assert.AreNotEqual(gaps.Count, 0, "gaps");
            // закомментировал, т.к. пересекается с другими тестами
            //moq.Verify(lw => lw.GetMetadataByCategory(It.Is<string>(s => s == "DayOff")), Times.Once(), 
            //    "выходные дни должны быть зачитаны из базы ровно один раз");
        }

        [Test]
        public void SmallGapIsStillAGap()
        {
            var gapInterval = new DateSpan(new DateTime(2013, 5, 31, 17, 49, 0), 
                new DateTime(2013, 5, 31, 17, 51, 16));
            var miniHoles = DaysOff.Instance.GetIntersected(gapInterval);
            var gaps = QuoteCacheManager.SubtractPeriods(gapInterval, miniHoles);

            Assert.AreNotEqual(gaps.Count, 0, "после склейки таки должны оставаться гэпы (SmallGapIsStillAGap)");
            // закомментировал, т.к. пересекается с другими тестами
            //moq.Verify(lw => lw.GetMetadataByCategory(It.Is<string>(s => s == "DayOff")), Times.Once(),
            //    "выходные дни должны быть зачитаны из базы ровно один раз");
        }

        [Test]
        public void CheckTimeIsDayOff()
        {
            var times = new Dictionary<DateTime, bool>
                {
                    {new DateTime(2013, 1, 1, 0, 30, 15), true},
                    {new DateTime(2013, 9, 21, 7, 30, 15), true},
                    {new DateTime(2013, 9, 30, 7, 30, 15), false},
                };

            foreach (var time in times)
            {
                var isOff = DaysOff.Instance.IsDayOff(time.Key);
                Assert.AreEqual(time.Value, isOff, string.Format("{0} {1} be day off",
                    time.Key.ToStringUniform(), time.Value ? "should" : "should not"));
            }
        }

        [Test]
        public void GapsMapsRecordsMerge()
        {
            GapMap.Instance.Clear();
            var serverGaps = new List<DateSpan>
                {
                    new DateSpan(new DateTime(2013, 9, 3, 11, 5, 0), new DateTime(2013, 9, 3, 11, 8, 0)),
                    new DateSpan(new DateTime(2013, 9, 10, 4, 51, 0), new DateTime(2013, 9, 10, 10, 24, 0)),
                    new DateSpan(new DateTime(2013, 9, 10, 10, 28, 0), new DateTime(2013, 9, 10, 10, 33, 0)),
                    new DateSpan(new DateTime(2013, 9, 11, 1, 1, 0), new DateTime(2013, 9, 11, 13, 18, 0))
                };
            var dateStart = serverGaps[0].start;
            
            var clientGaps = new List<DateSpan>
                {
                    new DateSpan(new DateTime(2013, 9, 10, 10, 28, 0), new DateTime(2013, 9, 10, 10, 33, 0)),
                    new DateSpan(new DateTime(2013, 9, 11, 1, 1, 0), new DateTime(2013, 9, 11, 13, 18, 0)),
                    new DateSpan(new DateTime(2013, 9, 17, 1, 1, 0), new DateTime(2013, 9, 17, 1, 8, 0)) // new
                };
            var cliStart = clientGaps[0].start;

            GapMap.Instance.UpdateGaps("EURUSD", /*dateStart, */serverGaps);
            GapMap.Instance.UpdateGaps("EURUSD", /*cliStart, */clientGaps);
            var gaps = GapMap.Instance.GetServerGaps("EURUSD");
            Assert.AreEqual(serverGaps.Count + 1, gaps.serverGaps.Count, "Gaps are merged");
        }
    }
}
