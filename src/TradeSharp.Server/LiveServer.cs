using System;
using System.ServiceModel;
using System.ServiceProcess;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Server.BL;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.TradeLib;
using TradeSharp.Util;

namespace TradeSharp.Server
{
    public partial class LiveServer : ServiceBase
    {
        private ServiceHost hostDic;
        private ServiceHost hostServerTrade;
        private ServiceHost hostModuleController;
        private ServiceHost hostRobotFarm;
        private ServiceHost hostWallet;
        private ServiceHost hostServerManager;
        private ServiceHost hostAccountManager;
        private ServiceHost hostPlatformManager;
        private TcpQuoteReceiver quoteReceiver;
        private HttpRequestProcessor webServer;

        public LiveServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // подгрузить метаданные
            DalSpot.Instantiate(DictionaryManager.Instance);
            // инициализация торговой библиотеки
            StartTradeLib();
            // запустить сервера WCF
            StartWcfHosts();
            // подгрузить срез котировок
            QuoteStorage.Instance.LoadQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
            // старт монитора ордеров
            AccountCheckStream.Instance.Start();
            // старт сохранения событий по счетам
            UserEventStorage.Instance.Start();

            try
            {
                hostModuleController = new ServiceHost(ModuleStatusController.Instance);
                hostModuleController.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ModuleController", ex);
            }

            // старт получения живой котиры
            try
            {
                quoteReceiver = new TcpQuoteReceiver();
                quoteReceiver.OnQuotesReceived += OnQuotesReceived;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта получения живой котировки", ex);
            }
            StartWebServer();
            // стартовать прослушку очереди сообщений от провайдеров
            try
            {
                ProviderQueueReader.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта прослушки очереди сообщений от провайдера", ex);
                throw;
            }
            try
            {
                UserEventManager.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска UserEventManager", ex);
            }
            Logger.Info("Сервис запущен");
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 8061);
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

        private static void OnQuotesReceived(string[] names, QuoteData[] quotes)
        {
            ModuleStatusController.Instance.lastQuoteTime.Value = DateTime.Now;
        }

        protected override void OnStop()
        {
            QuoteStorage.Instance.SaveQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
            AccountCheckStream.Instance.Stop();
            StopWcfHosts();
            quoteReceiver.Stop();
            StopWebServer();
            try
            {
                UserEventManager.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова UserEventManager", ex);
            }
            ProviderQueueReader.Instance.Stop();
            ServiceManagerClientManagerProxy.Instance.StopDistribution();
            UserEventStorage.Instance.Stop();
            Logger.Info("Сервис остановлен");
        }

        private static void StartTradeLib()
        {
            Logger.Info("Старт торговой библиотеки...");
            var managerTrade = ManagerTrade.Instance;
            try
            {
                var tradeManager = new TradeManager(
                    managerTrade, 
                    ManagerAccount.Instance,
                    QuoteStorage.Instance,
                    AccountRepository.Instance.GetAccountGroup);
                managerTrade.tradeManager = tradeManager;
                ProviderQueueReader.Instance.orderManager = managerTrade;
                ProviderQueueReader.Instance.OnExecutionReportReceived += managerTrade.OnProviderReport;
                ServiceManagerClientManagerProxy.Instance.StartDistribution();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка инициализации торговой библиотеки", ex);
                throw;
            }
        }

        private void StartWcfHosts()
        {
            Logger.Info("Старт WCF...");
            try
            {
                hostDic = new ServiceHost(DictionaryManager.Instance);
                hostServerTrade = new ServiceHost(ManagerTrade.Instance);
                hostRobotFarm = new ServiceHost(new RobotFarmManager());
                hostWallet = new ServiceHost(new WalletManager());
                hostServerManager = new ServiceHost(new TradeSharpServerManager());
                hostAccountManager = new ServiceHost(ManagerAccount.Instance);
                hostPlatformManager = new ServiceHost(new PlatformManager());
            }
            catch (Exception ex)
            {
                Logger.Error("StartWCFHosts : ServiceHost... ctor", ex);
                throw;
            }
            try
            {
                hostDic.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта менеджера метаданных", ex);
                throw;
            }
            try
            {
                hostServerTrade.Open();
                Logger.Info("Старт WCF (торговля) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта серверного менеджера (торговля)", ex);
                throw;
            }
            try
            {
                hostRobotFarm.Open();
                Logger.Info("Старт WCF (фермы роботов) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта серверного менеджера (фермы роботов)", ex);
                throw;
            }

            try
            {
                hostWallet.Open();
                Logger.Info("Старт WCF (Wallet Manager) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта серверного менеджера (Wallet Manager)", ex);
                throw;
            }

            try
            {
                hostAccountManager.Open();
                Logger.Info("Старт WCF (TradeSharpeAccountManager) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта TradeSharpeAccountManager", ex);
                throw;
            }

            try
            {
                hostServerManager.Open();
                Logger.Info("Старт WCF (hostServerManager) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта hostServerManager", ex);
                throw;
            }
            
            try
            {
                hostPlatformManager.Open();
                Logger.Info("Старт WCF (PlatformManager) - OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта PlatformManager", ex);
                throw;
            }

            Logger.Info("Старт WCF - OK");
        }

        private void StopWcfHosts()
        {
            Logger.Info("Останов WCF...");
            try
            {
                hostDic.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера метаданных", ex);
            }
            try
            {
                hostServerTrade.Close();
                ManagerTrade.Instance.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера сервера (торговля)", ex);
            }
            
            try
            {
                hostModuleController.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера состояния сервера (счет)", ex);
            }

            try
            {
                hostRobotFarm.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера ферм роботов", ex);
            }

            try
            {
                hostWallet.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова Wallet Manager", ex);
            }

            try
            {
                hostServerManager.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова TradeSharpServerManager", ex);
            }

            try
            {
                hostAccountManager.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова TradeSharpAccountManager", ex);
            }

            try
            {
                hostPlatformManager.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова PlatformManager", ex);
            }
            Logger.Info("Останов WCF - OK");
        }
    }
}
