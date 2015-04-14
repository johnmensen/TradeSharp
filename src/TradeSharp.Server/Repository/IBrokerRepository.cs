using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Server.Repository
{
    interface IBrokerRepository
    {
        string BrokerCurrency { get; }

        Dictionary<string, object> GetMetadataByCategory(string catName);

        List<AccountGroup> GetAccountGroupsWithSessionInfo();
    }
}
