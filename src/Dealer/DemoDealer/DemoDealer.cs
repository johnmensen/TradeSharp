using System;
using System.Collections.Generic;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.DealerInterface;

namespace DemoDealer
{
    public class DemoDealer : SimpleDealer, IDealer
    {
        public string DealerCode { get; private set; }

        public List<string> GroupCodes { get; private set; }

        public DemoDealer(DealerDescription desc, List<string> accountGroupCodes)
        {
            GroupCodes = accountGroupCodes;
            DealerCode = desc.Code;
        }

        public virtual void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public virtual string GetErrorString()
        {
            return string.Empty;
        }

        public virtual void ClearError()
        {
        }

        public virtual RequestStatus SendNewOrderRequest(Account account,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            order.State = PositionState.Opened;
            order.TimeEnter = DateTime.Now;
            
            //if (magic.HasValue)
            //    order.ExpertComment = comment;
            //else
            //    order.Comment = comment;
            // подставить текущую цену
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;
            order.PriceEnter = order.Side > 0 ? quote.ask : quote.bid;
            // проверить проскальзывание
            if (slippagePoints != 0)
            {
                var slippageAbs = DalSpot.Instance.GetAbsValue(order.Symbol, slippagePoints);
                var delta = Math.Abs(order.PriceEnter - (float)requestedPrice);
                if (delta > (float)slippageAbs) return RequestStatus.Slippage;
            }
            
            int posID;
            // сохранить ордер (и уведомить клиента)
            var result = ServerInterface.SaveOrderAndNotifyClient(order, out posID);
            return result ? RequestStatus.OK : RequestStatus.SerializationError;
        }

        public virtual RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason)
        {
            // получить цену
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;
            var price = order.Side > 0 ? quote.GetPrice(QuoteType.Bid) : quote.GetPrice(QuoteType.Ask);

            // закрыть ордер немедленно
            return ServerInterface.CloseOrder(order, (decimal)price, reason)
                ? RequestStatus.OK : RequestStatus.ServerError;
        }
    }
}