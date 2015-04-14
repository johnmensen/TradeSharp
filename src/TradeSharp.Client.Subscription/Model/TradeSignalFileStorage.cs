using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Model
{
    /// <summary>
    /// сохраняет торговые сигналы в папке, получает сигналы из папки
    /// (читает файлы, определяет время обновления сигнала...)
    /// </summary>
    public class TradeSignalFileStorage
    {
        private static TradeSignalFileStorage instance;

        public static TradeSignalFileStorage Instance
        {
            get { return instance ?? (instance = new TradeSignalFileStorage()); }
        }

        private readonly ThreadSafeStorage<string, ReaderWriterLock> fileLocker =
            new ThreadSafeStorage<string, ReaderWriterLock>();

        private const int LockTimeout = 1000;

        private static readonly string signalFolder =
            ExecutablePath.ExecPath + "\\" + AppConfig.GetStringParam("SignalFolder", "TradeSignals");

        private TradeSignalFileStorage()
        {
            if (!Directory.Exists(signalFolder))
            {
                try
                {
                    Directory.CreateDirectory(signalFolder);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("TradeSignalFileStorage - невозможно создать каталог \"{0}\": {1}",
                        signalFolder, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// вернуть время обновления торг. сигнала (из файла, если есть таковой) или null
        /// </summary>        
        public TradeSignalUpdate FindTradeSignal(int category, string ticker, string timeframeStr)
        {
            var fileName = MakeFileName(category, ticker, timeframeStr);
            if (!File.Exists(fileName)) return null;
            var locker = fileLocker.ReceiveValue(fileName);
            if (locker == null)
            {
                locker = new ReaderWriterLock();
                fileLocker.UpdateValues(fileName, locker);
            }
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("TradeSignalFileStorage.FindTradeSignal() read lock timeout");
                return null;
            }
            try
            {
                // получить из документа время обновления
                DateTime timeUpdated;
                int objectsCount;
                if (!TradeSignalXml.GetSignalUpdateParamsFromFile(fileName, TradeSignalXml.DefaultEncoding,
                                                          out timeUpdated, out objectsCount)) return null;
                return new TradeSignalUpdate(category, ticker)
                    {
                        TimeUpdated = timeUpdated,
                        ObjectCount = objectsCount
                    };
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalFileStorage.FindTradeSignal() error", ex);
                return null;
            }
            finally
            {
                locker.ReleaseLock();
            }
        }

        /// <summary>
        /// сохранить торг. сигнал в файл
        /// </summary>
        public void SaveTradeSignal(int category, string ticker, string timeframeStr, string signalXml)
        {
            var fileName = MakeFileName(category, ticker, timeframeStr);
            var locker = fileLocker.ReceiveValue(fileName);
            if (locker == null)
            {
                locker = new ReaderWriterLock();
                fileLocker.UpdateValues(fileName, locker);
            }
            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("TradeSignalFileStorage.SaveTradeSignal() writer lock timeout");
                return;
            }
            try
            {
                // сохранить файл
                using (var sw = new StreamWriter(fileName, false, TradeSignalXml.DefaultEncoding))
                {
                    sw.Write(signalXml);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalFileStorage.SaveTradeSignal() error", ex);
                return;
            }
            finally
            {
                locker.ReleaseLock();
            }
        }

        /// <summary>
        /// прочитать торг. сигнал из файла
        /// </summary>        
        public string LoadTradeSignalXml(int category, string ticker, string timeframeStr)
        {
            var fileName = MakeFileName(category, ticker, timeframeStr);
            var locker = fileLocker.ReceiveValue(fileName);
            if (locker == null)
            {
                locker = new ReaderWriterLock();
                fileLocker.UpdateValues(fileName, locker);
            }
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("TradeSignalFileStorage.LoadTradeSignalXml() reader lock timeout");
                return string.Empty;
            }
            try
            {
                // загрузить из файла
                using (var sr = new StreamReader(fileName, TradeSignalXml.DefaultEncoding))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalFileStorage.LoadTradeSignalXml() error", ex);
                return string.Empty;
            }
            finally
            {
                locker.ReleaseLock();
            }
        }

        /// <summary>
        /// обертка над LoadTradeSignalXml
        /// </summary>
        public XmlDocument LoadTradeSignalDoc(int category, string ticker, string timeframeStr)
        {
            var strXml = LoadTradeSignalXml(category, ticker, timeframeStr);
            if (string.IsNullOrEmpty(strXml)) return null;
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(strXml);
                if (doc.DocumentElement == null ||
                    doc.DocumentElement.ChildNodes.Count == 0)
                {
                    Logger.ErrorFormat("TradeSignalFileStorage.LoadTradeSignalDoc({0}, {1}, {2}) - doc is empty",
                        category, ticker, timeframeStr);
                    return null;
                }
                return doc;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("TradeSignalFileStorage.LoadTradeSignalDoc({0}, {1}, {2}) - parsing error: {3}",
                        category, ticker, timeframeStr, ex);
                return null;
            }
        }

        public List<TradeSignalUpdate> GetAllTradeSignalUpdates()
        {
            var updates = new List<TradeSignalUpdate>();
            try
            {
                foreach (var filePath in Directory.GetFiles(signalFolder, "*_*_*.xml"))
                {
                    var update = ParseFileName(Path.GetFileNameWithoutExtension(filePath));
                    if (update == null) continue;

                    // прочитать время обновления из файла
                    // получить доступ на чтение файла
                    var locker = fileLocker.ReceiveValue(filePath);
                    if (locker == null)
                    {
                        locker = new ReaderWriterLock();
                        fileLocker.UpdateValues(filePath, locker);
                    }
                    try
                    {
                        locker.AcquireReaderLock(LockTimeout);
                    }
                    catch (ApplicationException)
                    {
                        Logger.Error("TradeSignalFileStorage.GetAllTradeSignalUpdates() reader lock timeout");
                        continue;
                    }
                    try
                    {
                        // загрузить из файла
                        DateTime updateDate;
                        int objectsCount;
                        if (TradeSignalXml.GetSignalUpdateParamsFromFile(filePath, TradeSignalXml.DefaultEncoding,
                            out updateDate, out objectsCount))
                        {
                            update.TimeUpdated = updateDate;
                            update.ObjectCount = objectsCount;
                            updates.Add(update);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("TradeSignalFileStorage.GetAllTradeSignalUpdates() error", ex);
                        continue;
                    }
                    finally
                    {
                        locker.ReleaseLock();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetAllTradeSignalUpdates()", ex);
                return null;
            }
            return updates;
        }

        private static string MakeFileName(int category, string ticker, string timeframeStr)
        {
            return signalFolder + "\\" + category + "_" + ticker + "_" + timeframeStr + ".xml";
        }

        private static TradeSignalUpdate ParseFileName(string fileName)
        {
            var parts = fileName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return null;
            var catId = parts[0].ToIntSafe();
            if (!catId.HasValue) return null;
            var ticker = parts[1];
            if (string.IsNullOrEmpty(ticker)) return null;
            if (!DalSpot.Instance.GetTickerNames().Contains(ticker)) return null;
            var timeframe = BarSettings.TryParseString(parts[2]);
            if (timeframe == null) return null;
            return new TradeSignalUpdate(catId.Value, ticker, timeframe, DateTime.Now);
        }
    }
}
