using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.Contract.Util.BL;
using TradeSharp.TradeSignalExecutor.BL;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor
{
    public partial class TradeSignalExecutor : ServiceBase
    {
        private ServiceHost hostServerStat;
        private TcpQuoteReceiver quoteReceiver;
        private ServiceHost hostServerStatusController;
        private HttpRequestProcessor webServer;

        public TradeSignalExecutor()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                hostServerStatusController = new ServiceHost(ModuleStatusController.Instance);
                hostServerStatusController.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStart для ModuleStatusController", ex);
                throw;
            }

            quoteReceiver = new TcpQuoteReceiver();

            try
            {
                SignalExecutor.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта контроллера", ex);
                throw;
            }

            try
            {
                PAMMFeeManager.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта PAMM-менеджера", ex);
                throw;
            }

            try
            {
                hostServerStat = new ServiceHost(SignalExecutor.Instance);
                hostServerStat.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта WCF-хоста", ex);
                throw;
            }
            StartWebServer();
            Logger.Info("Служба TradeSignalExecutor запущена");
        }

        protected override void OnStop()
        {
            quoteReceiver.Stop();
            StopWebServer();
            try
            {
                SignalExecutor.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова контроллера", ex);
            }

            try
            {
                PAMMFeeManager.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова PAMM-менеджера", ex);
            }

            try
            {
                hostServerStatusController.Close();
                Logger.Info("Объект состояния остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова объекта состояния сервиса", ex);
            }

            try
            {
                hostServerStat.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова WCF-хоста", ex);
            }
            Logger.Info("Служба TradeSignalExecutor остановлена");
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 8092);
            try
            {
                webServer = new HttpRequestProcessor();
                webServer.Start(port, WebServer.Instance.ProcessHttpRequest,
                    BaseWebServer.needAuthentication);
                Logger.InfoFormat("Старт web-сервера - ОК ({0})", port);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка старта веб-сервера (порт {0}): {1}", port, ex);
                webServer = null;
                throw;
            }
        }

        private void StopWebServer()
        {
            if (webServer == null) return;
            try
            {
                webServer.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова веб-сервера", ex);
            }
        }
    }
}
