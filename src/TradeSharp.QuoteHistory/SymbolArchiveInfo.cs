using System;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    public class SymbolArchiveInfo
    {
        public bool Selected { get; set; }

        public string Symbol { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateEnd { get; set; }

        public bool IsFavorite { get; set; }

        public string GapsTotalString { get; set; }

        public bool Loaded
        {
            get { return DateStart.HasValue && DateEnd.HasValue; }
        }

        static public string GetFileName(string symbol, string quoteCacheFolder)
        {
            return string.Format("{0}{1}\\{2}.quote", ExecutablePath.ExecPath, quoteCacheFolder, symbol);
        }

        public string DateRange
        {
            get
            {
                return DateStart.HasValue && DateEnd.HasValue
                           ? string.Format("{0:dd.MM.yyyy HH:mm} - {1:dd.MM.yyyy HH:mm}",
                                           DateStart.Value, DateEnd.Value)
                           : " - ";
            }
        }

        public bool IsEmpty
        {
            get
            {
                return !(DateStart.HasValue && DateEnd.HasValue);
            }
        }

        public SymbolArchiveInfo(string symbol, Cortege2<DateTime, DateTime>? dateRange)
        {
            Symbol = symbol;
            if (dateRange.HasValue)
            {
                DateStart = dateRange.Value.a;
                DateEnd = dateRange.Value.b;
            }
            var record = GapMap.Instance.GetServerGaps(symbol);
            if (record != null && record.serverGaps != null && record.serverGaps.Count > 0)
                GapsTotalString =
                    new TimeSpan(0, (int) record.serverGaps.Sum(g => g.TotalMinutes), 0).ToStringUniform(false, false);
        }
    }
}
