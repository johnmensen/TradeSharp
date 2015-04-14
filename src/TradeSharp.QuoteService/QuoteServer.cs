using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteService.Distribution;
using TradeSharp.QuoteService.Feeder;
using TradeSharp.QuoteService.Index;
using TradeSharp.QuoteService.ModuleControl;
using TradeSharp.QuoteService.News;
using TradeSharp.QuoteService.QuoteStorage;
using TradeSharp.QuoteService.WebServer;
using TradeSharp.Util;

namespace TradeSharp.QuoteService
{
    public partial class QuoteServer : ServiceBase
    {
        private IQuoteFeeder feeder;
        private ServiceHost hostCache;
        private ServiceHost hostNewsReceiver;
        private ServiceHost hostNewsStorage;
        private ServiceHost hostModuleController;
        private IndexMaker indexMaker;
        private readonly ThreadSafeStorage<string, QuoteData>
            lastPrices = new ThreadSafeStorage<string, QuoteData>();
        private readonly bool distributeQuotesMt4 = AppConfig.GetBooleanParam("MT4.DeliverQuotes", false);
        
        private readonly float maxQuoteDeltaPercent = AppConfig.GetStringParam("Quote.MaxDeltaPercent", "8").ToFloatUniform();
        private readonly bool checkJpyLess50 = AppConfig.GetBooleanParam("Quote.CheckJPYLess50", true);
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);
        private const int LogMsgWrongQuote = 1;
        private const int LogMsgQuoteFeedNew = 2;
        private const int LogMsgQuoteFeedFiltered = 3;
        private HttpRequestProcessor webServer;
        private QuoteFilter filter;

        public QuoteServer()
        {
            InitializeComponent();

            
        }

        protected override void OnStart(string[] args)
        {
            ThreadPool.QueueUserWorkItem(StartRoutine);
            Logger.Info("Starting server...");
        }

        private void StartRoutine(object ptrs)
        {
            // получение котировок
            filter = new QuoteFilter();
            var feederType = AppConfig.GetStringParam("Feeder", "Test");
            if (feederType.ToUpper() == "UDP") MakeUDPFeeder();
            if (feederType.ToUpper() == "TCP") MakeTCPFeeder();
            if (feederType.ToUpper() == "TEST") MakeRandomFeeder();
            feeder.OnQuotesReceived += FeederOnQuotesReceived;
            indexMaker = new IndexMaker();

            feeder.Start();

            hostCache = new ServiceHost(QuoteStorageManager.Instance);
            try
            {
                hostNewsReceiver = new ServiceHost(NewsReceiver.Instance);
            }
            catch (Exception ex) { Logger.Error("hostNewsReceiver error", ex); }
            try
            {
                hostNewsStorage = new ServiceHost(NewsStorage.Instance);
            }
            catch (Exception ex) { Logger.Error("hostNewsStorage error", ex); }

            try
            {
                hostModuleController = new ServiceHost(ModuleStatusController.Instance);
                hostModuleController.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ModuleController", ex);
            }

            try
            {
                hostCache.Open();
                NewsReceiver.Instance.Start();
                hostNewsReceiver.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта менеджера котировок - новостей", ex);
                throw;
            }

            try
            {
                hostNewsStorage.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта хранилища новостей", ex);
                throw;
            }
            try
            {
                BaseNewsDistributor.Instance.StartPolling();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта раздатчика котировок и новостей", ex);
                throw;
            }

            try
            {
                CandleStoreStream.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта потока сохранения котировок", ex);
                throw;
            }

            // стартовать раздачу котировок в MT4
            if (distributeQuotesMt4)
                try
                {
                    Mt4Feeder.Instance.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка старта раздачи котировок в МТ4", ex);
                }

            StartWebServer();

            Logger.InfoFormat("Сервис запущен");
        }

        protected override void OnStop()
        {
            StopWebServer();

            try
            {
                feeder.Stop();
                Logger.Info("Источник котировок остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова источника котировок", ex);
            }

            try
            {
                BaseNewsDistributor.Instance.StopPolling();
                Logger.Info("Раздача новостей остановлена");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова раздачи новостей/котировок", ex);
            }

            try
            {
                hostCache.Close();
                Logger.Info("Менеджер котировок остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера (кеша) котировок", ex);
            }
            try
            {
                NewsReceiver.Instance.Stop();
                hostNewsReceiver.Close();
                Logger.Info("Получение новостей остановлено");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова менеджера получения новостей", ex);
            }

            try
            {
                hostNewsStorage.Close();
                Logger.Info("Хранилище новостей остановлено");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова хранилища новостей", ex);
            }

            try
            {
                hostModuleController.Close();
                Logger.Info("Объект состояния остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова объекта состояния сервиса", ex);
            }

            try
            {
                CandleStoreStream.Instance.Stop();
                Logger.Info("Поток сохранения котировок остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка останова потока сохранения котировок", ex);
            }
            // остановить раздачу котировок в MT4
            if (distributeQuotesMt4)
                try
                {
                    Mt4Feeder.Instance.Stop();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка останова раздачи котировок в МТ4", ex);
                }

            Logger.InfoFormat("Сервис остановлен");
        }

        private void FeederOnQuotesReceived(List<string> symbolList, List<QuoteData> quoteList)
        {
            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgQuoteFeedNew,
                1000 * 30, "FeederOnQuotesReceived({0})", symbolList.Count);
            // отфильтровать котировки и обновить список актуальных котировок
            FilterQuotes(symbolList, quoteList);
            if (symbolList.Count == 0)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgQuoteFeedFiltered,
                    1000 * 30, "All quotes have been filtered");
                return;
            }
            
            var list = new List<IBaseNews>();
            for (var i = 0; i < symbolList.Count; i++)
            {
                var quote = quoteList[i];
                list.Add(new TickerQuoteData { Ticker = symbolList[i], bid = quote.bid, ask = quote.ask });
            }
            if (list.Count == 0) return;

            // слепить из котировок индексы и тоже добавить их в список рассылки
            if (indexMaker.HasIndicies)
            {
                var indexes = indexMaker.MakeIndicies(list);
                if (indexes.Count > 0) list.AddRange(indexes);
            }

            BaseNewsDistributor.Instance.Insert(list);
            if (distributeQuotesMt4) Mt4Feeder.Instance.Enqueue(symbolList, quoteList);
            
            // обновить котировки в БД
            var tickerQuotes = list.Cast<TickerQuoteData>().ToList();
            
            // обновить свечки в БД
            CandleStorage.Instance.UpdateQuotes(tickerQuotes);
            // ... и в безопасном словаре
            SafeQuoteStorage.Instance.UpdateQuotes(tickerQuotes);
        }

        private void FilterQuotes(List<string> symbolList, List<QuoteData> quoteList)
        {
            for (var i = 0; i < symbolList.Count; i++)
            {
                var quote = quoteList[i];
                var name = symbolList[i];

                if (!filter.IsValid(name, quote))
                {
                    symbolList.RemoveAt(i);
                    quoteList.RemoveAt(i);
                    i--;
                    //continue;
                }
            }

            if (symbolList.Count > 0)
                lastPrices.UpdateValues(symbolList.ToArray(), quoteList.ToArray());
        }

        private void MakeUDPFeeder()
        {
            var ownPort = AppConfig.GetIntParam("UDP.Port", 8045);
            feeder = new UdpFeeder(ownPort);
        }
        
        private void MakeRandomFeeder()
        {
            feeder = new TestFeeder();
        }

        private void MakeTCPFeeder()
        {
            feeder = new TCPFeeder();
        }

        private void StartWebServer()
        {
            var port = AppConfig.GetIntParam("WebServer.Port", 55060);
            try
            {
                webServer = new HttpRequestProcessor();
                webServer.Start(port, QuoteWebServer.Instance.ProcessHttpRequest,
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
