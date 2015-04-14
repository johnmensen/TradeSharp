using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin.BL
{
    public static class ForexiteDownloader
    {
        private const string ForexiteUri = "http://www.forexite.com/free_forex_quotes/";

        public static Dictionary<int, List<CandleData>> ReadDayQuotesFromForexite(DateTime dt,
            int shiftHours, List<string> symbolsToStore)
        {
            var dir = ExecutablePath.ExecPath + "\\Forexite";
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while creating target dir", ex);
                throw;
            }
            return ReadDayQuotesFromForexite(dt, dir, shiftHours, symbolsToStore);
        }

        public static Dictionary<int, List<CandleData>> ReadDayQuotesFromForexite(DateTime dt, string targetDir,
            int shiftHours, List<string> symbolsToStore)
        {
            var fileName = LoadFileFromForexite(dt, targetDir);
            if (string.IsNullOrEmpty(fileName)) return new Dictionary<int, List<CandleData>>();
            return ParseForexiteFile(fileName, shiftHours, symbolsToStore);
        }

        /// <summary>
        /// Загрузка из инета zip файла QuoteRoom
        /// </summary>
        private static string LoadFileFromForexite(DateTime dt, string targetDir)
        {
            // убедиться в наличии директории
            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Forexite loading: error create dir \"{0}\": {1}", targetDir, ex);
                return string.Empty;
            }

            var srcFileName = dt.ToString("ddMMyy") + ".zip";
            var destPath = ExecutablePath.ExecPath + "\\" + srcFileName;
            var uri = ForexiteUri + dt.ToString("yyyy") + @"/" + dt.ToString("MM") + @"/";
            var srcPath = uri + srcFileName;
            
            try
            {
                var user = new WebClient();
                try
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    user.DownloadFile(srcPath, destPath);
                }
                catch (WebException exc)
                {
                    // скачать не получилось, выходим
                    Logger.Error("Error loading file from Forexite", exc);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return destPath;
        }

        /// <summary>
        /// Распаковка и обработка архива Forexite
        /// </summary>
        private static Dictionary<int, List<CandleData>> ParseForexiteFile(string zipFilePath, 
            int shiftHours,
            List<string> symbolsToStore)
        {
            var candles = new Dictionary<int, List<CandleData>>();
            string unpackedFileName;
            
            // таки распаковать файл котировок
            try
            {
                var fileNames = CompressionHelper.UnzipFile(zipFilePath, Path.GetDirectoryName(zipFilePath));
                if (fileNames.Count == 0) return candles;
                unpackedFileName = fileNames[0];
            }
            catch (Exception ex)
            {
                Logger.Error("Error while decompressing file from Forexite", ex);
                return candles;
            }

            // распаковка закончена, теперь заполняем коллекции QRHistory
            try
            {            
                using (var sr = File.OpenText(unpackedFileName))
                {
                    // пропускаем заголовок
                    var input = sr.ReadLine();
                    if (string.IsNullOrEmpty(input)) return candles;                                
                    while ((input = sr.ReadLine()) != null)
                    {
                        // разбираем строку
                        var index = input.IndexOf(",");
                        var index1 = index;
                        var symbol = input.Substring(0, index);
                        //symbol = FullTradeCurrencyNameFromForexiteFormat(symbol);
                        if (!symbolsToStore.Contains(symbol)) continue;
                        var code = DalSpot.Instance.GetFXICodeBySymbol(symbol);
                        if (code == 0) continue;

                        // куда сохранять
                        List<CandleData> currentList;
                        if (!candles.TryGetValue(code, out currentList))
                        {
                            currentList = new List<CandleData>();
                            candles.Add(code, currentList);
                        }                        

                        index1 = input.IndexOf(",", index + 1);
                        var strDate = input.Substring(index + 1, index1 - index - 1);
                        index = index1;
                        index1 = input.IndexOf(",", index + 1);
                        var strTime = input.Substring(index + 1, index1 - index - 1);
                        index = index1;
                        index1 = input.IndexOf(",", index + 1);
                        var strOpen = input.Substring(index + 1, index1 - index - 1);
                        index = index1;
                        index1 = input.IndexOf(",", index + 1);
                        var strHigh = input.Substring(index + 1, index1 - index - 1);
                        index = index1;
                        index1 = input.IndexOf(",", index + 1);
                        var strLow = input.Substring(index + 1, index1 - index - 1);
                        var strClose = input.Substring(index1 + 1, input.Length - index1 - 1);
                        var quote = new CandleData();
                        var date = string.Format("{0}/{1}/{2}", strDate.Substring(0, 4), strDate.Substring(4, 2),
                                                    strDate.Substring(6, 2));
                        var provider = new NumberFormatInfo {NumberDecimalSeparator = "."};

                        quote.timeOpen = Convert.ToDateTime(date);
                        quote.timeOpen = quote.timeOpen.AddHours(Convert.ToDouble(strTime.Substring(0, 2)));
                        quote.timeOpen = quote.timeOpen.AddMinutes(Convert.ToDouble(strTime.Substring(2, 2)));
                        quote.timeOpen = quote.timeOpen.AddSeconds(Convert.ToDouble(strTime.Substring(4, 2)));
                        // QuoteRoom поставляет в GMT+1
                        quote.timeOpen = quote.timeOpen.AddHours(shiftHours);
                        // преобразуем в локальный формат
                        quote.timeOpen = quote.timeOpen.ToLocalTime();
                        quote.open = Convert.ToSingle(strOpen, provider);
                        quote.high = Convert.ToSingle(strHigh, provider);
                        quote.low = Convert.ToSingle(strLow, provider);
                        quote.close = Convert.ToSingle(strClose, provider);
                        currentList.Add(quote);
                    }                
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while parsing Forexite file", ex);
            }

            // удалить оба файла
            try
            {
                File.Delete(zipFilePath);
                File.Delete(unpackedFileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while finally deleting Forexite files", ex);
            }
            return candles;
        }

        private static string FullTradeCurrencyNameFromForexiteFormat(string symbol)
        {
            symbol = symbol.ToUpper();
            if (symbol.Length > 3) return symbol;
            return 
                symbol == "EUR" ? "EURUSD"
              : symbol == "JPY" ? "USDJPY"
              : symbol == "CHF" ? "USDCHF" 
              : symbol == "CAD" ? "USDCAD"
              : symbol == "AUD" ? "AUDUSD"
              : symbol == "NZD" ? "NZDUSD"
              : symbol;
        }
    }
}
