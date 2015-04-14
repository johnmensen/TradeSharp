using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TradeSharp.SiteAdmin.BL.Localisation
{
    public static class EnumLocalizer<T>
    {
        private static readonly Dictionary<T, string> names = new Dictionary<T, string>();

        static EnumLocalizer()
        {
            var namePreffix = "Enum" + typeof (T).Name;
            foreach (T name in Enum.GetValues(typeof (T)))
            {
                // EnumDealSideBuy
                // EnumSelectOrderПервые
                var resName = namePreffix + name;
                // ...
                names.Add(name, name.ToString());
            }
        }

        public static string GetName(T val)
        {
            return names[val];
        }
    }
}