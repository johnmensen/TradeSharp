using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// этот класс нужен в основном для метаданных
    /// Сам класс дублирует поля класа SERVICE
    /// </summary>
    public class TradeSignalModel
    {
        public int ID { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public int User { get; set; }


        public string UserLogin { get; set; }

        [StringLength(128, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess128")]
        public string Comment { get; set; }

        public string Currency { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public int? AccountId { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [Range(0, float.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeNotLess0")]
        public decimal FixedPrice { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public short ServiceType { get; set; }


        /// <summary>
        /// Количество подписчиков на данный сигнал
        /// </summary>
        public int СountSubscriber { get; set; }
    }
}