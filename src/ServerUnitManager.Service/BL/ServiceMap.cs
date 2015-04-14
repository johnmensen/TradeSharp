using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System;
using Entity;
using TradeSharp.Util;

namespace ServerUnitManager.Service.BL
{
    public class ServiceMap
    {
        private static readonly string fileName = ExecutablePath.ExecPath + "\\settings.xml";

        private List<ServiceUnit> items = new List<ServiceUnit>();
        
        [PropertyXMLTag("Items")]
        public List<ServiceUnit> Items
        {
            get { return items; }
            set { items = value; }
        }

        public static ServiceMap LoadSettings()
        {
            var sets = new ServiceMap();
            
            // прочитать файл
            if (!File.Exists(fileName)) return sets;
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                return sets;
            }

            PropertyXMLTagAttribute.InitObjectProperties(sets, doc.DocumentElement);
            return sets;
        }
    
        public void SaveSettings()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("settings"));
            PropertyXMLTagAttribute.SaveObjectProperties(this, doc.DocumentElement);
            using (var sw = new StreamWriterLog(fileName, false, Encoding.Unicode))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    doc.Save(xw);
                }
            }
        }
    }

    
    public class ServiceUnit
    {
        [PropertyXMLTag("Folder")]
        public string Folder { get; set; }

        [PropertyXMLTag("ServiceExecFileName")]
        public string ServiceExecFileName { get; set; }

        [PropertyXMLTag("ServiceName")]
        public string ServiceName { get; set; }

        [PropertyXMLTag("DependsOn")]
        public string DependsOn { get; set; }

        public DateTime? StopCommandSentTime { get; set; }
    }
}
