using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class TradeSharpAccountStatistics
    {
        private static TradeSharpAccountStatistics instance;
        public static TradeSharpAccountStatistics Instance
        {
            get { return instance ?? (instance = new TradeSharpAccountStatistics()); }
        }

        public IAccountStatistics proxy;

        private TradeSharpAccountStatistics(IAccountStatistics proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<IAccountStatistics>();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpAccountStatistics ctor", ex);
            }
        }

        public static void Initialize(IAccountStatistics proxyOrStub)
        {
            instance = new TradeSharpAccountStatistics(proxyOrStub);
        }
    }
}
