using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    class NuIniFile
    {
        private const string FileName = "test_file.ini";

        [Test]
        public void CreateAndReadSections()
        {
            var iniFile = new IniFile(FileName);
            var values = new Dictionary<string, string>();
            const string testSection = "TestSection";
            values.Add("param1", "text1");
            values.Add("param2", "text2");
            iniFile.WriteSection(testSection, values);
            iniFile.WriteValue(testSection, "param3", "text3");
            values.Add("param3", "text3");
            using (var fs = new StreamReader(FileName))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    Assert.IsTrue(!string.IsNullOrEmpty(line) && line == "[TestSection]", "Error creating [TestSection]");
                    line = fs.ReadLine();
                    Assert.IsTrue(!string.IsNullOrEmpty(line) && line == "param1=text1", "Error creating param1=text1");
                    line = fs.ReadLine();
                    Assert.IsTrue(!string.IsNullOrEmpty(line) && line == "param2=text2", "Error creating param2=text2");
                    line = fs.ReadLine();
                    Assert.IsTrue(!string.IsNullOrEmpty(line) && line == "param3=text3", "Error creating param3=text3");
                }
            }

            // reading
            iniFile = new IniFile(FileName);
            var readValues = iniFile.ReadSection(testSection);
            Assert.AreEqual(values.Count, readValues.Count, "Error reading [TestSection]: value count mismatch");
            if(values.Count == readValues.Count)
            {
                Assert.IsTrue(readValues.ContainsKey("param1"), "Error reading param1");
                Assert.AreEqual(values["param1"], readValues["param1"], "Error reading text1");
                Assert.IsTrue(readValues.ContainsKey("param2"), "Error reading param2");
                Assert.AreEqual(values["param2"], readValues["param2"], "Error reading text2");
                Assert.IsTrue(readValues.ContainsKey("param3"), "Error reading param3");
                Assert.AreEqual(values["param3"], readValues["param3"], "Error reading text3");
            }
        }

        [TestFixtureTearDown]
        public void StopAllTests()
        {
            File.Delete(FileName);
        }
    }
}
