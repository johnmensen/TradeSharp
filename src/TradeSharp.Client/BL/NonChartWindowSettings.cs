using Entity;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// окошко - не чарт
    /// например, окно Счет, окно доходности и т.п.
    /// </summary>
    public class NonChartWindowSettings : BaseWindowSettings
    {
        /// <summary>
        /// вкладка, на которой присутствует текущее окно (окно доходности / окно Счет ...)
        /// </summary>
        [PropertyXMLTag("ChartTab")]
        public long ChartTab { get; set; }      
  
        public enum WindowCode
        {
            Account = 0,
            Profit = 1,
            Chat = 2,
            RobotTest = 3,
            Quotes = 4,
            Subscription = 5,
            WebBrowser = 6,
            RiskForm = 7,
            WalletForm = 8
        }

        [PropertyXMLTag("WindowCode")]
        public WindowCode Window { get; set; }

        /// <summary>
        /// индекс текущей открытой закладки - если на страничке есть закладки
        /// </summary>
        [PropertyXMLTag("CurrentTabIndex")]
        public int CurrentTabIndex { get; set; }
    }
}
