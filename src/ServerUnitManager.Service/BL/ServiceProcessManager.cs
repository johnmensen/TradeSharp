using System;
using System.Diagnostics;
using System.ServiceProcess;
using ServerUnitManager.Contract;
using TradeSharp.Util;

namespace ServerUnitManager.Service.BL
{
    /// <summary>
    /// останаливает и запускает службы
    /// </summary>
    static class ServiceProcessManager
    {
        /// <summary>
        /// попытаться завершить процесс, если попытка неуспешна,
        /// попытаться найти и остановить процесс
        /// </summary>
        public static KillProcessStatus KillProcess(string processName, string processFileName)
        {
            if (TryKillProcess(processName)) return KillProcessStatus.OK;
            // попытаться убить процесс
            Process[] allProc;
            try
            {
                allProc = Process.GetProcessesByName(processFileName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка процесса ОС {0}: {1}",
                        processFileName, ex);
                return KillProcessStatus.TaskNotFound;
            }

            try
            {
                allProc[0].Kill();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка завершения процесса ОС {0}: {1}",
                        processFileName, ex);
                return KillProcessStatus.FailedToKillTask;
            }

            return KillProcessStatus.TaskKilled;
        }

        /// <summary>
        /// стартовать процесс
        /// </summary>
        public static StartProcessStatus StartProcess(string procName)
        {
            ServiceController sc;
            try
            {
                sc = new ServiceController(procName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка старта (получения статуса) процесса {0}: {1}",
                        procName, ex);
                return StartProcessStatus.NotFound;
            }
            try
            {
                if (sc.Status != ServiceControllerStatus.Stopped) return StartProcessStatus.IsPending;
                try
                {
                    sc.Start();
                    return StartProcessStatus.OK;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("ServiceProcessManager - ошибка старта процесса {0}: {1}",
                                       procName, ex);
                    return StartProcessStatus.FailedToStart;
                }
            }
            finally
            {
                sc.Close();
            }
        }

        public static ServiceControllerStatus GetServiceState(ServiceUnit service)
        {
            using (var sc = new ServiceController(service.ServiceName))
            {
                return sc.Status;
            }
        }

        private static bool TryKillProcess(string procName)
        {
            ServiceController sc;
            try
            {
                sc = new ServiceController(procName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка получения статуса процесса {0}: {1}",
                        procName, ex);
                return false;
            }
            try
            {

                if (sc.Status == ServiceControllerStatus.Stopped) return true;
                if (sc.Status == ServiceControllerStatus.StartPending ||
                    sc.Status == ServiceControllerStatus.StopPending)
                    return false;
                try
                {
                    sc.Stop();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("ServiceProcessManager - ошибка останова процесса {0}: {1}",
                                       procName, ex);
                    return false;
                }
            }
            finally
            {
                sc.Close();
            }
            return true;
        }
    }
}
