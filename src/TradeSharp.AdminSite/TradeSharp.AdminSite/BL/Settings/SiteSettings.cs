using System;
using System.IO;
using System.Text;
using System.Web.Hosting;
using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.BL.Settings
{   
    public class SiteSettings
    {
        private static SiteSettings instance;

        public static SiteSettings Instance
        {
            get { return instance ?? (instance = new SiteSettings()); }
        }

        private static readonly string filePath;

        private TraderFilterFunctionStorage functionSettings = new TraderFilterFunctionStorage();
        /// <summary>
        /// формулы, используемые для фильтрации списка пользователей (TOP-N лузеров)
        /// </summary>
        [PropertyXMLTag("FunctionSettings")]
        public TraderFilterFunctionStorage FunctionSettings
        {
            get { return functionSettings; }
            set { functionSettings = value; }
        }

        static SiteSettings()
        {
            filePath = HostingEnvironment.ApplicationPhysicalPath + "settings.xml";
        }

        /// <summary>
        /// закрытый конструктор - читает настройки из файла
        /// </summary>
        private SiteSettings()
        {
            // прочитать файл настроек
            if (!File.Exists(filePath)) return;

            var doc = new XmlDocument();
            try
            {
                lock (this)
                {
                    doc.Load(filePath);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения файла настроек {0} - невозможно загрузить документ: {1}",
                    filePath, ex);
                return;
            }
            
            if (doc.DocumentElement == null) return;
            try
            {
                lock (this)
                {
                    PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка применения настроек из файла", ex);
            }
        }
    
        public void SaveSettings()
        {
            var doc = new XmlDocument();
            var docNode = (XmlElement)doc.AppendChild(doc.CreateElement("settings"));
            try
            {
                lock (this)
                {
                    PropertyXMLTagAttribute.SaveObjectProperties(this, docNode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка преобразования настроек в формат XML", ex);
                return;
            }

            try
            {
                using (var sw = new StreamWriterLog(filePath, false, Encoding.Unicode))
                {
                    using (var xw = new XmlTextWriter(sw) {Formatting = Formatting.Indented})
                    {
                        lock (this)
                        {
                            doc.Save(xw);
                            Logger.Info("Файл настроек сохранен (" + filePath + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения файла настроек {0}: {1}",
                    filePath, ex);
            }
        }
    }
}