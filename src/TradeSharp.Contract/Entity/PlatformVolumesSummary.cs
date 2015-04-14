using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PlatformVolumesSummary
    {
        public TotalOpenedVolumesByTicker[] TotalVolumesByTicker { get; set; }

        public long TotalGrossInUsd { get; set; }

        public PlatformVolumesSummary()
        {
        }

        public PlatformVolumesSummary(TotalOpenedVolumesByTicker[] totalVolumesByTicker)
        {
            TotalVolumesByTicker = totalVolumesByTicker;
        }
    }
}
