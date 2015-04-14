using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IDealerRepository
    {
        IEnumerable<DealerDescription> GetAllDealerDescription();
        IEnumerable<DealerDescription> GetAllDealerDescription(TradeSharpConnection ctx);
        bool SaveChangesDealerFealdInAccountGroup(AccountGroup accGrp);
        bool SaveDealerChanges(DealerDescription dealerDescription);
        bool UpdateDealerForAccountGroup(string groupCode, string dealerCode);
    }
}