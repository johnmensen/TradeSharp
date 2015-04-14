using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    /// <summary>
    /// закачивает бюллетени с CME, парсит и передает в систему - 
    /// вызывает метод PutNews контракта INewsReceiver
    /// </summary>
    class CMEGrabber : BaseNewsGrabber
    {
        private const string CMEFtpPath = "ftp://ftp.cmegroup.com/bulletin/";
        private const string SettingsFileName = "cme.xml";
        public readonly CMETickerInfoCollection bulletinInfo;
        private const string FolderDownload = "cme\\pdf";
        private const string FolderTemp = "cme\\temp";
        // данные, относящиеся к классу News
        private string titleNewsOption, titleNewsFuture;
        private int newsChannelId;
        // опционы, открытый интерес или объем которых меньше указанного в настройках,
        // игнорируются
        private int minOi, minVolume;
        // не закачивать новые файлы, но обработать те, что в директории
        // FolderDownload
        private bool processDownloadedFiles;

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);
        private const int LogMsgFileIoPermissionRequired = 1;
        private const int LogMsgPutNewsFailed = 2;
        
        public CMEGrabber()
        {
            var setsFilePath = string.Format("{0}\\{1}",
                                             ExecutablePath.ExecPath, SettingsFileName);
            bulletinInfo = new CMETickerInfoCollection(setsFilePath);
            // загрузить настройки - Id канала, заголовок "новости"
            LoadNewsSettings(setsFilePath);
        }

        private void LoadNewsSettings(string setsFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(setsFilePath);
            if (doc.DocumentElement == null)
                throw new Exception("Xml doc: document element is missing");
            var setsNodes = doc.DocumentElement.SelectNodes("NewsSettings");
            if (setsNodes == null) throw new Exception("В файле настроек отсутствует узел NewsSettings");
            if (setsNodes.Count == 0) throw new Exception("В файле настроек отсутствует узел NewsSettings");
            var setsNode = (XmlElement) setsNodes[0];
            // прочитать Id канала, заголовок новости
            titleNewsOption = setsNode.Attributes["titleOption"].Value;
            titleNewsFuture = setsNode.Attributes["titleFuture"].Value;
            newsChannelId = setsNode.Attributes["newsChannelId"].Value.ToInt();
            minOi = setsNode.Attributes["minOI"].Value.ToInt();
            minVolume = setsNode.Attributes["minVolume"].Value.ToInt();
            if (setsNode.Attributes["processDownloadedFiles"] != null)
                processDownloadedFiles = setsNode.Attributes["processDownloadedFiles"].Value.ToBool();
            
        }

        public override void GetNews()
        {
            List<string> filesToProcess;
            if (!processDownloadedFiles)
            {
                Logger.Info("CME: старт получения бюллетеней");
                // обновить коллекцию скачанных бюллетеней (*.pdf)
                filesToProcess = DownloadLastBulletins();
                Logger.InfoFormat("CME: закачано {0} бюллетеней", filesToProcess.Count);
                if (filesToProcess.Count == 0) return;
            }
            else
            {
                var folderDonwnloadedPath = string.Format("{0}\\{1}",
                                                          ExecutablePath.ExecPath, FolderDownload);
                if (!Directory.Exists(folderDonwnloadedPath))
                    Directory.CreateDirectory(folderDonwnloadedPath);
                filesToProcess = Directory.GetFiles(folderDonwnloadedPath, "*.zip").Select(Path.GetFileName).ToList();
                Logger.InfoFormat("CME: обработка {0} сохраненных бюллетеней", filesToProcess.Count);
            }
            // распаковать файлы во временную папку
            int countOptions = 0, countFutures = 0;
            foreach (var fileName in filesToProcess)
            {
                DecompressBulletin(fileName);
                // сформировать из распакованных файлов файлы XML
                // c "пережеванной" информацией
                ProcessDecompressedBulletins();
                // прочитать файлы XML и закинуть данные в базу
                countOptions += MakeOptionLevels();
                countFutures += MakeFuturesVolumeFromXls();
                // очистить временные каталоги
                Cleanup();                
            }
            Logger.InfoFormat("Обработано {0} опционов и {1} фьючерсов", countOptions, countFutures);
        }

        private void Cleanup()
        {
            var tempFullPath = string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderTemp);
            foreach (var file in Directory.GetFiles(tempFullPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogMsgFileIoPermissionRequired, 1000 * 60,
                            "Невозможно удалить файл \"{0}\": IOException", file);
                        throw;
                    }                    
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка удаления временного файла \"{0}\": {1}", file, ex);
                }
            }
        }

        private static List<string> DownloadLastBulletins()
        {
            var availBulls = GetAllBulletinNamesFromCME();
            if (availBulls.Count == 0) return new List<string>();
            Logger.Info("В каталоге CME " + availBulls.Count + " бюллетеней");
            // сравнить со списком файлов, загруженных в FolderDownload
            var downloadedFiles = GetAllDownloadedBulletins();
            var filesToDownload = availBulls.Where(fileName => !downloadedFiles.Contains(fileName)).ToList();
            if (filesToDownload.Count == 0) return filesToDownload;
            // загрузить файлы
            foreach (var fileName in filesToDownload)
                DownloadBulletin(fileName);
            return filesToDownload;
        }

        private static void DecompressBulletin(string fileName)
        {
            var targetFolder = string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderTemp);
            // распаковать во временный архив
            var path = string.Format("{0}\\{1}\\{2}", ExecutablePath.ExecPath, FolderDownload, fileName);
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    try
                    {
                        CompressionHelper.DecompressFiles(fs, targetFolder);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("CompressionHelper.DecompressFiles error: \"{0}\", {1}",
                                            fileName, ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ProcessDecompressedBulletins error: \"{0}\", {1}",
                                    fileName, ex);
                throw;
            }            
        }

        /// <summary>
        /// создать XLS-файлы из бюллетеней
        /// </summary>
        private void ProcessDecompressedBulletins()
        {
            var futuresSummary = new List<CMEFuturesSummary>();
            StreamWriter sw = null;
            try
            {
                foreach (var fileName in Directory.GetFiles(
                    string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderTemp), 
                    "Section*.pdf"))
                {
                    // получить номер секции
                    var fileNameOnly = Path.GetFileNameWithoutExtension(fileName);
                    if (string.IsNullOrEmpty(fileNameOnly)) continue;
                    var endIndex = fileNameOnly.IndexOf('_');
                    var startIndex = "Section".Length;
                    var sectionString = fileNameOnly.Substring(startIndex, endIndex - startIndex);

                    // отбор только тех бюллетеней, что перечислены в bulletinInfo
                    if (!bulletinInfo.sectionsToParse.Contains(sectionString)) continue;

                    // преобразовать в текст
                    var fileNameTxt = fileName.Replace(".pdf", ".txt");
                    try
                    {
                        PdfConverter.PdfFileToText(fileName, fileNameTxt);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка в PdfFileToText({0}): {1}", fileName, ex);
                        throw;
                    }                    

                    // преобразовать в XLS                    
                    var parser = new CMEDocumentParser(fileNameTxt, bulletinInfo, futuresSummary);
                    if (!parser.Parsed) continue;

                    // создать поток вывода (один на все файлы)
                    if (sw == null)
                    {
                        var nameXls = string.Format("{0}\\{1}\\_{2:ddMMyyyy}.xls",
                            ExecutablePath.ExecPath, FolderTemp, parser.DocumentDate);
                        sw = new StreamWriter(nameXls, false, Encoding.Unicode);
                    }
                    parser.WriteTablesInExcelFormat(sw);
                    // записать информацию по фьючерсам в отдельный файл
                    if (futuresSummary.Count > 0)
                    {
                        var minDate = futuresSummary.Min(fs => fs.DocumentDate);
                        var maxDate = futuresSummary.Max(fs => fs.DocumentDate);
                        var sumFileName = string.Format("{0}\\{1}\\_{2:ddMMyy}_{3:ddMMyy}.txt",
                            ExecutablePath.ExecPath, FolderTemp, minDate, maxDate);
                        CMEDocumentParser.WriteFutureSummary(sumFileName, futuresSummary);
                    }
                }
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        private int MakeOptionLevels()
        {
            var tmpFolderPath = string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderTemp);
            var newsList = new List<News>();
            foreach (var fileName in Directory.GetFiles(tmpFolderPath, "*.xls"))
            {
                // ожидаем записи вида
                // BULLETIN	29	17.08.2010						
                // LEGEND:strike	volume	OI	ClosRange	OpRange	High	Low	Delta	SettPrice
                // TABLE	USDJPY	SEP10	CALL	American    100000  10000   1
                // 920		8	3,81				0,971	4,85

                DateTime? curDate = null;
                string curSymbol = null;
                var optType = OptionType.CALL;
                var optStyle = OptionStyle.American;
                ExpirationMonth month = null;
                decimal strikeToBase = 1, premiumToBase = 1;
                var invertRate = false;

                using (var sr = new StreamReader(fileName, Encoding.Unicode))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("LEGEND")) continue;

                        var parts = line.Split(new[] {' ', (char) 9}, StringSplitOptions.None);
                        if (parts[0] == "TABLE")
                        {
                            // получить текущие актив, дату экспирации, тип и стиль опциона
                            curSymbol = parts[1];
                            month = ExpirationMonth.Parse(parts[2]);
                            optType = (OptionType) Enum.Parse(typeof (OptionType), parts[3]);
                            optStyle = (OptionStyle) Enum.Parse(typeof (OptionStyle), parts[4]);
                            strikeToBase = parts[5].Replace(',', '.').ToDecimalUniform();
                            premiumToBase = parts[6].Replace(',', '.').ToDecimalUniform();
                            invertRate = parts[7] == "1" ? true : false;
                            continue;
                        }
                        if (parts[0] == "BULLETIN")
                        {
                            // получить дату бюллетеня
                            curDate = DateTime.ParseExact(parts[2], "dd.MM.yyyy", CultureProvider.Common);
                            continue;
                        }
                        // получить текущие данные: 
                        // strike	volume	OI	ClosRange	OpRange	High	Low	Delta	SettPrice                        
                        var strike = parts[0].Replace(',', '.').ToDecimalUniformSafe();
                        var volume = string.IsNullOrEmpty(parts[1]) ? 0 : parts[1].ToInt();
                        var OI = string.IsNullOrEmpty(parts[2]) ? 0 : parts[2].ToInt();
                        var closeRange = parts[3].Replace(',', '.').ToDecimalUniformSafe();
                        var openRange = parts[4].Replace(',', '.').ToDecimalUniformSafe();
                        var high = parts[5].Replace(',', '.').ToDecimalUniformSafe();
                        var low = parts[6].Replace(',', '.').ToDecimalUniformSafe();
                        var delta = parts[7].Replace(',', '.').ToDecimalUniformSafe();
                        var settPrice = parts[8].Replace(',', '.').ToDecimalUniformSafe();

                        // привести все цены к ценам БА
                        strike = strike/strikeToBase;
                        closeRange = closeRange.HasValue ? closeRange / premiumToBase : 0;
                        openRange = openRange.HasValue ? openRange / premiumToBase : 0;
                        high = high.HasValue ? high / premiumToBase : 0;
                        low = low.HasValue ? low / premiumToBase : 0;
                        delta = delta.HasValue ? delta / premiumToBase : 0;
                        settPrice = settPrice.HasValue ? settPrice / premiumToBase : 0;                            
                            
                        if (curDate.HasValue && (!string.IsNullOrEmpty(curSymbol)) && month != null)
                            if (OI >= minOi && volume >= minVolume)
                            {                                    
                                if (invertRate)                                                                            
                                {
                                    if (optType == OptionType.CALL)
                                    {
                                        optType = OptionType.PUT;
                                        high = high.HasValue ? 1/strike - 1/(strike + high.Value) : 0;
                                        low = low.HasValue ? 1/strike - 1/(strike + low.Value) : 0;
                                        delta = delta.HasValue ? 1 / strike - 1 / (strike + delta.Value) : 0;
                                        closeRange = closeRange.HasValue ? 1 / strike - 1 / (strike + closeRange.Value) : 0;
                                        openRange = openRange.HasValue ? 1 / strike - 1 / (strike + openRange.Value) : 0;
                                        settPrice = settPrice.HasValue ? 1 / strike - 1 / (strike + settPrice.Value) : 0;                                            
                                    }
                                    else
                                    {
                                        optType = OptionType.CALL;
                                        high = high.HasValue ? 1 / (strike - high.Value) - 1 / strike : 0;
                                        low = low.HasValue ? 1 / (strike - low.Value) - 1 / strike : 0;
                                        delta = delta.HasValue ? 1 / (strike - delta.Value) - 1 / strike : 0;
                                        closeRange = closeRange.HasValue ? 1 / (strike - closeRange.Value) - 1 / strike : 0;
                                        openRange = openRange.HasValue ? 1 / (strike - openRange.Value) - 1 / strike : 0;
                                        settPrice = settPrice.HasValue ? 1 / (strike - settPrice.Value) - 1 / strike : 0;
                                    }
                                    strike = 1 / strike;
                                }
                                var news = MakeOptionLevelNews(curDate.Value, curSymbol, month, optStyle,
                                                                        optType,
                                                                        strike.Value, volume, OI, closeRange.Value,
                                                                        openRange.Value,
                                                                        high.Value, low.Value, delta.Value,
                                                                        settPrice.Value);                                
                                newsList.Add(news);
                            }
                    } // while ! EOS
                } // using (var sr =
            } // foreach (var fileName

            // отправить каждую новость провайдеру
            return SendNewsToProvider(newsList);
        }

        private int SendNewsToProvider(List<News> newsList)
        {
            if (newsList.Count == 0) return 0;
            Logger.InfoFormat("Передача {0} новостей на сервер ({1})",
                newsList.Count, newsList[0].Title);
            var numFail = 0;
            const int maxFails = 3;
            for (var i = 0; i < newsList.Count; i+= MaxNewsInBlock)
            {
                var endIndex = i + MaxNewsInBlock;
                if (endIndex >= newsList.Count) endIndex = newsList.Count - 1;
                if (endIndex <= i) break;
                if (!PutNewsOnServer(newsList.GetRange(i, endIndex - i).ToArray())) numFail++;
                if (numFail == maxFails)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                             LogMsgPutNewsFailed, 1000 * 60, 
                                                             "Передача новостей (опционы) на сервер невозможна");
                    return i;
                }
            }
            return newsList.Count;
        }

        private News MakeOptionLevelNews(DateTime curDate,
           string curSymbol, ExpirationMonth expMonth,
           OptionStyle optStyle, OptionType optType, decimal strike, int volume,
           int OI, decimal closeRange, decimal openRange, decimal high, decimal low,
           decimal delta, decimal settPrice)
        {
            //[#fmt]#&newstype=option#&type=Call#&style=American#&baseactive=EURUSD#&
            //publishdate=09.03.2011 02:34:00#&dateexpiration=11.06.2011#&strikeprice=1.4550#&
            //high=0.0140#&low=0.0090#&close=0.0110#&oi=948#&volume=0
            var newsBody = string.Format(CultureProvider.Common,
                                 "[#fmt]#&newstype=option#&type={0}#&style={1}#&baseactive={2}#&" +
                                 "publishdate={3:dd.MM.yyyy HH:mm:ss}#&dateexpiration={4:dd.MM.yyyy}#&strikeprice={5}#&" +
                                 "high={6}#&low={7}#&close={8}#&oi={9}#&volume={10}",
                                 optType, optStyle, curSymbol, curDate, new DateTime(expMonth.Year, expMonth.Month, 11),
                                 strike, high, low, closeRange, OI, volume);
            return new News(newsChannelId, curDate, titleNewsOption, newsBody);
        }

        private int MakeFuturesVolumeFromXls()
        {
            var newsFutures = new List<News>();
            // "[#fmt]#;Ticker=EURUSD#;VolumeRTH=5412#;VolumeGlobex=25701#;OpenInterest=24790#;Date=10062011#;"    
            foreach (var fileName in Directory.GetFiles(
                string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderTemp), "*.txt"))
            {
                // fileName is _030910_030910.txt
                var namePart = Path.GetFileNameWithoutExtension(fileName); // _030910_030910
                var date = GetDateFromFuturesFileNamePart(namePart);
                if (!date.HasValue) continue;

                using (var fs = new StreamReader(fileName))
                {
                    // ожидаются строки вида
                    // "Ticker{0}VolumeRTH{0}VolumeGlobex{0}OI"
                    while (!fs.EndOfStream)
                    {
                        var line = fs.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("Ticker")) continue;
                        var newsBody = GetFuturesVolumeNewsBody(line, date.Value);
                        if (string.IsNullOrEmpty(newsBody)) continue;
                        newsFutures.Add(new News(newsChannelId, date.Value, titleNewsFuture, newsBody));                        
                    }
                }
            }
            // отправить каждую новость провайдеру
            return SendNewsToProvider(newsFutures);
        }

        private static string GetFuturesVolumeNewsBody(string line, DateTime date)
        {
            // из строки вида "Ticker{0}VolumeRTH{0}VolumeGlobex{0}OI"
            // строку вида "[#fmt]#;Ticker=EURUSD#;VolumeRTH=5412#;VolumeGlobex=25701#;OpenInterest=24790#;Date=10062011#;"
            var parts = line.Split(new[] { (char)9 }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4) return string.Empty;
            var ticker = parts[0];
            int volRTH, volGlobex, OI;
            if (!int.TryParse(parts[1], out volRTH)) return string.Empty;
            if (!int.TryParse(parts[2], out volGlobex)) return string.Empty;
            if (!int.TryParse(parts[3], out OI)) return string.Empty;
            return string.Format("[#fmt]#;Ticker={0}#;VolumeRTH={1}#;VolumeGlobex={2}#;OpenInterest={3}#;Date={4:ddMMyyyy}#;",
                ticker, volRTH, volGlobex, OI, date);
        }

        /// <summary>
        /// зайти на страничку архива CME, получить там все имена бюллетеней
        /// </summary>
        /// <returns></returns>
        private static List<string> GetAllBulletinNamesFromCME()
        {
            var bullets = new List<string>();
            try
            {
                var reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(CMEFtpPath));
                reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
                var response = (FtpWebResponse)reqFtp.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var filesString = streamReader.ReadToEnd();
                    var parts = filesString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    bullets.AddRange(parts.Where(part => part.EndsWith(".zip")));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка бюллетеней с CME", ex);
            }
            
            return bullets;
        }

        /// <summary>
        /// в каталоге Pdf получить имена всех файлов (архивы)
        /// </summary>
        /// <returns></returns>
        private static string[] GetAllDownloadedBulletins()
        {
            var fullFolderPath = string.Format("{0}\\{1}", ExecutablePath.ExecPath, FolderDownload);
            if (!Directory.Exists(fullFolderPath))
                Directory.CreateDirectory(fullFolderPath);
            // список всех архивов в директории
            return Directory.GetFiles(fullFolderPath, "*.zip").Select(Path.GetFileName).ToArray();
        }

        /// <summary>
        /// скачать указанный бюллетень за день с сайта CME
        /// </summary>
        /// <param name="name"></param>
        private static void DownloadBulletin(string name)
        {
            var fullName = CMEFtpPath + name;
            var targetFileName = string.Format("{0}\\{1}\\{2}", 
                ExecutablePath.ExecPath, FolderDownload, name);

            if (File.Exists(targetFileName)) return;

            // скачать файл в targetFolder
            var reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(fullName));
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.UseBinary = true;
            var response = (FtpWebResponse)reqFTP.GetResponse();

            const int buffLen = 2048;
            var buffer = new byte[buffLen];

            using (var streamReader = response.GetResponseStream())
            using (var streamWriter = new FileStream(targetFileName, FileMode.Create))
            {
                while (true)
                {
                    var read = streamReader.Read(buffer, 0, buffLen);
                    if (read == 0) break;
                    streamWriter.Write(buffer, 0, read);
                }
            }
        }

        private static DateTime? GetDateFromFuturesFileNamePart(string fileName)
        {
            // _030910_030910
            if (fileName.Length < 7) return null;
            int day, month, year;
            if (!int.TryParse(fileName.Substring(1, 2), out day)) return null;
            if (!int.TryParse(fileName.Substring(3, 2), out month)) return null;
            if (!int.TryParse(fileName.Substring(5, 2), out year)) return null;
            return new DateTime(2000 + year, month, day);
        }
    }
}
