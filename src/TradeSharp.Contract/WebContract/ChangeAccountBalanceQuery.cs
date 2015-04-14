using System;
using System.Runtime.Serialization;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.WebContract
{
    [DataContract]
    public class ChangeAccountBalanceQuery : HttpParameter
    {
        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public DateTime ValueDate { get; set; }

        [DataMember]
        public BalanceChangeType ChangeType { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
