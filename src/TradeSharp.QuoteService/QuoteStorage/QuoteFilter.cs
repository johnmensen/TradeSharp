using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.QuoteStorage
{
    /// <summary>
    /// по каждой котировке хранится ее последнее значение из БД
    /// чтобы не записать, к примеру, 102.63 после 1.0125
    /// </summary>
    class QuoteFilter
    {
        private readonly Dictionary<string, float> quoteByTicker;

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const float MinValue = 0.01f;

        private const float MaxDeltaRel = 0.25f;

        public QuoteFilter()
        {
            var prices = QuoteStorageManager.Instance.GetLastPriceByTicker();
            quoteByTicker = prices.ToDictionary(p => DalSpot.Instance.GetSymbolByFXICode(p.Key), p => p.Value);
        }

        public bool IsValid(string symbol, QuoteData quote)
        {
            var logMsgCode = symbol.GetHashCode();

            var price = (quote.ask + quote.bid) * 0.5f;
            if (price < MinValue)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, logMsgCode, 1000 * 60 * 120,
                    "Котировка {0} ({1}) меньше допустимого значения {2}",
                    symbol, price, MinValue);
                return false;
            }

            float val;
            if (!quoteByTicker.TryGetValue(symbol, out val))
                return true;

            var delta = Math.Abs(price - val) / val;
            if (delta > MaxDeltaRel)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, logMsgCode, 1000 * 60 * 120,
                    "Котировка {0} ({1}): последнее сохраненное значение {2}",
                    symbol, price, val);
                return false;
            }

            return true;
        }
    }
}
