using System;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class QuoteData
    {
        public DateTime time;
        public float bid;
        public float ask;

        public QuoteData() {}
        public QuoteData(float bid, float ask, DateTime time)
        {
            this.bid = bid;
            this.ask = ask;
            this.time = time;
        }
        public QuoteData(QuoteData q)
        {
            bid = q.bid;
            ask = q.ask;
            time = q.time;
        }

        public bool PricesAreSame(QuoteData q)
        {
            if (q == null) return false;
            return bid == q.bid && ask == q.ask;
        }

        public float GetPrice(QuoteType quoteType)
        {
            var price = quoteType == QuoteType.Ask
                            ? ask
                            : quoteType == QuoteType.Bid
                                  ? bid
                                  : quoteType == QuoteType.Middle 
                                    ? 0.5f * (ask + bid) 
                                    : ask == 0
                                        ? bid
                                        : bid == 0 ? ask : 0.5f * (ask + bid);
            return price;
        }

        public static QuoteData ParseQuoteStringNewFormat(string line,
            DateTime date)
        {
            // 0000 1.3570 1.3572
            var parts = line.Split(' ');
            if (parts.Length != 3) return null;
            var quote = new QuoteData();
            var timeNum = int.Parse(parts[0]);
            int hour = timeNum / 100;
            int minute = timeNum - 100 * hour;
            quote.time = date.AddMinutes(minute + hour * 60);
            quote.bid = parts[1].ToFloatUniform();
            quote.ask = parts[2].ToFloatUniform();
            return quote;
        }

       public static QuoteData ParseQuoteStringOldFormat(string line)
        {
            // 74.3304;74.3401;02.01.2009 02:09:34
            if (string.IsNullOrEmpty(line)) return null;
            var parts = line.Split(';');
            if (parts.Length != 3) return null;
            var bid = float.Parse(parts[0], CultureProvider.Common);
            var ask = float.Parse(parts[1], CultureProvider.Common);
            var time = DateTime.ParseExact(parts[2], "dd.MM.yyyy H:mm:ss",
                CultureProvider.Common);
            return new QuoteData { bid = bid, ask = ask, time = time };
        }

       public static bool IsNewFormatDateRecord(string line, out DateTime? date)
       {
           date = null;
           if (line.Length == 8)
           {
               var year = line.Substring(4).ToIntSafe();
               var month = line.Substring(2, 2).ToIntSafe();
               var day = line.Substring(0, 2).ToIntSafe();
               if (!year.HasValue || !month.HasValue || !day.HasValue) return false;
               date = new DateTime(year.Value, month.Value, day.Value);
               return true;
           }
           return false;
       }
    }
}