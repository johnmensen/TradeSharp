using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Client
{
    class FakeNewsServer : INewsStorage
    {
        private static readonly Random random = new Random();

        public readonly Dictionary<int, int[]> channelsByAccount;

        public readonly Dictionary<int, List<News>> newsByChannel;

        public FakeNewsServer(Dictionary<int, int[]> channelsByAccount)
        {
            var endDate = DateTime.Now.AddHours(-1);
            var startDate = endDate.AddDays(-40);

            this.channelsByAccount = channelsByAccount;
            var channels = channelsByAccount.SelectMany(ca => ca.Value).Distinct();
            newsByChannel = channels.ToDictionary(c => c,
                                                  c => NewsMaker.MakeSomeNews(c, startDate, endDate, 1,
                                                                              random.Next(100) < 50
                                                                                  ? random.Next(1, 10)
                                                                                  : random.Next(2, 1440)));
        }

        public void AddNews(int channel, List<News> newNews)
        {
            if (newsByChannel.ContainsKey(channel))
                newsByChannel[channel].AddRange(newNews);
        }

        #region INewsStorage
        public List<News> GetNews(int account, DateTime time, int[] newsChannelIds)
        {
            if (!channelsByAccount.ContainsKey(account))
                return new List<News>();

            var allNews = new List<News>();
            var date = time.Date;
            var channels = channelsByAccount[account];
            foreach (var chan in channels)
            {
                allNews.AddRange(newsByChannel[chan].Where(n => n.Time.Date == date));
            }

            return allNews;
        }

        public NewsMap GetNewsMap(int accountId)
        {
            var map = new NewsMap
            {
                channelIds = new int[0],
                records = new NewsMapRecord[0]
            };

            if (!channelsByAccount.ContainsKey(accountId))
                return map;

            map.channelIds = channelsByAccount[accountId];

            // посчитать количество записей на дату
            var records = new Dictionary<DateTime, NewsMapRecord>();
            foreach (var chan in map.channelIds)
            {
                var list = newsByChannel[chan];
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
        #endregion
    }
}
