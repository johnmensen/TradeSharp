using System;
using System.Collections.Generic;
using Entity;

namespace TradeSharp.QuoteHistory
{
    public class GapHuntResults
    {
        public string ticker;

        public List<CandleData> candles;

        public int candlesLoaded;

        public double candlesOfGapTotal;

        public int gapsCount;

        public string GapsTotalTimeFormatString
        {
            get
            {
                if (candlesOfGapTotal < 1) return "-";
                if (candlesOfGapTotal < 100) return candlesOfGapTotal.ToString("f0");
                var span = new TimeSpan(0, (int)candlesOfGapTotal, 0);
                var days = (int)span.TotalDays;
                var hours = span.Hours;
                var minutes = span.Minutes;

                return days > 0 ? string.Format("{0} дней, {1}:{2}", days, hours, minutes)
                    : string.Format("{0} ч. {1} мин.", hours, minutes);
            }
        }

        public GapHuntResults(string ticker)
        {
            candles = new List<CandleData>();
            this.ticker = ticker;
        }
    }
}
