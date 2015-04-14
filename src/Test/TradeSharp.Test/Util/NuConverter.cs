using System;
using System.Drawing;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Util
{
    [TestFixture]
    class NuConverter
    {
        int object1 = 1;
        int? object2 = 1;
        PositionExitReason object3 = PositionExitReason.Closed;
        MarketOrder object4 = new MarketOrder();
        int? object5 = null;
        MarketOrder object6 = null;

        [TestFixtureSetUp]
        public void TestSetup()
        {

        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
        }

        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void Teardown()
        {

        }

        [Test]
        public void TestConvertFromString()
        {
            Assert.AreEqual(-10002, Converter.GetObjectFromString("int", "-10002"));
            Assert.AreEqual(new Size(100, 200), Converter.GetObjectFromString<Size>("100;200"));

            Assert.AreEqual("-10002", Converter.GetObjectFromString("ushort", "-10002"));
            Assert.AreEqual("-10002", Converter.GetObjectFromString(null, "-10002"));
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


            Assert.AreEqual(101, Converter.GetObjectFromString(Converter.GetStringFromObject(101), typeof(short)));
            Assert.AreEqual(101, Converter.GetObjectFromString(Converter.GetStringFromObject(101), typeof(long)));
            Assert.AreEqual(101, Converter.GetObjectFromString(Converter.GetStringFromObject(101), typeof(ulong)));
            Assert.AreEqual(null, Converter.GetObjectFromString(Converter.GetStringFromObject(101), typeof(ushort)));
            Assert.AreEqual("1", Converter.GetStringFromObject((byte)1)); // для этого byte не должно быть в typeConvertor

            Assert.AreEqual(null, Converter.GetObjectFromString(null, typeof(int)));
            Assert.AreEqual(null, Converter.GetObjectFromString(null, typeof(ushort)));
            
            Assert.AreEqual("test", Converter.GetObjectFromString<string>("test"));
            Assert.AreEqual(100, Converter.GetObjectFromString<uint>("100"));
            Assert.AreEqual(0, Converter.GetObjectFromString<byte>("1")); // для этого должно выполняться услование : byte присутствует в "typeName", но отсутствует в "typeConvertor"
            Assert.AreEqual(default(ushort), Converter.GetObjectFromString<ushort>("100"));
            Assert.AreEqual(default(short), Converter.GetObjectFromString<short>(null));
      
            var o = new MarketOrder();
            var typeOrder = typeof(MarketOrder);
            var prop = typeOrder.GetProperty("Magic");
            prop.SetValue(o, Converter.GetNullableObjectFromString("12", prop.PropertyType), new object[0]);
            Assert.AreEqual(12, o.Magic);
            prop.SetValue(o, Converter.GetNullableObjectFromString("", prop.PropertyType), new object[0]);
            Assert.AreEqual(null, o.Magic);


            Assert.AreEqual("test", Converter.GetNullableObjectFromString("test", typeof(string)));
            Assert.AreEqual(1, Converter.GetNullableObjectFromString("1", typeof(int)));
        }

        [Test]
        public void NuIsNullableClass()
        {
            var type1 = Converter.IsNullable(typeof(int));
            var type2 = Converter.IsNullable(typeof(int?));
            var type3 = Converter.IsNullable(typeof(PositionExitReason));
            var type4 = Converter.IsNullable(typeof(MarketOrder));

            Assert.IsFalse(type1);
            Assert.IsTrue(type2);
            Assert.IsFalse(type3);
            Assert.IsTrue(type4);

            // Это принципиально для некоторых методов сайта администратора
            if (typeof(int).IsValueType && Nullable.GetUnderlyingType(typeof(int)) == null)
            {
                var res = Converter.IsNullable(typeof(int));
                Assert.IsFalse(res);
            }

            if (typeof(PositionExitReason).IsValueType && Nullable.GetUnderlyingType(typeof(PositionExitReason)) == null)
            {
                var res = Converter.IsNullable(typeof(PositionExitReason));
                Assert.IsFalse(res);
            }
        }

        [Test]
        public void NuIsNullableObject()
        {
            var res1 = Converter.IsNullable(object1);
            var res2 = Converter.IsNullable(object2);
            var res3 = Converter.IsNullable(object3);
            var res4 = Converter.IsNullable(object4);
            var res5 = Converter.IsNullable(object5);
            var res6 = Converter.IsNullable(object6);

            Assert.IsFalse(res1);
            Assert.IsTrue(res2);
            Assert.IsFalse(res3);
            Assert.IsTrue(res4);
            Assert.IsTrue(res5);
            Assert.IsTrue(res6);
        }

        [Test]
        public void NuSimpleMethod()
        {
            var res1 = Converter.GetObjectTypeName(object1);
            var res2 = Converter.GetObjectTypeName(object2);
            var res3 = Converter.GetObjectTypeName(object3);
            var res4 = Converter.GetObjectTypeName(object4);

            Assert.AreEqual("int", res1);
            Assert.AreEqual("int", res2);
            Assert.AreEqual("PositionExitReason", res3);
            Assert.AreEqual("MarketOrder", res4);

            try
            {
                Converter.GetObjectTypeName(object5);
            }
            catch (NullReferenceException)
            {

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            try
            {
                Converter.GetObjectTypeName(object6);
            }
            catch (NullReferenceException)
            {
                
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            var types = Converter.GetSupportedTypes();
            Assert.IsNotNull(types);

            var typeNames = Converter.GetSupportedTypeNames();
            Assert.IsNotNull(typeNames);


            var isConv1 = Converter.IsConvertable(typeof (long));
            Assert.IsTrue(isConv1);

            var isConv2 = Converter.IsConvertable(typeof(ushort));
            Assert.IsFalse(isConv2);
        }
    }
}
