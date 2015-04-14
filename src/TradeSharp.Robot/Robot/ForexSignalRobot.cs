using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("Торговый робот проекта ForexSignal")]
    [TypeConverter(typeof(PropertySorter))]
    public class ForexSignalRobot : BaseRobot
    {
        /// <summary>
        /// свечи для проверки условий входа
        /// </summary>
        private CandlePacker packer;

        private Dictionary<DiversByTimeframe, CandlePacker> packerDiverProtect;

        private List<CandleData> candles;

        private Dictionary<DiversByTimeframe, List<CandleData>> candles4Protect;

        private DateTime curTime;
        
        #region Визуальные настройки

        [Category("Визуальные")]
        [PropertyXMLTag("Robot.CalcDealChainStat")]
        [DisplayName("Распред. кол-ва входов")]
        [Description("Статистика распределения длин последовательностей входов")]
        [PropertyOrder(1)]
        public bool CalcDealChainStat { get; set; }

        private bool showEnterMarker = false;
        [Category("Визуальные")]
        [PropertyXMLTag("Robot.ShowEnterMarker")]
        [DisplayName("Маркер входа")]
        [Description("Показывать меркер входа в рынок")]
        [PropertyOrder(2)]
        public bool ShowEnterMarker
        {
            get { return showEnterMarker; }
            set { showEnterMarker = value; }
        }

        private bool showExitMarker = false;
        [Category("Визуальные")]
        [PropertyXMLTag("Robot.ShowExitMarker")]
        [DisplayName("Маркер входа")]
        [Description("Показывать меркер выхода из рынка")]
        [PropertyOrder(3)]
        public bool ShowExitMarker
        {
            get { return showExitMarker; }
            set { showExitMarker = value; }
        }

        private bool showProtectEventMarker = true;
        [Category("Визуальные")]
        [PropertyXMLTag("Robot.ShowProtectEventMarker")]
        [DisplayName("Маркер события \"защиты\"")]
        [Description("Показывать события \"защиты\" позиций")]
        [PropertyOrder(4)]
        public bool ShowProtectEventMarker
        {
            get { return showProtectEventMarker; }
            set { showProtectEventMarker = value; }
        }

        private bool showProtectMarker = false;
        [Category("Визуальные")]
        [PropertyXMLTag("Robot.ShowEventMarker")]
        [DisplayName("Маркер \"защиты\" сделки")]
        [Description("Показывать маркер \"защиты\" отдельных позиций")]
        [PropertyOrder(5)]
        public bool ShowProtectMarker
        {
            get { return showProtectMarker; }
            set { showProtectMarker = value; }
        }

        #endregion

        #region Настройки объема и количества входов

        private int maxDealsInChain = 5;
        [PropertyXMLTag("Robot.MaxDealsInChain")]
        [DisplayName("Макс. сделок")]
        [Category("Объемы")]
        [Description("Макс. количество подряд идущих покупок-продаж")]
        [PropertyOrder(6)]
        public int MaxDealsInChain
        {
            get { return maxDealsInChain; }
            set { maxDealsInChain = value; }
        }

        [PropertyXMLTag("Robot.FixedVolumeStep"),
        DisplayName("Фикс. шаг объема"),
        Category("Объемы"),
        Description("Шаг фиксированного объема")]
        [PropertyOrder(8)]
        public int FixedVolumeStep { get; set; }

        [PropertyXMLTag("Robot.FixedLeverageStep"),
         DisplayName("Шаг плеча сделки"),
         Category("Объемы"),
         Description("Фикс. шаг для плеча отдельных сделок (если не задан фикс. объем)")]
        [PropertyOrder(10)]
        public decimal FixedLeverageStep { get; set; }

        private int minVolume = 10000;
        [PropertyXMLTag("Robot.MinVolume"),
        DisplayName("Мин. объем сделки"),
        Category("Объемы"),
        Description("Мин. объем сделки (если задано плечо)")]
        [PropertyOrder(11)]
        public int MinVolume
        {
            get { return minVolume; }
            set { minVolume = value; }
        }

        private int volumeStep = 10000;
        [PropertyXMLTag("Robot.VolumeStep"),
        DisplayName("Мин. шаг объема"),
        Category("Объемы"),
        Description("Мин. шаг объема сделки (если задано плечо)")]
        [PropertyOrder(12)]
        public int VolumeStep
        {
            get { return volumeStep; }
            set { volumeStep = value; }
        }

        #endregion

        #region Настройки так любимых мной расширений
        private List<FibonacciFilterCondition> fiboFilters = new List<FibonacciFilterCondition>();
        [Category("Основные")]
        [PropertyXMLTag("Robot.FiboFilters")]
        [DisplayName("Фильтры расширений")]
        [Description("Фильтры \"расширений\" и \"коррекций\"")]
        [PropertyOrder(13)]
        public List<FibonacciFilterCondition> FiboFilters
        {
            get { return fiboFilters; }
            set { fiboFilters = value; }
        }

        [Category("Основные")]
        [PropertyXMLTag("Robot.FiboEnabled")]
        [DisplayName("Фильтр расширений")]
        [Description("Фильтр \"расширений\" включен / выключен")]
        [PropertyOrder(14)]
        public bool FiboEnabled { get; set; }

        #endregion

        #region Условия защиты по Фибоначчи

        private List<FibonacciFilterCondition> fibonacciProtect = new List<FibonacciFilterCondition>();
        [Category("Защита")]
        [PropertyXMLTag("Robot.FibonacciProtect")]
        [DisplayName("Защита по проекциям")]
        [Description("Условия защиты по проекциям и коррекциям")]
        [PropertyOrder(15)]
        public List<FibonacciFilterCondition> FibonacciProtect
        {
            get { return fibonacciProtect; }
            set { fibonacciProtect = value; }
        }

        #endregion
        /// <summary>
        /// список используемых для входа индексов
        /// </summary> 
        private List<IndexDivergencyInfo> diversToEnter = new List<IndexDivergencyInfo>();
        [Category("Дивергенции")]
        [PropertyXMLTag("Robot.DiversToEnter")]
        [DisplayName("Условия входа")]
        [Description("Дивергенции с польз. индексами, по которым осуществляется вход")]
        [PropertyOrder(16)]
        public List<IndexDivergencyInfo> DiversToEnter
        {
            get { return diversToEnter; }
            set { diversToEnter = value; }
        }

        private List<DiversByTimeframe> diversToProtect = new List<DiversByTimeframe>();        

        [Category("Дивергенции")]
        [PropertyXMLTag("Robot.DiversToProtect")]
        [DisplayName("Условия \"защиты\"")]
        [Description("Дивергенции с польз. индексами, по которым осуществляется \"защита\"")]
        [PropertyOrder(17)]
        public List<DiversByTimeframe> DiversToProtect
        {
            get { return diversToProtect; }
            set { diversToProtect = value; }
        }

        public enum ProtectType { НеЗащищать = 0, Индивидуально = 1, ПоСреднейЦене = 2, ПоХудшейЦене = 3 }
        [Category("Защита")]
        [DisplayName("Защита позиций")]
        [PropertyXMLTag("Robot.ProtectType")]
        [Description("Правила защиты позиций")]
        [PropertyOrder(18)]
        public ProtectType ProtectPosType { get; set; }

        private int protectTarget = 5;
        [Category("Защита")]
        [DisplayName("Смещение стопа")]
        [PropertyXMLTag("Robot.ProtectTarget")]
        [Description("Отступ в пунктах от защищаемой цены")]
        [PropertyOrder(19)]
        public int ProtectTarget
        {
            get { return protectTarget; }
            set { protectTarget = value; }
        }

        private int protectLevel = 30;
        [Category("Защита")]
        [DisplayName("Уровень")]
        [PropertyXMLTag("Robot.ProtectLevel")]
        [Description("Уровень, после которого производится защита")]
        [PropertyOrder(20)]
        public int ProtectLevel
        {
            get { return protectLevel; }
            set { protectLevel = value; }
        }

        private int unconditionalProtectPips = 0;
        [Category("Защита")]
        [DisplayName("Защита по цене, пп")]
        [PropertyXMLTag("Robot.UcProtectPips")]
        [Description("Безусловная защита, по достижению цены")]
        [PropertyOrder(21)]
        public int UnconditionalProtectPips
        {
            get { return unconditionalProtectPips; }
            set { unconditionalProtectPips = value; }
        }

        private int protectSensitivity = 1;
        [Category("Защита")]
        [DisplayName("Мин. перемещение стопа")]
        [PropertyXMLTag("Robot.ProtectSensitivity")]
        [Description("Минимальное перемещение стоп-ордера при защите")]
        [PropertyOrder(22)]
        public int ProtectSensitivity
        {
            get { return protectSensitivity; }
            set { protectSensitivity = value; }
        }

        private List<UserIndexFilter> filters = new List<UserIndexFilter>();
        [Category("Фильтры")]
        [DisplayName("Фильтры польз. инд.")]
        [PropertyXMLTag("Robot.Filter")]
        [Description("Фильтрация по пользовательским индексам")]
        [PropertyOrder(23)]
        public List<UserIndexFilter> Filters
        {
            get { return filters; }
            set { filters = value; }
        }

        private Dictionary<string, double> lastBids = new Dictionary<string, double>();

        /// <summary>
        /// содержит цены close для тикеров, входящих в индекс в виде close#5, eurgbp#1 ...
        /// </summary>
        private Dictionary<string, List<double>> lastBidLists;

        private string[] tickerNames;

        private int countCandles;

        private string ticker;

        private Random randomGener;

        private List<int> dealChainStat;

        /// <summary>
        /// список защищаемых сделок
        /// </summary>
        private ProtectList protectList;

        private int candlesInIndexHistory = 50;
        [PropertyXMLTag("Robot.CandlesInIndexHistory")]
        [DisplayName("Длина истории индекса")]
        [Category("Основные")]
        [Description("Длина истории индекса, свечей")]
        [PropertyOrder(24)]
        public int CandlesInIndexHistory
        {
            get { return candlesInIndexHistory; }
            set { candlesInIndexHistory = value; }
        }

        public enum TradeState { НетПозиций = 0, ПоискТочекВхода = 1, ВРынке = 2 }

        [PropertyXMLTag("Robot.UseProtectPositionsFlag")]
        [DisplayName("Защищать позиции")]
        [Category("Торговые")]
        [Description("Использовать защиту позиций")]
        [PropertyOrder(25)]
        public bool UseProtectPositionsFlag { get; set; }

        public override BaseRobot MakeCopy()
        {
            var res = new ForexSignalRobot { diversToEnter = new List<IndexDivergencyInfo>() };
            foreach (var indi in diversToEnter.Select(ind => new IndexDivergencyInfo(ind)))            
                res.diversToEnter.Add(indi);
            
            foreach (var protectSets in diversToProtect.Select(div => new DiversByTimeframe(div)))            
                res.diversToProtect.Add(protectSets);  
          
            foreach (var filter in filters)
                res.filters.Add(new UserIndexFilter(filter));

            foreach (var fibo in fibonacciProtect)
                res.fibonacciProtect.Add(new FibonacciFilterCondition(fibo));

            res.CandlesInIndexHistory = CandlesInIndexHistory;
            res.UseProtectPositionsFlag = UseProtectPositionsFlag;
            res.ProtectLevel = ProtectLevel;
            res.ProtectTarget = ProtectTarget;
            res.ProtectPosType = ProtectPosType;
            res.ProtectSensitivity = ProtectSensitivity;
            res.ShowEnterMarker = ShowEnterMarker;
            res.ShowExitMarker = ShowExitMarker;
            res.ShowProtectEventMarker = ShowProtectEventMarker;
            res.ShowProtectMarker = ShowProtectMarker;
            res.CalcDealChainStat = CalcDealChainStat;
            res.Leverage = Leverage;
            res.FixedLeverageStep = FixedLeverageStep;
            res.FixedVolume = FixedVolume;
            res.FixedVolumeStep = FixedVolumeStep;
            res.MaxDealsInChain = MaxDealsInChain;
            res.MinVolume = MinVolume;
            res.VolumeStep = VolumeStep;
            res.fiboFilters = fiboFilters.Select(f => new FibonacciFilterCondition(f)).ToList();
            res.FiboEnabled = FiboEnabled;
            res.UnconditionalProtectPips = UnconditionalProtectPips;
            res.Leverage = Leverage;
            res.RoundType = RoundType;
            res.NewsChannels = NewsChannels;
            res.RoundMinVolume = RoundMinVolume;
            res.RoundVolumeStep = RoundVolumeStep;

            CopyBaseSettings(res);
            return res;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("MultiIndexRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("MultiIndexRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;

            lastBids = new Dictionary<string, double>();

            foreach (var ind in DiversToEnter)
            {
                ind.Initialize();                    
                ind.lastIndicies = new List<double>();
                ind.indexPeaks = new List<Cortege3<decimal, int, decimal>>();
            }

            foreach (var protectSets in DiversToProtect)
                foreach (var ind in protectSets.IndexList)
            {
                ind.Initialize();
                ind.lastIndicies = new List<double>();
                ind.indexPeaks = new List<Cortege3<decimal, int, decimal>>();
            }

            foreach (var filter in filters)
                filter.Initialize();

            candles = new List<CandleData>();//CandlesInIndexHistory);
            packer = new CandlePacker(Graphics[0].b);
            packerDiverProtect = diversToProtect.Count == 0
                                     ? null
                                     : diversToProtect.ToDictionary(d => d, d => new CandlePacker(d.timeframe));
            candles4Protect = diversToProtect.Count == 0
                                     ? null
                                     : diversToProtect.ToDictionary(d => d, d => new List<CandleData>());
            
            tickerNames = DalSpot.Instance.GetTickerNames();
            randomGener = new Random(DateTime.Now.Millisecond);
            lastBidLists = new Dictionary<string, List<double>>();
            // по каждой валютной паре найти макс. количество отсчетов (из формулы индекса)
            InitLastBidLists();

            if (CalcDealChainStat)
            {
                dealChainStat = new List<int>();                
            }
        }

        private void InitLastBidLists()
        {
            var tickers = new List<string>();
            foreach (var ind in DiversToEnter)
                tickers.AddRange(ind.indexCalculator.formulaVariableNames);
            foreach (var filter in filters)
                tickers.AddRange(filter.indexCalculator.formulaVariableNames);
            tickers = tickers.Distinct().ToList();
                        
            foreach (var varName in tickers)
            {
                var queueLen = 1;
                var tickerPart = varName.ToUpper();
                if (varName.Contains('#'))
                {
                    var indexSharp = varName.IndexOf('#');
                    if (indexSharp == 0 || indexSharp == (varName.Length - 1)) continue;
                    var numPart = varName.Substring(indexSharp + 1).ToIntSafe();
                    if (!numPart.HasValue) continue;
                    queueLen = numPart.Value + 1;
                    tickerPart = varName.Substring(0, indexSharp).ToUpper();
                }

                if (tickerPart == "CLOSE" || tickerPart == "OPEN" ||
                    tickerPart == "HIGH" || tickerPart == "LOW")
                    tickerPart = ticker;
                if (!tickerNames.Contains(tickerPart)) continue;

                if (lastBidLists.ContainsKey(tickerPart)) continue;
                var queue = new List<double>();
                if (lastBidLists.ContainsKey(tickerPart))
                    lastBidLists[tickerPart] = queue;
                else lastBidLists.Add(tickerPart, queue);

                //if (tickerPart == ticker && candles.MaxQueueLength < queueLen)
                //    candles = new RestrictedQueue<CandleData>(queueLen);
            }            
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            var historyIndexStart = Graphics[0].b.GetDistanceTime(CandlesInIndexHistory + 1, -1, startTrade);
            // получить котиры из индексов
            var ret = base.GetRequiredSymbolStartupQuotes(startTrade);
            
            var formulas = DiversToEnter.Select(indi => indi.indexFormula.ToLower()).ToList();
            foreach (var diver in diversToProtect)
                formulas.AddRange(diver.IndexList.Select(indi => indi.indexFormula.ToLower()));
            formulas.AddRange(filters.Select(filter => filter.IndexFormula.ToLower()));

            foreach (var lowerFormula in formulas)
            {
                var formula = lowerFormula;
                var formulaTickers = tickerNames.Where(tn => formula.Contains(tn.ToLower())).ToList();
                if (lowerFormula.Contains("open") ||
                    lowerFormula.Contains("close") ||
                    lowerFormula.Contains("high") ||
                    lowerFormula.Contains("low"))
                    if (Graphics != null && Graphics.Count > 0)
                        if (!formulaTickers.Contains(Graphics[0].a))
                            formulaTickers.Add(Graphics[0].a);

                foreach (var tick in formulaTickers)
                {
                    if (ret.ContainsKey(tick))
                        ret[tick] = historyIndexStart;
                    else
                        ret.Add(tick, historyIndexStart);
                }
            }
            return ret;
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var hints = new List<RobotHint>();
            if (packer == null) return null;
            curTime = quotes[0].timeClose;

            // обновить табличку цен
            for (var i = 0; i < names.Length; i++)
            {
                if (lastBids.ContainsKey(names[i]))
                    lastBids[names[i]] = quotes[i].close;
                else
                    lastBids.Add(names[i], quotes[i].close);
            }

            CandleDataBidAsk curQuote = null;
            for (var i = 0; i < names.Length; i++)
                if (names[i] == ticker)
                {
                    curQuote = quotes[i];
                    break;
                }

            if (curQuote == null) return null; // нет торгуемой котировки

            // обновить свечки
            var candle = packer.UpdateCandle(curQuote);
            if (candle != null)
            {
                // закрылась полная свеча, проводим вычисления
                candles.Add(candle);
                countCandles++;
            }

            List<MarketOrder> orders = null;
            if (ProtectPosType != ProtectType.НеЗащищать)
            {
                GetMarketOrders(out orders);
                if (orders.Count > 0)
                {
                    if (unconditionalProtectPips > 0)
                        CheckUnconditionalProtect(orders, curQuote.GetCloseQuote(), hints);
                    // проверить множество индексов на нескольких ТФ - условия защиты
                    CheckProtectTrigger(orders, curQuote.GetCloseQuote(), hints, isHistoryStartOff, candle);
                    // проверить собственно защиту
                    CheckProtect(orders, curQuote.GetCloseQuote(), hints);
                }
            }
            if (CalcDealChainStat)
            {
                if (orders == null) GetMarketOrders(out orders);
                UpdateChainStatistics(orders);
            }            

            if (candle != null)
            {
                // закрылась полная свеча, проводим вычисления               
                // обновить очереди (для индекса, переменные вида usdjpy#15)
                if (lastBidLists.Count > 0)
                {
                    foreach (var listTicker in lastBidLists)
                    {
                        double price;
                        if (!lastBids.TryGetValue(listTicker.Key, out price)) price = 0;
                        listTicker.Value.Add(price);
                    }
                }

                // посчитать индексы
                foreach (var ind in DiversToEnter)
                    ind.CalculateValue(tickerNames, candles, lastBidLists, curTime, randomGener);                

                // индексы для фильтров
                foreach (var filter in filters)
                    filter.indexCalculator.CalculateValue(tickerNames, candles, lastBidLists, 
                        curTime, randomGener);

                // если это период "разгона" конвейера
                if (isHistoryStartOff) return null;

                // получить суммарный знак диверов на текущей свече
                var divergenceSign = 0;
                var commentBuilder = new StringBuilder();
                foreach (var ind in DiversToEnter)
                {
                    string commentOnDivergence;
                    var indiDiverSign = ind.GetDivergenceSign(candles, out commentOnDivergence);
                    divergenceSign += indiDiverSign;
                    if (!string.IsNullOrEmpty(commentOnDivergence))
                        commentBuilder.AppendLine(commentOnDivergence);
                }
                divergenceSign = Math.Sign(divergenceSign);
                
                // есть дивер?
                if (divergenceSign != 0)
                    TryEnterTheMarket(divergenceSign, curQuote.GetCloseQuote(), hints, commentBuilder.ToString());
            }

            var retHint = hints.Select(hint => hint.ToString()).ToList();
            return retHint.Count > 0 ? retHint : null;
        }

        private void TryEnterTheMarket(int divergenceSign, QuoteData curQuote,
            List<RobotHint> hints, string divergenceHint)
        {
            // проверить фильтры
            foreach (var filter in filters)
            {
                if (filter.IsEnterProhibided((DealType)divergenceSign)) 
                    return; // запрет входа фильтром
            }
            // фильтр Фибоначчпхи
            if (FiboEnabled)
                if (!IsFiboFilterPassed(divergenceSign)) return;

            List<MarketOrder> orders;
            GetMarketOrders(out orders);
            var ordersCount = orders.Count;
            
            // закрыть противоположные ордера
            var ordersToClose = orders.FindAll(o => o.Side != divergenceSign);
            
            foreach (var order in ordersToClose)
            {
                var requestRst = robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                    robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                // добавить объект - коментарий на график
                if (ShowExitMarker)
                {
                    var orderResult = order.Side == 1
                                          ? curQuote.bid - order.PriceEnter
                                          : order.PriceEnter - curQuote.ask;
                    orderResult = DalSpot.Instance.GetPointsValue(order.Symbol, orderResult);
                    var dealInfo = new StringBuilder();
                    dealInfo.AppendLine(string.Format(
                        "{0} №{1}. {2}, р-т {3}",
                        requestRst == RequestStatus.OK
                            ? "Закрытие"
                            : "[" + requestRst + "]",
                        order.ID,
                        order.Side == 1 ? "BUY" : "SELL",
                        (int) orderResult));
                    dealInfo.Append(string.Format("Открыта {0:dd.MM.yyyy HH:mm} по {1:f4}",
                                                  order.TimeEnter, order.PriceEnter));

                    hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), 
                        divergenceHint, dealInfo.ToString(), "q", curQuote.bid)
                                  {
                                      Time = curQuote.time,
                                      ColorFill = order.Side == 1 ? Color.Green : Color.Red,
                                      ColorLine = Color.Black,
                                      RobotHintType = RobotHint.HintType.Коментарий
                                  });
                }
                ordersCount--;
            }
            
            // открыть позу в направлении знака дивера
            if (!OpenDeal(divergenceSign, orders)) return;            

            if (ShowEnterMarker)
                hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), 
                    divergenceHint, divergenceSign > 0 ? "BUY" : "SELL", "e", curQuote.bid)
                              {
                                  Time = curQuote.time,
                                  ColorFill = divergenceSign > 0 ? Color.Green : Color.Red,
                                  ColorLine = Color.Black,
                                  RobotHintType =
                                      divergenceSign > 0
                                          ? RobotHint.HintType.Покупка
                                          : RobotHint.HintType.Продажа
                              });            
        }

        private bool IsFiboFilterPassed(int side)
        {
            foreach (var filter in fiboFilters)
            {
                if (FiboLevelReached(side, filter.ZigZagThreshold,
                    filter.FiboLevel, filter.ZigZagSource, filter.MaxPointsToFibo,
                    filter.MaxCandlesFromLevel)) return true;
            }
            // коментировать
            Logger.Info("Вход запрещен фильтром ЗЗ");
            return false;
        }

        /// <summary>
        /// проверить и осуществить трейлинг
        /// </summary>
        private void CheckUnconditionalProtect(List<MarketOrder> orders, QuoteData curQuote,
            List<RobotHint> hints)
        {
            foreach (var order in orders)
            {
                var result = order.Side > 0
                                 ? curQuote.bid - order.PriceEnter
                                 : order.PriceEnter - curQuote.ask;
                var resultPoints = DalSpot.Instance.GetPointsValue(order.Symbol, result);
                if (resultPoints < unconditionalProtectPips) continue;
                var stop = order.PriceEnter + order.Side *
                           DalSpot.Instance.GetAbsValue(order.Symbol, (float)protectTarget);
                var deltaPips = DalSpot.Instance.GetPointsValue(order.Symbol, Math.Abs(stop - (order.StopLoss ?? 0)));
                if (deltaPips <= protectSensitivity) continue;
                
                // передвинуть стоп
                order.StopLoss = stop;
                robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), order);
                if (ShowProtectMarker)
                {
                    var title = "Безусловная защита сделки " + order.ID;
                    hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), title, title, "p", curQuote.bid)
                    {
                        RobotHintType = RobotHint.HintType.Коментарий,
                        Time = curQuote.time,
                        ColorFill = Color.White,
                        ColorLine = Color.DarkBlue,
                        ColorText = Color.Black
                    });
                }
            }
        }

        private bool FiboLevelReached(int dealSide,
            float zzPercent, 
            float checkedFiboLevel,
            ZigZagSource zzSource, int maxPointsToFibo,
            int maxCandlesPassed)
        {
            // получить точки ЗЗ
            var countCandles = candles.Count;
            var candleList = candles.ToList();
            var pivots = ZigZag.GetPivots(candleList, zzPercent, zzSource);
            if (pivots.Count < 2)
            {
                return false;
            }
            // исключить последнюю точку ЗЗ?
            var deltaLast = 100 * Math.Abs(pivots[pivots.Count - 1].b - pivots[pivots.Count - 2].b) /
                pivots[pivots.Count - 2].b;
            var start = pivots.Count - 1;
            if (deltaLast < zzPercent) // исключаем последнюю точку
                start--;

            // поиск подходящего расширения, чтобы разрешить вход
            // ищем только на одну точку назад!
            for (var i = start; i > 0 && i >= start - 1; i--)
            {
                var b = pivots[i];
                if (b.a + maxCandlesPassed < countCandles) break;
                var a = pivots[i - 1];
                // только "поддержки" для покупок и "сопротивления" для продаж
                var isSupport = a.b < b.b;
                if ((!isSupport && dealSide > 0) || (isSupport && dealSide < 0)) continue;
                var level = a.b + (a.b - b.b) * (checkedFiboLevel - 1);

                // проверить, достигался ли уровень в пределах допустимого расстояния
                // любой из свечек
                var reached = false;
                for (var j = b.a + 1; j < countCandles; j++)
                {
                    var delta = isSupport ? level - candleList[j].low
                                    : candleList[j].high - level;

                    if (delta > 0) reached = true;
                    else
                        if (maxPointsToFibo > 0) // подсчитать сколько не дотянули в пп
                        {
                            var points = DalSpot.Instance.GetPointsValue(ticker, -delta);
                            if (points < maxPointsToFibo) reached = true;
                        }
                    if (reached) break;
                }
                if (reached) return true;
            }
            return false;
        }

        private bool OpenDeal(int divergenceSign, List<MarketOrder> orders)
        {
            // проверить длину цепочки
            var chain = 0;
            for (var i = orders.Count - 1; i >= 0; i--)
            {
                if (orders[i].Side != divergenceSign) break;
                chain++;
            }
            if (chain >= MaxDealsInChain) return false;

            // объем сделки
            int volume = FixedVolume ?? 0;
            if (volume > 0)
                volume += chain * FixedVolumeStep;
            else
            {
                // высчитать объем исходя из плеча
                var leverage = Leverage + chain * FixedLeverageStep;
                if (leverage == 0) return false;
                volume = (int)(robotContext.AccountInfo.Equity * leverage);
                if (volume < MinVolume) return false;
                volume /= VolumeStep;
                volume *= VolumeStep;
            }

            var rest = robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                    {
                        AccountID = robotContext.AccountInfo.ID,
                        Magic = Magic,
                        Symbol = ticker,
                        Volume = volume,
                        Side = divergenceSign,
                        ExpertComment = "ForexSignal robot"
                    },
                OrderType.Market, 0, 0);
            return rest == RequestStatus.OK;
        }

        private void UpdateChainStatistics(List<MarketOrder> orders)
        {            
            //if (lastChainLength > 0)
            //{
            //    // обновить стат. по длине серий сделок
            //    dealChainStat.Add(lastChainLength);
            //    // построить гистограмму и вывести в лог
            //    var max = dealChainStat.Max();
            //    var hist = new int[max];
            //    foreach (var chain in dealChainStat)
            //        hist[chain - 1]++;
            //    var sb = new StringBuilder("Распределение длин послед. сделок: ");
            //    for (var i = 0; i < max; i++)
            //    {
            //        if (hist[i] > 0)
            //            sb.AppendFormat("{0}-{1:f1}%  ", i + 1, 100*hist[i]/dealChainStat.Count);
            //    }
            //    Logger.Info(sb.ToString());
            //}                            
        }

        /// <summary>
        /// проверить условия защиты либо по диверам, либо - по Фибоначчи
        /// прошерстить все таймфреймы, на каждом ТФ - все диверы
        /// проверить, выполняется ли условие на защиту покупок или продаж
        /// </summary>
        private void CheckProtectTrigger(
            List<MarketOrder> orders,
            QuoteData curQuote,
            List<RobotHint> hints,
            bool isHistoryStartOff,
            CandleData newCandle)
        {
            if (diversToProtect.Count == 0 && fibonacciProtect.Count == 0) return;

            var protectConditions = new StringBuilder();
            int diverSign = CheckProtectByDivers(curQuote, isHistoryStartOff, protectConditions);
            if (diverSign == 0)
                diverSign = CheckProtectByFibos(protectConditions, orders, newCandle);
            if (diverSign == 0) return;
            // защищаем сделки с указанным знаком
            var protectSide = (DealType) diverSign;
            // создать новый список защиты
            // старый либо актуализируется, либо игнорируется
            orders = orders.Where(o => o.Side == (int)protectSide).ToList();
            if (orders.Count == 0) return;

            var newProtectList = new ProtectList
            {
                orderIds = orders.Select(o => o.ID).ToList(),
                side = protectSide
            };
            if (protectList != null)
                if (newProtectList.AreEqual(protectList)) return;
            protectList = newProtectList;

            // добавить маркер на график
            if (ShowProtectEventMarker)
            {
                var eventTitle = protectSide == DealType.Buy 
                    ? string.Format("Защита {0} покупок", orders.Count)
                    : string.Format("Защита {0} продаж", orders.Count);
                var eventText = "Условия: " + protectConditions;
                hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), eventText, eventTitle, "p", curQuote.bid)
                {
                    Time = curQuote.time,
                    ColorFill = Color.Yellow,
                    ColorLine = Color.Black,
                    RobotHintType = RobotHint.HintType.Коментарий
                        //diverSign > 0
                        //    ? RobotHint.HintType.Покупка
                        //    : RobotHint.HintType.Продажа
                });
            }            
            //Logger.InfoFormat("CheckProtectTrigger: защита сделок [{0}] типа {1}",
            //                  string.Join(", ", protectList.orderIds), protectList.side);
        }

        private int CheckProtectByDivers(QuoteData curQuote, bool isHistoryStartOff, StringBuilder diverKinds)
        {
            if (diversToProtect.Count == 0) return 0;
            var diverSign = 0;            
            foreach (var diverSet in diversToProtect)
            {
                var candle = packerDiverProtect[diverSet].UpdateCandle(curQuote.bid, curQuote.time);
                if (candle == null) continue;
                var candleQueue = candles4Protect[diverSet];
                candleQueue.Add(candle);
                
                // сформировалась новая свеча - проверить дивергенции с индексами);
                foreach (var diver in diverSet.IndexList)
                {
                    diver.CalculateValue(tickerNames, candleQueue, lastBidLists, curTime, randomGener);
                    if (isHistoryStartOff) continue;
                    string divergenceText;
                    var indiDiverSign = diver.GetDivergenceSign(candleQueue, out divergenceText);
                    //if (!string.IsNullOrEmpty(divergenceText))
                    //    Logger.InfoFormat("CheckProtectTrigger[{0}]: {1}",
                    //    BarSettingsStorage.Instance.GetBarSettingsFriendlyName(diverSet.Timeframe),
                    //    divergenceText);
                    diverSign += indiDiverSign;
                    if (indiDiverSign != 0 && ShowProtectEventMarker)
                    {
                        diverKinds.Append(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(diverSet.timeframe));                        
                        diverKinds.Append(indiDiverSign > 0 ? ":Б " : ":М ");
                    }
                }                
            }
            return -diverSign;
        }

        private int CheckProtectByFibos(StringBuilder conditions, List<MarketOrder> orders,
            CandleData newCandle)
        {
            if (newCandle == null) return 0;
            if (fibonacciProtect.Count == 0 || orders.Count == 0) return 0;
            var dealSign = orders[0].Side;
            foreach (var fibo in fibonacciProtect)
            {
                // получить точки ЗЗ, проверить условие (уровень Фибо или коррекции)
                if (FiboLevelReached(-dealSign, fibo.ZigZagThreshold, 
                    fibo.FiboLevel, fibo.ZigZagSource,
                    fibo.MaxPointsToFibo, fibo.MaxCandlesFromLevel))
                {
                    if (ShowProtectEventMarker)
                        conditions.AppendFormat("Защита по ЗЗ ({0:f1}%, {1:f3})",
                                                fibo.ZigZagThreshold, fibo.FiboLevel);
                    return dealSign;
                }
            }
            return 0;
        }

        /// <summary>
        /// собственно защита
        /// </summary>        
        private void CheckProtect(List<MarketOrder> orders, QuoteData curQuote,
            List<RobotHint> hints)
        {
            if (protectList == null) return;
            if (protectList.orderIds.Count == 0)
            {
                protectList = null;
                return;
            }
            var ordersToProtect = (from order in orders let orderId = order.ID where 
                                       protectList.orderIds.Contains(orderId) select order).ToList();
            if (ordersToProtect.Count == 0) return;
            
            var pointCost = DalSpot.Instance.GetAbsValue(ordersToProtect[0].Symbol, 1f);

            // индивидуальная "защита"
            if (ProtectPosType == ProtectType.Индивидуально)
            {
                foreach (var order in ordersToProtect)
                {
                    var targetStop = order.PriceEnter + order.Side*ProtectTarget*pointCost;
                    // проверка - не пытаться переставить стоп на 0.4 пп например
                    var delta = Math.Abs(targetStop - (order.StopLoss ?? 0));
                    delta = delta/pointCost;
                    if (delta < ProtectSensitivity) continue;
                    // проверка контрольной отметки
                    var orderProtectLevel = order.PriceEnter + order.Side*ProtectLevel*pointCost;
                    var shouldProtect = order.Side == 1
                                            ? curQuote.bid > orderProtectLevel
                                            : curQuote.bid < orderProtectLevel;
                    if (!shouldProtect) continue;
                    order.StopLoss = targetStop;
                    robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), order);
                    if (ShowProtectMarker)
                    {
                        var title = "Защита сделки " + order.ID;
                        hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), title, title, "p", curQuote.bid)
                                      {
                                          RobotHintType = RobotHint.HintType.Коментарий,
                                          Time = curQuote.time,
                                          ColorFill = Color.White,
                                          ColorLine = Color.DarkBlue,
                                          ColorText = Color.Black
                                      });
                    }
                    protectList.orderIds.Remove(order.ID);
                }
                if (protectList.orderIds.Count == 0) protectList = null;
                return;
            }

            // защита по "медианнной цене" или по "худшей цене"
            // найти ту самую медианную цену
            float medPrice = 0;
            if (ProtectPosType == ProtectType.ПоСреднейЦене)
            { // средняя цена...
                var sumPrice = ordersToProtect.Sum(o => o.PriceEnter * o.Volume);
                var sumVolume = ordersToProtect.Sum(o => o.Volume);
                medPrice = sumPrice/sumVolume;
            }
            if (ProtectPosType == ProtectType.ПоХудшейЦене)
            {
                var date = ordersToProtect[0].TimeEnter;
                medPrice = ordersToProtect[0].PriceEnter;
                for (var i = 1; i < ordersToProtect.Count; i++)
                {
                    if (ordersToProtect[i].TimeEnter >= date) continue;
                    date = ordersToProtect[i].TimeEnter;
                    medPrice = ordersToProtect[i].PriceEnter;
                }
            }

            var stopPrice = medPrice + ordersToProtect[0].Side * ProtectTarget * pointCost;
            // проверить все ордера
            var dealProtected = false;
            foreach (var order in ordersToProtect)
            {
                var delta = Math.Abs(stopPrice - (order.StopLoss ?? 0));
                delta = delta / pointCost;
                if (delta < ProtectSensitivity) continue;
                // цена прошла рубеж?
                var shouldProtect = order.Side == 1
                                            ? curQuote.bid > medPrice
                                            : curQuote.bid < medPrice;
                if (!shouldProtect) continue;
                dealProtected = true;
                order.StopLoss = stopPrice;                
                protectList.orderIds.Remove(order.ID);
            }
            if (ShowProtectMarker && dealProtected)
            {
                var text = new StringBuilder();
                text.AppendLine("Защита сделок " + string.Join(", ", ordersToProtect.Select(o => o.ID)));
                text.AppendFormat("Средневзвеш. цена: {0:f4}", medPrice);
                hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), "Защита сделок", text.ToString(), "p", curQuote.bid)
                {
                    RobotHintType = RobotHint.HintType.Коментарий,
                    Time = curQuote.time,
                    ColorFill = Color.White,
                    ColorLine = Color.DarkBlue,
                    ColorText = Color.Black
                });
            }

            if (protectList.orderIds.Count == 0) protectList = null;
            return;
        }
    }
    // ReSharper restore LocalizableElement

    /// <summary>
    /// список сделок, которые надо защитить - кровь из попы
    /// </summary>
    class ProtectList
    {
        /// <summary>
        /// знак защищаемых сделок
        /// </summary>
        public DealType side;

        public List<int> orderIds = new List<int>();

        public bool AreEqual(ProtectList list)
        {
            if (side != list.side) return false;
            if (orderIds.Count != list.orderIds.Count) return false;
            for (var i = 0; i < orderIds.Count; i++)
                if (orderIds[i] != list.orderIds[i]) return false;
            return true;
        }
    }

    /// <summary>
    /// описывает условия триггера на "защиту" по Фибоначчи или "коррекции"
    /// </summary>
    public class FibonacciFilterCondition
    {
        private float zigZagThreshold = 1;
        [Category("Защита ЗЗ")]
        [PropertyXMLTag("FiboFilter.Threshold")]
        [DisplayName("% ЗЗ")]
        [Description("Порог Зиг-Зага, %")]
        public float ZigZagThreshold
        {
            get { return zigZagThreshold; }
            set { zigZagThreshold = value; }
        }

        [Category("Защита ЗЗ")]
        [PropertyXMLTag("FiboFilter.ZigZagSource")]
        [DisplayName("Тип цены ЗЗ")]
        [Description("Тип цены ЗЗ, от которого считаются проекции")]
        public ZigZagSource ZigZagSource { get; set; }

        [Category("Защита ЗЗ")]
        [PropertyXMLTag("FiboFilter.MaxPointsToFibo")]
        [DisplayName("Макс расст, пп")]
        [Description("Макс расстояние от рынка до \"расширения\"")]
        public int MaxPointsToFibo { get; set; }

        private int maxCandlesFromLevel = 40;
        [Category("Защита ЗЗ")]
        [PropertyXMLTag("FiboFilter.MaxCandlesFromLevel")]
        [DisplayName("Макс свеч")]
        [Description("Макс свечей вперед от \"расширения\"")]
        public int MaxCandlesFromLevel
        {
            get { return maxCandlesFromLevel; }
            set { maxCandlesFromLevel = value; }
        }

        private float fiboLevel = 1.618F;
        [Category("Защита ЗЗ")]
        [PropertyXMLTag("FiboFilter.FiboLevel")]
        [DisplayName("Уровень расширения")]
        [Description("Уровень \"расширения\" (если > 1) или - коррекции (< 1)")]
        public float FiboLevel
        {
            get { return fiboLevel; }
            set { fiboLevel = value; }
        }

        public FibonacciFilterCondition()
        {
        }

        public FibonacciFilterCondition(FibonacciFilterCondition src)
        {
            zigZagThreshold = src.zigZagThreshold;
            ZigZagSource = src.ZigZagSource;
            MaxPointsToFibo = src.MaxPointsToFibo;
            maxCandlesFromLevel = src.maxCandlesFromLevel;
            fiboLevel = src.fiboLevel;
        }
    }
}
