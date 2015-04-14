using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("Двойная вершина")] 
    class DoubleTopRobot : BaseRobot
    {
        /// <summary>
        /// Минимальное плечо
        /// </summary>
        private const float MinShoulder = 0.6f;

        /// <summary>
        /// Максимальное плечё
        /// </summary>
        private const float MaxShoulder = 1.4f;

        /// <summary>
        /// Диапазон для расчёта локальных екстремумов
        /// </summary>
        private const int ExtremumRange = 3;

        #region Настройки
        // ReSharper disable ConvertToAutoProperty
        private int takeRange = 50;
        [PropertyXMLTag("Robot.TakeRange")]        
        [DisplayName("Коэффициент TakeRange, %")]
        [Category("Торговые")]
        [Description("Коэффициент, на который домножается разность между максимальным экстремумом и текущей ценой закрытия, при вычислении значения Take profit-а")]
        public int TakeRange
        {
            get { return takeRange; }
            set
            {
                if (value > 0 && value <= 100)
                takeRange = value;
            }
        }

        private bool closeDeal;
        [PropertyXMLTag("Robot.CloseDeal")]
        [DisplayName("автозакрытие сделок")]
        [Category("Торговые")]
        [Description("Закрывать ли все противонаправленные сделки при входе в рынок")]
        public bool CloseDeal
        {
            get { return closeDeal; }
            set { closeDeal = value; }
        }

        private int stopLossPoints;
        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        private int takeProfitPoints;
        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }
        // ReSharper restore ConvertToAutoProperty
        #endregion

        #region Переменные

        private string ticker;
        private CandlePacker packer;
        private float? stopLoss;
        private float? takeProfit;
        
        /// <summary>
        /// Специальная очередь для поиска экстремумов. В ней храниться диапазон значений, среди которых может быть только один локальный экстремум
        /// </summary>
        private RestrictedQueue<CandleData> extremumRangeQueue;

        /// <summary>
        /// Очередь хранит экстремумы - как максимумы так и минимумы
        /// </summary>
        private RestrictedQueue<Cortege3<float, bool, DateTime>> extremumQueue;
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new DoubleTopRobot
            {
                CloseDeal = CloseDeal,
                TakeRange = TakeRange,
                FixedVolume = FixedVolume,
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                Leverage = Leverage,
                RoundType = RoundType,
                NewsChannels = NewsChannels,
                RoundMinVolume = RoundMinVolume,
                RoundVolumeStep = RoundVolumeStep
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, Contract.Util.BL.CurrentProtectedContext protectedContextx)
        {
            base.Initialize(grobotContext, protectedContextx);
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("RobotMA: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("RobotMA: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            packer = new CandlePacker(Graphics[0].b);
            ticker = Graphics[0].a;
            stopLoss = null;
            takeProfit = null;


            extremumRangeQueue = new RestrictedQueue<CandleData>(ExtremumRange * 2 + 1);
            extremumQueue = new RestrictedQueue<Cortege3<float, bool, DateTime>>(4);
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            var historyIndexStart = Graphics[0].b.GetDistanceTime(ExtremumRange + 1, -1, startTrade);
            return new Dictionary<string, DateTime> { { Graphics[0].a, historyIndexStart } };
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var events = new List<string>();
            #region получить candle из quote
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

            var candle = packer.UpdateCandle(quote);
            if (candle == null) return events;
            #endregion
            
            extremumRangeQueue.Add(candle);                 
            if (extremumRangeQueue.Length != extremumRangeQueue.MaxQueueLength) 
                return events;


            //  Если очередь заполнена полностью, значит можно проверять её среднее значение на локальный экстремум
            CheckExtremum(extremumRangeQueue);

            //  Все ли 4 экстремума сейчас имеются для анализа
            if (extremumQueue.Length != extremumQueue.MaxQueueLength) 
                return events;

            var extrArray = extremumQueue.ToArray();
            //  Теперь можно проверить - чередуются ли экстремумы.
            var boofSign = !extrArray[0].b;
            foreach (var extremum in extrArray)
            {
                if (extremum.b == boofSign) 
                    return events; // Это значит что два максимума или два минимума идут друг за другом
                boofSign = !boofSign;
            }

            //  Экстремумы чередуются друг за другом.
            //  Теперь проверяем, больше ли по модулю "а", чем "b", "с" и "d" и close.
            var priceA = extrArray[0].a;
            var currentSide = extrArray[0].b ? 1 : -1;

            for (var i = 1; i < extrArray.Length; i++)
            {
                if (currentSide != Math.Sign(priceA - extrArray[i].a)) 
                    return events; 
            }
            if (currentSide != Math.Sign(priceA - candle.close))
                return events; 

            // соотнесение "плеч"
            var ab = Math.Abs(extrArray[0].a - extrArray[1].a);
            var bc = Math.Abs(extrArray[1].a - extrArray[2].a);
            var cd = Math.Abs(extrArray[2].a - extrArray[3].a);
            var dClose = Math.Abs(extrArray[3].a - candle.close);

            var bcCd = cd == 0 ? float.MaxValue : bc / cd;
            if (bcCd < MinShoulder || bcCd > MaxShoulder)
                return events;
            if (ab <= bc || ab <= cd || ab <= dClose)
                return events;
            
            //  Первый экстремум является большим по модулю, чем остальные - можно совершать сделку   
            if (!isHistoryStartOff) // линии скользящих средних пересеклись причём не на истории. Значит пора торговать!!!
            {
                // коментарий
                var extrNames = new [] {"A", "B", "C", "D"};
                var nameIndex = 0;
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var extr in extrArray)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    events.Add(new RobotHint(ticker, 
                            Graphics[0].b.ToString(), 
                            extrNames[nameIndex], 
                            extrNames[nameIndex],
                            extrNames[nameIndex++], extr.a)
                            {
                                Time = extr.c,
                                RobotHintType = RobotHint.HintType.Линия //extr.b ? RobotHint.HintType.Покупка : RobotHint.HintType.Продажа
                            }.ToString());
                }

                // закрыть противонаправленные, если автозакрытие включено пользователем
                if (CloseDeal)
                {
                    List<MarketOrder> orders;
                    robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                    foreach (
                        var order in
                            orders.Where(o => o.Side != currentSide && o.Magic == Magic && o.Symbol == ticker).ToList())
                        robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                                      robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                }

                OpenDeal(candle.close, currentSide);
                
            }           
            return events;
        }

        private void OpenDeal(float price, int dealSide)
        {
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

            // Посчитать stopLoss и takeProfit
            stopLoss = StopLossPoints == 0
                ? dealSide > 0 ? extremumQueue.Min(q => q.a) : extremumQueue.Max(q => q.a)
                : price - dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)StopLossPoints);
            takeProfit = TakeProfitPoints == 0
                                ? extremumQueue.First.a
                                : price + dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)TakeProfitPoints);

            // очистить очередь экстремумов
            extremumQueue.Clear();

            // открыть сделку
            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                {
                    AccountID = robotContext.AccountInfo.ID,
                    Magic = Magic,
                    Symbol = ticker,
                    Volume = dealVolumeDepo,
                    Side = dealSide,
                    StopLoss = stopLoss,
                    TakeProfit = takeProfit * TakeRange / 100,
                    ExpertComment = "DoubleTopRobot"
                },
            OrderType.Market, 0, 0);
        }

        /// <summary>
        /// Проверяет, является ли средний элемент в "queue" локальным экстремумом
        /// </summary>
        private void CheckExtremum(RestrictedQueue<CandleData> queue)
        {
            bool isMax = true, isMin = true;
            
            // Проверяемая на экстремум свеча
            var checkingCandle = queue.ElementAt(ExtremumRange);

            for (var i = 0; i < queue.MaxQueueLength; i++)
            {
                if (i == ExtremumRange) continue;
                var candle = queue.ElementAt(i);
                if (checkingCandle.close <= candle.close) isMax = false;
                if (checkingCandle.close >= candle.close) isMin = false;
                if (!isMax && !isMin) return;
            }

            if (isMax) extremumQueue.Add(new Cortege3<float, bool, DateTime>(checkingCandle.close, true, checkingCandle.timeClose));
            if (isMin) extremumQueue.Add(new Cortege3<float, bool, DateTime>(checkingCandle.close, false, checkingCandle.timeClose));
        }
    }
    // ReSharper restore LocalizableElement
}
