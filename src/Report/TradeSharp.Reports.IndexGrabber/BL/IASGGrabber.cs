using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using Entity;
using TradeSharp.Reports.Lib.IndexGrabber;
using TradeSharp.Util;

namespace TradeSharp.Reports.IndexGrabber.BL
{
    class IASGGrabber : IndexDataGrabber
    {
        public const string GrabberTag = "IASGGrabber";
        private const string queryString = "http://www.iasg.com/cta-indexes/index/iasg-cta-index";
        private static readonly Regex regCellValue = new Regex(@"[\-\.0-9]+");
        private static readonly Regex regCellValueInt = new Regex(@"[0-9]{4}");
        private const decimal ProfitMultiplier = 0.01M;

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

            const string tablePreffix = "<table id=\"dnn_ctr550_Indexes_ctl00_Monthly1_tblRecentReturns\"";
            var indexStart = queryResult.IndexOf(tablePreffix);
            if (indexStart < 0)
            {
                Logger.Error("IASGGrabber: не найдено начало таблицы");
                return result;
            }
            const string closeTag = "</table>";
            var indexEnd = queryResult.IndexOf(closeTag, indexStart);
            if (indexEnd < 0)
            {
                Logger.Error("IASGGrabber: не найден конец таблицы");
                return result;
            }
            var tableText = queryResult.Substring(indexStart, indexEnd - indexStart + closeTag.Length);
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(tableText);
                if (doc.DocumentElement == null) throw new Exception("Корневой элемент пуст");
            }
            catch (Exception ex)
            {
                Logger.Error("IASGGrabber: ошибка загрузки XML таблицы", ex);
                return result;
            }

            var indexData = new List<Cortege2<DateTime, decimal>>();
            foreach (XmlElement row in doc.DocumentElement.ChildNodes)
            {
                if (row.Name != "tr") continue;
                var cells = row.ChildNodes.OfType<XmlElement>().Where(c => c.Name.Equals("td", 
                    StringComparison.OrdinalIgnoreCase)).ToList();
                if (cells.Count != 15) continue;
                
                var year = GetCellValueInt(cells[0].InnerXml);
                if (!year.HasValue) continue;

                var monthValues = new List<decimal>();
                for (var i = 1; i <= 12; i++)
                {
                    var cell = cells[i];
                    var index = GetCellValueDecimal(cell.InnerXml);
                    if (!index.HasValue) break;
                    monthValues.Add(index.Value);
                }
                //if (monthValues.Count != 12) continue;
                for (var i = 0; i < monthValues.Count; i++)
                {
                    indexData.Add(new Cortege2<DateTime, decimal>(
                        new DateTime(year.Value, i + 1, 1).AddMonths(1), monthValues[i] * ProfitMultiplier));
                }
            }
            indexData = indexData.OrderBy(d => d.a).ToList();
            return indexData;
        }

        private static decimal? GetCellValueDecimal(string cellXml)
        {
            var matches = regCellValue.Matches(cellXml);
            if (matches.Count == 0) return null;
            return matches[matches.Count - 1].Value.ToDecimalUniformSafe();
        }

        private static int? GetCellValueInt(string cellXml)
        {
            var matches = regCellValueInt.Matches(cellXml);
            if (matches.Count == 0) return null;
            return matches[matches.Count - 1].Value.ToIntSafe();
        }

        public IASGGrabber(XmlElement nodeSets)
        {
        }
    }
}
