using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Тренд Ковжарова")]
    public class SimpleTrendRobot : BaseRobot
    {
        #region Настройки
        private int patternLength = 2;
        [PropertyXMLTag("Robot.PatternLength")]
        [DisplayName("Свечей в шаблоне")]
        [Category("Торговые")]
        [Description("Последовательно идущих свечей")]
        public int PatternLength
        {
            get { return patternLength; }
            set { patternLength = value; }
        }

        private int stopLossPoints = 200;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints = 200;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }
        #endregion

        #region Переменные
        private Dictionary<string, CandlePacker> packers;
        private Dictionary<string, decimal> pointCost;
        private Dictionary<string, List<CandleData>> lastCandles;
        private List<string> events;
        private const float MinDeltaStop = 2;
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new SimpleTrendRobot
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                PatternLength = PatternLength,
                FixedVolume = FixedVolume,
                Leverage = Leverage,
                RoundType = RoundType,
                NewsChannels = NewsChannels,
                RoundMinVolume = RoundMinVolume,
                RoundVolumeStep = RoundVolumeStep
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(BacktestServerProxy.RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
 	        base.Initialize(robotContext, protectedContext);
            if (Graphics.Count == 0) return;
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            pointCost = Graphics.ToDictionary(g => g.a, g => DalSpot.Instance.GetAbsValue(g.a, 1M));
            lastCandles = Graphics.ToDictionary(g => g.a, g => new List<CandleData>());
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return Graphics.ToDictionary(g => g.a, g => startTrade.AddMinutes(g.b.TotalMinutes*(patternLength + 1)));
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = new List<string>();
            if (packers == null) return events;

            for (var i = 0; i < quotes.Length; i++)
            {
                CandlePacker packer;
                if (!packers.TryGetValue(names[i], out packer))
                    continue;
                var candle = packer.UpdateCandle(quotes[i]);
                if (candle == null) continue;
                var candlesList = lastCandles[names[i]];
                candlesList.Add(candle);
                if (candlesList.Count > patternLength)
                    candlesList.RemoveAt(0);
                if (candlesList.Count < patternLength) continue;
                if (isHistoryStartOff) continue;

                List<PendingOrder> orders;
                GetPendingOrders(out orders);
                orders = orders.Where(o => o.Symbol == names[i]).ToList();
                
                PlaceNewOrder(candlesList, names[i], orders);                
                CheckOrders(candlesList, names[i]);
            }

            return events;
        }

        private void CheckOrders(List<CandleData> candles, string symbol)
        {
            // подтянуть стоп
            List<MarketOrder> orders;
            GetMarketOrders(out orders);
            orders = orders.Where(o => o.Symbol == symbol).ToList();
            if (orders.Count == 0) return;
            var high = candles.Max(c => c.high);
            var low = candles.Min(c => c.low);
            foreach (var order in orders)
            {
                var targetSl = order.Side > 0 ? low : high;
                var delta = order.Side > 0
                    ? targetSl - (order.StopLoss ?? 0)
                    : (order.StopLoss ?? float.MaxValue) - targetSl;
                if (delta <= 0) continue;
                var pointsDelta = delta/(float)pointCost[symbol];
                if (pointsDelta < MinDeltaStop) continue;
                // перетащить стоп
                order.StopLoss = targetSl;
                robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(),
                    order);
            }
        }

        private void PlaceNewOrder(List<CandleData> candles, string symbol, List<PendingOrder> orders)
        {
            OpenDeal(candles.Max(c => c.high), 1, symbol, orders);
            OpenDeal(candles.Min(c => c.low), -1, symbol, orders);
        }

        private void OpenDeal(float price, int dealSide, string symbol, List<PendingOrder> orders)
        {
            // такой ордер уже размещен?
            var minDeltaPrice = (float)pointCost[symbol] * 2;
            if (orders.Any(o => o.Side == dealSide && Math.Abs(o.PriceFrom - price) < minDeltaPrice))
                return;

            var dealVolumeDepo = CalculateVolume(symbol);
            if (dealVolumeDepo == 0) return;

            float? stop = StopLossPoints == 0
                                ? (float?)null
                                : price -
                                  dealSide * DalSpot.Instance.GetAbsValue(symbol, (float)StopLossPoints);
            float? take = TakeProfitPoints == 0
                                ? (float?)null
                                : price +
                                  dealSide * DalSpot.Instance.GetAbsValue(symbol, (float)TakeProfitPoints);

            robotContext.SendNewPendingOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new PendingOrder
                {
                    AccountID = robotContext.AccountInfo.ID,
                    Magic = Magic,
                    Symbol = symbol,
                    Volume = dealVolumeDepo,
                    Side = dealSide,
                    StopLoss = stop,
                    TakeProfit = take,
                    ExpertComment = "Kovzharoff",
                    PriceFrom = price,
                    PriceSide = PendingOrderType.Stop
                });
        }
    }
}
