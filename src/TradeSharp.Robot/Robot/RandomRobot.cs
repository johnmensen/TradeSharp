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
    [DisplayName("Вероятностная оценка")]
    public class RandomRobot : BaseRobot
    {
        #region Настройки
        private string workingHours = "10-18 20-23";
        [PropertyXMLTag("WorkingHours")]
        [DisplayName("Рабочие часы")]
        [Category("Торговые")]
        [Description("Часы, в которые, с наибольшей вероятностью, будет совершена сделка")]
        public string WorkingHours
        {
            get { return workingHours; }
            set { workingHours = value; }
        }

        private int probTrade = 10;
        [PropertyXMLTag("ProbTrade")]
        [DisplayName("P входа")]
        [Category("Торговые")]
        [Description("Вероятность входа после закрытия свечи")]
        public int ProbTrade
        {
            get { return probTrade; }
            set { probTrade = value; }
        }

        private int probClose = 11;
        [PropertyXMLTag("ProbClose")]
        [DisplayName("P выхода")]
        [Category("Торговые")]
        [Description("Вероятность закрытия сделки")]
        public int ProbClose
        {
            get { return probClose; }
            set { probClose = value; }
        }

        private int probNonWorkTrade = 15;
        [PropertyXMLTag("ProbTrade")]
        [DisplayName("P внерабоч.")]
        [Category("Торговые")]
        [Description("Вероятность входа в нерабочие часы")]
        public int ProbNonWorkTrade
        {
            get { return probNonWorkTrade; }
            set { probNonWorkTrade = value; }
        }

        private int probOpposOrder = 15;
        [PropertyXMLTag("ProbOpposOrder")]
        [DisplayName("P встречных ордеров.")]
        [Category("Торговые")]
        [Description("Вероятность входа против старого ордера")]
        public int ProbOpposOrder
        {
            get { return probOpposOrder; }
            set { probOpposOrder = value; }
        }

        private int probSecondOrder = 40;
        [PropertyXMLTag("ProbSecondOrder")]
        [DisplayName("P повтор.")]
        [Category("Торговые")]
        [Description("Вероятность повторного входа")]
        public int ProbSecondOrder
        {
            get { return probSecondOrder; }
            set { probSecondOrder = value; }
        }

        private int probStrictOnCandleCloseOrder = 36;
        [PropertyXMLTag("ProbStrictOnCandleCloseOrder")]
        [DisplayName("P закрытия.")]
        [Category("Торговые")]
        [Description("Вероятность входа точно на закрытии свечи")]
        public int ProbStrictOnCandleCloseOrder
        {
            get { return probStrictOnCandleCloseOrder; }
            set { probStrictOnCandleCloseOrder = value; }
        }

        private int probStopLoss = 55;
        [PropertyXMLTag("ProbStopLoss")]
        [DisplayName("P SL")]
        [Category("Стоп-тейк")]
        [Description("Вероятность установки SL")]
        public int ProbStopLoss
        {
            get { return probStopLoss; }
            set { probStopLoss = value; }
        }

        private int probTakeProfit = 55;
        [PropertyXMLTag("ProbTakeProfit")]
        [DisplayName("P TP")]
        [Category("Стоп-тейк")]
        [Description("Вероятность установки TP")]
        public int ProbTakeProfit
        {
            get { return probTakeProfit; }
            set { probTakeProfit = value; }
        }

        private int minStopPoints = 20, maxStopPoints = 200, stepStopPoints = 10, propRandomStopStep = 50;
        [PropertyXMLTag("StopOrderParams")]
        [DisplayName("Настройки SL")]
        [Category("Стоп-тейк")]
        [Description("Настройки SL")]
        public string StopOrderParams
        {
            get
            {
                return string.Format("min: {0}, max: {1}, step: {2}, randStepProb: {3}",
                                     minStopPoints, maxStopPoints, stepStopPoints, propRandomStopStep);
            }
            set
            {
                var values = value.ToIntArrayUniform();
                if (values.Length != 4 || values.Any(v => v <= 0)) return;
                minStopPoints = values[0];
                maxStopPoints = values[1];
                if (maxStopPoints <= minStopPoints)
                {
                    maxStopPoints = minStopPoints + 10;
                }
                stepStopPoints = values[2];
                propRandomStopStep = values[3];
            }
        }

        #endregion

        #region Данные

        private readonly List<Cortege2<int, int>> workHours = new List<Cortege2<int, int>>();

        private Dictionary<Cortege2<string, BarSettings>, CandlePacker> packers;

        private Random rnd;

        private List<Cortege3<string, int, DateTime>> plannedOrders;

        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new RandomRobot
                {
                    FixedVolume = FixedVolume,
                    Leverage = Leverage,
                    ProbNonWorkTrade = ProbNonWorkTrade,
                    ProbSecondOrder = ProbSecondOrder,
                    ProbOpposOrder = ProbOpposOrder,
                    ProbStopLoss = ProbStopLoss,
                    ProbStrictOnCandleCloseOrder = ProbStrictOnCandleCloseOrder,
                    ProbTakeProfit = ProbTakeProfit,
                    ProbTrade = ProbTrade,
                    ProbClose = ProbClose,
                    StopOrderParams = StopOrderParams,
                    WorkingHours = WorkingHours,                
                    RoundType = RoundType,
                    NewsChannels = NewsChannels,
                    RoundMinVolume = RoundMinVolume,
                    RoundVolumeStep = RoundVolumeStep
                };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, Contract.Util.BL.CurrentProtectedContext gprotectedContext)
        {
            base.Initialize(grobotContext, gprotectedContext);

            if (Graphics.Count == 0) return;

            plannedOrders = new List<Cortege3<string, int, DateTime>>();
            rnd = new Random();
            packers = Graphics.ToDictionary(g => g, g => new CandlePacker(g.b));

            // перевести рабочие часы в удобную форму
            workHours.Clear();
            var workHoursParts = WorkingHours.ToIntArrayUniform();
            
            for (var i = 0; i < workHoursParts.Length / 2; i++)
            {
                var start = workHoursParts[i*2];
                var end = workHoursParts[i*2 + 1];
                if (end == start || end < start) continue;

                workHours.Add(new Cortege2<int, int>(start, end));
            }

            if (workHours.Count < 1)
            {
                workHours.Clear();
                workHours.AddRange(new List<Cortege2<int, int>> { new Cortege2<int, int>(10, 18), new Cortege2<int, int>(20, 22) });
            }

        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            if (packers.Count == 0) return new List<string>();
            if (isHistoryStartOff) return new List<string>();
            var time = quotes[0].timeClose;


            // подготовить события входа
            for (var i = 0; i < names.Length; i++)
            {
                var ticker = names[i];
                foreach (var packer in packers)
                {
                    if (packer.Key.a != ticker) continue;
                    var candle = packer.Value.UpdateCandle(quotes[i]);
                    if (candle == null) continue;

                    // закрыть сделку / сделки?
                    if (CheckDice(probClose))
                        TryCloseDeal(ticker);

                    // войти в рынок или закрыть сделку или забить?
                    if (!CheckDice(probTrade)) continue;
                    
                    // в рабочий час...?
                    var isWorkingHour = workHours.Any(hr => hr.a >= time.Hour && hr.b <= time.Hour);
                    if (!isWorkingHour && !CheckDice(probNonWorkTrade)) continue;

                    List<MarketOrder> orders;
                    GetMarketOrders(out orders, true);
                    orders = orders.Where(o => o.Symbol == ticker).ToList();
                    
                    // повторный вход?
                    if (orders.Count > 0)
                        if (!CheckDice(probSecondOrder)) continue;

                    var side = CheckDice(50) ? 1 : -1;
                    // противоположный вход?
                    if (orders.Count > 0 && orders[orders.Count - 1].Side != side)                        
                        if (!CheckDice(probOpposOrder)) continue;
                    
                    // время входа
                    var timeEnter = time;
                    if (!CheckDice(probStrictOnCandleCloseOrder))
                    {
                        var deltaMinutes = rnd.Next((int) (packer.Key.b.Intervals.Sum()/1.5));
                        timeEnter = timeEnter.AddMinutes(deltaMinutes);
                    }

                    plannedOrders.Add(new Cortege3<string, int, DateTime>(ticker, side, timeEnter));
                }
            }

            // если есть события для входа - войти в рынок
            for (var i = 0; i < plannedOrders.Count; i++)
            {
                if (time > plannedOrders[i].c) continue;
                OpenOrder(plannedOrders[i]);
                plannedOrders.RemoveAt(i);
                i--;
            }
            
            return null;
        }

        private void OpenOrder(Cortege3<string, int, DateTime> order)
        {
            var ticker = order.a;
            var side = order.b;

            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

            float? sl = null;
            if (CheckDice(probStopLoss))
            {
                var quote = QuoteStorage.Instance.ReceiveValue(ticker);
                if (quote != null)
                {
                    var points = minStopPoints;
                    if (CheckDice(propRandomStopStep))
                        points = points + rnd.Next(maxStopPoints - minStopPoints + 1);
                    else
                        points = points + rnd.Next((maxStopPoints - minStopPoints)/stepStopPoints + 1)*stepStopPoints;
                    sl = side > 0 ? quote.ask : quote.bid;
                    sl -= side * DalSpot.Instance.GetAbsValue(ticker, (float)points);
                }
            }

            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                    {
                        Symbol = ticker,
                        Side = side,
                        AccountID = robotContext.AccountInfo.ID,
                        Magic = Magic,
                        Volume = dealVolumeDepo,
                        ExpertComment = "RandomRobot",
                        Comment = "",
                        StopLoss = sl,
                        TakeProfit = null
                    },
                OrderType.Market, 0, 0);
        }

        private void TryCloseDeal(string ticker)
        {
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            foreach (var order in orders.Where(o => o.Symbol == ticker && CheckDice(50)))
            {
                CloseMarketOrder(order.ID);
            }
        }

        private bool CheckDice(int percent)
        {
            return rnd.Next(101) <= percent;
        }
    }
}
