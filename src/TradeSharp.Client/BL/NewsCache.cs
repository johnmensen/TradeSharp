using System;
using System.Threading;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// хранит кеш новостей, актуализирует его по запросу
    /// </summary>
    class NewsCache
    {
        private static NewsCache instance;
        public static NewsCache Instance
        {
            get { return instance ?? (instance = new NewsCache()); }
        }

        private const int StopAskingServerAfterFails = 4;
        
        private readonly string mapPath;

        private NewsMap map;

        private Thread threadActualize;

        private volatile bool isTerminating;

        public ManualResetEventSlim syncCompletedEvent;

        private Action<int> actualizationCompleted;

        public event Action<int> ActualizationCompleted
        {
            add { actualizationCompleted += value; }
            remove { actualizationCompleted -= value; }
        }

        private NewsCache()
        {
            syncCompletedEvent = new ManualResetEventSlim(false);
            mapPath = NewsLocalStorage.newsPath + "\\news_map.xml";
            NewsLocalStorage.Instance.EnsureNewsPath();
            
            // подкачать карту новостей (канал - дата - количество новостей)
            map = NewsMap.LoadFromFile(mapPath) ?? new NewsMap
                {
                    channelIds = new int[0],
                    records = new NewsMapRecord[0]
                };
        }

        /// <summary>
        /// запустить асинхронный процесс актуализации новостей
        /// </summary>
        public void ActualizeAsync(int accountId = 0)
        {
            if (threadActualize != null && threadActualize.IsAlive) return;
            threadActualize = new Thread(ActualizeSync);

            if (isTerminating) return;
            syncCompletedEvent.Reset();
            threadActualize.Start(accountId);
        }

        public void Terminate()
        {
            isTerminating = true;
            try
            {
                threadActualize.Join();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в NewsCache.Terminate()", ex);
            }
        }

        private void ActualizeSync(object accountIdObj)
        {
            var totalNews = 0;
            try
            {
                if (isTerminating) return;
                var accountId = (int)accountIdObj;
                if (accountId == 0)
                {
                    var accountData = AccountStatus.Instance.AccountData;
                    if (accountData != null)
                        accountId = accountData.ID;
                    if (accountId == 0) return;
                }

                // получить от сервера карту новостей
                NewsMap serverNewsMap = null;
                try
                {
                    using (var proxy = new NewsStorageProxy(TerminalBindings.BindingNewsStorage))
                    {
                        serverNewsMap = proxy.GetNewsMap(accountId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("NewsCache({0}) - ошибка получения карты новостей: {1}",
                        accountId, ex);
                }
                if (serverNewsMap == null) return;
                if (isTerminating) return;

                // сформировать список каналов / дат, на которые нужно подкачать новости
                var lackMap = map.MakeMapOfLackedNews(serverNewsMap);
                if (lackMap.records.Length == 0 || serverNewsMap.channelIds.Length == 0)
                    return;

                // подкачать новости по указанным каналам за указанные даты
                var numFailsLeft = StopAskingServerAfterFails + 1;
                using (var proxy = new NewsStorageProxy(TerminalBindings.BindingNewsStorage))
                    foreach (var rec in lackMap.records)
                    {
                        if (isTerminating) break;

                        try
                        {
                            var news = proxy.GetNews(accountId, rec.date, serverNewsMap.channelIds);
                            if (news == null || news.Count == 0) continue;
                            totalNews += news.Count;
                            // обновить кеш новостей
                            NewsLocalStorage.Instance.UpdateNews(news);
                        }
                        catch (Exception ex)
                        {
                            numFailsLeft--;
                            if (numFailsLeft == 0) break;
                            Logger.Error("Ошибка в NewsCache.ActualizeSync()", ex);
                        }
                    }

                // сохранить кешированные новости
                NewsLocalStorage.Instance.SaveNewsInFiles();
                // обновить карту новостей
                map = NewsLocalStorage.Instance.MakeNewsMap(); //serverNewsMap;
                map.SaveInFile(mapPath);
            }
            finally
            {
                syncCompletedEvent.Set();
            }
            if (actualizationCompleted != null && !isTerminating)
                actualizationCompleted(totalNews);
        }        
    }
}
