using System.Collections.Generic;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.BL.Contract
{
    public interface IServerInstanceRepository
    {
        PagedListResult<ServerInstance> GetAllServers(string sortBy, bool ascending, int skip, int take);
        List<string> GetServerCodes();
    }
}
