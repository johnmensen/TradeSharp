using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.QuoteService.Feeder
{
    interface IQuoteFeeder
    {
        void Start();
        void Stop();
        event QuotesReceived OnQuotesReceived;
    }

    public delegate void QuotesReceived(List<string> names, List<QuoteData> quotes);
}
