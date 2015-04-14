using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxyContract.Entity
{
    [Serializable]
    public class MarketOrder : MessssageQueueItem
    {
        public BrokerOrder brokerOrder;

        public string AccountGroupCode { get; set; }

        /// <summary>
        /// за промежуток MillisecondsToStale [мс]
        /// сообщение устаревает. проверяется поле TimeSent
        /// </summary>
        public const int MillisecondsToStale = 5000;

        public override string Title
        {
            get { return string.Format("MarketOrder_{0}", brokerOrder.Id); }
            protected set { }
        }

        public MarketOrder() {}

        public MarketOrder(BrokerOrder brokerOrder)
        {
            this.brokerOrder = brokerOrder;            
        }
        
        public MarketOrder(int id, string ticker, Instrument instrument, int volume, int side,
            OrderPricing orderPricing, decimal? requestedPrice, decimal? enabledAbsoluteSlippage,
            string accountGroupCode)
        {
            AccountGroupCode = accountGroupCode;
            brokerOrder = new BrokerOrder
                              {
                                  Id = id,
                                  Ticker = ticker,
                                  Instrument = instrument,
                                  Volume = volume,
                                  Side = side,
                                  OrderPricing = orderPricing,
                                  RequestedPrice = requestedPrice,
                                  Slippage = enabledAbsoluteSlippage
                              };
        }

        public MarketOrder(int id, string ticker, Instrument instrument, int volume, int side,
            OrderPricing orderPricing, string accountGroupCode)
        {
            AccountGroupCode = accountGroupCode;
            brokerOrder = new BrokerOrder
            {
                Id = id,
                Ticker = ticker,
                Instrument = instrument,
                Volume = volume,
                Side = side,
                OrderPricing = orderPricing
            };
        }

        public override string ToString()
        {
            if (brokerOrder == null) return "<empty market order>";
            var str = string.Format(CultureProvider.Common,
                "Id={0};Ticker={1};Instr={2};Volume={3};Side={4};Pricing={5};",
                brokerOrder.Id, brokerOrder.Ticker, brokerOrder.Instrument, brokerOrder.Volume, 
                brokerOrder.Side, brokerOrder.OrderPricing);
            if (brokerOrder.Slippage.HasValue && brokerOrder.RequestedPrice.HasValue)
                str = str + string.Format(CultureProvider.Common,
                    "Price={0};Slippage={1};", brokerOrder.RequestedPrice, brokerOrder.Slippage);
            return str;
        }

        /// <summary>
        /// отправить в очередь MQ с предопределенным хардкодом именем
        /// </summary>        
        public override bool SendToQueue(bool isError)
        {
            if (brokerOrder == null)
            {
                Logger.Error("Попытка отправить незаполненный MarketOrder");
                return false;
            }
            // получить имя очереди по имени сессии
            var messageQueueName = GetQueueNameByGroup(AccountGroupCode, isError);
            if (string.IsNullOrEmpty(messageQueueName)) return false;

            try
            {                
                SendToQueue(messageQueueName, isError);
                return true;
            }
            catch
            {
                return false;
            }
        }        
    }
}
