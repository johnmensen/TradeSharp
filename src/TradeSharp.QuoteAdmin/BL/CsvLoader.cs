using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin.BL
{
    class CsvLoader
    {
        private const int BufferSize = 1000 * 10;

        public int LoadFromCsv(string fileName, int tickerCode)
        {
            var storeThread = new DBStoreThread(true);
            var candles = new List<CandleData>();
            var totalStored = 0;
            
            using (var sr = new StreamReader(fileName, Encoding.GetEncoding(1252)))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var parts = line.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    // date - time - o-h-l-c
                    if (parts.Length < 6) continue;
                    DateTime time;
                    if (!DateTime.TryParseExact(parts[0] + " " + parts[1],
                                                "yyyy.MM.dd HH:mm", CultureProvider.Common, DateTimeStyles.AssumeLocal,
                                                out time))
                        continue;
                    var open = parts[2].ToFloatUniformSafe() ?? 0;
                    var high = parts[3].ToFloatUniformSafe() ?? 0;
                    var low = parts[4].ToFloatUniformSafe() ?? 0;
                    var close = parts[5].ToFloatUniformSafe() ?? 0;

                    if (open == 0 || high == 0 || low == 0 || close == 0) continue;
                    candles.Add(new CandleData(open, high, low, close, time, time.AddMinutes(1)));
                    if (candles.Count > BufferSize)
                    {
                        storeThread.PushQuotes(new Dictionary<int, List<CandleData>>
                            { { tickerCode, candles.ToList() } });
                        totalStored += candles.Count;
                        candles.Clear();
                    }
                }
                totalStored += candles.Count;
                if (candles.Count > 0)
                    storeThread.PushQuotes(new Dictionary<int, List<CandleData>>
                        { {tickerCode, candles.ToList()} });
            }

            while (storeThread.CandlesLeftInQueue > 0)
            {
                Thread.Sleep(200);
            }
            storeThread.Stop();


            return totalStored;
        }
    }
}
