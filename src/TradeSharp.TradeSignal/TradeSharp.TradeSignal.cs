using System;
using System.ServiceProcess;
using TradeSharp.TradeSignal.BL;
using TradeSharp.Util;

namespace TradeSharp.TradeSignal
{
    public partial class TradeSignal : ServiceBase
    {
        private HttpRequestProcessor webServer;

        public TradeSignal()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartWebServer();
        }

        protected override void OnStop()
        {
            StopWebServer();
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 8062);
            try
            {
                webServer = new HttpRequestProcessor();
                webServer.Start(port, SignalWebServer.Instance.ProcessHttpRequest, 
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
