using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Круглые цены")]
    public class RoundPriceRobot : BaseRobot
    {
        #region Настройки
        private bool closeOpposite = true;
        [PropertyXMLTag("CloseOpposite")]
        [DisplayName("Закрывать сделки")]
        [Category("Торговые")]
        [Description("Закрывать сделки противоположного знака")]
        public bool CloseOpposite
        {
            get { return closeOpposite; }
            set { closeOpposite = value; }
        }

        private int roundDigits = 2;
        [PropertyXMLTag("RoundDigits")]
        [DisplayName("Круглые разряды")]
        [Category("Торговые")]
        [Description("Количество нулей в послед. разрядах цены")]
        public int RoundDigits
        {
            get { return roundDigits; }
            set { roundDigits = value; }
        }

        private int stopLossPoints = 100, takeProfitPoints = 100;

        [PropertyXMLTag("Robot.StopLossPoints")]
        [DisplayName("Стоплосс, пп")]
        [Category("Торговые")]
        [Description("Стоплосс, пунктов. 0 - не задан")]
        public int StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        [PropertyXMLTag("Robot.TakeProfitPoints")]
        [DisplayName("Тейкпрофит, пп")]
        [Category("Торговые")]
        [Description("Тейкпрофит, пунктов. 0 - не задан")]
        public int TakeProfitPoints
        {
            get { return takeProfitPoints; }
            set { takeProfitPoints = value; }
        }
        #endregion

        #region Оперативные данные
        private Dictionary<string, CandlePacker> packers;

        private int digitsDen;
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new RoundPriceRobot
                {
                    CloseOpposite = CloseOpposite,
                    RoundDigits = RoundDigits,
                    StopLossPoints = StopLossPoints,
                    TakeProfitPoints = TakeProfitPoints,
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

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, 
            Contract.Util.BL.CurrentProtectedContext protectedContextx)
        {
            base.Initialize(grobotContext, protectedContextx);
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            digitsDen = (int) Math.Pow(10, roundDigits);
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var events = new List<string>();
            if (isHistoryStartOff) return events;

            // сформировать свечи
            for (var i = 0; i < names.Length; i++)
            {
                CandlePacker packer;
                var ticker = names[i];
                if (!packers.TryGetValue(ticker, out packer)) continue;
                var candle = packer.UpdateCandle(quotes[i]);
                if (candle == null) continue;
                
                // проверить условия для входа в рынок
                string comment;
                var side = CheckTradeCondition(candle, ticker, out comment);
                if (side == 0) continue;

                // закрыть сделки противоположного знака
                if (CloseOpposite)
                {
                    List<MarketOrder> orders;
                    GetMarketOrders(out orders, true);
                    foreach (var order in orders.Where(o => o.Symbol == ticker && o.Side != side))
                        CloseMarketOrder(order.ID);
                }

                // создать ордер
                OpenDeal(quotes[i].close, side, ticker, comment);
            }
            return events;
        }

        private void OpenDeal(float price, int side, string ticker, string comment)
        {
            var dealVolumeDepo = CalculateVolume(ticker);
            if (dealVolumeDepo == 0) return;

            float? stop = StopLossPoints == 0
                                ? (float?)null
                                : price -
                                  side * DalSpot.Instance.GetAbsValue(ticker, (float)StopLossPoints);
            float? take = TakeProfitPoints == 0
                                ? (float?)null
                                : price +
                                  side * DalSpot.Instance.GetAbsValue(ticker, (float)TakeProfitPoints);

            robotContext.SendNewOrderRequest(
                protectedContext.MakeProtectedContext(),
                RequestUniqueId.Next(),
                new MarketOrder
                    {
                        Symbol = ticker,
                        Side = side,
                        AccountID = robotContext.AccountInfo.ID,
                        Magic = Magic,
                        Volume = dealVolumeDepo,
                        ExpertComment = comment,
                        Comment = comment,
                        StopLoss = stop,
                        TakeProfit = take
                    },
                OrderType.Market, 0, 0);
        }

        private int CheckTradeCondition(CandleData candle, string symbol, out string comment)
        {
            comment = string.Empty;
            // искать круглую цену в пределах тени H, тела или тени L
            var top = Math.Max(candle.open, candle.close);
            var bottom = Math.Min(candle.open, candle.close);

            var pointCost = DalSpot.Instance.GetPrecision10(symbol);
            var pHigh = (int) Math.Round(candle.high * pointCost);
            var pLow = (int)Math.Round(candle.low * pointCost);
            var pTop = (int)Math.Round(top * pointCost);
            var pBotm = (int)Math.Round(bottom * pointCost);

            int rH, rB, rL;
            var h = HasRoundValueBetweenAundB(pHigh, pTop, out rH);
            var b = HasRoundValueBetweenAundB(pTop, pBotm, out rB);
            var l = HasRoundValueBetweenAundB(pBotm, pLow, out rL);

            // правила входа
            if (!(h || l || b)) return 0;
            // более одного пересечения круглой цены игнорируются
            if ((h && b) || (h && l) || (b && l)) return 0;

            var isBullish = candle.close > candle.open;
            if (l && !isBullish)
            {
                comment = string.Format("Отскок от цены {0}", 
                    DalSpot.Instance.GetAbsValue(symbol, (float)rL).ToStringUniformPriceFormat());
                return 1;
            }
            if (b)
            {
                comment = string.Format("Пройдена цена {0} ({1})",
                    DalSpot.Instance.GetAbsValue(symbol, (float)rB).ToStringUniformPriceFormat(),
                    isBullish ? "вверх" : "вниз");
                return isBullish ? 1 : -1;
            }
            if (h && isBullish)
            {
                comment = string.Format("Отскок от цены {0}",
                    DalSpot.Instance.GetAbsValue(symbol, (float)rH).ToStringUniformPriceFormat());
                return -1;
            }
            return 0;
        }

        private bool HasRoundValueBetweenAundB(int a, int b, out int rVal)
        {
            rVal = b / digitsDen;
            rVal *= digitsDen;
            return rVal >= a || rVal == b;
        }
    }
}
