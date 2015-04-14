using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;

namespace TradeSharp.Util
{
    /// <summary>
    /// в XML-файле в разделе ТипОбъекта (например, IndicatorCurrencyIndex)
    /// ищются записи именем свойства и размещаются в списке подстановки
    /// для строковых свойств
    /// </summary>
    public class LookupTypeEditor : UITypeEditor
    {
        /// <summary>
        /// макс. количество сохраняемых значений для одного свойства
        /// одного индикатора
        /// убивается самый старый
        /// </summary>
        public const int MaxLookupValues = 15;

        private static string GetLookupFilePath()
        {
            const string xmlFileName = "indicator_lookup.xml";
            var execPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return string.Format("{0}\\{1}", execPath, xmlFileName);
        }

        /// <summary>
        /// заполняется либо pa, либо pd
        /// </summary>        
        public static List<string> GetPropValuesFromXML(PropertyInfo pi, PropertyDescriptor pd, object instance)
        {
            var items = new List<string>();
            // получить файл с данными для подстановки
            var path = GetLookupFilePath();
            if (!File.Exists(path)) return items;

            using (var fs = new StreamReader(path))
            {
                var doc = new XmlDocument();
                doc.Load(fs);
                if (doc.DocumentElement == null) return items;

                // читаем файл
                string section, key;
                if (pi != null)
                    GetPropertySectionKey(pi, instance, out section, out key);
                else
                    GetPropertySectionKey(pd, instance, out section, out key);

                var nodes = doc.DocumentElement.SelectNodes(string.Format("/*/{0}/{1}", section, key));
                if (nodes == null) return items;

                foreach (XmlNode node in nodes)
                    items.Add(node.InnerText);
            }
            return items;
        }

        /// <summary>
        /// обновить файл с данными для подстановки по завершению редактирования объекта
        /// (например, IndicatorRSI)
        /// </summary>
        /// <param name="instance">отредактированный объект</param>
        public static void UpdatePropValues(object instance)
        {
            // загрузить или прочитать документ
            var doc = new XmlDocument();
            var path = GetLookupFilePath();
            if (File.Exists(path))
            {
                using (var fs = new StreamReader(path))
                {
                    doc.Load(fs);
                }
            }
            if (doc.DocumentElement == null)
                doc.AppendChild(doc.CreateElement("IndicatorSettings"));
            // получить значения свойств объекта
            // свойства типа Строка помеченные атрибутом Editor
            var props = instance.GetType().GetProperties().Where(
                p => p.PropertyType == typeof(string) && 
                    p.GetCustomAttributes(true).Any(at => at is PropertyLookupStorePathAttribute)).ToList();
            if (props.Count == 0) return;

            foreach (var prop in props)
            {
                var val = (string)prop.GetValue(instance, null);
                // если такое значение не сохранено...                                
                string section, key;
                GetPropertySectionKey(prop, instance, out section, out key);
                var nodes = doc.DocumentElement.SelectNodes(string.Format("/*/{0}[{1}='{2}']",
                    section, key, val));
                if (nodes != null)
                    if (nodes.Count != 0) continue;
                // убрать лишние
                RemoveExcessNode(doc, section, key);
                // добавить
                AddNodeToXmlDoc(doc.DocumentElement, val, section, key);
            }

            // сохранить элемент
            doc.Save(path);
        }

        private static void GetPropertySectionKey(PropertyDescriptor pi, object instance,
            out string section, out string key)
        {
            var attrs = pi.Attributes.OfType<PropertyLookupStorePathAttribute>().ToArray();
            if (attrs.Length == 0)
            {
                section = instance.GetType().ToString();
                key = pi.Name;
                return;
            }
            var plsp = attrs[0];
            section = plsp.nameSection;
            key = plsp.nameKey;
        }

        private static void GetPropertySectionKey(PropertyInfo pi, object instance,
            out string section, out string key)
        {
            var attrs = pi.GetCustomAttributes(typeof(PropertyLookupStorePathAttribute), true);
            if (attrs.Length == 0)
            {
                section = instance.GetType().ToString();
                key = pi.Name;
                return;
            }
            var plsp = (PropertyLookupStorePathAttribute)attrs[0];
            section = plsp.nameSection;
            key = plsp.nameKey;
        }

        private static void RemoveExcessNode(XmlDocument doc, string parent, string nodeName)
        {
            var nodes = doc.DocumentElement.SelectNodes(string.Format("/*/{0}/{1}",
                    parent, nodeName));
            if (nodes == null) return;
            if (nodes.Count < MaxLookupValues) return;

            var oldestNodeTime = DateTime.MaxValue;
            XmlElement oldest = null;
            foreach (XmlElement node in nodes)
            {
                var date = DateTime.Parse(node.Attributes["TimeCreated"].Value, CultureProvider.Common);
                if (date < oldestNodeTime)
                {
                    oldest = node;
                    oldestNodeTime = date;
                }
            }
            oldest.ParentNode.RemoveChild(oldest);
        }

        /// <summary>
        /// добавить все узлы, если какой-то пропущен в иерархии
        /// и установить значение элемента
        /// </summary>        
        private static void AddNodeToXmlDoc(XmlElement parent, string val, params string[] nodes)
        {
            for (var i = 0; i < nodes.Length - 1; i++)
            {
                var sub = parent.SelectNodes(nodes[i]);
                if (sub != null)
                    if (sub.Count != 0)
                        parent = (XmlElement)sub[0];
                    else
                        parent = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(nodes[i]));
            }
            parent = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(nodes[nodes.Length - 1]));
            parent.InnerXml = val;
            // временная метка
            var atrTime = parent.Attributes.Append(parent.OwnerDocument.CreateAttribute("TimeCreated"));
            atrTime.Value = DateTime.Now.ToString(CultureProvider.Common);
        }

        // This class is the list that will pop up when we click
        // on the down arrow of the mock-combo box in the designer.
        private class PropertiesList : ListBox
        {
            public PropertiesList(ITypeDescriptorContext context)
            {
                var listItems = GetPropValuesFromXML(null, context.PropertyDescriptor, context.Instance);
                foreach (var val in listItems)
                    Items.Add(val);

                Sorted = true;
                BorderStyle = BorderStyle.None;
            }
        }

        /// <summary>
        /// Displays a list of available values for the specified component than sets the value.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services may be obtained.</param>
        /// <param name="value">An instance of the value being edited.</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed, this method should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                // This service is in charge of popping our ListBox.
                var service1 = ((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService)));

                if (service1 != null)
                {
                    // This is an internal Microsoft class representing the PropertyGrid entry for our component.
                    if (provider.GetType().FullName == "System.Windows.Forms.PropertyGridInternal.PropertyDescriptorGridEntry")
                    {
                        var list = new PropertiesList(context);
                        // Drop the list control.
                        service1.DropDownControl(list);

                        if (list.SelectedIndices.Count == 1)
                        {
                            value = list.SelectedItem.ToString();
                        }
                        // Close the list control after selection.
                        service1.CloseDropDown();
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the editing style of the <see cref="EditValue"/> method.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <returns>Returns the DropDown style, since this editor uses a drop down list.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // We're using a drop down style UITypeEditor.
            return UITypeEditorEditStyle.DropDown;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyLookupStorePathAttribute : Attribute
    {
        public string nameSection { get; set; }
        public string nameKey { get; set; }

        public PropertyLookupStorePathAttribute(string _nameSection, string _nameKey)
        {
            nameSection = _nameSection;
            nameKey = _nameKey;
        }
    }

    /// <summary>
    /// интерфейсом помечается класс, если его
    /// поля поддерживают подстановку
    /// </summary>
    public interface IPropertyLookupSupported
    { }
}
