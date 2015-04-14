using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class RobotFarmProxy : IRobotFarm
    {
        private ChannelFactory<IRobotFarm> factory;
        private IRobotFarm channel;
        private readonly string endpointName;

        public RobotFarmProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<IRobotFarm>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception)
            {
                Logger.Error("RobotFarmProxy: невозможно создать прокси");
                channel = null;
            }
        }

        public bool GetAccountData(string login, string pwrd, int accountId,
            out Account account, out List<MarketOrder> openedOrders)
        {
            if (channel == null) throw new Exception("RobotFarmProxy: связь не установлена");
            try
            {
                return channel.GetAccountData(login, pwrd, accountId, out account, out openedOrders);
            }
            catch (Exception)
            {
                RenewFactory();
                return channel.GetAccountData(login, pwrd, accountId, out account, out openedOrders);
            }
        }
    }
}
