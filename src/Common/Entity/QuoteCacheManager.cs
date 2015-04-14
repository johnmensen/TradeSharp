using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace Entity
{
    public class QuoteCacheManager
    {
        public const int MinSecondsOfGap = 120;

        public static void GetFirstAndLastFileDates(string fileName,
            out DateTime? dateFirst, out DateTime? dateLast, out bool endsUpNewLine)
        {
            dateFirst = null;
            dateLast = null;
            endsUpNewLine = false;
            if (!File.Exists(fileName)) return;

            using (var sr = new StreamReaderLog(fileName))
            {
                // получить первую строку
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    try
                    {
                        if (!dateFirst.HasValue)
                        {
                            DateTime lineDate;
                            if (!DateTime.TryParseExact(line, "ddMMyyyy", CultureProvider.Common, DateTimeStyles.None, out lineDate))
                            {
                                Logger.DebugFormat("Файл котировок \"{0}\" - начало повреждено", fileName);
                                continue;
                            }
                            dateFirst = lineDate;
                        }
                        else
                        {
                            var parts = line.Split(' ');
                            var nH = int.Parse(parts[0].Substring(0, 2));
                            var nM = int.Parse(parts[0].Substring(2));
                            dateFirst = dateFirst.Value.AddMinutes(nM + nH * 60);
                            break;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
                if (sr.EndOfStream) return;

                // получить последнюю строку                
                var strLast = ReadEndTokens(sr.BaseStream, 1500, Encoding.ASCII, "\n");
                if (!string.IsNullOrEmpty(strLast))
                    if (strLast[strLast.Length - 1] == '\n')
                        endsUpNewLine = true;
                var quoteLines = strLast.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                int? lastHour = null, lastMin = null;
                for (var i = quoteLines.Length - 1; i >= 0; i--)
                {
                    var quoteLineParts = quoteLines[i].Split(' ');
                    if (quoteLineParts.Length == 3 && lastHour.HasValue == false)
                    {
                        lastHour = int.Parse(quoteLineParts[0].Substring(0, 2));
                        lastMin = int.Parse(quoteLineParts[0].Substring(2));
                        continue;
                    }
                    if (quoteLineParts.Length == 1)
                    {
                        DateTime lineDate;
                        if (!DateTime.TryParseExact(quoteLines[i], "ddMMyyyy", 
                            CultureProvider.Common, DateTimeStyles.None, out lineDate))
                            continue;
                        dateLast = lineDate;
                        if (lastHour.HasValue)
                            dateLast = dateLast.Value.AddMinutes(lastMin.Value + lastHour.Value * 60);
                        break;
                    }
                }
            }
        }

        public static DateTime? GetFirstDate(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            DateTime? dateFirst = null;

            using (var sr = new StreamReaderLog(fileName))
            {
                // получить первую строку
                var linesToTryParseMax = 10;
                while (!sr.EndOfStream)
                {
                    linesToTryParseMax--;
                    if (linesToTryParseMax < 0) break;

                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.Length == "ddMMyyyy".Length)
                    {
                        DateTime lineDate;
                        if (DateTime.TryParseExact(line, "ddMMyyyy", CultureProvider.Common, DateTimeStyles.None, out lineDate))
                            dateFirst = lineDate;
                        continue;
                    }

                    if (dateFirst == null) continue;
                    try
                    {
                        var parts = line.Split(' ');
                        if (parts.Length != 3) continue;
                        var hour = parts[0].Substring(0, 2).ToInt();
                        var minute = parts[0].Substring(2).ToInt();
                        dateFirst = dateFirst.Value.AddMinutes(hour*60 + minute);
                        return dateFirst;
                    }
                    catch
                    {
                    }
                }

                return dateFirst;
            }
        }

        private static string ReadEndTokens(Stream fs, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
        {
            var sizeOfChar = encoding.GetByteCount("\n");
            var buffer = encoding.GetBytes(tokenSeparator);

            Int64 tokenCount = 0;
            Int64 endPosition = fs.Length / sizeOfChar;
            for (Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar)
            {
                fs.Seek(-position, SeekOrigin.End);
                fs.Read(buffer, 0, buffer.Length);
                if (encoding.GetString(buffer) == tokenSeparator)
                {
                    tokenCount++;
                    if (tokenCount == numberOfTokens)
                    {
                        var returnBuffer = new byte[fs.Length - fs.Position];
                        fs.Read(returnBuffer, 0, returnBuffer.Length);
                        return encoding.GetString(returnBuffer);
                    }
                }
            }
            // handle case where number of tokens in file is less than numberOfTokens         
            fs.Seek(0, SeekOrigin.Begin);
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            return encoding.GetString(buffer);
        }

        public static void LoadCandlesSilent(
            Dictionary<string, Cortege2<DateTime, DateTime>> tickersToUpload,
            QuoteStorageProxy quoteStorage,
            string quoteFolder,
            int minMinutesOfGap)
        {
            const int daysInRequest = 5, minDaysInRequest = 2;
            foreach (var ticker in tickersToUpload)
            {
                var pointCost = DalSpot.Instance.GetPrecision10(ticker.Key);
                var filePath = string.Format("{0}\\{1}.quote",
                                             ExecutablePath.ExecPath + quoteFolder,
                                             ticker.Key);

                DateTime? lastDateStore = null;
                DateTime? oldStart = null, oldEnd = null;
                bool endsUpNewLine;
                
                if (File.Exists(filePath))
                    GetFirstAndLastFileDates(filePath, out oldStart, out oldEnd, out endsUpNewLine);
                else
                {
                    var dirName = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dirName))
                    {
                        try
                        {
                            Directory.CreateDirectory(dirName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(String.Format("Невозможно создать каталог котировок \"{0}\": {1}", dirName, ex));
                            throw;
                        }
                    }
                }
                
                if (oldStart.HasValue && oldEnd.HasValue)
                {
                    var deltaStart = (oldStart.Value - ticker.Value.a).TotalMinutes;
                    var deltaEnd = (ticker.Value.b - oldEnd.Value).TotalMinutes;
                    if (deltaStart <= minMinutesOfGap &&
                        deltaEnd <= minMinutesOfGap) continue;

                    if (deltaStart <= minMinutesOfGap)
                    {                        
                        lastDateStore = oldEnd.Value;
                    }
                }
                
                var dateLast = DateTime.Now;
                for (var dateStart = lastDateStore.HasValue ? lastDateStore.Value.AddMinutes(1) : ticker.Value.a; 
                    dateStart < ticker.Value.b; )
                {
                    var dateEnd = dateStart.AddDays(daysInRequest);
                    if ((dateLast - dateEnd).TotalDays < minDaysInRequest) dateEnd = dateLast;

                    // запрос
                    List<CandleData> curCandles = null;
                    var numFaultsLeft = 2;
                    while (numFaultsLeft > 0)
                    {
                        // попытка подгрузить котировки
                        try
                        {
                            var packedQuotes = quoteStorage.GetMinuteCandlesPacked(ticker.Key, dateStart, dateEnd);
                            if (packedQuotes != null && packedQuotes.count > 0)
                            {
                                var denseCandles = packedQuotes.GetCandles();
                                if (denseCandles != null && denseCandles.Count > 0)
                                {
                                    curCandles = denseCandles.Select(c => new CandleData(c, pointCost)).ToList();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Ошибка закачки котировок {0} c {1:dd.MM.yyyy}: {2}",
                                                ticker.Key, dateStart, ex.Message);
                            curCandles = null;
                        }

                        if (curCandles == null)
                        {
                            // попытка неуспешна - еще одна?
                            numFaultsLeft--;
                            if (numFaultsLeft > 0) continue;
                            break;
                        }
                        break;
                    }

                    if (curCandles != null && curCandles.Count > 0)
                    {
                        // записать в файл прочитанные из БД котировки
                        try
                        {
                            var existData = CandleData.LoadFromFile(filePath, ticker.Key);
                            if (existData != null && existData.Count > 0)
                                CandleData.MergeCandles(ref curCandles, existData, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("QuoteCacheManager.LoadQuotesSilent() - ошибка чтения / склейки котировок \"{0}\": {1}",
                                filePath, ex);
                        }
                        try
                        {
                            
                            CandleData.SaveInFile(filePath, ticker.Key, curCandles);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("QuoteCacheManager.LoadQuotesSilent() - ошибка сохранения котировок \"{0}\": {1}", 
                                filePath, ex);
                        }
                            
                    }
                    dateStart = dateEnd;
                }
            }            
        }

        /// <summary>
        /// определение Gap-ов на указанном временном интервале
        /// <param name="startTime">начало интервала запроса; если не указан, принимается равным времени первой свечки</param>
        /// <param name="endTime">конец интервала запроса; если не указан, принимается равным времени последней свечки</param>
        /// предназначено для работы с минутными свечками
        /// </summary>
        public static List<DateSpan> GetGaps(List<CandleData> quotes, DateTime? startTime, DateTime? endTime)
        {
            var result = new List<DateSpan>();
            DateTime? previousTime = null; // время последней встретившейся свечки
            // detect first gap head
            if (startTime.HasValue)
                previousTime = startTime.Value.AddMinutes(-1); // зд. время фиктивной свечки
                                                               // из предположения, что за пределами запрошенного интервала свечки есть
            var currentIndex = startTime.HasValue ? quotes.FindIndex(c => c.timeOpen >= startTime.Value) : 0;
            if (currentIndex == -1)
                currentIndex = quotes.Count;
            for (; currentIndex < quotes.Count; currentIndex++)
            {
                var quote = quotes[currentIndex];
                var currentTime = quote.timeOpen;
                if (endTime.HasValue && currentTime > endTime.Value)
                    break;
                if (previousTime.HasValue)
                {
                    var gaps = DecorateGaps(previousTime.Value, currentTime);
                    if (gaps != null)
                        result.AddRange(gaps);
                }
                previousTime = currentTime;
            }
            if (!endTime.HasValue)
                return result;
            // tail gaps
            DateTime tailBeginTime;
            if (currentIndex == 0) // все свечки правее интервала запроса
            {
                if (!previousTime.HasValue) // условие принципиально не выполняется
                    return result;
                tailBeginTime = previousTime.Value;
            }
            else
            {
                var tailQuote = quotes[currentIndex - 1];
                tailBeginTime = tailQuote.timeOpen; // время, на котором обработка свечек прервалась
                if (startTime.HasValue && tailBeginTime < startTime.Value) // все свечки левее интервала запроса
                    tailBeginTime = startTime.Value.AddMinutes(-1);
            }
            var tailGaps = DecorateGaps(tailBeginTime, endTime.Value.AddMinutes(1));
            if (tailGaps != null)
                result.AddRange(tailGaps);
            return result;
        }

        private static IEnumerable<DateSpan> DecorateGaps(DateTime start, DateTime end)
        {
            if (end < start)
                return null;
            if ((int) Math.Ceiling((end - start).TotalMinutes) <= 1) // гэп длиной больше свечки
                return null;
            var startGap = start.AddMinutes(1);
            var endGap = end.AddMinutes(-1);
            var hoursOff = DaysOff.Instance.GetIntersected(new DateSpan(startGap, endGap));
            return
                SubtractPeriods(new DateSpan(startGap, endGap), hoursOff)
                    .Where(g => (int) Math.Ceiling(g.TotalMinutes) != 0);
        }

        // вычитание из временного интервала множества интервалов
        // ReSharper disable ReturnTypeCanBeEnumerable.Local
        public static List<DateSpan> SubtractPeriods(DateSpan p1, List<DateSpan> p2)
        // ReSharper restore ReturnTypeCanBeEnumerable.Local
        {
            if (p2 == null || p2.Count == 0) return new List<DateSpan> { p1 };
            var result = new List<DateSpan>();
            var hasCross = false;
            foreach (var period in p2)
            {
                // нет пересечения
                if ((period.end < p1.start) || (period.start > p1.end))
                    continue;
                hasCross = true;
                // возможно разделение на 2 части
                if (p1.start < period.start)
                    result.Add(new DateSpan(p1.start, period.start));
                p1.start = period.end;
                if (p1.start > p1.end)
                    return result;
            }
            if (hasCross)
                result.Add(p1);
            return result;
        }
    }
}