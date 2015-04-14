using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IUserRepository
    {
        List<AccountUserModel> GetAllPlatformUser();
        bool DeletePlatformUser(int id, out List<Account> accountsWoOwners);
        AccountUserModel GetUserInfoById(int id);
        PlatformUser GetAccountOwner(int id);
        bool EditUserInfo(AccountUserModel userModelData);
        List<string> GetLoginSubscriber(int signalId);
        int? GetSignalCount(int id);
    }
}