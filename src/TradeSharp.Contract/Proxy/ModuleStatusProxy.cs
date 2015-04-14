using System;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class ModuleStatusProxy : IModuleStatus
    {
        private ChannelFactory<IModuleStatus> factory;
        private IModuleStatus channel;
        private readonly string endpointName;
        private ServiceStateInfo stateInfo;

        public ModuleStatusProxy(string endpointName)
        {
            this.endpointName = endpointName;
            stateInfo = new ServiceStateInfo(ServiceProcessState.OK);
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<IModuleStatus>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("ModuleStatusProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }

        public ServiceStateInfo GetModuleStatus()
        {
            if (channel == null) throw new Exception("ModuleStatusProxy: связь не установлена");
            try
            {
                return channel.GetModuleStatus();
            }
            catch (CommunicationException)
            {

                RenewFactory();
                try
                {
                    return channel.GetModuleStatus();
                }
                catch (Exception ex)
                {
                    return new ServiceStateInfo(ServiceProcessState.Offline);
                }
            }
        }        

        public string GetModuleExtendedStatusString()
        {
            if (channel == null) throw new Exception("ModuleStatusProxy: связь не установлена");
            try
            {
                return channel.GetModuleExtendedStatusString();
            }
            catch (CommunicationException)
            {
                RenewFactory();
                try
                {
                    return channel.GetModuleExtendedStatusString();
                }
                catch (Exception ex)
                {
                    // !!
                    Logger.Error("GetModuleExtendedStatusString error", ex);
                    return string.Empty;
                    
                }
                
            }
        }

        public void ResetStatus()
        {
            if (channel == null) throw new Exception("ModuleStatusProxy: связь не установлена");
            try
            {
                channel.ResetStatus();
            }
            catch (CommunicationException)
            {
                RenewFactory();
                if (channel != null) channel.ResetStatus();
            }
        }
    }    
}
