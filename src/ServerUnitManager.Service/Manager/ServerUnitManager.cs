using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceProcess;
using ServerUnitManager.Contract;
using ServerUnitManager.Contract.Util;
using ServerUnitManager.Service.BL;
using System.Linq;
using TradeSharp.Util;

namespace ServerUnitManager.Service.Manager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServerUnitManager : IServerUnitManager
    {
        class ServiceUpdateStatus
        {
            public string ServiceName { get; set; }

            public int FilesUpdated { get; set; }

            public int FilesLeft { get; set; }
        }

        private readonly ThreadSafeList<ServiceUpdateStatus> serviceStatus = new ThreadSafeList<ServiceUpdateStatus>();

        public List<ServiceUnitStatus> GetServicesStatus()
        {
            var map = ServiceMap.LoadSettings();
            if (map.Items.Count == 0)
                return new List<ServiceUnitStatus>();

            var servicesBeingUpdated = UpdateServiceManager.Instance.GetNamesOfUpdatingServices();

            var statuses = map.Items.Select(i =>
                {
                    var stat = new ServiceUnitStatus
                        {
                            ServiceName = i.ServiceName,
                            IsInUpdateList = servicesBeingUpdated.Contains(i.ServiceName),
                            dependsOn = string.IsNullOrEmpty(i.DependsOn) 
                                ? new string[0] 
                                : i.DependsOn.Split(new [] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        };
                    // получить статус процесса (запущен / остановлен / ...)
                    using (var sc = new ServiceController(i.ServiceName))
                    {
                        stat.Status = sc.Status;
                    }
                    // получить статус обновления...
                    if (stat.IsInUpdateList)
                    {
                        var updateStatus = serviceStatus.Find(s => s.ServiceName == stat.ServiceName, 2000);
                        if (updateStatus != null)
                        {
                            stat.FilesUpdated = updateStatus.FilesUpdated;
                            stat.FilesLeft = updateStatus.FilesLeft;
                        }
                    }

                    return stat;
                }).ToList();

            return statuses;
        }

        public StartProcessStatus TryStartService(string serviceName)
        {
            var map = ServiceMap.LoadSettings();
            var process = map.Items.FirstOrDefault(i => i.ServiceName == serviceName);
            if (process == null)
                return StartProcessStatus.NotFound;

            return ServiceProcessManager.StartProcess(process.ServiceName);
        }

        public KillProcessStatus TryStopService(string serviceName)
        {
            var map = ServiceMap.LoadSettings();
            var process = map.Items.FirstOrDefault(i => i.ServiceName == serviceName);
            if (process == null)
                return KillProcessStatus.TaskNotFound;

            return ServiceProcessManager.KillProcess(process.ServiceName, process.ServiceExecFileName);
        }

        /// <summary>
        /// добавить службы в список менеджера обновлений
        /// </summary>
        public void TryUpdateServices(List<string> serviceNames)
        {
            var map = ServiceMap.LoadSettings();
            var services = map.Items.Where(i => serviceNames.Contains(i.ServiceName)).ToList();
            if (services.Count == 0) return;
            UpdateServiceManager.Instance.ScheduleUpdate(services);
        }

        /// <summary>
        /// этот метод вызовет программа обновления службы, сообщив свою
        /// локальную директорию и статус обновления
        /// </summary>
        public void UpdateServiceFilesStates(string serviceFolder, int filesUpdated, int filesLeft, bool updateFinished)
        {
            // найти сервис по директории
            var map = ServiceMap.LoadSettings();
            if (map.Items.Count == 0) return;
            var srv = map.Items.FirstOrDefault(m => m.Folder == serviceFolder);
            if (srv == null)
            {
                Logger.DebugFormat("Информация по сервису в каталоге [{0}] - сервис не найден",
                    serviceFolder);
                return;
            }
            
            if (updateFinished)
            {
                serviceStatus.TryRemove(s => s.ServiceName == srv.ServiceName, 2000);
                return;
            }

            var srvStatus = new ServiceUpdateStatus
                {
                    FilesLeft = filesLeft,
                    FilesUpdated = filesUpdated,
                    ServiceName = srv.ServiceName
                };
            serviceStatus.TryRemove(s => s.ServiceName == srv.ServiceName, 2000);
            serviceStatus.Add(srvStatus, 2000);
        }
    }
}
