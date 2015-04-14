using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Entity;
using System.Linq;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.TradeSignal.BL
{
    /// <summary>
    /// хранит "торговые сигналы", разбивая их по категориям, парам и таймфреймам
    /// </summary>
    public class SignalStorage
    {
        private static SignalStorage instance;

        public static SignalStorage Instance
        {
            get { return instance ?? (instance = new SignalStorage()); }
        }

        private readonly string storageFolder = ExecutablePath.ExecPath + "\\" +
                                                AppConfig.GetStringParam("StorageFolder", "storage");

        private readonly Dictionary<SignalStorageKey, DateTime> updateTimes = new Dictionary<SignalStorageKey, DateTime>();

        private readonly ReaderWriterLock dicUpdateLocker = new ReaderWriterLock();
        
        private const int LockTimeout = 1000;

        private readonly ThreadSafeStorage<SignalStorageKey, ReaderWriterLock> fileLockers = 
            new ThreadSafeStorage<SignalStorageKey, ReaderWriterLock>();

        public SignalStorage()
        {
            EnsureStorageFolder();
            InitUpdateTimesFromFiles();
        }

        /// <summary>
        /// обновить торговые рекомендации (список объектов в формате XML)
        /// </summary>
        /// <param name="signalCatId">категория сигнала</param>
        /// <param name="ticker">тикер (USDCHF...)</param>
        /// <param name="timeframe">таймфрейм прогноза</param>
        /// <param name="signalXml">XML (well-formed) в виде строки</param>
        public void UpdateSignal(int signalCatId, string ticker, BarSettings timeframe, string signalXml)
        {
            // проверить параметры
            if (string.IsNullOrEmpty(signalXml))
            {
                Logger.ErrorFormat("UpdateSignal (cat={0}) error: signalXml is empty", signalCatId);
                return;
            }
            if (!DalSpot.Instance.GetTickerNames().Contains(ticker))
            {
                Logger.ErrorFormat("UpdateSignal (cat={0}) error: ticker \"{1}\" is not found", signalCatId, ticker);
                return;
            }

            // проверить XML
            if (!TradeSignalXml.XmlIsValid(ref signalXml, true))
            {
                Logger.ErrorFormat("UpdateSignal (cat={0}) error: ", signalCatId, ticker);
                return;
            }

            // записать файл
            var key = new SignalStorageKey {categoryId = signalCatId, ticker = ticker, timeframe = timeframe};
            if (!WriteFile(key, signalXml)) return;

            // обновить словарь dicUpdateLocker
            try
            {
                dicUpdateLocker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("SignalStorage - updateTimes write lock timeout");
                return;
            }

            try
            {
                if (updateTimes.ContainsKey(key)) updateTimes[key] = DateTime.Now;
                else updateTimes.Add(key, DateTime.Now);
            }
            catch (Exception ex)
            {
                Logger.Error("SignalStorage - updateTimes update error", ex);
                return;
            }
            finally
            {
                dicUpdateLocker.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// вернять время последнего обновления по каждому тикеру / ТФ в указанных категориях
        /// </summary>
        /// <param name="categoryIds">список категорий, по которым запрошены времена обновлений</param>
        /// <returns>категория, тикер, таймфрейм - время обновления</returns>
        public List<Cortege2<SignalStorageKey, DateTime>> GetLastUpdateTimes(List<int> categoryIds)
        {
            var resultDic = new List<Cortege2<SignalStorageKey, DateTime>>();
            try
            {
                dicUpdateLocker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("SignalStorage - updateTimes read lock timeout");
                return resultDic;
            }

            try
            {
                resultDic.AddRange(from pair in updateTimes
                                   where categoryIds.Contains(pair.Key.categoryId)
                                   select new Cortege2<SignalStorageKey, DateTime>(pair.Key, pair.Value));
                return resultDic;
            }
            catch (Exception ex)
            {
                Logger.Error("SignalStorage - updateTimes read error", ex);
                return resultDic;
            }
            finally
            {
                dicUpdateLocker.ReleaseReaderLock();
            }
        }

        public string GetTradeSignalXml(int signalCatId, string ticker, BarSettings timeframe)
        {
            return GetTradeSignalXml(new SignalStorageKey
                { categoryId = signalCatId, ticker = ticker, timeframe = timeframe });
        }

        /// <summary>
        /// получить собственно XML торгового сигнала (объекты графика)
        /// </summary>
        public string GetTradeSignalXml(SignalStorageKey key)
        {
            // проверить наличие файла
            var filePath = MakeFilePath(key);
            try
            {
                if (!File.Exists(filePath)) return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalStorage.GetTradeSignalXml({0}) error: {1}", filePath, ex);
                return string.Empty;
            }

            // получить доступ к файлу
            var locker = fileLockers.ReceiveValue(key);
            if (locker == null)
            {
                locker = new ReaderWriterLock();
                fileLockers.UpdateValues(key, locker);
            }
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("SignalStorage.GetTradeSignalXml - read lock timeout");
                return string.Empty;
            }
            try
            {
                // прочитать файл
                using (var sw = new StreamReader(filePath, TradeSignalXml.DefaultEncoding))
                {
                    return sw.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalStorage.GetTradeSignalXml - error reading file ({0}): {1}", filePath, ex);
                return string.Empty;
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
            // загрузить XML из файла
        }

        /// <summary>
        /// thread safe
        /// </summary>        
        private bool WriteFile(SignalStorageKey key, string xml)
        {
            var filePath = MakeFilePath(key);
            // получить доступ на запись файла
            var locker = fileLockers.ReceiveValue(key);
            if (locker == null)
            {
                locker = new ReaderWriterLock();
                fileLockers.UpdateValues(key, locker);
            }
            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("SignalStorage.WriteFile - writer lock timeout");
                return false;
            }
            try
            {
                // обеспечить каталог файла
                if (!EnsureFileFolder(filePath)) return false;
                // сохранить файл
                using (var sw = new StreamWriter(filePath, false, TradeSignalXml.DefaultEncoding))
                {
                    sw.Write(xml);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalStorage.WriteFile - error writing file ({0}): {1}", filePath, ex);
                return false;
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
            return true;
        }

        private void EnsureStorageFolder()
        {
            if (Directory.Exists(storageFolder)) return;
            try
            {
                Directory.CreateDirectory(storageFolder);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalStorage - can not create directory {0}: {1}", storageFolder, ex);
                throw;
            }
        }

        private bool EnsureFileFolder(string filePath)
        {
            var fileDir = Path.GetDirectoryName(filePath);
            if (Directory.Exists(fileDir)) return true;
            try
            {
                Directory.CreateDirectory(fileDir);
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalStorage - can not create file directory {0}: {1}", storageFolder, ex);
                return false;
            }
        }
    
        private string MakeFilePath(SignalStorageKey key)
        {
            return storageFolder + "\\" + key.categoryId + "\\" + key.ticker + "_" + key.timeframe + ".xml";
        }

        private static SignalStorageKey GetKeyFromFilePath(int catId, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var nameParts = fileName.Split('_');
            if (nameParts.Length != 2) 
                return default(SignalStorageKey);
            if (string.IsNullOrEmpty(nameParts[0]) || string.IsNullOrEmpty(nameParts[1])) 
                return default(SignalStorageKey);
            if (!DalSpot.Instance.GetTickerNames().Contains(nameParts[0])) 
                return default(SignalStorageKey);
            var ticker = nameParts[0];
            try
            {
                var timeframe = new BarSettings(nameParts[1]);
                return new SignalStorageKey {categoryId = catId, ticker = ticker, timeframe = timeframe};
            }
            catch
            {
                return default(SignalStorageKey);
            }            
        }
    
        /// <summary>
        /// прочитать файлы
        /// </summary>
        private void InitUpdateTimesFromFiles()
        {            
            try
            {
                // перебрать все каталоги
                foreach (var dirPath in Directory.GetDirectories(storageFolder))
                {
                    // получить из пути директории имя
                    var dirName = Path.GetFileName(dirPath);
                    // получить категорию
                    var catId = dirName.ToIntSafe();
                    if (!catId.HasValue) continue;
                    // прочитать все файлы в категории
                    foreach (var filePath in Directory.GetFiles(dirPath, "*_*.xml"))
                    {
                        var key = GetKeyFromFilePath(catId.Value, filePath);
                        if (key.categoryId == 0) continue;
                        // прочитать XML, убедившись что он валиден, и получить из него
                        // время обновления
                        DateTime updateTime;
                        int objectsCount;
                        if (!TradeSignalXml.GetSignalUpdateParamsFromFile(filePath, TradeSignalXml.DefaultEncoding,
                            out updateTime, out objectsCount)) continue;
                        // добавить в словарь
                        updateTimes.Add(key, updateTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SignalStorage.InitUpdateTimesFromFiles() error", ex);
                throw;
            }
        }
    }    
}
