using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Admin.Util
{
    public class UserTag
    {
        public PLATFORM_USER user;
        public string Title { get { return user.Title; } }
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(user.Name) && string.IsNullOrEmpty(user.Surname)) return user.Title;
                if (string.IsNullOrEmpty(user.Name)) return user.Surname;
                if (string.IsNullOrEmpty(user.Surname)) return user.Name;
                return string.Format("{0} {1}", user.Name, user.Surname);
            }
        }
        public UserRole RoleMask { get { return (UserRole)user.RoleMask; } }

        public IEnumerable<int> UserAccounts { get; set; }

        public string UserAccountsString
        {
            get { return UserAccounts == null ? "" : UserAccounts.ToStringUniform(", "); }
        }
    }
}
