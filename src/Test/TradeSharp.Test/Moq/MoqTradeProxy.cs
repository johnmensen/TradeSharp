using System.Collections.Generic;
using Moq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Test.Moq
{
    static class MoqTradeProxy
    {
        public static Mock<ITradeSharpServerTrade> Moq
        {
            get
            {
                var moq = new Mock<ITradeSharpServerTrade>();
                moq.Setup(s => s.GetTradeSignalsSubscribed(It.IsAny<int>())).Returns<int>(
                    accountId =>
                    accountId == 3
                        ? new List<TradeSignalCategory>
                            {
                                new TradeSignalCategory
                                    {
                                        Id = 1,
                                        Title = "Test signals",
                                        PercentLeverage = 50,
                                        MaxLeverage = 10,
                                        MinVolume = 10000,
                                        StepVolume = 5000,
                                        RightsMask = TradeSignalCategory.SubscriberFlag | TradeSignalCategory.TradeAutoFlag
                                    }
                            }
                        : new List<TradeSignalCategory>());
                
                moq.Setup(s => s.BindToTradeSignal(It.IsAny<int>(), It.IsAny<TradeSignalCategory>()))
                   .Returns<int, TradeSignalCategory>(
                       (accountId, cat) =>
                           {
                               
                               return true;
                           });
                return moq;
            }
        }
    }
}
