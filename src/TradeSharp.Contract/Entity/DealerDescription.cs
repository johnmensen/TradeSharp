using System;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class DealerDescription
    {
        [LocalizedDisplayName("TitleCode")]
        public string Code { get; set; }

        [LocalizedDisplayName("TitleFileName")]
        public string FileName { get; set; }

        [LocalizedDisplayName("TitleDealerEnabled")]
        public bool DealerEnabled { get; set; }

        public override string ToString()
        {
            return Code;
        }
    }
}
