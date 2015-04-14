namespace TradeSharp.Util
{
    /// <summary>
    /// хранит константы - предопределенные имена очередей MQ
    /// </summary>
    public static class MessageQueues
    {
        public static readonly string QueueFromProvider = AppConfig.GetStringParam("MQ.FromProvider", "ts.out");
        public static readonly string QueueFromProviderError = AppConfig.GetStringParam("MQ.FromProvider.Error", "ts.out.error");

        public static string FormatQueueName(string host, string queue)
        {
            string str = host.ToUpper();
            if (HostName.GetDnsHostName().ToUpper().Equals(str))
                return string.Format(@".\Private$\{0}", queue);
            return string.Format(@"FormatName:DIRECT=OS:{0}\Private$\{1}", str, queue);
        }
    }
}
