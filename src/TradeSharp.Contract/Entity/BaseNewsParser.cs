using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public static class BaseNewsParser
    {
        public static readonly String[] MessagesSeparator = new []{"&#"};

        private static readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        public const int LogMsgParseError = 1;

        public static List<IBaseNews> Parse(String strNews)
        {
            var lst = new List<IBaseNews>();
            try
            {
                var msgs = strNews.Split(MessagesSeparator, StringSplitOptions.None);
                
                if (msgs.Count() == 0)
                    return lst;
                foreach (var msg in msgs)
                {
                    if (msg.Contains("N:"))
                    {
                        // встретили новость
                        var item = News.Parse(msg);
                        if (item != null) lst.Add(item);
                    }
                    else
                        if (msg.Contains("Q:"))
                        {
                            // встретили котировку
                            var item = new TickerQuoteData();
                            item.Parse(msg);
                            lst.Add(item);
                        }
                        else
                            throw new Exception("Неизвестный формат новостей");
                }
                return lst;
            }
            catch(Exception ex)
            {
                // тут он не виден, надо что то придумать
                Logger.Error("BaseNewsParser.Parse: Возникла ошибка ", ex);
            }
            return lst;
        }

        public static IBaseNews ParseItem(string msg)
        {
            IBaseNews news = null;
            try
            {
                if (string.IsNullOrEmpty(msg)) return news;
                
                if (msg.Contains("N:"))
                {
                    // встретили новость
                    news = News.Parse(msg);
                }
                else
                    if (msg.Contains("Q:"))
                    {
                        // встретили котировку
                        news = new TickerQuoteData();
                        try
                        {
                            ((TickerQuoteData) news).Parse(msg);
                        }
                        catch
                        {
                            Logger.ErrorFormat("Котировка не распознана ({0})", msg);
                            news = null;
                        }
                    }
                    else
                        throw new Exception("Неизвестный формат новости");
                return news;
            }
            catch (Exception ex)
            {
                // тут он не виден, надо что то придумать
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgParseError, 1000 * 60 * 10,
                    "Не удалось разобрать сообщение BaseNews: {0}", msg);
            }
            return news;
        }

        public static String ToString(List<IBaseNews> list)
        {
            if (list.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            foreach(var n in list)
            {
                sb.AppendFormat("{0}{1}", n, MessagesSeparator[0]);
            }
            return sb.ToString();
        }

        public static void StoreNews(string fileName, News[] news)
        {
            using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                StoreNews(sw, news);
            }
        }

        public static void StoreNews(StreamWriter sw, News[] news)
        {
            if (news.Length == 0) return;
            foreach (var ns in news)
            {
                sw.Write(ns.ToString());
                sw.Write(MessagesSeparator[0]);
            }
        }

        public static List<News> LoadNews(string fileName)
        {
            using (var sr = new StreamReader(fileName, Encoding.UTF8))
            {
                return LoadNews(sr);
            }
        }

        public static List<News> LoadNews(StreamReader sr)
        {
            var strTail = "";
            var news = new List<News>();
            const int bufSize = 1024;
            var bytes = new char[bufSize];            

            while (!sr.EndOfStream)
            {
                var bytesRead = sr.Read(bytes, 0, bufSize);
                ReadNewsBlock(bytes, bytesRead, news, ref strTail);
            }
            return news;
        }

        private static void ReadNewsBlock(char[] buffer, int bytesRead, List<News> news, ref string tail)
        {
            // слепить строку с хвостом
            if (bytesRead == 0) return;
            var strBuf = new string(buffer, 0, bytesRead);
            var str = tail + strBuf;
            // смотрим - завершена ли строка разделителем?
            var indexSt = str.LastIndexOf(MessagesSeparator[0]);
            if (indexSt < 0)
            {
                tail = str;
                return;
            }
            var isTerminated = str.EndsWith(MessagesSeparator[0]);                
            
            // прочитать все новости, оставшийся кусок новости записать в tail
            var parts = str.Split(MessagesSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                tail = "";
                return;
            }
            var lastIndex = isTerminated ? parts.Length - 1 : parts.Length - 2;

            for (var i = 0; i <= lastIndex; i++)
            {
                var ns = News.Parse(parts[i]);
                if (ns != null) news.Add(ns);
            }

            // сформировать хвост
            tail = isTerminated ? "" : parts[parts.Length - 1];
        }
    }
}
