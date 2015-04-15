using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    /// <summary>
    /// "сырые", необработанные данные по перформансу счета - кривая средств
    /// и кривая макс.
    /// </summary>
    public class AccountPerformanceRaw
    {
        public List<EquityOnTime> equity;

        public List<EquityOnTime> leverage;

        public long totalTradedVolume;

        public AccountPerformanceRaw()
        {
            equity = new List<EquityOnTime>();
            leverage = new List<EquityOnTime>();
        }
    }    
}
