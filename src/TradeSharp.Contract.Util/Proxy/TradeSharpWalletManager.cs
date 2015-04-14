using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class TradeSharpWalletManager
    {
        private static TradeSharpWalletManager instance;
        public static TradeSharpWalletManager Instance
        {
            get { return instance ?? (instance = new TradeSharpWalletManager()); }
        }

        public IWalletManager proxy;

        private TradeSharpWalletManager(IWalletManager proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<IWalletManager>();
            }
            catch (Exception ex)
            {
                Logger.Error("WalletManager ctor", ex);
            }
        }

        public static void Initialize(IWalletManager proxyOrStub)
        {
            instance = new TradeSharpWalletManager(proxyOrStub);
        }
    }
}
