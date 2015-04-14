using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    public interface IRobotFarm
    {
        [OperationContract(IsOneWay = false)]
        bool GetAccountData(string login, string pwrd, int accountId,
            out Account account, out List<MarketOrder> openedOrders);
    }
}
