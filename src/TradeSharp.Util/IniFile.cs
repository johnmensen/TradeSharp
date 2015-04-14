using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeSharp.Util
{
    public class IniFile
    {
        private static readonly char[] TrimSpace = new[] { ' ', (char)9 };

        public string fileName;

        public Encoding encoding = Encoding.UTF8;

        public IniFile(string fileName)
        {
            this.fileName = fileName;
        }

        public IniFile(string fileName, Encoding encoding)
        {
            this.fileName = fileName;
            this.encoding = encoding;
        }

        public string ReadValue(string section, string paramName, string defaultValue)
        {
            if (!File.Exists(fileName)) return defaultValue;

            try
            {
                using (var fs = new StreamReader(fileName, encoding))
                {
                    var curSection = string.Empty;
                    while (!fs.EndOfStream)
                    {
                        var line = fs.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        line = line.Trim(TrimSpace);
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.Length < 3) continue;

                        if (line[0] == '[' && line[line.Length - 1] == ']')
                        {
                            curSection = line.Substring(1, line.Length - 2);
                            continue;
                        }

                        if (curSection != section) continue;
                        var delimIndex = line.IndexOf('=');
                        if (delimIndex <= 0) continue;
                        var ptrName = line.Substring(0, delimIndex);
                        if (ptrName != paramName) continue;
                        var ptrValue = line.Substring(delimIndex + 1, line.Length - delimIndex - 1);
                        return ptrValue;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool WriteValue(string section, string paramName, string paramValue)
        {
            var sections = ReadFile();
            Dictionary<string, string> sectionDic;
            if (sections.ContainsKey(section))
                sectionDic = sections[section];
            else
            {
                sectionDic = new Dictionary<string, string>();
                sections.Add(section, sectionDic);
            }
            if (sectionDic.ContainsKey(paramName))
                sectionDic[paramName] = paramValue;
            else
                sectionDic.Add(paramName, paramValue);
            return WriteFile(sections);
        }

        public Dictionary<string, string> ReadSection(string section)
        {
            var sections = ReadFile();
            Dictionary<string, string> sect;
            if (sections.TryGetValue(section, out sect)) return sect;
            return new Dictionary<string, string>();
        }

        public bool WriteSection(string section, Dictionary<string, string> values)
        {
            var sections = ReadFile();
            if (sections.ContainsKey(section)) sections[section] = values;
            else
                sections.Add(section, values);
            return WriteFile(sections);
        }

        private Dictionary<string, Dictionary<string, string>> ReadFile()
        {
            var sections = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                using (var fs = new StreamReader(fileName, encoding))
                {
                    var curSection = string.Empty;
                    while (!fs.EndOfStream)
                    {
                        var line = fs.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        line = line.Trim(TrimSpace);
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.Length < 3) continue;

                        if (line[0] == '[' && line[line.Length - 1] == ']')
                        {
                            curSection = line.Substring(1, line.Length - 2);
                            continue;
                        }

                        var delimIndex = line.IndexOf('=');
                        if (delimIndex <= 0) continue;
                        var ptrName = line.Substring(0, delimIndex);
                        var ptrValue = line.Substring(delimIndex + 1, line.Length - delimIndex - 1);

                        Dictionary<string, string> curSectionDic;
                        if (!sections.ContainsKey(curSection))
                        {
                            curSectionDic = new Dictionary<string, string> { { ptrName, ptrValue } };
                            sections.Add(curSection, curSectionDic);
                        }
                        else
                        {
                            curSectionDic = sections[curSection];
                            if (!curSectionDic.ContainsKey(ptrName))
                                curSectionDic.Add(ptrName, ptrValue);
                        }
                    }
                }
            }
            catch
            {
                return sections;
            }
            return sections;
        }

        private bool WriteFile(Dictionary<string, Dictionary<string, string>> sections)
        {
            try
            {
                using (var fs = new StreamWriter(fileName, false, encoding))
                {
                    foreach (var dic in sections)
                    {
                        fs.WriteLine(string.Format("[{0}]", dic.Key));
                        foreach (var pair in dic.Value)
                        {
                            fs.WriteLine(string.Format("{0}={1}", pair.Key, pair.Value));
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
