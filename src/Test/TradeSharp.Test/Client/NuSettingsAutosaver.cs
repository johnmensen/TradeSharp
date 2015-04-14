using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Candlechart;
using Candlechart.Core;
using Candlechart.Indicator;
using Castle.Components.DictionaryAdapter;
using NUnit.Framework;
using TradeSharp.Client.BL;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuSettingsAutosaver
    {
        private string autosaveFolderPath = string.Empty;
        private string testFileFolderPath = string.Empty;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            var directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
            if (directoryName != null)
            {
                autosaveFolderPath = Path.Combine(directoryName, "autosave");
                testFileFolderPath = Path.Combine(directoryName.Replace("\\bin\\Debug", ""), "AnyFilesToTest\\autosave");             
            }
            Localizer.ResourceResolver = new MockResourceResolver();
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            Localizer.ResourceResolver = null;
        }

        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(autosaveFolderPath)) 
                Directory.Delete(autosaveFolderPath, true);
            Directory.CreateDirectory(autosaveFolderPath); 
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(autosaveFolderPath)) Directory.Delete(autosaveFolderPath, true);
        }

        [Test]
        public void AreSettingsUpdated()
        {
            var timeLastSaved = UserSettings.Instance.lastTimeModified.GetLastHitIfHitted() ?? DateTime.Now.AddDays(-1);
            var areUpdated = SettingsAutosaver.AreSettingsUpdated(timeLastSaved);
            Assert.IsFalse(areUpdated, "AreSettingsUpdated - обновления еще не было");
            
            UserSettings.Instance.WindowSize = new Size(10, 10);
            areUpdated = SettingsAutosaver.AreSettingsUpdated(timeLastSaved);
            Assert.IsTrue(areUpdated, "AreSettingsUpdated - обновление таки имело место");
            timeLastSaved = UserSettings.Instance.lastTimeModified.GetLastHit();

            var chart = new CandleChartControl();
            chart.ActivateIndiAddEvent(new FiboForkIndicator());
            areUpdated = SettingsAutosaver.AreSettingsUpdated(timeLastSaved);
            Assert.IsTrue(areUpdated, "AreSettingsUpdated - обновление имело место - были обновлены индюки");
        }

        [Test]
        public void PackAndSaveSettingsFiles()
        {
            const string zipName = "1111111111_1111111111.zip";

            // Временные файлы не генерируются на лету, а копируются
            File.Copy(Path.Combine(testFileFolderPath, "indicator.xml"), Path.Combine(autosaveFolderPath, "indicator.xml"));
            File.Copy(Path.Combine(testFileFolderPath, "objects.xml"), Path.Combine(autosaveFolderPath, "objects.xml"));
            File.Copy(Path.Combine(testFileFolderPath, "settings.xml"), Path.Combine(autosaveFolderPath, "settings.xml"));


            SettingsAutosaver.PackAndSaveSettingsFiles(autosaveFolderPath, Path.Combine(autosaveFolderPath, zipName), false);

            var residualFilesXml = Directory.GetFiles(autosaveFolderPath, "*.xml");
            Assert.AreEqual(3, residualFilesXml.Length);
            var residualFilesZip = Directory.GetFiles(autosaveFolderPath, "*.zip");
            Assert.AreEqual(1, residualFilesZip.Length);

            // Удаляем временные файлы
            SettingsAutosaver.DeleteTempSettingsFileInAutosaveFolder(autosaveFolderPath);

            // после всей процедуры в папке "autosave" должен появиться на 1 архив больше и не должно остаться xml файлов 
            residualFilesXml = Directory.GetFiles(autosaveFolderPath, "*.xml");
            Assert.AreEqual(0, residualFilesXml.Length);
            residualFilesZip = Directory.GetFiles(autosaveFolderPath, "*.zip");
            Assert.AreEqual(1, residualFilesZip.Length);
        }

        [Test]
        public void CleanupAutosaves()
        {
            var zipFileNames = new[] {"2703165731_2703170058.zip", "2703165731_2703170118.zip", "2703165731_2703170408.zip", "2703165731_2703170434.zip", "2703165731_2703170633.zip"};

            foreach (var zipFileName in zipFileNames)
                File.Copy(Path.Combine(testFileFolderPath, zipFileName), Path.Combine(autosaveFolderPath, zipFileName));

            SettingsAutosaver.CleanupAutosaves(autosaveFolderPath, 4);

            var residualFiles = Directory.GetFiles(autosaveFolderPath);
            Assert.AreEqual(4, residualFiles.Length);

            foreach (var residualFile in residualFiles)
                if (!zipFileNames.Skip(1).Contains(residualFile.Split('\\').Last()))
                    Assert.Fail("файл с именем {0} не должен был быть удалён", residualFile);
        }
    }
}
