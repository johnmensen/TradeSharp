using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// потокобезопасное хранилище новостей
    /// </summary>
    public class NewsStorage
    {
        private static NewsStorage instance;
        public static NewsStorage Instance
        {
            get { return instance ?? (instance = new NewsStorage()); }
        }

        private NewsStorage()
        {
        }
        private readonly Dictionary<int, List<News>> dicNews = new Dictionary<int, List<News>>();
        private readonly ReaderWriterLock lockNews = new ReaderWriterLock();
        private const int LockTimeout = 1000;        
        
        public bool AddNews(News[] news)
        {
            if (news.Length == 0) return false;
            try
            {
                lockNews.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return false;
            }
            try
            {
                foreach (var ns in news)
                {
                    if (!dicNews.ContainsKey(ns.ChannelId))
                    {
                        var lst = new List<News> {ns};
                        dicNews.Add(ns.ChannelId, lst);
                    }
                    else
                        dicNews[ns.ChannelId].Add(ns);
                }
            }
            finally
            {
                lockNews.ReleaseWriterLock();
            }
            return true;
        }

        public bool AddNews(List<News> news)
        {
            if (news == null || news.Count == 0) return false;
            try
            {
                lockNews.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return false;
            }
            try
            {
                foreach (var ns in news)
                {
                    if (!dicNews.ContainsKey(ns.ChannelId))
                    {
                        var lst = new List<News> {ns};
                        dicNews.Add(ns.ChannelId, lst);
                    }
                    else
                        dicNews[ns.ChannelId].Add(ns);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в AddNews(" + news.Count + "): " + ex);
            }
            finally
            {
                lockNews.ReleaseWriterLock();
            }
            return true;
        }
    
        /// <summary>
        /// читает новости, не извлекая их
        /// </summary>
        /// <param name="channelId">опционально - вернуть новости только для переданного канала</param>
        /// <param name="lstNews"></param>
        /// <returns>операция успешна</returns>
        public bool ReadNews(int? channelId, out List<News> lstNews)
        {
            lstNews = new List<News>();
            try
            {
                lockNews.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return false;
            }
            try
            {
                if (channelId.HasValue)
                {
                    if (!dicNews.ContainsKey(channelId.Value)) return true;
                    var lst = dicNews[channelId.Value];
                    foreach (var ns in lst) lstNews.Add(ns);
                    return true;
                }

                foreach (var lst in dicNews.Values)
                    foreach (var ns in lst) lstNews.Add(ns);
                return true;
            }
            finally
            {
                lockNews.ReleaseReaderLock();
            }            
        }

        public bool GetLastNews(out News news)
        {
            news = null;
            try
            {
                lockNews.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return false;
            }
            try
            {
                foreach (var lst in dicNews.Values)
                {
                    foreach (var ns in lst)
                    {
                        if (news == null)
                        {
                            news = ns;
                            continue;
                        }
                        if (ns.Time > news.Time) news = ns;
                    }
                }
                return true;
            }
            finally
            {
                lockNews.ReleaseReaderLock();
            }
        }
    
        public void SaveNews()
        {
            var dir = ExecutablePath.ExecPath + "\\News\\";
            try
            {
                lockNews.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.ErrorFormat("Сохранение новостей - таймаут");
                return;
            }
            try
            {
                foreach (var pair in dicNews)
                {
                    var fileName = dir + pair.Key;
                    BaseNewsParser.StoreNews(fileName, pair.Value.ToArray());
                }                
            }
            finally
            {
                lockNews.ReleaseReaderLock();
            }
        }

        public void LoadNews()
        {
            var dir = ExecutablePath.ExecPath + "\\News\\";
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception)
                {
                    Logger.ErrorFormat("Ошибка создания директории News");
                    return;
                }
            }
            foreach (var fname in Directory.GetFiles(dir, "*.txt"))
            {
                Logger.Info("LoadNews(" + fname + ") ...");
                List<News> news = null;
                try
                {
                    news = BaseNewsParser.LoadNews(fname);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в BaseNewsParser.LoadNews(" + fname + "): " + ex);
                    continue;
                }
                
                AddNews(news);
            }
            Logger.Info("LoadNews() ... OK");
        }
    }
}
