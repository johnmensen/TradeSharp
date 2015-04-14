using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Admin.Util
{
    public class ColumnFilterUser : BaseColumnFilter
    {
        public ColumnFilterCriteria critTitle,
                                    critName,
                                    critSurname,
                                    critPatronym,
                                    critPhone,
                                    critEmail,
                                    critRole,
                                    critAccount;

        public string valTitle, valName, valSurname, valPatronym, valPhone, valEmail;
        public string valAccounts;
        public UserRole valRole;

        public override bool IsOff
        {
            get
            {
                return critTitle == ColumnFilterCriteria.Нет && critName == ColumnFilterCriteria.Нет &&
                       critSurname == ColumnFilterCriteria.Нет && critPatronym == ColumnFilterCriteria.Нет &&
                       critPhone == ColumnFilterCriteria.Нет && critEmail == ColumnFilterCriteria.Нет &&
                       critRole == ColumnFilterCriteria.Нет && critAccount == ColumnFilterCriteria.Нет;
            }
        }

        public ColumnFilterUser() {}

        public ColumnFilterUser(ColumnFilterUser us)
        {
            critTitle = us.critTitle;
            critName = us.critName;
            critSurname = us.critSurname;
            critPatronym = us.critPatronym;
            critPhone = us.critPhone;
            critEmail = us.critEmail;
            critRole = us.critRole;
            critAccount = us.critAccount;
            valTitle = us.valTitle;
            valName = us.valName;
            valSurname = us.valSurname;
            valPatronym = us.valPatronym;
            valPhone = us.valPhone;
            valEmail = us.valEmail;
            valAccounts = us.valAccounts;
            valRole = us.valRole;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ColumnFilterUser == false) return false;
            var ac = (ColumnFilterUser)obj;
            return ac.critAccount == critAccount && ac.critEmail == critEmail && ac.critName == critName
                && ac.critPatronym == critPatronym && ac.critPhone == critPhone && ac.critRole == critRole
                && ac.critSurname == critSurname && ac.critTitle == critTitle;
        }

        public override int GetHashCode()
        {
            return 150005 * (int)critAccount + 90002 * (int)critEmail + 30000 * (int)critName
                   + 10000 * (int)critPatronym + 1000 * (int)critPhone + 100 * (int)critRole
                   + 10 * (int)critSurname + (int)critSurname
                   + valTitle.GetHashCode() + valName.GetHashCode() * 10 + valSurname.GetHashCode() * 100
                   + valPatronym.GetHashCode() + valPhone.GetHashCode() * 10 + valEmail.GetHashCode() * 100
                   + valAccounts.GetHashCode() + (int)valRole * 200;
        }

        /// <summary>
        /// счета не проверяются здесь
        /// </summary>        
        public bool PredicateFunc(UserTag user)
        {
            if (IsOff) return true;
            if (critTitle != ColumnFilterCriteria.Нет)
                if (!CheckStringCriteria(critTitle, user.user.Title, valTitle)) return false;
            if (critName != ColumnFilterCriteria.Нет)
                if (!CheckStringCriteria(critName, user.user.Name, valName)) return false;
            if (critSurname != ColumnFilterCriteria.Нет)
                if (!CheckStringCriteria(critSurname, user.user.Surname, valSurname)) return false;
            if (critPatronym != ColumnFilterCriteria.Нет)
                if (!CheckStringCriteria(critPatronym, user.user.Patronym, valPatronym)) return false;
            if (critPhone != ColumnFilterCriteria.Нет)
                if (!CheckPhoneCriteria(critPhone, user.user, valPhone)) return false;
            if (critEmail != ColumnFilterCriteria.Нет)
                if (!CheckStringCriteria(critEmail, user.user.Email, valEmail)) return false;
            if (critRole != ColumnFilterCriteria.Нет)
            {
                if (critRole == ColumnFilterCriteria.Равно && user.user.RoleMask != (int)valRole) return false;
                if (critRole == ColumnFilterCriteria.НеРавно && user.user.RoleMask == (int)valRole) return false;
            }
            return true;
        }

        private static bool CheckPhoneCriteria(ColumnFilterCriteria crit, PLATFORM_USER user, string specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && user.Phone1 != specimen && user.Phone2 != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && (user.Phone1 == specimen || user.Phone2 == specimen)) return false;
            if (crit == ColumnFilterCriteria.Включает && (user.Phone1.Contains(specimen) ||
                user.Phone2.Contains(specimen)) == false) return false;
            if (crit == ColumnFilterCriteria.НачинаетсяС && (user.Phone1.StartsWith(specimen) ||
                user.Phone2.StartsWith(specimen)) == false) return false;
            if (crit == ColumnFilterCriteria.КончаетсяНа && (user.Phone1.EndsWith(specimen) ||
                user.Phone2.EndsWith(specimen)) == false) return false;
            return true;
        }
    }
}
