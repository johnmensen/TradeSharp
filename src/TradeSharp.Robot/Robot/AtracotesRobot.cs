using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Atracotes MTS")]
    [TypeConverter(typeof(PropertySorter))]
    class AtracotesRobot : BaseRobot
    {
        [Serializable]
        public struct ExtensionsLevel
        {
            public float price;
            public int startIndex;
            public int length;
            public float delta; // зазор попаданий цены на уровень
            public int goalFrom; // -1 уровень выше текущей цены, 1 - уровень ниже текущей цены
        }

        private CandlePacker packer;

        private List<CandleData> candles;

        private DateTime curTime;

        // найденные уровни расширений
        private readonly List<ExtensionsLevel> extLevels = new List<ExtensionsLevel>();

        // фибоуровни для расширений, читаем из настроек
        private readonly List<float> fiboLevels = new List<float>();

        // список используемых индексов
        private List<IndexDivergencyInfo> indexList = new List<IndexDivergencyInfo>();

        [LocalizedDisplayName("TitleIndexList")]
        [Description("Список используемых индексов и искомых дивергенций по ним")]
        [LocalizedCategory("TitleMain")]
        [PropertyXMLTag("Robot.IndexName")]
        [PropertyOrder(1)]
        public List<IndexDivergencyInfo> IndexList
        {
            get { return indexList; }
            set { indexList = value; }
        }

        private float thresholdPercent = 1;
        [LocalizedDisplayName("TitleThresholdInPercents")]
        [Description("Пороговая величина, %, на которую должен отклониться курс для формирования нового экстремума")]
        [PropertyXMLTag("Robot.ThresholdPercent")]
        [LocalizedCategory("TitleExtensionLevelSettings")]
        [PropertyOrder(2)]
        public float ThresholdPercent
        {
            get { return thresholdPercent; }
            set { thresholdPercent = value; }
        }

        [LocalizedDisplayName("TitleZigzagPrice")]
        [Description("Цены Зиг-Зага")]
        [Category("Проекции")]
        [PropertyXMLTag("Robot.ZigZagSourceType")]
        public ZigZagSource ZigZagSourceType { get; set; }

        [LocalizedDisplayName("TitleExtensionLevels")]
        [Description("Значения уровней расширений, которые будут откладыватся по ZigZag")]
        [LocalizedCategory("TitleExtensionLevelSettings")]
        [PropertyXMLTag("Robot.FiboLevels")]
        [PropertyOrder(3)]
        public string FiboLevels { get; set; }

        [LocalizedDisplayName("TitleUsedBarsCount")]
        [Description("Количество баров для заглядывания вперед")]
        [LocalizedCategory("TitleExtensionLevelSettings")]
        [PropertyXMLTag("Robot.BarsCount")]
        [PropertyOrder(4)]
        public int BarsCount { get; set; }

        [LocalizedDisplayName("TitlePriceDeviationFromLevel")]
        [Description("Максимальное допустимое недостижение ценой уровня в пунктах")]
        [LocalizedCategory("TitleExtensionLevelSettings")]
        [PropertyXMLTag("Robot.DeltaLevels")]
        [PropertyOrder(5)]
        public float DeltaLevel { get; set; }

        public enum ProtectType { ПоХудшейПозиции = 0, ПоЛучшейПозиции = 1, ПоУсредненнойЦене = 2 }
        [LocalizedDisplayName("TitlePositionProtection")]
        [Description("Правила защиты открытых позиций")]
        [LocalizedCategory("TitleMain")]
        [PropertyXMLTag("Robot.ProtectType")]
        public ProtectType ProtectPosType { get; set; }

        [DisplayName("Поджатие позиций в профит в п.")]
        [Description("Отступ в пунктах от защищаемой цены")]
        [LocalizedCategory("TitleMain")]
        [PropertyXMLTag("Robot.DeltaProtect")]
        public int DeltaProtect { get; set; }

        private Dictionary<string, double> lastBids = new Dictionary<string, double>();

        /// <summary>
        /// содержит цены close для тикеров, входящих в индекс в виде close#5, eurgbp#1 ...
        /// </summary>
        private Dictionary<string, List<double>> lastBidLists;

        private string[] tickerNames;


        private int countCandles;

        private string ticker;

        private Random randomGener;

        private int candlesInIndexHistory = 50;
        [PropertyXMLTag("Robot.CandlesInIndexHistory")]
        [DisplayName("Длина истории индекса")]
        [LocalizedCategory("TitleMain")]
        [Description("Длина истории индекса, свечей")]
        public int CandlesInIndexHistory
        {
            get { return candlesInIndexHistory; }
            set { candlesInIndexHistory = value; }
        }

        private int volume = 10000;
        [PropertyXMLTag("Robot.Volume")]
        [DisplayName("Объем сделки")]
        [Category("Торговые")]
        [Description("Объем сделки, ед. базовой валюты")]
        [PropertyOrder(6)]
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        private int leverage = 1;
        [PropertyXMLTag("Robot.Leverage")]
        [DisplayName("Торговое плечо")]
        [Category("Торговые")]
        [Description("Максимальное торговое плечо, разрешенное роботу")]
        [PropertyOrder(7)]
        public int Leverage
        {
            get { return leverage; }
            set { leverage = value; }
        }

        private int countTrades = 5;
        [PropertyXMLTag("Robot.CountTrades")]
        [DisplayName("Количество позиций")]
        [Category("Торговые")]
        [Description("Количество позиций, которые можно открывать в одном направлении")]
        [PropertyOrder(8)]
        public int CountTrades
        {
            get { return countTrades; }
            set { countTrades = value; }
        }

        public enum AddPositionRule { НаБареЛучшейЦены = 0, ПоНовымТочкамВходаИЛучшейЦене = 1 }
        [PropertyXMLTag("Robot.AddPositionRule")]
        [DisplayName("Способ наращивания объема")]
        [Category("Торговые")]
        [Description("Способ наращивания уже открытых позиций")]
        [PropertyOrder(9)]
        public AddPositionRule AddPositionRules
        {
            get;
            set;
        }

        public enum TradeState { НетПозиций = 0, ПоискТочекВхода = 1, НаращиваниеПокупок = 2, НаращиваниеПродаж = 3, НабранПолныйОбъем = 4 }

        [PropertyXMLTag("Robot.TradeState")]
        [DisplayName("Торговое состояние робота")]
        [Category("Торговые")]
        [Description("Текущее состояние торгового робота")]
        [PropertyOrder(10)]
        public TradeState RobotTradeState
        {
            get;
            set;
        }

        [PropertyXMLTag("Robot.UseProtectPositionsFlag")]
        [DisplayName("Защищать позиции")]
        [Category("Торговые")]
        [Description("Использовать защиту позиций")]
        [PropertyOrder(11)]
        public bool UseProtectPositionsFlag { get; set; }

        [PropertyXMLTag("Robot.ActualFiboLevel")]
        [DisplayName("Уровень расширения для торговли")]
        [Category("Торговые")]
        [Description("Актуальный уровень расширения, который необходимо достичь для открытия позиций")]
        [PropertyOrder(12)]
        public float? ActualFiboLevel { get; set; }
        
        enum StopPositionsState { Незащищены = 0, ЗащитаУстановлена = 1 }

        private StopPositionsState stopsOnPositions = StopPositionsState.Незащищены;

        public override BaseRobot MakeCopy()
        {
            var res = new AtracotesRobot { indexList = new List<IndexDivergencyInfo>() };
            foreach (var ind in indexList)
            {
                var indi = new IndexDivergencyInfo
                {
                    IndexFormulaL = ind.IndexFormulaL,
                    IndexMarginUp = ind.IndexMarginUp,
                    IndexMarginDn = ind.IndexMarginDn,
                    IsNaN = ind.IsNaN,
                    DiverType = ind.DiverType,
                    PeriodExtremum = ind.PeriodExtremum,
                    MaxPastExtremum = ind.MaxPastExtremum
                };
                res.indexList.Add(indi);
            }
            res.ThresholdPercent = ThresholdPercent;
            res.FiboLevels = FiboLevels;
            res.BarsCount = BarsCount;
            res.DeltaLevel = DeltaLevel;
            res.CandlesInIndexHistory = CandlesInIndexHistory;
            res.Volume = Volume;
            res.CountTrades = CountTrades;
            res.UseProtectPositionsFlag = UseProtectPositionsFlag;
            res.stopsOnPositions = stopsOnPositions;
            res.DeltaProtect = DeltaProtect;
            res.RobotTradeState = RobotTradeState;
            res.Leverage = Leverage;
            res.ZigZagSourceType = ZigZagSourceType;
            CopyBaseSettings(res);
            return res;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("Atracotes MTS: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("Atracotes MTS: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;

            lastBids = new Dictionary<string, double>();

            foreach (var ind in IndexList)
            {
                ind.Initialize();
                ind.lastIndicies = new List<double>();
                ind.indexPeaks = new List<Cortege3<decimal, int, decimal>>();
            }

            candles = new List<CandleData>();
            packer = new CandlePacker(Graphics[0].b);

            var levels = FiboLevels.ToFloatArrayUniform();
            fiboLevels.Clear();
            foreach (var level in levels)
            {
                try
                {
                    fiboLevels.Add(level);
                }
                catch (FormatException)
                {

                }
            }

            tickerNames = DalSpot.Instance.GetTickerNames();
            randomGener = new Random(DateTime.Now.Millisecond);
            lastBidLists = new Dictionary<string, List<double>>();
            // по каждой валютной паре найти макс. количество отсчетов (из формулы индекса)
            InitLastBidLists();
            //RobotTradeState = TradeState.НетПозиций;
            stopsOnPositions = StopPositionsState.Незащищены;
            TerminalLog.Instance.SaveRobotLog("Atracotes MTS: RobotTradeState = " + RobotTradeState);
        }

        private void InitLastBidLists()
        {
            foreach (var ind in IndexList)
            {
                foreach (var varName in ind.indexCalculator.formulaVariableNames)
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

                    if (lastBidLists.ContainsKey(tickerPart))
                        /*if (lastBidLists[tickerPart].MaxQueueLength >= queueLen)*/ continue;
                    var queue = new List<double>();
                    if (lastBidLists.ContainsKey(tickerPart))
                        lastBidLists[tickerPart] = queue;
                    else lastBidLists.Add(tickerPart, queue);

                    //if (tickerPart == ticker && candles.MaxQueueLength < queueLen)
                    //    candles = new RestrictedQueue<CandleData>(queueLen);
                }
            }
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            var historyIndexStart = Graphics[0].b.GetDistanceTime(CandlesInIndexHistory + 1, -1, startTrade);
            // получить котиры из индексов
            var ret = base.GetRequiredSymbolStartupQuotes(startTrade);
            foreach (var ind in IndexList)
            {
                if (string.IsNullOrEmpty(ind.indexFormula)) continue;
                var lowerFormula = ind.indexFormula.ToLower();
                var formulaTickers = tickerNames.Where(tn => lowerFormula.Contains(tn.ToLower())).ToList();
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
            if (/*formulaResolver == null || */packer == null) return null;
            curTime = quotes[0].timeClose;

            // обновить табличку цен
            for (var i = 0; i < names.Length; i++)
            {
                if (lastBids.ContainsKey(names[i]))
                    lastBids[names[i]] = (double)quotes[i].close;
                else
                    lastBids.Add(names[i], (double)quotes[i].close);
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

            var hints = new List<RobotHint>();
            if (candle != null)
            {
                // закрылась полная свеча, проводим вычисления
                candles.Add(candle);


                countCandles++;
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
                foreach (var ind in IndexList)
                    ind.CalculateValue(tickerNames, candles, lastBidLists, curTime, randomGener);
                
                // если это период "разгона" конвейера
                if (isHistoryStartOff) return null;

                var hintText = new StringBuilder();
                List<MarketOrder> orders;

                #region ТОРГОВЫЙ МОДУЛЬ

                #region проверяем необходимость добавок к открытым позициям

                // проверяем состояние робота, если у нас нет позиций и робот торгует, возвращаем в исходное состояние
                GetMarketOrders(out orders);
                if (orders.Count == 0 && stopsOnPositions == StopPositionsState.ЗащитаУстановлена)
                {
                    RobotTradeState = TradeState.ПоискТочекВхода;
                    TerminalLog.Instance.SaveRobotLog("Atracotes MTS: Позиции закрылись");
                    TerminalLog.Instance.SaveRobotLog("Atracotes MTS: RobotTradeState = " + RobotTradeState);
                    stopsOnPositions = StopPositionsState.Незащищены;
                }
                var addPosFlag = 0;
                if (RobotTradeState == TradeState.НаращиваниеПокупок || RobotTradeState == TradeState.НаращиваниеПродаж)
                {
                    var side = RobotTradeState == TradeState.НаращиваниеПокупок ? 1 :
                                RobotTradeState == TradeState.НаращиваниеПродаж ? -1 : 0;
                    if (AddPositionRules == AddPositionRule.НаБареЛучшейЦены)
                    {
                        //GetMarketOrders(out orders);
                        if (orders.Count == 0)
                        {
                            addPosFlag = RobotTradeState == TradeState.НаращиваниеПокупок ? 1 : -1;
                        }
                        else
                        {
                            var averagePrice = GetAveragePrice(orders, side, curQuote.close, ProtectType.ПоЛучшейПозиции);
                            if (side == 1 && curQuote.close < averagePrice) addPosFlag = 1;
                            if (side == -1 && curQuote.close > averagePrice) addPosFlag = -1;

                            // проверяю достижение актуального фибоуровня
                            if (ActualFiboLevel != null)
                            {
                                // если условие выполнилось, то мы не достигли уровня для открытия позиций
                                if ((addPosFlag == 1 && ActualFiboLevel < curQuote.close) || 
                                    (addPosFlag == -1 && ActualFiboLevel > curQuote.close))
                                    addPosFlag = 0;
                            }
                        }
                    }
                }

                if (addPosFlag != 0)
                {
                    GetMarketOrders(out orders);
                    var ordersCount = orders.Count;
                    // проверяем можно ли еще открыть позиции
                    if (ordersCount < CountTrades)
                    {

                        // открыть позу в направлении знака дивера
                        robotContext.SendNewOrderRequest(
                            protectedContext.MakeProtectedContext(),
                            RequestUniqueId.Next(),
                            new MarketOrder
                                {
                                    AccountID = robotContext.AccountInfo.ID,
                                    Magic = Magic,
                                    Symbol = ticker,
                                    Volume = GetTradeLot(),
                                    Side = addPosFlag,
                                    ExpertComment = "Atracotes MTS"
                                },
                            OrderType.Market, 0, 0);
                            
                        if (ordersCount > 0)
                        {
                            hintText.AppendLine(string.Format("Добавка к {0}, текущая цена {1}",
                                                              addPosFlag == 1 ? "покупкам" : "продажам", curQuote.close));
                        }
                        else
                        {
                            hintText.AppendLine(string.Format("{0}, текущая цена {1}", addPosFlag == 1 ? "покупка" : "продажа",
                                                curQuote.close));
                        }
                        TerminalLog.Instance.SaveRobotLog("Atracotes MTS: " + hintText);
                        hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), hintText.ToString(),
                            addPosFlag > 0 ? "BUY" : "SELL", "e", curQuote.close)
                        {
                            Time = curQuote.timeClose,
                            ColorFill = addPosFlag > 0 ? Color.Green : Color.Red,
                            ColorLine = Color.Black,
                            RobotHintType = addPosFlag > 0 ? RobotHint.HintType.Покупка : RobotHint.HintType.Продажа
                        });
                        stopsOnPositions = StopPositionsState.Незащищены;
                        if (ordersCount + 1 == CountTrades)
                        {
                            RobotTradeState = TradeState.НабранПолныйОбъем;
                            TerminalLog.Instance.SaveRobotLog("Atracotes MTS: RobotTradeState = " + RobotTradeState);
                        }
                    }
                }
                #endregion

                #region считаем зигзагу и уровни расширений

                var quotesList = candles.ToList();
                var pivots = ZigZag.GetPivots(quotesList, ThresholdPercent, ZigZagSourceType);
                if (pivots.Count > 1)
                {
                    // появились точки, рассчитываем уровни
                    // проверяем процентный порог перешагнули или нет

                    for (var i = pivots.Count; i > 1; i--)
                    {
                        var index0 = i - 1;
                        var index1 = i - 2;
                        var percent = Math.Abs(pivots[index0].b - pivots[index1].b) / pivots[index1].b * 100;
                        if (percent > ThresholdPercent)
                        {
                            // процентный порог пройден, вычисляем уровни
                            // теперь вычисляем уровни расширений
                            var sign = pivots[index0].b > pivots[index1].b ? -1 : 1;
                            var delta = Math.Abs(pivots[index0].b - pivots[index1].b);

                            // очищаем все предыдущие уровни расширений как устаревшие
                            // extLevels.Clear();

                            foreach (var level in fiboLevels)
                            {
                                var lev = new ExtensionsLevel
                                {
                                    startIndex = countCandles > CandlesInIndexHistory ? countCandles - CandlesInIndexHistory + pivots[index0].a : pivots[index0].a,
                                    length = BarsCount,
                                    price = pivots[index0].b + delta * sign * (1 + level),
                                    delta = DalSpot.Instance.GetAbsValue(ticker, DeltaLevel),
                                    goalFrom = -sign
                                };
                                extLevels.Add(lev);
                            }
                            break;
                        }
                    }
                }
                #endregion

                #region ищем дивергенции на индексах и курсе валютной пары
                var divergenceSign = 0;

                foreach (var ind in IndexList)
                {
                    var commentOnDivergence = string.Empty;

                    var indiDiverSign = ind.GetDivergenceSign(candles, out commentOnDivergence);
                    if (indiDiverSign != 0)
                    {
                        if (hintText.Length != 0)
                            hintText.AppendLine();
                        hintText.AppendLine("Дивергенция на индексе №" + indexList.IndexOf(ind));
                        hintText.AppendLine(commentOnDivergence);
                        hintText.AppendLine("Переменные:");
                        // ReSharper disable PossibleNullReferenceException)
                        foreach (var pair in ind.indexCalculator.varValues)
                        // ReSharper restore PossibleNullReferenceException
                        {
                            hintText.AppendLine(string.Format("{1}{0}{2:f4}", (char)9, pair.Key, pair.Value));
                        }

                        //hints.Add(new RobotHint(hintText.ToString(),
                        //indiDiverSign > 0 ? "BUY" : "SELL", "e", curQuote.Bid)
                        //{
                        //    Time = candle.timeOpen,
                        //    ColorFill = indiDiverSign > 0 ? Color.Green : Color.Red,
                        //    ColorLine = Color.Black
                        //});

                    }


                    divergenceSign += indiDiverSign;

                }
                #endregion


                // теперь получен список дивергенций и уровни расширения. 

                #region удаляем старые уровни
                for (var i = 0; i < extLevels.Count; i++)
                {
                    // проверка актуальности уровня по свечам
                    if (countCandles - 1 <= extLevels[i].startIndex + extLevels[i].length) continue;
                    // уровень устарел, прошли максимальную длину свечей
                    extLevels.RemoveAt(i);
                    i--;
                }
                #endregion

                // проверяем достижение уровней расширения текущей ценой и если есть - входим в рынок
                divergenceSign = Math.Sign(divergenceSign);



                if (stopsOnPositions == StopPositionsState.Незащищены && divergenceSign != 0)
                {
                    GetMarketOrders(out orders);
                    stopsOnPositions = ProtectPositions(orders, -divergenceSign, curQuote.close);
                    if (stopsOnPositions == StopPositionsState.ЗащитаУстановлена)
                    {
                        TerminalLog.Instance.SaveRobotLog("Atracotes MTS: Позиции защищены ны дивергенции");
                        hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), 
                            "Защита позиций на дивергенции", "PROTECT", -divergenceSign > 0 ? "PB" : "PS", curQuote.close)
                        {
                            Time = curQuote.timeClose,
                            ColorFill = Color.Blue,
                            ColorLine = Color.Black,
                            RobotHintType = RobotHint.HintType.Поджатие
                        });
                    }
                }

                if (true || RobotTradeState == TradeState.НетПозиций || RobotTradeState == TradeState.ПоискТочекВхода || RobotTradeState == TradeState.НабранПолныйОбъем)
                {
                    foreach (var level in extLevels)
                    {
                        // проверяем попадание цены на уровень
                        //var prevCandle = candles.GetItemByIndex(candles.Count() - 2, true);
                        if ((level.goalFrom == -1 && candles[candles.Count - 1].close >= (level.price - level.delta)) ||
                            (level.goalFrom == 1 && candles[candles.Count - 1].close <= (level.price + level.delta)))
                        {
                            GetMarketOrders(out orders);
                            if (stopsOnPositions == StopPositionsState.Незащищены && divergenceSign == 0)
                            {
                                // надо поджать позиции
                                stopsOnPositions = ProtectPositions(orders, -level.goalFrom, curQuote.close);
                                if (stopsOnPositions == StopPositionsState.ЗащитаУстановлена)
                                {
                                    TerminalLog.Instance.SaveRobotLog("Atracotes MTS: Позиции защищены по достижению уровня расширения");
                                    hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), 
                                        "Защита позиций по достижению уровня расширения",
                                        "PROTECT", -level.goalFrom > 0 ? "PB" : "PS", curQuote.close)
                                    {
                                        Time = curQuote.timeClose,
                                        ColorFill = Color.Blue,
                                        ColorLine = Color.Black,
                                        RobotHintType = RobotHint.HintType.Поджатие
                                    });
                                }
                                break;
                            }

                            if (divergenceSign == 0) continue;

                            var ordersCount = orders.Count;
                            // цена поднялась до целевого уровня
                            var level1 = level;
                            var ordersToClose = orders.FindAll(o => o.Side != divergenceSign);

                            foreach (var order in ordersToClose)
                            {
                                robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                    robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                                ordersCount--;
                            }
                            // закрыли ордера и теперь начинаем открывать позиции в обратную сторону

                            // проверяем можно ли еще открыть позиции
                            if (ordersCount >= CountTrades) break;

                            // не добавляем позиции по новому диверу
                            if (RobotTradeState == TradeState.НаращиваниеПокупок && divergenceSign == 1 ||
                                RobotTradeState == TradeState.НаращиваниеПродаж && divergenceSign == -1) break;

                            // открыть позу в направлении знака дивера
                            robotContext.SendNewOrderRequest(
                            protectedContext.MakeProtectedContext(),
                            RequestUniqueId.Next(),
                            new MarketOrder
                            {
                                AccountID = robotContext.AccountInfo.ID,
                                Magic = Magic,
                                Symbol = ticker,
                                Volume = GetTradeLot(),
                                Side = divergenceSign,
                                ExpertComment = "Atracotes MTS"
                            },
                            OrderType.Market, 0, 0);

                            hintText.AppendLine(string.Format("Пересечение уровня {0} {1}, текущая цена {2}",
                                                              level1.price,
                                                              level1.goalFrom == -1 ? "снизу" : "сверху", curQuote.close));

                            TerminalLog.Instance.SaveRobotLog("Atracotes MTS: " + hintText + ", " + (divergenceSign > 0 ? "покупка" : "продажа"));
                            hints.Add(new RobotHint(Graphics[0].a, Graphics[0].b.ToString(),
                                hintText.ToString(), divergenceSign > 0 ? "BUY" : "SELL", "e", curQuote.close)
                            {
                                Time = curQuote.timeClose,
                                ColorFill = divergenceSign > 0 ? Color.Green : Color.Red,
                                ColorLine = Color.Black,
                                RobotHintType = divergenceSign > 0 ? RobotHint.HintType.Покупка : RobotHint.HintType.Продажа
                            });

                            RobotTradeState = divergenceSign == 1
                                                  ? TradeState.НаращиваниеПокупок

                                                  : TradeState.НаращиваниеПродаж;
                            TerminalLog.Instance.SaveRobotLog("Atracotes MTS: RobotTradeState = " + RobotTradeState);
                            stopsOnPositions = StopPositionsState.Незащищены;
                            break;
                        }
                    }
                }

                #endregion
            }

            var retHint = hints.Select(hint => hint.ToString()).ToList();
            return retHint.Count > 0 ? retHint : null;
        }

        /// <summary>
        /// защита позиций выставлением стопов
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="side"> </param>
        /// <param name="currPrice"> </param>
        /// <returns></returns>
        private StopPositionsState ProtectPositions(List<MarketOrder> orders, int side, float currPrice)
        {
            if (UseProtectPositionsFlag == false || orders.Count == 0)
                return StopPositionsState.Незащищены;

            var sideOrders = orders.Where(order => order.Side == side);
            if (sideOrders.Count() == 0)
                return StopPositionsState.Незащищены;

            var res = StopPositionsState.ЗащитаУстановлена;
            var price = GetAveragePrice(orders, side, currPrice, ProtectPosType);

            // текущая цена не позволяет установить стопы
            if ((side > 0 && price > currPrice) || (side < 0 && price < currPrice))
                return StopPositionsState.Незащищены;

            foreach (var order in sideOrders)
            {
                order.StopLoss = price;
                var ordRes = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), order);
                if (ordRes != RequestStatus.OK)
                    res = StopPositionsState.Незащищены;
            }
            return res;
        }

        /// <summary>
        /// Вычисление средней цены серии открытых позиций, зависит от настройки параметра ProtectPosType
        /// либо средняя цена всей серии либо цена худшей открытой позиции
        /// </summary>
        private float GetAveragePrice(List<MarketOrder> orders, int side, float currPrice, ProtectType type)
        {
            var price = 0f;
            switch (type)
            {
                case ProtectType.ПоУсредненнойЦене:
                    {
                        // вычисляем по позициям средневзвешенную цену
                        float sumBuys = 0, sumSell = 0;
                        var sumDeals = orders.FindAll(o => o.Symbol == ticker && o.Magic == Magic);
                        var exposition = 0;
                        foreach (var sumDeal in sumDeals)
                        {
                            exposition += sumDeal.Side * sumDeal.Volume;

                            if (sumDeal.Side > 0)
                                sumBuys += sumDeal.Volume * sumDeal.PriceEnter;
                            else
                                sumSell += sumDeal.Volume * sumDeal.PriceEnter;
                        }
                        price = exposition == 0 ? 0 : (sumBuys - sumSell) / exposition;
                    }
                    break;
                case ProtectType.ПоХудшейПозиции:
                    foreach (var order in orders)
                    {
                        if (price == 0)
                        {
                            price = order.PriceEnter;
                            continue;
                        }
                        if (order.Side != side) continue;
                        if (side == 1) //покупки
                        {
                            if (price < order.PriceEnter)
                                price = order.PriceEnter;
                        }
                        else
                        {
                            // продажи
                            if (price > order.PriceEnter)
                                price = order.PriceEnter;
                        }
                    }
                    break;
                case ProtectType.ПоЛучшейПозиции:
                    foreach (var order in orders)
                    {
                        if (price == 0)
                        {
                            price = order.PriceEnter;
                            continue;
                        }
                        if (order.Side != side) continue;
                        if (side == 1) //покупки
                        {
                            if (price > order.PriceEnter)
                                price = order.PriceEnter;
                        }
                        else
                        {
                            // продажи
                            if (price < order.PriceEnter)
                                price = order.PriceEnter;
                        }
                    }
                    break;
            }
            // price = получили цену для поджатия
            if (side == 1)
                price += DalSpot.Instance.GetAbsValue(ticker, (float)DeltaProtect);
            else
                price -= DalSpot.Instance.GetAbsValue(ticker, (float)DeltaProtect);
            return price;
        }

        private int GetTradeLot()
        {
            return Volume;
            //return Convert.ToInt32(Math.Round(robotContext.accountInfo.Equity * Leverage / CountTrades / 1000) * 1000);
        }
    }
}