using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class BrokerResponse
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public decimal? Price { get; set; }
        public decimal? Swap { get; set; }
        public OrderStatus Status { get; set; }
        public OrderRejectReason? RejectReason { get; set; }
        public string RejectReasonString { get; set; }
        public DateTime ValueDate { get; set; }

        public override string ToString()
        {
            var strMain = string.Format("BrokerResponse: [Id={0}, ReqId={1}, Price={2:f4}, Swap={3:f4}, Status={4}, Date={5:dd.MM.yy HH:mm:ss}",
                                        Id, RequestId, Price ?? 0, Swap ?? 0, Status,
                                        ValueDate);
            if (!string.IsNullOrEmpty(RejectReasonString) || RejectReason.HasValue)
                strMain = strMain + string.Format("Reject={0},\"{1}\"]",
                                                  RejectReason ?? OrderRejectReason.None, RejectReasonString);
            else
                strMain = strMain + "]";
            return strMain;
        }
    }
}
