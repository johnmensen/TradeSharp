using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.Processing.WebMoney.Server;
using TradeSharp.Util;

namespace TradeSharp.Processing.WebMoney
{
    public partial class WebMoneyService : ServiceBase
    {
        private ServiceHost hostPaymentProcessor;

        public WebMoneyService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WebMoneyProcessor.Instance.Start();
                hostPaymentProcessor = new ServiceHost(WebMoneyProcessor.Instance);
                hostPaymentProcessor.Open();
                Logger.Info("Служба WebMoneyService стартовала");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStart для WebMoneyService", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                WebMoneyProcessor.Instance.Stop();
                hostPaymentProcessor.Close();
                Logger.Info("WebMoneyService остановлена");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStop WebMoneyService", ex);
            }
        }
    }
}
