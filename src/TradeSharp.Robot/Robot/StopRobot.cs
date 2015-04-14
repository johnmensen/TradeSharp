using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Условный стоп")]
    public class StopRobot : BaseRobot
    {
        #region Неактуальные настройки
        [Browsable(false)]
        public override string NewsChannels { get; set; }
        #endregion

        #region Свойства
        private bool checkIndividualStop = true;
        [PropertyXMLTag("Robot.CheckIndividualStop")]
        [DisplayName("Индивид. стоп")]
        [Category("Торговые")]
        [Description("Проверять стоп, заданный в комментарии сделки")]
        public bool CheckIndividualStop
        {
            get { return checkIndividualStop; }
            set { checkIndividualStop = value; }
        }
        
        [PropertyXMLTag("Robot.StopLevel")]
        [DisplayName("Уровень SL")]
        [Category("Торговые")]
        [Description("Условный SL, проверяется для сделок без индивид. SL")]
        public decimal StopLevel { get; set; }

        public enum PriceSide { Выше = 0, Ниже = 1 }

        [PropertyXMLTag("Robot.Side")]
        [DisplayName("Знак SL")]
        [Category("Торговые")]
        [Description("Закрытие должно быть ниже либо выше указанной цены")]
        public PriceSide Side { get; set; }

        [PropertyXMLTag("Robot.OverrideIndividualStop")]
        [DisplayName("Игнор. инд. стоп")]
        [Category("Торговые")]
        [Description("Закрывать по общему стопу, даже если указан индивид. стоп")]
        public bool OverrideIndividualStop { get; set; }
        #endregion

        #region Переменные
        private string ticker;
        private CandlePacker packer;
        private readonly Regex regConStop = new Regex(@"CS>[0-9]*\.?[0-9]+");
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new StopRobot
            {
                CheckIndividualStop = CheckIndividualStop,
                StopLevel = StopLevel,
                Side = Side,
                OverrideIndividualStop = OverrideIndividualStop
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return Graphics.Count != 1 ? null
                : new Dictionary<string, DateTime> { { Graphics[0].a, startTrade } };
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);
            // проверка настроек графиков
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("StopRobot: настройки графиков не заданы");
                return;
            }
            if (Graphics.Count > 1)
            {
                Logger.DebugFormat("StopRobot: настройки графиков должны описывать один тикер / один ТФ");
                return;
            }
            ticker = Graphics[0].a;
            packer = new CandlePacker(Graphics[0].b);
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var events = new List<string>();
            if (string.IsNullOrEmpty(ticker)) return events;

            CandleDataBidAsk quote = null;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] != ticker) continue;
                quote = quotes[i];
                break;
            }
            
            if (quote == null) return events;
            
            // обновить свечку
            var candle = packer.UpdateCandle(quote);
            if (candle == null) return events;

            // получить ордера
            List<MarketOrder> orders;
            GetMarketOrders(out orders, false);
            orders = orders.Where(o => o.Symbol == ticker).ToList();
            if (orders.Count == 0 || isHistoryStartOff) return events;

            // проверить стоп по уровням и по индивидуальным настрофкам сделок
            if (checkIndividualStop) 
                DoCheckIndividualStop(candle, orders, events);

            // проверить стоп по заданному уровню
            if (StopLevel != 0)
                DoCheckCommonStop(candle, orders, events);

            return events;
        }

        public void DoCheckCommonStop(CandleData candle, List<MarketOrder> orders, List<string> events)
        {
            if ((Side == PriceSide.Выше && candle.close < (float)StopLevel) ||
                (Side == PriceSide.Ниже && candle.close > (float)StopLevel)) return;
            var eventStr = string.Format("Условный стоп сделок закрытие свечи на {0} ({1} отметки {2})",
                        candle.close.ToStringUniformPriceFormat(),
                        Side.ToString().ToLower(),
                        StopLevel.ToStringUniformPriceFormat());
            events.Add(eventStr);
            Logger.Info(eventStr);

            foreach (var order in orders)
            {
                // проверить - у ордера не должно быть индивидуальной цены закрытия
                if (OverrideIndividualStop)
                if (!string.IsNullOrEmpty(order.Comment))
                {
                    if (regConStop.IsMatch(order.Comment.Replace(',', '.'))) continue;
                }

                // закрыть ордер
                CloseMarketOrder(order.ID);
            }
        }

        public void DoCheckIndividualStop(CandleData candle, List<MarketOrder> orders, List<string> events)
        {
            // формат комментария - "CS>1.652" ... "CS<92.38" ...
            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.Comment)) continue;
                var comText = order.Comment.Trim();
                if (!comText.StartsWith("CS")) continue;
                comText = comText.Substring("CS".Length);
                if (string.IsNullOrEmpty(comText)) continue;

                // получить знак и цену
                var side = comText[0] == '>' ? 1 : comText[0] == '<' ? -1 : 0;
                if (side == 0) continue;

                var price = comText.Substring(1).Replace(',', '.').ToFloatUniformSafe();
                if (!price.HasValue) continue;

                // проверить условие стопа
                if ((side > 0 && candle.close >= price.Value) || (side < 0 && candle.close <= price.Value))
                {
                    // произвести стоп
                    var eventStr = string.Format("Условный стоп сделки #{0} (CS>{1}): закрытие свечи на {2}",
                        order.ID, price.Value.ToStringUniformPriceFormat(),
                        candle.close.ToStringUniformPriceFormat());
                    events.Add(eventStr);
                    Logger.Info(eventStr);

                    CloseMarketOrder(order.ID);
                }
            }
        }
    }
}
