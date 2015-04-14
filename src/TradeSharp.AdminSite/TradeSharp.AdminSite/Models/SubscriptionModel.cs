using System;
using System.ComponentModel.DataAnnotations;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// вспомогательный класс переопределяющий метаданные класса Subscription
    /// </summary>
    public class SubscriptionMetadata
    {
        [LocalizedDisplayName("TitleTradeSignalNumber")]
        public int Service { get; set; }

        [LocalizedDisplayName("TitleTradeSignalNumber")]
        public int User { get; set; }

        [LocalizedDisplayName("TitleSubscriptionStartTime")]
        public DateTime TimeEnd { get; set; }

        [LocalizedDisplayName("TitleSubscriptionEndTime")]
        public DateTime TimeStarted { get; set; }

        [LocalizedDisplayName("TitleAutoExtensionShort")]
        public bool RenewAuto { get; set; }      
    }


    /// <summary>
    /// Класс описывает отдельный элемет из списка всех сигналов, на которое подписан какой либо счёт
    /// </summary>
    [MetadataType(typeof(SubscriptionMetadata))]
    public class SubscriptionModel : Subscription
    {
        public int SignalOwnerId { get; set; }

        [LocalizedDisplayName("TitleType")]
        public int ServiceType { get; set; }

        [LocalizedDisplayName("TitleSignaller")]
        public string SignalOwnerLogin { get; set; }
    }
}