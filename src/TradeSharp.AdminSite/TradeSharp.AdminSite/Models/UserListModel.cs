using MvcPaging;

namespace TradeSharp.SiteAdmin.Models
{
    public class UserListModel : ListModel
    {
        /// <summary>
        /// Записи текущей страници таблици
        /// </summary>
        public PagedList<AccountUserModel> CurrentPageItems { get; set; }
    }
}