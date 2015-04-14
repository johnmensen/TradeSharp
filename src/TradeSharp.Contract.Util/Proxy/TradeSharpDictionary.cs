using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    /// <summary>
    /// singletone-обертка над TradeSharpDictionaryProxy
    /// </summary>
    public class TradeSharpDictionary
    {
        private static TradeSharpDictionary instance;
        public static TradeSharpDictionary Instance
        {
            get { return instance ?? (instance = new TradeSharpDictionary()); }
        }

        public ITradeSharpDictionary proxy;

        private TradeSharpDictionary(ITradeSharpDictionary proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<ITradeSharpDictionary>();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpDictionary ctor", ex);
            }
        }

        public static void Initialize(ITradeSharpDictionary proxyOrStub)
        {
            instance = new TradeSharpDictionary(proxyOrStub);
        }
    }
}