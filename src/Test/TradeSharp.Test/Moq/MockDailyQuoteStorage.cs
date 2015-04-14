using System;
using System.Collections.Generic;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    class MockDailyQuoteStorage : IDailyQuoteStorage
    {
        public List<Cortege2<DateTime, float>> GetQuotes(string ticker)
        {
            return new List<Cortege2<DateTime, float>>();
        }

        public void UpdateStorageSync()
        {
        }
    }
}
