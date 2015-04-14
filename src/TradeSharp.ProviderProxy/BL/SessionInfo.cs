using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.BL
{
    class SessionInfo
    {
        private static SessionInfo instance;

        public static SessionInfo Instance
        {
            get { return instance ?? (instance = new SessionInfo()); }
        }
        
        public readonly string dealerCode;
        public readonly ProviderSession[] dicQueueSession;

        private SessionInfo()
        {
            dealerCode = AppConfig.GetStringParam("Dealer", string.Empty);
            if (string.IsNullOrEmpty(dealerCode))
            {
                const string msg = "Код дилера не задан в конфигурации (параметр Dealer)";
                Logger.Error(msg);
                throw new Exception(msg);
            }
            dicQueueSession = TradeSharpDictionary.Instance.proxy.GetQueueAndSession(dealerCode);
        }
    }
}