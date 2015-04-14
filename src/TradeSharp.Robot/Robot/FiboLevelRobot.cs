using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("FX")]
    public class FiboLevelRobot : BaseRobot
    {
        private const string HintCodePriceLevel = "FXBotEnterLevel";
        private const string HintPriceBIsUpdated = "FXBotBUpdated";

        private CandlePacker packer;
        private readonly List<CandleData> candles = new List<CandleData>();
        private string ticker;
        public const string RobotNamePreffix = "FiboLevelRobot";

        /// <summary>
        /// содержит дополнительные атрибуты ордера,
        /// определяемые через поле ExpertComment
        /// (цены A - B, номер в серии)
        /// </summary>
        class FiboRobotPosition
        {
            public MarketOrder order;

            public BarSettings timeframe;

            public decimal PriceA { get; set; }

            public decimal PriceB { get; set; }

            /// <summary>
            /// 1 для "первичного" входа, 2 для "вторичного" и т.д.
            /// </summary>
            public int Sequence { get; set; }

            public static void MakeOrderComments(MarketOrder order, BarSettings timeframe,
                decimal priceA, decimal priceB, int sequence = 1)
            {
                order.ExpertComment = RobotNamePreffix;
                order.Comment = string.Join(";",
                                            BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe),
                                            priceA.ToStringUniformPriceFormat(),
                                            priceB.ToStringUniformPriceFormat(),
                                            sequence);
            }

            public static List<FiboRobotPosition> GetRobotPositions(string ticker,
                IEnumerable<MarketOrder> orders, BarSettings timeframe,
                decimal? priceAFilter, decimal? priceBFilter)
            {
                var maxDeltaPriceAbs = DalSpot.Instance.GetAbsValue(ticker, 1.5M);
                var deals = orders.Select(MakeFiboPosition)
                          .Where(o => o != null)
                          .Where(d =>
                              d.timeframe == timeframe &&
                              (!priceAFilter.HasValue || d.PriceA.RoughCompares(priceAFilter.Value, maxDeltaPriceAbs)) &&
                              (!priceBFilter.HasValue || d.PriceB.RoughCompares(priceBFilter.Value, maxDeltaPriceAbs)))
                          .ToList();
                return deals;
            }

            private static FiboRobotPosition MakeFiboPosition(MarketOrder order)
            {
                if (order.ExpertComment != RobotNamePreffix) return null;
                var barSetsPreffixLength = order.Comment.IndexOf(';');
                if (barSetsPreffixLength <= 0) return null;
                var barSetsPreffix = order.Comment.Substring(0, barSetsPreffixLength);
                var barSettings = BarSettingsStorage.Instance.GetBarSettingsByName(barSetsPreffix);
                if (barSettings == null)
                    return null;

                var commentStr = order.Comment.Substring(barSetsPreffixLength + 1);

                var commentParts = commentStr.ToDecimalArrayUniform();
                if (commentParts.Length < 2 || commentParts.Length > 3) return null;

                return new FiboRobotPosition
                {
                    timeframe = barSettings,
                    order = order,
                    PriceA = commentParts[0],
                    PriceB = commentParts[1],
                    Sequence = commentParts.Length > 2 ? (int)commentParts[2] : 1
                };
            }
        }

        #region Неактуальные настройки
        [Browsable(false)]
        public override string NewsChannels { get; set; }
        #endregion

        #region Money Management

        [PropertyXMLTag("Robot.FixedVolume")]
        [DisplayName("Фикс. объем входа")]
        [Category("Money Management")]
        [Description("Объём, которым робот входит в рынок. 0 - не задан")]
        [PropertyOrder(12, 4)]
        public override int? FixedVolume { get; set; }

        private decimal leverageStep = 0.025M;
        [DisplayName("Шаг плеча")]
        [Description("Шаг плеча сделки")]
        [Category("Money Management")]
        [PropertyXMLTag("LeverageStep")]
        [PropertyOrder(10, 4)]
        public decimal LeverageStep
        {
            get { return leverageStep; }
            set { leverageStep = value; }
        }

        private int maxDealsInSeries = 5;
        [PropertyXMLTag("Robot.MaxDealsInSeries")]
        [DisplayName("Макс сделок подряд")]
        [Category("Money Management")]
        [Description("Макс. количество последовательно идущих входов")]
        [PropertyOrder(11, 4)]
        public int MaxDealsInSeries { get { return maxDealsInSeries; } set { maxDealsInSeries = value; } }

        #endregion

        #region Правила входа

        private decimal priceA;
        [DisplayName("Цена A")]
        [Description("1-я цена Фибоначчи")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.PriceA")]
        [RuntimeAccess(true)]
        [PropertyOrder(1, 1)]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public decimal PriceA
        {
            get { return priceA; }
            set
            {
                priceA = value;
                side = PriceB > PriceA ? 1 : PriceB < PriceA ? -1 : 0;
            }
        }

        private decimal priceB;
        [DisplayName("Цена B")]
        [Description("2-я цена Фибоначчи")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.PriceB")]
        [RuntimeAccess(true)]
        [PropertyOrder(2, 1)]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public decimal PriceB
        {
            get { return priceB; }
            set
            {
                priceB = value;
                side = PriceB > PriceA ? 1 : PriceB < PriceA ? -1 : 0;
            }
        }

        private decimal koefEnter = 0.38M;
        [DisplayName("Уровень входа 1")]
        [Description("Уровень \"коррекции\" или \"расширения\" для входа в рынок")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.KoefEnter")]
        [RuntimeAccess(true)]
        [PropertyOrder(10, 1)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoefEnter
        {
            get { return koefEnter; }
            set { koefEnter = value; }
        }

        private decimal koeffEnterLowB = 0.618M;
        [DisplayName("Уровень входа 1 - верх")]
        [Description("Верхняя (вторая) граница уровня входа (1)")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.KoeffEnterLowB")]
        [RuntimeAccess(true)]
        [PropertyOrder(11, 1)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoeffEnterLowB
        {
            get { return koeffEnterLowB; }
            set { koeffEnterLowB = value; }
        }

        private decimal koeffEnterHighA = 1.618M;
        [DisplayName("Уровень входа 2 -  низ")]
        [Description("Нижняя (первая) граница уровня входа (2)")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.KoeffEnterHighA")]
        [RuntimeAccess(true)]
        [PropertyOrder(12, 1)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoeffEnterHighA
        {
            get { return koeffEnterHighA; } 
            set { koeffEnterHighA = value; }
        }

        private decimal koeffEnterHighB = 2.618M;
        [DisplayName("Уровень входа 2 - верх")]
        [Description("Верхняя (вторая) граница уровня входа (2)")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.KoeffEnterHighB")]
        [RuntimeAccess(true)]
        [PropertyOrder(13, 1)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoeffEnterHighB
        {
            get { return koeffEnterHighB; } 
            set { koeffEnterHighB = value; }
        }

        private bool closeOtherSides = true;
        [PropertyXMLTag("Robot.CloseOtherSides")]
        [DisplayName("Закрывать сделки")]
        [Category("Основные")]
        [Description("Закрывать сделки противоположного направления")]
        [PropertyOrder(21, 1)]
        public bool CloseOtherSides
        {
            get { return closeOtherSides; }
            set { closeOtherSides = value; }
        }

        [DisplayName("Направление входа")]
        [Category("Основные")]
        [Description("Направление возможного входа в рынок")]
        [RuntimeAccess(true)]
        public string Side
        {
            get { return side > 0 ? "BUY" : side < 0 ? "SELL" : "-"; }
        }

        private bool priceBwasUpdated;

        [DisplayName("Цена B обновлена")]
        [Description("Цена B была обновлена с момента запуска робота")]
        [Category("Текущие")]
        [RuntimeAccess(true)]
        public bool PriceBWasUpdated
        {
            get { return priceBwasUpdated; }
        }

        [DisplayName("Текущий уровень")]
        [Description("Уровень коррекции-расширения для входа в рынок")]
        [Category("Текущие")]
        [RuntimeAccess(true)]
        public string LevelString
        {
            get { return (PriceB + (decimal)KoefEnter * (PriceA - PriceB)).ToStringUniformPriceFormat(); }
        }

        private int minPointsToApplyMoreFibo = 40;
        [DisplayName("Мин. пунктов Фибо-II")]
        [Description("Пунктов для отсчета еще одного Фибоначчи")]
        [Category("Основные")]
        [PropertyXMLTag("MinPointsToApplyMoreFibo")]
        [RuntimeAccess(true)]
        [PropertyOrder(32, 1)]
        public int MinPointsToApplyMoreFibo
        {
            get { return minPointsToApplyMoreFibo; }
            set { minPointsToApplyMoreFibo = value; }
        }

        private decimal moreFiboLevel = 1.618M;
        [DisplayName("Уровень Фибо-II")]
        [Description("Уровень еще одного Фибоначчи")]
        [Category("Основные")]
        [PropertyXMLTag("MoreFiboLevel")]
        [RuntimeAccess(true)]
        [PropertyOrder(33, 1)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal MoreFiboLevel
        {
            get { return moreFiboLevel; }
            set { moreFiboLevel = value; }
        }

        #endregion

        #region Правила повторного входа

        private int minPointsForDoubleEnter = 4;
        [DisplayName("Мин. пунктов от сделки")]
        [Description("Целый свод бизнес-правил отталкивается от этого параметра")]
        [Category("Противо. вход")]
        [PropertyXMLTag("MinPointsForDoubleEnter")]
        [RuntimeAccess(true)]
        [PropertyOrder(31, 2)]
        public int MinPointsForDoubleEnter
        {
            get { return minPointsForDoubleEnter; }
            set { minPointsForDoubleEnter = value; }
        }

        [DisplayName("Точка A (время)")]
        [Description("Время в точке A")]
        [Category("Противо. вход")]
        [PropertyXMLTag("TimeOfA")]
        [PropertyOrder(10, 2)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime TimeOfA { get; set; }

        [DisplayName("Точка B (время)")]
        [Description("Время в точке B")]
        [Category("Противо. вход")]
        [PropertyXMLTag("TimeOfB")]
        [PropertyOrder(10, 2)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime TimeOfB { get; set; }

        [DisplayName("Противоположный вход")]
        [Description("Входить повторно по завершении серии сделок, в обратном направлении")]
        [Category("Противо. вход")]
        [PropertyXMLTag("DoubleEnter")]
        [RuntimeAccess(true)]
        [PropertyOrder(11, 2)]
        public bool DoubleEnter { get; set; }

        private decimal koefVersaEnterLowA = 0.38M;
        [DisplayName("Уровень входа 1")]
        [Description("Уровень \"коррекции\" или \"расширения\" для противо. входа в рынок")]
        [Category("Противо. вход")]
        [PropertyXMLTag("Robot.KoefEnterVersaLowA")]
        [RuntimeAccess(true)]
        [PropertyOrder(12, 2)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoefVersaEnterLowA
        {
            get { return koefVersaEnterLowA; }
            set { koefVersaEnterLowA = value; }
        }

        private decimal koefVersaEnterLowB = 0.618M;
        [DisplayName("Уровень входа 2")]
        [Description("Уровень \"коррекции\" или \"расширения\" для противо. входа в рынок")]
        [Category("Противо. вход")]
        [PropertyXMLTag("Robot.BoefVersaEnterLowB")]
        [RuntimeAccess(true)]
        [PropertyOrder(13, 2)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoefVersaEnterLowB
        {
            get { return koefVersaEnterLowB; }
            set { koefVersaEnterLowB = value; }
        }

        private decimal koefVersaEnterHighA = 1.618M;
        [DisplayName("Уровень входа 3")]
        [Description("Уровень \"коррекции\" или \"расширения\" для противо. входа в рынок")]
        [Category("Противо. вход")]
        [PropertyXMLTag("Robot.KoefVersaEnterHighA")]
        [RuntimeAccess(true)]
        [PropertyOrder(14, 2)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoefVersaEnterHighA
        {
            get { return koefVersaEnterHighA; }
            set { koefVersaEnterHighA = value; }
        }

        private decimal koefVersaEnterHighB = 2.618M;
        [DisplayName("Уровень входа 4")]
        [Description("Уровень \"коррекции\" или \"расширения\" для противо. входа в рынок")]
        [Category("Противо. вход")]
        [PropertyXMLTag("Robot.KoefVersaEnterHighA")]
        [RuntimeAccess(true)]
        [PropertyOrder(15, 2)]
        [DisplayFormat(DataFormatString = "{0:F3}")]
        public decimal KoefVersaEnterHighB
        {
            get { return koefVersaEnterHighB; }
            set { koefVersaEnterHighB = value; }
        }
        #endregion

        #region Правила трейлинга
        private float koefProtect = 0.38f;
        [DisplayName("Уровень \"защиты\"")]
        [Description("Уровень \"коррекции\" или \"расширения\" для \"защиты\" сделки")]
        [Category("Правила входа")]
        [PropertyXMLTag("Robot.koefProtect")]
        [PropertyOrder(10, 3)]
        public float KoefProtect
        {
            get { return koefProtect; }
            set { koefProtect = value; }
        }

        private float koefProtectDoubleEnter = 0.38f;
        [DisplayName("Уровень \"защиты\"")]
        [Description("Уровень \"коррекции\" или \"расширения\" для \"защиты\" сделки")]
        [Category("Противо. вход")]
        [PropertyXMLTag("Robot.koefProtectDoubleEnter")]
        [PropertyOrder(20, 2)]
        public float KoefProtectDoubleEnter
        {
            get { return koefProtectDoubleEnter; }
            set { koefProtectDoubleEnter = value; }
        }

        private int protectPoints = 10;
        [DisplayName("Цель \"защиты\", пункты")]
        [Description("Перенос стоп-ордера при \"защите\" сделки")]
        [Category("Трейлинг")]
        [PropertyXMLTag("Robot.ProtectPoints")]
        public int ProtectPoints
        {
            get { return protectPoints; }
            set { protectPoints = value; }
        }

        private float koefTrailing = 0.68f;
        [DisplayName("Уровень \"трейлинга\"")]
        [Description("Уровень \"коррекции\" или \"расширения\" для трейлинга по сделке")]
        [Category("Трейлинг")]
        [PropertyXMLTag("Robot.KoefTrailing")]
        public float KoefTrailing
        {
            get { return koefTrailing; }
            set { koefTrailing = value; }
        }

        private float koefTakeprofit = 1.618f;
        [DisplayName("Уровень TakeProfit")]
        [Description("Уровень \"коррекции\" или \"расширения\" для закрытия сделки с прибылью")]
        [Category("Трейлинг")]
        [PropertyXMLTag("Robot.KoefTakeprofit")]
        public float KoefTakeprofit
        {
            get { return koefTakeprofit; }
            set { koefTakeprofit = value; }
        }

        private bool protectAllOrdersByStrictPrice = true;
        [DisplayName("Защищать по первой")]
        [Description("Переносить SL \"защищенных\" сделок на \"лучшую\" цену выхода")]
        [Category("Трейлинг")]
        [PropertyXMLTag("Robot.ProtectAllOrdersByStrictPrice")]
        public bool ProtectAllOrdersByStrictPrice
        {
            get { return protectAllOrdersByStrictPrice; }
            set { protectAllOrdersByStrictPrice = value; }
        }
        #endregion

        #region Сигналы

        private bool verboseLogging = true;

        [DisplayName("Подробный лог")]
        [Description("Подробная запись в лог событий в работе робота")]
        [Category("Сигналы")]
        [PropertyXMLTag("VerboseLogging")]
        public bool VerboseLogging
        {
            get { return verboseLogging; }
            set { verboseLogging = value; }
        }

        [DisplayName("Пресигналы")]
        [Description("Отправлять пресигналы")]
        [Category("Сигналы")]
        [PropertyXMLTag("ShouldMakePresignals")]
        public bool ShouldMakePresignals { get; set; }

        private string signalUrl = "http://forexinvest.com/api/put_presignals";
        [DisplayName("Сервис пресигналов")]
        [Description("URL-адрес сервиса размещения пресигналов")]
        [Category("Сигналы")]
        [PropertyXMLTag("SignalUrl")]
        //[ValueList(true, "http://forexinvest.com/api/put_presignals", "smth else")] // ValueListAttribute usage example for string values
        public string SignalUrl
        {
            get { return signalUrl; }
            set { signalUrl = value; }
        }

        [DisplayName("Пресиг. на сайт")]
        [Description("Отправлять пресигналы на сайт")]
        [Category("Сигналы")]
        [PropertyXMLTag("SendPresignalsOnSite")]
        public bool SendPresignalsOnSite { get; set; }

        private int[] presignalMinutesToClose = new[] { 1440, 15 };

        [DisplayName("Интервалы пресиг.")]
        [Description("Время до выхода пресигнала от закрытия свечи, мин.")]
        [Category("Сигналы")]
        [PropertyXMLTag("PresignalMinutesToClose")]
        public string PresignalIntervalsMin
        {
            get { return string.Join(", ", presignalMinutesToClose); }
            set
            {
                var nums = string.IsNullOrEmpty(value) ? new int[0] : value.ToIntArrayUniform().OrderByDescending(n => n).ToArray();
                if (nums.Length > 0)
                    presignalMinutesToClose = nums;
            }
        }
        #endregion

        #region Run-time свойства
        [DisplayName("След. пресигнал")]
        [Description("Время след. пресигнала")]
        [Category("Текущие")]
        [RuntimeAccess(true)]
        public string TimeOfNextPresignal
        {
            get
            {
                if (robotContext == null) return "-";
                if (currentPresignalPrimeTimes == null || currentPresignalPrimeTimes.Count == 0) return "-";
                var time = (packer.CandleCloseTime.AddMinutes(-currentPresignalPrimeTimes[0]));
                return time.ToString("HH:mm:ss");
            }
        }

        [DisplayName("Объем входа")]
        [Description("Расчетные объемы входа")]
        [Category("Money Management")]
        [RuntimeAccess(true)]
        public string EnterVolumes
        {
            get
            {
                if (FixedVolume.HasValue && FixedVolume > 0)
                    return FixedVolume.Value.ToStringUniformMoneyFormat();

                if (!Leverage.HasValue || Leverage <= 0) return "-";

                var equity = 100000M;
                var depoCurx = "USD";
                if (robotContext != null)
                {
                    equity = robotContext.AccountInfo.Equity;
                    depoCurx = robotContext.AccountInfo.Currency;
                }
            

                var allTickers = DalSpot.Instance.GetTickerNames();
                var ticker = allTickers.Contains("EURUSD") ? "EURUSD" : allTickers[0];

                var volumeStart = (int) Math.Round(equity * Leverage.Value);
                var lotStep = DalSpot.Instance.GetMinStepLot(ticker, robotContext == null
                                                                 ? "Demo" : robotContext.AccountInfo.Group);
                var volStartRound = MarketOrder.RoundDealVolume(volumeStart, RoundType, lotStep.a, lotStep.b);
                var volMax = (int) Math.Round(volumeStart + leverageStep * equity * (maxDealsInSeries - 1));
                var volMaxRound = MarketOrder.RoundDealVolume(volMax, RoundType, lotStep.a, lotStep.b);
                var volmStep = (int) Math.Round(leverageStep*equity);

                return string.Format("Депо. {0} {1}, {2}({3}) .. {4}, шаг {5}",
                                     equity.ToStringUniformMoneyFormat(false),
                                     depoCurx,
                                     volStartRound.ToStringUniformMoneyFormat(),
                                     ticker,
                                     volMaxRound.ToStringUniformMoneyFormat(),
                                     volmStep.ToStringUniformMoneyFormat());
            }
        }

        private string currentOrdersState;
        [DisplayName("Открытые сделки")]
        [Description("Сделки, открытые роботом")]
        [Category("Текущие")]
        [RuntimeAccess(true)]
        public string CurrentOrdersState
        {
            get { return currentOrdersState; }
        }

        private string currentLevelString;
        [DisplayName("Уровень входа")]
        [Description("Уровень входа с учетом открытых сделок")]
        [Category("Текущие")]
        [RuntimeAccess(true)]
        public string CurrentLevelString
        {
            get { return currentLevelString; }
        }

        #endregion

        #region Скрываемые свойства
        [Browsable(false)]
        public override int RoundMinVolume { get { return 0; } set { } }

        [Browsable(false)]
        public override int RoundVolumeStep { get { return 0; } set { } }
        #endregion

        #region Служебные переменные

        private BarSettings timeframe;
        
        /// <summary>
        /// 0 - нет входов, 1 - цена B больше A (покупки), -1 - цена B меньше A (продажи)
        /// </summary>
        private volatile int side;

        private List<int> currentPresignalPrimeTimes;

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int LogMsgErrorPresignal = 1;

        private int authoredTradeSignalCategory;

        private bool firstRealTimeQuoteIsProcessed;

        #endregion

        #region Служебные переменные - определение волатильности

        private const int minutesBetweenMeasure = 20;

        private int minuteCandlesCount;

        private double minuteCandlesBodiesSum;

        private double lastBid;

        private DateTime timeOfLastCandle;

        #endregion

        private readonly List<RobotHint> recentRobotHints = new List<RobotHint>();

        public override BaseRobot MakeCopy()
        {
            var bot = new FiboLevelRobot
            {
                Leverage = Leverage,
                LeverageStep = LeverageStep,
                PriceA = PriceA,
                PriceB = PriceB,
                koefEnter = koefEnter,
                MaxDealsInSeries = MaxDealsInSeries,
                CloseOtherSides = CloseOtherSides,
                koefProtect = koefProtect,
                ProtectPoints = ProtectPoints,
                koefTrailing = koefTrailing,
                koefTakeprofit = koefTakeprofit,
                FixedVolume = FixedVolume,
                RoundType = RoundType,
                priceBwasUpdated = priceBwasUpdated,
                side = side,
                ShouldMakePresignals = ShouldMakePresignals,
                SignalUrl = SignalUrl,
                SendPresignalsOnSite = SendPresignalsOnSite,
                PresignalIntervalsMin = PresignalIntervalsMin,
                ticker = ticker,
                NewsChannels = NewsChannels,
                RoundMinVolume = RoundMinVolume,
                RoundVolumeStep = RoundVolumeStep,
                DoubleEnter = DoubleEnter,
                TimeOfB = TimeOfB,
                TimeOfA = TimeOfA,
                KoeffEnterLowB = KoeffEnterLowB,
                KoeffEnterHighA = KoeffEnterHighA,
                KoeffEnterHighB = KoeffEnterHighB,
                MinPointsForDoubleEnter = MinPointsForDoubleEnter,
                VerboseLogging = VerboseLogging,
                MinPointsToApplyMoreFibo = MinPointsToApplyMoreFibo,
                MoreFiboLevel = MoreFiboLevel,
                KoefVersaEnterLowA = KoefVersaEnterLowA,
                KoefVersaEnterLowB = KoefVersaEnterLowB,
                KoefVersaEnterHighA = KoefVersaEnterHighA,
                KoefVersaEnterHighB = KoefVersaEnterHighB,
                KoefProtectDoubleEnter = KoefProtectDoubleEnter,
                ProtectAllOrdersByStrictPrice = ProtectAllOrdersByStrictPrice
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (string.IsNullOrEmpty(ticker)) return null;

            // определить, когда была открыта последняя сделка робота
            // нужно знать все свечи на интервале от этого входа до момента старта
            var filter = new OrderFilterAndSortOrder
            {
                filterExpertComment = RobotNamePreffix,
                filterTicker = ticker,
                filterMagic = Magic,
                sortAscending = false,
                sortByTimeEnter = true,
                takeCount = 1
            };
            List<MarketOrder> ordersClosed;
            robotContext.GetOrdersByFilter(robotContext.AccountInfo.ID, true, filter, out ordersClosed);
            List<MarketOrder> orders;
            robotContext.GetOrdersByFilter(robotContext.AccountInfo.ID, false, filter, out orders);
            var ordersTotal = (ordersClosed ?? new List<MarketOrder>()).Union(orders ?? new List<MarketOrder>());
            var lastOrder = ordersTotal.OrderByDescending(o => o.TimeEnter).FirstOrDefault();

            var requestStart = startTrade;
            if (lastOrder != null && lastOrder.TimeEnter < requestStart)
                requestStart = lastOrder.TimeEnter;
            // дать запас времени для точного формирования свечи
            requestStart = requestStart.AddMinutes(-1024);

            return new Dictionary<string, DateTime> { { ticker, requestStart } };
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("FiboLevelRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("FiboLevelRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;
            timeframe = Graphics[0].b;
            packer = new CandlePacker(timeframe);
            // расчет вспомогательных переменных
            side = PriceB > PriceA ? 1 : PriceB < PriceA ? -1 : 0;

            minuteCandlesCount = 0;
            minuteCandlesBodiesSum = 0;
            lastBid = 0;
            timeOfLastCandle = new DateTime(1972, 1, 1);

            // получить категории торговых сигналов
            if (ShouldMakePresignals)
            {
                currentPresignalPrimeTimes = presignalMinutesToClose.ToList();
                var signals = grobotContext is BacktestServerProxy.RobotContextLive
                                  ? ((BacktestServerProxy.RobotContextLive)grobotContext).GetAuthoredTradeSignals()
                                  : new List<PaidService>();

                if (signals.Count > 0)
                {
                    var signal = signals[0];
                    if (signal != null)
                    {
                        authoredTradeSignalCategory = signal.Id;
                        Logger.InfoFormat("Робот {0} - отправляет сигналы {1} ({2})",
                            GetUniqueName(), authoredTradeSignalCategory, signal.Comment);
                    }
                }
            }

            // поставить на графике точечки
            AddRobotHintOnPriceUpdated("A", 0, priceA, TimeOfA);
            AddRobotHintOnPriceUpdated("B", 0, priceB, TimeOfB);
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            // получить котировку по наблюдаемому тикеру
            CandleDataBidAsk quote = null;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] != ticker) continue;
                quote = quotes[i];
                break;
            }
            var events = recentRobotHints.Select(h => h.ToString()).ToList();
            recentRobotHints.Clear();
            if (quote == null) return events;

            // обновить свечку
            var candle = packer.UpdateCandle(quote);
            if (candle != null)
                candles.Add(candle);

            // обновить данные по волатильности
            if (lastBid == 0)
                lastBid = quote.close;
            else
            {
                if ((quote.timeClose - timeOfLastCandle).TotalMinutes >= minutesBetweenMeasure)
                {
                    var delta = quote.close - lastBid;
                    lastBid = quote.close;
                    minuteCandlesCount++;
                    minuteCandlesBodiesSum += (delta * delta);
                    timeOfLastCandle = quote.timeClose;
                }
            }

            if (isHistoryStartOff) return events;

            // сопроводить сделки
            List<MarketOrder> orders;
            GetMarketOrders(out orders, true);
            orders = orders.Where(o => o.Symbol == ticker).ToList();
            var robotOrders = FiboRobotPosition.GetRobotPositions(ticker, orders, timeframe, null, null);
            orders = robotOrders.Select(o => o.order).ToList();

            // показать черточками уровни входа
            if (!firstRealTimeQuoteIsProcessed || candle != null)
                MakeRobotEnterPricesLines(quote, events, orders);
            firstRealTimeQuoteIsProcessed = true;

            UpdateCurrentOrderStateString(robotOrders, quote);
            
            if (robotOrders.Count > 0)
                CheckDeals(robotOrders);

            MakePresignals(candle != null, quote.timeClose, orders);

            if (string.IsNullOrEmpty(currentLevelString) || candle != null)
                currentLevelString = GetCurrentLevel();

            if (candle == null) return events;

            // открыть новые сделки?
            var dealOpened = CheckEnterCondition(candle, orders, events, isHistoryStartOff);
            if (!dealOpened && DoubleEnter)
                CheckVersaEnterCondition(candle, orders, events);

            return events;
        }

        public override string ReportState()
        {
            if (PriceA == PriceB)
                return "Не инициализирован";

            if (robotContext == null)
                return "Уровень " +
                       (PriceA > PriceB ? "продажи" : "покупки") + ": " + GetCurrentLevel();

            return CurrentOrdersState + "\n" + GetCurrentLevel();
        }

        public void MakePresignals(bool madeNewCandle, DateTime time, List<MarketOrder> orders)
        {
            if (!ShouldMakePresignals || PriceA == 0 || PriceB == 0) return;

            // "пресигналы" на вход
            if (madeNewCandle)
            {
                currentPresignalPrimeTimes = presignalMinutesToClose.ToList();
                return;
            }
            if (currentPresignalPrimeTimes.Count == 0) return;

            var minutesToClose = (int)Math.Round((packer.CandleCloseTime - time).TotalMinutes);
            if (minutesToClose > currentPresignalPrimeTimes[0]) return;
            currentPresignalPrimeTimes.RemoveAt(0);

            var level = PriceB + KoefEnter * (PriceA - PriceB);

            // уточнить уровень по открытым позам
            // может, уже есть позы, открытые "лучше"
            var deals = FiboRobotPosition.GetRobotPositions(ticker, orders, timeframe, PriceA, null);

            if (deals.Count > 0)
            {
                var priceLast = side > 0 ? deals.Min(d => d.order.PriceEnter) : deals.Max(d => d.order.PriceEnter);
                if ((side > 0 && priceLast < (float)level) || (side < 0 && priceLast > (float)level))
                    level = (decimal)priceLast;
            }

            var signalText = string.Format(
                "Планирую {0} {1} по цене {2} {3} в {4:HH:mm}",
                side > 0 ? "купить" : "продать",
                ticker,
                side > 0 ? "ниже" : "выше",
                DalSpot.Instance.FormatPrice(ticker, level),
                packer.CandleCloseTime);

            var acEvent = new UserEvent
            {
                //Account = robotContext.accountInfo.ID,
                Action = AccountEventAction.DefaultAction,
                Code = AccountEventCode.TradeSignal,
                Text = string.Join("#-#", ticker, timeframe.ToString(), signalText),
                Time = time,
                Title = "Торговый сигнал"
            };

            if (authoredTradeSignalCategory != 0)
            {
                try
                {
                    robotContext.SendTradeSignalEvent(
                        protectedContext.MakeProtectedContext(),
                        robotContext.AccountInfo.ID,
                        authoredTradeSignalCategory,
                        acEvent);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка отправки \"пресигнала\"", ex);
                }
            }

            // отправить "пресигнал" на сайт
            if (SendPresignalsOnSite)
                PostSignalOnSite(side, level, packer.CandleCloseTime);
        }

        private void UpdateCurrentOrderStateString(List<FiboRobotPosition> orders, CandleDataBidAsk quote)
        {
            var ownOrders =
                orders.Where(o => o.order.Side == side &&
                    o.PriceA.RoughCompares(PriceA, DalSpot.Instance.GetAbsValue(ticker, 1.5M)))
                      .Select(o => o.order).ToList();

            var stateString = ownOrders.Count + " сделок открыто из " + MaxDealsInSeries;
            if (ownOrders.Count > 0)
            {
                stateString += "\nПо ценам от " + ownOrders.Min(o => o.PriceEnter).ToStringUniformPriceFormat() +
                               " до " + ownOrders.Max(o => o.PriceEnter).ToStringUniformPriceFormat();
                var points = DalSpot.Instance.GetPointsValue(ticker, ownOrders.Sum(o => o.Side > 0
                                                                                         ? quote.close - o.PriceEnter
                                                                                         : o.PriceEnter - quote.closeAsk));
                stateString += "\nРезультат: " +
                               ownOrders.Sum(o => o.ResultDepo).ToStringUniformMoneyFormat(false) + " " +
                               robotContext.AccountInfo.Currency + ", " + points.ToStringUniform(0) + "пп";

                var exposure = ownOrders.Sum(o => o.Side * o.Volume);
                if (exposure != 0)
                {
                    var sumSide = exposure < 0 ? "SELL" : "BUY";
                    stateString += "\n" + sumSide + " " + exposure.ToStringUniformMoneyFormat() + " " + ticker;
                }
            }

            var verseOrders = orders.Where(o => o.order.Side != side && o.Sequence == 2).Select(o => o.order).ToList();
            if (verseOrders.Count > 0)
            {
                stateString += "\nОткрыто " + verseOrders.Count + " обратных ордера";
                stateString += "\nПо ценам от " + verseOrders.Min(o => o.PriceEnter).ToStringUniformPriceFormat() +
                               " до " + verseOrders.Max(o => o.PriceEnter).ToStringUniformPriceFormat();
            }

            currentOrdersState = stateString;
        }

        private void MakeRobotEnterPricesLines(CandleDataBidAsk quote, List<string> events, List<MarketOrder> orders)
        {
            var levels = MakeLevelsPriceTitle();
            var colors = new [] { Color.Chocolate, Color.Chocolate, Color.LightSeaGreen, Color.LightSeaGreen };
            var colorIndex = 0;

            // приказ - удалить старые линии
            events.Add(new RobotMarkClear(ticker, timeframe, HintCodePriceLevel)
                {
                    RobotHintType = RobotMark.HintType.Линия
                }.ToString());

            // новые отметки входов
            foreach (var level in levels)
            {
                var hint = MakeRobotComment(level.a, level.b, quote.timeClose, colors[colorIndex++]);
                hint.HintCode = HintCodePriceLevel;
                events.Add(hint.ToString());
            }

            // отметки противо-входов
            List<FiboRobotPosition> allOrders;
            FiboRobotPosition lastOrder;
            decimal a, b;
            if (GetVersaEnterAandB(orders, out allOrders, out lastOrder, out a, out b))
            {
                var koeffs = new[] {KoefVersaEnterLowA, KoefVersaEnterLowB, KoefVersaEnterHighA, KoefVersaEnterHighB};
                foreach (var koeff in koeffs)
                {
                    if (koeff <= 0) break;
                    var price = b + (a - b) * koeff;
                    var hint = MakeRobotComment((float) price,
                        "обратный: " + koeff.ToString("f3") + ", от ордера " + lastOrder.order.ID + 
                        ",\nA: " + a.ToStringUniformPriceFormat() + 
                        ", B: " + b.ToStringUniformPriceFormat(), 
                        quote.timeClose, Color.DarkGray);
                    hint.HintCode = HintCodePriceLevel;
                    events.Add(hint.ToString());
                }
            }
        }

        private List<Cortege2<float, string>> MakeLevelsPriceTitle()
        {
            var levels = new List<Cortege2<float, string>>();
            var koeffs = new[] { KoefEnter, KoeffEnterLowB, KoeffEnterHighA, KoeffEnterHighB };
            foreach (var k in koeffs)
            {
                if (k <= 0) break;
                levels.Add(MakePriceLineHintRecord(k));
            }

            return levels;
        }

        private Cortege2<float, string> MakePriceLineHintRecord(decimal level)
        {
            var price = (float) (priceB + (priceA - priceB) * level);
            return new Cortege2<float, string>(price, GetUniqueName() + " - уровень " + (side > 0 ? "BUY " : "SELL ") +
                level.ToStringUniformPriceFormat() + " / " + price.ToStringUniformPriceFormat());
        }

        private List<MarketOrder> GetLastClosedOrders()
        {
            var filter = new OrderFilterAndSortOrder
            {
                filterExpertComment = RobotNamePreffix,
                filterMagic = Magic,
                takeCount = MaxDealsInSeries + 1,
                sortAscending = false,
                sortByTimeEnter = true,
                filterTicker = ticker
            };

            List<MarketOrder> ordersClosed;
            robotContext.GetOrdersByFilter(robotContext.AccountInfo.ID, true, filter, out ordersClosed);
            return ordersClosed ?? new List<MarketOrder>();
        }

        private void PostSignalOnSite(int signalSide, decimal price, DateTime timeOfDeal)
        {
            try
            {
                var avgCandle = minuteCandlesCount == 0
                                    ? 0
                                    : Math.Sqrt(minuteCandlesBodiesSum / minuteCandlesCount);
                avgCandle = (float)(avgCandle / Math.Pow(minutesBetweenMeasure, 0.5));

                //var quote = QuoteStorage.Instance.ReceiveValue(ticker);
                //var probability = ProbabilityCore.CalculateLevelProb(avgCandle,
                //                                                    (timeOfDeal - DateTime.Now).TotalMinutes,
                //                                                    price, quote.bid, signalSide > 0);

                var msg = string.Format("{{ \"ticker\": \"{0}\", \"side\": \"{1}\", " +
                                        " \"condition\": \"{2} {3}\", \"time\": \"{4:dd.MM.yyyy HH:mm:ss}\", " +
                                        "\"minuteCandle\": \"{5}\", \"shouldBeLess\": \"{6}\", \"target\": \"{7}\", \"account\": \"{8}\" }}",
                                        ticker, signalSide > 0 ? "BUY" : "SELL",
                                        signalSide > 0 ? "Цена ниже" : "Цена выше",
                                        price.ToStringUniformPriceFormat(),
                                        timeOfDeal,
                                        avgCandle.ToStringUniform(5),
                                        signalSide > 0,
                                        price.ToStringUniform(5),
                                        robotContext.AccountInfo.ID);

                var url = signalUrl + (signalUrl.Contains('?') ? "&" : "?") +
                          "jsn=" + msg;
                var req = WebRequest.Create(url);
                req.Method = "GET";

                Logger.Info("Пресигнал отправлен на сайт (" + msg + "), url: " + url);

                req.GetResponse();
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                      LogMsgErrorPresignal, 1000 * 60 * 60,
                                                      "Ошибка в PostSignalOnSite: " + ex);
            }
            //string strData;
            //var resp = req.GetResponse();
            //using (var stream = resp.GetResponseStream())
            //{
            //    if (stream == null) return;
            //    using (var reader = new StreamReader(stream, Encoding.UTF8))
            //    {
            //        strData = reader.ReadToEnd();
            //        if (string.IsNullOrEmpty(strData)) return;
            //    }
            //}
        }

        private void CheckDeals(List<FiboRobotPosition> fiboDeals)
        {
            // наивысший TP для покупки и низший для продажи
            var maxTp = new Dictionary<int, float?>
                {
                    {1, null},
                    {-1, null}
                };

            foreach (var fiboDeal in fiboDeals)
            {
                // проверить трейлинги и TP
                // должно быть не менее двух уровней трейлинга и уровень TP
                if (fiboDeal.order.trailingLevels[0] != 0 &&
                    fiboDeal.order.trailingLevels[1] != 0 && (fiboDeal.order.TakeProfit ?? 0) > 0)
                    continue;

                // трейлинг-1
                var kProtect = fiboDeal.Sequence == 1 ? KoefProtect : KoefProtectDoubleEnter;
                if (kProtect > 0.00001)
                {
                    var trailLevel1 = fiboDeal.order.PriceEnter + ((float)fiboDeal.PriceB - fiboDeal.order.PriceEnter) * kProtect;
                    var trailLevelPips1 =
                        Math.Abs(DalSpot.Instance.GetPointsValue(ticker, trailLevel1 - fiboDeal.order.PriceEnter));
                    fiboDeal.order.trailingLevels[0] = trailLevelPips1;
                    fiboDeal.order.trailingTargets[0] = protectPoints;

                    // трейлинг-2
                    if (koefTrailing > 0.00001)
                    {
                        var trailLevel2 = fiboDeal.order.PriceEnter + ((float)fiboDeal.PriceB - fiboDeal.order.PriceEnter) * koefTrailing;
                        var trailLevelPips2 =
                            Math.Abs(DalSpot.Instance.GetPointsValue(ticker, trailLevel2 - fiboDeal.order.PriceEnter));
                        fiboDeal.order.trailingLevels[1] = trailLevelPips2;
                        fiboDeal.order.trailingTargets[1] = trailLevelPips1;
                    }
                }

                // тейк
                if (KoefTakeprofit > 0.00001)
                {
                    fiboDeal.order.TakeProfit = fiboDeal.order.PriceEnter + ((float)fiboDeal.PriceB - fiboDeal.order.PriceEnter) * KoefTakeprofit;
                    if (!maxTp[fiboDeal.order.Side].HasValue ||
                        (fiboDeal.order.Side == 1 && maxTp[fiboDeal.order.Side].Value < fiboDeal.order.TakeProfit) ||
                        (fiboDeal.order.Side == -1 && maxTp[fiboDeal.order.Side].Value > fiboDeal.order.TakeProfit))
                        maxTp[fiboDeal.order.Side] = fiboDeal.order.TakeProfit;
                }

                var msg = string.Format("#{0} ({1} {2} {3} at {4:f4}), A-B: {5}. trailings are: " +
                                        "[{6:f4}-{7:f4}, {8:f4}-{9:f4}], TP is {10:f4}",
                                        fiboDeal.order.ID, fiboDeal.order.Side > 0 ? "BUY" : "SELL", fiboDeal.order.Volume, fiboDeal.order.Symbol,
                                        fiboDeal.order.PriceEnter, fiboDeal.order.Comment,
                                        fiboDeal.order.trailingLevels[0], fiboDeal.order.trailingTargets[0],
                                        fiboDeal.order.trailingLevels[1], fiboDeal.order.trailingTargets[1],
                                        fiboDeal.order.TakeProfit);
                Logger.Info(msg);

                robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), fiboDeal.order);
            }

            // перетащить тейки всех ордеров на крайнее (наибольшее в пунктах) значение
            if (fiboDeals.Count > 1 && maxTp.Values.Any(v => v.HasValue))
            {
                foreach (var order in fiboDeals.Where(o => o.order.Magic == Magic))
                {
                    var tp = maxTp[order.order.Side];
                    if (!tp.HasValue) continue;
                    if (!order.order.TakeProfit.HasValue ||
                        (order.order.Side > 0 && order.order.TakeProfit < tp) ||
                        (order.order.Side < 0 && order.order.TakeProfit > tp))
                    // редактировать ордер
                    {
                        order.order.TakeProfit = tp;
                        robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), order.order);
                    }
                }
            }

            // перетащить стоп на одну цену для "защищенных" сделок
            if (ProtectAllOrdersByStrictPrice)
            {
                var sides = new [] {1, -1};

                foreach (var dealSide in sides)
                {
                    var buysOrSells = fiboDeals.Where(d => d.order.Side == dealSide).ToList();
                    if (buysOrSells.Count <= 0) continue;

                    var bestPrice = dealSide > 0 ? buysOrSells.Max(b => b.order.StopLoss) : buysOrSells.Min(b => b.order.StopLoss);
                    if (!bestPrice.HasValue) continue;

                    var targetSide = dealSide;
                    foreach (var deal in buysOrSells.Where(d =>
                        targetSide > 0 ? d.order.StopLoss > d.order.PriceEnter : d.order.StopLoss < d.order.PriceEnter))
                    {
                        var delta = DalSpot.Instance.GetPointsValue(ticker, 
                            targetSide > 0 
                                ? bestPrice.Value - deal.order.StopLoss.Value
                                : deal.order.StopLoss.Value - bestPrice.Value);
                        if (delta < 1) continue;
                        
                        if (VerboseLogging)
                        {
                            Logger.InfoFormat("Перенести SL ордера {0} с {1} на \"лучшую\" цену {2}",
                                deal.order.ToStringShort(), deal.order.StopLoss, bestPrice.Value.ToStringUniformPriceFormat(true));
                        }

                        // перетащить стоп
                        deal.order.StopLoss = bestPrice;
                        robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), deal.order);
                    }
                }
            }
        }

        private bool CheckEnterCondition(CandleData candle, List<MarketOrder> openedOrders, List<string> events, bool isHistoryStartOff)
        {
            if (side == 0) return false;

            // проверить количество сделок
            var ownOrders = FiboRobotPosition.GetRobotPositions(ticker, openedOrders, timeframe, PriceA, null);
            var ordersCount = ownOrders.Count;
            if (ordersCount >= MaxDealsInSeries) return false;

            // обновить цену B?
            var bWasUpdated = false;
            if ((side > 0 && candle.close > (float)PriceB) ||
                (side < 0 && candle.close < (float)PriceB))
            {
                // удалить старую отметку - цена В обновлена
                events.Add(new RobotMarkClear(ticker, timeframe, HintPriceBIsUpdated)
                {
                    RobotHintType = RobotMark.HintType.Тейк
                }.ToString());

                // и добавить новую
                var msg = string.Format("Цена \"B\" обновлена с {0} на {1}",
                                        PriceB.ToStringUniformPriceFormat(), candle.close.ToStringUniformPriceFormat());
                var hint = MakeRobotComment(
                    candle.close, msg, 
                    candle.timeClose, Color.Crimson,
                    RobotMark.HintType.Тейк);
                hint.HintCode = HintPriceBIsUpdated;
                events.Add(hint.ToString());
                events.Add(msg);

                PriceB = (decimal)candle.close;
                bWasUpdated = true;
                priceBwasUpdated = true;
            }

            if (isHistoryStartOff) return false;

            // проверить, преодолен ли уровень входа
            var levelBroken =
                IsPriceInsideTargetLevels(candle.close, PriceA, PriceB, false);
            if (!levelBroken) return false;

            var level = PriceB + KoefEnter * (PriceA - PriceB);
            var msgLevelBroken = string.Format("Уровень {0} преодолен, направление - {1}",
                                    level.ToStringUniformPriceFormat(), Side);

            // получить закрытые ордера
            var closedOrders = GetLastClosedOrders();
            var orders = openedOrders.Union(closedOrders).OrderByDescending(o => o.TimeEnter).ToList();
            var openedAndClosedFiboOrders = FiboRobotPosition.GetRobotPositions(ticker, orders, timeframe, PriceA, null);

            // если цена B не была обновлена свечкой, проверить, нет ли сделок
            // по данному расширению A-B, открытых "лучше" этой цены
            if (!bWasUpdated)
            {
                if (openedAndClosedFiboOrders.Count > 0)
                {
                    if ((side > 0 && openedAndClosedFiboOrders.Any(d => d.order.PriceEnter < candle.close)) ||
                        (side < 0 && openedAndClosedFiboOrders.Any(d => d.order.PriceEnter > candle.close)))
                    {
                        msgLevelBroken += ", есть сделки по \"лучшей\" цене";
                        events.Add(msgLevelBroken);
                        return false;
                    }
                }
            }

            // второй уровень Фибо преодолен?
            if (!CheckEnterThenFiboCondition(candle, side, openedAndClosedFiboOrders.Select(o => o.order).ToList()))
            {
                // создать отметку на графике и выйти
                var hint = new RobotHint(ticker, timeframe.ToString(),
                    "Уровень ФБ-II не преодолен", "ФБ-II", "Ф", candle.close)
                {
                    Time = candle.timeClose,
                    ColorFill = Color.Coral,
                    ColorLine = Color.DarkRed,
                    RobotHintType = RobotHint.HintType.Коментарий,
                    Timeframe = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe)
                };
                events.Add(hint.ToString());

                return false;
            }

            if (CloseOtherSides)
                CloseOtherSideDeals(ownOrders.Select(o => o.order).ToList());

            msgLevelBroken += ", вход в рынок";
            events.Add(msgLevelBroken);
            // таки войти в рынок
            var enterIsOk = OpenDeal(orders);
            if (enterIsOk && (ordersCount >= maxDealsInSeries - 1))
                events.Add(string.Format("Робот \"FX\" {0} осуществил {1} входов в рынок",
                    HumanRTickers, maxDealsInSeries));
            return true;
        }

        private RobotHint MakeRobotComment(float price, string text, DateTime time,
            Color? colorLine = null, RobotMark.HintType hintType = RobotMark.HintType.Линия)
        {
            var hint = new RobotHint(ticker, BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe), text,
                                     GetUniqueName(), "*", price)
                {
                    Time = time,
                    ColorFill = Color.LightGray,
                    ColorLine = colorLine ?? Color.DarkBlue,
                    RobotHintType = hintType
                };
            return hint;
        }

        /// <summary>
        /// если от последней сделки рынок прошел вверх / вниз N пунктов,
        /// взять от этой дельты проекцию Фибо и проверить, что закрытие текущей свечи выше / ниже
        /// 
        /// иначе вернуть false (входа нет)
        /// </summary>
        private bool CheckEnterThenFiboCondition(CandleData candle, int dealSide, List<MarketOrder> lastOrders)
        {
            var lastOrder =
                lastOrders.Where(o => o.Side == dealSide).OrderByDescending(o => o.TimeEnter).FirstOrDefault();
            if (lastOrder == null) return true;

            // найти мин - макс на этом диапазоне
            var lastCandleCloses = candles.Where(c => c.timeClose > lastOrder.TimeEnter).Select(c => c.close).ToList();
            if (lastCandleCloses.Count == 0) return true;

            var peak = dealSide < 0 ? lastCandleCloses.Min() : lastCandleCloses.Max();
            var delta = dealSide < 0 ? (lastOrder.PriceEnter - peak) : (peak - lastOrder.PriceEnter);
            var deltaPoints = DalSpot.Instance.GetPointsValue(ticker, delta);
            if (deltaPoints < minPointsToApplyMoreFibo)
            {
                if (VerboseLogging)
                    Logger.InfoFormat("Крайняя цена close от сделки " + lastOrder.ToStringShort() + " " +
                        peak.ToStringUniformPriceFormat() + ": разница меньше " + minPointsToApplyMoreFibo + " пунктов");
                return true;
            }

            // отсчитать уровень Фибо и проверить превышение (или нахождение ниже) текущей ценой
            var fiboLevel = dealSide < 0
                                ? peak + delta * (float)moreFiboLevel
                                : peak - delta * (float)moreFiboLevel;
            var enterAllowed = dealSide < 0
                                   ? candle.close >= fiboLevel
                                   : candle.close <= fiboLevel;
            if (!enterAllowed && VerboseLogging)
                Logger.InfoFormat("Крайняя цена close от сделки " + lastOrder.ToStringShort() + " " +
                        peak.ToStringUniformPriceFormat() + ", рассчитан уровень " + fiboLevel.ToStringUniformPriceFormat() +
                        " не преодолен текущей ценой " + candle.close.ToStringUniformPriceFormat());

            return enterAllowed;
        }

        /// <summary>
        /// проверить условия повторного входа (в другом направлении)
        /// </summary>
        private void CheckVersaEnterCondition(CandleData candle, List<MarketOrder> orders, List<string> events)
        {
            List<FiboRobotPosition> allOrders;
            FiboRobotPosition lastOrder;
            decimal a;
            decimal b;
            
            if (!GetVersaEnterAandB(orders, out allOrders, out lastOrder, out a, out b)) return;
            var doubleSide = a < b ? DealType.Buy : DealType.Sell;

            var shouldEnter = IsPriceInsideTargetLevels(candle.close, a, b, true);
            if (!shouldEnter) return;

            // проверить - нет ли повторных ("второй серии") входов по "лучшей" цене?
            // не слишком ли много повторных входов?
            var doubledDeals = allOrders.Where(d => d.Sequence > 1).ToList();
            if (doubledDeals.Count(d => d.order.IsOpened) >= MaxDealsInSeries) return;
            var hasBetterDoubledDeal = doubleSide > 0
                                           ? doubledDeals.Any(d => d.order.PriceEnter < candle.close)
                                           : doubledDeals.Any(d => d.order.PriceEnter > candle.close);
            if (hasBetterDoubledDeal) return;

            // проверить - есть ли N свечек и M пунктов с момента последней сделки?
            var deltaPoints = DalSpot.Instance.GetPointsValue(ticker, Math.Abs((decimal)candle.close - b));
            if (deltaPoints < MinPointsForDoubleEnter)
            {
                if (VerboseLogging)
                    Logger.InfoFormat(RobotNamePreffix + ": повторный вход ({0} {1}) невозможен - расстояние [C, D] слишком мало",
                        doubleSide, ticker);
                return;
            }

            // добавить отметку - повторный вход в рынок
            var hint = new RobotHint(ticker, timeframe.ToString(),
                "Повторный вход, цена A: " + a.ToStringUniformPriceFormat() + ", цена B: " + b.ToStringUniformPriceFormat() +
                " (из сделки #" + lastOrder.order.ID + ")",
                "повторный " + doubleSide, "d", candle.close)
            {
                Time = candle.timeClose,
                ColorFill = Color.LawnGreen,
                ColorLine = Color.MediumSeaGreen,
                RobotHintType = RobotHint.HintType.Коментарий,
                Timeframe = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe)
            };
            events.Add(hint.ToString());

            // войти в рынок повторно
            OpenDeal(orders, a, b, 2, (int)doubleSide);
        }

        private bool GetVersaEnterAandB(List<MarketOrder> orders, 
            out List<FiboRobotPosition> allOrders, 
            out FiboRobotPosition lastOrder, 
            out decimal a, out decimal b)
        {
            a = 0;
            b = 0;
            lastOrder = null;

            // среди закрытых и открытых ордеров найти самый последний из "первой серии"
            List<MarketOrder> closedOrders;
            robotContext.GetHistoryOrders(robotContext.AccountInfo.ID, TimeOfB, out closedOrders);
            closedOrders = closedOrders ?? new List<MarketOrder>();
            closedOrders = closedOrders.Where(o => o.Symbol == ticker && o.Magic == Magic).ToList();

            allOrders = FiboRobotPosition.GetRobotPositions(ticker, orders.Union(closedOrders), timeframe, null, null);
            if (allOrders.Count == 0) return false;

            lastOrder = allOrders.Where(o => o.Sequence == 1).OrderBy(o => o.order.TimeEnter).LastOrDefault();
            if (lastOrder == null) return false;

            // точка входа того ордера станет точкой B, точка B ордера станет точкой А
            a = lastOrder.PriceB;
            b = (decimal) lastOrder.order.PriceEnter;
            
            // точку В надо уточнить - возможно, за ценой входа по последнему ордеру имела место лучшая (худшая) цена?
            b = RefineSecondPriceForDoubleEnter((float) b, lastOrder.order.TimeEnter);
            return true;
        }

        private decimal RefineSecondPriceForDoubleEnter(float lastDealPrice, DateTime lastOrderTimeEnter)
        {
            var timOfFirstCandle = TimeOfB == default(DateTime) ? lastOrderTimeEnter : TimeOfB;

            var dueCandles = candles.Where(c => c.timeOpen >= timOfFirstCandle).ToList();
            if (dueCandles.Count == 0) return (decimal)lastDealPrice;

            var doubleSide = -side;
            var price = doubleSide == -1
                            ? dueCandles.Min(c => Math.Min(c.close, c.open))
                            : dueCandles.Max(c => Math.Min(c.close, c.open));
            if ((doubleSide == -1 && price >= lastDealPrice) ||
                (doubleSide == 1 && price <= lastDealPrice)) return (decimal)lastDealPrice;

            if (VerboseLogging)
                Logger.InfoFormat("Цена В уточнена по {0} свечкам - {1}, цена последнего входа {2}",
                    candles.Count, price.ToStringUniformPriceFormat(),
                    lastDealPrice.ToStringUniformPriceFormat());
            return (decimal) price;
        }

        ///// <summary>
        ///// собрать комментарий сделки из цен A - B вида
        ///// 1.3560;1.3595
        ///// </summary>
        //private string MakeDealComment()
        //{
        //    return DalSpot.Instance.FormatPrice(ticker, PriceA, true) + ";" +
        //           DalSpot.Instance.FormatPrice(ticker, PriceB, true);
        //}

        //private string MakeDealCommentPreffix()
        //{
        //    return DalSpot.Instance.FormatPrice(ticker, PriceA, true) + ";";
        //}

        private void CloseOtherSideDeals(List<MarketOrder> orders)
        {
            foreach (var order in orders)
            {
                if (order.Side != side)
                    CloseMarketOrder(order.ID);
            }
        }

        private bool OpenDeal(List<MarketOrder> orders, decimal dealPriceA, decimal dealPriceB, int sequence, int dealSide)
        {
            var countSame = orders.Count(o => o.Side == dealSide);
            var lever = Leverage + leverageStep * countSame;
            var dealVolumeDepo = CalculateVolume(ticker, lever);
            if (VerboseLogging)
                Logger.InfoFormat(RobotNamePreffix + ": планируемый вход ({0} {1}) отменен - объем входа равен 0",
                    dealSide, ticker);

            if (dealVolumeDepo == 0) return false;

            var newOrder = new MarketOrder
            {
                AccountID = robotContext.AccountInfo.ID,
                Magic = Magic,
                Symbol = ticker,
                Volume = dealVolumeDepo,
                Side = dealSide,
            };
            FiboRobotPosition.MakeOrderComments(newOrder, timeframe, dealPriceA, dealPriceB, sequence);

            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                newOrder,
                OrderType.Market, 0, 0);

            if (VerboseLogging)
                Logger.InfoFormat(RobotNamePreffix + ": запрос на вход ({0} {1}) отправлен", dealSide, ticker);
            return true;
        }

        private bool OpenDeal(List<MarketOrder> orders)
        {
            return OpenDeal(orders, PriceA, PriceB, 1, side);
        }

        private void AddRobotHintOnPriceUpdated(string pricePreffix, 
            decimal priceOld, decimal priceNew, DateTime eventTime)
        {
            if (eventTime == default(DateTime))
                eventTime = DateTime.Now;

            var hint = new RobotHint(ticker, timeframe.ToString(),
                "Цена " + pricePreffix + " обновлена с " + priceOld.ToStringUniformPriceFormat() +
                " на " + priceNew.ToStringUniformPriceFormat(), "цена " + pricePreffix, pricePreffix, (float)priceNew)
            {
                Time = eventTime,
                ColorFill = Color.Moccasin,
                ColorLine = Color.DarkGoldenrod,
                RobotHintType = RobotHint.HintType.Тейк,
                Timeframe = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe)
            };
            recentRobotHints.Add(hint);
        }
    
        private bool IsPriceInsideTargetLevels(float price, decimal a, decimal b, bool doubleEnter)
        {
            if (a.RoughCompares(b, 0.00001M)) return false;
            var deltaPrice = ((decimal)price - b) / (a - b);

            var levelLowA = doubleEnter ? KoefVersaEnterLowA : KoefEnter;
            if (deltaPrice < levelLowA)
                return false;

            // если вторая граница диапазона не указана - достаточно первой проверки
            var levelLowB = doubleEnter ? KoefVersaEnterLowB : KoeffEnterLowB;
            if (levelLowB <= KoefEnter)
                return true;

            if (deltaPrice <= levelLowB)
                return true;

            // в первый диапазон не попали - но, возможно, попали во второй диапазон
            var levelHighA = doubleEnter ? KoefVersaEnterHighA : KoeffEnterHighA;
            var levelHighB = doubleEnter ? KoefVersaEnterHighB : KoeffEnterHighB;

            if (levelHighA <= levelLowB) return false;
            if (levelHighB <= levelHighA) return false;

            return (deltaPrice >= levelHighA && deltaPrice <= levelHighB);
        }

        private string GetCurrentLevel()
        {
            if (robotContext == null)
            {
                // показать просто уровень от цен А - В
                if (priceA > 0 && priceB > 0 && KoefEnter > 0)
                    return (priceB + (priceA - priceB) * koefEnter).ToStringUniformPriceFormat();

                return "-";
            }
            if (PriceA == 0 || PriceB == 0) return "вне рынка";
            var level = PriceB + KoefEnter * (PriceA - PriceB);

            // уточнить уровень по открытым позам
            // может, уже есть позы, открытые "лучше"
            List<MarketOrder> orders;
            GetMarketOrders(out orders, false);
            orders = orders == null ? new List<MarketOrder>() : orders.Where(o => o.Symbol == ticker).ToList();
            var deals = FiboRobotPosition.GetRobotPositions(ticker, orders, timeframe, PriceA, null);
            if (deals.Count > 0)
            {
                var firstDeal = deals[0];
                if ((side > 0) && (firstDeal.order.PriceEnter < (float)level) ||
                        (side < 0) && (firstDeal.order.PriceEnter > (float)level))
                    level = (decimal)firstDeal.order.PriceEnter;
            }

            return GetDealLevelAndRangeString(level);
        }

        private string GetDealLevelAndRangeString(decimal correctedEnterLevel)
        {
            var level = correctedEnterLevel;
            var plannedSide = priceA > priceB ? -1 : 1;
            var mainPart = plannedSide < 0 
                    ? "Продать выше " + level.ToStringUniformPriceFormat(true)
                    : "Купить ниже " + level.ToStringUniformPriceFormat(true);

            if (KoeffEnterLowB == 0)
                return mainPart;

            var koeffPairs = new List<Cortege2<decimal, decimal>>
                {
                    new Cortege2<decimal, decimal>(PriceB + (PriceA - PriceB) * KoefEnter, PriceB + (PriceA - PriceB) * KoeffEnterLowB)
                };

            if (KoeffEnterHighA > 0 && KoeffEnterHighB > 0)
            {
                koeffPairs.Add(new Cortege2<decimal, decimal>(PriceB + (PriceA - PriceB) * KoeffEnterHighA,
                                                              PriceB + (PriceA - PriceB) * KoeffEnterHighB));
            }

            return mainPart + ", " + string.Join(" - ", koeffPairs.Select(p => string.Format("[{0} .. {1}]",
                p.a.ToStringUniformPriceFormat(), p.b.ToStringUniformPriceFormat())));
        }
    }
    // ReSharper restore LocalizableElement
}