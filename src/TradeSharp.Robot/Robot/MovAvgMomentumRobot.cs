using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Робот А. Деева")]
    public class MovAvgMomentumRobot : BaseRobot
    {
        class MovArgRobotTag
        {
            public int Trend { get; set; }

            public int MartinChainNum { get; set; }

            public bool EnterEnabled { get { return Trend != 0; }}

            public MovArgRobotTag()
            {
                Trend = 1;
                MartinChainNum = 1;
            }

            public MovArgRobotTag(int trend, int martinChainNum)
            {
                Trend = trend;
                MartinChainNum = martinChainNum;
            }

            public string MakeComment()
            {
                return string.Format("{0};{1}", Trend < 0 ? "counter" : "trend", MartinChainNum);
            }

            public static MovArgRobotTag ParseComment(string comment)
            {
                if (string.IsNullOrEmpty(comment)) return new MovArgRobotTag();
                var parts = comment.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) return new MovArgRobotTag();
                var trend = parts[0] == "counter" ? -1 : 1;
                var chainNum = parts[1].ToIntSafe() ?? 1;
                return new MovArgRobotTag(trend, chainNum);
            }
        }

        #region Настройки
        private int movAvgPeriod = 15;
        [PropertyXMLTag("MovAvgPeriod")]
        [DisplayName("Период СС")]
        [Category("Торговые")]
        [Description("Период СС")]
        public int MovAvgPeriod
        {
            get { return movAvgPeriod; }
            set { movAvgPeriod = value; }
        }

        private int momentumPeriod = 15;
        [PropertyXMLTag("MomentumPeriod")]
        [DisplayName("Период Моментума")]
        [Category("Торговые")]
        [Description("Период Моментума")]
        public int MomentumPeriod
        {
            get { return momentumPeriod; }
            set { momentumPeriod = value; }
        }

        private bool closeOpposite = false;
        [PropertyXMLTag("CloseOpposite")]
        [DisplayName("Закрывать сделки")]
        [Category("Торговые")]
        [Description("Закрывать сделки, открытые против нового тренда")]
        public bool CloseOpposite
        {
            get { return closeOpposite; }
            set { closeOpposite = value; }
        }

        private int stopLossPoints = 100;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints = 100;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        private int dealsToChangeTrend = 5;
        [PropertyXMLTag("DealsToChangeTrend")]
        [DisplayName("Свеч для смены трейда")]
        [Category("Торговые")]
        [Description("Свечек для определения среднего значения")]
        public int DealsToChangeTrend
        {
            get { return dealsToChangeTrend; }
            set { dealsToChangeTrend = value; }
        }

        private int martingalePercent = 0;
        [PropertyXMLTag("MartingalePercent")]
        [DisplayName("Процент Мартингейл")]
        [Category("Торговые")]
        [Description("Процент, прибавляемый к объему предыдущей сделки")]
        public int MartingalePercent
        {
            get { return martingalePercent; }
            set { martingalePercent = value; }
        }

        private int martingaleChainLength = 8;
        [PropertyXMLTag("MartingaleChainLength")]
        [DisplayName("Колен Мартингейл")]
        [Category("Торговые")]
        [Description("Когда закончить наращивать объем")]
        public int MartingaleChainLength
        {
            get { return martingaleChainLength; }
            set { martingaleChainLength = value; }
        }

        private bool enterWhileOpened = true;
        [PropertyXMLTag("EnterWhileOpened")]
        [DisplayName("Вход при открытой поз.")]
        [Category("Торговые")]
        [Description("Входить в рынок, когда есть уже открытая позиция")]
        public bool EnterWhileOpened
        {
            get { return enterWhileOpened; }
            set { enterWhileOpened = value; }
        }
        #endregion

        #region Переменные

        const string ExpertComment = "Deev";

        private int candlesStoredCount;

        /// <summary>
        /// последние свечки по каждой валютной паре
        /// </summary>
        private Dictionary<string, List<CandleData>> storedCandles;

        /// <summary>
        /// знаки пересечения со скользящей средней
        /// </summary>
        private Dictionary<string, List<int>> storedMovAvg;

        private Dictionary<string, decimal> pointCost;

        private Dictionary<string, CandlePacker> packers;

        private List<string> events;
        #endregion

        public override void Initialize(BacktestServerProxy.RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(robotContext, protectedContext);

            if (Graphics.Count == 0) return;
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            pointCost = Graphics.ToDictionary(g => g.a, g => DalSpot.Instance.GetAbsValue(g.a, 1M));
            candlesStoredCount = Math.Max(MovAvgPeriod, MomentumPeriod);
            storedCandles = Graphics.ToDictionary(g => g.a, g => new List<CandleData>(candlesStoredCount));
            storedMovAvg = Graphics.ToDictionary(g => g.a, g => new List<int>(3));
        }

        public override BaseRobot MakeCopy()
        {
            var bot = new MovAvgMomentumRobot
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                MovAvgPeriod = MovAvgPeriod,
                CloseOpposite = CloseOpposite,
                DealsToChangeTrend = DealsToChangeTrend,
                MartingalePercent = MartingalePercent,
                MartingaleChainLength = MartingaleChainLength,
                EnterWhileOpened = EnterWhileOpened,
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

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return Graphics.ToDictionary(g => g.a, g => g.b.GetDistanceTime(candlesStoredCount + 5, 
                -1, startTrade));
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
                if (candle == null)
                    continue;

                var enterSign = CheckEnterCondition(names[i], candle);

                if (isHistoryStartOff || enterSign == 0) continue;

                // по последним сделкам решить - вход по тренду или против тренда?
                var trendSide = GetTrendSide(names[i]);
                if (!trendSide.EnterEnabled) continue;
                enterSign *= trendSide.Trend;

                // если есть открытые сделки против текущего направления - закрыть их
                if (CloseOpposite)
                    CloseCounterOrders(enterSign, names[i]);

                // открыть сделку
                OpenOrder(enterSign, names[i], quotes[i], trendSide);
            }

            return events;
        }

        private MovArgRobotTag GetTrendSide(string symbol)
        {
            // получить открытые ордера
            List<MarketOrder> openedOrders;
            GetMarketOrders(out openedOrders);
            openedOrders = openedOrders ?? new List<MarketOrder>();
            openedOrders = openedOrders.Where(o => o.Symbol == symbol && 
                o.ExpertComment == ExpertComment).ToList();

            if (openedOrders.Count > 0 && !enterWhileOpened)
                return new MovArgRobotTag(0, 0);

            // получить еще и закрытые ордера - нужно 5 последних
            List<MarketOrder> closedOrders;
            robotContext.GetOrdersByFilter(robotContext.AccountInfo.ID,
                true, new OrderFilterAndSortOrder
                {
                    filterMagic = Magic,
                    filterTicker = symbol,
                    takeCount = Math.Max(dealsToChangeTrend, MartingaleChainLength),
                    filterExpertComment = ExpertComment,
                    sortByTimeEnter = true,
                    sortAscending = false
                }, out closedOrders);
            openedOrders.AddRange(closedOrders ?? new List<MarketOrder>());
            if (openedOrders.Count == 0) return new MovArgRobotTag();
            openedOrders = openedOrders.OrderByDescending(o => o.TimeEnter).ToList();
            
            var sideLength = 1;
            var lastTag = MovArgRobotTag.ParseComment(openedOrders[0].Comment);
            for (var i = 1; i < openedOrders.Count; i++)
            {
                var tag = MovArgRobotTag.ParseComment(openedOrders[i].Comment);
                if (tag.Trend != lastTag.Trend) break;
                sideLength++;
            }
            var newTrend = sideLength < dealsToChangeTrend ? lastTag.Trend : -lastTag.Trend;
            var martinChainNum = (openedOrders[0].IsClosed && openedOrders[0].ResultDepo > 0) ? 1
                : lastTag.MartinChainNum + 1;
            return new MovArgRobotTag(newTrend, martinChainNum);
        }

        private void OpenOrder(int dealSign, string symbol, CandleDataBidAsk lastCandle, MovArgRobotTag orderTag)
        {
            var volumeMultiplier = (100 + (MartingalePercent * (orderTag.MartinChainNum  - 1))) / 100M;
            var volume = CalculateVolume(symbol, 
                base.Leverage == null ? null : base.Leverage * volumeMultiplier,
                FixedVolume == null ? null : (int?) (FixedVolume.Value * volumeMultiplier));
            if (volume == 0)
            {
                events.Add(string.Format("{0} {1} отменена - объем равен 0", dealSign > 0 ? "покупка" : "продажа", symbol));
                return;
            }

            var enterPrice = dealSign > 0 ? lastCandle.closeAsk : lastCandle.close;
            var point = pointCost[symbol];
            var stopLoss = enterPrice - dealSign * (float)point * StopLossPoints;
            var takeProfit = enterPrice + dealSign * (float)point * TakeProfitPoints;
            
            var order = new MarketOrder
            {
                AccountID = robotContext.AccountInfo.ID,
                Magic = Magic,
                Symbol = symbol,
                Volume = volume,
                Side = dealSign,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                ExpertComment = ExpertComment,
                Comment = orderTag.MakeComment()
            };
            var status = NewOrder(order,
                OrderType.Market,
                0, 0);
            if (status != RequestStatus.OK)
                events.Add(string.Format("Ошибка добавления ордера {0} {1}: {2}",
                    dealSign > 0 ? "BUY" : "SELL", symbol, status));
        }

        private void CloseCounterOrders(int dealSign, string symbol)
        {
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            if (orders.Count == 0) return;
            var ordersToClose = orders.Where(o => o.Symbol == symbol && o.Side != dealSign);

            foreach (var order in ordersToClose)
                CloseMarketOrder(order.ID, PositionExitReason.ClosedByRobot);
        }

        private int CheckEnterCondition(string symbol, CandleData candle)
        {
            // не входить на свечах - пустышках
            if (Math.Abs(candle.open - candle.close) < 0.000001) return 0;
            var candlesList = storedCandles[symbol];
            candlesList.Add(candle);
            if (candlesList.Count > candlesStoredCount)
                candlesList.RemoveAt(0);
            if (candlesList.Count < MovAvgPeriod || candlesList.Count < MomentumPeriod)
                return 0;
            
            var average = candlesList.Skip(candlesList.Count - MovAvgPeriod).Average(x => x.close);
            var movAvgList = storedMovAvg[symbol];
            movAvgList.Add(Math.Sign(candle.close - average));
            if (movAvgList.Count < 3) return 0;
            if (movAvgList.Count > 3) movAvgList.RemoveAt(0);

            // вторая свеча после пробития СС?
            if (movAvgList[2] != movAvgList[1] || movAvgList[1] == movAvgList[0])
                return 0;
            var signal = movAvgList[2];
            if (signal == 0) return 0;

            var momenumA = candlesList[candlesList.Count - momentumPeriod].close;
            var momenumB = candlesList[candlesList.Count - 1].close;
            var momentum = momenumA == 0 ? 100 : 100 * momenumB / momenumA;
            if ((signal > 0 && momentum < 100) || (signal < 0 && momentum > 100))
                return 0;

            return signal;
        }
    }
}
