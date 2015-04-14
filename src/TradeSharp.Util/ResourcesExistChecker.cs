using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace TradeSharp.Util
{
    public class ResourcesExistChecker
    {
        private static Regex[] regUserCode = new[]
            {
                new Regex("(?<=Localizer.GetString\\(\")[^\"]+(?=\"\\))"),
                new Regex("(?<=\\[Localized\\D+\\(\")[^\"]+(?=\"\\)\\]$)")
            };

        public static Regex[] RegUserCode
        {
            get { return regUserCode; }
            set { regUserCode = value; }
        }

        private static readonly Regex[] regDesignerCode = new[]
            {
                new Regex("(?<=.Tag = \")[^\"]+(?=\")")
            };

        private string codeBase;

        public ResourcesExistChecker(string codeBase)
        {
            this.codeBase = codeBase;
        }

        /// <summary>
        /// Получить имена ресурсных файлов
        /// </summary>
        public List<string> GetNamesLackInResxFile(string fileName, List<string> names)
        {
            var docOrig = new XmlDocument();
            docOrig.Load(new StreamReader(fileName, Encoding.UTF8));

            var namesInFile =
                docOrig.GetElementsByTagName("data").Cast<XmlElement>().Select(n => n.Attributes["name"].Value).ToList();
            var lackNames = names.Where(n => !namesInFile.Contains(n)).ToList();
            return lackNames;
        }

        /// <summary>
        /// получить имена всех ресурсов, которые использовались в коде всего решения
        /// </summary>
        public void GetAllResourceReferences(List<string> names, string path = "")
        {
            if (string.IsNullOrEmpty(path))path = codeBase;

            foreach (var file in Directory.GetFiles(path, "*.cs"))
                PopulateResourceRefsFromFile(file, names);
            foreach (var dir in Directory.GetDirectories(path))
                GetAllResourceReferences(names, dir);
        }

        private void PopulateResourceRefsFromFile(string fileName, List<string> names)
        {
            var regex = regUserCode;
            if (fileName.EndsWith(".Designer.cs"))
                regex = regDesignerCode;

            using (var sr = new StreamReader(fileName, Encoding.UTF8))
            {
                var fileStr = sr.ReadToEnd();
                foreach (var reg in regex)
                {
                    foreach (Match match in reg.Matches(fileStr))
                        names.Add(match.Value);
                }
            }
        }
    }
}