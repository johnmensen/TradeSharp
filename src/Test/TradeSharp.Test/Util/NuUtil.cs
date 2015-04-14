using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Util
{
    [TestFixture]
    class NuUtil
    {
        [TestFixtureSetUp]
        public void Setup()
        {
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
        }

        /// <summary>
        /// Тестируется метод приведения строки к дате
        /// </summary>
        [Test]
        public void NuConvertStrToDateTime()
        {
            var unValidData1 = "16.01.2014".ToDateTimeUniformSafe();
            var unValidData3 = "16.01.14".ToDateTimeUniformSafe();
            var unValidData4 = "16.01.14 22:14:15".ToDateTimeUniformSafe();
            var unValidData5 = "16-01-2014".ToDateTimeUniformSafe();
            var unValidData6 = "16.01.2014 22-14-15".ToDateTimeUniformSafe();
            var unValidData7 = String.Empty.ToDateTimeUniformSafe();
            var unValidData8 = "".ToDateTimeUniformSafe();
            var unValidData9 = " ".ToDateTimeUniformSafe();
            var unValidData10 = "16.16.2014 22-14-15".ToDateTimeUniformSafe();
            var unValidData11 = "16.11.2014 26-14-15".ToDateTimeUniformSafe();

            Assert.IsNull(unValidData1);
            Assert.IsNull(unValidData3);
            Assert.IsNull(unValidData4);
            Assert.IsNull(unValidData5);
            Assert.IsNull(unValidData6);
            Assert.IsNull(unValidData7);
            Assert.IsNull(unValidData8);
            Assert.IsNull(unValidData9);
            Assert.IsNull(unValidData10);
            Assert.IsNull(unValidData11);

            Assert.IsNotNull("16.01.2014 22:14:15".ToDateTimeUniformSafe());
            Assert.IsNotNull("16.01.2014 22:14:15.561".ToDateTimeUniformSafeMils());


            var dateTimeFormat = new DateTimeFormatInfo { DateSeparator = ".", ShortDatePattern = "dd.MM.yyyy" };
            var validData1 = "16.01.2014".ToDateTimeUniformSafe(dateTimeFormat);
            var validData2 = "16.01.2014 22:14:15".ToDateTimeUniformSafe(dateTimeFormat);

            Assert.AreEqual(validData1, new DateTime(2014, 1, 16));
            Assert.AreEqual(validData2, new DateTime(2014, 1, 16, 22, 14, 15));
        }
    }
}
