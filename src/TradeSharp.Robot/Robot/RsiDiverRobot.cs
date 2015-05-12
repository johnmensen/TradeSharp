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
    ///<summary>
    /// характеристики робота:
    /// 1) интенсивность торговли - при дефолтовых настройках на EURUSD:H1 порядка 1 сделки на 1.5 - 2 свечи 
    /// 
    /// варьируемые параметры:
    /// 1) Period: не меньше 1. Если 2, 3 - не эффективно. Обычно 15,  ..
    /// 2) N, M: Левая и Правая граници временного лага. Предполагается, что чем дальше "коридор" N - M от текущей свечи и чем от уже, 
    /// тем меньше интенсивность сделок. 
    /// Но на практике это пока не подтвердилось. 
    /// Примерно одинаковые результаты дают измерения на временном лаге 
    /// N = 10 M = 5  и Period = 15 
    /// N = 20 M = 15 и Period = 25 
    /// N = 20 M = 18 и Period = 25  (сужение интервала на 3 единици результата не дало - сделки открываются оочень часто)
    /// второстепенные
    /// 3) CloseOpposite: true, false
    /// 4) StopLoss, TakeProfit: 15 ... 800
    ///</summary>
    // ReSharper disable LocalizableElement
    [DisplayName("Индекс относительной силы")]
    public class RsiDiverRobot : BaseRobot 
    {
        public class PriceRsi
        {
            public float Price { get; set; }

            public double? Rsi { get; set; }

            public PriceRsi()
            {
            }

            public PriceRsi(float price, double? rsi)
            {
                Price = price;
                Rsi = rsi;
            }
        };

        #region Настройки
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

        private int period = 15;
        [PropertyXMLTag("Robot.Period")]
        [DisplayName("Период расчёта")]
        [Category("Торговые")]
        [Description("Определяет количество свечей, на которых анализируются цены закрытия")]
        public int Period
        {
            get { return period; }
            set
            {
                period = value;

                if (value < N)
                    N = period;
            }
        }

        private int n = 10;
        [PropertyXMLTag("Robot.N")]
        [DisplayName("Левая граница временного лага")]
        [Category("Торговые")]
        [Description("Левая граница временного лага")]
        public int N
        {
            get { return n; }
            set
            {
                // N не может быть больше чем период расчёта (в крайнем случае равен периоду)
                n = (value > period) ? period : value;

                if (value <= M)
                    M = N - 1;
            }
        }

        private int m = 5;
        [PropertyXMLTag("Robot.M")]
        [DisplayName("Правая граница временного лага")]
        [Category("Торговые")]
        [Description("Правая граница временного лага")]
        public int M
        {
            get { return m; }
            set
            {
                // MN не может быть больше чем N (в крайнем случае меньше на единицу, что бы N - M был равен хотя бы 1)
                m = (value >= N) ? N - 1 : value;
            }
        }

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
        #endregion

        #region Переменные
        private Dictionary<string, RestrictedQueue<PriceRsi>> rsiClosePairs;
        private Dictionary<string, CandlePacker> packers;       
        #endregion

        private List<string> events;

        public override BaseRobot MakeCopy()
        {
            var bot = new RsiDiverRobot
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                FixedVolume = FixedVolume,
                Leverage = Leverage,
                RoundType = RoundType,
                NewsChannels = NewsChannels,
                RoundMinVolume = RoundMinVolume,
                RoundVolumeStep = RoundVolumeStep,
                M = M,
                N = N,
                Period = Period,
                CloseOpposite = CloseOpposite
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(RobotContext grobotContext, CurrentProtectedContext gprotectedContext)
        {
            base.Initialize(grobotContext, gprotectedContext);

            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            rsiClosePairs = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<PriceRsi>(period));

            lastMessages = new List<string>();
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0)
                return null;

            try
            {
                return Graphics.ToDictionary(g => g.a, g => g.b.GetDistanceTime(period, -1, startTrade));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = lastMessages.ToList();
            lastMessages.Clear();

            if (packers == null) 
                return events;

            for (var i = 0; i < quotes.Length; i++)
            {
                CandlePacker packer;
                if (!packers.TryGetValue(names[i], out packer))
                    continue;

                var candle = packer.UpdateCandle(quotes[i]);
                if (candle == null)
                    continue;

                var enterSign = CheckEnterCondition(names[i], candle.close);

                if (enterSign == 0)
                    continue;

                if (isHistoryStartOff) continue;

                if (CloseOpposite)
                    CloseCounterOrders(enterSign, names[i]);

                OpenOrder(enterSign, names[i], quotes[i]);
            }

            return events;
        }

        private int CheckEnterCondition(string name, float candleClose)
        {
            var currentPair = new PriceRsi(candleClose, null);
            var currentPairs = rsiClosePairs[name];
            currentPairs.Add(currentPair);
            if (currentPairs.Length < currentPairs.MaxQueueLength) 
                return 0;

            currentPair.Rsi = CalculateRsi(currentPairs);

            for (var i = M; i < N; i++)
            {
                var previousPair = currentPairs.ElementAt(currentPairs.MaxQueueLength - i - 1);
                if (!previousPair.Rsi.HasValue)
                    continue;

                var diffClose = Math.Sign(currentPair.Price - previousPair.Price);
                var diffRsi = Math.Sign(currentPair.Rsi.Value - previousPair.Rsi.Value);

                if (diffClose == diffRsi || diffRsi == 0)
                    continue;

                return diffRsi;
            }

            return 0;
        }

        private static double CalculateRsi(RestrictedQueue<PriceRsi> currentPairs)
        {
            var u = 0f;
            var d = 0f;
            for (var i = 1; i < currentPairs.MaxQueueLength; i++)
            {
                var deltaPrice = currentPairs.ElementAt(i).Price - currentPairs.ElementAt(i - 1).Price;
                if (deltaPrice < 0)
                    d -= deltaPrice;
                else
                    u += deltaPrice;
            }

            var rsi = (u == 0 && d == 0) ? 50 : 100 * u / (u + d);
            return rsi;
        }
        
        private void CloseCounterOrders(int dealSign, string symbol)
        {
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            if (orders.Count == 0) return;
            var ordersToClose = orders.Where(o => o.Symbol == symbol && o.Side != dealSign);
            foreach (var order in ordersToClose)
                CloseMarketOrder(order.ID);
        }

        private void OpenOrder(int dealSign, string symbol, CandleDataBidAsk lastCandle)
        {
            var volume = CalculateVolume(symbol, base.Leverage);
            if (volume == 0)
            {
                events.Add(string.Format("{0} {1} отменена - объем равен 0 (L:{2:f3}, #{3})",
                    dealSign > 0 ? "покупка" : "продажа", symbol, base.Leverage, robotContext.AccountInfo.ID));
                return;
            }

            var enterPrice = dealSign > 0 ? lastCandle.closeAsk : lastCandle.close;
            var stopLoss = enterPrice - dealSign * DalSpot.Instance.GetAbsValue(symbol, (float)StopLossPoints);
            var takeProfit = enterPrice + dealSign * DalSpot.Instance.GetAbsValue(symbol, (float)TakeProfitPoints);

            var order = new MarketOrder
            {
                Symbol = symbol,                            // Инструмент по которому совершается сделка
                Volume = volume,                            // Объём средств, на который совершается сделка
                Side = dealSign,                            // Устанавливаем тип сделки - покупка или продажа
                StopLoss = stopLoss,                        // Устанавливаем величину Stop loss для открываемой сделки
                TakeProfit = takeProfit,                    // Устанавливаем величину Take profit для открываемой сделки
                ExpertComment = "RsiDiverRobot"             // Комментарий по сделке, оставленный роботом
            };
            var status = NewOrder(order,
                OrderType.Market, // исполнение по рыночной цене - можно везде выбирать такой вариант
                0, 0); // последние 2 параметра для OrderType.Market не имеют значения
            if (status != RequestStatus.OK)
                events.Add(string.Format("Ошибка добавления ордера ({0}): {1} (#{2} bal: {3})",
                    order, status,
                    robotContext.AccountInfo.ID, 
                    robotContext.AccountInfo.Balance.ToStringUniformMoneyFormat(true)));
        }
    }
    // ReSharper restore LocalizableElement
}
