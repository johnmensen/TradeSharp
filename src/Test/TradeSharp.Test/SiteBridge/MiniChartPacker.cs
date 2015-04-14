using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteBridge.Lib.Contract;

namespace TradeSharp.Test.SiteBridge
{
    [TestFixture]
    public class NuMiniChartPacker
    {
        [Test]
        public void TestPackUnpack()
        {
            // подготовить липу
            var lstEq = new List<EquityOnTime>();
            var timeStart = DateTime.Now.Date.AddDays(-190);
            var timeEnd = DateTime.Now.Date;

            var i = 0;
            for (var date = timeStart; date < timeEnd; date = date.AddDays(1))
            {
                var y = 1.5 + Math.Sin(i++/20.0);
                lstEq.Add(new EquityOnTime((float)y, date));
            }

            // упаковать
            var chartMini = MiniChartPacker.PackChartInArray(lstEq);
            Assert.Greater(chartMini.Length, 0, "Chart is not empty");
            var min = chartMini.Min();
            var max = chartMini.Max();
            Assert.Greater(max - min, 250, "Chart range is close to 255");

            // распаковать
            var points = MiniChartPacker.MakePolygon(chartMini, 60, 30, 2, 2);
            Assert.AreEqual(chartMini.Length + 2, points.Length, "unpacked has the same size");            
        }
    }
}
