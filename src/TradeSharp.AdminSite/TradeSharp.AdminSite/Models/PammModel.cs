using MvcPaging;
using TradeSharp.SiteAdmin.BL.Localisation;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Models
{
    public class PammModel
    {
        /// <summary>
        /// Записи текущей страници таблици
        /// </summary>
        public PagedList<PammItem> CurrentPageItems { get; set; }

        private readonly int[] pageSizeItems = new[] { 10, 15, 25, 50, 100 };
        /// <summary>
        /// возможные значения размеров страници
        /// </summary>
        public int[] PageSizeItems
        {
            get
            {
                return pageSizeItems;
            }
        }

        /// <summary>
        /// Номер текущей страници
        /// </summary>
        public int PageNomber { get; set; }

        /// <summary>
        /// Текущий размер страници
        /// </summary>
        public int CurrentPageSize { get; set; }

        /// <summary>
        /// Показавать только те ПАММЫ-ы, у которых есть хоть ли один инвестор
        /// </summary>
        [LocalizedDisplayName("TitleAnyInvestor")]
        public bool AnyInvestor { get; set; }
    }
}