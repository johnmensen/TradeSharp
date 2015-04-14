using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Util
{
    [TestFixture]
    public class NuUiFieldAttributeTest
    {
        enum SomeEnum { One = 1, Two = 2}

        [UiField]
        public int SomeField { get; set; }

        [UiField(SomeEnum.One)]
        public string AnotherField { get; set; }

        [UiField("Some")]
        public string MoreField { get; set; }

        [Test]
        public void TestAttributeItself()
        {
            var attrs = typeof (NuUiFieldAttributeTest).GetProperties().Select(pro => 
                (UiFieldAttribute) pro.GetCustomAttributes(typeof (UiFieldAttribute), false).First()).ToList();
            Assert.IsNull(attrs[0].Category);
            Assert.AreEqual((int)SomeEnum.One, attrs[1].Category.Value);
        }

        [Test]
        public void TestPropertyDic()
        {
            var sets = new AutoTradeSettings
                {
                    MaxLeverage = 6,
                    TradeAuto = true
                };
            var nameVal = UiFieldAttribute.GetAttributeNameValue(sets);
            Assert.Greater(nameVal.Count, 0);

            var cat = new AutoTradeSettings();
            UiFieldAttribute.SetFieldsFromPropNameValue(
                nameVal.ToDictionary(p => p.PropName, p => Converter.GetStringFromObject(p.Value)), cat, false);

            Assert.AreEqual(sets.MaxLeverage, cat.MaxLeverage);
            Assert.AreEqual(sets.TradeAuto, cat.TradeAuto);
        }
    }
}
