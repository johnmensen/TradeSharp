using System.Net;

namespace TradeSharp.Util
{
    public static class HostName
    {
        public static string GetDnsHostName()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).HostName;
        }

        private static string messageQueueHostName;
        public static string MessageQueueHostName
        {
            get
            {
                if (!string.IsNullOrEmpty(messageQueueHostName)) return messageQueueHostName;
                return messageQueueHostName = AppConfig.GetStringParam("MSMQ.Host", "mercedes");
            }
        }
    }
}
