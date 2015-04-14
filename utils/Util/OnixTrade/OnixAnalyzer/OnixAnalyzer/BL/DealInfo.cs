using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace OnixAnalyzer.BL
{
    class DealInfo
    {
        public string Ticket { get; set; }
        public DateTime OpenTime { get; set; }
        public int DealType { get; set; }
        public decimal Lots { get; set; }
        public string Symbol { get; set; }
        public decimal Open { get; set; }
        public decimal SL { get; set; }
        public decimal TP { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal Close { get; set; }
        public decimal Swap { get; set; }

        public decimal Profit { get; set; }
        public string Comment { get; set; }

        #region Statistics
        public decimal OpenDeposit { get; set; }
        public decimal? CloseDeposit { get; set; }
        #endregion

        private static CultureInfo parseCulture = CultureInfo.InvariantCulture;

        public DealInfo(List<string> cells)
        {
            // 0-Ticket 1-OpenTime 2-Type 3-Lots 4-Symbol 5-Open 6-SL 7-TP 8-CloseTime 9-Close 10-Swap 11-(-hidden-) 12-Profit 13-Comment 
            // 0)32344939 1)2009-04-03 22:00:00 2)sell 3)0.10 4)EURUSD 5)1.3482 6)0.0000 7)1.3467 
            // 8)2009-04-03 22:07:00 9)1.3467 10)0.00 11)(-0.00-) 12)15.00 13)(51) 
            Ticket = cells[0];
            OpenTime = DateTime.ParseExact(cells[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DealType = cells[2] == "sell" ? -1 : 1;
            Lots = ParseDecimalSafe(cells[3]);
            Symbol = cells[4];
            Open = ParseDecimalSafe(cells[5]);
            SL = ParseDecimalSafe(cells[6]);
            TP = ParseDecimalSafe(cells[7]);
            CloseTime = DateTime.ParseExact(cells[8], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Close = ParseDecimalSafe(cells[9]);
            Swap = ParseDecimalSafe(cells[10]);

            Profit = ParseDecimalSafe(cells[12]);
            Comment = cells[13];
        }

        public DealInfo(XmlNode dealNode)
        {
            Ticket = dealNode.Attributes["Ticket"].Value;
            OpenTime = DateTime.ParseExact(dealNode.Attributes["OpenTime"].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DealType = dealNode.Attributes["DealType"].Value == "sell" ? -1 : 1;

            Lots = ParseDecimalSafe(dealNode.Attributes["Lots"].Value);
            Symbol = dealNode.Attributes["Symbol"].Value;
            Open = ParseDecimalSafe(dealNode.Attributes["Open"].Value);
            SL = ParseDecimalSafe(dealNode.Attributes["SL"].Value);
            TP = ParseDecimalSafe(dealNode.Attributes["TP"].Value);
            CloseTime = DateTime.ParseExact(dealNode.Attributes["CloseTime"].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Close = ParseDecimalSafe(dealNode.Attributes["Close"].Value);
            Swap = ParseDecimalSafe(dealNode.Attributes["Swap"].Value);

            Profit = ParseDecimalSafe(dealNode.Attributes["Profit"].Value);
            Comment = dealNode.Attributes["Comment"].Value;
        }

        public static decimal ParseDecimalSafe(string val)
        {
            return Decimal.Parse(val.Replace(" ", ""), parseCulture);
        }

        public void SaveInXml(XmlNode parent)
        {
            var nod = parent.AppendChild(parent.OwnerDocument.CreateElement("deal"));
            AddAttribute(nod, "Ticket", Ticket);

            AddAttribute(nod, "OpenTime", OpenTime);
            AddAttribute(nod, "DealType", DealType);
            AddAttribute(nod, "Lots", Lots);
            AddAttribute(nod, "Symbol", Symbol);
            AddAttribute(nod, "Open", Open);
            AddAttribute(nod, "SL", SL);
            AddAttribute(nod, "TP", TP);
            AddAttribute(nod, "CloseTime", CloseTime);
            AddAttribute(nod, "Close", Close);
            AddAttribute(nod, "Swap", Swap);
            AddAttribute(nod, "Profit", Profit);
            AddAttribute(nod, "Comment", Comment);
        }

        public static void AddAttribute(XmlNode node, string name, string val)
        {
            var attr = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
            attr.Value = val;
        }
        public static void AddAttribute(XmlNode node, string name, decimal val)
        {
            AddAttribute(node, name, val.ToString(parseCulture));
        }
        public static void AddAttribute(XmlNode node, string name, DateTime val)
        {
            AddAttribute(node, name, val.ToString("yyyy-MM-dd HH:mm:ss", parseCulture));
        }
    }
}
