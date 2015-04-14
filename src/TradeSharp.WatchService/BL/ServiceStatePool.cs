using System;
using System.Collections.Generic;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    public class ServiceStatePool
    {
        private readonly List<ServiceStateUnit> units = new List<ServiceStateUnit>();

        /// <summary>
        /// интервал опроса состояния сервисов, секунд
        /// </summary>
        private readonly int pollInterval;

        private Thread pollThread;

        private volatile bool serviceIsStopping;

        #region Singleton

        private static readonly Lazy<ServiceStatePool> instance =
            new Lazy<ServiceStatePool>(() => new ServiceStatePool());

        public static ServiceStatePool Instance
        {
            get { return instance.Value; }
        }

        private ServiceStatePool()
        {
            try
            {
                pollInterval = int.Parse(AppConfig.GetStringParam("PollIntervalSeconds", "30"));
                units.AddRange(ConfigParser.ReadUnits().FindAll(u => u.UdpBinding == false));
            }
            catch (Exception ex)
            {
                Logger.Error("Error in ServiceStatePool ctor", ex);
                throw;
            }
        }

        #endregion

        public void Start()
        {
            pollThread = new Thread(Poll);
            pollThread.Start();
        }

        public void Stop()
        {
            Logger.Info("Останов сервиса...");
            serviceIsStopping = true;
            pollThread.Join();
            Logger.Info("Сервис остановлен");
        }

        private void Poll()
        {
            var pollCounter = 0;
            while (!serviceIsStopping)
            {
                pollCounter++;
                if (pollCounter >= pollInterval)
                {
                    DoPollServices();
                    pollCounter = 0;
                }

                Thread.Sleep(1000);
            }
        }

        private void DoPollServices()
        {
            foreach (var stateUnit in units)
            {
                try
                {
                    stateUnit.GetServiceState();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in stateUnit.GetServiceState()", ex);
                    throw;
                }

                if (stateUnit.ShouldReport())
                {
                    // доложить об ошибке (сбое)
                    try
                    {
                        Reporter.SendReports(stateUnit);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error in SendReports()", ex);
                        throw;
                    }
                }
            }
        }

        public Dictionary<string, ServiceStateInfo> GetServiceState()
        {
            var states = new Dictionary<string, ServiceStateInfo>();
            foreach (var unit in units)
            {
                states.Add(unit.Name, unit.LastServiceState);
            }
            return states;
        }

        public void ResetServiceError(string name)
        {
            try
            {
                var unit = units.Find(u => u.Name == name);
                unit.ResetErrorState();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("ResetServiceError({0})", name), ex);
            }
        }
    }
}
