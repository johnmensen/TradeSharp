using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    public class QuoteArchive
    {
        public readonly Dictionary<string, List<QuoteData>> dicQuote;
        private Dictionary<string, int?> dicIndex;

        public QuoteArchive(Dictionary<string, List<QuoteData>> dic)
        {
            dicQuote = dic;
            ToBegin();
        }

        /// <summary>
        /// сбрасывает индексы для последовательного поиска котировки по времени.
        /// см. <see cref="GetQuoteOnDateSeq"/>
        /// </summary>
        public void ToBegin()
        {
            dicIndex = new Dictionary<string, int?>();
            foreach (var key in dicQuote.Keys)
                dicIndex.Add(key, null);
        }

        /// <summary>
        /// вернуть котиру по символу на определенную дату
        /// метод последовательный, т.е. перебор котировок ведется от последнего индекса
        /// сохраненные индексы сбрасывает метод ToBegin()
        /// </summary>        
        public QuoteData GetQuoteOnDateSeq(string symbol, DateTime date)
        {
            if (!dicQuote.ContainsKey(symbol)) return null;
            var list = dicQuote[symbol];

            int? startIndex;
            if (!dicIndex.TryGetValue(symbol, out startIndex))
            {
                var errorStr = string.Format("Валютная пара {0} не найдена", symbol);
                Logger.Error(errorStr);
                throw new ArgumentOutOfRangeException(errorStr);
            }
            var lastQuote = startIndex.HasValue ? list[startIndex.Value] : null;
            if (startIndex.HasValue)
                if (startIndex.Value == list.Count - 1) return lastQuote;

            QuoteData nextQuote;
            for (var i = startIndex ?? 0; i < list.Count; i++)
            {
                nextQuote = list[i];
                if (nextQuote.time < date) continue;
                if (i > 0)
                {
                    dicIndex[symbol] = i - 1;
                    return list[i - 1];
                }
            }
            return null;
        }

        public QuoteData GetLastQuote(string symbol, DateTime date, int maxDeltaSeconds = 60 * 60 * 2)
        {
            if (!dicQuote.ContainsKey(symbol)) return null;
            var quotes = dicQuote[symbol];
            if (quotes.Count == 0) return null;
            var quote = quotes[quotes.Count - 1];
            var secondsStale = (date - quote.time).TotalSeconds;
            return secondsStale > maxDeltaSeconds ? null : quote;
        }

        /// <summary>
        /// вернуть последние котировки по каждому тикеру
        /// </summary>        
        public Dictionary<string, QuoteData> GetCurrentQuotes()
        {
            return dicQuote.ToDictionary(q => q.Key, q => q.Value[q.Value.Count - 1]);
        }
    }
}
