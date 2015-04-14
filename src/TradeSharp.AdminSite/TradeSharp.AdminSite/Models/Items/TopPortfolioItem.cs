using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models.Items
{
    public class TopPortfolioItemMetadata
    {
        [LocalizedDisplayName("TitleTopPortfolioName")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Name { get; set; }

        [LocalizedDisplayName("TitleFormulaScreeningCriterion")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public string Criteria { get; set; }

        [LocalizedDisplayName("TitleParticipantsPortfolioMax")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess1")]
        public int ParticipantCount { get; set; }

        [LocalizedDisplayName("TitleSortByFormula")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public bool DescendingOrder { get; set; }

        [LocalizedDisplayName("TitleFormulaLimitValue")]
        public float? MarginValue { get; set; }

        [LocalizedDisplayName("TitleTopPortfolioTradeAccount")]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess1")]
        public int? ManagedAccount { get; set; }
    }

    [MetadataType(typeof(TopPortfolioItemMetadata))]
    public class TopPortfolioItem : TopPortfolio
    {
        /// <summary>
        /// Создавать ли новый эталонный счёт для портфеля роботов
        /// </summary>
        [LocalizedDisplayName("TitleCreatePortfolioWithAccount")]
        public bool CreateNewAccount { get; set; }

        public AddAccountModel AddAccountModel { get; set; }
    }
}