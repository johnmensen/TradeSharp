using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.DealerInterface
{
    public interface IFixDealer
    {
        void ProcessExecutionReport(BrokerResponse response, BrokerOrder request);
    }
}