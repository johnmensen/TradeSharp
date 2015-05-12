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
    /// <summary>
    /// название из DisplayName отображается в списке роботов
    /// 
    /// характеристики робота:
    /// 1) интенсивность торговли - при дефолтовых настройках на EURUSD:H1 порядка 190 сделок в год
    /// 
    /// варьируемые параметры:
    /// 1) SigmaCoeffs: [3] .. [15], [4, 6, 10, 14] ..
    /// 2) EnterDirection: ПротивТренда, ПоТренду
    /// второстепенные
    /// 3) CloseOpposite: true, false
    /// 4) StopLoss, TakeProfit: 15 ... 800
    /// 5) CandlesToDetermineAverage: 20 ... 200
    /// </summary>
    [DisplayName("Порванный шаблон")]
    public class TornPatternRobot : BaseRobot
    {
        #region Настройки
        
        private decimal[] sigmaCoeffs = { 4M, 7M };

        /// <summary>
        /// [PropertyXMLTag] - без него параметр не сохранится в файле
        /// [DisplayName, Category, Description] - нужны все
        /// [Category] - типовые категории настроек - Торговые, Money Management, Визуальные, Дополнительно...
        /// не забыть скопировать параметр в MakeCopy()!
        /// 
        /// Конкретно этот параметр означает - пример:
        /// 4, 7, 10
        /// одна свеча должна быть в 4 раза больше средней свечи (измеряется |C - O|), чтобы войти в рынок
        /// две свечи в сумме (|C[1] - O[0]|) должны быть в 7 раз больше длины средней свечи
        /// ... три свечи - в 10 раз больше
        /// 
        /// Параметр типа "массив чисел" удобно представить строкой - так он и сохраняется в файл,
        /// и редактируется без проблем
        /// </summary>
        [PropertyXMLTag("SigmaCoeffs")]
        [DisplayName("Шаблон - множитель")]
        [Category("Торговые")]
        [Description("Первое значение - множитель сигма для шаблона из одной свечи...")]
        public string SigmaCoeffs
        {
            get { return string.Join(", ", sigmaCoeffs.Select(c => c.ToStringUniform())); }
            set
            {
                var array = value.ToDecimalArrayUniform();
                if (array.Length > 0 && array.All(c => c >= 0))
                    sigmaCoeffs = array;
            }
        }

        [PropertyXMLTag("EnterDirection")]
        [DisplayName("Вход")]
        [Category("Торговые")]
        [Description("В направлении шаблона или против шаблона")]
        public EnterDirection EnterDirection { get; set; }

        private bool closeOpposite = true;
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

        private int candlesToDetermineAverage = 80;
        [PropertyXMLTag("CandlesToDetermineAverage")]
        [DisplayName("Свеч для среднего")]
        [Category("Торговые")] 
        [Description("Свечек для определения среднего значения")]
        public int CandlesToDetermineAverage
        {
            get { return candlesToDetermineAverage; }
            set { candlesToDetermineAverage = value; }
        }
        #endregion

        #region Переменные
        /// <summary>
        /// рантайм-переменные (не настроечные параметры робота)
        /// не копируются в MakeCopy()
        /// 
        /// packers - словарь: валютная пара - "упаковщик" свечей
        /// "упаковщику" скармливаются котировки (в главном методе OnQuotesReceived), на выходе выдает свечи
        /// </summary>
        private Dictionary<string, CandlePacker> packers;

        /// <summary>
        /// словарь: валютная пара / цена пункта
        /// например: EURUSD: 0.0001, USDJPY: 0.01
        /// в настройках удобно задавать SL, TP в пунктах
        /// но в серверных методах нужны абсолютные величины... чтобы не считать каждый раз,
        /// не вызывать DalSpot.Instance.GetAbsValue() - завел словарь
        /// </summary>
        private Dictionary<string, decimal> pointCost;
        /// <summary>
        /// просто списки по каждой валютной паре, куда складываются сформированные по ним свечи
        /// (при дефолтовых параметрах хранится по 2 свечки)
        /// </summary>
        private Dictionary<string, List<CandleData>> patternCandles;
        /// <summary>
        /// последние свечки по каждой валютной паре, для расчета длины тела средней свечи
        /// (хранится 80 штук при настройках по-умолчанию)
        /// </summary>
        private Dictionary<string, RestrictedQueue<CandleData>> storedCandles;
        /// <summary>
        /// главный метод OnQuotesReceived() выдает на выходе список строк
        /// там могут быть простые сообщения, а могут быть особым образом форматированные строки:
        /// они превратятся в отметки на графике (на графиках)
        /// </summary>
        private List<string> events;
        #endregion

        /// <summary>
        /// надо вернуть экземпляр того же класса, не забыть скопировать все настроечные параметры
        /// run-time переменные робота копировать не нужно
        /// не забыть вызвать CopyBaseSettings()
        /// </summary>
        /// <returns></returns>
        public override BaseRobot MakeCopy()
        {
            var bot = new TornPatternRobot
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                SigmaCoeffs = SigmaCoeffs,
                EnterDirection = EnterDirection,
                CloseOpposite = CloseOpposite,
                CandlesToDetermineAverage = CandlesToDetermineAverage,
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

        /// <summary>
        /// переопределяется почти в каждом роботе
        /// </summary>
        public override void Initialize(BacktestServerProxy.RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            // обязательная строка
            base.Initialize(robotContext, protectedContext);
            // для робота не заданы торгуемый инструмент (иинструменты) / таймфрейм (таймфреймы)
            if (Graphics.Count == 0) return;
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            pointCost = Graphics.ToDictionary(g => g.a, g => DalSpot.Instance.GetAbsValue(g.a, 1M));
            patternCandles = Graphics.ToDictionary(g => g.a, g => new List<CandleData>());
            storedCandles = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<CandleData>(CandlesToDetermineAverage));
        }

        /// <summary>
        /// робот объявляет, по каким валютным парам ему нужны котировки и за 
        /// какой период (с какой даты / времени ему нужны эти котировки)
        /// в данной реализации - беру все валютные пары, для каждой беру
        /// среднюю длительность интервала свечи и умножаю на количество интервалов
        /// в шаблоне...
        /// </summary>
        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return Graphics.ToDictionary(g => g.a, g => startTrade.AddMinutes(g.b.Intervals.Average() * (sigmaCoeffs.Length + 1)));
        }

        /// <summary>
        /// главная функция робота
        /// на выходе - список текстовых комментариев (см выше)
        /// </summary>
        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = new List<string>();
            if (packers == null) return events;
            
            // цикл по всем котировкам (одна валютная пара - одна котировка)
            for (var i = 0; i < quotes.Length; i++)
            {
                CandlePacker packer;
                if (!packers.TryGetValue(names[i], out packer))
                    continue; // эта котировка роботу не интересна, он ее не обрабатывает

                var candle = packer.UpdateCandle(quotes[i]);
                if (candle == null) 
                    continue; // свеча просто обновилась, новая свеча не сформирована

                // тут принимается решение - входить в рынок?
                // 0 - не входить, 1 - покупка, -1 - продажа
                var enterSign = CheckEnterCondition(names[i], candle);

                // но реальный вход в рынок возможен только при !isHistoryStartOff
                // важный флаг: если он true, робот "разогревается" на истории котировок и не должен пытаться торговать
                if (isHistoryStartOff) continue;
                
                if (enterSign == 0) continue;
                var dealSign = EnterDirection == EnterDirection.ПоТренду ? enterSign : -enterSign;

                // если есть открытые сделки против текущего направления - закрыть их
                if (CloseOpposite)
                    CloseCounterOrders(dealSign, names[i]);

                // открыть сделку
                OpenOrder(dealSign, names[i], quotes[i]);
            }

            return events;
        }

        /// <summary>
        /// тут проверяются условия для принятия решения: входить или не входить в рынок?
        /// </summary>
        private int CheckEnterCondition(string symbol, CandleData candle)
        {
            // могут быть пустые свечи - например, на выходных
            if (candle.open == candle.close) return 0;

            var candlesList = patternCandles[symbol];
            candlesList.Add(candle);
            // если нас интересуют шаблоны из 2-х свечей максимум - нет смысла хранить больше
            if (candlesList.Count > sigmaCoeffs.Length)
                candlesList.RemoveAt(0);
            var storedList = storedCandles[symbol];
            storedList.Add(candle);
            if (storedList.Length < CandlesToDetermineAverage) return 0;
            // длина средней свечи
            var avgLen = storedList.Average(c => Math.Abs(c.close - c.open));
            
            // если последние N свечек из candlesList однонаправлены
            // и их суммарная длина в N раз больше длины средней свечи - войти в рынок
            var close = candlesList[candlesList.Count - 1].close;
            var sign = Math.Sign(candlesList[candlesList.Count - 1].close -
                                 candlesList[candlesList.Count - 1].open);
            for (var i = candlesList.Count - 1; i >= 0; i--)
            {
                var c = candlesList[i];
                var candleSign = Math.Sign(c.close - c.open);
                if (candleSign != sign) return 0;
                var patternLength = Math.Abs(c.open - close);
                var coeff = sigmaCoeffs[candlesList.Count - i - 1];
                var patternMargin = (float)coeff * avgLen;
                if (patternLength < patternMargin)
                    continue;
                // суммарная длина шаблона больше или равна N * среднюю длину свечи
                // вернуть 1 для растущего шаблона, -1 для падающего
                return sign;
            }
            return 0;
        }

        /// <summary>
        /// закрыть ордера, направление которых не совпадает с предполагаемым
        /// направлением входа в рынок
        /// </summary>
        private void CloseCounterOrders(int dealSign, string symbol)
        {
            // базовый метод - получает все открытые ордера по счету,
            // если стоит флаг - выбирает только те, Magic у которых тот же, что и у робота
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            if (orders.Count == 0) return;
            var ordersToClose = orders.Where(o => o.Symbol == symbol && o.Side != dealSign);
            // отправить на сервер команду - закрыть ордер
            foreach (var order in ordersToClose)
                CloseMarketOrder(order.ID, PositionExitReason.ClosedByRobot);
        }

        private void OpenOrder(int dealSign, string symbol, CandleDataBidAsk lastCandle)
        {
            // функция считает и округляет объем входа
            var volume = CalculateVolume(symbol, base.Leverage);
            if (volume == 0)
            {
                events.Add(string.Format("{0} {1} отменена - объем равен 0",
                    dealSign > 0 ? "покупка" : "продажа", symbol));
                return;
            }

            var enterPrice = dealSign > 0 ? lastCandle.closeAsk : lastCandle.close;
            var point = pointCost[symbol];
            var stopLoss = enterPrice - dealSign * (float)point * StopLossPoints;
            var takeProfit = enterPrice + dealSign * (float)point * TakeProfitPoints;
            var order = new MarketOrder
            {
                Symbol = symbol,                            // Инструмент по которому совершается сделка
                Volume = volume,                            // Объём средств, на который совершается сделка
                Side = dealSign,                            // Устанавливаем тип сделки - покупка или продажа
                StopLoss = stopLoss,                        // Устанавливаем величину Stop loss для открываемой сделки
                TakeProfit = takeProfit,                    // Устанавливаем величину Take profit для открываемой сделки
                ExpertComment = "TornAssholeRobot"          // Комментарий по сделке, оставленный роботом
            };
            var status = NewOrder(order,
                OrderType.Market, // исполнение по рыночной цене - можно везде выбирать такой вариант
                0, 0); // последние 2 параметра для OrderType.Market не имеют значения
            if (status != RequestStatus.OK)
                events.Add(string.Format("Ошибка добавления ордера ({0}): {1}",
                    order.ToStringShort(), status));
        }
    }

    public enum EnterDirection
    {
        ПротивТренда = 0, ПоТренду = 1
    }
}
