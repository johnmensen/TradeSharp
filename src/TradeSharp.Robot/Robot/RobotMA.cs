using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    /// <summary>
    /// Робот, торгующий на основе скользящих средних
    /// </summary>
    // ReSharper disable LocalizableElement
    [DisplayName("Скользящие средние")]
    public class RobotMA : BaseRobot 
    {
        /// <summary>
        /// Для тестирования. Что бы понимать, какое текущее состояние робота.
        /// Объявлен внутри класса самого робота, что бы "RobotMAInnerState" лишний раз не "светился" в структуре классов.
        /// </summary>
        public class RobotMAInnerState
        {
            public int maDifSign;
            public double maValueFast;
            public double maValueSlow;
            public CandleData lastCandle;

            public bool AreSame(RobotMAInnerState state)
            {
                if (maDifSign != state.maDifSign) return false;
                if (!maValueFast.SameDouble(state.maValueFast)) return false;
                if (!maValueSlow.SameDouble(state.maValueSlow)) return false;
                if (lastCandle != null && state.lastCandle != null)
                    if (!lastCandle.close.SameDouble(state.lastCandle.close)) return false;
                return true;
            }
        }

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

        private int rangeSlowMA = 14;
        [PropertyXMLTag("Robot.RangeSlowMA")]
        [DisplayName("Диапазон медленной скользящей средней")]
        [Category("Торговые")]
        [Description("Диапазон для расчёта медленной скользящей средней")]
        public int RangeSlowMA
        {
            get { return rangeSlowMA; }
            set { rangeSlowMA = value; }
        }

        private int rangeFastMA = 5;
        [PropertyXMLTag("Robot.RangeFastMA")]
        [DisplayName("Диапазон быстрой скользящей средней")]
        [Category("Торговые")]
        [Description("Диапазон для быстрой медленной скользящей средней")]
        public int RangeFastMA
        {
            get { return rangeFastMA; }
            set { rangeFastMA = value; }
        }

        private bool deriveSiteFromHistory = true;
        [PropertyXMLTag("Robot.DeriveSiteFromHistory")]
        [DisplayName("Выводить знак сделки из стории")]
        [Category("Торговые")]
        [Description("При принятии решения об типе открываемой сделки (покупка или продажа), робот будет оринтироваться не только " +
                     "на способ пересечения скользящих средних, но и на историю уже заключенный сделок.")]
        public bool DeriveSiteFromHistory
        {
            get { return deriveSiteFromHistory; }
            set { deriveSiteFromHistory = value; }
        }

        private int countCandelToDerive = 200;
        [PropertyXMLTag("Robot.CountCandelToDerive")]
        [DisplayName("История для смены знака")]
        [Category("Торговые")]
        [Description("Количество свечей для оценки знака сделки из истории")]
        public int CountCandelToDerive
        {
            get { return countCandelToDerive; }
            set
            {
                if (value > 1)
                    countCandelToDerive = value;
            }
        }

        /// <summary>
        /// Диапазон "не заключения" сделки
        /// </summary>
        private float unknownUnitProfit = 5;
        [PropertyXMLTag("Robot.UnknownUnitProfit")]
        [DisplayName("Неопределённый удельный профит")]
        [Category("Торговые")]
        [Description("Величина удельного профита колеблеться около нуля - сделка не заключается")]
        public float UnknownUnitProfit
        {
            get { return unknownUnitProfit; }
            set { unknownUnitProfit = value; }
        }

        private int periodVirtualResults = 15;
        [PropertyXMLTag("Robot.PeriodVirtualResults")]
        [DisplayName("Период учёта виртуальных сделок")]
        [Category("Торговые")]
        [Description("Определяет количество свечей, на которых анализируются виртуальные сделки")]
        public int PeriodVirtualResults
        {
            get { return periodVirtualResults; }
            set { periodVirtualResults = value; }
        }
        #endregion

        #region Переменные
        private string ticker;

        /// <summary>
        /// объект "packer" реализует преобразование котировок в свечи заданного типа (m5, H1, H4). 
        /// Очередная котировка передается в метод  "UpdateCandle", что бы узнать - закрылась ли свеча. 
        /// </summary>
        private CandlePacker packer;
         
        /// <summary>
        /// Знак сделки, которая сейчас рассматривается роботом, как кандидат на совершение.
        /// Если 0 - не торгует
        /// 1 - входит в направлении быстрой СС
        /// -1 - входит против быстрой СС
        /// </summary>
        private int maDifSign;

        /// <summary>
        /// Очередь с вытеснением из значений цен закрытия свечей для расчёта медленной СС
        /// </summary>
        private RestrictedQueue<float> queueSlow;

        /// <summary>
        /// Очередь с вытеснением из значений цен закрытия свечей для расчёта быстрой СС
        /// </summary>
        private RestrictedQueue<float> queueFast;

        /// <summary>
        /// Значение знака предыдущей заключённой сделки
        /// </summary>
        private int prevSign;

        /// <summary>
        /// Хранит текущее состояние на закрытии текущей свечи, что бы на следующей итерации использовать для "виртуальных" сделок
        /// </summary>
        private MarketOrder virtualDeal;

        /// <summary>
        /// Очередь с вытеснением из значений прибылей виртуальных сделок
        /// </summary>
        private RestrictedQueue<float> virtualResults;

        /// <summary>
        /// Список событий для отчёта о ходе работы робота
        /// </summary>
        private List<string> events;

        /// <summary>
        /// Для тестирования робота. Вызывается в конце меотда анализа ситуации (торговать или нет) - CandlesAnalysisAndTrade
        /// </summary>
        public Action<RobotMAInnerState, DateTime> debugAction;
        #endregion

        /// <summary>
        /// создать полную копию робота,
        /// оперативные настройки (переменные-члены) копировать необязательно
        /// </summary>
        public override BaseRobot MakeCopy()
        {
            var bot = new RobotMA
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                RangeSlowMA = RangeSlowMA,
                RangeFastMA = RangeFastMA,
                FixedVolume = FixedVolume,
                CountCandelToDerive = CountCandelToDerive,
                PeriodVirtualResults = PeriodVirtualResults,
                UnknownUnitProfit = UnknownUnitProfit,
                Leverage = Leverage,
                RoundType = RoundType,
                NewsChannels = NewsChannels,
                RoundMinVolume = RoundMinVolume,
                RoundVolumeStep = RoundVolumeStep

            };
            CopyBaseSettings(bot);
            return bot;
        }

        public RobotMA()
        {
            lastMessages = new List<string>();
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContextx)
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
            maDifSign = 0;
            queueSlow = new RestrictedQueue<float>(RangeSlowMA);
            queueFast = new RestrictedQueue<float>(RangeFastMA);

            virtualDeal = null;
            prevSign = 0;
            virtualResults = new RestrictedQueue<float>(PeriodVirtualResults);
        }

        /// <summary>
        /// На какой отрезок времени в истории должен заглянуть робот при старте и какие котировки ему понадобятся 
        /// </summary>
        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0)
            {
                Logger.ErrorFormat("Робот {0} не может продолжать работу: не найден ни один объект графика для этого робота", TypeName);
                return null;
            }
            try
            {
                var historyIndexStart = Graphics[0].b.GetDistanceTime(rangeSlowMA + countCandelToDerive, -1, startTrade);
                return new Dictionary<string, DateTime> {{Graphics[0].a, historyIndexStart}};
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Робот {0} не может продолжать работу: {1}", TypeName, ex.Message), ex);
                return null;
            }
        }

        /// <summary>
        /// Этот метод вызывается каждый раз при новых данных котировки. Метод реализует основную логику торговли робота. 
        /// Здесь принимается решение о открытии нового ордера и закрытии открытых сделок
        /// Совершение сделок будет поисходить на закрытии свечи. При закрытии свечки метод  UpdateCandle объекта packer вернёт эту свечу, иначе Null.
        /// В случае если свеча закрылась, тогда методом CalculateMA расчитываем значение скользящей средней для текущей котировки.
        /// Если быстрая и медленная скользящие седние пересекаются, а так же если текущая котировка не является "исторической", тогда можно
        /// принимать решение об открытии новой сделки.
        /// </summary>
        /// <param name="quotes"></param>
        /// <param name="isHistoryStartOff">Флаг, показывающий, является текущая котировка взятой из истории, или это значение катировки на текущий момент на рынке</param>
        /// <param name="names"></param>
        public override List<string> OnQuotesReceived(string[] names, 
            CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = lastMessages.ToList();
            lastMessages.Clear();

            #region Получение текущей свечи
            // Массивы "names" и "quotes" всегда содержат одинаковое количество элементов (по одному). Фактически это пары ключ/значение
            
            if (string.IsNullOrEmpty(ticker))
            {
                Logger.ErrorFormat("Название текущего инструмента (ticker) для робота {0} задано не корректно", TypeName);
                return events;
            }         
            var tickerIndex = -1;
            for (var i = 0; i < names.Length; i++)
                if (names[i] == ticker)
                {
                    tickerIndex = i;
                    break;
                }
            if (tickerIndex < 0)
            {
                //Logger.InfoFormat("Не удалось получить котировку для робота {0}", TypeName);
                return events;
            }

            //Выбираем из всего массива текущих катировкок, катировку для того инструмента, которым торгует робот
            var quote = quotes[tickerIndex];

            
            var candle = packer.UpdateCandle(quote);
            #endregion

            // Если свеча закрылась, тогда candle != null  (сделки соверши)
            if (candle == null) return events;

            queueSlow.Add(candle.close);
            queueFast.Add(candle.close);
            if (queueSlow.Length < queueSlow.MaxQueueLength) return events;
            CandlesAnalysisAndTrade(candle, isHistoryStartOff);
            return events;
        }

        /// <summary>
        /// Содержит главную логику робота по анализу свечей и принятия решений о заключении сделок. Этот метод открытый, потому что он используется при 
        /// тестировании робота.
        /// </summary>
        public void CandlesAnalysisAndTrade(CandleData candle, bool isHistoryStartOff)
        {
            if (virtualDeal != null)
            {
                var virtDealResult = DalSpot.Instance.GetPointsValue(ticker, virtualDeal.Side * (candle.close - virtualDeal.PriceEnter));
                virtualResults.DequeueLast();
                virtualResults.Add(virtDealResult);
            }

            var currentSign = Math.Sign(queueFast.Average() - queueSlow.Average()); // Вычисляем текущее расположение скользящих средних

            try
            {
                if (prevSign == 0 || prevSign == currentSign) return;
                // СС пересеклись
                // Считаем средней результат виртуальных сделок
                var avgVirtualResult = virtualResults.Length == 0 ? 0 : virtualResults.Average();

                // Если следний редультат виртуальных сделок не входит в диапазон "не торговать", то высчитываем знак текущей сделки
                maDifSign = Math.Abs(avgVirtualResult) < unknownUnitProfit ? 0 : Math.Sign(avgVirtualResult);

                // "На будущее" запоминаем текущую стуацию, для расчёта виртуальных сделок на следующих итерациях
                virtualDeal = new MarketOrder
                {
                    PriceEnter = candle.close,
                    Side = currentSign
                };
                virtualResults.Add(0); // тут будет результат новой сделки на следующей итерации

                // пересечение скользящих средних произошло не на истории
                if (!isHistoryStartOff)
                {
                    List<MarketOrder> orders;
                    robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                    var tradeSide = deriveSiteFromHistory ? currentSign * maDifSign : currentSign;
                    Trade(candle, orders, tradeSide);
                }
            }
            finally
            {
                prevSign = currentSign;

                if (debugAction != null)
                    debugAction(new RobotMAInnerState
                        {
                            maDifSign = maDifSign,
                            maValueFast = queueFast.Average(),
                            maValueSlow = queueSlow.Average(),
                            lastCandle = candle,
                        }, 
                        candle.timeClose);
            }
        }

        /// <summary>
        /// Торговля
        /// </summary>
        private void Trade(CandleData candle, List<MarketOrder> orders, int currentSign)
        {
            // коментарий
            if (currentSign != 0)
                events.Add(new RobotHint(ticker, Graphics[0].b.ToString(), "вход по MA", "вход", "e", candle.close).ToString());

            // закрыть сделки противонаправленные текущему знаку
            foreach (var order in orders.Where(o => o.Side != currentSign && o.Magic == Magic && o.Symbol == ticker).ToList())
                robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                              robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);

            if (currentSign == 0) return;

            // открыть сделку в соответствии с текущим знаком
            OpenDeal(candle.close, currentSign);
        }

        /// <summary>
        /// Открыть новую сделку по указанной цене price, указанного типа dealSide (покупка или продажа)
        /// </summary>
        private void OpenDeal(float price, int dealSide)
        {
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

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
                    ExpertComment = "MARobot"                   // Комментарий по сделке, оставленный роботом
                },
                OrderType.Market, 0, 0);
        }
    }
    // ReSharper restore LocalizableElement
}
