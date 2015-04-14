using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml;
using Candlechart.Indicator;
using NUnit.Framework;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    class BaseChartIndicatorTest
    {
        private readonly List<IChartIndicator> indicators = new List<IChartIndicator>();

        [TestFixtureSetUp]
        public void CreateIndicators()
        {
            var indi1 = new IndicatorRSI();
            Type indiType = indi1.GetType();
            indiType.GetProperty("Period").SetValue(indi1, 7);
            indiType.GetProperty("MarginPercent").SetValue(indi1, 10);
            indiType.GetProperty("LineColor").SetValue(indi1, Color.Yellow);
            indicators.Add(indi1);

            var indi2 = new IndicatorDiver();
            indiType = indi2.GetType();
            indiType.GetProperty("IndicatorDrawStyle").SetValue(indi2, IndicatorDiver.DrawStyle.Стрелки);
            indiType.GetProperty("DiverType").SetValue(indi2, IndicatorDiver.DivergenceType.Классические);
            indiType.GetProperty("PeriodExtremum").SetValue(indi2, 12);
            indicators.Add(indi2);
        }

        [Test]
        public void TestIndicators()
        {
            var doc = new XmlDocument();
            foreach (var indicator in indicators)
            {
                var node = doc.CreateElement("indicator");
                BaseChartIndicator.MakeIndicatorXMLNode(indicator, node);
                var loadedIndicator = BaseChartIndicator.LoadIndicator(node);
                Assert.IsTrue(CopmareIndicators(indicator, loadedIndicator));
            }
        }

        private bool CopmareIndicators(IChartIndicator indi1, IChartIndicator indi2)
        {
            if (indi1 == null || indi2 == null)
                return false;
            if (indi1.GetType() != indi2.GetType())
                return false;
            var equal = true;
            foreach (var property in indi1.GetType().GetProperties())
            {
                if (!property.GetCustomAttributes(true).Any(a => a is DisplayNameAttribute))
                    continue;
                if (!property.PropertyType.IsValueType)
                    continue;
                if (property.PropertyType == typeof (Color))
                {
                    var c1 = (Color) property.GetValue(indi1);
                    var c2 = (Color) property.GetValue(indi2);
                    if (c1.ToArgb() != c2.ToArgb())
                    {
                        equal = false;
                        break;
                    }
                }
                else if (!property.GetValue(indi1).Equals(property.GetValue(indi2)))
                {
                    equal = false;
                    break;
                }
            }
           return equal;
        }
    }
}
