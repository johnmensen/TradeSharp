using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [XmlSerializerFormat]
    [ServiceContract]
    public interface ITradeSignalExecutor
    {
        [OperationContract(IsOneWay = true)]
        void ProcessTradeSignals(List<TradeSignalAction> actions);
    }
}
