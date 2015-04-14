using System;
using System.ServiceProcess;
using TradeSharp.ProviderProxy.BL;
using TradeSharp.ProviderProxy.Quote;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy
{
    public partial class ProviderProxy : ServiceBase
    {
        private bool? shouldDistributeQuotes;
        private bool ShouldDistributeQuotes
        {
            get
            {
                return shouldDistributeQuotes ??
                       (shouldDistributeQuotes =
                        AppConfig.GetStringParam("Quote.ShouldDistribute", "true") == "true").Value;
            }
        }

        public ProviderProxy()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Debug("Запуск сервиса");
            try
            {
                FileMessageLogQueue.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта очереди вывода в файл", ex);
                throw;
            }

            try
            {
                FixProxyServer.Instance.Start();
                MessageQueueReaderPool.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStart", ex);
                throw;
            }

            if (ShouldDistributeQuotes)
                try
                {
                    QuoteDistributor.Instance.StartDistribution();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка старта раздачи котировок", ex);
                    throw;
                }

            Logger.Debug("Сервис запущен");
        }

        protected override void OnStop()
        {
            Logger.Debug("Останов сервиса");

            if (ShouldDistributeQuotes)
                try
                {
                    QuoteDistributor.Instance.StopDistribution();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка останова раздачи котировок", ex);
                    throw;
                }
            FixProxyServer.Instance.Stop();
            MessageQueueReaderPool.Instance.Stop();
            try
            {
                FileMessageLogQueue.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова очереди вывода в файл", ex);
            }
            Logger.Debug("Сервис остановлен");
        }
    }
}
