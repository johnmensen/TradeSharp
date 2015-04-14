using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    public static class FileGapActualizator
    {
        private static readonly int daysInQuoteServerRequest = AppConfig.GetIntParam("DaysInQuoteServerRequest", 3);

        // private static readonly int minutesToStickGaps = AppConfig.GetIntParam("minutesToStickGaps", 240);

        public volatile static bool currentTickerCancelled;

        /// <summary>
        /// получение карты потенциальных гэпов;
        /// она составляются по отсутвующим данным в котировке ticker-а,
        /// но учитывает карту гэпов сервера, составленную по предыдущим запросам
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="timeStart"></param>
        /// <param name="quoteCacheFolder"></param>
        /// <param name="worker"></param>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        public static List<GapInfo> VerifyTickerHistory(string ticker, DateTime timeStart,
                                                        string quoteCacheFolder,
                                                        BackgroundWorker worker,
                                                        DateTime nowTime = default(DateTime))
        {
            if (worker != null && worker.CancellationPending) return new List<GapInfo>();
            if (nowTime == default(DateTime))
                nowTime = DateTime.Now;

            var histStart = GapMap.Instance.GetServerTickerHistoryStart(ticker);
            if (timeStart < histStart)
                timeStart = histStart;

            // получить список гэпов, предположительно, имеющихся на сервере
            var serverGaps = GapMap.Instance.GetServerGaps(ticker);

            // получить имеющуюся историю
            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker);
            if (candles == null || candles.Count == 0)
            {
                var loadedHist = AtomCandleStorage.Instance.LoadFromFile(quoteCacheFolder, ticker);
                if (loadedHist.HasValue)
                    candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker);
            }
            candles = candles ?? new List<CandleData>();

            if (worker != null && worker.CancellationPending)
                return new List<GapInfo>();

            // определить гэпы
            var gaps = candles.Count > 0
                           ? QuoteCacheManager.GetGaps(candles, timeStart, nowTime)
                           : new List<DateSpan> {new DateSpan(timeStart, nowTime)};
            // исключить фрагменты, отсутствующие на сервере
            if (serverGaps != null && serverGaps.IsActual)
                GapMap.ExcludeGapsThatLackedOnServer(ref gaps, serverGaps.serverGaps);
            else
            {
                // получить время первой записи в БД и удалить / порезать все гэпы, начинающиеся
                // позже, чем первая запись базы
                var gapsTrimmed = new List<DateSpan>();

                foreach (var gap in gaps)
                {
                    if (gap.end <= histStart) continue;
                    if (gap.start < histStart)
                        gapsTrimmed.Add(new DateSpan(histStart, gap.end));
                    else
                        gapsTrimmed.Add(gap);
                }

                gaps = gapsTrimmed;
            }

            var gapInfos = gaps.Select(g => new GapInfo { start = g.start, end = g.end }).ToList();

            // склеиваем рядом стоящие гэпы
            GapInfo.StickSmallGaps(ref gapInfos, 30);

            if (worker != null && worker.CancellationPending)
                return new List<GapInfo>();

            return gapInfos;
        }

        /// <summary>
        /// устранение гэпов
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="startTime"></param>
        /// <param name="gaps"></param>
        /// <param name="quoteCacheFolder"></param>
        /// <param name="worker"></param>
        /// <param name="gapsUpdatedAction"></param>
        public static void FillGapsByTicker(string ticker,
                                            DateTime startTime,
                                            List<GapInfo> gaps,
                                            string quoteCacheFolder,
                                            BackgroundWorker worker,
                                            Action<string, List<GapInfo>> gapsUpdatedAction)
        {
            // закачать с сервера
            // параллельно обновляя контрол
            var candlesOld = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker) ?? new List<CandleData>();
            var candlesNew = new List<CandleData>();
            var oldIndex = 0;

            if (worker != null && worker.CancellationPending) return;

            for (var i = 0; i < gaps.Count; i++)
            {
                var gap = gaps[i];
                // добавить имеющиеся свечи
                for (; oldIndex < candlesOld.Count; oldIndex++)
                {
                    if (candlesOld[oldIndex].timeOpen > gap.start) break;
                    candlesNew.Add(candlesOld[oldIndex]);
                }

                // подкачать свечи с сервера
                var indexToInsert = i;
                var wasInserted = false;

                var status = LoadQuotesFromServer(ticker, gap.start, gap.end.AddMinutes(1),
                    candlesNew,
                    (time, gapStatus) =>
                    {
                        // добавить или обновить временный гэп
                        var passedGap = new GapInfo { start = gap.start, end = time, status = gapStatus };
                        if (!wasInserted)
                        {
                            gaps.Insert(indexToInsert, passedGap);
                            wasInserted = true;
                        }
                        else
                            gaps[indexToInsert] = passedGap;
                        gaps[indexToInsert + 1] = new GapInfo
                        {
                            start = time.AddMinutes(1),
                            end = gaps[indexToInsert + 1].end,
                            status = gaps[indexToInsert + 1].status
                        };
                        // перерисовать контрол
                        gapsUpdatedAction(ticker, gaps);
                    },
                    worker);
                if (wasInserted)
                    gaps.RemoveAt(indexToInsert);

                if (status == GapInfo.GapStatus.FailedToFill)
                {
                    // в старом списке могли быть какие-то котировки на интервале гэпа - добавить их
                    for (; oldIndex < candlesOld.Count; oldIndex++)
                    {
                        if (candlesOld[oldIndex].timeOpen > gap.end) break;
                        candlesNew.Add(candlesOld[oldIndex]);
                    }
                }

                // перекрасить гэп другим цветом
                gaps[i] = new GapInfo
                {
                    start = gap.start,
                    end = gap.end,
                    status = status
                };
                if (worker != null && worker.CancellationPending) return;
                if (currentTickerCancelled)
                {
                    currentTickerCancelled = false;
                    return;
                }

                gapsUpdatedAction(ticker, gaps);

                // сместиться вперед в списке старых свеч
                for (; oldIndex < candlesOld.Count; oldIndex++)
                {
                    if (candlesOld[oldIndex].timeOpen > gap.end) break;
                }
            }

            if (worker != null && worker.CancellationPending) return;

            if (candlesNew.Count == 0) return;

            // добавить "хвостик" - недостающие свечки
            var lastTime = candlesNew[candlesNew.Count - 1].timeOpen;
            for (; oldIndex < candlesOld.Count; oldIndex++)
            {
                if (candlesOld[oldIndex].timeOpen <= lastTime) continue;
                candlesNew.Add(candlesOld[oldIndex]);
            }

            if (candlesNew.Count <= candlesOld.Count)
                return;

            // убрать дублирующиеся свечки
            for (var i = 1; i < candlesNew.Count; i++)
            {
                if (candlesNew[i].timeOpen <= candlesNew[i - 1].timeOpen)
                {
                    candlesNew.RemoveAt(i);
                    i--;
                }
            }

            if (worker != null && worker.CancellationPending) return;
            // дырки, оставшиеся в истории, и есть серверные гэпы
            if (candlesNew.Count > 0)
            {
                var updatedGapsInfo = QuoteCacheManager.GetGaps(candlesNew, startTime, DateTime.Now);
                GapMap.Instance.UpdateGaps(ticker, updatedGapsInfo);
                GapMap.Instance.SaveToFile();
            }
            if (worker != null && worker.CancellationPending) return;

            // сохранить свечи в хранилище и в файл
            AtomCandleStorage.Instance.RewriteCandles(ticker, candlesNew);
            AtomCandleStorage.Instance.FlushInFile(quoteCacheFolder, ticker);
        }

        public static void FillGapsByTickerFast(string ticker,
                                                DateTime? startTime,
                                                DateTime? endTime,
                                                List<GapInfo> gaps,
                                                string quoteCacheFolder,
                                                BackgroundWorker worker,
                                                Action<string, List<GapInfo>> gapsUpdatedAction)
        {
            if (gaps == null || gaps.Count == 0)
                return;

            // старые свечи
            var candlesOld = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker) ?? new List<CandleData>();
            var visualGaps = new List<GapInfo>();

            if (worker != null && worker.CancellationPending)
                return;

            // корректируем интервал запроса
            var requestGaps = new List<GapInfo>();
            var histStart = GapMap.Instance.GetServerTickerHistoryStart(ticker);

            var startFillTime = startTime.HasValue
                                    ? (startTime.Value < histStart ? histStart : startTime.Value)
                                    : histStart;
            var endFillTime = endTime.HasValue ? endTime.Value : DateTime.Now;
            if (startFillTime > endFillTime)
                return;

            // отсекаем части гэпов или гэпы целиком, находящиеся за пределами интервала запроса
            foreach (var gap in gaps)
            {
                if (gap.end < startFillTime || gap.start > endFillTime)
                {
                    visualGaps.Add(new GapInfo
                        {
                            start = gap.start,
                            end = gap.end,
                            status = GapInfo.GapStatus.FailedToFill
                        });
                    continue;
                }
                var start = gap.start;
                var end = gap.end;
                if (start < startFillTime)
                {
                    visualGaps.Add(new GapInfo
                    {
                        start = gap.start,
                        end = startFillTime.AddMinutes(-1),
                        status = GapInfo.GapStatus.FailedToFill
                    });
                    start = startFillTime;
                }
                if (end > endFillTime)
                {
                    visualGaps.Add(new GapInfo
                    {
                        start = endFillTime.AddMinutes(1),
                        end = gap.end,
                        status = GapInfo.GapStatus.FailedToFill
                    });
                    end = endFillTime;
                }
                requestGaps.Add(new GapInfo {start = start, end = end});
                visualGaps.Add(new GapInfo
                {
                    start = start,
                    end = end
                });
            }

            // группируем - составляем запросы
            var gapLists = GapList.CreateGapList(requestGaps, daysInQuoteServerRequest);

            // обновляем контрол
            gapsUpdatedAction(ticker, visualGaps);

            if (gapLists.Count == 0 || gapLists.First().Gaps.Count == 0)
                return;

            // в результат добавляем старые начальные свечи, не вошедшие в запрос
            var candlesOldIndex = 0;
            var candlesNew = new List<CandleData>(); // результат - обновленные свечи
            var nextGap = gapLists.First().Gaps.First();
            for (; candlesOldIndex < candlesOld.Count; candlesOldIndex++)
            {
                if (candlesOld[candlesOldIndex].timeOpen > nextGap.start)
                    break;
                candlesNew.Add(candlesOld[candlesOldIndex]);
            }

            // отправляем запросы, параллельно обновляя контрол
            for (var gapListIndex = 0; gapListIndex < gapLists.Count; gapListIndex++)
            {
                // проверяем на завершение операции
                if (worker != null && worker.CancellationPending)
                    return;
                if (currentTickerCancelled)
                {
                    currentTickerCancelled = false;
                    return;
                }

                var gapList = gapLists[gapListIndex];
                if (gapList.Gaps.Count == 0)
                    continue;
                var ok = LoadQuotesFromServerFast(ticker, gapList, candlesNew);

                // добавляем старые свечи, встретившиеся между гэпами (после gapIndex до gapIndex + 1)
                DateTime lastTime;
                for (var gapIndex = 0; gapIndex < gapList.Gaps.Count - 1; gapIndex++)
                {
                    lastTime = gapList.Gaps[gapIndex].end;
                    var nextTime = gapList.Gaps[gapIndex + 1].start;
                    var candlesOldFoundIndexForGap = candlesOld.FindIndex(candlesOldIndex, c => c.timeOpen > lastTime);
                    if (candlesOldFoundIndexForGap == -1)
                        continue;
                    var insertPosition = candlesNew.FindIndex(c => c.timeOpen >= nextTime);
                    if (insertPosition == -1)
                        continue;
                    var candlesOldForGap = new List<CandleData>();
                    for (; candlesOldFoundIndexForGap < candlesOld.Count; candlesOldFoundIndexForGap++)
                    {
                        if (candlesOld[candlesOldFoundIndexForGap].timeOpen >= nextTime)
                            break;
                        candlesOldForGap.Add(candlesOld[candlesOldFoundIndexForGap]);
                    }
                    candlesOldIndex = candlesOldFoundIndexForGap;
                    candlesNew.InsertRange(insertPosition, candlesOldForGap);
                }

                // обновляем контрол
                foreach (var gap in gapList.Gaps)
                {
                    var thisGap = gap;
                    visualGaps.RemoveByPredicate(g => g.start == thisGap.start && g.end == thisGap.end, true);
                    visualGaps.Add(new GapInfo
                        {
                            start = gap.start,
                            end = gap.end,
                            status = ok ? GapInfo.GapStatus.Filled : GapInfo.GapStatus.FailedToFill
                        });
                }
                gapsUpdatedAction(ticker, visualGaps);

                // добавляем старые свечи, встретившиеся между запросами
                lastTime = gapList.Gaps.Last().end;
                var candlesOldFoundIndex = candlesOld.FindIndex(candlesOldIndex, c => c.timeOpen > lastTime);
                if (candlesOldFoundIndex == -1)
                    continue;
                candlesOldIndex = candlesOldFoundIndex;
                // если есть еще гэп, то добавляем свечки до него
                if (gapListIndex + 1 >= gapLists.Count)
                    continue;
                nextGap = gapLists[gapListIndex + 1].Gaps.First();
                for (; candlesOldIndex < candlesOld.Count; candlesOldIndex++)
                {
                    if (candlesOld[candlesOldIndex].timeOpen >= nextGap.start)
                        break;
                    candlesNew.Add(candlesOld[candlesOldIndex]);
                }
            }

            // сохраняем результат
            // сохраняем карту гэпов
            // дырки, оставшиеся в истории, и есть серверные гэпы
            if (candlesNew.Count > 0)
            {
                var updatedGapsInfo = QuoteCacheManager.GetGaps(candlesNew, startFillTime, endFillTime);
                GapMap.Instance.UpdateGaps(ticker, updatedGapsInfo);
                GapMap.Instance.SaveToFile();
            }
            // сохранить свечи в хранилище и в файл
            AtomCandleStorage.Instance.RewriteCandles(ticker, candlesNew);
            AtomCandleStorage.Instance.FlushInFile(quoteCacheFolder, ticker);
        }

        private static GapInfo.GapStatus LoadQuotesFromServer(string ticker, DateTime start, DateTime end,
                                                              List<CandleData> candles,
                                                              Action<DateTime, GapInfo.GapStatus> onPartLoaded,
                                                              BackgroundWorker worker)
        {
            var histStart = GapMap.Instance.GetServerTickerHistoryStart(ticker);
            if (start < histStart)
                start = histStart;
            // данные за этот период вообще не хранятся на сервере
            if (start > end)
            {
                onPartLoaded(end, GapInfo.GapStatus.FailedToFill);
                return GapInfo.GapStatus.FailedToFill;
            }

            const int minMinutesInQuoteRequest = 60 * 6;
            int numOk = 0, numFailed = 0;

            for (var time = start; time <= end; time = time.AddDays(daysInQuoteServerRequest))
            {
                // получить время окончания запроса
                var requestEnd = time.AddDays(daysInQuoteServerRequest);
                if (requestEnd > end) requestEnd = end;
                else
                {
                    var deltaEnd = (end - requestEnd).TotalMinutes;
                    if (deltaEnd < minMinutesInQuoteRequest)
                        requestEnd = end;
                }
                if ((worker != null && worker.CancellationPending) || currentTickerCancelled)
                    return GapInfo.GapStatus.FailedToFill;

                // запросить данные на сервере
                var loadedOk = false;
                try
                {
                    bool error;
                    var loaded = LoadQuotesFromDbSynch(ticker, time, requestEnd, out error);
                    if (loaded.Count > 0)
                    {
                        loadedOk = true;
                        candles.AddRange(loaded);
                    }
                }
                catch
                {
                    // запросить повторно - вдруг получится?
                    if ((worker != null && worker.CancellationPending) || currentTickerCancelled)
                        return GapInfo.GapStatus.FailedToFill;
                    try
                    {
                        bool error;
                        var loaded = LoadQuotesFromDbSynch(ticker, time, requestEnd, out error);
                        if (loaded.Count > 0)
                        {
                            loadedOk = true;
                            candles.AddRange(loaded);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // обновить гэп
                onPartLoaded(requestEnd, loadedOk ? GapInfo.GapStatus.Filled : GapInfo.GapStatus.FailedToFill);
                if (loadedOk) numOk++;
                else numFailed++;
            }
            return (numFailed == 0 && numOk > 0)
                       ? GapInfo.GapStatus.Filled
                       : numOk == 0
                             ? GapInfo.GapStatus.FailedToFill
                             : GapInfo.GapStatus.PartiallyFilled;
        }

        private static bool LoadQuotesFromServerFast(string ticker, GapList gaps, List<CandleData> candles)
        {
            // запросить данные на сервере
            var intervals = gaps.Gaps.Select(g => new Cortege2<DateTime, DateTime>(g.start, g.end)).ToList();
            IQuoteStorage storage;
            try
            {
                storage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
            }
            catch (Exception)
            {
                Logger.Error("LoadQuotesFromServerFast: Связь с сервером (IQuoteStorageFastBinding) не установлена");
                return false;
            }
            try
            {
                var packedQuoteStream = storage.GetMinuteCandlesPackedFast(ticker, intervals);
                if (packedQuoteStream == null || packedQuoteStream.count == 0)
                    return true;
                var packedCandles = packedQuoteStream.GetCandles();
                candles.AddRange(packedCandles.Select(c => new CandleData(c, DalSpot.Instance.GetPrecision10(ticker))));
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("LoadQuotesFromServerFast: Ошибка закачки котировок {0}: {1}", ticker, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// тупо и синхронно загрузить котировки с сервера
        /// </summary>
        public static List<CandleData> LoadQuotesFromDbSynch(string symbol, DateTime startTime, DateTime endTime,
            out bool error)
        {
            IQuoteStorage quoteStorage;
            error = false;
            try
            {
                quoteStorage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
            }
            catch (Exception)
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
                error = true;
                return new List<CandleData>();
            }
            try
            {
                var packedQuoteStream = quoteStorage.GetMinuteCandlesPacked(symbol, startTime, endTime);
                if (packedQuoteStream == null || packedQuoteStream.count == 0) return new List<CandleData>();
                var candles = packedQuoteStream.GetCandles();
                var candlesUnpacked = candles.Select(c => new CandleData(c, DalSpot.Instance.GetPrecision10(symbol))).ToList();
                return candlesUnpacked;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка закачки котировок {0} c {1:dd.MM.yyyy}: {2}",
                    symbol, startTime, ex.Message);
                error = true;
                return new List<CandleData>();
            }
        }
    }
}
