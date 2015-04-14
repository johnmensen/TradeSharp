using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.Yahoo
{
    class YahooGrabber : BaseNewsGrabber
    {
        private const string SettingsFileName = "yahoo.xml";
        private const string QueryBase = "http://ca.finance.yahoo.com/q?s=";
        private readonly List<YahooTickerInfo> tickers = new List<YahooTickerInfo>();
        private int channelId;
        private string newsTitle;
        
        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);
        private const int LogMsgNoResponce = 1;
        private const int LogMsgNoData = 2;
        private const int LogMsgReadOk = 3;

        public YahooGrabber()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            var xmlPath = string.Format("{0}\\{1}", ExecutablePath.ExecPath, SettingsFileName);
            if (!File.Exists(xmlPath)) return;
            var doc = new XmlDocument();
            doc.Load(xmlPath);
            if (doc.DocumentElement == null)
                throw new Exception("Yahoo xml doc: document element is missing");

            var setsNode = doc.DocumentElement.SelectNodes("settings");
            if (setsNode == null) return;
            if (setsNode.Count == 0) return;

            channelId = setsNode[0].Attributes["channelId"].Value.ToInt();
            newsTitle = setsNode[0].Attributes["newsTitle"].Value;
            
            var tickerNodes = doc.DocumentElement.SelectNodes("ticker");
            if (tickerNodes == null) return;            
            
            foreach (XmlElement node in tickerNodes)
            {
                tickers.Add(new YahooTickerInfo(node));
            }
        }

        public override void GetNews()
        {
            if (tickers.Count == 0) return;
            var newsBody = new StringBuilder(string.Format("[#fmt]#&newstype=yahoo_index#&" +
                                 "publishdate={0:dd.MM.yyyy HH:mm:ss}", DateTime.Now));
            int countRead = 0;
            foreach (var ticker in tickers)
            {
                // запросить страницу
                var tickValue = ticker.ParseHttpResponce(QueryPage(QueryBase, ticker.NameYahoo));
                if (!tickValue.HasValue) continue;
                countRead++;
                newsBody.AppendFormat("#&{0}={1}", ticker.TickerName, tickValue.ToStringUniform());
            }
            if (countRead == 0)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgNoData, 1000 * 60 * 10, "Нет данных");
                return;
            }
            // отправить новостишку на сервер
            var news = new News(channelId, DateTime.Now, newsTitle, newsBody.ToString());
            PutNewsOnServer(new [] {news});

            loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                    LogMsgReadOk, 1000 * 60 * 10, "Yahoo: данные успешно прочитаны");
        }

        private string QueryPage(string queryBase, string tickerNameYahoo)
        {
            var req = WebRequest.Create(queryBase + tickerNameYahoo);
            //req.Proxy = WebProxy.GetDefaultProxy();
            WebResponse responce = null;
            try
            {
                responce = req.GetResponse();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка запроса к URL({0}): {1}", req.RequestUri, ex);
                return string.Empty;
            }
            
            if (responce == null)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgNoResponce, 1000 * 60 * 10, "Нет ответа от {0}", queryBase + tickerNameYahoo);
                return string.Empty;
            }
            var stream = responce.GetResponseStream();
            if (stream == null)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgNoResponce, 1000 * 60 * 10, "Нет ответа от {0}", queryBase + tickerNameYahoo);
                return string.Empty;
            }
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }

    class YahooTickerInfo
    {
        public string TickerName { get; set; }
        public string NameYahoo { get; set; }
        public string SearchPattern { get; set; }

        public YahooTickerInfo(XmlElement xmlNode)
        {
            TickerName = xmlNode.Attributes["name"].Value;
            NameYahoo = xmlNode.Attributes["nameYahoo"].Value;
            SearchPattern = xmlNode.Attributes["searchPattern"].Value;
        }

        public decimal? ParseHttpResponce(string html)
        {
            if (string.IsNullOrEmpty(html)) return null;
            
            // найти вхождение шаблона, за которым должно следовать цифровое значение
            var patternStart = html.IndexOf(SearchPattern);
            if (patternStart < 0) return null;

            var numStr = new StringBuilder();
            for (var i = patternStart + SearchPattern.Length; i < html.Length; i++)
            {
                var smb = html[i];
                if (smb == '.' || smb == ',' || (smb >= '0' && smb <= '9')) numStr.Append(smb);
                else break;
            }

            var str = numStr.ToString().Replace(",", "");
            return str.ToDecimalUniformSafe();
        }
    }
}
