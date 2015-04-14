using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using Moq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    static class MoqQuoteStorage
    {
        public static Mock<IQuoteStorage> MakeMoq(Dictionary<string, List<CandleData>> candlesM1)
        {
            var moq = new Mock<IQuoteStorage>();

            moq.Setup(s => s.GetTickersHistoryStarts()).Returns(
                 () => candlesM1.ToDictionary(c => c.Key, c => new DateSpan(c.Value[0].timeOpen,
                                                                       c.Value[c.Value.Count - 1].timeOpen)));

            moq.Setup(s => s.GetMinuteCandlesPacked(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
               .Returns((string ticker, DateTime start, DateTime end) =>
                   {
                       if (!candlesM1.ContainsKey(ticker)) 
                           return new PackedCandleStream(new List<CandleDataPacked>(), false);
                       var candles = candlesM1[ticker].Where(c => c.timeOpen >= start && c.timeOpen <= end).Select(c => new CandleDataPacked
                           {
                               open = c.open,
                               timeOpen = c.timeOpen
                           }).ToList();

                       var stream = new PackedCandleStream(candles, candles.Count > 100);
                       return stream;
                   });

            moq.Setup(s => s.GetMinuteCandlesPackedFast(It.IsAny<string>(), It.IsAny<List<Cortege2<DateTime, DateTime>>>()))
               .Returns((
                   string symbol, List<Cortege2<DateTime, DateTime>> intervals) =>
               {
                   if (!candlesM1.ContainsKey(symbol))
                       return new PackedCandleStream(new List<CandleDataPacked>(), false);
                   var candles = new List<CandleDataPacked>();
                   foreach (var interval in intervals)
                   {
                       candles.AddRange(
                           candlesM1[symbol].Where(c => c.timeOpen >= interval.a && c.timeOpen <= interval.b)
                                            .Select(c => new CandleDataPacked
                                            {
                                                open = c.open,
                                                timeOpen = c.timeOpen
                                            }));
                   }
                   var stream = new PackedCandleStream(candles, candles.Count > 100);
                   return stream;
               });

            return moq;
        }
    }
}
