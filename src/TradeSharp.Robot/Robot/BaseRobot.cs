using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [TypeConverter(typeof(PropertySorter))]
    public abstract class BaseRobot
    {
        protected RobotContext robotContext;

        protected CurrentProtectedContext protectedContext;

        [PropertyXMLTag("Robot.TypeName")]
        [LocalizedDisplayName("TitleTradeRobotName")]
        [LocalizedCategory("TitleMain")]
        [Description("Название торгового робота")]
        [ReadOnly(true)]
        [Browsable(false)]
        public string TypeName { get; set; }

        private volatile int magic;
        [PropertyXMLTag("Robot.Magic")]
        [LocalizedDisplayName("TitleIdentifyNumber")]
        [LocalizedCategory("TitleMain")]
        [Description("Уникальный идентификатор торгового робота")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int Magic
        {
            get { return magic; }
            set { magic = value; }
        }

        private decimal? leverage = 0.2M;
        [LocalizedDisplayName("TitleLeverage")]
        [Description("Плечо сделки")]
        [Category("Money Management")]
        [PropertyXMLTag("Leverage")]
        [PropertyOrder(31, 2)]
        public virtual decimal? Leverage
        {
            get { return leverage; }
            set { leverage = value; }
        }

        private int? fixedVolume = 10000;
        [PropertyXMLTag("Robot.FixedVolume")]
        [LocalizedDisplayName("TitleEnterVolume")]
        [Category("Money Management")]
        [Description("Объём, которым робот входит в рынок. 0 - не задан")]
        [PropertyOrder(33)]
        //[ValueList(false, 10000, 50000, 100000)] // ValueListAttribute usage example for numeric values
        public virtual int? FixedVolume
        {
            get { return fixedVolume; }
            set { fixedVolume = value; }
        }

        private List<Cortege2<string, BarSettings>> tickers = new List<Cortege2<string, BarSettings>>();
        [PropertyXMLTag("Robot.TimeSeries")]
        [LocalizedDisplayName("TitleUsedCharts")]
        [LocalizedCategory("TitleMain")]
        [Description("Список валютных пар и временных интервалов")]
        [PropertyOrder(1, 1)]
        [Editor(typeof(SymbolTimeframesUITypeEditor), typeof(UITypeEditor))]
        public string HumanRTickers
        {
            get { return GetStringFromTickerTimeframe(tickers); }
            set
            {
                tickers = ParseTickerTimeframeString(value);
            }
        }

        public static List<Cortege2<string, BarSettings>> ParseTickerTimeframeString(string str)
        {
            var resTickers = new List<Cortege2<string, BarSettings>>();
            var res = str.Split(new [] { ' ', ',', '.', '\n', '\t' });
            var allTickerNames = DalSpot.Instance.GetTickerNames();
            
            foreach (var item in res)
            {
                try
                {
                    string[] graph = item.Split(':');
                    if (graph.Length != 2)
                        return new List<Cortege2<string, BarSettings>>(); // должна быть структура типа EURUSD:H1

                    var ticker = graph[0].ToUpper();
                    if (string.IsNullOrEmpty(ticker)) continue;
                    if (!allTickerNames.Any(n => n == ticker)) continue;
                    
                    var barSets = BarSettingsStorage.Instance.GetBarSettingsByName(graph[1]);
                    if (barSets == null) continue;

                    resTickers.Add(new Cortege2<string, BarSettings>(ticker, barSets));
                }
                catch
                {
                    continue;
                }
            }
            return resTickers;
        }

        public static string GetStringFromTickerTimeframe(List<Cortege2<string, BarSettings>> tickerTimframeCol)
        {
            var res = new StringBuilder();
            foreach (var ticker in tickerTimframeCol)
            {
                if (res.Length != 0) res.Append(" ");
                res.Append(ticker.a);
                res.Append(":");
                res.Append(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(ticker.b));
            }
            return res.ToString();
        }

        /// <summary>
        /// Список графков, к которым привязан робот.
        /// Первый параметр (string) – торгуемый актив, например, USDCAD. Второй параметр (BarSettings) – таймфрейм, например, H4 
        /// </summary>
        [Browsable(false)]
        public List<Cortege2<string, BarSettings>> Graphics { get { return tickers; } set { tickers = value; } }

        public static readonly List<string> defaultTickers = new List<string>
            {
                "EURUSD" //, "GBPUSD", "USDJPY", "USDCHF", "USDCAD", "AUDUSD"
            };

        private List<int> newsChannels = new List<int>();

        [PropertyXMLTag("Robot.NewsChannels")]
        [LocalizedDisplayName("TitleNewsChannels")]
        [LocalizedCategory("TitleMain")]
        [Description("Подписка на новостные каналы")]
        [PropertyOrder(35)]
        public virtual string NewsChannels 
        {
            get { return string.Join(", ", newsChannels); }
            set 
            {
                try
                {
                    var res = value.Split(new [] {' ', ',', '.', '\n', '\t'});
                    var resList = new List<int>();
                    foreach (var item in res)
                    {
                        resList.Add(Convert.ToInt32(item));
                    }
                    newsChannels = resList;
                }
                catch(Exception)
                {
                    
                }
            }
        }

        [PropertyXMLTag("Robot.RoundType")]
        [LocalizedDisplayName("TitleRoundingType")]
        [Category("Money Management")]
        [Description("Тип округления")]
        [PropertyOrder(36)]
        public VolumeRoundType RoundType { get; set; }

        private int roundMinVolume = 10000;
        [PropertyXMLTag("Robot.RoundMinVolume")]
        [LocalizedDisplayName("TitleMinimalVolumeForDealRounding")]
        [Category("Money Management")]
        [Description("Минимальный объём для округления")]
        [PropertyOrder(37)]
        public virtual int RoundMinVolume
        {
            get { return roundMinVolume; }
            set { roundMinVolume = value; }
        }

        private int roundVolumeStep = 10000;
        [PropertyXMLTag("Robot.RoundVolumeStep")]
        [LocalizedDisplayName("TitleRoundStep")]
        [Category("Money Management")]
        [Description("Шаг округления")]
        [PropertyOrder(38)]
        public virtual int RoundVolumeStep
        {
            get { return roundVolumeStep; }
            set { roundVolumeStep = value; }
        }

        protected List<string> lastMessages;

        protected BaseRobot()
        {
            Magic = 0;
            defaultTickers.ForEach(t =>
                {
                    if (DalSpot.Instance.GetTickerNames().Contains(t))
                        Graphics.Add(new Cortege2<string, BarSettings>(t, BarSettingsStorage.Instance.GetBarSettingsByName("H1")));
                });
        }

        protected BaseRobot(BaseRobot robot) : this()
        {
            Magic = robot.Magic;
            TypeName = robot.TypeName;
            Leverage = robot.Leverage;
            FixedVolume = robot.FixedVolume;
            Graphics = robot.Graphics;
            NewsChannels = robot.NewsChannels;
        }

        public abstract BaseRobot MakeCopy();
        
        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        public virtual void Initialize(RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            this.protectedContext = protectedContext;
            this.robotContext = robotContext;
            TerminalLog.Instance.SaveRobotLog(string.Format("Запущен робот {0} #{1}", 
                GetUniqueName(), robotContext.AccountInfo.ID));
            if ((FixedVolume ?? 0) <= 0 && (Leverage ?? 0) <= 0)
                if (lastMessages != null)
                    lastMessages.Add("Не указаны ни плечо сделки, ни фиксированный объем входа. Торговля осуществляться не будет");
        }

        public virtual void UpdateAccountInfo(Account accountInfo)
        {
            robotContext.AccountInfo = accountInfo;
        }

        /// <summary>
        /// This method is used to unsubscribe all events.
        /// </summary>
        public virtual void DeInitialize()
        {
            TerminalLog.Instance.SaveRobotLog(string.Format("Остановлен робот {0}", GetUniqueName()));
        }

        /// <summary>
        /// Возвращает список используемых инструментов с указанием начальной даты
        /// </summary>
        public virtual Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            // поместить котировки из списка Graphics
            var dic = new Dictionary<string, DateTime>();
            foreach (var graphic in Graphics.Where(graphic => !dic.ContainsKey(graphic.a)))
            {
                dic.Add(graphic.a, startTrade);
            }
            return dic;
            
        }

        /// <summary>
        /// Выполняется на запуске торгового робота
        /// </summary>
        protected virtual void OnStartUp()
        {
            
        }

        /// <summary>
        /// Выполняется при завершении работы торгового робота
        /// </summary>
        protected virtual void OnShutDown()
        {
            
        }

        /// <summary>
        /// Вызывается на торговых событиях открытие закрытие позиции, срабатывание тейков и стопов
        /// </summary>
        /// <param name="order"></param>
        public virtual void OnTradeEvent(MarketOrder order)
        {
            TerminalLog.Instance.SaveRobotLog(string.Format("робот {0}: срабатывание рыночного ордера {1}, {2}, состояние {3}, side = {4}", 
                TypeName, order.ID, order.Symbol, order.State, order.Side));
        }

        /// <summary>
        /// Вызывается на торговых событиях установка снятие ордера, изменение параметров, срабатывание ордера
        /// </summary>
        /// <param name="order"></param>
        public virtual void OnTradeEvent(PendingOrder order)
        {
            TerminalLog.Instance.SaveRobotLog(string.Format("робот {0}: срабатывание отложенного ордера {1}, {2}, статус {3}, side = {4}",
                TypeName, order.ID, order.Symbol, order.Status, order.Side));
        }

        /// <summary>
        /// Вызывается с приходом новой котировки
        /// </summary>
        public abstract List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff);

        /// <summary>
        /// Called on news incoming
        /// </summary>
        public virtual List<string> OnNewsReceived(News[] news)
        {
            return null;
        }

        public virtual string ReportState()
        {
            if (robotContext == null)
                return Localizer.GetString("TitleNotInitialized");
            return Localizer.GetString("TitleStarted");
        }

        #region Trading functions

        /// <summary>
        /// Send market or instant order
        /// </summary>
        /// <param name="order">все параметры ордера</param>
        /// <param name="ordType"></param>
        /// <param name="slippagePoints"></param>        
        /// <returns></returns>
        protected RequestStatus NewOrder(MarketOrder order, OrderType ordType, 
            decimal requestedPrice, decimal slippagePoints)
        {
            order.Magic = magic;
            order.AccountID = robotContext.AccountInfo.ID;
            return robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(), 
                order,
                ordType, requestedPrice, slippagePoints);
        }

        /// <summary>
        /// Send pending order
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="symbol"></param>
        /// <param name="volume"></param>
        /// <param name="side"></param>
        /// <param name="pendingOrdType"></param>
        /// <param name="targetPriceFrom"></param>
        /// <param name="targetPriceTo"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="stopPrice"></param>
        /// <param name="takePrice"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        protected RequestStatus NewPendingOrder(PendingOrder order)
        {
            order.AccountID = robotContext.AccountInfo.ID;

            return robotContext.SendNewPendingOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(), order);
        }

        /// <summary>
        /// Close position request
        /// </summary>
        protected RequestStatus CloseMarketOrder(int orderId, PositionExitReason reason = PositionExitReason.ClosedByRobot)
        {
            return robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(), 
                robotContext.AccountInfo.ID, orderId, reason);
        }

        /// <summary>
        /// Delete pending order request
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        protected RequestStatus DeletePendingOrder(PendingOrder order)
        {
            return robotContext.SendDeletePendingOrderRequest(protectedContext.MakeProtectedContext(), order, 
                PendingOrderStatus.Отменен, null, "Отменен роботом");
        }

        /// <summary>
        /// Change parameters for market 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected RequestStatus ModifyMarketOrder(MarketOrder pos)
        {
            return robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), pos);
        }

        /// <summary>
        /// Change parameters for pending order
        /// </summary>
        /// <param name="ord"></param>
        /// <returns></returns>
        protected RequestStatus ModifyPendingOrder(PendingOrder ord)
        {
            return robotContext.SendEditPendingRequest(protectedContext.MakeProtectedContext(), ord);
        }
        #endregion

        /// <summary>
        /// получить список открытых позиций, которые принадлежат роботу
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="filterByMagic"></param>
        /// <returns></returns>
        protected RequestStatus GetMarketOrders(out List<MarketOrder> orders, bool filterByMagic = true)
        {
            List<MarketOrder> allOrders;
            var res = robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out allOrders);
            orders = filterByMagic ? allOrders.Where(order => order.Magic == Magic).ToList() : allOrders;
            return res;
        }

        /// <summary>
        /// получить список отложенных ордеров, которые принадлежат роботу
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        protected RequestStatus GetPendingOrders(out List<PendingOrder> orders)
        {
            List<PendingOrder> allOrders;
            var res = robotContext.GetPendingOrders(robotContext.AccountInfo.ID, out allOrders);
            orders = allOrders.Where(order => order.Magic == Magic).ToList();
            return res;
        }

        /// <summary>
        /// робот, переданный в параметре, получит копию настроек робота this
        /// </summary>
        protected void CopyBaseSettings(BaseRobot dest)
        {
            dest.TypeName = TypeName;
            dest.Magic = Magic;
            dest.Leverage = Leverage;
            dest.FixedVolume = FixedVolume;
            dest.tickers = tickers.Select(t => new Cortege2<string, BarSettings>(t.a, new BarSettings(t.b))).ToList();
            dest.NewsChannels = NewsChannels;
        }

        protected int CalculateVolumeInBaseCurrency(decimal volumeDepo, string tradeTicker,
            VolumeRoundType roundType, QuoteData quoteByTicker = null)
        {
            var volumeBase = volumeDepo;

            var depoCurx = robotContext.AccountInfo.Currency;
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            bool inverse, pairsEqual;
            var tickerTrans = DalSpot.Instance.FindSymbol(tradeTicker, true, depoCurx,
                out inverse, out pairsEqual);
            
            if (!pairsEqual)
            {
                QuoteData quote;
                if (tickerTrans == tradeTicker && quoteByTicker != null)
                    quote = quoteByTicker;
                else
                    quotes.TryGetValue(tickerTrans, out quote);
                if (quote == null)
                {
                    var msgError = string.Format(
                        "Невозможно рассчитать объем - отсутствует котировка \"{0}\"", tickerTrans);
                    Logger.Info(msgError);
                    return 0;
                }
                var priceTrans = inverse ? 1 / quote.bid : quote.ask;
                volumeBase /= (decimal)priceTrans;
            }

            return MarketOrder.RoundDealVolume((int)volumeBase, roundType, RoundMinVolume, RoundVolumeStep);
        }

        /// <summary>
        /// Расчёт объёма сделки в валюте депозита с учётом наличия или отсутствия фиксированного объёма
        /// </summary>
        protected int CalculateVolume(string ticker, decimal? calculateLeverage = null)
        {
            if (FixedVolume.HasValue && FixedVolume.Value != 0) return FixedVolume.Value;

            calculateLeverage = calculateLeverage ?? Leverage;

            Account ac;
            robotContext.GetAccountInfo(robotContext.AccountInfo.ID, true, out ac);
            if (ac == null || ac.Equity <= 0)
            {
                var errorStr = ac == null
                    ? string.Format("Счет {0} не найден", robotContext.AccountInfo.ID)
                    : string.Format("На счете {0} недостаточно средств ({1})",
                        robotContext.AccountInfo.ID, ac.Equity.ToStringUniformMoneyFormat());
                Logger.Info(errorStr);
                return 0;
            }
           
            string error;
            var depoDealVolume = DalSpot.Instance.ConvertToTargetCurrency(ticker, true, ac.Currency,
                (double)(ac.Equity * calculateLeverage), robotContext.QuotesStorage.ReceiveAllData(),
                out error);

            if (!depoDealVolume.HasValue)
            {
                Logger.InfoFormat("Не удалось перевести средства в валюту депозита. " + error);
                return 0;
            }

            var roundMinVolm = RoundMinVolume;
            var roundVolmStep = RoundVolumeStep;

            if (roundMinVolm == 0 || roundVolmStep == 0)
            {
                var minStepLot = DalSpot.Instance.GetMinStepLot(ticker, robotContext.AccountInfo.Group);
                if (roundMinVolm == 0)
                    roundMinVolm = minStepLot.b;
                if (roundVolmStep == 0)
                    roundVolmStep = minStepLot.a;
            }

            var volume = MarketOrder.RoundDealVolume((int)depoDealVolume.Value, RoundType, roundMinVolm, roundVolmStep);

            if (volume == 0)
                Logger.InfoFormat("{0}: OpenDeal({0} {1}) - объем в валюте депозита ({2:f2}) недостаточен",TypeName,ticker, volume);                            

            return volume;
        }

        protected float GetPointsDeviation(MarketOrder order, float newValue, bool stopLoss)
        {
            var value = stopLoss ? order.StopLoss : order.TakeProfit;
            if (!value.HasValue)
                return 0;
            var pointsValue = DalSpot.Instance.GetPointsValue(order.Symbol, value.Value);
            var newPointsValue = DalSpot.Instance.GetPointsValue(order.Symbol, newValue);
            return newPointsValue - pointsValue;
        }

        protected string MakeLog(string message)
        {
            return string.Format("{0} [Magic = {1}]: {2}", TypeName, Magic, message);
        }

        public string GetUniqueName()
        {
            return TypeName + " (" + string.Join(", ", Graphics.Select(g =>
                string.Format("{0}:{1}", g.a, 
                BarSettingsStorage.Instance.GetBarSettingsFriendlyName(g.b)))) + ")";
        }
    }
}
