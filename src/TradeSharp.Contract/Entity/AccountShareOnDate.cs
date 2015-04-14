using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class AccountShareOnDate
    {
        public DateTime date;

        public decimal? oldShare, oldHWM;

        public decimal newShare, newHWM, shareAmount;
    }
}
