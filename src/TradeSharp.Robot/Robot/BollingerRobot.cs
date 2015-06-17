using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.Robot.Robot
{
    /// <summary>
    /// Bollinger
    /// </summary>
    [DisplayName("Боллинджер")]
    public class Bollinger : BaseRobot
    {
        #region Настройки
        private decimal bollingerCoeff = 2;
        [PropertyXMLTag("bollingerCoeffs")]
        [DisplayName("Множитель волатильности")]
        [Category("Торговые")]
        [Description("Множитель волатильности")]
        public decimal BollingerCoeff
        {
            get { return bollingerCoeff; }
            set { bollingerCoeff = value; }
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

        private int candlesToDetermineAverage = 15;
        [PropertyXMLTag("CandlesToDetermineAverage")]
        [DisplayName("Свеч для среднего")]
        [Category("Торговые")]
        [Description("Свечек для определения среднего значения")]
        public int CandlesToDetermineAverage
        {
            get { return candlesToDetermineAverage; }
            set { candlesToDetermineAverage = value; }
        }

        private int candlesAfter = 5;
        [PropertyXMLTag("CandlesAfter")]
        [DisplayName("Контрольных свечей")]
        [Category("Торговые")]
        [Description("Количество свечей, на протяжении которых не заключается новых сделок")]
        public int CandlesAfter
        {
            get { return candlesAfter; }
            set { candlesAfter = value; }
        }
        #endregion

        #region Переменные
        /// <summary>
        /// последние свечки по каждой валютной паре
        /// </summary>
        private Dictionary<string, RestrictedQueue<CandleData>> storedCandles;

        private Dictionary<string, decimal> pointCost;

        private Dictionary<string, RestrictedQueue<int>> bollingerSignalHistory;

        private Dictionary<string, CandlePacker> packers;

        private List<string> events;
        #endregion

        public override void Initialize(BacktestServerProxy.RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(robotContext, protectedContext);

            if (Graphics.Count == 0) return;
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            pointCost = Graphics.ToDictionary(g => g.a, g => DalSpot.Instance.GetAbsValue(g.a, 1M));
            storedCandles = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<CandleData>(CandlesToDetermineAverage));
            bollingerSignalHistory = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<int>(CandlesAfter));
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return Graphics.ToDictionary(g => g.a, g => g.b.GetDistanceTime(candlesToDetermineAverage + candlesAfter, -1, startTrade));
        }

        public override BaseRobot MakeCopy()
        {
            var bot = new Bollinger
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                BollingerCoeff = BollingerCoeff,
                CloseOpposite = CloseOpposite,
                CandlesToDetermineAverage = CandlesToDetermineAverage,
                CandlesAfter = CandlesAfter,
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

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = new List<string>();
            if (packers == null) return events;

            for (var i = 0; i < quotes.Length; i++)
            {
                CandlePacker packer;
                if (!packers.TryGetValue(names[i], out packer))
                    continue;

                var candle = packer.UpdateCandle(quotes[i]);
                if (candle == null)
                    continue;

                var enterSign = CheckEnterCondition(names[i], candle);

                if (isHistoryStartOff || enterSign == 0) continue;

                // если есть открытые сделки против текущего направления - закрыть их
                if (CloseOpposite)
                    CloseCounterOrders(enterSign, names[i]);

                // открыть сделку
                OpenOrder(enterSign, names[i], quotes[i]);
            }

            return events;
        }

        private void OpenOrder(int dealSign, string symbol, CandleDataBidAsk lastCandle)
        {
            var volume = CalculateVolume(symbol, base.Leverage);
            if (volume == 0)
            {
                events.Add(string.Format("{0} {1} отменена - объем равен 0", dealSign > 0 ? "покупка" : "продажа", symbol));
                return;
            }

            var enterPrice = dealSign > 0 ? lastCandle.closeAsk : lastCandle.close;
            var point = pointCost[symbol];
            var stopLoss = enterPrice - dealSign * (float)point * StopLossPoints;
            var takeProfit = enterPrice + dealSign * (float)point * TakeProfitPoints;
            var order = new MarketOrder
            {
                AccountID = robotContext.AccountInfo.ID,
                Magic = Magic,
                Symbol = symbol,
                Volume = volume,
                Side = dealSign,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                // [A] не забывай уникальный комент
                ExpertComment = "Bollinger" 
            };
            var status = NewOrder(order,
                OrderType.Market,
                0, 0);
            if (status != RequestStatus.OK)
                events.Add(string.Format("Ошибка добавления ордера {0} {1}: {2}",
                    dealSign > 0 ? "BUY" : "SELL", symbol, status));
        }

        private void CloseCounterOrders(int dealSign, string symbol)
        {
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            if (orders.Count == 0) return;
            var ordersToClose = orders.Where(o => o.Symbol == symbol && o.Side != dealSign);

            foreach (var order in ordersToClose)
                CloseMarketOrder(order.ID, PositionExitReason.ClosedByRobot);
        }

        private int CheckEnterCondition(string symbol, CandleData candle)
        {
            if (Math.Abs(candle.open - candle.close) < 0.000001) return 0;
            var candlesList = storedCandles[symbol];
            candlesList.Add(candle);
            if (candlesList.Length < candlesList.MaxQueueLength)
                return 0; // [A] список для вычисления Б-линджера еще не заполнен

            var bollingerList = bollingerSignalHistory[symbol];
            if (bollingerList.Any(x => x != 0))
            {
                bollingerList.Add(0);
                return 0;
            }
            // [A] по-хорошему надо завести RestrictedQueue из кортежей:
            // 1) close 2) (close - open) * (close - open)
            // ... и вычислять средние значения close и корень из среднего значения (close - open) * (close - open)
            // Вычислений - возведений в квадрат - будет меньше
            var average = candlesList.Average(x => x.close);
            var v = Math.Sqrt(candlesList.Average(x => (x.close - x.open) * (x.close - x.open)));
            var high = average + v * (float)BollingerCoeff;
            var low = average - v * (float)BollingerCoeff;

            var signal = candle.close < low ? 1 : candle.close > high ? -1 : 0;
            bollingerList.Add(signal);
            return signal;
        }
    }
}
