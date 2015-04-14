using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    class NewsLocalStorage
    {
        class NewsByDateComparer : IEqualityComparer<News>
        {
            public bool Equals(News x, News y)
            {
                return x.Time == y.Time;
            }

            public int GetHashCode(News obj)
            {
                return obj.Time.GetHashCode();
            }
        }

        private static NewsLocalStorage instance;

        public static NewsLocalStorage Instance
        {
            get { return instance ?? (instance = new NewsLocalStorage()); }
        }

        private readonly ThreadSafeStorage<int, List<News>> newsStore = new ThreadSafeStorage<int, List<News>>();

        public static readonly string newsPath = ExecutablePath.ExecPath + "\\news";

        private NewsLocalStorage()
        {
            EnsureNewsPath();
            BrowseCache();
        }

        public static void ReInstantiate()
        {
            instance = new NewsLocalStorage();
        }

        public List<News> GetNews(int channelId, DateSpan dateRange = null)
        {
            bool contains;
            var listNews = newsStore.ReceiveValueCheckContains(channelId, out contains);
            if (!contains)
                return null;

            if (listNews != null)
            {
                // вернуть либо весь список, либо отфильтрованные по дате значения
                if (dateRange == null) return listNews;
                var subList = listNews.Where(n => dateRange.IsIn(n.Time)).ToList();
                return subList;
            }

            // подкачать новости из файла и вернуть список
            listNews = ReadNewsFromFile(channelId);
            if (listNews == null) return null;
            return dateRange == null ? listNews 
                : listNews.Where(n => dateRange.IsIn(n.Time)).ToList();
        }

        public void UpdateNews(List<News> newsList)
        {
            foreach (var pair in newsList.GroupBy(n => n.ChannelId))
                UpdateNews(pair.Key, pair.ToList());            
        }

        /// <summary>
        /// обновить новости в потокобезопасном хранилище
        /// </summary>
        public void UpdateNews(int channelId, List<News> newsList)
        {
            bool contains;
            var newsStored = newsStore.ReceiveValueCheckContains(channelId, out contains);
            if (newsStored == null)
            {
                newsStore.UpdateValues(channelId, newsList);
                return;
            }

            // склеить списки и обновить хранилище
            var comparer = new NewsByDateComparer();
            var result = newsStored.Union(newsList).Distinct(comparer).OrderBy(n => n.Time).ToList();
            newsStore.UpdateValues(channelId, result);
        }

        public void SaveNewsInFiles()
        {
            var newsDic = newsStore.ReceiveAllData();
            foreach (var chanNews in newsDic)
            {
                if (chanNews.Value == null ||
                    chanNews.Value.Count == 0) continue;
                var fileName = MakeFileNameFromChannelId(chanNews.Key);
                try
                {
                    using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        foreach (var news in chanNews.Value)
                            sw.WriteLine(news.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("SaveNewsInFiles(), file \"{0}\" error: {1}",
                        fileName, ex);
                }
            }
        }

        private List<News> ReadNewsFromFile(int channelId)
        {
            var fileName = MakeFileNameFromChannelId(channelId);
            try
            {
                if (!File.Exists(fileName)) return null;
                var newsList = new List<News>();
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        var news = GetNewsFromTextLine(line);
                        if (news == null) continue;
                        newsList.Add(news);
                    }
                }

                // обновить словарь канал-новости
                newsStore.UpdateValues(channelId, newsList);
                return newsList;
            }
            catch (Exception ex)
            {
                Logger.Error("NewsLocalStorage.ReadNewsFromFile() error: ", ex);
                return null;
            }
        }

        /// <summary>
        /// не заполнять списки новостей по каждому каналу, но заполнить словарь
        /// пустыми списками по каждому каналу, для которого есть свой файл
        /// </summary>
        private void BrowseCache()
        {
            try
            {
                var keys = new List<int>();
                foreach (var fileName in Directory.GetFiles(newsPath, "news_*.txt"))
                {
                    var channelId = GetChannelIdFromFileName(fileName);
                    if (!channelId.HasValue) continue;
                    keys.Add(channelId.Value);
                }
                newsStore.UpdateValues(keys.ToArray(), keys.Select(k => (List<News>)null).ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error("NewsLocalStorage.BrowseCache() error: ", ex);
                throw;
            }
        }

        private static News GetNewsFromTextLine(string line)
        {
            return News.Parse(line);
        }

        private static int? GetChannelIdFromFileName(string fileName)
        {
            var fileNameSolo = Path.GetFileNameWithoutExtension(fileName).Substring("news_".Length);
            if (string.IsNullOrEmpty(fileNameSolo)) return null;
            return fileNameSolo.ToIntSafe();
        }
    
        private static string MakeFileNameFromChannelId(int channelId)
        {
            return newsPath + "\\news_" + channelId + ".txt";
        }

        public void EnsureNewsPath()
        {
            try
            {
                if (Directory.Exists(newsPath)) return;
                Directory.CreateDirectory(newsPath);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в NewsLocalStorage.EnsureNewsPath()", ex);
                throw;
            }
        }
    
        public NewsMap MakeNewsMap()
        {
            var allNews = newsStore.ReceiveAllData();

            var map = new NewsMap
            {
                channelIds = new int[0],
                records = new NewsMapRecord[0]
            };

            map.channelIds = allNews.Keys.ToArray();

            // посчитать количество записей на дату
            var records = new Dictionary<DateTime, NewsMapRecord>();
            foreach (var chanNews in allNews.Where(n => n.Value != null))
            {
                var list = chanNews.Value;
                foreach (var news in list)
                {
                    var date = news.Time.Date;
                    if (!records.ContainsKey(date))
                        records.Add(date, new NewsMapRecord(date, 1));
                    else
                        records[date] = new NewsMapRecord(date, records[date].recordsCount + 1);
                }
            }
            map.records = records.Values.OrderBy(r => r.date).ToArray();
            return map;
        }
    }
}
