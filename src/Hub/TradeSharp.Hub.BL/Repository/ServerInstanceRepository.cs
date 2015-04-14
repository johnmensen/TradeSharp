using System.Collections.Generic;
using System.Linq;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Model;
using TradeSharp.Util;

namespace TradeSharp.Hub.BL.Repository
{
    public class ServerInstanceRepository : EntityRepository<ServerInstance>, IServerInstanceRepository
    {
        public PagedListResult<ServerInstance> GetAllServers(string sortBy, bool ascending, int skip, int take)
        {
            return GetItemsPaged(speciman.Property(s => s.Currency), sortBy, ascending, skip, take);
        }

        public List<string> GetServerCodes()
        {
            return context.ServerInstance.Select(x => x.Code).ToList();
        }
    }
}