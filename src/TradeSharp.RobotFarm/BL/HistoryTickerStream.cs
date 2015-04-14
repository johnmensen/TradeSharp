using System;
using System.Collections.Generic;
using System.Linq;
using Entity;

namespace TradeSharp.RobotFarm.BL
{
    class HistoryTickerStream
    {
        private readonly Dictionary<string, List<CandleDataBidAsk>> quotes;

        private readonly Dictionary<string, int> currentIndex;

        private DateTime timeNow;

        public HistoryTickerStream(Dictionary<string, List<CandleDataBidAsk>> quotes)
        {
            this.quotes = quotes;
            if (quotes.Count == 0) return;
            currentIndex = quotes.ToDictionary(q => q.Key, q => 0);
            timeNow = quotes.Select(q => q.Value.Count == 0 ? DateTime.MaxValue : q.Value[0].timeClose).Min();
        }

        public bool Step(out string[] names, out CandleDataBidAsk[] currentQuotes)
        {
            var lstNames = new List<string>();
            var lstQuotes = new List<CandleDataBidAsk>();
            var nextTimes = new List<DateTime>();

            foreach (var pair in quotes)
            {
                var symbol = pair.Key;
                var quotesList = pair.Value;
                var index = currentIndex[symbol];
                if (index < 0) continue;

                var quote = quotesList[index];
                if (quote.timeClose == timeNow)
                {
                    lstQuotes.Add(quote);
                    lstNames.Add(pair.Key);
                    index++;
                    if (index >= quotesList.Count) index = -1;
                    currentIndex[symbol] = index;
                }

                if (index >= 0) nextTimes.Add(quotesList[index].timeClose);
            }
            if (nextTimes.Count > 0)
                timeNow = nextTimes.Min();
            names = lstNames.ToArray();
            currentQuotes = lstQuotes.ToArray();

            return names.Length > 0;
        }
    }
}
