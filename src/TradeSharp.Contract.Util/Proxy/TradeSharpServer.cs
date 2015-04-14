using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    /// <summary>
    /// singletone-обертка над TradeSharpDictionaryProxy
    /// </summary>
    public class TradeSharpServer
    {
        private static TradeSharpServer instance;
        public static TradeSharpServer Instance
        {
            get { return instance ?? (instance = new TradeSharpServer()); }
        }

        public ITradeSharpServer proxy;

        private TradeSharpServer(ITradeSharpServer proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<ITradeSharpServer>();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpServer ctor", ex);
            }
        }

        public static void Initialize(ITradeSharpServer proxyOrStub)
        {
            instance = new TradeSharpServer(proxyOrStub);
        }
    }
}
