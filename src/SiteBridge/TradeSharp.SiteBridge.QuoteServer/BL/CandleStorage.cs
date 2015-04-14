using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.QuoteServer.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CandleStorage : ICandleStorage
    {
        private static CandleStorage instance;

        public static CandleStorage Instance
        {
            get { return instance ?? (instance = new CandleStorage()); }
        }

        private CandleStorage()
        {            
        }

        private volatile bool initializationIsCompleted;

        private readonly string quoteDir = ExecutablePath.ExecPath + "\\quotes";

        private readonly List<string> tickersToStore = new List<string>();

        private Dictionary<string, DateSpan> tickerFirstRecord;

        private readonly int daysInHistoryMax = AppConfig.GetIntParam("Days.History", 365);

        private TcpQuoteReceiver quoteReceiver;

        private readonly Dictionary<string, Dictionary<string, CandlePacker>> packers 
            = new Dictionary<string, Dictionary<string, CandlePacker>>();

        private readonly Dictionary<string, Dictionary<string, List<CandleData>>> candles
            = new Dictionary<string, Dictionary<string, List<CandleData>>>();

        private Dictionary<string, List<CandleData>> candlesM1;

        private readonly ThreadSafeTimeStamp lastTimeCandlesSaved = new ThreadSafeTimeStamp();

        private readonly int offsetHours = AppConfig.GetIntParam("Offset.Hours", 0);

        private readonly int flushCandlesIntervalSec = AppConfig.GetIntParam("FlushCandles.Seconds", 60);

        private string barSettingsCodeM1;

        private readonly ReaderWriterLockSlim lockCandles = new ReaderWriterLockSlim();

        private readonly ReaderWriterLockSlim lockCandlesM1 = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;
        
        /// <summary>
        /// подкачать из БД свечки m1 за указанный период по указанным тикерам
        /// </summary>
        public void Initialize()
        {
            GetTickersToLoad();
            
            // актуализировать историю свеч m1
            var thread = new Thread(() =>
                {
                    AtomCandleStorage.Instance.LoadFromFiles(quoteDir);
                    Logger.Info("Старт получения котировок от сервера");
                    GetDateRangeOnServer();
                    foreach (var ticker in tickersToStore)
                        ActualizeHistoryByTicker(ticker);

                    Logger.Info("Старт упаковки котировок в свечи");
                    InitCandlePackers();

                    Logger.Info("Подписка на получение котировок");
                    // подписаться на свежачок
                    quoteReceiver = new TcpQuoteReceiver();
                    quoteReceiver.OnQuotesReceived += OnQuotesReceived;

                    initializationIsCompleted = true;
                    Logger.Info("Иницииализация завершена");
                });
            thread.Start();
        }

        public void Stop()
        {
            quoteReceiver.OnQuotesReceived -= OnQuotesReceived;
            quoteReceiver.Stop();
        }

        #region ICandleStorage
        public string[] GetEnabledTickersAndTimeframes()
        {
            return new[]
                {
                    string.Join(" ", tickersToStore),
                    string.Join(" ", BarSettingsStorage.Instance.GetCollection().Select(s => 
                        BarSettingsStorage.Instance.GetBarSettingsFriendlyName(s)))
                };
        }

        public List<KosherCandle> GetCandlesSilentDateString(string timeframe,
                                                      string ticker, string timeStartStr)
        {
            DateTime timeStart;
            try
            {
                timeStart = DateTime.ParseExact(timeStartStr, "ddMMyyyyHHmmss", CultureProvider.Common);                    
            }
            catch
            {
                Logger.ErrorFormat("GetCandlesSilentDateString({0}) - недопустимый формат времени", timeStartStr);
                return new List<KosherCandle>();
            }
            return GetCandlesSilent(timeframe, ticker, timeStart);
        }

        public List<KosherCandle> GetCandlesSilent(string timeframe,
            string ticker, DateTime timeStart)
        {
            if (!initializationIsCompleted) return new List<KosherCandle>();

            try
            {
                string errorCode;
                var wobblerCandles =
                    GetWobblerCandles(timeframe, ticker, timeStart, out errorCode);
                Logger.InfoFormat("Sending {0} candles, error is: {1}",
                                  wobblerCandles.Count, errorCode);
                if (wobblerCandles.Count == 0) return new List<KosherCandle>();
                return wobblerCandles.Select(c => new KosherCandle(c)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetCandlesSilent error", ex);
                return new List<KosherCandle>();
            }
        }

        public List<KosherCandle> GetCandles(string timeframe,
            string ticker, DateTime timeStart, out string errorCode)
        {
            if (!initializationIsCompleted)
            {
                errorCode = "Not ready yet";
                return new List<KosherCandle>();
            }

            var wobblerCandles = GetWobblerCandles(timeframe, ticker, timeStart, out errorCode);
            Logger.InfoFormat("Sending {0} candles, error is: {1}",
                              wobblerCandles.Count, errorCode);
            if (wobblerCandles.Count == 0) return new List<KosherCandle>();
            return wobblerCandles.Select(c => new KosherCandle(c)).ToList();
        }
        #endregion

        #region Получение котировок по запросу
        private List<CandleData> GetWobblerCandles(string timeframe, 
            string ticker, DateTime timeStart, out string errorCode)
        {
            var barSets = BarSettingsStorage.Instance.GetBarSettingsByName(timeframe);
            if (barSets == null)
            {
                errorCode = "timeframe string was not recognized";
                return new List<CandleData>();
            }

            if (!tickersToStore.Contains(ticker))
            {
                errorCode = "ticker is not stored";
                return new List<CandleData>();
            }

            errorCode = string.Empty;
            timeframe = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(barSets);

            if (!lockCandles.TryEnterReadLock(LockTimeout))
            {
                errorCode = "internal error: timeout";
                return new List<CandleData>();
            }
            try
            {
                // получить свечи из списка или слепить их из свечек m1
                Dictionary<string, List<CandleData>> candleDic;
                if (!candles.TryGetValue(timeframe, out candleDic))
                    return MakeCandlesFromM1(barSets, ticker, timeStart);
                return candleDic[ticker].Where(c => c.timeOpen >= timeStart).ToList();
            }
            catch (Exception ex)
            {
                errorCode = ex.Message;
                return new List<CandleData>();
            }
            finally
            {
                lockCandles.ExitReadLock();
            }
        }

        private List<CandleData> MakeCandlesFromM1(BarSettings timeframe, string ticker, DateTime timeStart)
        {
            var result = new List<CandleData>();
            var minutes = candles[barSettingsCodeM1][ticker];
            var packer = new CandlePacker(timeframe);
            
            foreach (var canM1 in minutes)
            {
                var candle = packer.UpdateCandle(canM1);
                if (candle != null)
                    result.Add(candle);
            }

            return result.Where(c => c.timeOpen >= timeStart).ToList();
        }
        #endregion

        #region Обработка "свежих" котировок
        private void OnQuotesReceived(string[] names, QuoteData[] quotes)
        {
            var nowTime = DateTime.Now.AddDays(offsetHours);

            // лок на запись в словарь свечей
            if (!lockCandles.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("OnQuotesReceived - таймаут записи");
                return;
            }

            try
            {
                if (!lockCandlesM1.TryEnterWriteLock(LockTimeout))
                {
                    Logger.Error("OnQuotesReceived - таймаут записи (m1)");
                    return;
                }

                try
                {
                    // обновить свечи
                    foreach (var packerDic in packers)
                    {
                        var timeframeCode = packerDic.Key;
                        var isM1 = timeframeCode == barSettingsCodeM1;

                        for (var i = 0; i < names.Length; i++)
                        {
                            var name = names[i];
                            CandlePacker packer;
                            packerDic.Value.TryGetValue(name, out packer);
                            if (packer == null) continue;
                            var newCandle = packer.UpdateCandle(quotes[i].bid, nowTime);
                            if (newCandle != null)
                            {
                                candles[timeframeCode][name].Add(newCandle);
                                if (isM1) candlesM1[name].Add(newCandle);
                            }
                        }
                    }
                }
                finally
                {
                    lockCandlesM1.ExitWriteLock();
                }
            }
            finally
            {
                lockCandles.ExitWriteLock();
            }

            // если пришло время выгрузить m1 в файл... выгрузить
            var timeSaved = lastTimeCandlesSaved.GetLastHitIfHitted();
            if (timeSaved.HasValue)
            {    
                if ((DateTime.Now - timeSaved.Value).TotalSeconds > flushCandlesIntervalSec)
                {
                    FlushCandlesInFiles(); // таки выгрузить
                    lastTimeCandlesSaved.Touch();
                }
            }
            else lastTimeCandlesSaved.Touch();
        }

        private void InitCandlePackers()
        {
            var barSetsM1 = new BarSettings {Intervals = new List<int> {1}};
            barSettingsCodeM1 = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(barSetsM1);
            packers.Add(barSettingsCodeM1, tickersToStore.ToDictionary(t => t, t => new CandlePacker(barSetsM1)));
            candles.Add(barSettingsCodeM1, tickersToStore.ToDictionary(t => t, t => new List<CandleData>()));
            candlesM1 = tickersToStore.ToDictionary(t => t, t => new List<CandleData>());

            // прочитать из настроек, какие свечи нужны
            var barSetsStr = AppConfig.GetStringParam("Candles", "H1 H4 D1");
            var parts = barSetsStr.Split(new[] {' ', ',', '.'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part == barSettingsCodeM1) continue;
                var sets = BarSettingsStorage.Instance.GetBarSettingsByName(part);
                if (ReferenceEquals(sets, null))
                {
                    Logger.ErrorFormat("InitCandlePackers - таймфрейм {0} не распознан", part);
                    continue;
                }
                var nameFriendly = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(sets);
                packers.Add(nameFriendly, tickersToStore.ToDictionary(t => t, t => new CandlePacker(sets)));
                candles.Add(nameFriendly, tickersToStore.ToDictionary(t => t, t => new List<CandleData>()));
            }

            // по каждому пакеру слепить свечи из минуток
            Logger.InfoFormat("Формировать свечи для {0} тикеров, {1} таймфреймов",
                tickersToStore.Count, packers.Count);
            foreach (var ticker in tickersToStore)
            {
                var atomCandles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker) ?? new List<CandleData>();
                
                foreach (var cm1 in atomCandles)
                foreach (var packTimeframe in packers)
                {
                    var packer = packTimeframe.Value[ticker];
                    if (ReferenceEquals(packer, null))
                    {
                        Logger.ErrorFormat("packer({0}, {1}) = null", ticker, packTimeframe.Key);
                        continue;
                    }
                    if (!candles.ContainsKey(packTimeframe.Key))
                    {
                        Logger.ErrorFormat("!candles.ContainsKey({0})", packTimeframe.Key);
                        continue;
                    }
                    if (!candles[packTimeframe.Key].ContainsKey(ticker))
                    {
                        Logger.ErrorFormat("!candles[{0}].ContainsKey({1})",
                            packTimeframe.Key, ticker);
                        continue;
                    }
                    
                    // добавить минутную свечу как есть
                    if (packer.BarSettings.IsAtomic)
                    {
                        candles[packTimeframe.Key][ticker].Add(cm1);
                        continue;
                    }

                    var candle = packer.UpdateCandle(cm1);
                    if (candle == null) continue;
                    candles[packTimeframe.Key][ticker].Add(candle);
                }
            }
        }
        
        private void FlushCandlesInFiles()
        {
            var symbols = new List<string>();
            var symbolCandles = new List<List<CandleData>>();
            
            if (!lockCandlesM1.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("FlushCandlesInFiles - таймаут чтения");
                return;
            }

            // прочитать свечи m1
            try
            {
                foreach (var list in candlesM1)
                {
                    if (list.Value.Count == 0) continue;
                    symbols.Add(list.Key);
                    symbolCandles.Add(list.Value.ToList());
                }
            }
            finally
            {
                lockCandlesM1.ExitReadLock();
            }

            AtomCandleStorage.Instance.AddMinuteCandles(symbols, symbolCandles);

            // очистить список свечей m1
            if (!lockCandlesM1.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("FlushCandlesInFiles - таймаут записи");
                return;
            }
            try
            {
                foreach (var list in candlesM1)
                {
                    list.Value.Clear();
                }
            }
            finally
            {
                lockCandlesM1.ExitWriteLock();
            }

            // записать на диск
            AtomCandleStorage.Instance.FlushInFiles(quoteDir);
        }
        #endregion

        #region Получение имен инструментов

        private void GetTickersToLoad()
        {
            var tickersToInclude = AppConfig.GetStringParam("IncludeTickers", "^.+USD ^USD.+").Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(MakePatternRegex).ToList();
            var tickersToExclude = AppConfig.GetStringParam("ExcludeTickers", "RUB TRY SEK").Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(MakePatternRegex).ToList();

            var allTickers = DalSpot.Instance.GetTickerNames();
            foreach (var name in allTickers)
            {
                if (tickersToInclude.All(r => r.IsMatch(name)))
                    if (!tickersToExclude.Any(r => r.IsMatch(name))) 
                        tickersToStore.Add(name);
            }

            Logger.InfoFormat("Хранит котировки {0} из {1} инструментов", tickersToStore.Count, allTickers.Length);
        }

        private static Regex MakePatternRegex(string pattern)
        {
            try
            {
                return new Regex(pattern, RegexOptions.IgnoreCase);                
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка разбора выражения \"" + pattern + "\"", ex);
                throw;
            }
        }
        #endregion

        #region Закачка котировок
        private void GetDateRangeOnServer()
        {
            try
            {
                IQuoteStorage quoteStorage;
                try
                {
                    quoteStorage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
                }
                catch (Exception)
                {
                    Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
                    throw;
                }

                tickerFirstRecord = quoteStorage.GetTickersHistoryStarts();
            }
            catch (Exception ex)
            {
                Logger.Error("GetDateRangeOnServer() - ошибка получения истории", ex);
                throw;
            }
        }

        private void ActualizeHistoryByTicker(string ticker)
        {
            DateSpan serverRange;
            if (!tickerFirstRecord.TryGetValue(ticker, out serverRange)) return;
            var queryStart = serverRange.start;
            var timeStart = DateTime.Now.AddDays(-daysInHistoryMax);
            if (queryStart < timeStart)
                queryStart = timeStart;
            
            var rangeHist = AtomCandleStorage.Instance.GetDataRange(ticker);
            var spansToLoad = new List<DateSpan>();
            if (!rangeHist.HasValue)
                spansToLoad.Add(new DateSpan(queryStart, DateTime.Now));
            else
            {
                // актуализировать начало и конец интервала
                // начало...
                if ((rangeHist.Value.a - queryStart).TotalMinutes > 60)
                    spansToLoad.Add(new DateSpan(queryStart, rangeHist.Value.a));
                // конец...
                if ((DateTime.Now - rangeHist.Value.b).TotalMinutes > 5)
                    spansToLoad.Add(new DateSpan(rangeHist.Value.b, DateTime.Now));
            }

            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker) ?? new List<CandleData>();
            var oldCount = candles.Count;

            foreach (var span in spansToLoad)
                LoadHistoryFromServer(ticker, candles, span);

            if (candles.Count == oldCount) return;

            // обновить хранилище
            AtomCandleStorage.Instance.RewriteCandles(ticker, candles);
            AtomCandleStorage.Instance.FlushInFiles(quoteDir);
        }

        private void LoadHistoryFromServer(string ticker, List<CandleData> candles, DateSpan range)
        {
            var loadedList = new List<CandleData>();
            var pointCost = DalSpot.Instance.GetPrecision10(ticker);

            try
            {
                IQuoteStorage quoteStorage;
                try
                {
                    quoteStorage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
                }
                catch (Exception)
                {
                    Logger.Error("LoadHistoryFromServer - связь с сервером (IQuoteStorageBinding) не установлена");
                    throw;
                }

                for (var start = range.start.AddMinutes(1); start < range.end;)
                {
                    var end = start.AddDays(10);
                    if (end > range.end)
                        end = range.end;
                    else
                    {
                        if ((range.end - end).TotalDays < 2)
                            end = range.end;
                    }

                    var candlesPacked = quoteStorage.GetMinuteCandlesPacked(ticker, start, end);
                    start = end.AddMinutes(1);

                    if (candlesPacked == null || candlesPacked.count == 0) continue;

                    // добавить свечи в список закачанных
                    loadedList.AddRange(candlesPacked.GetCandles().Select(c => new CandleData(c, pointCost)));
                }

                if (loadedList.Count == 0) return;

                // добавить свечи в общий список
                if (candles.Count == 0 || candles[candles.Count - 1].timeOpen < loadedList[0].timeOpen)
                {
                    candles.AddRange(loadedList);
                    return;
                }
                candles.InsertRange(0, loadedList);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в LoadHistoryFromServer (" + ticker + ")", ex);
                throw;
            }
        }
        #endregion
    }
}
