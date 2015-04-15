using System;
using System.Collections.Generic;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Quotes
{
    public interface IDailyQuoteStorage
    {
        List<Cortege2<DateTime, float>> GetQuotes(string ticker);

        void UpdateStorageSync();
    }
}
