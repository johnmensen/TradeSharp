using System.Globalization;
using NUnit.Framework;
using TradeSharp.Localisation;
using TradeSharp.Util;

namespace TradeSharp.Test.Localisation
{
    [TestFixture]
    public class NuLocalisationManager
    {
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

        //[Test]
        public void GetString()
        {
            var retry = LocalisationManager.Instance.GetString("Retry");
            Logger.Debug("Culture - " + CultureInfo.CurrentUICulture.Name);

            switch (CultureInfo.CurrentUICulture.Parent.Name)
            {
                case "en":
                    Assert.AreEqual("Retry", retry, "Не произошла локализация на английский (американский) язык");
                    break;
                default:
                    Assert.AreEqual("Повторить", retry, "Не произошла локализация на русский язык");
                    break;
            }

            var retry1 = LocalisationManager.Instance.GetString("Retry1");
            Assert.AreEqual("Retry1", retry1, "Ошибка локализации отсутствующей строки");
        }
    }
}
