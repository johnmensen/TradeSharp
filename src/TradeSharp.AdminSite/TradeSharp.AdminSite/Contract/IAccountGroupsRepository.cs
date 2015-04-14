using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IAccountGroupsRepository
    {
        IEnumerable<AccountGroupItem> GetAccountGroups(string groupCode = "");
        string SaveAccountGroupChanges(AccountGroup accGrp);
        string AddAccountGroup(AccountGroupItem newAccountGroup);
        string DeleteVoidAccountGroup(string accountGroupCode);
    }
}