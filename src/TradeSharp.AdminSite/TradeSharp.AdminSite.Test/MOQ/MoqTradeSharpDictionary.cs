using System.Collections.Generic;
using Moq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.AdminSite.Test.MOQ
{
    static class MoqTradeSharpDictionary
    {
        public static Mock<ITradeSharpDictionary> Moq
        {
            get
            {
                var moq = new Mock<ITradeSharpDictionary>();
                moq.Setup(s => s.GetMetadataByCategory(It.IsAny<string>())).Returns<string>(
                    cat =>
                        cat == "DayOff" ? new Dictionary<string, object>
                            {
                                { "New Year's Day", "D:01/01/-;0-24" },
                                { "New Year holiday", "D:02/01/-;0-96" },
                                { "Christmas", "D:07/01/-;0-24" },
                                { "Defender of the Fatherland Day", "D:23/02/-;0-24" },
                                { "Internatuin Women's Day", "D:08/03/-;0-24" },
                                { "Spring and Labour Day", "D:01/05/-;0-24" },
                                { "Victory Day", "D:09/05/-;0-24" },
                                { "Russia Day", "D:12/06/-;0-24" },
                                { "Unity Day", "D:04/11/-;0-24" },
                                { "WeekEnd", "WD:6;1-51" }                                
                            }
                        : new Dictionary<string, object>());
                long hash = 0;
                moq.Setup(s => s.GetTickers(out hash)).Returns<List<TradeTicker>>(list => new List<TradeTicker>
                    {
                        new TradeTicker
                            {
                                Title = "EURUSD",
                                ActiveBase = "EUR",
                                ActiveCounter = "USD",
                                CodeFXI = 1,
                                Precision = 4,
                                Instrument = Instrument.Spot
                            },
                        new TradeTicker
                            {
                                Title = "GBPUSD",
                                ActiveBase = "GBP",
                                ActiveCounter = "USD",
                                CodeFXI = 2,
                                Precision = 4,
                                Instrument = Instrument.Spot
                            }
                    });
                return moq;
            }
        }
    }
}
