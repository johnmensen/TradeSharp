using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using TradeSharp.Util;

namespace NewsRobot
{
    public partial class NewsRobot
    {
        public static List<RobotNews> GrabNews(DateTime dateTime, out List<string> error)
        {
            var result = new List<RobotNews>();
            error = new List<string>();
            var startdt = DateTime.Parse("01/01/1970");
            var requestDate = dateTime.Date;
            var span = requestDate.Subtract(startdt);
            var secs = span.TotalSeconds;
            var myWebRequest =
                WebRequest.Create("http://www.alpari.ru/ru/analytics/calendar/data/list/?lang=ru&timeZone=4&time=" +
                                  secs);
            var myWebResponse = myWebRequest.GetResponse();
            if (myWebResponse == null)
            {
                error.Add(DateTime.Now + ": server not responding (WebRequest.GetResponse() = null)");
                return result;
            }
            var stream = myWebResponse.GetResponseStream();
            if (stream == null)
            {
                error.Add(DateTime.Now + ": internal error (WebResponse.GetResponseStream() = null)");
                return result;
            }
            string srcStr;
            using (var sr = new StreamReader(stream, Encoding.UTF8))
                srcStr = sr.ReadToEnd();
            stream.Close();
            var rowPos = 0;
            while (true)
            {
                var row = GetTagData(srcStr, "tr", ref rowPos);
                if (row == null)
                    break;
                var news = new RobotNews();
                var addNews = true;
                var cellPos = 0;
                for (var column = 0; column < 7; column++)
                {
                    var cell = GetTagData(row, "td", ref cellPos);
                    if (cell == null)
                    {
                        error.Add(string.Format("news parse error at {0}: incomplete data ({1})", requestDate, news));
                        addNews = false;
                        break;
                    }
                    cellPos += cell.Length;
                    var internalPos = 0;
                    var txt = GetTagData(cell, "div", ref internalPos);
                    if (txt != null)
                        cell = txt;
                    internalPos = 0;
                    txt = GetTagData(cell, "strong", ref internalPos);
                    if (txt != null)
                    {
                        cell = txt;
                        news.Valuable = true;
                    }
                    var completeText = GetTagText(cell).Trim();
                    double? value;
                    if (completeText == "") // skip empty cells
                    {
                        addNews = false;
                        continue;
                    }
                    switch (column)
                    {
                        case 0:
                            if (completeText.Contains("–")) // skip date period
                                addNews = false;
                            else
                                try
                                {
                                    news.Time = requestDate + TimeSpan.Parse(completeText);
                                }
                                catch // (Exception e)
                                {
                                    //Console.WriteLine("{0}\n{1}\n{2}", e, completeText, cell);
                                    error.Add(string.Format("news parse error at {0}: bad date format ({1}, {2})",
                                                            requestDate, completeText, news));
                                    addNews = false;
                                    continue;
                                }
                            break;
                        case 1:
                            news.CountryCode = completeText;
                            break;
                        case 2:
                            news.Title = completeText;
                            break;
                        case 5:
                            if (completeText == "—")
                                addNews = false;
                            else
                            {
                                value = GetFirstWordAsDouble(completeText);
                                if (value == null)
                                {
                                    error.Add(string.Format("news parse error at {0}: bad number format ({1}, {2})",
                                                            requestDate, completeText, news));
                                    addNews = false;
                                }
                                else
                                    news.ProjectedValue = value.Value;
                            }
                            break;
                        case 6:
                            if (completeText == "—")
                                addNews = false;
                            else
                            {
                                value = GetFirstWordAsDouble(completeText);
                                if (value == null)
                                {
                                    error.Add(string.Format("news parse error at {0}: bad number format ({1}, {2})",
                                                            requestDate, completeText, news));
                                    addNews = false;
                                }
                                else
                                    news.Value = value.Value;
                            }
                            break;
                    }
                }
                if (addNews)
                    result.Add(news);
                rowPos += row.Length;
            }
            return result;
        }

        // пока пусть будет тут
        public static string GetTagAttributeValue(XmlNode parentNode, string tag)
        {
            var node = parentNode.SelectNodes(tag);
            if (node == null || node.Count != 1 || node[0].Attributes == null)
                return null;
            var attr = node[0].Attributes["value"];
            return attr == null ? null : attr.Value;
        }

        private static double? GetFirstWordAsDouble(string value)
        {
            var w = "";
            value = value.Trim().Replace("CHF", "").Replace("NZD", "");
            foreach (var ch in value)
            {
                if (char.IsNumber(ch))
                    w += ch;
                else
                {
                    var endOfWord = true;
                    switch (ch)
                    {
                        case '-':
                            endOfWord = false;
                            w += "-";
                            break;
                        case '+':
                            endOfWord = false;
                            break;
                        case '.':
                            endOfWord = false;
                            w += ",";
                            //w += NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                            break;
                    }
                    if (endOfWord)
                        break;
                }
            }
            if (w.Length == 0)
                return null;
            try
            {
                var result = w.ToDoubleUniformSafe();
                return result;
                //return double.Parse(w);
            }
            catch// (Exception e)
            {
                //Console.WriteLine("{0}\n{1}", e, w);
                return null;
            }
        }

        #region working with HTML
        private static string GetTagData(string src, string tag, ref int pos)
        {
            pos = src.IndexOf("<" + tag, pos);
            if (pos == -1)
                return null;
            var endPos = src.IndexOf("</" + tag + ">", pos);
            if (endPos == -1)
            {
                pos = -1;
                return null;
            }
            var nextPos = pos + 1;
            var nextTagData = GetTagData(src, tag, ref nextPos);
            if ((nextTagData != null) && (nextPos < endPos))
                endPos = src.IndexOf("</" + tag + ">", nextPos + nextTagData.Length);
            if (endPos == -1)
            {
                pos = -1;
                return null;
            }
            return src.Substring(pos, endPos - pos + ("</" + tag + ">").Length);
        }

        private static string GetTagText(string src)
        {
            if (src[0] != '<')
                return null;
            var openTagEndPos = src.IndexOf(' ');
            var openTagEndPos2 = src.IndexOf('>');
            if (openTagEndPos == -1)
                openTagEndPos = openTagEndPos2;
            if ((openTagEndPos != -1) && (openTagEndPos2 != -1))
                if (openTagEndPos > openTagEndPos2)
                    openTagEndPos = openTagEndPos2;
            if (openTagEndPos == -1)
                return null;
            var tag = src.Substring(1, openTagEndPos - 1);
            var textBeginPos = src.IndexOf('>');
            if (textBeginPos == -1)
                return null;
            textBeginPos++;
            var textEndPos = src.LastIndexOf("</" + tag + ">");
            if (textEndPos == -1)
                return null;
            var text = src.Substring(textBeginPos, textEndPos - textBeginPos);
            var pos = 0;
            while (true)
            {
                var internalTag = GetNextTag(text, ref pos);
                if (internalTag == null)
                    break;
                var internalTagData = GetTagData(text, internalTag, ref pos);
                if (internalTagData == null)
                    break;
                text = text.Remove(pos, internalTagData.Length);
            }
            return text;
        }

        private static string GetNextTag(string src, ref int pos)
        {
            pos = src.IndexOf('<');
            if (pos == -1)
                return null;
            var openTagEndPos = src.IndexOf(' ', pos + 1);
            var openTagEndPos2 = src.IndexOf('>', pos + 1);
            if (openTagEndPos == -1)
                openTagEndPos = openTagEndPos2;
            if ((openTagEndPos != -1) && (openTagEndPos2 != -1))
                if (openTagEndPos > openTagEndPos2)
                    openTagEndPos = openTagEndPos2;
            if (openTagEndPos == -1)
                return null;
            var tag = src.Substring(pos + 1, openTagEndPos - pos - 1);
            return tag;
        }
        #endregion
    }
}
