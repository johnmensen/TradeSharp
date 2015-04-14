using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TradeSharp.Util;

namespace NewsRobot
{
    public class NewsSettings
    {
        public string Pair;
        public string CountryCode;
        public string Title;
        public int Weight;
        public int MinDelta;

        public static List<NewsSettings> LoadNewsSettings(string fileName, out string error)
        {
            var doc = new XmlDocument();
            Stream stream;
            try
            {
                stream = File.OpenRead(fileName);
            }
            catch (Exception e)
            {
                error = "LoadNewsSettings error: " + e.GetType().Name;
                return null;
            }
            using (var streamReader = new StreamReader(stream))
            {
                try
                {
                    doc.Load(streamReader);
                }
                catch (XmlException e)
                {
                    error = "LoadNewsSettings error: " + e.GetType().Name;
                    return null;
                }
            }
            stream.Close();
            if (doc.DocumentElement == null)
            {
                error = "LoadNewsSettings error: bad file format";
                return null;
            }
            var records = doc.DocumentElement.SelectNodes("News");
            if (records == null)
            {
                error = "LoadNewsSettings error: bad file format";
                return null;
            }
            var result = new List<NewsSettings>();
            foreach (XmlNode record in records)
            {
                var robotNewsSetting = new NewsSettings();
                robotNewsSetting.Pair = NewsRobot.GetTagAttributeValue(record, "NewsPair");
                robotNewsSetting.CountryCode = NewsRobot.GetTagAttributeValue(record, "CountryCode");
                robotNewsSetting.Title = NewsRobot.GetTagAttributeValue(record, "NewsTitle");
                robotNewsSetting.Weight = NewsRobot.GetTagAttributeValue(record, "Weight").ToInt();
                robotNewsSetting.MinDelta = NewsRobot.GetTagAttributeValue(record, "MinDelta").ToIntSafe() ?? 0;
                result.Add(robotNewsSetting);
            }
            error = null;
            return result;
        }
    }
}
