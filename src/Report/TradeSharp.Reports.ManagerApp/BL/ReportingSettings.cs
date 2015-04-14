using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Reports.ManagerApp.BL
{
    public class ReportingSettings
    {
        private static ReportingSettings instance;
        public static ReportingSettings Instance
        {
            get { return instance ?? (instance = new ReportingSettings()); }
        }

        [PropertyXMLTag("TemplateFolder")]
        public string TemplateFolder { get; set; }

        [PropertyXMLTag("TempFolder")]
        public string TempFolder { get; set; }

        [PropertyXMLTag("DestFolder")]
        public string DestFolder { get; set; }

        [PropertyXMLTag("AccountId")]
        public int AccountId { get; set; }

        [PropertyXMLTag("BenchmarkA")]
        public string BenchmarkA { get; set; }

        [PropertyXMLTag("BenchmarkB")]
        public string BenchmarkB { get; set; }

        [PropertyXMLTag("ServerPath")]
        public string ServerSidePath { get; set; }

        private ReportingSettings()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            var doc = new XmlDocument();
            doc.Load(string.Format("{0}\\settings.xml", ExecutablePath.ExecPath));
            PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("settings"));
            PropertyXMLTagAttribute.SaveObjectProperties(this, doc.DocumentElement);
            doc.Save(string.Format("{0}\\settings.xml", ExecutablePath.ExecPath));
        }
    }
}
