using System;
using System.IO;
using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Processing.WebMoney.Server
{
    /// <summary>
    /// Сохранаяет и читает настройки WebMoney из файла XML
    /// </summary>
    public class WebMoneySettings
    {
        private static WebMoneySettings instance;
        public static WebMoneySettings Instance
        {
            get { return instance ?? (instance = new WebMoneySettings()); }
        }

        /// <summary>
        /// Время последнего обновления файла, хранящего время обращения к серверам WebMoney
        /// </summary>
        [PropertyXMLTag("LastUpdateDate")]
        public string LastUpdateDate { get; set; }

        /// <summary>
        /// Уникальный идентификатор последней проведённой транзакции
        /// </summary>
        [PropertyXMLTag("LastTransferId")]
        public string LastTransferId { get; set; }

        private readonly string fileName = string.Format("{0}\\wmSettings.xml", ExecutablePath.ExecPath);

        public string FullFileName
        {
            get { return fileName; }
        }

        private WebMoneySettings()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            var doc = new XmlDocument();
            if (!File.Exists(fileName))
            {
                SaveSettings();
                return;
            }

            try
            {
                doc.Load(fileName);
                PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
            }
            catch (Exception ex)
            {
                Logger.Error("LoadSettings()", ex);
            }
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("settings"));
            PropertyXMLTagAttribute.SaveObjectProperties(this, doc.DocumentElement);
            doc.Save(fileName);
        }
    }
}
