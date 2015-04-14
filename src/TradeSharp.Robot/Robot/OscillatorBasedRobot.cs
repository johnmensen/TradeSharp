using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("ТС дивергенций курса/индекса")]
    [TypeConverter(typeof(PropertySorter))]
    class OscillatorBasedRobot : BaseRobot
    {
        private CandlePacker packer;

        private RestrictedQueue<CandleData> candles;

        private float? lastIndexValue;

        private DateTime curTime;

        private RestrictedQueue<float> lastIndicies;

        /// <summary>
        /// массив "пиков" индикатора: индекс, знак (1 - перекупленность), цена
        /// </summary>
        private List<Cortege3<float, int, float>> indexPeaks;

        private Dictionary<string, float> lastBids = new Dictionary<string, float>();

        /// <summary>
        /// содержит цены close для тикеров, входящих в индекс в виде close#5, eurgbp#1 ...
        /// </summary>
        private Dictionary<string, RestrictedQueue<float>> lastBidLists;
        
        private ExpressionResolver formulaResolver;

        private List<string> formulaVariableNames;

        private string[] tickerNames;

        private Random randomGener;

        private int countCandles;

        private string indexFormula = "close-(close+close#1+close#2+close#3+close#4+close#5+close#6+close#7+close#8+close#9+close#10+close#11)/12"; //"100/usdx-100/usdx#12";

        private string ticker;

        [PropertyXMLTag("Robot.IndexFormula")]
        [DisplayName("Валютный индекс")]
        [Category("Основные")]
        [Description("Валютный индекс, с которым ищутся дивергенции")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormulaL
        {
            get { return indexFormula; }
            set { indexFormula = value; }
        }

        private float indexMarginUp = 0.03f;
        [PropertyXMLTag("Robot.IndexMarginUp")]
        [DisplayName("Верх зоны п/п")]
        [Category("Основные")]
        [Description("Верхняя граница - зона условной перекупленности")]
        public float IndexMarginUp
        {
            get { return indexMarginUp; }
            set { indexMarginUp = value; }
        }

        private float indexMarginDn = -0.03f;
        [PropertyXMLTag("Robot.IndexMarginDn")]
        [DisplayName("Низ зоны п/п")]
        [Category("Основные")]
        [Description("Нижняя граница - зона условной перепроданности")]
        public float IndexMarginDn
        {
            get { return indexMarginDn; }
            set { indexMarginDn = value; }
        }

        private int candlesInIndexHistory = 50;
        [PropertyXMLTag("Robot.CandlesInIndexHistory")]
        [DisplayName("Длина истории индексов")]
        [Category("Основные")]
        [Description("Длина истории индексов, свечей")]
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
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        private int stopLossPoints = 500;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
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
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }

        public override BaseRobot MakeCopy()
        {
            var robo = new OscillatorBasedRobot
                           {
                               indexFormula = indexFormula,
                               IndexMarginUp = IndexMarginUp,
                               IndexMarginDn = IndexMarginDn,
                               CandlesInIndexHistory = CandlesInIndexHistory,
                               Volume = Volume,
                               StopLossPoints = StopLossPoints,
                               TakeProfitPoints = TakeProfitPoints
                           };
            CopyBaseSettings(robo);
            return robo;
        }
        
        public override void Initialize(BacktestServerProxy.RobotContext grobotContext)
        {
            base.Initialize(grobotContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("OscillatorBasedRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("OscillatorBasedRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;

            try
            {
                formulaResolver = new ExpressionResolver(indexFormula);
                formulaVariableNames = formulaResolver.GetVariableNames();
                lastBids = new Dictionary<string, float>();
                lastIndicies = new RestrictedQueue<float>(CandlesInIndexHistory);
                candles = new RestrictedQueue<CandleData>(CandlesInIndexHistory);
                packer = new CandlePacker(Graphics[0].b);
                indexPeaks = new List<Cortege3<float, int, float>>();
                tickerNames = DalSpot.Instance.GetTickerNames();
                randomGener = new Random(DateTime.Now.Millisecond);
                lastBidLists = new Dictionary<string, RestrictedQueue<float>>();
                // по каждой валютной паре найти макс. количество отсчетов (из формулы индекса)
                InitLastBidLists();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("OscillatorBasedRobot: ошибка парсинга выражения \"{0}\": {1}",
                                   indexFormula, ex);
                formulaResolver = null;
            }            
        }

        private void InitLastBidLists()
        {
            foreach (var varName in formulaVariableNames)
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
                    if (lastBidLists[tickerPart].MaxQueueLength >= queueLen) continue;
                var queue = new RestrictedQueue<float>(queueLen);
                if (lastBidLists.ContainsKey(tickerPart))
                    lastBidLists[tickerPart] = queue;
                else lastBidLists.Add(tickerPart, queue);

                if (tickerPart == ticker && candles.MaxQueueLength < queueLen)
                    candles = new RestrictedQueue<CandleData>(queueLen);
            }
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            var historyIndexStart = Graphics[0].b.GetDistanceTime(CandlesInIndexHistory + 1, -1, startTrade);
            // получить котиры из индексов
            var ret = base.GetRequiredSymbolStartupQuotes(startTrade);
            if (!string.IsNullOrEmpty(indexFormula))
            {
                if (string.IsNullOrEmpty(indexFormula)) return ret;
                var lowerFormula = indexFormula.ToLower();
                var formulaTickers = tickerNames.Where(tn => lowerFormula.Contains(tn.ToLower())).ToList();
                // также формула может содержать записи вида open, high, low, close
                // заменить их на основной торгуемый символ
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

        public override List<string> OnQuotesReceived(string[] names, QuoteData[] quotes, bool isHistoryStartOff)
        {
            if (formulaResolver == null || packer == null) return null;            
            curTime = quotes[0].time;

            // обновить табличку цен
            for (var i = 0; i < names.Length; i++)
            {
                if (lastBids.ContainsKey(names[i]))
                    lastBids[names[i]] = quotes[i].bid;
                else
                    lastBids.Add(names[i], quotes[i].bid);
            }

            QuoteData curQuote = null;
            for (var i = 0; i < names.Length; i++)            
                if (names[i] == ticker)
                {
                    curQuote = quotes[i];
                    break;
                }
            
            if (curQuote == null) return null; // нет торгуемой котировки

            // обновить свечки
            var candle = packer.UpdateCandle(curQuote.bid, curQuote.time);
            Dictionary<string, double> varValues = null;
            if (candle != null)
            {
                candles.Add(candle);
                countCandles++;
                // обновить очереди (для индекса, переменные вида usdjpy#15)
                if (lastBidLists.Count > 0)
                {
                    foreach (var listTicker in lastBidLists)
                    {
                        float price;
                        if (!lastBids.TryGetValue(listTicker.Key, out price)) price = 0;
                        listTicker.Value.Add(price);
                    }
                }
                // посчитать индекс
                double index;
                varValues = GetVariableValues();
                if (formulaResolver.Calculate(varValues, out index))
                    lastIndexValue = double.IsNaN(index) ? 0 : (float)index;
                lastIndicies.Add(lastIndexValue ?? 0);
            }            
            
            // если это период "разгона" конвейера
            if (isHistoryStartOff) return null;

            RobotHint hint = null;

            // получить знак дивергенции
            if (candle != null)
            {
                string commentOnDivergence = string.Empty;
                var divergenceSign = GetDivergenceSign(out commentOnDivergence);
                List<MarketOrder> orders;
                robotContext.GetMarketOrders(robotContext.accountInfo.ID, out orders);
                if (divergenceSign != 0)
                {
                    var hintText = new StringBuilder();
                    hintText.AppendLine(commentOnDivergence);
                    hintText.AppendLine("Переменные:");
                    // ReSharper disable PossibleNullReferenceException)
                    foreach (var pair in varValues)
                        // ReSharper restore PossibleNullReferenceException
                    {
                        hintText.AppendLine(string.Format("{1}{0}{2:f4}", (char) 9, pair.Key, pair.Value));
                    }

                    hint = new RobotHint(Graphics[0].a, Graphics[0].b.ToString(), 
                        hintText.ToString(), divergenceSign > 0 ? "BUY" : "SELL", "e", curQuote.bid)
                               {
                                   Time = candle.timeOpen,
                                   //curTime
                                   ColorFill = divergenceSign > 0 ? Color.Green : Color.Red,
                                   ColorLine = Color.Black,
                                   RobotHintType = divergenceSign > 0
                                                       ? RobotHint.HintType.Покупка
                                                       : RobotHint.HintType.Продажа
                               };

                    // если получен сигнал на покупку - купить, закрыв продажи
                    // наоборот, если получен сигнал на продажу - продать, закрыв покупки
                    var ordersToClose = orders.FindAll(o => o.Side != divergenceSign);
                    foreach (var order in ordersToClose)
                    {
                        robotContext.SendCloseRequest(CurrentProtectedContext.Instance.MakeProtectedContext(),
                            robotContext.accountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                    }

                    // открыть позу в направлении знака дивера
                    decimal? stop = StopLossPoints == 0
                                        ? (decimal?) null
                                        : (decimal)curQuote.bid -
                                          divergenceSign*DalSpot.Instance.GetAbsValue(ticker, (decimal)StopLossPoints);
                    decimal? take = TakeProfitPoints == 0
                                        ? (decimal?) null
                                        : (decimal)curQuote.bid +
                                          divergenceSign*DalSpot.Instance.GetAbsValue(ticker, (decimal)TakeProfitPoints);

                    robotContext.SendNewOrderRequest(CurrentProtectedContext.Instance.MakeProtectedContext(),
                                                     RequestUniqueId.Next(), robotContext.accountInfo.ID, Magic, ticker, Volume,
                                                     divergenceSign, OrderType.Market, 0, 0,
                                                     stop, take, null, null, string.Empty, "OscillatorBasedRobot");
                }
            }

            return hint != null ? new List<string> {hint.ToString()} : null;
        }        

        /// <summary>
        /// располагая очередями candles и lastIndicies
        /// определить наличие бычьей (1) или медвежьей дивергенции (-1)
        /// в противном случае - 0
        /// </summary>        
        private int GetDivergenceSign(out string comment)
        {
            comment = string.Empty;
            if (candles.Length < 2) return 0;
                    
            var indexNow = lastIndicies.Last;
            var priceNow = candles.Last.close;

            var isAboveMarg = indexNow > IndexMarginUp;
            var isBelowMarg = indexNow < IndexMarginDn;

            // обновить список indexPeaks
            if (!isAboveMarg) indexPeaks = indexPeaks.Where(p => p.b < 0).ToList();
            if (!isBelowMarg) indexPeaks = indexPeaks.Where(p => p.b > 0).ToList();
            if (!isAboveMarg && !isBelowMarg) return 0;
            // добавить пик?
            var newPeak = false;
            if (isAboveMarg)            
                if (indexPeaks.Count == 0 || indexNow > indexPeaks[indexPeaks.Count - 1].a)
                {
                    indexPeaks.Add(new Cortege3<float, int, float>(indexNow, 1, priceNow));
                    newPeak = true;
                }
            if (isBelowMarg)            
                if (indexPeaks.Count == 0 || indexNow < indexPeaks[indexPeaks.Count - 1].a)
                {
                    indexPeaks.Add(new Cortege3<float, int, float>(indexNow, -1, priceNow));
                    newPeak = true;
                }

            if (newPeak || indexPeaks.Count == 0) return 0;
            // сравнить разницу
            var peakSign = indexPeaks[indexPeaks.Count - 1].b;
            var deltaIndex = indexNow - indexPeaks[indexPeaks.Count - 1].a;
            var deltaPrice = priceNow - indexPeaks[indexPeaks.Count - 1].c;

            var divSign = peakSign < 0 ? 1 : -1;
            
            if ((peakSign < 0 && deltaIndex > 0 && deltaPrice < 0) ||
               (peakSign > 0 && deltaIndex < 0 && deltaPrice > 0))
            {
                comment = string.Format("Текущая цена {1:f4}, пред. цена {2:f4}. Текущий индекс {3:f4}, пред. индекс {4:f4}{0}" + 
                    "Зона: {5}. Знак послед. экстремума: {6}",
                    Environment.NewLine, priceNow, indexPeaks[indexPeaks.Count - 1].c,
                    indexNow, indexPeaks[indexPeaks.Count - 1].a,
                    isAboveMarg ? "перекупленность" : "перепроданность", peakSign);
                return divSign;
            }
            return 0;
        }

        /// <summary>
        /// подготовить словарь Имя-Значение для 
        /// </summary>        
        private Dictionary<string, double> GetVariableValues()
        {
            var varValues = new Dictionary<string, double>();
            if (formulaVariableNames == null) return new Dictionary<string, double>();
            // специальные имена переменных:
            // open, low, high, close - текущие OHLC-уровни для свечной серии
            // для серий типа LineSeries доступен только close
            // close#2 - цена два бара назад
            // i - индекс текущей свечи
            // count - количество данных в основном ряду
            // year, month, day, weekday, hour, minute, second
            // eurusd, audjpy#100 - close соотв. валютной пары
            // random - равномерно распред. СЧ на интервале 0..1

            // переменные с неизвестными именами инициализируются нулями
            foreach (var name in formulaVariableNames)
            {
                var variableName = name;
                if (name == "i")
                {
                    varValues.Add(name, countCandles);
                    continue;
                }
                if (name == "count")
                {
                    varValues.Add(name, countCandles);
                    continue;
                }

                var tickerName = tickerNames.FirstOrDefault(tn =>
                    variableName.StartsWith(tn, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(tickerName))
                {// котировка
                    var price = 0.0;
                    if (lastBidLists.ContainsKey(tickerName))
                    {
                        var tickerQueue = lastBidLists[tickerName];
                        var datIndex = GetDeltaIndexFromSharpSeparatedString(variableName);
                        price = datIndex >= tickerQueue.Length ? 0 : tickerQueue.GetItemByIndex(datIndex, false);                        
                    }
                    varValues.Add(name, price);
                    continue;
                }
                if (name == "random")
                {
                    varValues.Add(name, randomGener.NextDouble());
                    continue;
                }
                if (name == "year" || name == "month" || name == "day" || name == "weekday" || name == "hour"
                    || name == "minute" || name == "second")
                {
                    // получить дату текущей точки графика                    
                    if (name == "year") varValues.Add(name, curTime.Year);
                    else if (name == "month") varValues.Add(name, curTime.Month);
                    else if (name == "day") varValues.Add(name, curTime.Day);
                    else if (name == "weekday") varValues.Add(name, (int)curTime.DayOfWeek);
                    else if (name == "hour") varValues.Add(name, curTime.Hour);
                    else if (name == "minute") varValues.Add(name, curTime.Minute);
                    else if (name == "second") varValues.Add(name, curTime.Second);

                    continue;
                }
                if (name.StartsWith("close") || name.StartsWith("open") || name.StartsWith("low") || name.StartsWith("high"))
                {// "close", "close#17" ...
                    var datIndex = GetDeltaIndexFromSharpSeparatedString(name);
                    if (datIndex >= candles.Length)
                    {
                        varValues.Add(name, 0);
                        continue;
                    }
                    var candle = candles.GetItemByIndex(datIndex, false);
                    if (name.StartsWith("open")) varValues.Add(name, (double)candle.open);
                    else if (name.StartsWith("high")) varValues.Add(name, (double)candle.high);
                    else if (name.StartsWith("low")) varValues.Add(name, (double)candle.low);
                    else if (name.StartsWith("close")) varValues.Add(name, (double)candle.close);                    
                    continue;
                }
                // переменная не определена вообще - инициализируем 0-м
                varValues.Add(name, 0);
            }
            return varValues;
        }

        /// <summary>
        /// пример: для EURUSD#15 вернуть 15 (без знака!)
        /// </summary>        
        private static int GetDeltaIndexFromSharpSeparatedString(string varName)
        {
            var indexSharp = varName.IndexOf('#');
            if (indexSharp < 0) return 0;
            var partIndex = varName.Substring(indexSharp + 1);
            var deltaIndex = partIndex.ToIntSafe();
            return deltaIndex ?? 0;
        }
    }
}
