using System.Collections.Generic;

namespace TradeSharp.ProviderProxy.BL
{
    public enum FixVersion
    {
        Undefined, Fix42, Fix43, Fix44
    }

    public static class FixVersionTitle
    {
        public static readonly Dictionary<FixVersion, string> versionTitle
            = new Dictionary<FixVersion, string>
                {
                    {FixVersion.Fix42, "FIX4.2"},
                    {FixVersion.Fix43, "FIX4.3"},
                    {FixVersion.Fix44, "FIX4.4"},
                };
        public static string GetVersionTitle(FixVersion vers)
        {
            string title;
            if (versionTitle.TryGetValue(vers, out title))
                return title;
            return vers.ToString();
        }
    }
}
