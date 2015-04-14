using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.ServiceModel.Configuration;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    public class ServerUnitInfoContext
    {
        private static readonly List<string> endpointNames;
        private static readonly Dictionary<string, string> endpointFriendlyName;

        static ServerUnitInfoContext()
        {
            // прочитать endpoint-ы из файла web.config
            var clientSection = (ClientSection)ConfigurationManager.GetSection("system.serviceModel/client");

            var endpointCollection = (ChannelEndpointElementCollection)clientSection.ElementInformation.Properties[string.Empty].Value;
            endpointNames = new List<string>();
            foreach (ChannelEndpointElement endpointElement in endpointCollection)
            {
                if (!endpointElement.Contract.EndsWith(".IModuleStatus"))
                    continue;
                endpointNames.Add(endpointElement.Name);
            }

            // прочитать записи - читаемые имена endpoint-ов
            endpointFriendlyName = new Dictionary<string, string>();
            foreach (var endpoint in endpointNames)
            {
                var key = AppConfig.GetStringParam("Name." + endpoint, endpoint);
                var friendlyName = Resource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
                endpointFriendlyName.Add(endpoint, friendlyName);
            }
        }

        public List<ServerUnitInfo> ServerUnits
        {
            get
            {
                var infos = new List<ServerUnitInfo>();

                foreach (var endpoint in endpointNames)
                {
                    var inf = new ServerUnitInfo
                        {
                            UnitName = endpointFriendlyName[endpoint]
                        };
                    try
                    {
                        var proxy = new ModuleStatusProxy(endpoint);
                        var status = proxy.GetModuleStatus();
                        inf.ModuleStatus = status.State;
                        // доп. сведения об ошибке
                        if (status.State != ServiceProcessState.OK)
                        {
                            inf.ExtendedStatusInfo = proxy.GetModuleExtendedStatusString();
                        }
                    }
                    catch
                    {
                        inf.ModuleStatus = ServiceProcessState.Offline;
                    }
                    infos.Add(inf);
                }
                return infos;
            }
        }
    }
}