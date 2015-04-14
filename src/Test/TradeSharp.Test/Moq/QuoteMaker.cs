using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    static class QuoteMaker
    {
        private static readonly Dictionary<string, float> defaultQuoteBySymbol = new Dictionary<string, float>
            {
                { "AUDJPY", 80.35f },
                { "AUDUSD", 0.9725f },
                { "AUDCAD", 0.93435f },
                { "AUDCHF", 0.95342f },
                { "AUDNZD", 1.24302f },
                { "CADJPY", 83.821f },
                { "CHFJPY", 80.630f },
                { "CHFX", 0.7275f },
                { "EURJPY", 113.56f },
                { "EURAUD", 1.27620f },
                { "EURCAD", 1.35605f },
                { "EURCHF", 1.20070f },
                { "EURDKK", 7.4592f },
                { "EURGBP", 0.8021f },
                { "EURNOK", 7.8869f },
                { "EURNZD", 1.74875f },
                { "EURSEK", 9.3734f },
                { "EURUSD", 1.3520f },
                { "EURX", 109.801f },
                { "GBPAUD", 1.73647f },
                { "GBPCAD", 1.63235f },
                { "GBPCHF", 1.49640f },
                { "GBPJPY", 120.690f },
                { "GBPNZD", 2.05773f },
                { "GBPUSD", 1.5402f },
                { "GBPX", 1.5707f },
                { "JPYX", 1.0587f },
                { "NZDUSD", 0.7529f },
                { "USDCAD", 1.03260f },
                { "USDCHF", 0.9715f },
                { "USDRUB", 32.3692f },
                { "USDJPY", 113.56f } 
            };

        public static void FillQuoteStorageWithDefaultValues()
        {
            var date = DateTime.Now;
            QuoteStorage.Instance.UpdateValues(
                defaultQuoteBySymbol.Select(q => q.Key).ToArray(),
                defaultQuoteBySymbol.Select(q => new QuoteData(q.Value, q.Value * 1.0001f, date)).ToArray());
        }

        public static Dictionary<string, List<Cortege2<DateTime, float>>> MakeQuotesForQuoteDailyStorage(Dictionary<string, List<Cortege2<DateTime, float>>> startQuotes)
        {
            var startTime = DateTime.Now.Date.AddDays(-30);
            var quotes = startQuotes ??
                         defaultQuoteBySymbol.ToDictionary(q => q.Key,
                                                           q =>
                                                           new List<Cortege2<DateTime, float>>
                                                               {
                                                                   new Cortege2<DateTime, float>(startTime, q.Value)
                                                               });

            var iterator = 0;
            for (var time = startTime.AddMinutes(1); time < DateTime.Now; time = time.AddMinutes(1), iterator++)
            {
                foreach (var pair in quotes)
                    pair.Value.Add(new Cortege2<DateTime, float>(time, pair.Value[0].b + pair.Value[0].b * 0.15f * (float)Math.Sin(iterator / 100.0)));
            }
            return quotes;
        }

        public static List<CandleData> MakeQuotes(DateTime timeHistStart, DateTime timeHistEnd,
            DateSpan[] serverGaps)
        {
            // подготовить список котировок для "клиента" и "сервера"
            var allCandles = new List<CandleData>();
            
            var index = 0;
            for (var time = timeHistStart; time <= timeHistEnd; time = time.AddMinutes(1))
            {
                // проверить попадание в дырку на сервере
                if (serverGaps.Any(g => g.IsIn(time))) continue;

                if (DaysOff.Instance.IsDayOff(time)) continue;
                var o = (float)Math.Sin((index++) / 15.0);
                var candle = new CandleData(o, o + 0.001f, o - 0.003f, o - 0.001f, time, time.AddMinutes(1));
                allCandles.Add(candle);
            }

            return allCandles;
        }

        public static DateSpan[] MakeGapsOnDaysOff(DateTime startTime, DateTime endTime)
        {
            var spans = new List<DateSpan>();
            DateSpan curSpan = null;
            for (var time = startTime; time <= endTime; time = time.AddHours(1))
            {
                if (DaysOff.Instance.IsDayOff(time))
                {
                    if (curSpan == null)
                        curSpan = new DateSpan(time, time);
                    else
                        curSpan.end = time;
                    continue;
                }
                if (curSpan != null)
                {
                    if (curSpan.end != curSpan.start)
                        spans.Add(curSpan);
                    curSpan = null;
                }
            }

            return spans.ToArray();
        }

        public static QuoteData GetQuoteByKey(string key)
        {
            return QuoteStorage.Instance.ContainsKey(key) ? QuoteStorage.Instance.ExtractData(key) : null;
        }
    }
}
