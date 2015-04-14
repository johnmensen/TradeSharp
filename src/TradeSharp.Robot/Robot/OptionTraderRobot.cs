using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable once LocalizableElement
    [DisplayName("Тест опционов")]
    public class OptionTraderRobot : BaseRobot
    {
        public enum OptionType { Call = 1, Put = -1 }
        public enum PriceModel { Gauss = 0, Model }
        class Option
        {
            public string Symbol { get; set; }
            public OptionType OptionType { get; set; }
            public decimal Volume { get; set; }
            public DateTime TimeEnter { get; set; }
            public decimal PriceEnter { get; set; }
            public decimal PriceStrike { get; set; }
            public int ExpirationMinutes { get; set; }
            public DateTime ExpireTime { get { return TimeEnter.AddMinutes(ExpirationMinutes); }}
            public decimal Premium { get; set; }

            public decimal Profit { get; set; }
        }

        [PropertyXMLTag("Robot.Model")]
        [DisplayName("Модель")]
        [Category("Торговые")]
        [Description("Алгоритм моделирования цены")]
        public PriceModel Model { get; set; }

        private int minStrikeMinutes = 1, maxStrikeMinutes = 60 * 4;

        [PropertyXMLTag("Robot.StrikeMinutes")]
        [DisplayName("Контракт, минут")]
        [Category("Торговые")]
        [Description("Интервал трейда, минут")]
        public string StrikeMinutes
        {
            get { return string.Format("{0}..{1}", minStrikeMinutes, maxStrikeMinutes); }
            set
            {
                var numbers = value.ToIntArrayUniform();
                if (numbers.Length != 2) return;
                minStrikeMinutes = numbers.Min();
                maxStrikeMinutes = numbers.Max();
            }
        }

        private int minVolume = 10000, maxVolume = 10000;

        [PropertyXMLTag("Robot.MinMaxVolume")]
        [DisplayName("Объем")]
        [Category("Торговые")]
        [Description("Объем опциона")]
        public string MinMaxVolume
        {
            get { return string.Format("{0}..{1}", minVolume, maxVolume); }
            set
            {
                var numbers = value.ToIntArrayUniform();
                if (numbers.Length != 2) return;
                minVolume = numbers.Min();
                maxVolume = numbers.Max();
            }
        }

        private int minPoints = 3, maxPoints = 100;

        [PropertyXMLTag("Robot.MinMaxPoints")]
        [DisplayName("Пункты")]
        [Category("Торговые")]
        [Description("Смещение цены Strike")]
        public string MinMaxPoints
        {
            get { return string.Format("{0}..{1}", minPoints, maxPoints); }
            set
            {
                var numbers = value.ToIntArrayUniform();
                if (numbers.Length != 2) return;
                minPoints = numbers.Min();
                maxPoints = numbers.Max();
            }
        }

        private int intervalBetweenTradesMinutes = 60 * 2;

        [PropertyXMLTag("Robot.StrikeMinutes")]
        [DisplayName("Интервал трейда, минут")]
        [Category("Торговые")]
        [Description("Интервал трейда, минут")]
        public int IntervalBetweenTradesMinutes
        {
            get { return intervalBetweenTradesMinutes; }
            set { intervalBetweenTradesMinutes = value; }
        }

        private int iterationsCount = 5000;

        [PropertyXMLTag("Robot.IterationsCount")]
        [DisplayName("Итераций")]
        [Category("Торговые")]
        [Description("Итераций для расчета премии")]
        public int IterationsCount
        {
            get { return iterationsCount; }
            set { iterationsCount = value; }
        }

        private decimal minPremium = 0.5M;

        [PropertyXMLTag("Robot.MinPremium")]
        [DisplayName("Мин. премия")]
        [Category("Торговые")]
        [Description("Мин. премия в валюте депо.")]
        public decimal MinPremium
        {
            get { return minPremium; }
            set { minPremium = value; }
        }

        private int premiumPercent = 100;

        [PropertyXMLTag("Robot.PremiumPercent")]
        [DisplayName("Процент премии")]
        [Category("Торговые")]
        [Description("Процент реалььной премии от расчетной")]
        public int PremiumPercent
        {
            get { return premiumPercent; }
            set { premiumPercent = value; }
        }

        private bool useCommaSeparator = true;

        [PropertyXMLTag("Robot.UseCommaSeparator")]
        [DisplayName("Формат - запятая")]
        [Category("Торговые")]
        [Description("Процент реалььной премии от расчетной")]
        public bool UseCommaSeparator
        {
            get { return useCommaSeparator; }
            set { useCommaSeparator = value; }
        }

        #region Переменные состояния

        private string[] tickers;

        private DateTime? timeOfLastDeal;

        private readonly Random rand = new Random(DateTime.Now.Millisecond);

        private readonly List<Option> activeOptions = new List<Option>();
        private readonly List<Option> histOptions = new List<Option>();
        private Dictionary<string, IVolatilityCalculator> tickerVolatility; 
        #endregion

        #region Копирование инициализация
        public override BaseRobot MakeCopy()
        {
            return new OptionTraderRobot
            {
                Model = Model,
                Leverage = Leverage,
                FixedVolume = FixedVolume,
                HumanRTickers = HumanRTickers,
                TypeName = TypeName,
                RoundVolumeStep = RoundVolumeStep,
                RoundType = RoundType,
                RoundMinVolume = RoundMinVolume,

                StrikeMinutes = StrikeMinutes,
                MinMaxPoints = MinMaxPoints,
                IntervalBetweenTradesMinutes = IntervalBetweenTradesMinutes,
                MinMaxVolume = MinMaxVolume,
                IterationsCount = IterationsCount,
                MinPremium = MinPremium,
                PremiumPercent = PremiumPercent,
                UseCommaSeparator = UseCommaSeparator
            };
        }

        public override void Initialize(RobotContext grobotContext, CurrentProtectedContext protectedContextx)
        {
            base.Initialize(grobotContext, protectedContextx);
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("OptionTraderRobot: настройки графиков не заданы");
                return;
            }
            tickers = Graphics.Select(r => r.a).ToArray();
            tickerVolatility = tickers.ToDictionary(t => t,
                t => Model == PriceModel.Gauss ?
                    (IVolatilityCalculator)new GaussVolatilityCalculator(200) : new ReverseVolatilityCalculator(1000));
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return tickers.ToDictionary(t => t, t => startTrade);
        }

        public override void DeInitialize()
        {
            base.DeInitialize();
            SaveResults();
        }

        #endregion

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var messages = new List<string>();

            var nowTime = quotes[0].timeClose;
            var minutesFromLastDeal = timeOfLastDeal.HasValue
                ? (nowTime - timeOfLastDeal.Value).TotalMinutes
                : int.MaxValue;

            for (var i = 0; i < names.Length; i++)
            {
                if (!tickers.Contains(names[i])) continue;
                tickerVolatility[names[i]].UpdateVolatility(quotes[i]);
            }

            if (minutesFromLastDeal >= intervalBetweenTradesMinutes)
            {
                for (var i = 0; i < names.Length; i++)
                {
                    if (!tickers.Contains(names[i])) continue;
                    MakeTrade(names[i], quotes[i]);
                }

                timeOfLastDeal = nowTime;
            }

            CheckOptions(names, quotes);

            return messages;
        }

        private void CheckOptions(string[] names, CandleDataBidAsk[] quotes)
        {
            var nowTime = quotes.Max(q => q.timeClose);

            for (var i = 0; i < activeOptions.Count; i++)
            {
                var option = activeOptions[i];
                // опцион проэкспарился
                if (option.ExpireTime < nowTime)
                {
                    activeOptions.RemoveAt(i);
                    i--;
                    histOptions.Add(option);
                    continue;
                }

                // опцион сработал
                var nameIndex = names.IndexOf(option.Symbol);
                if (nameIndex < 0) continue;
                var quote = quotes[nameIndex];

                if ((option.OptionType == OptionType.Call && quote.high >= (float) option.PriceStrike) ||
                    (option.OptionType == OptionType.Put && quote.low <= (float) option.PriceStrike))
                {
                    option.Profit = 
                        option.Volume * (option.PriceStrike - option.PriceEnter) * (int) option.OptionType;
                    activeOptions.RemoveAt(i);
                    i--;
                    histOptions.Add(option);
                }
            }
        }

        private void MakeTrade(string name, CandleDataBidAsk quote)
        {
            var option = new Option
            {
                Symbol = name,
                TimeEnter = quote.timeClose,
                ExpirationMinutes = rand.Next(minStrikeMinutes, maxStrikeMinutes),
                OptionType = rand.Next(100) < 50 ? OptionType.Call : OptionType.Put,
                Volume = rand.Next(minVolume, maxVolume),                
            };
            option.PriceEnter = (decimal)(option.OptionType == OptionType.Call
                ? quote.closeAsk
                : quote.close);
            var deltaStrikeAbs = DalSpot.Instance.GetAbsValue(name, (decimal) rand.Next(minPoints, maxPoints));
            var deltaSide = option.OptionType == OptionType.Call ? 1 : -1;
            option.PriceStrike = option.PriceEnter + deltaSide * deltaStrikeAbs;
            CalculatePremium(option);
            if (option.Premium == 0)
                return;
            activeOptions.Add(option);
        }

        private void SaveResults()
        {
            using (var fileWriter = new StreamWriter(ExecutablePath.ExecPath + "\\option_trader.txt",
                false, Encoding.UTF8))
            {
                fileWriter.WriteLine("Symbol;Type;Volume;Enter;Strike;Premium;Profit;TimeEnter;ExpireTime");
                foreach (var option in histOptions)
                {
                    var line = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                        option.Symbol, option.OptionType, option.Volume,
                        option.PriceEnter.ToStringUniformPriceFormat(),
                        option.PriceStrike.ToStringUniformPriceFormat(),
                        option.Premium.ToStringUniformMoneyFormat(),
                        option.Profit.ToStringUniformMoneyFormat(),
                        option.TimeEnter.ToString("dd-MM-yyyy HH:mm:ss"),
                        option.ExpireTime.ToString("dd-MM-yyyy HH:mm:ss"));
                    if (useCommaSeparator)
                        line = line.Replace('.', ',');
                    fileWriter.WriteLine(line);
                }                    
            }
        }

        private void CalculatePremium(Option option)
        {
            //var timeStart = DateTime.Now;
            {
                var calculator = tickerVolatility[option.Symbol];
                if (!calculator.PrepareForCalculation())
                    return;
                var intervalsCount = option.ExpirationMinutes;
                var totalProfit = 0.0;

                for (var i = 0; i < iterationsCount; i++)
                {
                    var price = (double) option.PriceEnter;

                    for (var j = 0; j < intervalsCount; j++)
                    {
                        price += calculator.CalculateDelta();
                    }
                    var profit = (int) option.OptionType*(price - (double) option.PriceStrike);
                    if (profit < 0)
                        profit = 0;
                    totalProfit += profit;
                }

                option.Premium = (decimal) totalProfit*option.Volume/iterationsCount;
                option.Premium = option.Premium*PremiumPercent/100;
                if (option.Premium < MinPremium)
                    option.Premium = MinPremium;
            }
            // var milsSpent = (DateTime.Now - timeStart).TotalMilliseconds; // 0
        }
    }

    interface IVolatilityCalculator
    {
        void UpdateVolatility(CandleDataBidAsk candle);

        bool PrepareForCalculation();

        double CalculateDelta();
    }

    class GaussVolatilityCalculator : IVolatilityCalculator
    {
        private readonly int maxLength;

        private readonly List<double> items;

        public double? volatility;

        private double? prevValue;

        private double sum;

        public GaussVolatilityCalculator(int size)
        {
            maxLength = size;
            items = new List<double>(size);
        }

        public void UpdateVolatility(CandleDataBidAsk candle)
        {
            var x = candle.close;
            if (prevValue == null)
            {
                prevValue = x;
                return;
            }
            var delta = x - prevValue.Value;
            var delta2 = delta * delta;
            prevValue = x;
            sum += delta2;

            items.Add(delta2);
            if (items.Count > maxLength)
            {
                sum -= items[0];
                items.RemoveAt(0);
                volatility = Math.Sqrt(sum / maxLength);
            }
        }

        public bool PrepareForCalculation()
        {
            return volatility.HasValue;
        }

        public double CalculateDelta()
        {
            return Gaussian.BoxMuller(0, volatility.Value);
        }
    }

    class ReverseVolatilityCalculator : IVolatilityCalculator
    {
        private PriceModel model;

        private readonly List<double> deltas = new List<double>();

        private readonly int maxSize;

        public ReverseVolatilityCalculator(int size)
        {
            maxSize = size;
        }

        public void UpdateVolatility(CandleDataBidAsk candle)
        {
            deltas.Add(candle.close - candle.open);
            if (deltas.Count > maxSize)
                deltas.RemoveAt(0);
        }

        public bool PrepareForCalculation()
        {
            if (deltas.Count < maxSize) return false;
            model = new PriceModel(deltas.OrderBy(d => d).ToList());
            return true;
        }

        public double CalculateDelta()
        {
            return model.GetRandomDelta();
        }
    }
}
