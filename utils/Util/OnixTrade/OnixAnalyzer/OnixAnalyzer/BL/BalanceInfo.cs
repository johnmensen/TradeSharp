using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace OnixAnalyzer.BL
{
    class BalanceInfo
    {
        public string Ticket { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        public BalanceInfo(List<string> cells)
        {
            Ticket = cells[0];
            Amount = DealInfo.ParseDecimalSafe(cells[3]);
            Date = DateTime.ParseExact(cells[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);            
        }

        public BalanceInfo(XmlNode dealNode)
        {
            Ticket = dealNode.Attributes["Ticket"].Value;
            Date = DateTime.ParseExact(dealNode.Attributes["Date"].Value, "yyyy-MM-dd HH:mm:ss",
                                           CultureInfo.InvariantCulture);
            Amount = DealInfo.ParseDecimalSafe(dealNode.Attributes["Amount"].Value);
        }

        public void SaveInXml(XmlNode parent)
        {
            var nod = parent.AppendChild(parent.OwnerDocument.CreateElement("balance"));
            DealInfo.AddAttribute(nod, "Ticket", Ticket);
            DealInfo.AddAttribute(nod, "Amount", Amount);
            DealInfo.AddAttribute(nod, "Date", Date);
        }
    }
}
