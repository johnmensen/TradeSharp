using System.Collections.Generic;
using System.Linq;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Models.CommonClass
{
    public class BaseActive
    {
        public static List<COMMODITY> GetBaseActiveList()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                return ctx.COMMODITY.ToList();
            }
        }
    }
}