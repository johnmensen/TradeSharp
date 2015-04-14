using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.TradeLib
{
    public interface IProfitCalculator
    {
        decimal CalculateAccountEquity(int accountId, decimal balance, string depoCurx,
                                       Dictionary<string, QuoteData> curPrices, ITradeSharpAccount proxyAccount);

        decimal CalculatePositionProfit(MarketOrder pos, string depoCurx, Dictionary<string, QuoteData> curPrices);

        Dictionary<string, decimal> CalculateAccountExposure(int accountId,
                                                             out decimal equity,
                                                             out decimal reservedMargin,
                                                             out decimal exposure,
                                                             Dictionary<string, QuoteData> curPrices,
                                                             ITradeSharpAccount proxyAccount,
                                                             Func<int, AccountGroup> getAccountGroup);
    }
}
