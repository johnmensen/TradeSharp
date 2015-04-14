using System;
using System.Collections.Generic;
using System.IO;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Test.Moq
{
    public static class NewsMaker
    {
        private static readonly string[] titles = new[] { "Breaking news!", "News", "Option Data", "Futures data", "CFD", "Market Index" };

        private static readonly string[] bodies = new[]
            {
                "Justin Bieber apparently called a young woman a beach whale while in Australia!", 
                "Dow Jones повысился на 0,03%", 
                "Strike: 107.56; Ticker: E6; Expires: MAY13; OI: 2051; Volume: 2890; Premium: 7.51", 
                "Price: 107.61; Ticker: E6; Expires: MAY13", 
                "Bid: 1231.22; Ask: 1232.50; Ticker: SP500", "IASG: 703.21; DJ100: 96.13"
            };

        private static readonly Random rnd = new Random();

        public static List<News> MakeSomeNews(int channelId, DateTime startDate, DateTime endDate,
                                        int minIntervalMinutes, int maxIntervalMinutes)
        {
            var result = new List<News>();
            
            for (var date = startDate; date <= endDate; )
            {
                result.Add(new News(channelId, date, titles[rnd.Next(titles.Length)],
                    bodies[rnd.Next(bodies.Length)]));

                var delta = rnd.Next(minIntervalMinutes, maxIntervalMinutes);
                date = date.AddMinutes(delta);
            }

            return result;
        }

        public static News MakeSingleNews(int channelId, DateTime date)
        {
            return new News(channelId, date, titles[rnd.Next(titles.Length)],
                    bodies[rnd.Next(bodies.Length)]);
        }
    
        public static void ClearNewsFolder()
        {
            try
            {
                if (!Directory.Exists(NewsLocalStorage.newsPath)) return;
                foreach (var file in Directory.GetFiles(NewsLocalStorage.newsPath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}
