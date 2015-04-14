using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Robot.Robot
{
    [DisplayName("Робот для тестирования")]
    [TypeConverter(typeof(PropertySorter))]
    public class SimpleRobot : BaseRobot
    {
        [PropertyXMLTag("Robot.SimpleSettings")]
        [DisplayName("Настройки simple робота")]
        [Category("Custom")]
        [Description("Настройки simple робота")]
        public string CustomSettings { get; set; }

        public enum StateInfo
        {
            NoTrade = 0,
            OpenBuy = 1,
            OpenSell = 2
        };

        [PropertyXMLTag("Robot.StateRobot")]
        [DisplayName("Текущее состояние робота")]
        [Category("Custom")]
        [Description("Текущее состояние робота")]
        public StateInfo StateRobot { get; set; }

        public int? takeProfit = 50;
        [PropertyXMLTag("Robot.TakeProfit")]
        [DisplayName("Take-profit, пп")]
        [Category("Custom")]
        [Description("Take-profit для открытых позиций, пунктов")]
        public int? TakeProfit
        {
            get { return takeProfit; }
            set { takeProfit = value; }
        }

        private int? stopLoss = 50;
        [PropertyXMLTag("Robot.StopLoss")]
        [DisplayName("StopLoss, пп")]
        [Category("Custom")]
        [Description("Stop-loss для открытых позиций, пунктов")]
        public int? StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = value; }
        }
        
        public override BaseRobot MakeCopy()
        {
            var robo = new SimpleRobot
                           {
                               CustomSettings = CustomSettings,
                               StateRobot = StateRobot,
                               TakeProfit = TakeProfit,
                               StopLoss = StopLoss
                           };
            CopyBaseSettings(robo);
            return robo;
        }

        /// <summary>
        /// Вызывается на торговых событиях установка снятие ордера, изменение параметров, срабатывание ордера
        /// </summary>
        /// <param name="order"></param>
        public override void OnTradeEvent(PendingOrder order) { }

        /// <summary>
        /// Вызывается с приходом новой котировки
        /// </summary>
        public override List<string> OnQuotesReceived(string[] names, QuoteData[] quotes, bool isHistoryStartOff)
        {
            if (isHistoryStartOff) return null;
             
            List<MarketOrder> orders;
            
            robotContext.GetMarketOrders(robotContext.accountInfo.ID, out orders);
            
            if (orders == null || orders.Count == 0)
                StateRobot = StateInfo.NoTrade;
            if (quotes[0] == null) return null;

            for (var i = 0; i < names.Count(); i++)
            {
                // пробегаемся по тикерам и ищем те на которые подписан робот
                foreach (var ticker in Graphics)
                {
                    if (ticker.a == names[i])
                    {
                        //BarSettings set = ticker.b;
                        //var dt = set.GetDistanceTime(15, -1, quotes[i].Time);


                        // валютная пара совпадает
                        if (StateRobot == StateInfo.NoTrade)
                        {
                            // надо открыть позицию допустим buy
                            var sl = quotes[i].ask - DalSpot.Instance.GetAbsValue(ticker.a, (float) (StopLoss ?? 0));
                            var tp = quotes[i].ask + DalSpot.Instance.GetAbsValue(ticker.a, (float) (TakeProfit ?? 0));
                            if (NewOrder(Magic, ticker.a, 10000, 1, OrderType.Market, 0, 0, (decimal?)sl,
                                     (decimal?)tp, null, null, string.Empty, "simple_robot") == RequestStatus.OK)
                            {
                                StateRobot = StateInfo.OpenBuy;
                            }
                        }   
                    }
                }
            }
            return null;
        }
        
    }
}
