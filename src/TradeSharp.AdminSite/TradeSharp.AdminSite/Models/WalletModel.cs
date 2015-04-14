using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Модель для педактирования средств на кошельке и перевода средств
    /// </summary>
    public class WalletModel
    {
        [LocalizedDisplayName("TitleWalletUserId")]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess1")]
        public int WalletId { get; set; }

        [LocalizedDisplayName("Login")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(50, MinimumLength = 7, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength7_50")]
        public string UserLogin { get; set; }

        /// <summary>
        /// Валюта кошелька T#, пренадлежащего данному пользователю 
        /// </summary>
        [LocalizedDisplayName("TitleCurrencyPurse")]
        public string WalletCurrency { get; set; }

        /// <summary>
        /// Средства на кошельке T#, пренадлежащего данному пользователю 
        /// </summary>
        [LocalizedDisplayName("TitleWalletBalance")]
        public decimal WalletBalance { get; set; }

        [LocalizedDisplayName("TitleTransferVolume")]
        [Range(0.01, float.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess001")]
        public decimal TransferVolume { get; set; }

        [LocalizedDisplayName("TitleAccountToTransfer")]
        public int AccountId { get; set; }

        /// <summary>
        /// Пересчитывать ли средства на кошельке в новой валюте.
        /// если было 10 000 USD то, выбирая RUR получим 38 520 RUR
        /// </summary>
        [LocalizedDisplayName("TitleRecalculationInNewCurrency")]
        public bool RecalculationBalance { get; set; }

        [LocalizedDisplayName("TitleAutoTransferOnAccount")]
        public bool TransferToAccount { get; set; }

        [LocalizedDisplayName("TitleAutoWithdrawFromAccount")]
        public bool TransferToWallet { get; set; }
    }
}