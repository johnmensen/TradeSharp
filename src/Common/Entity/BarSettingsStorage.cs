using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TradeSharp.Util;

namespace Entity
{
    public class BarSettingsStorage
    {
        private const string BarSettingsFileName = "BarSettings.xml";

        #region Singletone

        private static readonly Lazy<BarSettingsStorage> instance =
            new Lazy<BarSettingsStorage>(() => new BarSettingsStorage());

        public static BarSettingsStorage Instance
        {
            get { return instance.Value; }
        }

        #endregion

        private BarSettingsStorage()
        {
            InitDefaultValues();
            ReadSeriesFromXml();
        }

        private readonly List <BarSettings> barSettingsSeries = new List <BarSettings>();

        public List<BarSettings> GetCollection()
        {
            return barSettingsSeries;
        }

        #region Save
        public void SaveSeriesSettings()
        {
            var pathFile = string.Format("{0}\\{1}", ExecutablePath.ExecPath, BarSettingsFileName);

            var doc = new XmlDocument();
            SaveBarSettings(doc);
            // сохранить
            doc.Save(pathFile);
        }

        private void SaveBarSettings(XmlDocument doc)
        {
            var xmlNode = doc.AppendChild(doc.CreateElement("BarSettingsList"));
            foreach (var item in barSettingsSeries.Where(b => b.IsUserDefined))
            {
                var child = xmlNode.AppendChild(doc.CreateElement("BarSettings"));
                var titleAttr = child.Attributes.Append(doc.CreateAttribute("Title"));
                titleAttr.Value = item.Title;
                child.InnerText = item.ToString();
            }
        }
        #endregion

        public void ReadSeriesFromXml()
        {
            var pathFile = string.Format("{0}\\{1}", ExecutablePath.ExecPath, BarSettingsFileName);
            var doc = new XmlDocument();
            try
            {
                if (!File.Exists(pathFile))
                    return;
                doc.Load(pathFile);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка загрузки данных BarSettings \"{0}\": {1}", pathFile, ex);
            }
            try
            {
                // разбираем xml
                var node = doc.SelectSingleNode("BarSettingsList");
                if (node != null)
                {
                    var nodes = node.SelectNodes("BarSettings");

                    if (nodes != null)
                        foreach (XmlElement item in nodes)
                        {
                            var bars = new BarSettings {Title = item.Attributes["Title"].Value, IsUserDefined = true};
                            try
                            {
                                bars.ReadFromTagString(item.InnerText, ";#");
                            }
                            catch
                            {
                                continue;
                            }
                            
                            if (!barSettingsSeries.Any(s => s.Title == bars.Title))
                                barSettingsSeries.Add(bars);
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BarSettings.ReadSeriesFromXml error", ex);
            }
        }

        private void InitDefaultValues()
        {
            // предопределенные
            barSettingsSeries.Clear();
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 5 }, Title = "M5" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 15 }, Title = "M15" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 30 }, Title = "M30" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 60 }, Title = "H1"});
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 240 }, Title = "H4" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 360 }, Title = "H6" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 1440 }, Title = "D1" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 1440*5 }, Title = "W1" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 120 }, Title = "H2" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 180 }, Title = "H3" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 540 }, Title = "H9" });
            barSettingsSeries.Add(new BarSettings { Intervals = new List<int> { 720 }, Title = "H12" });
        }

        /// <summary>
        /// на входе строка вида M5, W1... либо 0;#1440;#
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BarSettings GetBarSettingsByName(string name)
        {
            var knownSettings = barSettingsSeries.FirstOrDefault(bs => bs.Title.Equals(name,
                StringComparison.OrdinalIgnoreCase));
            if (knownSettings != null) return knownSettings;
            try
            {
                return new BarSettings(name);
            }
            catch
            {
                return null;
            }
        }

        public string GetBarSettingsFriendlyName(BarSettings sets)
        {
            var exists = barSettingsSeries.FirstOrDefault(bs => bs == sets);
            return !ReferenceEquals(exists, null) ? exists.Title : sets.ToString();
        }

        public string GetBarSettingsFriendlyName(string timeframeString)
        {
            var sets = BarSettings.TryParseString(timeframeString);
            if (ReferenceEquals(sets, null)) return timeframeString;
            var exists = barSettingsSeries.FirstOrDefault(bs => bs.Equals(sets));
            return !ReferenceEquals(exists, null) ? exists.Title : sets.ToString();
        }

        public void RemoveBarSettingsByName(string name)
        {
            var knownSettings = barSettingsSeries.FirstOrDefault(bs => bs.Title.Equals(name,
                StringComparison.OrdinalIgnoreCase));
            if (knownSettings == null)
                return;
            barSettingsSeries.Remove(knownSettings);
        }
    }
}
