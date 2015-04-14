using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// "карта" новостей на сайте - показывает, сколько новостей хранится на каждую дату
    /// </summary>
    [Serializable]
    public class NewsMap
    {
        public int[] channelIds;
        
        public NewsMapRecord[] records;

        public NewsMap MakeMapOfLackedNews(NewsMap serverMap)
        {
            var resulted = new NewsMap
                {
                    channelIds = serverMap.channelIds.Where(channelId => !channelIds.Any(id => id == channelId)).ToArray(),
                    records = (from record in serverMap.records let existRec = records.FirstOrDefault(r => r.date == record.date) 
                               where existRec.recordsCount < record.recordsCount select record).ToArray()
                };
            return resulted;
        }

        public void SaveInFile(string path)
        {
            var doc = new XmlDocument();
            var node = doc.AppendChild(doc.CreateElement("newsMap"));
            node.Attributes.Append(doc.CreateAttribute("channels")).Value = string.Join(",", channelIds);
            foreach (var record in records)
            {
                var child = node.AppendChild(doc.CreateElement("record"));
                child.Attributes.Append(doc.CreateAttribute("date")).Value = record.date.ToString("ddMMyyyy");
                child.Attributes.Append(doc.CreateAttribute("records")).Value = record.recordsCount.ToString();
            }

            using (var sw = new StreamWriter(path, false, Encoding.Unicode))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    doc.Save(xw);
                }
            }
        }

        public static NewsMap LoadFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            var doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                Logger.Error("NewsMap.LoadFromFile() error: ", ex);
                return null;
            }

            if (doc.DocumentElement == null) return null;

            var map = new NewsMap();
            var records = new List<NewsMapRecord>();
            map.channelIds = doc.DocumentElement.GetAttributeString("channels").ToIntArrayUniform();
            foreach (XmlElement node in doc.DocumentElement)
            {
                DateTime date;
                if (!DateTime.TryParseExact(node.GetAttributeString("date"), "ddMMyyyy",
                                            CultureProvider.Common, DateTimeStyles.None, out date)) continue;
                var count = node.GetAttributeString("records", "0").ToIntSafe() ?? 0;
                records.Add(new NewsMapRecord { date = date, recordsCount = count });
            }
            map.records = records.ToArray();
            return map;
        }
    
        public bool AreSame(NewsMap map)
        {
            if (map.channelIds.Length != channelIds.Length ||
                map.records.Length != records.Length) return false;
            if (!map.channelIds.SequenceEqual(channelIds)) return false;
            if (!map.records.SequenceEqual(records)) return false;

            return true;
        }
    }

    [Serializable]
    public struct NewsMapRecord
    {
        public DateTime date;

        public int recordsCount;

        public NewsMapRecord(DateTime date, int recordsCount)
        {
            this.date = date;
            this.recordsCount = recordsCount;
        }
    }
}
