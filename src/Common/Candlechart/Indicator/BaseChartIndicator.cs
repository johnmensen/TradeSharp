using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [TypeConverter(typeof(PropertySorter))]
    public abstract class BaseChartIndicator
    {
        public ChartControl owner;

        /// <summary>
        /// создать копию индикатора
        /// </summary>        
        public abstract BaseChartIndicator Copy();

        /// <summary>
        /// переданный параметром индикатор получит настройки текущего
        /// </summary>        
        public abstract void Copy(BaseChartIndicator indi);
        
        protected void CopyBaseSettings(BaseChartIndicator indi)
        {
            indi.owner = owner;

            #region Shadow

            indi.DrawShadow = DrawShadow;
            indi.ShadowWidth = ShadowWidth;
            indi.ShadowAlpha = ShadowAlpha;

            #endregion

            #region настройки источников данных

            indi.SeriesSourcesDisplay = SeriesSourcesDisplay;
            if (indi.SeriesSources == null)
                indi.SeriesSources = new List<Series.Series>();
            else 
                indi.SeriesSources.Clear();
            indi.SeriesSources.AddRange(SeriesSources);
            if (indi.SeriesResult == null)
                indi.SeriesResult = new List<Series.Series>();
            else
                indi.SeriesResult.Clear();
            indi.SeriesResult.AddRange(SeriesResult);
            indi.DrawPane = DrawPane;
            indi.DrawPaneDisplay = DrawPaneDisplay;
            indi.CreateOwnPanel = CreateOwnPanel;
            indi.IsPanelVisible = IsPanelVisible;

            #endregion

            #region основные настройки
            indi.UniqueName = UniqueName;
            #endregion
        }

        #region Shadow

        private bool drawShadow;
        [LocalizedDisplayName("TitleShadow")]
        [LocalizedCategory("TitleShadow")]
        [LocalizedDescription("MessageShadowDescription")]
        [PropertyOrder(100)]
        public bool DrawShadow
        {
            get { return drawShadow; }
            set
            {
                drawShadow = value;
                ShadowSettingsChanged();
            }
        }

        private int shadowWidth = 3;
        [LocalizedDisplayName("TitleShadowThickness")]
        [LocalizedCategory("TitleShadow")]
        [LocalizedDescription("MessageShadowThicknessDescription")]
        [PropertyOrder(105)]
        public int ShadowWidth
        {
            get { return shadowWidth; }
            set
            {
                shadowWidth = value;
                ShadowSettingsChanged();
            }
        }

        private int shadowAlpha = 70;
        [LocalizedDisplayName("TitleShadowBrightness")]
        [LocalizedCategory("TitleShadow")]
        [LocalizedDescription("MessageShadowBrightnessDescription")]
        [PropertyOrder(110)]
        [Editor(typeof(TransparencyUITypeWEditor), typeof(UITypeEditor))]
        public int ShadowAlpha
        {
            get { return shadowAlpha; }
            set
            {
                shadowAlpha = value;
                ShadowSettingsChanged();
            }
        }

        #endregion

        #region Настройки источников данных

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedDescription("MeesageSourceSeriesDescription")]
        [LocalizedCategory("TitleMain")]
        [Editor("Candlechart.Indicator.ComboBoxSeriesUITypeEditor, System.Drawing.Design.UITypeEditor", typeof(UITypeEditor))]
        [PropertyOrder(30)]
        public virtual string SeriesSourcesDisplay { get; set; }

        [Browsable(false)]
        public List<Series.Series> SeriesSources { get; set; }

        [Browsable(false)]
        public List<Series.Series> SeriesResult { get; set; }

        /// <summary>
        /// собственная панель, созданная индикатором
        /// </summary>
        public Pane ownPane;
        
        [Browsable(false)]
        public Pane DrawPane { get; set; }

        [LocalizedDisplayName("TitleDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageDrawingPanelDescription")]
        [Editor("Candlechart.Indicator.ComboBoxDrawPaneUITypeEditor, System.Drawing.Design.UITypeEditor", typeof(UITypeEditor))]
        [PropertyOrder(35)]
        public string DrawPaneDisplay { get; set; }

        public string GetFullyQualifiedPaneName()
        {
            return DrawPane == null ? string.Empty : UniqueName + Separators.IndiNameDelimiter[0] + DrawPane.Name;
        }

        private bool createOwnPanel = true;
        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(40)]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public virtual bool CreateOwnPanel { get { return createOwnPanel; } set { createOwnPanel = value; } }

        private bool isPanelVisible = true;
        [LocalizedDisplayName("TitleShowDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageShowDrawingPanelDescription")]
        [PropertyOrder(45)]
        public bool IsPanelVisible { get { return isPanelVisible; } set { isPanelVisible = value; } }

        #endregion

        #region основные настройки

        [Browsable(false)]
        public abstract string Name { get; }

        [LocalizedDisplayName("TitleUniqueName")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageUniqueNameDescription")]
        [PropertyOrder(25)]
        public string UniqueName { get; set; }
        #endregion

        #region Save Load
        public static void MakeIndicatorXMLNode(IChartIndicator indi, XmlNode node)
        {
            var xmlAttr = node.OwnerDocument.CreateAttribute("ClassName");
            xmlAttr.Value = indi.GetType().ToString();
            node.Attributes.Append(xmlAttr);

            foreach (var prop in indi.GetType().GetProperties())
            {
                var attrs = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                var attr = (DisplayNameAttribute)attrs.FirstOrDefault(a => a is DisplayNameAttribute);
                if (attr == null) continue;
                var propVal = prop.GetValue(indi, null);

                // есть свой сериализатор для свойства?
                attrs = prop.GetCustomAttributes(typeof(CustomXmlSerializationAttribute), true);
                if (attrs.Length > 0)
                {
                    var atrSerializer = (CustomXmlSerializationAttribute)attrs[0];
                    atrSerializer.SerializeProperty(propVal, node);
                    continue;
                }

                var attrName = prop.Name;

                var attrVal = propVal == null ? string.Empty
                    : prop.PropertyType == typeof(string) ?
                    string.IsNullOrEmpty((string)propVal) ? "" : (string)propVal
                    : prop.PropertyType == typeof(Color)
                    ? ((Color)propVal).ToArgb().ToString()
                    : string.Format(CultureProvider.Common, "{0}", propVal ?? "null");

                xmlAttr = node.OwnerDocument.CreateAttribute(attrName);
                xmlAttr.Value = attrVal;
                node.Attributes.Append(xmlAttr);
            }
        }

        public static IChartIndicator LoadIndicator(XmlElement node)
        {
            var indiType = Assembly.GetExecutingAssembly().GetType(
                    node.Attributes["ClassName"].Value);
            var defaultCons = indiType.GetConstructor(new Type[0]);
            if (defaultCons == null)
                throw new Exception(string.Format(
                    "Для типа {0} не определен конструктор без аргументов", indiType));
            var indi = (IChartIndicator)defaultCons.Invoke(new object[0]);
            // заполнить свойства объекта
            try
            {
                LoadIndicatorProperties(indi, node);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки свойств индикатора", ex);
                return null;
            }
            return indi;
        }

        private static void LoadIndicatorProperties(IChartIndicator indi, XmlElement node)
        {
            foreach (var prop in indi.GetType().GetProperties())
            {
                var attrs = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                var attr = (DisplayNameAttribute)attrs.FirstOrDefault(a => a is DisplayNameAttribute);
                if (attr == null) continue;

                object val;

                // свойство само умеет себя десериализовать?
                attrs = prop.GetCustomAttributes(typeof(CustomXmlSerializationAttribute), true);
                if (attrs.Length > 0)
                {
                    var atrSerializer = (CustomXmlSerializationAttribute)attrs[0];
                    val = atrSerializer.DeserializeProperty(node);
                    prop.SetValue(indi, val, new object[0]);
                    continue;
                }

                // получить атрибут узла
                var xmlAttr = node.Attributes[prop.Name];
                if (xmlAttr == null) continue;

                // получить значение из строки
                if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                    val = Enum.Parse(prop.PropertyType, xmlAttr.Value);
                else
                {
                    //if (prop.Name == "ClLine") continue;
                    if (prop.PropertyType == typeof(Color))
                        val = Color.FromArgb(int.Parse(xmlAttr.Value));
                    else
                        val = Converter.GetObjectFromString(xmlAttr.Value, prop.PropertyType);
                }
                prop.SetValue(indi, val, new object[0]);
            }
        }   
        #endregion
        /// <summary>
        /// вызывается в методе Add
        /// </summary>
        public void EntitleIndicator()
        {
            if (string.IsNullOrEmpty(UniqueName))
                UniqueName = Name;
            owner.Owner.EnsureUniqueName((IChartIndicator)this);
        }

        /// <summary>
        /// если в процессе редактирования польз. не поменял имя индикатора,
        /// вызывается этот метод. Если метод возвращает непустую строку -
        /// она станет новым UniqueName, возможно, с добавлением суфикса
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateNameBySettings()
        {
            return string.Empty;
        }

        public virtual bool OnMouseButtonDown(MouseEventArgs e, Keys modifierKeys)
        {
            return false;
        }

        public virtual bool OnMouseButtonUp(MouseEventArgs e, Keys modifierKeys)
        {
            return false;
        }

        public virtual bool OnMouseButtonMove(MouseEventArgs e, Keys modifierKeys)
        {
            return false;
        }

        public virtual void ShadowSettingsChanged()
        {
        }

        protected int GetSourceDataCount()
        {
            if (SeriesSources == null) return 0;
            if (SeriesSources.Count < 1) return 0;
            return SeriesSources[0] is StockSeries
                            ? ((StockSeries)SeriesSources[0]).Data.Count
                            : SeriesSources[0] is LineSeries
                                    ? ((LineSeries)SeriesSources[0]).Data.Count : 0;
        }

        protected double GetSourcePrice(int index, int indexSeries)
        {
            if (SeriesSources[indexSeries] is StockSeries)
            {
                var stockSrc = (StockSeries)SeriesSources[indexSeries];
                return index < 0 || index >= stockSrc.Data.Count
                           ? 0 : (double)stockSrc.Data.Candles[index].close;
            }
            if (SeriesSources[indexSeries] is LineSeries)
            {
                var lineSrc = (LineSeries)SeriesSources[indexSeries];
                return index < 0 || index >= lineSrc.Data.Count
                           ? 0 : lineSrc.Data[index];
            }
            return 0;
        }

        protected static bool GetKnownColor(int argbValue, out string strKnownColor)
        {
            var aListofKnownColors = Enum.GetValues(typeof(KnownColor));
            foreach (KnownColor eKnownColor in aListofKnownColors)
            {
                var someColor = Color.FromKnownColor(eKnownColor);
                if (argbValue != someColor.ToArgb() || someColor.IsSystemColor) continue;
                strKnownColor = someColor.Name;
                return true;
            }
            strKnownColor = string.Empty;
            return false;
        }
    }
}
