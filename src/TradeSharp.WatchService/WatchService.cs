using System;
using System.ServiceProcess;
using TradeSharp.Util;
using TradeSharp.WatchService.BL;

namespace TradeSharp.WatchService
{
    public partial class WatchService : ServiceBase
    {
        private HttpRequestProcessor webServer;

        public WatchService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Запуск сервиса...");
            try
            {
                ServiceStatePool.Instance.Start();
                UdpPacketControl.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска сервиса", ex);
                throw;
            }
            StartWebServer();
            Logger.Info("Сервис запущен");
        }

        protected override void OnStop()
        {
            Logger.Info("Останов сервиса...");
            StopWebServer();
            try
            {
                ServiceStatePool.Instance.Stop();
                UdpPacketControl.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова сервиса", ex);
                throw;
            }
            Logger.Info("Сервис остановлен");
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 55061);
            try
            {
                webServer = new HttpRequestProcessor();
                webServer.Start(port, WatchWebServer.Instance.ProcessHttpRequest,
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
