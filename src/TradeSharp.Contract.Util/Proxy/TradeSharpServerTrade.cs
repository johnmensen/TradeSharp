using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class TradeSharpServerTrade
    {
        private const string BindingName = "ITradeSharpServerTradeBinding";
        
        public ITradeSharpServerTrade proxy;

        private TradeSharpServerTradeProxy proxyWCF;

        public event Action OnConnectionReestablished
        {
            add
            {
                if (proxyWCF == null) return;
                proxyWCF.OnConnectionReestablished += value;
            }
            remove
            {
                if (proxyWCF == null) return;
                proxyWCF.OnConnectionReestablished -= value;
            }
        }

        public TradeSharpServerTrade(ITradeSharpServerCallback callback)
        {            
            try
            {
                proxyWCF = new TradeSharpServerTradeProxy(BindingName, callback);
                proxy = proxyWCF;
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpServerTrade ctor", ex);
            }
        }

        public TradeSharpServerTrade(ITradeSharpServerTrade trade)
        {
            proxy = trade;
        }   
     
        public void AbortWCFCall()
        {
            if (proxyWCF != null)
                proxyWCF.Abort();
        }
    }
}