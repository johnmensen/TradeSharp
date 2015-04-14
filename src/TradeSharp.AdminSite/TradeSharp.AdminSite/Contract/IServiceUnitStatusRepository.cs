using System.Collections.Generic;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IServiceUnitStatusRepository
    {
        List<ServiceUnitStatusViewModel> GetServiceUnitStatuses();

        bool StartService(string hostName, string serviceName, out string errorString);

        bool StopService(string hostName, string serviceName, out string errorString);

        bool UpdateService(string hostName, string serviceName, out string errorString);
    }
}