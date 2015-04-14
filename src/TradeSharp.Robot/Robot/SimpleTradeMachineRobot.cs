using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("BUY-SELL")]
    [TypeConverter(typeof(PropertySorter))]
    public class SimpleTradeMachineRobot : BaseRobot
    {
        #region Настройки
        private int tradeIntervalSec = 30;
        [PropertyXMLTag("TradeIntervalSec")]
        [DisplayName("Интервал, с")]
        [Category("Торговые")]
        [Description("Интервал между сделками, сек")]
        public int TradeIntervalSec
        {
            get { return tradeIntervalSec; }
            set { tradeIntervalSec = value; }
        }

        private int stopLossPoints = 100, takeProfitPoints = 100;

        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        private int fixedVolume = 10000;
        [PropertyXMLTag("Robot.FixedVolume")]
        [DisplayName("Объем входа")]
        [Category("Торговые")]
        [Description("Объем входа, ед. базового актива")]
        public int FixedVolume
        {
            get { return fixedVolume; }
            set { fixedVolume = value; }
        }
        #endregion

        private string[] tickers;

        private ThreadSafeTimeStamp lastTrade;

        public override BaseRobot MakeCopy()
        {
            var bot = new SimpleTradeMachineRobot
                {
                    TradeIntervalSec = TradeIntervalSec,
                    StopLossPoints = StopLossPoints,
                    TakeProfitPoints = TakeProfitPoints,
                    FixedVolume = FixedVolume
                };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            tickers = Graphics.Select(g => g.a).ToArray();
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var events = new List<string>();
            if (tickers.Length == 0 || isHistoryStartOff) return events;

            if (lastTrade == null)
            {
                lastTrade = new ThreadSafeTimeStamp();
                lastTrade.Touch();
            }

            // секунд с последнего трейда...
            var timeSince = (DateTime.Now - lastTrade.GetLastHit()).TotalSeconds;
            if (timeSince < tradeIntervalSec) return events;
            
            lastTrade.Touch();

            // затребовать все сделки со своим magic
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            var sides = tickers.ToDictionary(t => t, t => 1);
            if (orders != null)
                foreach (var order in orders)
                {
                    if (sides.ContainsKey(order.Symbol))
                        sides[order.Symbol] = -order.Side;
                    // закрыть старую сделку
                    CloseMarketOrder(order.ID);
                }
            
            // открыть новые сделки
            foreach (var tickerSide in sides)
            {
                var newOrd = new MarketOrder
                    {
                        Symbol = tickerSide.Key,
                        Side = tickerSide.Value,
                        AccountID = robotContext.AccountInfo.ID,
                        Magic = Magic,
                        Volume = FixedVolume,
                        ExpertComment = "SimpleTradeMachine"
                    };
                var rst = robotContext.SendNewOrderRequest(protectedContext.MakeProtectedContext(),
                                                 RequestUniqueId.Next(), newOrd, OrderType.Market, 0, 0);
            }

            return events;
        }
    }
    // ReSharper restore LocalizableElement
}
