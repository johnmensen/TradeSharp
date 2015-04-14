using System;
using System.Collections.Generic;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Feeder
{
    static class QuoteFeederParser
    {
        public static void ParseQuotes(string str,
            out List<string> names, out List<QuoteData> quotes)
        {
            // GBPUSD,1.55209,1.55222;EURAUD,1.30496,1.30511;EURGBP,0.83115,0.83125;
            names = new List<string>();
            quotes = new List<QuoteData>();

            if (string.IsNullOrEmpty(str)) return;

            var quoteStrings = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var quoteStr in quoteStrings)
            {
                var parts = quoteStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                var symbol = parts[0];
                var bid = parts[1].ToFloatUniform();
                var ask = parts[2].ToFloatUniform();
                names.Add(symbol);
                quotes.Add(new QuoteData(bid, ask, DateTime.Now));
            }
        }

        public static void ParseQuotes(byte[] data, int count,
            out List<string> names, out List<QuoteData> quotes)
        {
            var str = Encoding.ASCII.GetString(data, 0, count);
            ParseQuotes(str, out names, out quotes);
        }
    }
}
