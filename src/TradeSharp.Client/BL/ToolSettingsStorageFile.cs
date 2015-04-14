using System;
using System.IO;
using System.Xml;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    static class ToolSettingsStorageFile
    {
        public const string NodeNameToolButtons = "toolButtons";
        public const string NodeNameToolSysButtons = "toolSysButtons";
        public const string NodeNameScripts = "scripts";
        public const string NodeNameSeries = "series";
        public const string NodeNameMessageTemplates = "messageTemplates";
        public const string NodeNameChartTemplates = "chartTemplates";
        public const string NodeNameChartTemplate = "chartTemplate";
        public const string NodeNameChartTemplateIndicator = "indicator";

        private static readonly string filePath = string.Format("{0}\\toolset.xml", ExecutablePath.ExecPath);

        public static XmlElement LoadOrCreateNode(string nodeName, bool removeChildNodes = true)
        {
            var doc = new XmlDocument();
            if (File.Exists(filePath))
            {
                try
                {
                    doc.Load(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка чтения файла настроек", ex);
                }
            }

            var mainNode = doc.DocumentElement ?? (XmlElement)doc.AppendChild(doc.CreateElement("settings"));
            var nodesToolBtnSets = mainNode.GetElementsByTagName(nodeName);
            var nodeSets = nodesToolBtnSets.Count == 0
                ? (XmlElement)mainNode.AppendChild(doc.CreateElement(nodeName))
                : (XmlElement)nodesToolBtnSets[0];
            if (removeChildNodes)
            while (nodeSets.ChildNodes.Count > 0)
                nodeSets.RemoveChild(nodeSets.ChildNodes[0]);

            return nodeSets;
        }

        public static XmlElement LoadNode(string nodeName)
        {
            if (!File.Exists(filePath)) return null;
            var doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка чтения файла настроек", ex);
                return null;
            }
            if (doc.DocumentElement == null) return null;
            var nodesToolBtnSets = doc.DocumentElement.GetElementsByTagName(nodeName);
            if (nodesToolBtnSets.Count == 0) return null;
            return (XmlElement)nodesToolBtnSets[0];
        }

        public static void SaveXml(XmlDocument doc)
        {
            using (var sw = new StreamWriter(filePath, false))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    doc.Save(xw);
                }
            }
        }
    }
}
