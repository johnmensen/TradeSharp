using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("Ренко")]
    [TypeConverter(typeof(PropertySorter))]
    public class RencoRobot : BaseRobot
    {
        #region Настройки
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

        private int brickSize = 50;
        [PropertyXMLTag("Robot.BrickSize")]
        [DisplayName("Размер кирпича, пунктов")]
        [Category("Торговые")]
        [Description("Размер кирпича, пунктов")]
        public int BrickSize
        {
            get { return brickSize; }
            set { brickSize = value; }
        }

        [PropertyXMLTag("Robot.AutoBrickSize")]
        [DisplayName("Авторазмер кирпича")]
        [Category("Торговые")]
        [Description("Размер кирпича считается от волатильности")]
        public bool BrickSizeAuto { get; set; }

        private float autosizeScale = 1;
        [PropertyXMLTag("Robot.AutoScale")]
        [DisplayName("Автомасштаб")]
        [Category("Торговые")]
        [Description("Масштаб авторазмера кирпича (множитель волатильности)")]
        public float AutosizeScale
        {
            get { return autosizeScale; }
            set { autosizeScale = value; }
        }

        private int autosizePeriod = 30;
        [PropertyXMLTag("Robot.AutosizePeriod")]
        [DisplayName("Период волатильности")]
        [Category("Торговые")]
        [Description("Период измерения волатильности (для автомасштаба)")]
        public int AutosizePeriod
        {
            get { return autosizePeriod; }
            set { autosizePeriod = value; }
        }

        public enum RencoVolatilityType { Размах = 0, ATR = 1 }
        [PropertyXMLTag("Robot.VolatilityType")]
        [DisplayName("Тип волатильности")]
        [Description("Тип измерения волатильности для аввторазмера")]
        [Category("Торговые")]
        public RencoVolatilityType VolatilityType { get; set; }

        #endregion

        #region Переменные
        private float pointCost;
        private string ticker;
        private RestrictedQueue<CandleData> lastPrices;
        private CandlePacker packer;
        private RencoRobotBrick lastBrick;
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new RencoRobot
            {
                StopLossPoints = StopLossPoints,
                TakeProfitPoints = TakeProfitPoints,
                BrickSize = BrickSize,
                BrickSizeAuto = BrickSizeAuto,
                AutosizeScale = AutosizeScale,
                VolatilityType = VolatilityType,
                AutosizePeriod = AutosizePeriod,
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

        public override void Initialize(BacktestServerProxy.RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
 	        base.Initialize(robotContext, protectedContext);
            if (Graphics.Count == 0) return;
            ticker = Graphics[0].a;
            pointCost = DalSpot.Instance.GetAbsValue(ticker, 1f);
            if (BrickSizeAuto) lastPrices = new RestrictedQueue<CandleData>(AutosizePeriod);
            packer = new CandlePacker(Graphics[0].b);
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var events = new List<string>();
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

            var signal = GetRencoSignal(candle);
            if (signal != 0)
                if (!isHistoryStartOff)
            {
                // коментарий
                events.Add(new RobotHint(ticker, Graphics[0].b.ToString(), 
                    "вход по Ренко", "вход", "e", candle.close)
                {
                    ColorFill = signal < 0 ? Color.Pink : Color.LightGreen,
                    Time = candle.timeClose,
                    RobotHintType = signal > 0 ? RobotHint.HintType.Покупка : RobotHint.HintType.Продажа
                }.ToString());
                
                // закрыть противонаправленные
                List<MarketOrder> orders;
                robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                var ordersToClose = orders.FindAll(o => o.Side != signal && o.Magic == Magic);
                foreach (var order in ordersToClose)
                    robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                        robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                
                // открыть сделку
                OpenDeal(candle.close, signal);
            }
            
            return events;
        }

        private void OpenDeal(float price, int dealSide)
        {
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

            float? stop = StopLossPoints == 0
                                ? (float?)null
                                : price -
                                  dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)StopLossPoints);
            float? take = TakeProfitPoints == 0
                                ? (float?)null
                                : price +
                                  dealSide * DalSpot.Instance.GetAbsValue(ticker, (float)TakeProfitPoints);

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
                        StopLoss = stop,
                        TakeProfit = take,
                        ExpertComment = "RencoRobot"
                    },
                OrderType.Market, 0, 0);
        }

        private int GetRencoSignal(CandleData candle)
        {
            var brickSizeAbs = brickSize*pointCost;
            // автомасштаб...
            if (BrickSizeAuto)
            {
                lastPrices.Add(candle);
                brickSizeAbs = CalculateBrickSize();
            }

            // проверить - добавлен ли новый кирпич? сменился ли знак кирпича?
            if (lastBrick == null)
            {
                var brickDir = candle.close < candle.open ? -1 : 1;
                lastBrick = new RencoRobotBrick(candle.open, candle.close, brickDir);
                return 0;
            }

            var deltaClose = (int) ((candle.close - lastBrick.priceClose)/brickSizeAbs);
            var deltaOpen = (int) ((candle.close - lastBrick.priceOpen)/brickSizeAbs);
            if (deltaClose > 0 && lastBrick.direction > 0)
            {
                lastBrick = new RencoRobotBrick(
                    lastBrick.priceClose + (deltaClose - 1) * brickSizeAbs, 
                    lastBrick.priceClose + deltaClose * brickSizeAbs, 1);
                return 0;
            }
            if (deltaOpen < 0 && lastBrick.direction > 0)
            {
                lastBrick = new RencoRobotBrick(
                    lastBrick.priceOpen + (deltaOpen + 1) * brickSizeAbs,
                    lastBrick.priceOpen + deltaOpen * brickSizeAbs, -1);
                return -1;
            }
            if (deltaClose < 0 && lastBrick.direction < 0)
            {
                lastBrick = new RencoRobotBrick(
                    lastBrick.priceClose + (deltaClose + 1) * brickSizeAbs, 
                    lastBrick.priceClose + deltaClose * brickSizeAbs, -1);
                return 0;
            }
            if (deltaOpen > 0 && lastBrick.direction < 0)
            {
                lastBrick = new RencoRobotBrick(
                    lastBrick.priceOpen + (deltaOpen - 1) * brickSizeAbs,
                    lastBrick.priceOpen + deltaOpen * brickSizeAbs, 1);
                return 1;
            }
            return 0;
        }

        private float CalculateBrickSize()
        {
            if (lastPrices.Length < AutosizePeriod) return pointCost*brickSize;
            if (VolatilityType == RencoVolatilityType.Размах)
                return (lastPrices.Max(c => c.high) - lastPrices.Min(c => c.low)) * AutosizeScale;
            //if (VolatilityType == RencoVolatilityType.ATR)
                return lastPrices.Average(p => p.high - p.low) * AutosizeScale;
        }
    }
    // ReSharper restore LocalizableElement

    class RencoRobotBrick
    {
        public float priceOpen, priceClose;
        public int direction;

        public RencoRobotBrick(float priceOpen, float priceClose, int direction)
        {
            this.priceOpen = priceOpen;
            this.priceClose = priceClose;
            this.direction = direction;
        }
    }
}
