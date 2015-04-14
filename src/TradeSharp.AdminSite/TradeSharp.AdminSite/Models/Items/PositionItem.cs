using System;
using System.ComponentModel.DataAnnotations;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.BL.Localisation;
using TradeSharp.SiteAdmin.Helper;

namespace TradeSharp.SiteAdmin.Models.Items
{
    /// <summary>
    /// вспомогательный класс переопределяющий метаданные класса 'MarketOrder'
    /// </summary>
    public class PositionItemMetadata
    {
        // Аттрибут 'Editable' показывает, что поле отебражается в таблице 'Безопасно' редактируемых полей. 
        // Если этот аттрибут отсутствует, то поле может быть редактируемо в полях с 'перещетом на сервере' 

        #region общие свойства
        [ValidationStringArrayUnallowable(unallowableValue = new[] {"-1" })]
        public int AccountID { get; set; }

        /// <summary>
        /// Инструмент
        /// </summary>
        [Editable(true)]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        [ValidationStringArrayUnallowable(unallowableValue = new[] { "-1", "null" })]
        public string Symbol { get; set; }

        [Editable(true)]
        [ValidationArrayAllow(allowValue = new []{-1, 1})]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        [LocalizedDescriptionAttribute("TextPropertyPossibleValues1")]      
        public int Side { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [LocalizedDescriptionAttribute("TextPropertyPossibleValues0123")]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        public PositionState State { get; set; }

        [Editable(true)]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        [Range(0.0, int.MaxValue)]
        public int Volume { get; set; }

        [Editable(true)]
        [ValidationVector]
        [Range(0.0, (double)decimal.MaxValue)]
        public float? TakeProfit { get; set; }

        [Editable(true)]
        public int? Magic { get; set; }

        [Editable(true)]
        public string ExpertComment { get; set; }

        [Editable(true)]
        [Range(0.0, (double)decimal.MaxValue)]
        public float? StopLoss { get; set; }

        [Editable(true)]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        [Range(0.0, (double)decimal.MaxValue)]
        public decimal PriceEnter { get; set; }

        [Editable(true)]
        [DanderRange(true, "TextPropertyRequirRecalculationDeposit")]
        [Range(0.0, (double)decimal.MaxValue)]
        public decimal? PriceExit { get; set; }

        [Editable(true)]
        [LocalizedDescriptionAttribute("TextPropertyPossibleValues01234567")]
        public PositionExitReason ExitReason { get; set; }
       
        // ReSharper disable InconsistentNaming
        [Editable(true)]
        public int? PendingOrderID { get; set; }
        // ReSharper restore InconsistentNaming

        [Editable(true)]
        public string Comment { get; set; }

        [Editable(true)]
        public DateTime TimeEnter { get; set; }

        [LocalizedDisplayName("TitleTimeExit")]
        [Editable(true)]
        public string StrTimeEnter { get; set; }
        #endregion

        #region открытые
        [Editable(true)]
        [DealState(true)]
        public float? TrailLevel1 { get; set; }

        [Editable(true)]
        [DealState(true)]
        public float? TrailLevel2 { get; set; }

        [Editable(true)]
        [DealState(true)]
        public float? TrailLevel3 { get; set; }

        [Editable(true)]
        [DealState(true)]
        public float? TrailLevel4 { get; set; }
        #endregion
        
        #region закрытые
        [Editable(true)]
        [DealState(false)]
        public DateTime? TimeExit { get; set; }
        #endregion
    }

    /// <summary>
    /// Вспомогательный класс, переопределяющий некоторые свойства "Position" в более удобный вид для таблици представления PositionListModel
    /// </summary>
    [MetadataType(typeof(PositionItemMetadata))]
    public class PositionItem : MarketOrder
    {
        /// <summary>
        /// Цена входа в ранок. Переопределено для того, что бы оно было decimal, а не float
        /// </summary>
        [LocalizedDisplayName("TitlePriceEntry")]
        public new decimal PriceEnter { get; set; }

        /// <summary>
        /// Цена выхода из рынка. Для таблици открытых позиций это всегда null, для таблици закрытых - переопределённым. 
        /// Переопределено для того, что бы оно было decimal, а не float
        /// </summary>
        [LocalizedDisplayName("TitlePriceExit")]
        public new decimal? PriceExit { get; set; }

        /// <summary>
        /// профит по этой сделке, расчитанный в пунктах. Есть POSITION_CLOSED, но нет в POSITION
        /// Переопределено для того, что бы оно было decimal, а не float
        /// </summary>    
        public new decimal? ResultPoints { get; set; }

        /// <summary>
        /// Профит по этой сделке, расчитанный в валюте депозита. Есть POSITION_CLOSED, но нет в POSITION
        /// Переопределено для того, что бы оно было decimal, а не float
        /// </summary>
        [LocalizedDisplayName("TitleResultDepo")]
        public new decimal? ResultDepo { get; set; }

        /// <summary>
        /// Свойство нужно для выбора галочкой нескольких строк
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Вспомогательное свойство. возникло из за того, что с типом DateTime возникли проблемы валидации на клиенте
        /// </summary>
        [LocalizedDisplayName("TitleTimeOpen")]
        public string StrTimeEnter { get; set; }


        public PositionItem()
        {
        }
        
        /// <summary>
        /// Конструктор преобразования MarketOrder в PositionItem
        /// </summary>
        public PositionItem(MarketOrder c)
        {
            foreach (var prop in c.GetType().GetProperties())
            {
                var prop2 = c.GetType().GetProperty(prop.Name);
                if (prop2.GetSetMethod() != null)
                    prop2.SetValue(this, prop.GetValue(c, null), null);
            }

            PriceEnter = (decimal) c.PriceEnter;
            PriceExit = (decimal?)c.PriceExit;
            ResultPoints = (decimal?)c.ResultPoints;
            ResultDepo = (decimal?)c.ResultDepo;
        }
    }
}