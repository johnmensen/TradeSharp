using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Localisation
{
    #if CI
    #else
    [TestFixture]
    #endif
    public class NuCheckAllResourcesExist
    {
        /// <summary>
        /// Имена ресурстный файлов, которые не должны проверяться в тесте
        /// </summary>
        public string[] excludedResourcesFiles = new[] { "Resources.ru-RU.resx" };

        private readonly string[] exceptionString = new[]
            {
                "40", "50", "60"
            };

        private string codeBase;
        private ResourcesExistChecker checker;

        [TestFixtureSetUp]
        public void Setup()
        {
            codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
            codeBase = Path.GetDirectoryName(codeBase);
            codeBase = codeBase.Replace(@"\Test\TradeSharp.Test\bin\Debug", ""); //TODO если "\bin\Debug" измениться на "\bin\Release", то всё перестанет работать

            checker = new ResourcesExistChecker(codeBase);
        }

        [Test]
        public void CheckStringsDeclared()
        {
            var names = new List<string>();
            checker.GetAllResourceReferences(names);
            Assert.Greater(names.Count, 600, "Должно быть много ресурсных строчек");
            // убрать исключения и повторы
            names = names.Distinct().Where(n => !exceptionString.Contains(n)).ToList();

            // проверить, есть ли они в файлах ресурсов
            var errorsInFile = new Dictionary<string, List<string>>();
            var resxFileNames = Directory.GetFiles(codeBase + @"\TradeSharp.Localisation\Properties", "*.resx");
            
            foreach (var resxFileName in resxFileNames)
            {
                if (excludedResourcesFiles.Contains(resxFileName.Split('\\').Last())) continue;

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
