using System.Linq;
using NUnit.Framework;
using TradeSharp.Test.Moq;
using TradeSharp.UI.Util.Update;
using TradeSharp.Util;

namespace TradeSharp.Test.UI.Util
{
    [TestFixture]
    public class NuClientFileBrowser
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

        [Test]
        public void CompareWithServer()
        {
            var diffFileNames = new[] { "\\mt4\\udpcommandtranslator.mq4", "\\sounds\\error.wav" };

            var serverFiles = TestDataGenerator.GetServerFiles();
            var ownFiles = TestDataGenerator.GetOwnFiles();

            var diffs = ClientFileBrowser.CompareWithServer(serverFiles, ownFiles);

            Assert.AreEqual(diffFileNames.Length, diffs.Count, "Количество обновлённых файлов определено неверно");

            foreach (var diffFileName in diffs)
            {
                if (!diffFileNames.Contains(diffFileName.FileName))
                    Assert.Fail("не найден изменённый файл {0}", diffFileName);
            }
        }

        [Test]
        public void GetFileVersions()
        {
            var fileVersions = ClientFileBrowser.GetFileVersions(Logger.ErrorFormat);
            Assert.NotNull(fileVersions);
        }
    }
}
