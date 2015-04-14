using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteAdmin.Models
{
    public class BalanceChangeRequest
    {
        public int AccountId { get; set; }

        public float Amount { get; set; }

        [StringLength(60, MinimumLength = 0, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess60")]
        public string Description { get; set; }

        public BalanceChangeType ChangeType { get; set; }

        public string ValueDate { get; set; }
    }
}