using System;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using System.Drawing;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuConverter
    {
        [Test]
        public void TestConvertFromString()
        {
            Assert.AreEqual(-10002, Converter.GetObjectFromString("int", "-10002"));
            Assert.AreEqual(new Size(100, 200), Converter.GetObjectFromString<Size>("100;200"));
        }

        [Test]
        public void TestConvertBothSides()
        {
            Assert.AreEqual(-555, Converter.GetObjectFromString<int>(Converter.GetStringFromObject(-555)));
            Assert.AreEqual(20f, Converter.GetObjectFromString<float>(Converter.GetStringFromObject(20f)));
            Assert.AreEqual(false, Converter.GetObjectFromString<bool>(Converter.GetStringFromObject(false)));
            Assert.AreEqual(101.1M, Converter.GetObjectFromString<decimal>(Converter.GetStringFromObject(101.1M)));
            Assert.AreEqual(101.1M, Converter.GetObjectFromString(Converter.GetStringFromObject(101.1M), typeof(decimal)));
            Assert.AreEqual(new DateTime(2010, 11, 11, 23, 11, 36),
                Converter.GetObjectFromString<DateTime>(Converter.GetStringFromObject(new DateTime(2010, 11, 11, 23, 11, 36))));
            Assert.AreEqual(new Size(100, 200), Converter.GetObjectFromString<Size>(Converter.GetStringFromObject(new Size(100, 200))));

            var o = new MarketOrder();
            var typeOrder = typeof (MarketOrder);
            var prop = typeOrder.GetProperty("Magic");
            prop.SetValue(o, Converter.GetNullableObjectFromString("12", prop.PropertyType), new object[0]);
            Assert.AreEqual(12, o.Magic);
            prop.SetValue(o, Converter.GetNullableObjectFromString("", prop.PropertyType), new object[0]);
            Assert.AreEqual(null, o.Magic);
        }
    }
}
