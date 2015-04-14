using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.Delivery.Service.WebServer;
using TradeSharp.Util;

namespace TradeSharp.Delivery.Service
{
    /// <summary>
    /// Класс Windows-службы для хостинга службы 'EmailSender'
    /// </summary>
    public partial class DeliveryService : ServiceBase
    {
        private ServiceHost hostServerStat;
        private ServiceHost hostServerStatusController;


        private HttpRequestProcessor webServer;


        public DeliveryService()
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

            try
            {
                EmailSender.Instance.Start();
                hostServerStat = new ServiceHost(EmailSender.Instance);
                hostServerStat.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStart для EmailSender", ex);
                throw;
            }
            StartWebServer();
        }

        protected override void OnStop()
        {
            try
            {
                hostServerStatusController.Close();
                Logger.Info("ModuleStatusController остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStop ModuleStatusController", ex);
            }


            try
            {
                hostServerStat.Close();
                EmailSender.Instance.Stop();
                Logger.Info("EmailSender остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStop EmailSender", ex);
            }
            StopWebServer();

            Logger.Info("Сервис остановлен");
        }

        #region Работа с сервером (отчет о состоянии сужбы)
        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 8093);
            try
            {
                webServer = new HttpRequestProcessor();
                webServer.Start(port, DeliveryWebServer.Instance.ProcessHttpRequest, BaseWebServer.needAuthentication);
                Logger.InfoFormat("Старт web-сервера - ОК ({0})", port);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка старта веб-сервера (порт {0}): {1}", port, ex);
                webServer = null;
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
        #endregion

    }
}
