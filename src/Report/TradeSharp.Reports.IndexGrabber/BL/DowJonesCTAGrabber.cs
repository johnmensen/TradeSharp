using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Entity;
using TradeSharp.Reports.Lib.IndexGrabber;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Reports.IndexGrabber.BL
{
    class DowJonesCTAGrabber : IndexDataGrabber
    {
        public const string GrabberTag = "DowJonesCTAGrabber";

        private const string queryString = "http://www.hedgeindex.com/hedgeindex/en/default.aspx?cy=USD";
        private static readonly Regex regIndex = new Regex(@"[\-\.0-9]+(?=%)");
        private static readonly Regex regDate = new Regex(@"\w{3}\-\d{2}");
        private const decimal ProfitMultiplier = 0.01M;

        public DowJonesCTAGrabber(XmlElement nodeSets)
        {            
        }

        public override List<Cortege2<DateTime, decimal>> GrabIndexData()
        {
            var result = new List<Cortege2<DateTime, decimal>>();
            string queryResult;
            try
            {
                queryResult = QueryPage(queryString);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка запроса \"{0}\": {1}", queryString, ex);
                return result;
            }

            // поиск текста строки таблицы            
            const string tablePreffix = "<tr class=\"content11\">";
            var indexStart = queryResult.IndexOf(tablePreffix);
            if (indexStart < 0)
            {
                Logger.Error("DowJonesCTAGrabber: не найдено начало таблицы");
                return result;
            }
            const string closeTag = "</tr>";
            var indexEnd = queryResult.IndexOf(closeTag, indexStart);
            if (indexEnd < 0)
            {
                Logger.Error("DowJonesCTAGrabber: не найден конец таблицы");
                return result;
            }
            var rowText = queryResult.Substring(indexStart, indexEnd - indexStart + closeTag.Length);
            // ищем подстроку [-]XX.XX%
            var matches = regIndex.Matches(rowText);
            if (matches.Count != 5)
            {
                Logger.ErrorFormat("DowJonesCTAGrabber: подстрока не содержит данных индекса ({0} записей)",
                    matches.Count);
                return result;
            }
            var indexValue = matches[3].Value.ToDecimalUniform();

            // найти дату
            const string datePreffix = "<td id=\"ctl00_ContentPlaceHolder1_homepageperformancenew1_tdNav\" align=\"right\">";
            indexStart = queryResult.IndexOf(datePreffix);
            if (indexStart < 0)
            {
                Logger.Error("DowJonesCTAGrabber: не найдено начало ячейки даты");
                return result;
            }
            const string closeDateTag = "</td>";
            indexEnd = queryResult.IndexOf(closeDateTag, indexStart);
            if (indexEnd < 0)
            {
                Logger.Error("DowJonesCTAGrabber: не найден конец ячейки даты");
                return result;
            }
            var cellText = queryResult.Substring(indexStart, indexEnd - indexStart);
            matches = regDate.Matches(cellText);
            if (matches.Count == 0)
            {
                Logger.ErrorFormat("DowJonesCTAGrabber: подстрока не содержит данных по дате");
                return result;
            }
            var date = DateFromString(matches[0].Value);

            result.Add(new Cortege2<DateTime, decimal>(date, indexValue * ProfitMultiplier));
            return result;
        }

        private static readonly Dictionary<string, int> monthNames = 
            new Dictionary<string, int>
                {
                    { "Jan", 1 }, { "Feb", 2 }, { "Mar", 3 }, { "Apr", 4 }, { "May", 5 }, { "Jun", 6 },
                    { "Jul", 7 }, { "Aug", 8 }, { "Sep", 9 }, { "Oct", 10 }, { "Nov", 11 }, { "Dec", 12 }
                };
        /// <summary>
        /// Oct-31, Nov-30 ...
        /// </summary>        
        private static DateTime DateFromString(string str)
        {
            var monthName = str.Substring(0, 3);
            var month = monthNames.First(n => n.Key.Equals(monthName, StringComparison.OrdinalIgnoreCase)).Value;
            var day = str.Substring(4).ToInt();
            var year = DateTime.Now.Year;
            if (day == 31 && month == 12) year--;
            
            return new DateTime(year, month, day).AddDays(1);
        }
    }
}
