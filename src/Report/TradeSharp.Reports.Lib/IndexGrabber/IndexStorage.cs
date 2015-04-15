using System;
using System.Collections.Generic;
using System.IO;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Reports.Lib.IndexGrabber
{
    /// <summary>
    /// файловое хранилище котировок по индексам (бенчмаркам)
    /// данные хранятся с частотой не более 1 в день
    /// </summary>
    public class IndexStorage
    {
        private readonly string folderPath;
        private readonly string grabberSettingsFile;

        public IndexStorage(string folderPath, string grabberSettingsFile)
        {
            this.folderPath = folderPath;
            this.grabberSettingsFile = grabberSettingsFile;
        }

        public Dictionary<string, List<Cortege2<DateTime, float>>> GetTickerData()
        {
            var data = new Dictionary<string, List<Cortege2<DateTime, float>>>();
            foreach (var fileName in Directory.GetFiles(folderPath, "*.txt"))
            {
                var tickerName = Path.GetFileNameWithoutExtension(fileName);
                var tickerData = GetTickerDataFromFile(fileName);
                if (tickerData != null && tickerData.Count > 0)
                    data.Add(tickerName, tickerData);
            }
            return data;
        }

        public List<Cortege2<DateTime, float>> GetTickerData(string tickerName)
        {
            var filePath = string.Format("{0}\\{1}.txt", folderPath, tickerName);
            return !File.Exists(filePath) ? new List<Cortege2<DateTime, float>>() : 
                GetTickerDataFromFile(filePath);
        }

        private static List<Cortege2<DateTime, float>> GetTickerDataFromFile(string filePath)
        {
            var data = new List<Cortege2<DateTime, float>>();
            using (var sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var parts = line.Split(new[] {(char) 9}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;
                    data.Add(new Cortege2<DateTime, float>(DateTime.ParseExact(parts[0], "dd.MM.yyyy",
                                                                                 CultureProvider.Common), 
                                                                                 parts[1].ToFloatUniform()));
                }
            }
            return data;
        }

        public int UpdateTickers()
        {
            var grabbers = IndexDataGrabber.ReadGrabbers(grabberSettingsFile);
            if (grabbers == null) return 0;
            
            // прочитать данные каждым граббером
            var indexData = new Dictionary<string, List<Cortege2<DateTime, decimal>>>();
            foreach (var grabber in grabbers)
            {
                var data = grabber.GrabIndexData();
                if (data.Count == 0) continue;
                indexData[grabber.IndexName] = data;
            }
            if (indexData.Count == 0) return 0;
            
            // обновить хранилище, не дублировать данные
            var countUpdated = 0;
            foreach (var pair in indexData)
            {
                var newData = pair.Value;
                if (newData.Count == 0) continue;
                var oldData = GetTickerData(pair.Key);
                if (oldData.Count > 0)
                {
                    var oldEnd = oldData[oldData.Count - 1].a;
                    var indexStart = newData.FindIndex(d => d.a > oldEnd);
                    if (indexStart < 0) continue;
                    newData = newData.GetRange(indexStart, newData.Count - indexStart);
                }
                countUpdated += newData.Count;
                AppendTickerData(pair.Key, newData);
            }
            
            return countUpdated;
        }

        private void AppendTickerData(string tickerName, Cortege2<DateTime, decimal> tickerData)
        {
            AppendTickerData(tickerName, new List<Cortege2<DateTime, decimal>> {tickerData});
        }

        private void AppendTickerData(string tickerName, List<Cortege2<DateTime, decimal>> tickerData)
        {
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception)
                {
                    Logger.ErrorFormat("IndexStorage: невозможно создать каталог \"{0}\"", folderPath);
                    throw;
                }
            }
            var filePath = string.Format("{0}\\{1}.txt", folderPath, tickerName);
            using (var sw = new StreamWriter(filePath, true))
            {
                foreach (var data in tickerData)
                {
                    var dataStr = string.Format("{1:dd.MM.yyyy}{0}{2}", 
                        (char) 9, data.a, data.b.ToStringUniform());
                    sw.WriteLine(dataStr);
                }
            }
        }
    }
}
