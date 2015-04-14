using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuCopyValueAttribute
    {
        class TestClass
        {
            [CopyValue]
            public int X { get; set; }

            [CopyValue]
            public double Y { get; set; }

            public string Text { get; set; }

            [CopyValue]
            public string Code { get; set; }
        }

        [Test]
        public void TestCopy()
        {
            var a = new TestClass
                {
                    X = 100,
                    Y = 3.141592654,
                    Text = "Some",
                    Code = "CLT"
                };

            var copier = CopyValueAttribute.MakeCopyValuesRoutine(typeof (TestClass));
            var b = (TestClass)copier(a);

            Assert.IsNotNull(b, "TestCopy - not null");
            Assert.AreEqual(a.X, b.X, "TestCopy - int OK");
            Assert.AreEqual(a.Y, b.Y, "TestCopy - double OK");
            Assert.AreEqual(a.Code, b.Code, "TestCopy - string OK");

            a = new TestClass();
            b = (TestClass)copier(a);
            Assert.IsNotNull(b, "TestCopy - empty but not null");
        }
    }
}
