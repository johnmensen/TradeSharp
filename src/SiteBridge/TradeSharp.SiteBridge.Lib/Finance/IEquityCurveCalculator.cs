using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteHistory;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    public interface IEquityCurveCalculator
    {
        AccountPerformanceRaw CalculateEquityCurve(List<MarketOrder> deals, string depoCurrency, QuoteArchive quoteArc, List<BalanceChange> transactions);

        AccountPerformanceRaw CalculateEquityCurve(List<MarketOrder> deals, string depoCurrency, QuoteArchive quoteArc, List<BalanceChange> transactions, DateTime timeStart, DateTime timeEnd);

        float CalculateProfitInDepoCurx(bool useBase, float profit, string dealTicker, string depoCurrency, QuoteArchive arc, DateTime date);

        void CalculateDealVolumeInDepoCurrency(MarketOrder deal, QuoteData dealQuote, string depoCurrency, QuoteArchive arc, DateTime date);

    }
}
