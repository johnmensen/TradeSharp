using System;
using System.Net;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Processing.Lib
{
    /// <summary>
    /// Получает катировки с сайта 
    /// </summary>
    public static class YahooQuoteProvider
    {
        private static readonly WebClient webClient;
        static YahooQuoteProvider()
        {
            webClient = new WebClient();

        }

        /// <summary>
        /// возвращает в кортеже Bid / Ask значения для указанной катировки
        /// </summary>
        public static QuoteData GetQuoteByKey(string quoteKey)
        {
            string str;
            try
            {
                str = webClient.DownloadString(string.Format("http://finance.yahoo.com/q?s={0}=X", quoteKey.ToUpper()));
            }
            catch (Exception ex)
            {
                Logger.Error("GetQuoteByKey", ex);
                return null;
            }

            var strQuvoteBid = Parse(str, string.Format("<span id=\"yfs_b00_{0}=x", quoteKey.ToLower()));
            if (!strQuvoteBid.HasValue) return null;

            var strQuvoteAsk = Parse(str, string.Format("<span id=\"yfs_a00_{0}=x", quoteKey.ToLower()));
            if (!strQuvoteAsk.HasValue) return null;

            return new QuoteData(strQuvoteBid.Value, strQuvoteAsk.Value, DateTime.Now);
        }

        private static float? Parse(string str, string standardStr)
        {
            var index = str.IndexOf(standardStr, StringComparison.Ordinal) + standardStr.Length;

            var read = false;
            var strQuvote = string.Empty;
            for (var i = index; i < str.Length; i++)
            {
                if (str[i] == '<') break;
                if (read) strQuvote += str[i];
                if (str[i] == '>') read = true;
            }
            return strQuvote.ToFloatUniformSafe();
        }
    }
}
