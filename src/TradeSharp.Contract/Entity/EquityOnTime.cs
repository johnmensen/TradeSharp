using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public struct EquityOnTime
    {
        public float equity;
        public DateTime time;
        public EquityOnTime(float equity, DateTime time)
        {
            this.equity = equity;
            this.time = time;
        }
    }
}
