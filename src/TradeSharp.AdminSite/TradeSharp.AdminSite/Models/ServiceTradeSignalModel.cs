using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// вспомогательный класс переопределяющий метаданные класса 'PaidService'
    /// </summary>
    public class ServiceTradeSignalMetadata
    {
        /// <summary>
        /// Уникальный идентификатор сигнала
        /// </summary>
        [LocalizedDisplayName("TitleUniqueIdentifier")]
        public int Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя - владельца сигналов
        /// </summary>
        [LocalizedDisplayName("TitleUser")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public int User { get; set; }

        [LocalizedDisplayName("TitleComment")]
        public string Comment { get; set; }

        [LocalizedDisplayName("TitleCurrency")]
        public string Currency { get; set; }

        /// <summary>
        /// если это ПАММ или подписка на сигналы - ссылка на торговый счет
        /// Это должен быть не любой торговый счёт, а один из счетов пользователя сигнаьльщика
        /// </summary>
        [LocalizedDisplayName("TitleAccount")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public string AccountId { get; set; }

        [LocalizedDisplayName("TitleFixedPrice")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public decimal FixedPrice { get; set; }

        [LocalizedDisplayName("TitleTradeSignalType")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public PaidServiceType ServiceType { get; set; }
    }

    /// <summary>
    /// Класс описывает отдельный элемет из списка всех сигналов, на которое подписан какой либо счёт
    /// </summary>
    [MetadataType(typeof(ServiceTradeSignalMetadata))]
    public class ServiceTradeSignalModel : PaidService
    {
        public string UserLogin { get; set; }

        public int CountSubscriber { get; set; }
    }
}