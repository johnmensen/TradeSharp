using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
// ReSharper disable LocalizableElement
    [DisplayName("Торговля от проекций")]
    [TypeConverter(typeof(PropertySorter))]
    public class FibonacciRobot : BaseRobot
    {
        private CandlePacker packer;
        private string ticker;
        private List<CandleData> candles;
        /// <summary>
        /// цена одного пункта (0.0001 для EURUSD)
        /// </summary>
        private decimal pointPrice;

        //private decimal fiboDistanceAbs;

        /// <summary>
        /// если робот сообщает о пройденных вершинах и барах Фибо -
        /// сохранять отмеченные вершины, дабы не отмечать их дважды
        /// </summary>
        private List<int> markedZigZagPivots;

        private float zigZagPeriodPercent = 1;
        [DisplayName("Процент Зигзага")]
        [Description("Пороговый процент изменения курса для формирования вершины зигзага")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.ZigZagPeriodPercent")]
        [PropertyOrder(1)]
        public float ZigZagPeriodPercent
        {
            get { return zigZagPeriodPercent; }
            set { zigZagPeriodPercent = value; }
        }

        [DisplayName("Цена для ЗЗ")]
        [Description("Цены Зиг-Зага")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.ZigZagSourceType")]
        [PropertyOrder(2)]
        public ZigZagSource ZigZagSourceType { get; set; }

        private float[] fiboLevels = new [] { 0.618f };
        [DisplayName("Уровни расширений")]
        [Description("Значения уровней расширений, которые будут откладыватся по ZigZag")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.FiboLevels")]
        [PropertyOrder(3)]
        public string FiboLevels
        {
            get { return fiboLevels.ToStringUniform(";"); }
            set { fiboLevels = value.ToFloatArrayUniform(); }
        }

        private int[] fiboBars = new [] { 5, 8, 13, 21 };
        [DisplayName("Бары разворота")]
        [Description("Бары разворота, отсчет от вершины ЗЗ (индекс вершины - 1)")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.FiboBars")]
        [PropertyOrder(4)]
        public string FiboBars
        {
            get { return fiboBars.ToStringUniform(";"); }
            set { fiboBars = value.ToIntArrayUniform(); }
        }

        private int fiboBarsLifetime = 30;
        [DisplayName("Длина расширений")]
        [Description("Время жизни расширений, в свечах")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.FiboLifetime")]
        [PropertyOrder(5)]
        public int FiboBarsLifetime
        {
            get { return fiboBarsLifetime; }
            set { fiboBarsLifetime = value; }
        }

        private decimal fiboReachDistancePoint = 30;
        [DisplayName("Расст. до Ф, пп")]
        [Description("Расстояние до Ф-уровня, пунктов")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.FiboReachDistancePoint")]
        [PropertyOrder(6)]
        public decimal FiboReachDistancePoint
        {
            get { return fiboReachDistancePoint; }
            set { fiboReachDistancePoint = value; }
        }

        private int candlesInIndexHistory = 100;
        [DisplayName("Длина истории индексов")]
        [Description("Длина истории индексов, свечей")]
        [Category("Основные")]        
        [PropertyXMLTag("Robot.CandlesInIndexHistory")]
        [PropertyOrder(8)]
        public int CandlesInIndexHistory
        {
            get { return candlesInIndexHistory; }
            set { candlesInIndexHistory = value; }
        }

        [DisplayName("Показ раз. баров")]
        [Description("Показать на графике бары разворота")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.ShowFiboTurnBars")]
        [PropertyOrder(7)]
        public bool ShowFiboTurnBars { get; set; }

        private int stopLossPoints = 500;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        [PropertyOrder(9)]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints = 500;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        [PropertyOrder(10)]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        private int maxDealsInSeries = 5;
        [PropertyXMLTag("Robot.MaxDealsInSeries")]
        [DisplayName("Макс сделок подряд")]
        [Category("Торговые")]
        [Description("Макс. количество последовательно идущих входов")]
        [PropertyOrder(11)]
        public int MaxDealsInSeries { get { return maxDealsInSeries; } set { maxDealsInSeries = value; } }

        public override BaseRobot MakeCopy()
        {
            var bot = new FibonacciRobot
                          {
                              zigZagPeriodPercent = zigZagPeriodPercent,
                              FiboLevels = FiboLevels,
                              FiboBars = FiboBars,
                              FiboBarsLifetime = FiboBarsLifetime,
                              CandlesInIndexHistory = CandlesInIndexHistory,
                              FiboReachDistancePoint = FiboReachDistancePoint,
                              ShowFiboTurnBars = ShowFiboTurnBars,
                              StopLossPoints = StopLossPoints,
                              TakeProfitPoints = TakeProfitPoints,
                              ZigZagSourceType = ZigZagSourceType,
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
            if (Graphics.Count != 1) return null;
            var historyIndexStart = Graphics[0].b.GetDistanceTime(CandlesInIndexHistory + 1, -1, startTrade);
            return new Dictionary<string, DateTime> { { Graphics[0].a, historyIndexStart } };
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("FibonacciRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("FibonacciRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;
            packer = new CandlePacker(Graphics[0].b);
            candles = new List<CandleData>();
            pointPrice = 1M/DalSpot.Instance.GetPrecision10(ticker);
            //fiboDistanceAbs = FiboReachDistancePoint*pointPrice;
            markedZigZagPivots = new List<int>();
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            CandleDataBidAsk quote = null;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == ticker)
                {
                    quote = quotes[i];
                    break;
                }
            }
            var events = new List<string>();
            if (quote == null) return events;
            // сопроводить сделки, коли треба...

            var candle = packer.UpdateCandle(quote);
            if (candle == null) return events;
            candles.Add(candle);

            // получить уровни ЗигЗага
            var pivots = ZigZag.GetPivots(candles, ZigZagPeriodPercent, ZigZagSourceType);
            if (pivots.Count < 3) return events;
            // проверить последнюю вершину ЗЗ - отстоит ли она от предпоследней на ZigZagPeriodPercent
            //var lastValid = Math.Abs(100*(pivots[pivots.Count - 1].b - pivots[pivots.Count - 2].b)/
            //                         pivots[pivots.Count - 2].b)
            //                >= ZigZagPeriodPercent;
            //if (!lastValid && pivots.Count == 3) return events;
            //var screenPointB = lastValid ? pivots[pivots.Count - 1] : pivots[pivots.Count - 2];
            //var screenPointA = lastValid ? pivots[pivots.Count - 2] : pivots[pivots.Count - 3];

            var ptB = pivots[pivots.Count - 2];
            var ptA = pivots[pivots.Count - 3];

            
            // отметить вершину ЗЗ            
            if (ShowFiboTurnBars && (markedZigZagPivots.Count == 0 ||
                markedZigZagPivots[markedZigZagPivots.Count - 1] < ptA.a))
            {
                markedZigZagPivots.Add(ptA.a);
                events.Add(new RobotHint(ticker, Graphics[0].b.ToString(), "З.З", "З.З", "z", ptA.b)
                {
                    ColorFill = Color.Olive,
                    Time = candles[ptA.a].timeOpen,
                    RobotHintType = RobotHint.HintType.Коментарий
                }.ToString());
            }

            // соблюдаются ли условия входа в рынок?
            // - уровень Фибо пройден N-м баром Фибо
            // - с клоза того бара рынок вырос (для продаж)
            var fiboLevel = ptA.b + (ptA.b - ptB.b)*fiboLevels[0];
            //var potentialDealSide = screenPointA.b > screenPointB.b ? -1 : 1;
            var fiboBreachIndex = -1;
            float fiboBreachPrice = 0;
            foreach (var fiboMember in fiboBars)
            {
                var index = fiboMember + ptB.a - 1;
                if (index >= candles.Count) break;
                // свеча закрылась дальше расширения Фибо?
                var close = candles[index].close;
                var fiboReached = ptA.b > ptB.b ? close > fiboLevel : close < fiboLevel;
                if (!fiboReached) continue;
                fiboBreachIndex = index;
                fiboBreachPrice = close;
                break;
            }

            // считаем, сколько потенциально могло быть совершено входов
            // с момента пробития Фибо
            if (fiboBreachIndex > 0)
            {
                var countDeals = 1;
                // потенциальные сделки
                for (var i = fiboBreachIndex + 1; i < candles.Count - 1; i++)
                {
                    if ((candles[i].close > fiboBreachPrice && ptA.b > ptB.b) ||
                        (candles[i].close < fiboBreachPrice && ptA.b < ptB.b))
                    {
                        fiboBreachPrice = candles[i].close;
                        countDeals++;
                    }
                }
                var isEnterBetter = (candle.close > fiboBreachPrice && ptA.b > ptB.b) ||
                                    (candle.close < fiboBreachPrice && ptA.b < ptB.b);

                if ((isEnterBetter || fiboBreachIndex == candles.Count - 1) && 
                    countDeals <= maxDealsInSeries)
                {// войти в рынок, закрыв противонаправленные сделки
                    var dealSide = ptA.b > ptB.b ? -1 : 1;

                    List<MarketOrder> orders;
                    robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                    if (orders.Count > 0) orders = orders.Where(o => o.Magic == Magic && o.Side != dealSide).ToList();
                    foreach (var order in orders)
                        robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                            robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);

                    // открыть сделку
                    OpenDeal(quote.GetCloseQuote(), dealSide);
                    events.Add(new RobotHint(ticker, Graphics[0].b.ToString(),
                        "Вход", "Вход по З.З", "e", dealSide < 0 ? quote.close : quote.closeAsk)
                    {
                        ColorFill = dealSide > 0 ? Color.Green : Color.Red,
                        Time = quote.timeClose,
                        RobotHintType = dealSide > 0 ? RobotHint.HintType.Покупка : RobotHint.HintType.Продажа
                    }.ToString());                    
                }
            }
            
            return events;
        }

        private void OpenDeal(QuoteData quote, int dealSide)
        {
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

            decimal? stop = StopLossPoints == 0
                                ? (decimal?)null
                                : (decimal)quote.bid -
                                  dealSide * DalSpot.Instance.GetAbsValue(ticker, (decimal)StopLossPoints);
            decimal? take = TakeProfitPoints == 0
                                ? (decimal?)null
                                : (decimal)quote.bid +
                                  dealSide * DalSpot.Instance.GetAbsValue(ticker, (decimal)TakeProfitPoints);
            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                    {
                        AccountID = robotContext.AccountInfo.ID,
                        Magic = Magic,
                        Symbol = ticker,
                        Volume = dealVolumeDepo,
                        Side = dealSide,
                        StopLoss = (float?)stop,
                        TakeProfit = (float?)take,
                        ExpertComment = "FibonacciRobot"
                    },
                OrderType.Market, 0, 0);
        }        
    }
    // ReSharper restore LocalizableElement
}
