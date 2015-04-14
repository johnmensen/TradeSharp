using System.Collections.Generic;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IPammRepository
    {
        List<PammItem> GetAllPamm(bool anyInvestor);
        PammItem GetPammById(int id);
        ACCOUNT_SHARE_HISTORY[] GetAccountHistory(int id);
    }
}