using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Contract.Util.Proxy
{
    public class TradeSharpAccount
    {
        private static TradeSharpAccount instance;
        public static TradeSharpAccount Instance
        {
            get { return instance ?? (instance = new TradeSharpAccount()); }
        }

        public ITradeSharpAccount proxy;

        private TradeSharpAccount(ITradeSharpAccount proxyOrStub = null)
        {
            if (proxyOrStub != null)
            {
                proxy = proxyOrStub;
                return;
            }
            try
            {
                proxy = ProxyBuilder.Instance.GetImplementer<ITradeSharpAccount>();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpAccount", ex);
            }
        }

        public static void Initialize(ITradeSharpAccount proxyOrStub)
        {
            instance = new TradeSharpAccount(proxyOrStub);
        }

        #region Методы - обертки

        public RequestStatus GetHistoryOrdersUncompressed(int? accountId, DateTime? startDate, out List<MarketOrder> orders)
        {
            orders = null;

            byte[] buffer;
            var retVal = proxy.GetHistoryOrdersCompressed(accountId, startDate, out buffer);
            if (buffer == null || buffer.Length == 0) return retVal;

            try
            {
                using (var reader = new SerializationReader(buffer))
                {
                    var array = reader.ReadObjectArray(typeof (MarketOrder));
                    if (array != null && array.Length > 0)
                        orders = array.Cast<MarketOrder>().ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetHistoryOrdersUncompressed() - serialization error", ex);
                return RequestStatus.SerializationError;
            }

            return retVal;
        }

        #endregion
    }
}
