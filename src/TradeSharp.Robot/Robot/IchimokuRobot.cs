using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;


namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("Ишимоку")]
    public partial class IchimokuRobot : BaseRobot
    {
        #region настройки
        
        private int stopLossPoints = 250;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints = 250;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        /// <summary>
        /// Основной тайм фрейм робота (поле добавлено для удобства)
        /// </summary>
        protected BarSettings MainTimeFrame
        {
            get { return Graphics.Count == 0 ? null : Graphics[0].b;}
            set { Graphics[0] = new Cortege2<string, BarSettings>(Graphics[0].a,  value); }
        }

        private BarSettings dayTimeFrame = new BarSettings { Intervals = new List<int> { 1440 }, Title = "D1" };
        [PropertyXMLTag("Robot.FirstSubTimeFrame")]
        [DisplayName("Первый вспомогательный тайм фрейм")]
        [Category("Основные")]
        [Description("Первый вспомогательный тайм фрейм торгового робота")]
        public string DayTimeFrame
        {
            get { return BarSettingsStorage.Instance.GetBarSettingsFriendlyName(dayTimeFrame); }
            set { dayTimeFrame = BarSettingsStorage.Instance.GetBarSettingsByName(value) ?? new BarSettings { Intervals = { 1440 }, Title = "D1" }; }
        }

        private BarSettings hourTimeFrame = new BarSettings { Intervals = new List<int> { 240 }, Title = "H4" };
        [PropertyXMLTag("Robot.SecondSubTimeFrame")]
        [DisplayName("Второй вспомогательный тайм фрейм")]
        [Category("Основные")]
        [Description("Второй вспомогательный тайм фрейм торгового робота")]
        public string HourTimeFrame
        {
            get { return BarSettingsStorage.Instance.GetBarSettingsFriendlyName(hourTimeFrame); }
            set { hourTimeFrame = BarSettingsStorage.Instance.GetBarSettingsByName(value) ?? new BarSettings { Intervals = { 240 }, Title = "H4" }; }
        }

        private int periodS = 9;
        [DisplayName("Короткий временной промежуток")]
        [Description("Количесво свечей короткого временного промежутка")]
        [Category("Основные")]
        public int PeriodS
        {
            get { return periodS; }
            set { if (value < periodM && value < periodL) periodS = value; }
        }

        private int periodM = 26;
        [DisplayName("Средний временной промежуток")]
        [Description("Количесво свечей среднего временного промежутка")]
        [Category("Основные")]
        public int PeriodM
        {
            get { return periodM; }
            set { if (value > periodS && value < periodL) periodM = value; }
        }

        private int periodL = 52;
        [DisplayName("Длинный временной промежуток")]
        [Description("Количесво свечей длинного временного промежутка")]
        [Category("Основные")]
        public int PeriodL
        {
            get { return periodL; }
            set { if (value > periodM && value > periodS) periodL = value; }
        }

        private int periodMSub1 = 26;
        [DisplayName("")]
        [Description("")]
        [Category("Основные")]
        public int PeriodMSub1
        {
            get { return periodMSub1; }
            set { periodMSub1 = value; }
        }

        private int periodMSub2 = 26;
        [DisplayName("")]
        [Description("")]
        [Category("Основные")]
        public int PeriodMSub2
        {
            get { return periodMSub2; }
            set { periodMSub2 = value; }
        }

        private int m = 20;
        [DisplayName("Период соприкосновения с облаком")]
        [Description("Количесво свечей, хлтябы одна из которых должна войти в «облако», что бы совершить сделку")]
        [Category("Основные")]
        public int M
        {
            get { return m; }
            set { if (m > 0) m = value; }
        }

        private float k = 0.001f;
        [DisplayName("Превышеине close над Киджуна")]
        [Description("Цена close текущей свечи должна быть выше Киджуна на K ценовых пунктов")]
        [Category("Основные")]
        public float K
        {
            get { return k; }
            set { if (k > 0) k = value; }
        }
        // ReSharper restore LocalizableElement
        #endregion
        
        #region основные методы
        public override BaseRobot MakeCopy()
        {
            return new IchimokuRobot
                {
                    Leverage = Leverage,
                    FixedVolume = FixedVolume,
                    HumanRTickers = HumanRTickers,
                    TypeName = TypeName,
                    RoundVolumeStep = RoundVolumeStep,
                    RoundType = RoundType,
                    RoundMinVolume = RoundMinVolume,

                    StopLossPoints = StopLossPoints,
                    TakeProfitPoints = TakeProfitPoints,
                    MainTimeFrame = MainTimeFrame,
                    HourTimeFrame = HourTimeFrame,
                    DayTimeFrame = DayTimeFrame
                };
        }

        public override void Initialize(RobotContext grobotContext, CurrentProtectedContext protectedContextx)
        {
            base.Initialize(grobotContext, protectedContextx);
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("IchimokuRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("IchimokuRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }

            mainCandlePacker = new CandlePacker(MainTimeFrame);
            dayCandlePacker = new CandlePacker(dayTimeFrame);
            hourCandlePacker = new CandlePacker(hourTimeFrame);

            // размер этой очереди именно periodL + periodM. Это связано с тем, что рассчитать "облако" надо для "сдвинутой" назад точки, а не для текущей.
            // А что бы это сделать, нужно отступить от "сдвинутой назад точки" ещё на periodL.
            shiftBackPoints = new RestrictedQueue<CandleData>(periodL + periodM);

            subQueue = new[] { new RestrictedQueue<CandleData>(periodMSub1), new RestrictedQueue<CandleData>(periodMSub2) };

            happenIntersection = new RestrictedQueue<bool>(m);
            ticker = Graphics[0].a;
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0)
            {
                Logger.ErrorFormat("Робот {0} не может продолжать работу: не найден ни один объект графика для этого робота", TypeName);
                return null;
            }
            try
            {
                var historyIndexStart = Graphics[0].b.GetDistanceTime(periodL, -1, startTrade);
                return new Dictionary<string, DateTime> { { Graphics[0].a, historyIndexStart } };
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Робот {0} не может продолжать работу: {1}", TypeName, ex.Message), ex);
                return null;
            }
        }



        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            OpenedDealOnCurrentCandle = false;
            var events = new List<string>();
            if (string.IsNullOrEmpty(ticker)) return events;
            var tickerIndex = -1;
            for (var i = 0; i < names.Length; i++)
                if (names[i] == ticker)
                {
                    tickerIndex = i;
                    break;
                }
            if (tickerIndex < 0) return events;
            var quote = quotes[tickerIndex];
            candleMainTimeFrame = mainCandlePacker.UpdateCandle(quote);
            planning = UpdateHigherTimeframesAndGetPlanDirection(quote, candleMainTimeFrame != null); 

            if (candleMainTimeFrame == null) return events;
            shiftBackPoints.Add(candleMainTimeFrame);

            happenIntersection.Add(CloudIntersection(candleMainTimeFrame));
                           
            // Не торгуем на исторической котировке или если не были полностью заполнены очереди
            if (shiftBackPoints.Length < shiftBackPoints.MaxQueueLength || isHistoryStartOff) return events;
            // Не торгуем, если это не было запланировано
            if (planning == 0) return events;
            // Не торгуем, если не было пересечение с облаком
            if (happenIntersection.Length < happenIntersection.MaxQueueLength || happenIntersection.All(x => false)) return events;
                      
            kijunMain = (shiftBackPoints.Skip(PeriodL).Min(x => x.low) + shiftBackPoints.Skip(PeriodL).Max(x => x.high)) / 2;
            // цена close текущей свечи должна быть выше (при покупке) Киджуна на K ценовых пунктов
            if ((planning > 0 && candleMainTimeFrame.close - kijunMain <= k) || (planning < 0 && kijunMain - candleMainTimeFrame.close <= k)) return events;

            OpenedDealOnCurrentCandle = OpenDeal(candleMainTimeFrame.close, planning);
            return events;
        }

        private bool OpenDeal(float price, int dealSide)
        {
            List<MarketOrder> orders;
            robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
            orders = orders.Where(o => o.Magic == Magic && o.Symbol == ticker).ToList();

            // закрыть сделки противонаправленные текущему знаку
            foreach (var order in orders.Where(o => o.Side != dealSide).ToList())
                robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                              robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
            // если была, например, покупка - повторно не покупаем
            if (orders.Any(o => o.Side == dealSide)) return false;        

            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return false;

            // Пересчёт значений Stop loss и Take profit из пунктов в конкретную цену. Эти значения не могут быть 0
            var stop = StopLossPoints == 0
                                ? (float?)null
                                : price -
                                    dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)StopLossPoints);
            var take = TakeProfitPoints == 0
                                ? (float?)null
                                : price +
                                    dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)TakeProfitPoints);
            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                {
                    AccountID = robotContext.AccountInfo.ID,    // Уникальный идентификатор чёта
                    Magic = Magic,                              // Этот параметр позволяет отличать сделки разных роботов
                    Symbol = ticker,                            // Инструмент по которому совершается сделка
                    Volume = dealVolumeDepo,                    // Объём средств, на который совершается сделка
                    Side = dealSide,                            // Устанавливаем тип сделки - покупка или продажа
                    StopLoss = stop,                            // Устанавливаем величину Stop loss для открываемой сделки
                    TakeProfit = take,                          // Устанавливаем величину Take profit для открываемой сделки
                    ExpertComment = "IchimokuRobot"             // Комментарий по сделке, оставленный роботом
                },
                OrderType.Market, 0, 0);
            return true;
        }
        #endregion

        #region вспомогательные методы (вычисления)
        /// <summary>
        /// планирование покупки или продажи.
        /// </summary>
        private int UpdateHigherTimeframesAndGetPlanDirection(CandleDataBidAsk quote, bool calcKijun)
        {
            var packers = new[] { dayCandlePacker, hourCandlePacker };
            bool[] higherThanKijuns = { false, false };

            for (var i = 0; i < subQueue.Length; i++)
            {    
                var candle =  packers[i].UpdateCandle(quote);
                if (candle != null) subQueue[i].Add(candle);

                if (!calcKijun) continue;
                var candles = candle != null
                                  ? subQueue[i].ToList()
                                  : subQueue[i].Union(new[] {packers[i].CurrentCandle}).ToList();
                
                var kijun = (candles.Min(c => c.low) + candles.Max(c => c.high))/2;
                higherThanKijuns[i] = quote.close > kijun;
            }

            if (!calcKijun || subQueue.Any(q => q.Length < q.MaxQueueLength)) return 0;
            return higherThanKijuns.Any(x => x != higherThanKijuns[0]) ? 0 : higherThanKijuns[0] ? 1 : -1;
        }

        /// <summary>
        /// Пересекает ли переданная свеча облако на заданном диапазоне свечей
        /// </summary>
        /// <param name="candle">Свеча, посчитанная по главному таймфрейму (M30)</param>
        private bool CloudIntersection(CandleData candle)
        {
            // от переданной свечи отступаем назад на средний промежуток времени (periodM)
            // считаем там 2 точки облака (SenkouA и SenkouB)
            // проверяем - пересекает ли их свеча

            var firstCandleDataPeriodL = shiftBackPoints.Take(PeriodL).ToList();
            var kijunPoints = firstCandleDataPeriodL.Skip(firstCandleDataPeriodL.Count - PeriodM).ToList(); //TODO тут может быть исключение, есть PeriodL < PeriodM
            var kijunShift = (kijunPoints.Min(x => x.close) + kijunPoints.Max(x => x.close)) / 2;


            var tencanPoints = firstCandleDataPeriodL.Skip(firstCandleDataPeriodL.Count - PeriodS).ToList();
            var tencanShift = (tencanPoints.Min(x => x.close) + tencanPoints.Max(x => x.close)) / 2;


            var senkouA = (tencanShift + kijunShift) / 2;
            var senkouB = (firstCandleDataPeriodL.Min(x => x.close) + firstCandleDataPeriodL.Max(x => x.close)) / 2;

            if (candle.high > senkouA && senkouA > candle.low) return true;
            if (candle.high > senkouB && senkouB > candle.low) return true;

            var up = senkouA > senkouB ? senkouA : senkouB;
            var dn = senkouA > senkouB ? senkouB : senkouA;

            return up > candle.close && candle.close > dn;
        }
        #endregion
    }
}
