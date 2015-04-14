using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Models.Items
{
    public class PammItem : Account
    {
        public int OwnerCount { get; set; }
        public List<ACCOUNT_SHARE> ShareItems { get; set; }
    }
}