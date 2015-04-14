using System.Globalization;
using System.Xml.Serialization;

namespace MarkupCalculator.CommonClasses
{
    [XmlRoot("account")]
    public class AccountXml
    {
        public static DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        static AccountXml()
        {
            DateTimeFormatInfo = new DateTimeFormatInfo
            {
                ShortTimePattern = "yyyy-MM-dd HH:mm:ss"
            };
        }

        [XmlAttribute("Id")]
        public int Id { get; set; }

        [XmlAttribute("InitBalance")]
        public decimal InitBalance { get; set; }

        [XmlElement("deals")]
        public DealsXml Deals { get; set; }

        [XmlElement("balances")]
        public BalancesXml Balances { get; set; }
    }

    public class DealsXml
    {
        [XmlElement("deal")]
        public DealXml[] Items { get; set; }
    }

    public class DealXml
    {
        [XmlAttribute("Ticket")]
        public int Ticket { get; set; }

        [XmlAttribute("OpenTime")]
        public string OpenTime { get; set; }

        [XmlAttribute("CloseTime")]
        public string CloseTime { get; set; }

        [XmlAttribute("DealType")]
        public int DealType { get; set; }

        [XmlAttribute("Lots")]
        public decimal Lots { get; set; }

        [XmlAttribute("Symbol")]
        public string Symbol { get; set; }

        [XmlAttribute("Open")]
        public float Open { get; set; }

        [XmlAttribute("Close")]
        public float Close { get; set; }

        [XmlAttribute("SL")]
        public float SL { get; set; }

        [XmlAttribute("TP")]
        public float TP { get; set; }

        [XmlAttribute("Swap")]
        public float Swap { get; set; }

        [XmlAttribute("Profit")]
        public decimal Profit { get; set; }

        [XmlAttribute("Comment")]
        public string Comment { get; set; }
    }

    public class BalancesXml
    {
        [XmlElement("balance")]
        public BalanceXml[] Items { get; set; }
    }

    public class BalanceXml
    {
        [XmlAttribute("Ticket")]
        public int Ticket { get; set; }

        [XmlAttribute("Amount")]
        public decimal Amount { get; set; }

        [XmlAttribute("Date")]
        public string Date { get; set; }
    }
}
