using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.UpdateServer.BL;

namespace TradeSharp.UpdateServer
{
    public partial class UpdateServer : ServiceBase
    {
        private ServiceHost hostUpdateManager;

        public UpdateServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Старт WCF...");
            try
            {
                hostUpdateManager = new ServiceHost(UpdateManager.Instance);
                hostUpdateManager.Open();
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
                hostUpdateManager.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("StartWCFHosts : ServiceHost... stopping", ex);                
            }
        }
    }
}
