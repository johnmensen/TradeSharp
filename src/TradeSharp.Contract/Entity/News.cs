using System;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class News : IBaseNews
    {
        public int ChannelId { get; set; }
        public DateTime Time { get; set; }
        public String Title { get; set; }
        public String Body { get; set; }
        public const string NewsPrefix = "N:";
        public const string PartSeparator = ";";
        public const string PartSeparatorPlaceHolder = "&smc&";
        public News()
        {
        }
        public News(News news)
        {
            ChannelId = news.ChannelId;
            Time = news.Time;
            Title = news.Title;
            Body = news.Body;
        }

        public News(int channelId, DateTime date, String title, String body)
        {
            ChannelId = channelId;
            Time = date;
            Title = title;
            Body = body;
        }

        public override String ToString()
        {
            return string.Format("{0}{1}{2}{3}{2}{4}{2}{5}", 
                NewsPrefix, 
                ChannelId, 
                PartSeparator,
                Time.ToStringUniform(),
                Title.Replace(PartSeparator, PartSeparatorPlaceHolder),
                Body.Replace(PartSeparator, PartSeparatorPlaceHolder));
        }

        /// <summary>
        /// формат строки:  N:ChannelId;Time;Title;[Body]        
        /// </summary>
        public static News Parse(String str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (!str.StartsWith(NewsPrefix)) return null;
            str = str.Substring(NewsPrefix.Length);
            if (string.IsNullOrEmpty(str)) return null;

            var nsParts = str.Split(new[] { PartSeparator }, StringSplitOptions.None);
            if (nsParts.Length < 3 || nsParts.Length > 4) return null;

            try
            {
                var news = new News
                               {
                                   ChannelId = nsParts[0].ToInt(),
                                   Time = nsParts[1].ToDateTimeUniform(),
                                   Title = nsParts[2].Replace(PartSeparatorPlaceHolder, PartSeparator)
                               };
                if (nsParts.Length > 3)
                    news.Body = nsParts[3].Replace(PartSeparatorPlaceHolder, PartSeparator);
                return news;
            }
            catch (Exception ex)
            {
                Logger.Error("News.Parse: неправильный формат данных", ex);
                return null;
            }
        }
    }
}
