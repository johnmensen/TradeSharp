using System;
using System.Collections.Generic;
using TradeSharp.Util;

namespace Entity
{
    /// <summary>
    /// описывает новость, содержащую публикуемые индексы
    /// 
    /// пример:
    /// insert into NEWS (Channel, DateNews, Title, Body) values(3, '13.10.2010 17:00', 
    /// 'Yahoo Index', '[#fmt]#&newstype=yahoo_index#&publishdate=13.10.2010 
    /// 17:00:00#&CRB=300.55#&SP500A=1179.37#&DJIA30=11120.11')
    /// </summary>
    [Serializable]
    public class IndexVectorNewsRecord
    {
        public DateTime date;
        public string[] indexNames;
        public decimal[] indexValues;
        public string newsType;

        private static readonly char[] keyValSeparator = new [] { '=' };

        public IndexVectorNewsRecord()
        {            
        }

        public static IndexVectorNewsRecord ParseNews(string body, DateTime publishDate)
        {
            if (string.IsNullOrEmpty(body)) return null;
            if (!body.StartsWith("[#fmt]")) return null;
            body = body.Substring("[#fmt]".Length);
            var parts = body.Split(Separators.FormattedNewsSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return null;
            
            var news = new IndexVectorNewsRecord {date = publishDate};
            var tickers = new List<string>();
            var prices = new List<decimal>();

            foreach (var part in parts)
            {
                var keyVal = part.Split(keyValSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (keyVal.Length != 2) continue;
                if (keyVal[0] == "newstype")
                {
                    news.newsType = keyVal[1];
                    continue;
                }
                if (keyVal[0] == "publishdate") continue;
                var indValue = keyVal[1].ToDecimalUniformSafe();
                if (indValue.HasValue)
                {
                    tickers.Add(keyVal[0]);
                    prices.Add(indValue.Value);
                }
            }
            if (tickers.Count == 0) return null;
            news.indexNames = tickers.ToArray();
            news.indexValues = prices.ToArray();
            return news;
        }
    }
}
