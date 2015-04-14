using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public class QuoteStorage
    {
        private const string BindingName = "IQuoteStorageBinding";

        private static QuoteStorage instance;
        public static QuoteStorage Instance
        {
            get { return instance ?? (instance = new QuoteStorage()); }
        }

        public IQuoteStorage proxy;

        private QuoteStorage(IQuoteStorage proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = new QuoteStorageProxy(BindingName);
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteStorage ctor", ex);
            }
        }

        public static void Initialize(IQuoteStorage proxyOrStub)
        {
            instance = new QuoteStorage(proxyOrStub);
        }
    }
}
