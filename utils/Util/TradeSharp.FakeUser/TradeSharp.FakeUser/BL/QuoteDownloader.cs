using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    static class QuoteDownloader
    {
        public static void DownloadForYear(int year, string destinationFolder)
        {
            EnsurePath(destinationFolder);

            var unzippedFileName = string.Format("{0}.txt", year);
            var unzippedFilePath = destinationFolder + "\\" + unzippedFileName;

            if (!File.Exists(unzippedFilePath))
            {
                // скачать
                var url = "http://www.forexite.com/free_forex_quotes/2011/2011.zip".Replace("2011", year.ToString());
                var fileName = string.Format("{0}.zip", year);
                var filePath = destinationFolder + "\\" + fileName;
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, filePath);
                }

                // распаковать
                using (var sr = new FileStream(filePath, FileMode.Open))
                    CompressionHelper.DecompressFiles(sr, destinationFolder);
            }

            ProcessUnzippedFile(unzippedFilePath);
            MergeDownloadedFiles(destinationFolder, year);
        }

        private static void ProcessUnzippedFile(string path)
        {
            //L,EUR,GBP,CHF,JPY,EURGBP,EURCHF,EURJPY,GBPCHF,GBPJPY,CHFJPY,CAD,EURCAD,AUD,AUDJPY,NZD,NZDJPY,XAU,XAG
            //S,EUR
            //D,41275
            //1,1.3184,1.3184,1.3184,1.3184

            using (var sr = new StreamReader(path, Encoding.ASCII))
            {
                var firstLine = sr.ReadLine();
                var tickers = firstLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(
                    GetTickerFullName).ToList();

                var streams = tickers.ToDictionary(t => t, t => new StreamWriter(
                    path + "." + t, false, Encoding.ASCII));

                try
                {
                    StreamWriter currentStream = null;
                    var curDay = new DateTime();
                    DateTime? lastSavedDate = null;
                    
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("S,"))
                        {
                            var currentTicker = GetTickerFullName(line.Substring("S,".Length));
                            currentStream = streams[currentTicker];
                            continue;
                        }
                        if (line.StartsWith("D,"))
                        {
                            var dayPart = line.Substring("D,".Length).ToInt();
                            curDay = new DateTime(1900, 1, 1).AddDays(dayPart);
                            lastSavedDate = null;
                            continue;
                        }

                        if (currentStream == null) continue;
                        var lineParts = line.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        var time = curDay.AddMinutes(lineParts[0].ToInt());
                        var open = lineParts[1].ToFloatUniform();
                        var high = lineParts[2].ToFloatUniform();
                        var low = lineParts[3].ToFloatUniform();
                        var close = lineParts[4].ToFloatUniform();
                        var candle = new CandleData(open, high, low, close, time, time.AddMinutes(1));
                        var pointCost = open < 20 ? 10000 : open < 70 ? 1000 : 100;
                        
                        if (lastSavedDate == null || lastSavedDate.Value != curDay)
                        {
                            lastSavedDate = curDay;
                            currentStream.WriteLine(lastSavedDate.Value.ToString("ddMMyyyy"), curDay);
                        }
                        currentStream.WriteLine("{0} {1} {2}",
                            candle.timeOpen.ToString("HHmm"), 
                            candle.open.ToStringUniformPriceFormat(true),
                            candle.GetHlcOffsetHEX16(pointCost));
                    }
                }
                finally
                {
                    foreach (var stream in streams)
                        stream.Value.Close();
                }
            }
        }

        private static void MergeDownloadedFiles(string folder, int year)
        {
            foreach (var fileName in Directory.GetFiles(folder, year + ".txt.*"))
            {
                var ticker = Path.GetExtension(fileName).Replace(".", "");
                if (string.IsNullOrEmpty(ticker) || ticker.Length < 5) continue;

                var existFileName = folder + "\\" + ticker + ".quote";
                if (!File.Exists(existFileName))
                {
                    File.Move(fileName, existFileName);
                    continue;
                }
                Merge2Files(fileName, existFileName);
            }
        }

        public static void Merge2Files(string downloadedFilePath, string existFilePath)
        {
            // "c:\\Sources\\github\\sharputil\\trunk\\Util\\TradeSharp.FakeUser\\TradeSharp.FakeUser\\bin\\Debug\\quotes\\XAGUSD.quote"
            var ownDateStart = GetFileDate(downloadedFilePath);
            var existDateStart = GetFileDate(existFilePath);

            if (existDateStart == null)
            {
                File.Move(downloadedFilePath, existFilePath);
                //File.Delete(existFilePath);
                //Merge2Files(downloadedFilePath, existFilePath);
                return;
            }

            var startFile = ownDateStart.Value < existDateStart.Value
                ? downloadedFilePath
                : existFilePath;
            var appendingFile = startFile == downloadedFilePath
                ? existFilePath
                : downloadedFilePath;

            using (var sw = new StreamWriter(startFile, true, Encoding.ASCII))
            {
                using (var sr = new StreamReader(appendingFile, Encoding.ASCII))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        sw.WriteLine(line);
                    }
                }
            }

            if (startFile == downloadedFilePath)
            {
                File.Delete(existFilePath);
                File.Move(downloadedFilePath, existFilePath);
            }
            else
                File.Delete(downloadedFilePath);
        }

        private static string GetTickerFullName(string shortName)
        {
            if (shortName.Length > 5) return shortName;

            bool inverse;
            var tickerFromDb = DalSpot.Instance.FindSymbol("USD", shortName, out inverse);
            if (!string.IsNullOrEmpty(tickerFromDb))
                return tickerFromDb;
            return shortName + "USD";
        }

        private static void EnsurePath(string path)
        {
            var directories = new List<string> { path };
            while (true)
            {
                var folder = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder)) break;
                directories.Insert(0, folder);
                path = folder;
            }

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        private static DateTime? GetFileDate(string path)
        {
            if (!File.Exists(path)) return null;
            using (var sr = new StreamReader(path, Encoding.ASCII))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    return DateTime.ParseExact(line, "ddMMyyyy", CultureProvider.Common);
                }
            }
            return null;
        }
    }
}
