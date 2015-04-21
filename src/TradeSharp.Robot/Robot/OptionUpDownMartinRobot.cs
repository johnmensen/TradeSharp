using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Тест Б.О.")]
    public class OptionUpDownMartinRobot : BaseRobot
    {
        #region Settings

        private decimal betSize = 10;
        [PropertyXMLTag("BetSize")]
        [DisplayName("Ставка")]
        [Category("Торговые")]
        [Description("Ставка, USD")]
        public decimal BetSize
        {
            get { return betSize; }
            set { betSize = value; }
        }

        private decimal martinRate = 2;

        [PropertyXMLTag("MartinRate")]
        [DisplayName("К. Мартина")]
        [Category("Торговые")]
        [Description("Коэффициент Мартингейла")]
        public decimal MartinRate
        {
            get { return martinRate; }
            set { martinRate = value; }
        }

        private int percentWin = 70;
        [PropertyXMLTag("PercentWin")]
        [DisplayName("Ставка - выигрыш")]
        [Category("Торговые")]
        [Description("Процент выигрыша по сработавшему опциону")]
        public int PercentWin
        {
            get { return percentWin; }
            set { percentWin = value; }
        }
        
        private int percentLose = 5;
        [PropertyXMLTag("PercentLose")]
        [DisplayName("Ставка - возврат")]
        [Category("Торговые")]
        [Description("Процент возврата по не сработавшему опциону")]
        public int PercentLose
        {
            get { return percentLose; }
            set { percentLose = value; }
        }

        private int maxMartinSeries = 5;
        [PropertyXMLTag("MaxMartinSeries")]
        [DisplayName("Макс сделок")]
        [Category("Торговые")]
        [Description("Макс. сделок подряд")]
        public int MaxMartinSeries
        {
            get { return maxMartinSeries; }
            set { maxMartinSeries = value; }
        }

        private int candlesBeforeEnter = 4;
        [PropertyXMLTag("CandlesBeforeEnter")]
        [DisplayName("Шаблон, свечек")]
        [Category("Торговые")]
        [Description("Свечек подряд перед входом")]
        public int CandlesBeforeEnter
        {
            get { return candlesBeforeEnter; }
            set { candlesBeforeEnter = value; }
        }
        #endregion

        #region Variables

        private List<string> events;

        private Dictionary<string, CandlePacker> packers;

        private Dictionary<string, RestrictedQueue<CandleData>> lastCandles;

        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new OptionUpDownMartinRobot
            {
                MartinRate = MartinRate,
                PercentWin = PercentWin,
                PercentLose = PercentLose,
                MaxMartinSeries = MaxMartinSeries,
                CandlesBeforeEnter = CandlesBeforeEnter
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(robotContext, protectedContext);
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("OptionUpDownMartinRobot: настройки графиков не заданы");
                return;
            }
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            lastCandles = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<CandleData>(candlesBeforeEnter));
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = new List<string>();
            if (packers == null || packers.Count == 0) return events;

            for (var i = 0; i < quotes.Length; i++)
            {
                var quote = quotes[i];
                var name = names[i];
                CandlePacker packer;
                if (!packers.TryGetValue(name, out packer))
                    continue;
                var candle = packer.UpdateCandle(quote);
                lastCandles[name].Add(candle);
            }

            return events;
        }
    }
}
