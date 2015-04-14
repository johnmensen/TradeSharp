using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using TradeSharp.Contract.WebContract;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    /// <summary>
    /// останаливает и запускает службы
    /// </summary>
    static class ServiceProcessManager
    {
        public enum KillProcessStatus
        {
            OK = 0, FailedToKillTask, TaskNotFound, TaskKilled
        }

        public enum StartProcessStatus
        {
            OK = 0, NotFound, IsPending, FailedToStart
        }

        public static readonly List<TradeSharpServiceProcess> processNames;

        static ServiceProcessManager()
        {
            // заполнить словарь из файла настроек
            processNames = ConfigParser.ReadProcessInfo();
        }

        /// <summary>
        /// получить статусы всех процессов из словаря
        /// </summary>
        /// <returns></returns>
        public static List<TradeSharpServiceProcess> GetProcessesStates()
        {
            var procStatus = new List<TradeSharpServiceProcess>();
            foreach (var procName in processNames)
            {
                procName.Status = "Unknown";
                try
                {
                    using (var sc = new ServiceController(procName.Name))
                    {
                        procName.Status = EnumFriendlyName<ServiceControllerStatus>.GetString(sc.Status);
                        procStatus.Add(procName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("ServiceProcessManager - ошибка получения статуса процесса {0}: {1}",
                                       procName.Name, ex);
                }
            }
            return procStatus;
        }

        /// <summary>
        /// попытаться завершить процесс, если попытка неуспешна,
        /// попытаться найти и остановить процесс
        /// </summary>
        public static KillProcessStatus KillProcess(string procName)
        {
            if (TryKillProcess(procName)) return KillProcessStatus.OK;
            // попытаться убить процесс
            var procFile = processNames.First(p => p.Name == procName);
            Process[] allProc;
            try
            {
                allProc = Process.GetProcessesByName(procFile.FileName);	
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка процесса ОС {0}: {1}",
                        procFile, ex);
                return KillProcessStatus.TaskNotFound;
            }

            try
            {
                allProc[0].Kill();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ServiceProcessManager - ошибка завершения процесса ОС {0}: {1}",
                        procFile, ex);
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
