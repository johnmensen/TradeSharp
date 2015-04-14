using System.ComponentModel;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public class AccountGroup
    {
        public enum MarkupType
        {
            NoMarkup = 0,
            Markup = 1,
            MarkupIsIncludedInQuote = 2
        }

        /// <summary>
        /// уникальный код группы (ключ)
        /// </summary>
        [LocalizedDisplayName("TitleAccountGroupCode")]
        public string Code { get; set; }

        /// <summary>
        /// наименование группы
        /// </summary>
        [LocalizedDisplayName("TitleAccountGroupName")]
        public string Name { get; set; }

        [LocalizedDisplayName("TitleAccountGroupIsReal")]
        public bool IsReal { get; set; }

        [LocalizedDisplayName("TitleInitialVirtualDepo")]
        public int DefaultVirtualDepo { get; set; }

        [LocalizedDisplayName("TitleBrokerLeverage")]
        public float BrokerLeverage { get; set; }

        /// <summary>
        /// 100 * reserved margin / equity
        /// </summary>
        [DisplayName("Margin Call")]
        public float MarginCallPercentLevel { get; set; }

        private float stopoutPercentLevel;
        /// <summary>
        /// 100 * reserved margin / equity
        /// </summary>
        [DisplayName("Stopout")]
        public float StopoutPercentLevel
        {
            get { return stopoutPercentLevel; }
            set 
            { 
                stopoutPercentLevel = value;
                if (value > 100) stopoutPercentLevel = 100;
            }
        }
        
        // ассоциация с дилером организована как многие ко многим
        // в действительности с группой может быть ассоциировано не более
        // одного дилера
        /// <summary>
        /// заполняется отдельным запросом
        /// </summary>
        [LocalizedDisplayName("TitleAccountGroupDealer")]
        public DealerDescription Dealer { get; set; }

        /// <summary>
        /// атрибут ассоциации с дилером (заполняется отдельным запросом)
        /// </summary>
        [LocalizedDisplayName("TitleMessageQueue")]
        public string MessageQueue { get; set; }

        /// <summary>
        /// атрибут ассоциации с дилером (заполняется отдельным запросом)
        /// </summary>
        [LocalizedDisplayName("TitleSessionName")]
        public string SessionName { get; set; }

        /// <summary>
        /// счет провайдера, на котором хеджируются сделки данной группы (заполняется отдельным запросом)
        /// </summary>
        [LocalizedDisplayName("TitleHedgingAccount")]
        public string HedgingAccount { get; set; }

        /// <summary>
        /// ID компьютера-отправителя (заполняется отдельным запросом)
        /// </summary>
        [LocalizedDisplayName("TitleSenderCompId")]
        public string SenderCompId { get; set; }

        /// <summary>
        /// тип начисления маркапа (уже включен в котировку или прибавляется к ней)
        /// </summary>
        [LocalizedDisplayName("TitleMarkup")]
        public MarkupType Markup { get; set; }

        /// <summary>
        /// значение марк-апа по-умолчанию
        /// </summary>
        [LocalizedDisplayName("TitleDefaultMarkup")]
        public float DefaultMarkupPoints { get; set; }

        /// <summary>
        /// Соответствующее поле из БД таблици ACCOUNT_GROUP
        /// </summary>
        [LocalizedDisplayName("TitleSwapFree")]
        public bool SwapFree { get; set; }
    }
}