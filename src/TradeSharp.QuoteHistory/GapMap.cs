using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    /// <summary>
    /// карта гэпов, обновляется в процессе актуализации истории
    /// в нее заносятся те промежутки, которые отсутствуют на сервере
    /// 
    /// данные по каждому тикеру актуальны в течении какого-то времени,
    /// т.к. на сервере дырки могут быть "заткнуты"
    /// </summary>
    public class GapMap
    {
        private static GapMap instance;
        public static GapMap Instance
        {
            get { return instance ?? (instance = new GapMap()); }
        }

        private GapMap()
        {
            LoadMapFromFile();
        }

        private readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private Dictionary<string, DateSpan> tickerFirstRecord;
        
        public int gapRecordActualMinutes = 60 * 24 * 200;

        private const int MinutesToRefreshTickerHistory = 60 * 3;

        private readonly string filePath = ExecutablePath.ExecPath + "\\servergap.txt";

        private readonly Dictionary<string, GapMapRecord> serverGaps =
            new Dictionary<string, GapMapRecord>();

        public void Clear()
        {
            serverGaps.Clear();
        }

        public GapMapRecord GetServerGaps(string ticker)
        {
            GapMapRecord record;
            serverGaps.TryGetValue(ticker, out record);
            return record;
        }

        public void ClearGaps(string ticker)
        {
            if (serverGaps.ContainsKey(ticker))
                serverGaps.Remove(ticker);
        }

        public void UpdateGaps(string ticker,
            List<DateSpan> gaps, bool mergeAdjacent = true)
        {
            if (gaps == null || gaps.Count == 0) return;
            GapMapRecord oldRec = null;
            serverGaps.TryGetValue(ticker, out oldRec);
            // если кеш пуст либо старая запись устарела...
            if (oldRec == null || !oldRec.IsActual || oldRec.serverGaps.Count == 0)
            {
                var record = new GapMapRecord(gaps, DateTime.Now, gaps[0].start);
                if (serverGaps.ContainsKey(ticker))
                    serverGaps[ticker] = record;
                else
                    serverGaps.Add(ticker, record);
                return;
            }

            if (gaps.Count == 0) return;

            // получить общий список гэпов
            var dicUnited = oldRec.serverGaps.ToDictionary(g => g.start, g => g);
            foreach (var g in gaps)
            {
                DateSpan exist;
                if (!dicUnited.TryGetValue(g.start, out exist))
                {
                    dicUnited.Add(g.start, g);
                    continue;
                }
                if (exist.end == g.end) continue;
                dicUnited[g.start] = g;
            }

            var newGaps = dicUnited.Values.OrderBy(v => v.start);
            
            // склеить соседние гэпы
            var resultGaps = new List<DateSpan>();
            foreach (var gap in newGaps)
            {
                if (resultGaps.Count == 0)
                {
                    resultGaps.Add(gap);
                    continue;
                }
                var lastGap = resultGaps[resultGaps.Count - 1];
                if (lastGap.end >= gap.start)
                {
                    // таки склеить
                    lastGap.end = gap.end;
                    continue;
                }
                resultGaps.Add(gap);
            }
            if (resultGaps.Count == 0) return;

            oldRec.serverGaps = resultGaps;
            if (oldRec.requestedStart < resultGaps[0].start) oldRec.requestedStart = resultGaps[0].start;
        }

        private void LoadMapFromFile()
        {
            if (!File.Exists(filePath)) return;
            serverGaps.Clear();

            try
            {
                using (var fs = new StreamReader(filePath, Encoding.ASCII))
                {
                    while (!fs.EndOfStream)
                    {
                        var line = fs.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        // разобрать строку - все гэпы по тикеру, вида:
                        // "EURUSD:14.06.2013 15:07,04.02.2012 00:00:201201050400-201201050503,2012010602120-2012010602136
                        var indexOfTick = line.IndexOf(':');
                        if (indexOfTick <= 0) continue;
                        var ticker = line.Substring(0, indexOfTick);
                        line = line.Substring(indexOfTick + 1);
                        if (string.IsNullOrEmpty(line)) continue;

                        indexOfTick = line.IndexOf(';');
                        if (indexOfTick <= 0) continue;
                        var timeRecStr = line.Substring(0, indexOfTick);
                        var timeParts = timeRecStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (timeParts.Length != 2) continue;
                        var timeRec = timeParts[0].ToDateTimeUniformSafe();
                        if (!timeRec.HasValue) continue;
                        var timeRequested = timeParts[1].ToDateTimeUniformSafe();
                        if (!timeRequested.HasValue) continue;

                        line = line.Substring(indexOfTick + 1);

                        var parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var record = new GapMapRecord(new List<DateSpan>(), timeRec.Value, timeRequested.Value);
                        foreach (var part in parts)
                        {
                            // 201201050400-201201050503
                            var startEndParts = part.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (startEndParts.Length != 2) continue;

                            DateTime timeStart;
                            if (!DateTime.TryParseExact(startEndParts[0], "yyyyMMddHHmm", CultureProvider.Common,
                                DateTimeStyles.None, out timeStart)) continue;
                            DateTime timeEnd;
                            if (!DateTime.TryParseExact(startEndParts[1], "yyyyMMddHHmm", CultureProvider.Common,
                                DateTimeStyles.None, out timeEnd)) continue;

                            if ((timeEnd - timeStart).TotalMinutes < 1) continue;

                            record.serverGaps.Add(new DateSpan(timeStart, timeEnd));
                        }

                        serverGaps.Add(ticker, record);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения \"{0}\": {1}", filePath, ex);
            }
        }

        public void SaveToFile()
        {
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    foreach (var gap in serverGaps)
                    {
                        sw.WriteLine(gap.Key + ":" + gap.Value.timeRead.ToStringUniform() + "," +
                            gap.Value.requestedStart.ToStringUniform() + ";" +
                            string.Join(",", gap.Value.serverGaps.Select(g =>
                                string.Format("{0:yyyyMMddHHmm}-{1:yyyyMMddHHmm}", g.start, g.end))));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка записи \"{0}\": {1}", filePath, ex);
            }
        }

        public static void ExcludeGapsThatLackedOnServer(ref List<DateSpan> gaps,
            List<DateSpan> serverGaps)
        {
            var newGaps = new List<DateSpan>();
            foreach (var gap in gaps)
            {
                var s = gap.start;
                var e = gap.end;
                if (serverGaps.Any(g => g.start.AddMinutes(-1) <= s && g.end.AddMinutes(1) >= e)) continue;
                newGaps.Add(gap);
            }
            gaps = newGaps;
        }

        public DateTime GetServerTickerHistoryStart(string ticker)
        {
            var lastUpTime = lastUpdated.GetLastHitIfHitted();
            var needUpdate = !lastUpTime.HasValue || (DateTime.Now - lastUpTime.Value).TotalMinutes >= MinutesToRefreshTickerHistory;
            if (needUpdate)
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
                        return DateTime.Now.Date.AddDays(-355);
                    }

                    tickerFirstRecord = quoteStorage.GetTickersHistoryStarts();
                }
                catch (Exception ex)
                {
                    tickerFirstRecord = new Dictionary<string, DateSpan>();
                    Logger.Error("GetServerTickerHistoryStart() - ошибка получения истории", ex);
                }
                finally
                {
                    lastUpdated.Touch();
                }
            }
            DateSpan span;
            return !tickerFirstRecord.TryGetValue(ticker, out span)
                ? DateTime.Now.Date.AddDays(-355)
                : span.start;
        }
    }

    public class GapMapRecord
    {
        public List<DateSpan> serverGaps;

        public DateTime timeRead;

        public DateTime requestedStart;

        public GapMapRecord(List<DateSpan> serverGaps,
            DateTime timeRead, DateTime requestedStart)
        {
            this.serverGaps = serverGaps;
            this.timeRead = timeRead;
            this.requestedStart = requestedStart;
        }

        public bool IsActual
        {
            get { return (DateTime.Now - timeRead).TotalMinutes <= GapMap.Instance.gapRecordActualMinutes; }
        }
    }
}
