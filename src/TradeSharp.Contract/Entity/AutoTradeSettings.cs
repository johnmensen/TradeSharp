using System;
using System.Xml;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class AutoTradeSettings
    {
        /// <summary>
        /// для сигнальщика: признак того, что по сигналу можно торговать
        /// для "инвестора": получать уведомления о открытии / закрытии сделки / передвижении SL- TP
        /// </summary>
        [UiField("Торговать автоматически")]
        public bool TradeAuto { get; set; }

        private double? maxLeverage = 5;
        [UiField("Макс. плечо")]
        public double? MaxLeverage
        {
            get { return maxLeverage; }
            set { maxLeverage = value; }
        }

        private double percentLeverage = 100;
        [UiField("Процент плеча")]
        public double PercentLeverage
        {
            get { return percentLeverage; }
            set { percentLeverage = value; }
        }
        
        [UiField("Хедж. ордера")]
        public bool? HedgingOrdersEnabled { get; set; }
        
        [UiField("Фиксированный объем")]
        public int? FixedVolume { get; set; }

        [UiField("Округление объема")]
        public VolumeRoundType? VolumeRound { get; set; }

        private int? minVolume = 10000;
        [UiField("Мин. объем")]
        public int? MinVolume
        {
            get { return minVolume; }
            set { minVolume = value; }
        }

        [UiField("Макс. объем")]
        public int? MaxVolume { get; set; }

        private int? stepVolume = 10000;
        [UiField("Шаг объема")]
        public int? StepVolume
        {
            get { return stepVolume; }
            set { stepVolume = value; }
        }

        /// <summary>
        /// счет, на котором будут исполняться ордера
        /// </summary>
        [UiField("Счет")]
        public int? TargetAccount { get; set; }

        public AutoTradeSettings MakeCopy()
        {
            var cat = new AutoTradeSettings
                          {
                              TradeAuto = TradeAuto,
                              MaxLeverage = MaxLeverage,
                              MinVolume = MinVolume,
                              MaxVolume = MaxVolume,
                              PercentLeverage = PercentLeverage,
                              StepVolume = StepVolume,
                              FixedVolume = FixedVolume,
                              HedgingOrdersEnabled = HedgingOrdersEnabled,
                              VolumeRound = VolumeRound,
                              TargetAccount = TargetAccount
                          };
            return cat;
        }

        public bool AreSame(AutoTradeSettings cat)
        {
            return MaxLeverage == cat.MaxLeverage && MinVolume == cat.MinVolume && MaxVolume == cat.MaxVolume &&
                   PercentLeverage == cat.PercentLeverage && StepVolume == cat.StepVolume &&
                   FixedVolume == cat.FixedVolume && HedgingOrdersEnabled == cat.HedgingOrdersEnabled &&
                   VolumeRound == cat.VolumeRound && TargetAccount == cat.TargetAccount;
        }

        public void SaveTradeSettings(XmlElement parent, string childNodeName = "autoTradeSettings")
        {
            var node = (XmlElement) parent.AppendChild(parent.OwnerDocument.CreateElement(childNodeName));
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("tradeAuto")).Value = TradeAuto.ToString();
            if (MaxLeverage.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("maxLeverage")).Value = MaxLeverage.Value.ToStringUniform();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("percentLeverage")).Value = PercentLeverage.ToString();
            if (HedgingOrdersEnabled.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("hedgingOrdersEnabled")).Value = HedgingOrdersEnabled.Value.ToString();
            if (FixedVolume.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("fixedVolume")).Value = FixedVolume.Value.ToString();
            if (VolumeRound.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("volumeRound")).Value = VolumeRound.Value.ToString();
            if (MinVolume.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("minVolume")).Value = MinVolume.Value.ToString();
            if (MaxVolume.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("maxVolume")).Value = MaxVolume.Value.ToString();
            if (StepVolume.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("stepVolume")).Value = StepVolume.Value.ToString();
            if (TargetAccount.HasValue)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("targetAccount")).Value = TargetAccount.Value.ToString();
        }

        public void LoadTradeSettings(XmlElement node)
        {
            if (node == null) return;
            var tradeAuto = node.GetAttributeBool("tradeAuto");
            if (tradeAuto.HasValue)
                TradeAuto = tradeAuto.Value;

            MaxLeverage = node.GetAttributeDouble("maxLeverage");
            PercentLeverage = node.GetAttributeInt("percentLeverage") ?? 100;
            HedgingOrdersEnabled = node.GetAttributeBool("hedgingOrdersEnabled");
            FixedVolume = node.GetAttributeInt("fixedVolume");
            if (node.Attributes["volumeRound"] != null)
            {
                VolumeRoundType volRnd;
                VolumeRound = Enum.TryParse(node.Attributes["volumeRound"].Value, out volRnd)
                                  ? volRnd
                                  : (VolumeRoundType?)null;
            }
            MinVolume = node.GetAttributeInt("minVolume");
            MaxVolume = node.GetAttributeInt("maxVolume");
            StepVolume = node.GetAttributeInt("stepVolume");
            TargetAccount = node.GetAttributeInt("targetAccount");
        }
    }
}
