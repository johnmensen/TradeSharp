using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class BrokerOrder
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Ticker { get; set; }
        public Instrument Instrument { get; set; }
        public int Volume { get; set; }
        public int Side { get; set; }
        public OrderPricing OrderPricing { get; set; }
        public decimal? RequestedPrice { get; set; }
        public decimal? Slippage { get; set; }
        public string DealerCode { get; set; }
        public int AccountID { get; set; }
        public int? ClosingPositionID { get; set; }
        public DateTime TimeCreated { get; set; }
        public int? Magic { get; set; }
        public string Comment { get; set; }
        public string ExpertComment { get; set; }
        public float MarkupAbs { get; set; }

        public override string ToString()
        {
            return string.Format("BrokerOrder: RequestId={0}; Id={1}; Ticker={2}; Instrument={3}; Volume={4}; Side={5}; " +
                                 "Pricing={6}; PriceReq={7:f4}; Slippage={8:f2}; Dealer={9}; Account={10}; " +
                                 "ClosPosId={11}; Created={12:dd.MM.yyyy HH:mm:ss}; Magic={13}; Comment={14}; ExpertComment={15}",
                                 RequestId, Id, Ticker, Instrument, Volume, Side,
                                 OrderPricing, RequestedPrice ?? 0, Slippage ?? 0, DealerCode,
                                 AccountID, ClosingPositionID ?? 0, TimeCreated, Magic, Comment, ExpertComment);
        }
    }
}
