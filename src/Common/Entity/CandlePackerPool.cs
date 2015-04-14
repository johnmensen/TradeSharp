using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Entity
{
    /// <summary>
    /// используется при передаче котировок в работу роботам
    /// котировки упаковываются в минутные свечки CandleDataBidAsk,
    /// и в таком виде отдаются роботам
    /// </summary>
    public class CandlePackerPool
    {
        class CandlePackerBidAsk
        {
            private static readonly BarSettings minuteTimeframe =
                new BarSettings { Intervals = new List<int> { 1 } };

            private readonly CandlePacker packerBid, packerAsk;

            public CandlePackerBidAsk()
            {
                packerBid = new CandlePacker(minuteTimeframe);
                packerAsk = new CandlePacker(minuteTimeframe);
            }

            public CandleDataBidAsk UpdateCandle(QuoteData q)
            {
                var candleBid = packerBid.UpdateCandle(q.bid, q.time) ?? packerBid.CurrentCandle;
                var candleAsk = packerAsk.UpdateCandle(q.ask, q.time) ?? packerAsk.CurrentCandle;
                return new CandleDataBidAsk(candleBid, candleAsk)
                    {
                        timeClose = q.time
                    };
            }
        }

        private readonly ConcurrentDictionary<string, CandlePackerBidAsk> packers;

        #region FloodSafeLogger
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);

        private const int LogMsgPackerForTickerMissed = 1;
        #endregion

        public CandlePackerPool()
        {
            packers = new ConcurrentDictionary<string, CandlePackerBidAsk>(DalSpot.Instance.GetTickerNames().ToDictionary(t => t,
                t => new CandlePackerBidAsk()));
        }

        public void MakeCandles(List<Cortege2<string, QuoteData>> quotes,
            out string[] names, out CandleDataBidAsk[] candles)
        {
            var namesList = new List<string>();
            var candlesList = new List<CandleDataBidAsk>();

            foreach (var quote in quotes)
            {
                CandlePackerBidAsk packer;
                if (!packers.TryGetValue(quote.a, out packer))
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgPackerForTickerMissed,
                        1000 * 60 * 5, "CandlePackerPool - не найден пакер для тикера {0}, всего {1} пакеров",
                        quote.a, packers.Count);
                    continue;
                }
                var candle = packer.UpdateCandle(quote.b);
                namesList.Add(quote.a);
                candlesList.Add(candle);
            }

            names = namesList.ToArray();
            candles = candlesList.ToArray();
        }

        public CandleDataBidAsk[] MakeCandles(QuoteData[] quotes, ref string[] names)
        {
            var candles = new CandleDataBidAsk[quotes.Length];

            List<int> indexesLacked = null;

            for (var i = 0; i < quotes.Length; i++)
            {
                CandlePackerBidAsk packer;
                if (!packers.TryGetValue(names[i], out packer))
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgPackerForTickerMissed,
                        1000 * 60 * 5, "CandlePackerPool - не найден пакер для тикера {0}, всего {1} пакеров",
                        names[i], packers.Count);

                    if (indexesLacked == null) 
                        indexesLacked = new List<int> {i};
                    else 
                        indexesLacked.Add(i);

                    continue;
                }
                var candle = packer.UpdateCandle(quotes[i]);
                candles[i] = candle;
            }

            if (indexesLacked != null)
            {
                if (indexesLacked.Count == quotes.Length)
                {
                    names = new string[0];
                    return new CandleDataBidAsk[0];
                }
                names = names.Where((n, i) => !indexesLacked.Contains(i)).ToArray();
                candles = candles.Where((n, i) => !indexesLacked.Contains(i)).ToArray();
            }
            
            return candles;
        }
    }
}
