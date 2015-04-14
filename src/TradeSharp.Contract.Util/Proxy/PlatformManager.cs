using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class PlatformManager
    {
        private static PlatformManager instance;
        public static PlatformManager Instance
        {
            get { return instance ?? (instance = new PlatformManager()); }
        }

        public IPlatformManager proxy;

        private PlatformManager(IPlatformManager proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<IPlatformManager>();
            }
            catch (Exception ex)
            {
                Logger.Error("PlatformManager ctor", ex);
            }
        }

        public static void Initialize(IPlatformManager proxyOrStub)
        {
            instance = new PlatformManager(proxyOrStub);
        }
    }
}
