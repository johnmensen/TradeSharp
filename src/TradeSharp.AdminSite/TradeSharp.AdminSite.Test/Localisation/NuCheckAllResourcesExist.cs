using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.AdminSite.Test.Localisation
{
    #if CI
    #else
    [TestFixture]
    #endif
    public class NuCheckAllResourcesExist
    {
        private string codeBase;
        private ResourcesExistChecker checker;

        [TestFixtureSetUp]
        public void Setup()
        {
            codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
            codeBase = Path.GetDirectoryName(codeBase);
            codeBase = codeBase.Replace(@"\TradeSharp.AdminSite.Test\bin\Debug", "") + "\\"; //TODO если "\bin\Debug" измениться на "\bin\Release", то всё перестанет работать

            checker = new ResourcesExistChecker(codeBase);
            ResourcesExistChecker.RegUserCode = new[]
                {
                    new Regex("(?<=Resource\\.)[a-zA-Z][a-zA-Z0-9]+"),
                    new Regex("(?<=\\[Localized\\D+\\(\")[^\"]+(?=\"\\)\\])"),
                    new Regex("(?<=ErrorMessageResourceName[\\s]*=[\\s]*\")[a-zA-Z][a-zA-Z0-9/_]+")
                };
        }

        [Test]
        public void CheckStringsDeclared()
        {
            var names = new List<string>();
            checker.GetAllResourceReferences(names);
            Assert.Greater(names.Count, 400, "Должно быть много ресурсных строчек");

            // убрать исключения и повторы
            names = names.Distinct().ToList();
            names.Remove("ResourceManager");

            // проверить, есть ли они в файлах ресурсов
            var errorsInFile = new Dictionary<string, List<string>>();
            var resxFileNames = Directory.GetFiles(codeBase + @"\TradeSharp.AdminSite\App_GlobalResources", "*.resx");

            foreach (var resxFileName in resxFileNames)
            {
                var errors = checker.GetNamesLackInResxFile(resxFileName, names);
                if (errors.Count > 0)
                    errorsInFile.Add(Path.GetFileName(resxFileName), errors);
            }

            if (errorsInFile.Count > 0)
            {
                var errorStr = string.Join("\n", errorsInFile.Select(e => e.Key + ": " + string.Join(", ", e.Value)));
                Assert.Fail(errorStr);
            }
        }
    }
}
