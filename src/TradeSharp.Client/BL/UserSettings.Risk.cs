using Entity;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.BL
{
    partial class UserSettings
    {
        public const string DefTickerPlaceholder = "default";

        [PropertyXMLTag("MessageOnEnterExceedLeverage")]
        public bool MessageOnEnterExceedLeverage { get; set; }

        [PropertyXMLTag("RoundCalculatedDealVolume")]
        public bool RoundCalculatedDealVolume { get; set; }

        [PropertyXMLTag("VolumeRoundType")]
        public VolumeRoundType VolumeRoundType { get; set; }

        private float riskLeverWarning = 5;
        [PropertyXMLTag("RiskLeverWarning")]
        public float RiskLeverWarning
        {
            get { return riskLeverWarning; }
            set { riskLeverWarning = value; }
        }

        private float riskLeverCritical = 10;
        [PropertyXMLTag("RiskLeverCritical")]
        public float RiskLeverCritical
        {
            get { return riskLeverCritical; }
            set { riskLeverCritical = value; }
        }

        [PropertyXMLTag("DealLeverage")]
        public decimal? DealLeverage { get; set; }

        private int dealByTickerCount = 5;
        [PropertyXMLTag("DealByTickerCount")]
        public int DealByTickerCount { get { return dealByTickerCount; } set { dealByTickerCount = value; } }

        private int tickerTradedCount = 4;
        [PropertyXMLTag("TickerTradedCount")]
        public int TickerTradedCount { get { return tickerTradedCount; } set { tickerTradedCount = value; } }

        private decimal maxLeverageTotal = 20;
        [PropertyXMLTag("MaxLeverageTotal")]
        public decimal MaxLeverageTotal { get { return maxLeverageTotal; } set { maxLeverageTotal = value; } }

        private bool useLeverageByDefault = true;
        [PropertyXMLTag("UseLeverageByDefault")]
        public bool UseLeverageByDefault
        {
            get { return useLeverageByDefault; }
            set { useLeverageByDefault = value; }
        }
    }
}
