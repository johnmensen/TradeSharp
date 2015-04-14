using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// XML представление файла торгового сигнала
    /// </summary>
    public class TradeSignalXml
    {
        #region Запросы к серверу
        public const string ReqPtrSignalUpdates = "querySignalUpdates";

        public const string ReqPtrGetForecast = "queryForecast";

        public const string ReqPtrForecastCategory = "category";

        public const string ReqPtrForecastTicker = "ticker";

        public const string ReqPtrForecastTimeframe = "timeframe";
        #endregion

        #region Шаблоны ответов сервера

        public const string ResponseForecastIsEmpty = "empty";

        public const string ResponseErrorPreffix = "error:";
        #endregion

        public static Encoding DefaultEncoding = Encoding.UTF8;

        public const string XmlEncodingDeclaration = "UTF-8";

        private static readonly bool validateXml = AppConfig.GetBooleanParam("ValidateXml", true);

        private static readonly XmlSchemaSet schemas;

        public const string TagNameForecast = "forecast";

        public const string TagNameObjects = "objects";

        public const string AttributeUpdateTime = "updated";

        public const string TimeframeSeparator = ",";

        /// <summary>
        /// конструктор, загружается XSD
        /// </summary>
        static TradeSignalXml()
        {
            if (!validateXml) return;
            var filePath = ExecutablePath.ExecPath + "\\ForecastValidationScheme.txt";
            if (!File.Exists(filePath)) return;

            try
            {
                string markup;
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    markup = sr.ReadToEnd();
                }
                if (string.IsNullOrEmpty(markup)) return;
                schemas = new XmlSchemaSet();
                schemas.Add("", XmlReader.Create(new StringReader(markup)));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения схемы XML ({0}): {1}", filePath, ex);
            }
        }

        /// <summary>
        /// проверить валидность XML прогноза и дополнить
        /// его временем обновления (текущим)
        /// </summary>        
        public static bool XmlIsValid(ref string xml, bool setUpdateTime)
        {
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                return false;
            }
            if (doc.DocumentElement == null) return false;
            if (validateXml)
                if (!ValidateXmlWithXsd(doc)) 
                    return false;

            var nodeForecast = (XmlElement)doc.DocumentElement.SelectSingleNode(TagNameForecast);
            if (nodeForecast == null) return false;

            // проставить время обновления
            if (setUpdateTime)
            {
                var timeString = DateTime.Now.ToStringUniform();

                if (nodeForecast.Attributes[AttributeUpdateTime] == null)
                    nodeForecast.Attributes.Append(doc.CreateAttribute(AttributeUpdateTime)).Value = timeString;
                else
                    nodeForecast.Attributes[AttributeUpdateTime].Value = timeString;

                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    doc.Save(sw);                    
                }
                xml = sb.ToString();
            }

            return true;
        }

        /// <summary>
        /// прочитать XML из файла, получить оттуда дату-время последнего обновления
        /// </summary>
        public static bool GetSignalUpdateParamsFromFile(string fileName, Encoding encoding, 
            out DateTime timeUpdated, out int objectsCount)
        {
            timeUpdated = new DateTime();
            objectsCount = 0;
            try
            {
                using (var sr = new StreamReader(fileName, encoding))
                {
                    var doc = new XmlDocument();
                    doc.Load(sr);
                    if (doc.DocumentElement == null) return false;

                    var nodeForecast = (XmlElement)doc.DocumentElement.SelectSingleNode(TagNameForecast);
                    if (nodeForecast == null) return false;
                    // время обновления
                    if (nodeForecast.Attributes[AttributeUpdateTime] == null) return false;
                    var timeString = nodeForecast.Attributes[AttributeUpdateTime].Value;
                    var timeUpdatedNul = timeString.ToDateTimeUniformSafe();
                    if (!timeUpdatedNul.HasValue) return false;
                    timeUpdated = timeUpdatedNul.Value;
                    // количество объектов
                    var nodeObjects = (XmlElement)doc.DocumentElement.SelectSingleNode(TagNameObjects);
                    objectsCount = nodeObjects == null ? 0 : nodeObjects.ChildNodes.Count;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("TradeSignalXml.GetSignalUpdateParamsFromFile({0}): {1}", fileName, ex);
                return false;
            }
        }

        /// <summary>
        /// !! потом допишу
        /// </summary>        
        private static bool ValidateXmlWithXsd(XmlDocument doc)
        {
            return true;
        }
    }
}
