using System.ComponentModel.DataAnnotations;
using TradeSharp.Contract.Entity;
using Localisation = TradeSharp.SiteAdmin.BL.Localisation;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.Report
{
    public class PerformerStatRecordMetadata
    {
        [Localisation.LocalizedDisplayName("TitleYearIncomeShort")]
        [ExpressionParameterName("AYP", "Сгеом. годовой доход, %", "%")]
        [DisplayFormat(DataFormatString = "{0:F1}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 15)]
        public float AvgYearProfit { get; set; }

        [Localisation.LocalizedDisplayName("TitleSharpFactor")]
        [ExpressionParameterName("Sharp", "Коэфф. Шарпа")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 0)]
        public float Sharp { get; set; }

        [Localisation.LocalizedDisplayName("TitleProfit")]
        [ExpressionParameterName("P", "Прибыль, %", "%")]
        [DisplayFormat(DataFormatString = "{0:F1}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 15)]
        [PropertyOrder(3)]
        public float Profit { get; set; }

        [Localisation.LocalizedDisplayName("TitleMaxLeverage")]
        [ExpressionParameterName("ML", "Макс. плечо")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 10)]
        public float MaxLeverage { get; set; }

        [Localisation.LocalizedDisplayName("TitleAvgLeverage")]
        [ExpressionParameterName("AL", "Среднее плечо")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 5)]
        public float AvgLeverage { get; set; }

        [Localisation.LocalizedDisplayName("TitleMaxDropDown")]
        [ExpressionParameterName("DD", "Макс. относительное проседание, %", "%")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 30)]
        [PropertyOrder(9)]
        public float MaxRelDrawDown { get; set; }
    }

    /// <summary>
    /// Привязывается к частичному представлению
    /// </summary>
    [MetadataType(typeof(PerformerStatRecordMetadata))]
    public class PerformerStatRecord : PerformerStat
    {
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public double FunctionValue { get; set; }

        public PerformerStatRecord()
        {            
        }

        public PerformerStatRecord(PerformerStat record) : base(record)
        {
        }
    }
}