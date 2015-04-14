using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TradeSharp.Util;

namespace Entity
{
    public class AtomCandleStorage
    {
        #region Singleton

        private static readonly Lazy<AtomCandleStorage> instance =
            new Lazy<AtomCandleStorage>(() => new AtomCandleStorage());

        public static AtomCandleStorage Instance
        {
            get { return instance.Value; }
        }

        private AtomCandleStorage()
        {
        }

        #endregion

        #region Данные

        private readonly Dictionary<string, ThreadSafeCandleList> candles = new Dictionary<string, ThreadSafeCandleList>();

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int LogMsgEnsureListReadTimout = 1;

        private const int LogMsgEnsureListWriteTimout = 2;

        #endregion

        #region Открытые методы

        public void AddMinuteCandles(List<string> symbols, List<List<CandleData>> candlesForSymbols)
        {
            for (var i = 0; i < symbols.Count; i++)
            {
                var list = EnsureList(symbols[i]);
                if (list == null) continue;
                // добавить котировки (свечки) в потокобезопасный список
                list.AddCandles(candlesForSymbols[i]);
            }
        }

        public List<CandleData> GetAllMinuteCandles(string symbol)
        {
            var list = TryGetList(symbol);
            if (list == null) return null;
            return list.GetAllCandles();
        }

        public List<CandleData> GetAllMinuteCandles(string symbol, DateTime start, DateTime end)
        {
            var list = TryGetList(symbol);
            if (list == null) return null;
            return list.GetCandlesOnRange(start, end);
        }

        public void RewriteCandles(string symbol, List<CandleData> newCandles)
        {
            var list = EnsureList(symbol);
            if (list == null) return;
            list.RewriteCandles(newCandles);
        }

        public void FlushInFiles(string quoteFilesDir)
        {
            // получить копию словаря
            if (!locker.TryEnterReadLock(LockTimeout)) return;
            Dictionary<string, ThreadSafeCandleList> deepCopyCandles;
            try
            {
                deepCopyCandles = candles.ToDictionary(p => p.Key, p => p.Value);
            }
            finally
            {
                locker.ExitReadLock();
            }

            try
            {
                Directory.CreateDirectory(quoteFilesDir);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в AtomCandleStorage.FlushInFiles()", ex);
            }

            // по каждому тикеру - сохранить свечки в файл
            foreach (var pair in deepCopyCandles)
            {
                var symbol = pair.Key;
                var list = pair.Value;
                var candlesByTicker = list.GetAllCandles();
                if (candlesByTicker == null || deepCopyCandles.Count == 0) return;
                var path = quoteFilesDir + "\\" + symbol + ".quote";

                try
                {
                    CandleData.SaveInFile(path, symbol, candlesByTicker);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в AtomCandleStorage.FlushInFiles({0}): {1}", symbol, ex);
                }
            }
        }

        public void FlushInFile(string quoteFilesDir, string symbol)
        {
            // получить копию словаря
            if (!locker.TryEnterReadLock(LockTimeout)) return;
            ThreadSafeCandleList candlesBySymbol;
            try
            {
                if (!candles.TryGetValue(symbol, out candlesBySymbol)) return;
            }
            finally
            {
                locker.ExitReadLock();
            }

            var lstCandles = candlesBySymbol.GetAllCandles();
            if (lstCandles == null || lstCandles.Count == 0) return;

            // по каждому тикеру - сохранить свечки в файл
            var path = quoteFilesDir + "\\" + symbol + ".quote";

            try
            {
                Directory.CreateDirectory(quoteFilesDir);
                CandleData.SaveInFile(path, symbol, lstCandles);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в AtomCandleStorage.FlushInFile({0}): {1}", symbol, ex);
            }            
        }

        public void LoadFromFiles(string quoteDir)
        {
            foreach (var fileName in Directory.GetFiles(quoteDir, "*.quote"))
            {
                // получить название торгуемого актива
                var symbol = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrEmpty(symbol))
                    continue;
                symbol = symbol.ToUpper();

                // прочитать файл
                List<CandleData> fileCandles;
                try
                {
                    fileCandles = CandleData.LoadFromFile(fileName, symbol);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в AtomCandleStorage.LoadFromFiles({0}): {1}",
                        symbol, ex);
                    continue;
                }
                if (fileCandles == null || fileCandles.Count == 0) continue;

                if (!locker.TryEnterWriteLock(LockTimeout))
                {
                    Logger.ErrorFormat("Ошибка в AtomCandleStorage.LoadFromFiles({0}): невозможно получить доступ на запись",
                        symbol);
                    continue;
                }

                // записать в словарь
                try
                {
                    if (candles.ContainsKey(symbol)) 
                        candles [symbol] = new ThreadSafeCandleList(fileCandles);
                    else
                        candles.Add(symbol, new ThreadSafeCandleList(fileCandles));
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public Cortege2<DateTime, DateTime>? LoadFromFile(string quoteDir, string symbol)
        {
            if (string.IsNullOrEmpty(quoteDir))
                throw new ArgumentException("LoadFromFile - директория пуста", "quoteDir");
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("LoadFromFile - котировка не указана", "symbol");

            var fileName = quoteDir + "\\" + symbol + ".quote";
            if (!File.Exists(fileName)) return null;
            // прочитать файл
            List<CandleData> fileCandles;
            try
            {
                fileCandles = CandleData.LoadFromFile(fileName, symbol);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в AtomCandleStorage.LoadFromFile({0}): {1}",
                    symbol, ex);
                return null;
            }

            if (fileCandles == null || fileCandles.Count == 0) return null;
            
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.ErrorFormat("Ошибка в AtomCandleStorage.LoadFromFile({0}): невозможно получить доступ на запись",
                    symbol);
                return null;
            }

            // записать в словарь
            var dates = new Cortege2<DateTime, DateTime>(fileCandles[0].timeOpen, fileCandles[fileCandles.Count - 1].timeOpen);
            try
            {
                if (candles.ContainsKey(symbol))
                    candles[symbol] = new ThreadSafeCandleList(fileCandles);
                else
                    candles.Add(symbol, new ThreadSafeCandleList(fileCandles));
            }
            finally
            {
                locker.ExitWriteLock();
            }
            return dates;
        }

        public void ClearHistory(string ticker, string quoteFilesDir)
        {
            // очистить в словаре
            if (!locker.TryEnterWriteLock(LockTimeout)) return;
            try
            {
                if (candles.ContainsKey(ticker))
                    candles.Remove(ticker);
            }
            finally 
            {
                locker.ExitWriteLock();
            }

            // удалить файл
            try
            {
                var path = quoteFilesDir.TrimEnd('\\') + "\\" + ticker + ".quote";
                if (!File.Exists(path)) return;
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в AtomCandleStorage.ClearHistory() - удаление файла", ex);
            }
        }

        public Dictionary<string, Cortege2<DateTime, DateTime>> GetDataRange()
        {
            var dic = new Dictionary<string, Cortege2<DateTime, DateTime>>();
            if (!locker.TryEnterReadLock(LockTimeout)) return dic;

            try
            {
                foreach (var pair in candles)
                {
                    var range = pair.Value.GetCandlesDataRange();
                    if (range.HasValue)
                        dic.Add(pair.Key, range.Value);
                }
            }
            finally
            {
                locker.ExitReadLock();
            }

            return dic;
        }

        public Cortege2<DateTime, DateTime>? GetDataRange(string symbol)
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;

            try
            {
                ThreadSafeCandleList list;
                candles.TryGetValue(symbol, out list);
                return list == null ? null : list.GetCandlesDataRange();
            }
            finally
            {
                locker.ExitReadLock();
            }            
        }

        /*public DateTime? GetFirstGapTimeStart(string symbol, int candlesFromEnd = 1440 * 5)
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                ThreadSafeCandleList list;
                candles.TryGetValue(symbol, out list);
                if (list == null) return null;
                return list.GetFirstGapStartTime(candlesFromEnd);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }*/

        #endregion

        #region Закрытые методы

        private ThreadSafeCandleList EnsureList(string symbol)
        {
            if (!locker.TryEnterUpgradeableReadLock(LockTimeout))
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgEnsureListReadTimout, 1000 * 60 * 15,
                    "AtomCandleStorage - EnsureList() read timeout");
                return null;
            }
            try
            {
                ThreadSafeCandleList list;
                if (candles.TryGetValue(symbol, out list)) return list;

                if (!locker.TryEnterWriteLock(LockTimeout))
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgEnsureListWriteTimout, 1000 * 60 * 15,
                        "AtomCandleStorage - EnsureList() write timeout");
                    return null;
                }
                try
                {
                    list = new ThreadSafeCandleList();
                    candles.Add(symbol, list);
                    return list;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        private ThreadSafeCandleList TryGetList(string symbol)
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                ThreadSafeCandleList list;
                candles.TryGetValue(symbol, out list);
                return list;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        #endregion

        #region Статические методы
        public static Dictionary<string, Cortege2<DateTime, DateTime>> GetFileDateRange(string quoteFolderPath)
        {
            var range = new Dictionary<string, Cortege2<DateTime, DateTime>>();
            var allSymbols = DalSpot.Instance.GetTickerNames();

            foreach (var fileName in Directory.GetFiles(quoteFolderPath, "*.quote"))
            {
                // да с какого хрена у файла не будет имени?
                // ReSharper disable PossibleNullReferenceException
                var symbol = Path.GetFileNameWithoutExtension(fileName).ToUpper();
                // ReSharper restore PossibleNullReferenceException
                if (!allSymbols.Contains(symbol)) continue;
                
                DateTime? start, end;
                bool someUnusedFlag;
                try
                {
                    QuoteCacheManager.GetFirstAndLastFileDates(fileName, out start, out end, out someUnusedFlag);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в AtomCandleStorage.GetFileDateRange(), {0}: {1}",
                        fileName, ex);
                    continue;
                }
                
                if (start.HasValue && end.HasValue)
                    range.Add(symbol, new Cortege2<DateTime, DateTime>(start.Value, end.Value));
            }

            return range;
        }        
        #endregion
    }
}
