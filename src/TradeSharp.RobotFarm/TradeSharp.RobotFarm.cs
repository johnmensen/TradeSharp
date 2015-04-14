using System;
using System.ServiceProcess;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.Robot;
using TradeSharp.RobotFarm.BL.Web;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm
{
    public partial class RobotFarmService : ServiceBase
    {
        private HttpRequestProcessor webServer;
        private TcpQuoteReceiver quoteReceiver;

        public RobotFarmService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                RobotCollection.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка инициализации коллекции роботов", ex);
                throw;
            }

            QuoteStorage.Instance.LoadQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
            try
            {
                quoteReceiver = new TcpQuoteReceiver();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения котировок", ex);
                throw;
            }
            
            Logger.InfoFormat("Старт сервиса - загрузка настроек");
            BL.RobotFarm.Instance.LoadSettings();
            quoteReceiver.OnQuotesReceived += BL.RobotFarm.Instance.OnQuotesReceived;
            StartWebServer();
        }

        protected override void OnStop()
        {
            quoteReceiver.OnQuotesReceived -= BL.RobotFarm.Instance.OnQuotesReceived;
            StopWebServer();
            try
            {
                BL.RobotFarm.Instance.StopFarm();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова фермы", ex);
            }
            try
            {
                QuoteStorage.Instance.SaveQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения последних котировок", ex);
            }
            try
            {
                quoteReceiver.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова получения котировок", ex);
            }
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 8091);
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
