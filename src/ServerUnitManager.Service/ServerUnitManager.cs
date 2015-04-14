using System;
using System.ServiceModel;
using System.ServiceProcess;
using ServerUnitManager.Service.BL;
using TradeSharp.Util;
using ServerManager = ServerUnitManager.Service.Manager.ServerUnitManager;

namespace ServerUnitManager.Service
{
    public partial class ServerUnitManager : ServiceBase
    {
        private ServiceHost hostServerUnitManager;

        private ServerManager serverManager;

        public ServerUnitManager()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            serverManager = new ServerManager();
            Logger.Info("Старт WCF...");
            try
            {
                hostServerUnitManager = new ServiceHost(serverManager);
                hostServerUnitManager.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("StartWCFHosts : ServiceHost... ctor", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                hostServerUnitManager.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("StartWCFHosts : ServiceHost... stopping", ex);                
            }

            UpdateServiceManager.Instance.Stop();
            Logger.Info("Служба остановлена");
        }
    }
}
