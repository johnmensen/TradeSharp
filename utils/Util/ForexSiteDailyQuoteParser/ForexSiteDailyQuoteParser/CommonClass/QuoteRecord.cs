using System;

namespace ForexSiteDailyQuoteParser.CommonClass
{
    /// <summary>
    /// Запись об одной дневной котировке
    /// </summary>
    public class QuoteRecord
    {
        public string Simbol { get; set; }

        public DateTime Date { get; set; }

        public float? Open { get; set; }

        public float? High { get; set; }

        public float? Low { get; set; }

        public float? Close { get; set; }
    }
}
