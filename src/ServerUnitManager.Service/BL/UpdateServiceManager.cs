using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using TradeSharp.Util;

namespace ServerUnitManager.Service.BL
{
    class UpdateServiceManager
    {
        #region Singletone
        private static readonly Lazy<UpdateServiceManager> instance = new Lazy<UpdateServiceManager>(() => new UpdateServiceManager());

        public static UpdateServiceManager Instance
        {
            get { return instance.Value; }
        }

        private UpdateServiceManager()
        {
            updateCheckingThread = new Thread(CheckUpdateRoutine);
            updateCheckingThread.Start();
        }
        #endregion

        private static readonly int maxSecondsBeforeKillProcess = 
            AppConfig.GetIntParam("MaxSecondsBeforeKillProcess", 10);

        private volatile bool isStopping;

        private readonly Thread updateCheckingThread;

        private readonly ConcurrentQueue<ServiceUnit> unitsUpdating = new ConcurrentQueue<ServiceUnit>();

        public void ScheduleUpdate(List<ServiceUnit> units)
        {
            foreach (var unit in units)
            {
                if (unitsUpdating.All(u => u.ServiceName != unit.ServiceName))
                    unitsUpdating.Enqueue(unit);
            }
        }
    
        public void Stop()
        {
            isStopping = true;
            updateCheckingThread.Join();
        }

        public List<string> GetNamesOfUpdatingServices()
        {
            return unitsUpdating.Select(u => u.ServiceName).ToList();
        }

        private void CheckUpdateRoutine()
        {
            while (!isStopping)
            {
                DoCheckUpdate();
                Thread.Sleep(200);
            }
        }  
      
        private void DoCheckUpdate()
        {
            ServiceUnit unit;
            if (!unitsUpdating.TryDequeue(out unit))
                return;
            // если служба запущена - попытатся остановить
            // если в статусе "Останавливается..." и прошло N секунд - убить
            // если остановлена - запустить программу обновления и убрать службу из очереди
            // (точнее, не добавлять в конец)
            var unitState = ServiceProcessManager.GetServiceState(unit);

            // если остановлена - запустить программу обновления и убрать службу из очереди
            if (unitState == ServiceControllerStatus.Stopped)
            {
                StartServiceUpdateManager(unit);
                return;
            }

            // если в статусе "Останавливается..." и прошло N секунд - убить
            if (unitState == ServiceControllerStatus.StopPending)
            {
                if (!unit.StopCommandSentTime.HasValue)
                    unit.StopCommandSentTime = DateTime.Now;
                else
                {
                    var deltaTime = (DateTime.Now - unit.StopCommandSentTime.Value).TotalSeconds;
                    if (deltaTime > maxSecondsBeforeKillProcess)
                        ServiceProcessManager.KillProcess(unit.ServiceName, unit.ServiceExecFileName);
                }
                unitsUpdating.Enqueue(unit);
                return;
            }

            // если служба запущена - попытатся остановить
            if (unitState == ServiceControllerStatus.Paused ||
                unitState == ServiceControllerStatus.StartPending ||
                unitState == ServiceControllerStatus.Running ||
                unitState == ServiceControllerStatus.ContinuePending)
            {
                ServiceProcessManager.KillProcess(unit.ServiceName, unit.ServiceExecFileName);
                unit.StopCommandSentTime = DateTime.Now;
                unitsUpdating.Enqueue(unit);
                return;
            }
        }

        private void StartServiceUpdateManager(ServiceUnit unit)
        {
            var updateMgrPath = unit.Folder.TrimEnd('\\') + "\\TradeSharp.UpdateManager.exe";
            if (!File.Exists(updateMgrPath))
            {
                Logger.ErrorFormat("Ошибка обновления службы {0} - файл не найден ({1})",
                    unit.ServiceName, updateMgrPath);
                return;
            }

            Process.Start(updateMgrPath);
        }
    }
}
