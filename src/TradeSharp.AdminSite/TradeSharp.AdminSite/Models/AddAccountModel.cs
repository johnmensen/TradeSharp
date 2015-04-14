using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.BL.Localisation;
using System.ComponentModel.DataAnnotations;

namespace TradeSharp.SiteAdmin.Models
{
    public class AddAccountModel : AccountUserModel
    {
        private string accountCurrency = "USD";
        private float accountBalance = 100000;
        private float walletBalance = 100000;
        private UserAccountRights userRightsMask = UserAccountRights.Trade;

        [LocalizedDisplayName("TitleRights")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public new UserAccountRights UserRightsMask 
        { 
            get { return userRightsMask; }
            set { userRightsMask = value; }
        }

        [LocalizedDisplayName("TitleCurrency")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public string AccountCurrency
        {
            get { return accountCurrency; }
            set { accountCurrency = value; }
        }

        [LocalizedDisplayName("TitleAccountGroup")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public string AccountGroup { get; set; }

        [LocalizedDisplayName("TitleBalance")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [Range(1, float.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess1")]
        public float AccountBalance
        {
            get { return accountBalance; }
            set { accountBalance = value; }
        }

        [LocalizedDisplayName("TitleMaxLeverage")]
        [Range(0, float.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess0")]
        public float AccountMaxLeverage { get; set; }

        [LocalizedDisplayName("TitleWalletBalance")]
        [Range(0, float.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess0")]
        public new float WalletBalance
        {
            get { return walletBalance; }
            set { walletBalance = value; }
        }
    }
}