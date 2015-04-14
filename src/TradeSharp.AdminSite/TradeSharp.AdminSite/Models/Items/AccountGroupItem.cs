using System.Collections.Generic;
using System.Web.Mvc;
using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteAdmin.Models.Items
{
    /// <summary>
    /// Это группа счетов для AccountGroupsPartialTable.cshtml
    /// Все необходимые поля наследуются от "AccountGroup"
    /// </summary>
    public class AccountGroupItem : AccountGroup
    {
        public int AccountsCount { get; set; }

        /// <summary>
        /// Выпадающий список в представлении для смены дилера
        /// </summary>
        public List<SelectListItem> Dealers { get; set; }
    }
}