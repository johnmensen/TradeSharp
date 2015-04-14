using System.ServiceProcess;
using ServerUnitManager.Contract.Util;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    public class ServiceUnitStatusViewModel : ServiceUnitStatus
    {
        public string HostEndpoint { get; set; }

        public ServiceUnitStatusViewModel()
        {
        }

        public ServiceUnitStatusViewModel(ServiceUnitStatus status, string hostEndpoint)
        {
            Status = status.Status;
            IsInUpdateList = status.IsInUpdateList;
            ServiceCode = status.ServiceCode;
            ServiceProcessName = status.ServiceProcessName;
            ServiceName = status.ServiceName;
            HostEndpoint = hostEndpoint;
            FilesLeft = status.FilesLeft;
            FilesUpdated = status.FilesUpdated;
            dependsOn = status.dependsOn;
        }

        public string StatusName
        {
            get { return EnumFriendlyName<ServiceControllerStatus>.GetString(Status); }
        }
    }
}