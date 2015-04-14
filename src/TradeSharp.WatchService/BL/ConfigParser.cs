using System;
using System.Collections.Generic;
using System.Xml;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.WebContract;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    static class ConfigParser
    {
        public static List<ServiceStateUnit> ReadUnits()
        {
            var units = new List<ServiceStateUnit>();
            var serviceLocation = ExecutablePath.ExecPath;
            var settingsPath = serviceLocation + "\\settings.xml";
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(settingsPath);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Невозможно открыть файл настроек {0}", settingsPath), ex);
                throw;
            }
            try
            {
                var docUnits = xmlDoc.SelectSingleNode("*/units");
                // сервисы для наблюдения
                foreach (XmlNode node in docUnits.ChildNodes)
                {
                    var binding = node.Attributes["binding"].Value;
                    var name = node.Attributes["name"].Value;
                    var severity = node.Attributes["severity"] == null
                                       ? GetReportSeverityFlagFromStr("")
                                       : GetReportSeverityFlagFromStr(node.Attributes["severity"].Value);

                    var additionalObject = new Dictionary<string, string>();
                    
                    // читаем дополнительные параметры
                    if (node.Attributes["url"] != null)
                        additionalObject.Add("url", node.Attributes["url"].Value);
                    if (node.Attributes["regexp"] != null)
                        additionalObject.Add("regexp", node.Attributes["regexp"].Value);


                    var unit = new ServiceStateUnit(binding, severity, name, additionalObject);
                    if (node.Attributes["code"] != null) unit.Code = node.Attributes["code"].Value;
                    if (node.Attributes["updateTimoutSec"] != null) unit.UpdateTimeoutSeconds =
                        int.Parse(node.Attributes["updateTimoutSec"].Value);

                    units.Add(unit);
                }
                return units;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка чтения файла настроек {0}", settingsPath), ex);
                throw;
            }
        }

        public static List<TradeSharpServiceProcess> ReadProcessInfo()
        {
            var units = new List<TradeSharpServiceProcess>();
            var serviceLocation = ExecutablePath.ExecPath;
            var settingsPath = serviceLocation + "\\settings.xml";
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(settingsPath);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Невозможно открыть файл настроек {0}", settingsPath), ex);
                throw;
            }
            try
            {
                var docUnits = xmlDoc.SelectSingleNode("*/processes");
                // сервисы для наблюдения
                foreach (XmlNode node in docUnits.ChildNodes)
                {
                    var proc = new TradeSharpServiceProcess
                        {
                            FileName = node.Attributes["file"].Value,
                            Name = node.Attributes["name"].Value,
                            Title = node.Attributes["title"].Value
                        };
                    units.Add(proc);
                }
                return units;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка чтения файла настроек {0}", settingsPath), ex);
                throw;
            }
        }

        private static ServiceProcessState GetReportSeverityFlagFromStr(string s)
        {
            if (string.IsNullOrEmpty(s))
                return ServiceProcessState.Offline | ServiceProcessState.HasCriticalErrors;
            var parts = s.Split(' ', ',', ';');
            var flags = ServiceProcessState.OK; // 0
            foreach (var part in parts)
            {
                flags = flags | (ServiceProcessState)Enum.Parse(typeof(ServiceProcessState), part);
            }
            return flags;
        }
    }
}
