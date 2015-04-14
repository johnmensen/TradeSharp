using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public static class CommonChannelSettings
    {
        public static bool workOffline = AppConfig.GetBooleanParam("Offline", false);
    }
}
