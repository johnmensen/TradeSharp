using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;

namespace TradeSharp.QuoteAdmin
{
    class TickerInfo : TradeTicker
    {
        public static List<TickerInfo> GetTickers()
        {
            long hash;
            var tickers = TradeSharpDictionary.Instance.proxy.GetTickers(out hash).Select(t => 
                new TickerInfo
                    {
                        ActiveBase = t.ActiveBase,
                        ActiveCounter = t.ActiveCounter,
                        CodeFXI = t.CodeFXI,
                        Description = t.Description,
                        Precision = t.Precision,
                        SwapBuy = t.SwapBuy,
                        SwapSell = t.SwapSell,
                        Title = t.Title
                    }).ToList();
            return tickers;
        }
    }
}
