using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using ServerUnitManager.Contract;
using TradeSharp.SiteAdmin.Contract;
using System.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class ServiceUnitStatusRepository : IServiceUnitStatusRepository
    {
        public List<ServiceUnitStatusViewModel> GetServiceUnitStatuses()
        {
            var allServices = new List<ServiceUnitStatusViewModel>();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("ServiceUnitStatus.Host")))
            {
                var host = ConfigurationManager.AppSettings[key];
                var client = CreateWcfClient(host);
                try
                {
                    var services = client.GetServicesStatus();
                    allServices.AddRange(services.Select(s => new ServiceUnitStatusViewModel(s, host)));
                }
                catch (EndpointNotFoundException)
                {
                    Logger.Error("Ошибка в GetServiceUnitStatuses - не найден хост " + host);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в GetServiceUnitStatuses()", ex);
                    throw;
                }
            }

            return allServices;
        }

        public bool StartService(string hostName, string serviceName,
                                 out string errorString)
        {
            return StartStopOrUpdateService(hostName, serviceName, false, true, false, out errorString);
        }

        public bool StopService(string hostName, string serviceName,
                                 out string errorString)
        {
            return StartStopOrUpdateService(hostName, serviceName, false, false, true, out errorString);
        }

        public bool UpdateService(string hostName, string serviceName,
                                 out string errorString)
        {
            return StartStopOrUpdateService(hostName, serviceName, true, false, false, out errorString);
        }

        private bool StartStopOrUpdateService(string hostName, string serviceName, 
            bool shouldUpdate, bool shouldStart, bool shouldStop,
            out string errorString)
        {
            SharpCodeContract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(hostName));
            SharpCodeContract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(serviceName));
            SharpCodeContract.Requires<ArgumentException>(shouldUpdate || shouldStart || shouldStop);

            errorString = string.Empty;
            try
            {
                var client = CreateWcfClient(hostName);
                if (shouldStart)
                {
                    var status = client.TryStartService(serviceName);
                    if (status != StartProcessStatus.OK)
                    {
                        errorString = status.ToString();
                        return false;
                    }
                    return true;
                }
                if (shouldStop)
                {
                    var status = client.TryStopService(serviceName);
                    if (status != KillProcessStatus.OK)
                    {
                        errorString = status.ToString();
                        return false;
                    }
                    return true;
                }
                
                // shouldUpdate == true
                client.TryUpdateServices(new List<string> { serviceName });
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка StartStopOrUpdateService({0}: {1}) - {2}",
                    hostName, serviceName, ex);
                errorString = ex.Message;
                return false;
            }
        }

        private IServerUnitManager CreateWcfClient(string endpointAddress)
        {
            var myBinding = new BasicHttpBinding();
            var myEndpoint = new EndpointAddress(endpointAddress);
            var myChannelFactory = new ChannelFactory<IServerUnitManager>(myBinding, myEndpoint);
            return myChannelFactory.CreateChannel();
        }
    }
}