using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Entity;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuNewsLocalStorage
    {
        [SetUp]
        public void Setup()
        {
            NewsMaker.ClearNewsFolder();
        }

        [Test]
        public void TestSaveLoad()
        {
            // перечитать
            NewsLocalStorage.ReInstantiate();

            var newsEmpty = NewsLocalStorage.Instance.GetNews(1);
            Assert.IsNull(newsEmpty, "no cache - no news");
            
            var startDate = DateTime.Now.Date.AddDays(-30);
            var endDate = DateTime.Now.AddHours(-6);
            var allNews = new Dictionary<int, List<News>>
                {
                    {1, NewsMaker.MakeSomeNews(1, startDate, endDate, 1, 1440)},
                    {3, NewsMaker.MakeSomeNews(3, startDate.AddDays(5), endDate, 1, 1440)},
                    {4, NewsMaker.MakeSomeNews(4, startDate, endDate, 1440, 1440)}
                };

            // запись новостей в пустое хранилище
            const int chanId = 1;
            NewsLocalStorage.Instance.UpdateNews(chanId, allNews[chanId]);
            var newsChan1 = NewsLocalStorage.Instance.GetNews(chanId);
            Assert.IsNotNull(newsChan1, "simple update & get: not null");
            Assert.AreEqual(allNews[chanId].Count, newsChan1.Count, "simple update & get: same count");
            var indexSpeciman = newsChan1.Count / 2;
            var newsOrig = allNews[chanId][indexSpeciman];
            var newsNew = newsChan1[indexSpeciman];
            Assert.IsTrue(newsOrig.ChannelId == newsNew.ChannelId &&
                newsOrig.Body == newsNew.Body && newsOrig.Time == newsNew.Time &&
                newsOrig.Title == newsNew.Title, "simple update & get: same news item");

            // добавить все новости
            NewsLocalStorage.Instance.UpdateNews(allNews[1]);
            NewsLocalStorage.Instance.UpdateNews(allNews[3]);
            NewsLocalStorage.Instance.UpdateNews(allNews[4]);

            // проверить количество новостей
            foreach (var pair in allNews)
                Assert.AreEqual(pair.Value.Count, NewsLocalStorage.Instance.GetNews(pair.Key).Count, "updated news: got all");
            
            // сохранить новости
            try
            {
                NewsLocalStorage.Instance.SaveNewsInFiles();
            }
            catch (Exception ex)
            {
                Assert.Fail("NewsLocalStorage.Instance.SaveNewsInFiles(): " + ex);
            }

            // перечитать
            NewsLocalStorage.ReInstantiate();

            // перечитать сохраненные новости
            foreach (var pair in allNews)
                Assert.AreEqual(pair.Value.Count, NewsLocalStorage.Instance.GetNews(pair.Key).Count, "reloaded news: got all");
        }

        [Test]
        public void TestMerge()
        {
            // перечитать
            NewsLocalStorage.ReInstantiate();
            var allNews = MakeSomeNews();
            
            // добавить свежака
            var chan1news = allNews[1];
            var midNews = chan1news[chan1news.Count/2];
            var insertedNews = new News(1, midNews.Time, "Middle news", "Empty body"); // эта будет перезатерта
            chan1news.Insert(chan1news.Count/2, insertedNews);
            chan1news.Insert(chan1news.Count / 2, new News(insertedNews) { Time = insertedNews.Time.AddSeconds(16) });
            chan1news.Add(new News(1, DateTime.Now.AddMinutes(-10), "Latest news", "Empty body"));
            chan1news.Add(new News(1, DateTime.Now.AddMinutes(-8), "Latest news - 2", "Empty body"));
            NewsLocalStorage.Instance.UpdateNews(chan1news);

            // проверить адекватность
            var updatedChan1news = NewsLocalStorage.Instance.GetNews(1);
            Assert.AreEqual(chan1news.Count - 1, updatedChan1news.Count, "rewritten news: count is ok");
            var midNewsNew = updatedChan1news.FirstOrDefault(n => n.Title == "Middle news");
            Assert.IsNotNull(midNewsNew, "rewritten news is not null");
        }

        [Test]
        public void TestNewsMap()
        {
            NewsMaker.ClearNewsFolder();
            NewsLocalStorage.ReInstantiate();
            var allNews = MakeSomeNews();

            // проверить карту новостей
            var map = NewsLocalStorage.Instance.MakeNewsMap();
            Assert.AreEqual(allNews.Count, map.channelIds.Length, "TestNewsMap - has all channels");
            
            // проверить записи на нек. дату
            var chan1News = allNews[1];
            var someDate = chan1News[chan1News.Count/2].Time.Date;
            var newsOnDate = allNews.Sum(cn => cn.Value.Count(n => n.Time.Date == someDate));
            var recordOnDate = map.records.FirstOrDefault(r => r.date == someDate);
            Assert.AreEqual(newsOnDate, recordOnDate.recordsCount, "TestNewsMap - map is OK on a selected date");
        }

        private Dictionary<int, List<News>> MakeSomeNews()
        {
            // генерировать новости
            var startDate = DateTime.Now.Date.AddDays(-30);
            var endDate = DateTime.Now.AddHours(-6);
            var allNews = new Dictionary<int, List<News>>
                {
                    {1, NewsMaker.MakeSomeNews(1, startDate, endDate, 1, 5)},
                    {3, NewsMaker.MakeSomeNews(3, startDate.AddDays(5), endDate, 1, 1440)},
                    {4, NewsMaker.MakeSomeNews(4, startDate, endDate, 1440, 1440)}
                };
            // запихнуть в хранилище
            foreach (var pair in allNews)
                NewsLocalStorage.Instance.UpdateNews(pair.Key, pair.Value);

            return allNews;
        }
    }
}
