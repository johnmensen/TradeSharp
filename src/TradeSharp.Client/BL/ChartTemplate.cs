using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;

namespace TradeSharp.Client.BL
{
    public static class ChartTemplate
    {
        public const string UniversalSymbol = "universal";
        public const string AttributeSymbol = "Symbol";
        public const string AttributeName = "Name";
        public const string AttributeChartType = "ChartType";
        public const string AttributeChartSize = "ChartSize";
        public const string AttributeIndicatorUniqueName = "UniqueName";
        private static string symbol;
        private static XmlNode[] templates;

        /// <summary>
        /// Вытаскиваем имена всех шаблонов, котору есть в файле toolset.xml упорядоченные.
        /// </summary>
        /// <param name="currentSymbol">текуший инструмент (валютная пара) по которому нужно упорядочевать шаблоны</param>
        /// <param name="onlyCurrentTickerTemplate">вытаскивать только шаблоны, относящиеся к указанному инструменту</param>
        /// <returns>массив строк из имён шаблонов</returns>
        public static string[] GetChartTemplateNames(string currentSymbol = UniversalSymbol, bool onlyCurrentTickerTemplate = false)
        {
            symbol = currentSymbol;
            FillTemplatesNames();

            var list = templates.OrderBy(node => node.Attributes[AttributeSymbol].Value,
                                         new TemplatesSymbolAttributesComparator())
                                .ThenBy(node => node.Attributes[AttributeName].Value);

            return onlyCurrentTickerTemplate
                       ? list.Where(x => x.Attributes[AttributeSymbol].Value == symbol).
                              Select(node => node.Attributes[AttributeName].Value).ToArray()
                       : list.Select(node => node.Attributes[AttributeName].Value).ToArray();
        }

        /// <summary>
        /// Заполнение значениями массива имён доступны шаблонов (из xml). Вытаскиваем только те у которых атрибуты Name и Symbol есть в наличии
        /// </summary>
        private static void FillTemplatesNames()
        {
            var rootNodeTemplates = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);
            templates = rootNodeTemplates.ChildNodes.Cast<XmlNode>().Where(node => node.Attributes != null &&
                                                                                   node.Attributes[AttributeSymbol] != null &&
                                                                                   node.Attributes[AttributeName] !=null).ToArray();
        }

        /// <summary>
        /// Получить xml узел с описанием шаблона графика, имя которого указано в templateName
        /// </summary>
        /// <param name="templateName">имя шаблона, xml описание которого нужно получить</param>
        /// <returns>xml описание шаблона</returns>
        internal static XmlNode GetChartTemplate(string templateName)
        {
            var rootNodeTemplates =
                            ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);

            var result =
                rootNodeTemplates.ChildNodes.Cast<XmlNode>().FirstOrDefault(template => template.Attributes != null &&
                                                                                        template.Attributes[AttributeName] != null &&                                                               
                                                                                        template.Attributes[AttributeName].Value == templateName);
            return result;
        }

        /// <summary>
        /// Получить все xml узелы с описаниями шаблонов графиков
        /// </summary>
        /// <returns>xml описание шаблонов</returns>
        internal static IEnumerable<XmlNode> GetChartAllTemplates()
        {
            var rootNodeTemplates = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);
            var result = rootNodeTemplates.ChildNodes.Cast<XmlNode>();
            return result;
        }

        /// <summary>
        /// Перевод строки с размерам графика в объект типа Size
        /// </summary>
        /// <param name="strSize">строка в формате "ширина;выста"</param>
        /// <returns>объект типа Size, либо Null, если не удалось привести строку</returns>
        internal static Size? StringToSize(string strSize)
        {
            Size? result;
            try
            {
                result = new Size(Convert.ToInt32(strSize.Split(';')[0]), Convert.ToInt32(strSize.Split(';')[1]));
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Проверка на то, является ли указанный шаблон привязанным к какому то конкретному инструменту
        /// </summary>
        /// <param name="templateName">имя проверяемого шаблона</param>
        /// <returns>является ли шаблон привязанным</returns>
        internal static bool IsUniversalTemplate(string templateName)
        {
            var template = GetChartTemplate(templateName);
            return template.Attributes != null &&
                   template.Attributes[AttributeSymbol] != null &&
                   template.Attributes[AttributeSymbol].Value != UniversalSymbol &&
                   !string.IsNullOrEmpty(template.Attributes[AttributeSymbol].Value);
        }

        /// <summary>
        /// Удаляет шаблон с указатым имением
        /// </summary>
        /// <param name="templateName">имя шаблона, который следует удалить</param>
        internal static void DellChartTemplate(string templateName)
        {
            var rootNodeTemplates = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);
            var doc = rootNodeTemplates.OwnerDocument;
            var item = rootNodeTemplates.ChildNodes.Cast<XmlNode>()
                             .FirstOrDefault(x => x.Attributes[AttributeName].Value == templateName);

            if (item != null) rootNodeTemplates.RemoveChild(item);

            ToolSettingsStorageFile.SaveXml(doc);
        }

        /// <summary>
        /// Обновление свойства Name какого либо тега chartTemplate
        /// </summary>
        /// <param name="oldName">текущее значение свойства</param>
        /// <param name="newName">новое значение имени</param>
        /// <returns>Был ли в списке элемент с именем указаным</returns>
        public static bool UpdateChartTemplateName(string oldName, string newName)
        {
            var rootNodeTemplates = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);
            var doc = rootNodeTemplates.OwnerDocument;
            var exist = false;

            // Вытаскиваем изменяемый элемент
            var item = rootNodeTemplates.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Attributes[AttributeName].Value == oldName);

            if (item != null)
            {
                // Вытаскиваем все имена
                var templateNames = rootNodeTemplates.ChildNodes.Cast<XmlNode>().Where(node => node.Attributes != null &&
                                                                                       node.Attributes[AttributeName] != null).
                                                                                       Select(x => x.Attributes[AttributeName].Value).ToArray();
                
                if (templateNames.Contains(newName))
                {
                    var existItem = rootNodeTemplates.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Attributes[AttributeName].Value == newName);
                    if (existItem != null) rootNodeTemplates.RemoveChild(existItem);
                    exist = true;
                }

                item.Attributes[AttributeName].Value = newName;
                ToolSettingsStorageFile.SaveXml(doc);
            }
            return exist;
        }

        /// <summary>
        /// Класс, сравнивающий строки по алгоритму, нужному для сортировки названий шаблонов. 
        /// Первыми будут названия тех шаблонов, которые созданы специально для этой катировки 
        /// </summary>
        public class TemplatesSymbolAttributesComparator : IComparer<string>
        {
            private readonly IComparer<string> baseComparer = StringComparer.CurrentCulture;

            public int Compare(string x, string y)
            {
                if (baseComparer.Compare(x, y) == 0)
                    return 0;

                if (baseComparer.Compare(x, symbol) == 0) return -1;
                if (baseComparer.Compare(y, symbol) == 0) return 1;

                if (baseComparer.Compare(x, UniversalSymbol) == 0) return -1;
                if (baseComparer.Compare(y, UniversalSymbol) == 0) return 1;

                return baseComparer.Compare(x, y);
            }
        }
    }
}