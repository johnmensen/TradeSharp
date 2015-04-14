using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Скользящие каналы")]
    [TypeConverter(typeof(PropertySorter))]
    public class SlideChannelRobot : BaseRobot
    {
        private CandlePacker packer;
        private string ticker;
        private List<CandleData> candles;
        
        #region основные настройки
        private int _candlesInHistory = 100;
        [DisplayName("Длина истории")]
        [Description("Длина истории свечей")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.CandlesInHistory")]
        public int CandlesInHistory
        {
            get { return _candlesInHistory; }
            set { _candlesInHistory = value; }
        }

        private int maxBody = 20;
        [DisplayName("Максимум свечей")]
        [Description("Максимальное количество свечей внутри канала")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.MaxBody")]
        public int MaxBody
        {
            get { return maxBody; }
            set { maxBody = value; }
        }

        private int minBody = 3;
        [DisplayName("Минимум свечей")]
        [Description("Минимальное количество свечей внутри канала")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.MinBody")]
        public int MinBody
        {
            get { return minBody; }
            set { minBody = value; }
        }

        private int countBefore = 1;
        [DisplayName("Cвечей до экстремума")]
        [Description("Количество свечей до экстремума внутри канала")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.CountBefore")]
        public int CountBefore
        {
            get { return countBefore; }
            set { countBefore = value; }
        }

        private int countAfter = 2;
        [DisplayName("Cвечей после экстремума")]
        [Description("Количество свечей после экстремума внутри канала")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.CountAfter")]
        public int CountAfter
        {
            get { return countAfter; }
            set { countAfter = value; }
        }

        private ChannelStateInfo channelState = ChannelStateInfo.НетКанала;
        [DisplayName("Состояние индикатора")]
        [Description("Тип текущего построенного канала")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.ChannelState")]
        public ChannelStateInfo ChannelState
        {
            get { return channelState; }
            set { channelState = value; }
        }
        #endregion

        #region настройки объемы
        [PropertyXMLTag("Robot.FixedVolumeStep"),
        DisplayName("Фикс. шаг объема"),
        Category("Объемы"),
        Description("Шаг фиксированного объема")]
        public int FixedVolumeStep { get; set; }

        [PropertyXMLTag("Robot.FixedLeverageStep"),
         DisplayName("Шаг плеча сделки"),
         Category("Объемы"),
         Description("Фикс. шаг для плеча отдельных сделок (если не задан фикс. объем)")]
        public float FixedLeverageStep { get; set; }
        #endregion

        #region торговые настройки
        private int maxLossSeries = 3;
        [PropertyXMLTag("Robot.MaxLossSeries")]
        [DisplayName("Макс. количество убытков подряд")]
        [Category("Торговые")]
        [Description("Максимальное количество убыточных позиций, после чего будет сделана пауза в торговле")]
        public int MaxLossSeries
        {
            get { return maxLossSeries; }
            set { maxLossSeries = value; }
        }

        private int silenceDays = 2;
        [PropertyXMLTag("Robot.SilenceDays")]
        [DisplayName("Режим STOP после серии убытков, дней")]
        [Category("Торговые")]
        [Description("Количество дней, когда робот не будет торговать после серии убытков")]
        public int SilenceDays
        {
            get { return silenceDays; }
            set { silenceDays = value; }
        }

        private int countLosses = 0;
        [PropertyXMLTag("Robot.CountLosses")]
        [DisplayName("Количество совершенных разворотов")]
        [Category("Торговые")]
        [Description("Количество уже совершенных разворотов при текущей торговле")]
        public int CountLosses
        {
            get { return countLosses; }
            set { countLosses = value; }
        }

        private int stopLossPoints = 57;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints = 0;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        private bool useTrailing = true;
        [PropertyXMLTag("Robot.UseTrailing")]
        [DisplayName("Использовать трейлинг позиции")]
        [Category("Торговые")]
        [Description("Использовать трейлинг при сопровождении открытых сделок")]
        public bool UseTrailing
        {
            get { return useTrailing; }
            set { useTrailing = value; }
        }

        private int trailing = 70;
        [PropertyXMLTag("Robot.Trailing")]
        [DisplayName("Размер трейлинга, п")]
        [Category("Торговые")]
        [Description("Дистанция рыночной цены до стопа")]
        public int Trailing
        {
            get { return trailing; }
            set { trailing = value; }
        }

        private int extraTrailing = 90;
        [PropertyXMLTag("Robot.ExtraTrailing")]
        [DisplayName("Размер трейлинга на тренде, п")]
        [Category("Торговые")]
        [Description("Дистанция рыночной цены до стопа при трендовом режиме, когда нет TP у позиции")]
        public int ExtraTrailing
        {
            get { return extraTrailing; }
            set { extraTrailing = value; }
        }

        private int stepTrailing = 10;
        [PropertyXMLTag("Robot.StepTrailing")]
        [DisplayName("Шаг трейлинга, п")]
        [Category("Торговые")]
        [Description("Шаг перемещения стопа при работе трейлинга")]
        public int StepTrailing
        {
            get { return stepTrailing; }
            set { stepTrailing = value; }
        }

        private int finalTrailing = 30;
        [PropertyXMLTag("Robot.FinalTrailing")]
        [DisplayName("Размер трейлинга близко к TP, п")]
        [Category("Торговые")]
        [Description("Дистанция рыночной цены до стопа при приближении зоны тейк-профита")]
        public int FinalTrailing
        {
            get { return finalTrailing; }
            set { finalTrailing = value; }
        }
        
        private int areaTP = 30;
        [PropertyXMLTag("Robot.AreaTP")]
        [DisplayName("Зона TP для трейлинга, п")]
        [Category("Торговые")]
        [Description("Зона тейк-профита, в которой размер трейлинга изменяется на параметр FinalTrailing - защита прибыли при приближении к противоположной границе канала")]
        public int AreaTP
        {
            get { return areaTP; }
            set { areaTP = value; }
        }

        private int supress = 60;
        [PropertyXMLTag("Robot.Supress")]
        [DisplayName("Дистанция поджатия в безубыток, п")]
        [Category("Торговые")]
        [Description("Дистанция переноса SL позиции в безубыточную зону")]
        public int Supress
        {
            get { return supress; }
            set { supress = value; }
        }
        
        private int supressPoints = 5;
        [PropertyXMLTag("Robot.SupressPoints")]
        [DisplayName("Поджатия в безубыток +, п")]
        [Category("Торговые")]
        [Description("Поджатие позиции на количество пунктов в безубыток")]
        public int SupressPoints
        {
            get { return supressPoints; }
            set { supressPoints = value; }
        }

        #endregion

        [Serializable]
        public enum ChannelStateInfo
        {
            НетКанала = 0, ПостроенПоМаксимумам, ПостроенПоМинимумам
        }

        private Cortege3<PointD, PointD, PointD> currChannel = new Cortege3<PointD, PointD, PointD>();

        // x координата точки b активного канала, чтобы следующий четный не рисовался в истории
        private double lastIndexB;
        // x координата точки a активного канала, чтобы следующий четный не рисовался в истории
        private double lastIndexA;
        
        private double currHighChannelPrice = 0;
        private double currLowChannelPrice = 0;
        private bool waitNextQuoteFlag;
        private DateTime noTradeDays;

        public override BaseRobot MakeCopy()
        {
            var bot = new SlideChannelRobot
            {
                MaxBody = MaxBody,
                MinBody = MinBody,
                CountBefore = CountBefore,
                CountAfter = CountAfter,
                ChannelState = ChannelState,
                FixedVolume = FixedVolume,
                FixedVolumeStep = FixedVolumeStep,
                Leverage = Leverage,
                FixedLeverageStep = FixedLeverageStep,
                MaxLossSeries = MaxLossSeries,
                CountLosses = CountLosses,
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                UseTrailing = UseTrailing,
                Trailing = Trailing,
                ExtraTrailing = ExtraTrailing,
                StepTrailing = StepTrailing,
                Supress = Supress,
                SupressPoints = SupressPoints,
                FinalTrailing = FinalTrailing,
                AreaTP = AreaTP,
                SilenceDays = SilenceDays,
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
            var historyIndexStart = Graphics[0].b.GetDistanceTime(CandlesInHistory + 1, -1, startTrade);
            return new Dictionary<string, DateTime> { { Graphics[0].a, historyIndexStart } };
        }

        public override void Initialize(RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("SlideChannelRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("SlideChannelRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;
            packer = new CandlePacker(Graphics[0].b);
            candles = new List<CandleData>();
        }

        /// <summary>
        /// Вызывается на торговых событиях установка снятие ордера, изменение параметров, срабатывание ордера
        /// </summary>
        /// <param name="order"></param>
        public override void OnTradeEvent(PendingOrder order)
        {
            return;
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            CandleDataBidAsk quote = null;
            
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] != ticker) continue;
                quote = quotes[i];
                break;
            }
            if (quote == null) return null;
            var candle = packer.UpdateCandle(quote);
            
            //if (candles.Count == 0) return null;

            if (candle == null)
            {
                // тут надо производить текущие действия с открытыми позициям
                // сопроводить сделки, коли треба... еще как треба
            }
            else
            {
                candles.Add(candle);
                waitNextQuoteFlag = false;
            }

            // если это период "разгона" конвейера
            if (isHistoryStartOff) return null;

            if (candles.Count < CandlesInHistory)
                return null; // не достаточно накоплено свечек
            List<MarketOrder> positions;
            List<PendingOrder> pendingOrders;
            GetMarketOrders(out positions);
            GetPendingOrders(out pendingOrders);
            var events = new List<string>();

            
            if (positions.Count != 0)
            {
                //Logger.InfoFormat("OnQuotesReceived: сопровождаем позицию,  данные quote bid={0} ask={1} time={2}", quote.bid, quote.ask, quote.time);
                ManageOpenPositions(positions, pendingOrders, quote.GetCloseQuote(), ref events);
            }

            
            if (candle == null)
            {
                if (waitNextQuoteFlag)
                {
                    //Logger.InfoFormat("OnQuotesReceived: данные quote bid={0} ask={1} time={2}", quote.bid, quote.ask, quote.time);
                    //Logger.InfoFormat("OnQuotesReceived: ждем следующую свечу... ");
                    return events;
                    // цена за границей канала
                }

                if (currHighChannelPrice != 0 && currLowChannelPrice != 0 && (pendingOrders.Count != 0 || positions.Count != 0) )
                    return events;
            }
            else
            {
                // появилась новая свечка
                if (noTradeDays < candle.timeOpen)
                    noTradeDays = candle.timeOpen;
               
            }
            if (positions.Count == 0)
            {
                if (CountLosses >= MaxLossSeries - 1)
                {
                    CountLosses = 0; // сбрасываем счетчик после убытков
                    // включаем режим "тишина"
                    //noTradeDays = candle.timeOpen.AddDays(SilenceDays);
                    ChannelState = ChannelStateInfo.НетКанала;
                    currChannel = new Cortege3<PointD, PointD, PointD>();
                }
            }
            
            

            if (noTradeDays > quote.timeClose)
            {
                return null; // включен режим тишина, ждем когда можно будет торговать
            }
            
            // --------------------------------------------------------------
            // вот тут нужно делать только действия при появлении новой свечи
            // --------------------------------------------------------------
            // строить только на новой свече
            if (candle != null)
            {
                //var s = string.Format("OnQuotesReceived: сформировалась свеча OHLC {0} {1} {2} {3},   timeopen={4} timeclose={5}",
                //        candle.open, candle.high, candle.low, candle.close, candle.timeOpen, candle.timeClose);
                //Logger.Info(s);
                //events.Add(s);
            }
            //else
                //Logger.InfoFormat("OnQuotesReceived: полный проход при несформированной свечке");
            
            //Logger.InfoFormat("OnQuotesReceived: данные quote bid={0} ask={1} time={2}", quote.bid, quote.ask, quote.time);

            
            if (positions.Count != 0 && positions[0].TakeProfit == null)
            {
                // позиция разворотная последняя, трейлингуем ее и больше ничего не делаем
                ChannelState = ChannelStateInfo.НетКанала;
                currChannel = new Cortege3<PointD, PointD, PointD>();
                currLowChannelPrice = 0;
                currHighChannelPrice = 0;
                RemoveAllOrders(pendingOrders, PendingOrderType.Limit);
                if (candle != null)
                    Logger.InfoFormat("Трейлингуется разворотная позиция id={0}", positions[0].ID);
                return events;
            }

            Logger.InfoFormat("ChannelState = {0}", ChannelState.ToString());
            Logger.InfoFormat("highPrice={0}, lowPrice={1}", currHighChannelPrice, currLowChannelPrice);
            
            
            ChannelStateInfo state;
            var channel = TryToFindChannel(ChannelState, out state);

            var a = channel.a;
            var b = channel.b;
            var k = (b.Y - a.Y) / (b.X - a.X);

            if (lastIndexB == 0 && lastIndexA == 0)
            {
                lastIndexA = a.X;
                lastIndexB = b.X;
            }

            if (lastIndexB > b.X || (lastIndexB == channel.b.X && lastIndexA > channel.a.X))
            {
                // найденный канал уже старый, игнорируем его
                channel = currChannel;
            }
            else
                ChannelState = state;
            currChannel = channel;
            lastIndexA = currChannel.a.X;
            lastIndexB = currChannel.b.X;
            // теперь определяем текущие точки границ канала
            // по двум экстремумам 
            a = currChannel.a;
            b = currChannel.b;
            k = (b.Y - a.Y) / (b.X - a.X);

            var price1 = b.Y + k*(candles.Count - b.X);
            // по одному экстремуму
            var c = channel.c;
            var price2 = c.Y + k*(candles.Count - c.X);

            if (price1 > price2)
            {
                currHighChannelPrice = price1;
                currLowChannelPrice = price2;
            }
            else
            {
                currHighChannelPrice = price2;
                currLowChannelPrice = price1;
            }

            SaveLog(string.Format("текущий канал - верхняя цена {0}, нижняя цена {1}, ChannelState={2}", currHighChannelPrice, currLowChannelPrice, ChannelState));
            if (quote.close < currLowChannelPrice || quote.close > currHighChannelPrice)
            {
                waitNextQuoteFlag = true;
                // редкая ситуация - цена находится вне канала, возможно после разворотов и сработанного стопа
                SaveLog(string.Format("цена {0} находится вне канала - не торгуем", quote.close)); 
                return events;
            }

            if (ChannelState == ChannelStateInfo.НетКанала)
            {
                SaveLog(RemoveAllOrders(pendingOrders, null)
                            ? "нет текущего канала, все отложенные ордеры сняты"
                            : "возникла ошибка при удалении ордеров");
                return events;
            }
            // надо переставить ордера
            // выбираем sell limit
            var ord = pendingOrders.FirstOrDefault(o => o.PriceSide == PendingOrderType.Limit && o.Side == -1);
            var pos = positions.FirstOrDefault(p => p.Side == -1);
            if (ord != null)
            {
                // меняем цену ордера
                var res = EditOrder(ord, (decimal)currHighChannelPrice, (decimal)currLowChannelPrice);
                if (res != RequestStatus.OK)
                {
                    Logger.ErrorFormat("SlideChannelRobot: Не могу изменить ордер №{0}, вход по цене {1}", ord.ID, ord.PriceFrom);
                }
            }
            else
                if (pos == null)
                {
                    // проверка есть ли разворотная позиция на покупку, тогда не будем ставить ордер
                    //pos = positions.FirstOrDefault(p => p.Side == 1);
                    //if (pos == null || pos.TakeProfit.HasValue)
                        InstallOrder(ticker, (decimal) currHighChannelPrice, (decimal) currLowChannelPrice,
                                 PendingOrderType.Limit, -1, RoundMinVolume);
                }

            // ордер может ставить только если нет открытой позиции в этом направлении либо он ставится за стопом открытой позиции
            if (pos != null)
            {
                //if (pos.TakeProfit != null && pos.TakeProfit != 0) // позиция не разворотная
                //{
                    pos.TakeProfit = (float) currLowChannelPrice;
                    var res = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), pos);
                    if (res != RequestStatus.OK)
                    {
                        Logger.ErrorFormat(
                            "SlideChannelRobot: Не могу выставить tp={0} на позицию №{1}, вход по цене {2}",
                            pos.TakeProfit, pos.ID, pos.PriceEnter);
                    }
                //}
                if (ord == null && pos.StopLoss < currHighChannelPrice)
                {
                    res = InstallOrder(ticker, (decimal) currHighChannelPrice, (decimal) currLowChannelPrice,
                                    PendingOrderType.Limit, -1, RoundMinVolume);
                    if (res != RequestStatus.OK)
                    {
                        Logger.ErrorFormat("SlideChannelRobot: Не могу выставить ордер по цене {0} за поджатой позицией №{1}", currHighChannelPrice, pos.ID);
                    }
                }
            }
            
            // выбираем buy limit
            ord = pendingOrders.FirstOrDefault(o => o.PriceSide == PendingOrderType.Limit && o.Side == 1);
            pos = positions.FirstOrDefault(p => p.Side == 1);
            if (ord != null)
            {
                // меняем цену ордера
                var res = EditOrder(ord, (decimal)currLowChannelPrice, (decimal)currHighChannelPrice);
                if (res != RequestStatus.OK)
                {
                    Logger.ErrorFormat("SlideChannelRobot: Не могу изменить ордер №{0}, вход по цене {1}", ord.ID, ord.PriceFrom);
                }
            }
            else
                if (pos == null)
                {
                    // проверка есть ли разворотная позиция на покупку, тогда не будем ставить ордер
                    //pos = positions.FirstOrDefault(p => p.Side == -1);
                    //if (pos == null || pos.TakeProfit.HasValue)
                        InstallOrder(ticker, (decimal) currLowChannelPrice, (decimal) currHighChannelPrice,
                                 PendingOrderType.Limit, 1, RoundMinVolume);
                }

            if (pos != null)
            {
                //if (pos.TakeProfit != null && pos.TakeProfit != 0) // позиция не разворотная
                //{
                    pos.TakeProfit = (float) currHighChannelPrice;
                    var res = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), pos);
                    if (res != RequestStatus.OK)
                    {
                        Logger.ErrorFormat(
                            "SlideChannelRobot: Не могу выставить tp={0} на позицию №{1}, вход по цене {2}",
                            pos.TakeProfit, pos.ID, pos.PriceEnter);
                    }
                //}
                if (ord == null && pos.StopLoss > currLowChannelPrice)
                {
                    res = InstallOrder(ticker, (decimal) currLowChannelPrice, (decimal) currHighChannelPrice,
                                    PendingOrderType.Limit, 1, RoundMinVolume);
                    if (res != RequestStatus.OK)
                    {
                        Logger.ErrorFormat("SlideChannelRobot: Не могу выставить ордер по цене {0} за поджатой позицией №{1}", currLowChannelPrice, pos.ID);
                    }
                }
            }
            return events;
        }

        private void SaveLog(string str)
        {
           if (robotContext.robotContextMode == RobotContext.ContextMode.Realtime)
                TerminalLog.Instance.SaveRobotLog(string.Format("{0}: {1}", TypeName, str));
            Logger.InfoFormat(string.Format("{0}: {1}", TypeName, str));
        }

        #region ManageOpenPositions Сопровождение открытой позиции
        /// <summary>
        /// Сопровождение открытой позиции
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="orders"></param>
        /// <param name="quote"></param>
        /// <param name="events"></param>
        private void ManageOpenPositions(List<MarketOrder> positions, List<PendingOrder> orders, QuoteData quote, ref List<string> events)
        {
            foreach (var pos in positions)
            {
                // var ord = orders.FirstOrDefault(o => o.PriceSide == PendingOrderType.Limit && o.Side == pos.Side);
                //if (ord != null)
                //{
                //    // нехрен ему тут делать
                //    var res = robotContext.SendDeletePendingOrderRequest(ord, PendingOrderStatus.Отменен, null,
                //                                                         string.Empty);
                //    if (res != RequestStatus.OK)
                //    {
                //        Logger.ErrorFormat("SlideChannelRobot: ManageOpenPositions - Не могу удалить ордер №{0} ", ord.ID);
                //    }
                //}

                if (pos.StopLoss == null && StopLossPoints != 0)
                {
                    // выставляем стоплосс на позицию
                    var stop = (decimal)pos.PriceEnter - pos.Side * DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)StopLossPoints);
                    pos.StopLoss = (float)stop;
                    var res = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), pos);
                    if (res != RequestStatus.OK)
                    {
                        Logger.ErrorFormat("SlideChannelRobot: Не могу установить stoploss={0} на позицию №{1}, вход по цене {2}", stop, pos.ID, pos.PriceEnter);
                    }
                }
                var profitAbs = -pos.Side * (pos.PriceEnter - (pos.Side == 1 ? quote.bid : quote.ask));
                // если стоплосс позиции в убыточной зоне то должен стоять разворотный ордер)
                if (pos.Side == 1 && pos.PriceEnter > pos.StopLoss || pos.Side == -1 && pos.PriceEnter < pos.StopLoss)
                {
                    // проверяем есть ли разворотный ордер на эту позицию
                    var ord = orders.FirstOrDefault(o => o.PriceSide == PendingOrderType.Stop);
                    if (ord == null || ord.Symbol != ticker)
                    {
                        // ордера нет - выставляем
                        if (CountLosses < MaxLossSeries - 1)
                        {
                            CountLosses++; // началась убыточная серия
                            // проверяем можно ли выставить тейк-профит, если у разворотного ордера цена открытия внутри текущего канала - то можем
                            double? take = null;
                            if (pos.Side == 1 && pos.StopLoss > currLowChannelPrice)
                                take = currLowChannelPrice;
                            else
                                if (pos.Side == -1 && pos.StopLoss < currHighChannelPrice)
                                    take = currHighChannelPrice;


                            var res = InstallOrder(ticker, (decimal)pos.StopLoss, (decimal?)take, PendingOrderType.Stop, -pos.Side,
                                         RoundMinVolume);
                            if (res != RequestStatus.OK)
                            {
                                Logger.ErrorFormat("SlideChannelRobot: Не могу установить разворотный ордер на позицию №{1}",pos.ID);
                            }       
                        }
                    }
                    else if (ord.PriceFrom != pos.StopLoss)
                    {
                        // у разворотного ордера не совпадает цена входа со стопом открытой позиции, корректируем
                        ord.PriceFrom = (float) pos.StopLoss;
                        ord.StopLoss = (float)((decimal) ord.PriceFrom -
                             ord.Side*DalSpot.Instance.GetAbsValue(ord.Symbol, (decimal) StopLossPoints));
                        var res = robotContext.SendEditPendingRequest(protectedContext.MakeProtectedContext(), ord);
                        if (res != RequestStatus.OK)
                        {
                            Logger.ErrorFormat(
                                "SlideChannelRobot: Не могу скорректировать ордер №{0}, вход по цене {1} stoploss={2}",
                                ord.ID, ord.PriceFrom, ord.StopLoss);
                        }
                    }
                    // ловим момент когда надо позицию поджимать
                    if (profitAbs > (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)Supress))
                    {
                        // поджимаем позицию в безубыток и удаляем разворотный ордер
                        var stop = (decimal)pos.PriceEnter + pos.Side * DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)SupressPoints);
                        pos.StopLoss = (float)stop;
                        var res = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), pos);
                        if (res != RequestStatus.OK)
                        {
                            Logger.ErrorFormat("SlideChannelRobot: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", stop, pos.ID, pos.PriceEnter);
                        }
                        if (ord != null)
                        {
                            res = robotContext.SendDeletePendingOrderRequest(protectedContext.MakeProtectedContext(), 
                                ord, PendingOrderStatus.Отменен, null, null);
                            if (res != RequestStatus.OK)
                            {
                                Logger.ErrorFormat(
                                    "SlideChannelRobot: Не могу удалить разворотный ордер №{1} при поджатии позиции №{1} в безубыток",
                                    ord.ID, pos.ID);
                            }
                        }
                        CountLosses = 0; // сбрасываем счетчик убыточной серии
                        //// надо выставить ордер с этой же стороны если он стоит за стопом
                        //if (pos.Side == 1 && pos.StopLoss > currLowChannelPrice)
                        //{
                        //    InstallOrder(ticker, (decimal)currLowChannelPrice, (decimal)currHighChannelPrice, PendingOrderType.Limit, 1, minVolume);
                        //}
                        //else
                        //    if (pos.Side == -1 && pos.StopLoss < currHighChannelPrice)
                        //    {
                        //        InstallOrder(ticker, (decimal)currHighChannelPrice, (decimal)currLowChannelPrice, PendingOrderType.Limit, 1, minVolume);
                        //    }
                    }
                }
                else
                {
                    CountLosses = 0; // сбрасываем счетчик убыточной серии
                    // стоп-лосс позиции в прибыльной зоне
                    // проверяем есть ли разворотный ордер на эту позицию
                    var ord = orders.FirstOrDefault(o => o.PriceSide == PendingOrderType.Stop);
                    if (ord != null && ord.Symbol == ticker)
                    {
                        // ордер надо снять, он уже не нужен
                        var res = robotContext.SendDeletePendingOrderRequest(protectedContext.MakeProtectedContext(), 
                            ord, PendingOrderStatus.Отменен, null, null);
                        if (res != RequestStatus.OK)
                        {
                            Logger.ErrorFormat("SlideChannelRobot: Не могу удалить разворотный ордер №{1} для позиции №{1}", ord.ID, pos.ID);
                        }
                    }
                    
                    // теперь проверяем трейлинг позиции
                    // трейлинг хитрый - сначала по обычным параметрам но при достижении тейкпрофита ближе 30 п - трейлинг включается укороченный - 30 п
                    CheckTrailing(pos, quote);
                }
            }
        }
        #endregion 


        private RequestStatus InstallOrder(string symbol, decimal price, decimal? takeProfit, PendingOrderType orderType, int dealSide, int volume)
        {
            
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return RequestStatus.MarginOrLeverageExceeded;

            decimal? stop = StopLossPoints == 0
                                ? (decimal?)null : price - dealSide * DalSpot.Instance.GetAbsValue(symbol, (decimal)StopLossPoints);
            decimal? take = TakeProfitPoints == 0
                                ? takeProfit : price + dealSide * DalSpot.Instance.GetAbsValue(symbol, (decimal)TakeProfitPoints);
            var order = new PendingOrder
                            {
                                Magic = Magic,
                                Symbol = symbol,
                                Volume = volume,
                                Side = dealSide,
                                PriceSide = orderType,
                                PriceFrom = (float)price,
                                StopLoss = (float?)stop,
                                TakeProfit = (float?)take,
                                ExpertComment = "SlideChannelRobot"
                            };
            
            return robotContext.SendNewPendingOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(), order);
        }

        #region CheckTrailing
        private void CheckTrailing(MarketOrder pos, QuoteData quote)
        {
            // проверяем близость текущей цене к TP
            decimal trail = Trailing;
            if (pos.TakeProfit != null && pos.TakeProfit != 0)
            {
                var distanceTake = Math.Abs((float) pos.TakeProfit - (pos.Side == 1 ? quote.bid : quote.ask));
                if (distanceTake <= (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)AreaTP))
                    trail = FinalTrailing;
            }
            else
                trail = ExtraTrailing;
            
            switch (pos.Side)
            {
                case -1:
                    {
                        var currPrice = quote.ask;
                        if (pos.PriceEnter > currPrice)
                        {
                            // первое подтягивание стопа при переходе в безубыточную зону
                            // если стоп не выставлен
                            var trPrice = pos.PriceEnter - (float)DalSpot.Instance.GetAbsValue(pos.Symbol, trail);
                            // если не выставлен стоп-лосс и его уже надо поджать в 0

                            if (pos.StopLoss == null)
                            {
                                if (currPrice <= trPrice)
                                {
                                    // переносим стоп в бу
                                    pos.StopLoss = pos.PriceEnter - (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)SupressPoints);
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }
                            }
                            else
                            {
                                // стоп лосс != 0
                                // если стоп-лосс меньше цены открытия и надо подтянуть его 
                                if (pos.StopLoss > pos.PriceEnter && currPrice <= trPrice)
                                {
                                    // переносим стоп в бу
                                    pos.StopLoss = pos.PriceEnter - (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)SupressPoints);
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }

                                var trstep = (float)pos.StopLoss - (float)DalSpot.Instance.GetAbsValue(pos.Symbol, trail + StepTrailing);
                        
                                if (currPrice <= trstep && pos.StopLoss <= pos.PriceEnter)
                                {
                                    pos.StopLoss = pos.StopLoss - (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)(StepTrailing));
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    {

                
                        var currPrice = quote.bid;
                        if (pos.PriceEnter < currPrice)
                        {
                            // первое подтягивание стопа при переходе в безубыточную зону
                            // если стоп не выставлен
                            var trPrice = pos.PriceEnter + (float)DalSpot.Instance.GetAbsValue(pos.Symbol, trail);
                            // если не выставлен стоп-лосс и его уже надо поджать в 0

                            if (pos.StopLoss == null)
                            {
                                if (currPrice >= trPrice)
                                {
                                    // переносим стоп в бу
                                    pos.StopLoss = pos.PriceEnter + (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)SupressPoints);
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }
                            }
                            else
                            {
                                // стоп лосс != 0
                                // если стоп-лосс меньше цены открытия и надо подтянуть его 
                                if (pos.StopLoss < pos.PriceEnter && currPrice >= trPrice)
                                {
                                    // переносим стоп в бу
                                    pos.StopLoss = pos.PriceEnter + (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)SupressPoints);
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }

                                var trstep = pos.StopLoss + (float)DalSpot.Instance.GetAbsValue(pos.Symbol, trail + StepTrailing);

                                if (currPrice >= trstep && pos.StopLoss >= pos.PriceEnter)
                                {
                                    pos.StopLoss = pos.StopLoss + (float)DalSpot.Instance.GetAbsValue(pos.Symbol, (decimal)(StepTrailing));
                                    var res = robotContext.SendEditMarketRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), pos);
                                    if (res != RequestStatus.OK)
                                    {
                                        Logger.ErrorFormat("CheckTrailing: Не могу переставить в безубыток stoploss={0} на позицию №{1}, вход по цене {2}", pos.StopLoss, pos.ID, pos.PriceEnter);
                                    }
                                }
                            }
                        }

                    }
                    break;
            }
        }
        #endregion

        private RequestStatus EditOrder(PendingOrder ord, decimal priceEnter, decimal? takeProfit)
        {
            ord.StopLoss =  StopLossPoints == 0
                                ? (float?)null
                                : (float?)((decimal)priceEnter - ord.Side * DalSpot.Instance.GetAbsValue(ord.Symbol, (decimal)StopLossPoints));
            ord.TakeProfit = TakeProfitPoints == 0
                                ? (float?)takeProfit
                                : (float?)((decimal)priceEnter + ord.Side * DalSpot.Instance.GetAbsValue(ord.Symbol, (decimal)TakeProfitPoints));
            ord.PriceFrom = (float)priceEnter;
            return robotContext.SendEditPendingRequest(CurrentProtectedContext.Instance.MakeProtectedContext(), ord);
        }

        private bool RemoveAllOrders(List<PendingOrder> orders, PendingOrderType? orderType)
        {
            var isOk = true;
            for (var i = 0; i < orders.Count; i++)
            {
                if (orderType != null && orderType != orders[i].PriceSide) continue;
                var res = robotContext.SendDeletePendingOrderRequest(protectedContext.MakeProtectedContext(), 
                    orders[i], PendingOrderStatus.Отменен, null, string.Empty);
                if (res == RequestStatus.OK) continue;
                isOk = false;
                SaveLog(string.Format("Ошибка удаления ордера №{0}", orders[i].ID));
            }
            return isOk;
        }

        #region Процедуры поиска каналов

        /// <summary>
        /// Поиск каналов
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private Cortege3<PointD, PointD, PointD> TryToFindChannel(ChannelStateInfo currState, out ChannelStateInfo state)
        {
            state = ChannelStateInfo.НетКанала;
            var channel = new Cortege3<PointD, PointD, PointD> { };
            switch (currState)
            {
                case ChannelStateInfo.НетКанала:
                    // канала еще нет, ищем сначала по верхним точкам, если не находим, пробуем по нижним найти
                    var channelhigh = GetHighLineChannel(candles, candles.Count - 1);
                    var channellow = GetLowLineChannel(candles, candles.Count - 1);
                    if ((channelhigh.a == new PointD(0, 0) || channelhigh.b == new PointD(0, 0))
                        && (channellow.a == new PointD(0, 0) || channellow.b == new PointD(0, 0)))
                    {
                        return currChannel;
                    }
                    
                    if (channelhigh.a == new PointD(0, 0) || channelhigh.b == new PointD(0, 0))
                    {
                        state = ChannelStateInfo.ПостроенПоМинимумам;
                        return channellow;
                    }
                    
                    if ((channellow.a == new PointD(0, 0) || channellow.b == new PointD(0, 0)))
                    {
                        state = ChannelStateInfo.ПостроенПоМаксимумам;
                        return channelhigh;
                    }
                    if (channelhigh.b.X > channellow.b.X || (channelhigh.b.X == channellow.b.X && channelhigh.a.X > channellow.a.X))
                    {
                        state = ChannelStateInfo.ПостроенПоМаксимумам;
                        return channelhigh;
                    }
                    state = ChannelStateInfo.ПостроенПоМинимумам;
                    return channellow;

                   
                case ChannelStateInfo.ПостроенПоМаксимумам:
                    channel = GetLowLineChannel(candles, candles.Count - 1);
                    if (channel.a == new PointD(0, 0) || channel.b == new PointD(0, 0))
                        channel = currChannel;
                    else
                        state = ChannelStateInfo.ПостроенПоМинимумам;
                    break;
                case ChannelStateInfo.ПостроенПоМинимумам:
                    channel = GetHighLineChannel(candles, candles.Count - 1);
                    if (channel.a == new PointD(0, 0) || channel.b == new PointD(0, 0))
                        channel = currChannel;
                    else
                        state = ChannelStateInfo.ПостроенПоМаксимумам;
                    break;
            }
            return channel;
        }

        private Cortege3<PointD, PointD, PointD> GetHighLineChannel(List<CandleData> candles, int from)
        {
            // цикл поиска точки b
            for (var i = from - countAfter; i >= from - countAfter - maxBody - 1; i--)
            {
                var b = new PointD(i, candles[i].high);
                // по всем свечкам делаем цикл построения линий от candles[i] до первой границы, ищем линию, в которую укладываются все свечи
                // то есть сдвигаем точку a на шаг назад и снова строим линии - ищем выполнение условий
                // цикл по точкам a
                for (var j = i - minBody - 1; j >= i - maxBody - 1; j--)
                {
                    // есть точки a и b вычисляем k - угловой коэффициент
                    var a = new PointD(j, candles[j].high);
                    var k = (b.Y - a.Y) / (b.X - a.X);

                    // прогоняем проверочные циклы
                    var isFound = true;

                    for (var index = j - countBefore; index <= i + countAfter; index++)
                    {
                        if (index == j || index == i) continue;
                        // получаем значение y на индексе i прямой, проведенной через максимум свечи по индексу b_i
                        var yb = candles[index].high + k * (i - index);
                        if (yb <= b.Y) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;

                    // условия выполнились, теперь надо проверить нижнюю границу, чтобы все свечки в нее вписались
                    var low = j;
                    var y_low = candles[low].low + k * (i - j);
                    for (var index = j; index <= i; index++)
                    {
                        var y_index = candles[index].low + k * (i - index);
                        if (y_low <= y_index) continue;
                        low = index;
                        y_low = y_index;
                    }
                    // теперь проверим свечи за экстремумами

                    for (var l = j - countBefore; l < j; l++)
                    {
                        var y = candles[l].low + k * (i - l);
                        if (y >= y_low) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    for (var l = i + 1; l <= i + countAfter; l++)
                    {
                        var y = candles[l].low + k * (i - l);
                        if (y >= y_low) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    return new Cortege3<PointD, PointD, PointD>(new PointD(j, candles[j].high), new PointD(i, candles[i].high), new PointD(low, candles[low].low));
                }
            }
            return new Cortege3<PointD, PointD, PointD>(new PointD(0, 0), new PointD(0, 0), new PointD(0, 0));
        }

        private Cortege3<PointD, PointD, PointD> GetLowLineChannel(List<CandleData> candles, int from)
        {
            // цикл поиска точки b
            for (var i = from - countAfter; i >= from - countAfter - maxBody - 1; i--)
            {
                var b = new PointD(i, candles[i].low);
                // по всем свечкам делаем цикл построения линий от candles[i] до первой границы, ищем линию, в которую укладываются все свечи
                // то есть сдвигаем точку a на шаг назад и снова строим линии - ищем выполнение условий
                // цикл по точкам a
                for (var j = i - minBody - 1; j >= i - maxBody - 1; j--)
                {
                    // есть точки a и b вычисляем k - угловой коэффициент
                    var a = new PointD(j, candles[j].low);
                    var k = (b.Y - a.Y) / (b.X - a.X);

                    // прогоняем проверочные циклы
                    var isFound = true;

                    for (var index = j - countBefore; index <= i + countAfter; index++)
                    {
                        if (index == j || index == i) continue;
                        // получаем значение y на индексе i прямой, проведенной через максимум свечи по индексу b_i
                        var yb = candles[index].low + k * (i - index);
                        if (yb >= b.Y) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;

                    // условия выполнились, теперь надо проверить верхнюю границу, чтобы все свечки в нее вписались
                    var high = j;
                    var y_high = candles[high].high + k * (i - j);
                    for (var index = j; index <= i; index++)
                    {
                        var y_index = candles[index].high + k * (i - index);
                        if (y_high >= y_index) continue;
                        high = index;
                        y_high = y_index;
                    }
                    // теперь проверим свечи за экстремумами

                    for (var l = j - countBefore; l < j; l++)
                    {
                        var y = candles[l].high + k * (i - l);
                        if (y <= y_high) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    for (var l = i + 1; l <= i + countAfter; l++)
                    {
                        var y = candles[l].high + k * (i - l);
                        if (y <= y_high) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    return new Cortege3<PointD, PointD, PointD>(new PointD(j, candles[j].low), new PointD(i, candles[i].low), new PointD(high, candles[high].high));
                }
            }
            return new Cortege3<PointD, PointD, PointD>(new PointD(0, 0), new PointD(0, 0), new PointD(0, 0));
        }
        #endregion
    }
}
