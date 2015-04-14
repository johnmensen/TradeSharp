using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteService.QuoteStorage;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.WebServer
{
    class QuoteWebServer : BaseWebServer
    {
        private static QuoteWebServer instance;

        public static QuoteWebServer Instance
        {
            get { return instance ?? (instance = new QuoteWebServer()); }
        }

        public override string ServiceName
        {
            get { return "QuoteService"; }
        }

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                if (context.Request.QueryString.HasKeys())
                {
                    var dic = PreProcessQueryString(context.Request.QueryString);
                    try
                    {
                        if (dic.ContainsKey("curr"))
                        {
                            var quotes = SafeQuoteStorage.Instance.GetQuotes();
                            var response = GetOrderedQuotesJSONString(quotes);
                            var r = Encoding.UTF8.GetBytes(response);
                            context.Response.ContentLength64 = r.Length;
                            context.Response.OutputStream.Write(r, 0, r.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ProcessHttpRequest: ошибка обработки метода GET", ex);
                    }
                }                
            }
            context.Response.OutputStream.Close();
        }

        private static Dictionary<string, string> PreProcessQueryString(NameValueCollection query)
        {
            var dic = new Dictionary<string, string>();
            for (var i = 0; i < query.Count; i++)
            {
                dic.Add(query.GetKey(i), query.GetValues(i)[0]);
            }
            return dic;
        }

        private static string GetOrderedQuotesJSONString(Dictionary<string, QuoteData> quotes)
        {
            var data = quotes.Select(q => new Cortege3<string, string, string>
                                              (q.Key, q.Value.bid.ToStringUniformPriceFormat(true),
                                               q.Value.ask.ToStringUniformPriceFormat(true))).ToList();

            var replaceTicker = new Func<string, string>(s =>
                {
                    if (s.Equals("EURUSD", StringComparison.OrdinalIgnoreCase)) return "1";
                    if (s.Equals("GBPUSD", StringComparison.OrdinalIgnoreCase)) return "2";
                    if (s.Equals("USDCHF", StringComparison.OrdinalIgnoreCase)) return "3";
                    if (s.Equals("USDJPY", StringComparison.OrdinalIgnoreCase)) return "4";
                    if (s.Equals("NZDUSD", StringComparison.OrdinalIgnoreCase)) return "5";
                    if (s.Equals("AUDUSD", StringComparison.OrdinalIgnoreCase)) return "6";
                    if (s.Equals("USDCAD", StringComparison.OrdinalIgnoreCase)) return "7";
                    return s;
                });
                
            
            data.Sort((a, b) =>
                {
                    var ac = replaceTicker(a.a);
                    var bc = replaceTicker(b.a);
                    return ac.CompareTo(bc);
                });

            var response = "[" + string.Join(",",
                data.Select(p => string.Format("[\"{0}\",\"{1}\",\"{2}\"]", p.a, p.b, p.c))) + "]";

            return response;
        }
    }
}
