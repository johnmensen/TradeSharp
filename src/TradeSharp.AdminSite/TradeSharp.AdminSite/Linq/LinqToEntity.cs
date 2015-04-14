using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Linq
{
    public static class SiteAdminLinqToEntity
    {
        public static AccountUserModel DecoratePlatformUser(PLATFORM_USER platformUser)
        {
            var result = new AccountUserModel
            {
                UserId = platformUser.ID,
                UserName = platformUser.Name,
                UserSurname = platformUser.Surname,
                UserPatronym = platformUser.Patronym,
                UserDescription = platformUser.Description,
                UserEmail = platformUser.Email,
                UserLogin = platformUser.Login,
                UserPassword = platformUser.Password,
                UserPhone1 = platformUser.Phone1,
                UserPhone2 = platformUser.Phone2,
                UserRoleMask = (UserRole)platformUser.RoleMask,
                UserRightsMask = new Dictionary<int, UserAccountRights>(),
                UserRegistrationDate = platformUser.RegistrationDate
            };

            return result;
        }

    }
}