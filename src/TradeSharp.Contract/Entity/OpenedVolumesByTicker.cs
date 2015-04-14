using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class TotalOpenedVolumesByTicker
    {
        public string Ticker { get; set; }
        
        public long TotalBuy { get; set; }
        
        public long TotalSell { get; set; }

        public DateTime TimeMeasured { get; set; }
    }
}
