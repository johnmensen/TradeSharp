using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    /// <summary>
    /// От "фракталов" строятся Фибоначчи, потом ищется дивер индекса - и - вуаля! вход в рынок
    /// </summary>
    [DisplayName("Кросс-конгруэнтные формации")]
    [TypeConverter(typeof(PropertySorter))]
    class MultiIndexRobot : BaseRobot
    {
        #region Настройки
        
        public enum EnterChainCondition
        {
            НаЗакрытииЛучше = 0,
            НаНовойПроекции = 1
        }

        [PropertyXMLTag("Robot.EnterChain")]
        [DisplayName("Повторн. входы")]
        [Description("Правила последующих входов")]
        [Category("Правила входа")]
        public EnterChainCondition EnterChain { get; set; }

        private int forgetDiverCandles = 50;

        [PropertyXMLTag("Robot.ForgetDiverCandles")]
        [DisplayName("Забыть дивер")]
        [Description("Забыть дивер после N свеч")]
        [Category("Правила входа")]
        public int ForgetDiverCandles
        {
            get { return forgetDiverCandles; }
            set { forgetDiverCandles = value; }
        }
        
        /// <summary>
        /// список используемых индексов 
        /// </summary>
        private List<IndexDivergencyInfo> indexList = new List<IndexDivergencyInfo>();

        [PropertyXMLTag("Robot.IndexName")]
        [DisplayName("Список индексов")]
        [Description("Список используемых индексов и искомых дивергенций по ним")]
        [Category("Правила входа")]
        public List<IndexDivergencyInfo> IndexList
        {
            get { return indexList; }
            set { indexList = value; }
        }

        private int fractalPeriod = 2;
        [DisplayName("Период фрактала")]
        [Description("Период экстремума, называемого в просторечии \"фракталом\"")]
        [PropertyXMLTag("Robot.FractalPeriod")]
        [Category("Правила входа")]
        public int FractalPeriod
        {
            get { return fractalPeriod; }
            set { fractalPeriod = value; }
        }

        [DisplayName("Цена для \"фракталов\"")]
        [Description("Тип цены для \"фракталов\"")]
        [PropertyXMLTag("Robot.FractalSourceType")]
        [Category("Правила входа")]
        public ZigZagSource FractalSourceType { get; set; }

        private float fiboLevel = 0.618f;
        [DisplayName("Уровень расширения")]
        [Description("Уровень \"расширения\", откладываемого по \"фракталам\"")]
        [PropertyXMLTag("Robot.FiboLevel")]
        [Category("Правила входа")]
        public float FiboLevel
        {
            get { return fiboLevel; }
            set { fiboLevel = value; }
        }

        private int fiboBarsCount = 30;
        [DisplayName("Количество используемых баров")]
        [Description("Количество баров для заглядывания вперед")]
        [PropertyXMLTag("Robot.FiboBarsCount")]
        [Category("Правила входа")]
        public int FiboBarsCount
        {
            get { return fiboBarsCount; }
            set { fiboBarsCount = value; }
        }

        [DisplayName("Отклонение цены от уровня")]
        [Description("Максимальное допустимое недостижение ценой уровня в пунктах")]
        [PropertyXMLTag("Robot.DeltaLevels")]
        [Category("Правила входа")]
        public float DeltaLevel { get; set; }

        [DisplayName("\"Защита\", пп")]
        [PropertyXMLTag("Robot.DeltaProtect")]
        [Description("Отступ в пунктах от защищаемой цены")]
        [Category("Торговые")]
        public int DeltaProtect { get; set; }

        public enum ProtectType { ПоХудшейПозиции = 0, ПоЛучшейПозиции = 1, ПоУсредненнойЦене = 2}
        [DisplayName("Защита позиций")]
        [PropertyXMLTag("Robot.ProtectType")]
        [Description("Правила защиты открытых позиций")]
        [Category("Торговые")]
        public ProtectType ProtectPosType { get; set; }

        private int candlesInIndexHistory = 50;
        [PropertyXMLTag("Robot.CandlesInIndexHistory")]
        [DisplayName("Длина истории индекса")]
        [Description("Длина истории индекса, свечей")]
        [Category("Правила входа")]
        public int CandlesInIndexHistory
        {
            get { return candlesInIndexHistory; }
            set { candlesInIndexHistory = value; }
        }

        private int volume = 10000;
        [PropertyXMLTag("Robot.Volume")]
        [DisplayName("Объем сделки")]
        [Description("Объем сделки, ед. базовой валюты")]
        [Category("Торговые")]
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        private int countTrades = 5;
        [PropertyXMLTag("Robot.CountTrades")]
        [DisplayName("Количество позиций")]
        [Description("Количество позиций, которые можно открывать в одном направлении")]
        [Category("Торговые")]
        public int CountTrades
        {
            get { return countTrades; }
            set { countTrades = value; }
        }

        [PropertyXMLTag("Robot.UseProtectPositionsFlag")]
        [DisplayName("Защищать позиции")]
        [Description("Использовать защиту позиций")]
        [Category("Торговые")]
        public bool UseProtectPositionsFlag { get; set; }

        [PropertyXMLTag("Robot.ExtendedMarks")]
        [DisplayName("Комментарии на графике")]
        [Description("Выводить доп. комментарии на графике")]
        [Category("Комментарии")]
        public bool ExtendedMarks { get; set; }

        #endregion

        #region Оперативные данные

        private Dictionary<string, double> lastBids = new Dictionary<string, double>();

        /// <summary>
        /// содержит цены close для тикеров, входящих в индекс в виде close#5, eurgbp#1 ...
        /// </summary>
        private Dictionary<string, List<double>> lastBidLists;

        private string[] tickerNames;

        private string ticker;

        private BarSettings timeframe;

        private Random randomGener;

        private CandlePacker packer;

        private List<CandleData> candles;

        private DateTime curTime;

        #endregion
        
        public override BaseRobot MakeCopy()
        {
            var res = new MultiIndexRobot { indexList = new List<IndexDivergencyInfo>() };
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
                    MaxPastExtremum = ind.MaxPastExtremum,                    
                };
                res.indexList.Add(indi);
            }
            res.FractalPeriod = FractalPeriod;
            res.FiboLevel = FiboLevel;
            res.FiboBarsCount = FiboBarsCount;
            res.DeltaLevel = DeltaLevel;
            res.CandlesInIndexHistory = CandlesInIndexHistory;
            res.Volume = Volume;
            res.CountTrades = CountTrades;
            res.UseProtectPositionsFlag = UseProtectPositionsFlag;
            res.DeltaProtect = DeltaProtect;
            res.FractalSourceType = FractalSourceType;
            res.EnterChain = EnterChain;
            res.ForgetDiverCandles = ForgetDiverCandles;
            res.ExtendedMarks = ExtendedMarks;

            CopyBaseSettings(res);
            return res;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext xprotectedContext)
        {
            base.Initialize(grobotContext, xprotectedContext);
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
            timeframe = Graphics[0].b;

            lastBids = new Dictionary<string, double>();

            foreach (var ind in IndexList)
            {
                ind.Initialize();                    
                ind.lastIndicies = new List<double>();
                ind.indexPeaks = new List<Cortege3<decimal, int, decimal>>();
            }
                
            candles = new List<CandleData>();
            packer = new CandlePacker(Graphics[0].b);
            tickerNames = DalSpot.Instance.GetTickerNames();
            randomGener = new Random(DateTime.Now.Millisecond);
            lastBidLists = new Dictionary<string, List<double>>();
            // по каждой валютной паре найти макс. количество отсчетов (из формулы индекса)
            InitLastBidLists();
        }

        private void InitLastBidLists()
        {
            foreach (var ind in IndexList)
            {
                foreach (var varName in ind.indexCalculator.formulaVariableNames)
                {
                    //var queueLen = 1;
                    var tickerPart = varName.ToUpper();
                    if (varName.Contains('#'))
                    {
                        var indexSharp = varName.IndexOf('#');
                        if (indexSharp == 0 || indexSharp == (varName.Length - 1)) continue;
                        var numPart = varName.Substring(indexSharp + 1).ToIntSafe();
                        if (!numPart.HasValue) continue;
                        //queueLen = numPart.Value + 1;
                        tickerPart = varName.Substring(0, indexSharp).ToUpper();
                    }

                    if (tickerPart == "CLOSE" || tickerPart == "OPEN" ||
                        tickerPart == "HIGH" || tickerPart == "LOW")
                        tickerPart = ticker;
                    if (!tickerNames.Contains(tickerPart)) continue;

                    if (lastBidLists.ContainsKey(tickerPart))
                        /*if (lastBidLists[tickerPart].MaxQueueLength >= queueLen) */continue;
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
            var historyIndexStart = Graphics[0].b.GetDistanceTime(FiboBarsCount + 1, -1, startTrade);
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

            // нет торгуемой котировки?
            if (curQuote == null) return null; 

            // обновить свечки
            var candle = packer.UpdateCandle(curQuote);

            if (candle == null) return null;
            
            // закрылась полная свеча, проводим вычисления
            candles.Add(candle);
                
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
                ind.CalculateValue(tickerNames, candles, lastBidLists, /*curTime*/candle.timeOpen, randomGener);
                
            // если это период "разгона" конвейера
            if (isHistoryStartOff) return null;

            // если "наращиваем" вход - ищем последнюю сделку и входим в ее направлении,
            // если закрытие "лучше"
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            var hints = new List<string>();
            // следует "нарастить" объем
            var shouldEnterByChain = false;
            var lastOrderSide = 0;

            if (EnterChain == EnterChainCondition.НаЗакрытииЛучше 
                && orders.Count > 0 && orders.Count < CountTrades)
            {
                var lastOrder = orders[orders.IndexOfMin(o => -o.TimeEnter.Ticks)];
                lastOrderSide = lastOrder.Side;
                shouldEnterByChain = lastOrderSide > 0
                                      ? (candle.close < lastOrder.PriceEnter)
                                      : (candle.close > lastOrder.PriceEnter);
            }
            
            // получить торговый сигнал
            string comment, expertComment;
            var dealSign = GetSignalSide(hints, curQuote.GetCloseQuote(), out comment, out expertComment);
            
            // сигнала нет
            if (dealSign == 0)
            {
                // ... но есть условие - "нарастить объем"
                if (shouldEnterByChain)
                    MakeOrder("", "повторный вход", lastOrderSide, curQuote.GetCloseQuote(), hints);
                return hints;
            }
            
            // от одной проекции не может быть открыто более 1 ордера
            // описание проекции сохраняем в спец. коменте
            // закрыть противонаправленные сделки
            var hasSameOrder = false;
            var hasSameDealType = false;
            foreach (var oldOrder in orders)
            {
                if (oldOrder.Side == dealSign)
                    hasSameDealType = true;
                if (oldOrder.Side == dealSign && 
                    oldOrder.ExpertComment == expertComment)
                {
                    hasSameOrder = true;
                    continue;
                }

                // закрыть ордер другого знака
                if (oldOrder.Side == dealSign) continue;
                var rst = CloseMarketOrder(oldOrder.ID);
                if (rst != RequestStatus.OK)
                    hints.Add("Невозможно закрыть противоположный ордер " + oldOrder.ID);
            }

            if (EnterChain == EnterChainCondition.НаНовойПроекции)
            {
                if (hasSameOrder) return hints;
                MakeOrder(expertComment, comment, dealSign, curQuote.GetCloseQuote(), hints);
                return hints;
            }

            if (EnterChain == EnterChainCondition.НаЗакрытииЛучше)
            {
                if (shouldEnterByChain || !hasSameDealType)
                    MakeOrder(expertComment, comment, dealSign, curQuote.GetCloseQuote(), hints);
                return hints;
            }

            return hints;
        }

        private int GetSignalSide(List<string> hints, QuoteData curQuote, 
            out string comment, out string expertComment)
        {
            comment = string.Empty;
            expertComment = string.Empty;

            // есть ли "пробитая" проекция?
            string projString;
            var projSign = GetLastProjectionSide(out projString);
            if (projSign == 0) return 0;

            // есть ли дивер?
            var diverSpan = GetDivergence();
            var lastSign = diverSpan == null ? 0 : diverSpan.sign;

            if (lastSign == 0) return 0;

            // знаки не совпали - входа нет
            if (lastSign != projSign)
            {
                if (ExtendedMarks)
                    hints.Add(new RobotHint(ticker, BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe),
                         string.Format("Знаки расширения {0} и дивергенции [{1}:{2}] не совпали",
                            projString, diverSpan.sign > 0 ? "бычья" : "медвежья",
                            candles[diverSpan.end].timeClose.ToStringUniform()), "Нет входа", "", curQuote.bid
                        )
                    {
                        Time = curQuote.time,
                        ColorFill = Color.LightGreen,
                        ColorLine = Color.Black,
                        RobotHintType = lastSign > 0 ? RobotHint.HintType.Покупка
                            : RobotHint.HintType.Продажа
                    }.ToString());
                return 0;
            }

            if (diverSpan != null)
            {
                expertComment = projString;
                comment = string.Format("Ф: {0}, дивер: [{1}:{2}]",
                                        projString,
                                        diverSpan.sign > 0 ? "бычья" : "медвежья",
                                        candles[diverSpan.end].timeOpen.ToStringUniform());
            }

            // вернуть знак предполагаемого входа в рынок
            return lastSign;
        }

        private void MakeOrder(string expertComment, string comment, int dealSign, 
            QuoteData curQuote, List<string> hints)
        {
            var tradeVolume = GetEnterVolume();
            if (tradeVolume == 0)
            {
                hints.Add(comment + ", объем входа 0");
                return;
            }

            var order = new MarketOrder
                {
                    ExpertComment = expertComment, // описание проекции
                    Comment = comment,
                    Symbol = ticker,
                    Volume = tradeVolume,
                    Side = dealSign,
                    //StopLoss = 
                };

            // запрос входа на сервере
            var status = NewOrder(order, OrderType.Market,
                                  (decimal) (dealSign > 0 ? curQuote.ask : curQuote.bid), 0);
            if (status != RequestStatus.OK)
                hints.Add(comment + ", вход неуспешен: " + status);
        }

        private DiverSpan GetDivergence()
        {
            var allDivers = new List<DiverSpan>();
            foreach (var ind in IndexList)
            {
                var indexDiv = ind;
                var divers = ind.DiverType == IndexDivergencyInfo.DivergenceType.Классические
                                 ? Divergency.FindDivergencePointsClassic(candles.Count,
                                                                          ind.PeriodExtremum, ind.MaxPastExtremum,
                                                                          index => candles[index].close,
                                                                          index => indexDiv.lastIndicies[index], ind.WaitOneBar)
                                 : Divergency.FindDivergencePointsQuasi(candles.Count,
                                                                        (double) ind.IndexMarginUp, (double) ind.IndexMarginDn,
                                                                        index => candles[index].close,
                                                                        index => indexDiv.lastIndicies[index]);
                allDivers.AddRange(divers);
            }
            // нас интересует знак последнего дивера
            var diverSpan = allDivers.Count == 0
                                ? null
                                : allDivers[allDivers.IndexOfMin(d => -d.end)];
            if (diverSpan != null && candles.Count - diverSpan.end > ForgetDiverCandles)
                diverSpan = null; // дивер устарел            
            return diverSpan;
        }

        private int GetEnterVolume()
        {
            return Volume;
        }

        /// <summary>
        /// построить "фракталы",
        /// от них построить проекции Фибоначчи,
        /// определить, есть ли проекция, цена которой пробита свечкой
        /// если есть - вернуть знак самой последней (актуальной) проекции
        /// </summary>
        /// <returns>-1 если есть проекция, пробитая снизу-вверх, 1 - ... сверху-вниз</returns>
        private int GetLastProjectionSide(out string projString)
        {
            projString = string.Empty;
            if (candles.Count < (FractalPeriod + 2)) return 0;

            // x - индекс свечи, y - знак
            var fractals = new List<Point>(); 

            for (var i = FractalPeriod; i < (candles.Count - FractalPeriod); i++)
            {
                bool isMin, isMax;
                GetCandleFractalSigns(i, out isMin, out isMax);
                if (isMax) fractals.Add(new Point(i, 1));
                if (isMin) fractals.Add(new Point(i, -1));
            }

            // от пар фракталов min - max строить проекции
            var lastSign = 0;
            var lastFractal = candles.Count - candlesInIndexHistory;
            for (var i = 1; i < fractals.Count; i++)
            {
                if (fractals[i].X < lastFractal) continue;
                if (fractals[i - 1].Y == fractals[i].Y) continue;

                var priceA = candles[fractals[i - 1].X].close;
                var priceB = candles[fractals[i].X].close;
                if (FractalSourceType == ZigZagSource.HighLow)
                {
                    priceA = fractals[i - 1].Y > 0
                                 ? candles[fractals[i - 1].X].high
                                 : candles[fractals[i - 1].X].low;
                    priceB = fractals[i].Y > 0
                                 ? candles[fractals[i].X].high
                                 : candles[fractals[i].X].low;
                }
                var priceF = priceA + (priceA - priceB) * fiboLevel;
                // уровень priceF преодолён?
                for (var j = fractals[i].X + 1; j < candles.Count; j++)
                {
                    if ((fractals[i].Y < 0 && candles[j].high >= priceF) ||
                        (fractals[i].Y > 0 && candles[j].low <= priceF))
                    {
                        lastSign = -fractals[i].Y;
                        projString = string.Format("[{0}:{1}, {2}:{3} - {4}]",
                            fractals[i - 1].Y < 0 ? "min" : "max",
                            candles[fractals[i - 1].X].timeOpen.ToString("dd.MM HH") + "ч",
                            fractals[i].Y < 0 ? "min" : "max",
                            candles[fractals[i].X].timeOpen.ToString("dd.MM HH") + "ч",
                            priceF.ToStringUniformPriceFormat());
                    }
                }
            }

            return -lastSign;
        }

        /// <summary>
        /// есть ли "фрактал" на свечке?
        /// </summary>
        private void GetCandleFractalSigns(int candleIndex, out bool isMin, out bool isMax)
        {
            isMin = true;
            isMax = true;
            for (var j = candleIndex - FractalPeriod; j <= candleIndex + FractalPeriod; j++)
            {
                if (j == candleIndex) continue;
                if (FractalSourceType == ZigZagSource.HighLow)
                {
                    if (candles[j].high >= candles[candleIndex].high)
                        isMax = false;
                    if (candles[j].low <= candles[candleIndex].low)
                        isMin = false;
                }
                else
                {
                    if (candles[j].close >= candles[candleIndex].close)
                        isMax = false;
                    if (candles[j].close <= candles[candleIndex].close)
                        isMin = false;
                }
            }
        }
    }
    // ReSharper restore LocalizableElement
}