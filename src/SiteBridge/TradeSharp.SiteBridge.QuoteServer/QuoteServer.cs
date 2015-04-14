using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.SiteBridge.QuoteServer.BL;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.QuoteServer
{
    public partial class QuoteServer : ServiceBase
    {
        private ServiceHost hostServer;

        public QuoteServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                CandleStorage.Instance.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска CandleStorage", ex);
                throw;
            }
            try
            {
                hostServer = new ServiceHost(CandleStorage.Instance);
                hostServer.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка открытия хоста", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                CandleStorage.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова CandleStorage", ex);
            }
            try
            {
                hostServer.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова хоста", ex);
            }
        }
    }
}
