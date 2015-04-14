using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NewsRobot
{
    public class CurrencySettings
    {
        public string CountryCode;
        public string CurrencyCode;

        public static List<CurrencySettings> LoadCurrencySettings(string fileName, out string error)
        {
            var doc = new XmlDocument();
            Stream stream;
            try
            {
                stream = File.OpenRead(fileName);
            }
            catch (Exception e)
            {
                error = "LoadCurrencySettings error: " + e.GetType().Name;
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
                    error = "LoadCurrencySettings error: " + e.GetType().Name;
                    return null;
                }
            }
            stream.Close();
            if (doc.DocumentElement == null)
            {
                error = "LoadCurrencySettings error: bad file format";
                return null;
            }
            var root = doc.DocumentElement.SelectSingleNode("Currencies");
            if (root == null)
            {
                error = "LoadCurrencySettings error: bad file format";
                return null;
            }
            var records = root.SelectNodes("Currency");
            if (records == null)
            {
                error = "LoadCurrencySettings error: bad file format";
                return null;
            }
            var result = new List<CurrencySettings>();
            foreach (XmlNode record in records)
            {
                var robotCurrencySettings = new CurrencySettings();
                robotCurrencySettings.CountryCode = NewsRobot.GetTagAttributeValue(record, "CountryCode");
                robotCurrencySettings.CurrencyCode = NewsRobot.GetTagAttributeValue(record, "CurrencyCode");
                result.Add(robotCurrencySettings);
            }
            error = null;
            return result;
        }
    }
}
