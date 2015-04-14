using MvcPaging;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models
{
    public class TradeSignalListModel : ListModel
    {
        [LocalizedDisplayName("TitleFiltrateSignaller")]
        public string SignallerFilterText { get; set; } 

        public PagedList<TradeSignalModel> CurrentPageItems { get; set; }
    }
}