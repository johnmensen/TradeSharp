using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class TradeSharpServerProxy : ITradeSharpServer
    {
        private ChannelFactory<ITradeSharpServer> factory;
        private ITradeSharpServer channel;
        private readonly string endpointName;

        public TradeSharpServerProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<ITradeSharpServer>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpServerProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }

        #region ITradeSharpServer
        public List<PerformerStat> GetAllManagers(PaidServiceType? serviceTypeFilter)
        {
            if (channel == null) throw new Exception("TradeSharpServerProxy: связь не установлена");
            try
            {
                return channel.GetAllManagers(serviceTypeFilter);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetAllManagers(serviceTypeFilter);
                }
                catch (Exception ex)
                {
                    Logger.Error("GetAllManagers()", ex);
                    return null;
                }
            }
        }

        public List<PerformerStat> GetCompanyTopPortfolioManagedAccounts()
        {
            if (channel == null) throw new Exception("TradeSharpServerProxy: связь не установлена");
            try
            {
                return channel.GetCompanyTopPortfolioManagedAccounts();
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetCompanyTopPortfolioManagedAccounts();
                }
                catch (Exception ex)
                {
                    Logger.Error("GetCompanyTopPortfolioManagedAccounts()", ex);
                    return null;
                }
            }
        }
        #endregion
    }
}
