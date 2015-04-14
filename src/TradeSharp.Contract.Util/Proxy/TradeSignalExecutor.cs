using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class TradeSignalExecutor
    {
        private static TradeSignalExecutor instance;
        public static TradeSignalExecutor Instance
        {
            get { return instance ?? (instance = new TradeSignalExecutor()); }
        }

        public ITradeSignalExecutor proxy;

        private TradeSignalExecutor(ITradeSignalExecutor proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<ITradeSignalExecutor>();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalExecutor ctor", ex);
            }
        }

        public static void Initialize(ITradeSignalExecutor proxyOrStub)
        {
            instance = new TradeSignalExecutor(proxyOrStub);
        }
    }
}
