using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    public interface IModuleStatus
    {
        [OperationContract(IsOneWay = false)]
        ServiceStateInfo GetModuleStatus();

        [OperationContract(IsOneWay = false)]
        string GetModuleExtendedStatusString();

        [OperationContract(IsOneWay = true)]
        void ResetStatus();
    }
}
