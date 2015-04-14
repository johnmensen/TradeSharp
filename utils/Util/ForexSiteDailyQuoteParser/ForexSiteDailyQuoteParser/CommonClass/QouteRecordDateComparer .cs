using System.Collections.Generic;

namespace ForexSiteDailyQuoteParser.CommonClass
{
    class QouteRecordDateComparer : IEqualityComparer<QuoteRecord>
    {
        public bool Equals(QuoteRecord x, QuoteRecord y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Date == y.Date && x.Simbol == y.Simbol;
        }

        public int GetHashCode(QuoteRecord obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            var hashProductName = obj.Date.GetHashCode();
            var hashProductCode = obj.Simbol.GetHashCode();
            return hashProductName ^ hashProductCode;
        }
    }
}
