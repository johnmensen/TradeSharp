using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    /// <summary>
    /// стрелочка и текст коментария
    /// </summary>
    public partial class ChartComment : IChartInteractiveObject, ICustomEditDialogChartObject
    {
        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleComment"); } }
        
        private static int nextCommentNumber = 1;

        /// <summary>
        /// точка на графике, куда указывает стрелка (цена)
        /// </summary>
        [LocalizedDisplayName("TitlePivotPrice")]
        [LocalizedCategory("TitleMain")]
        [Browsable(false)]
        public double PivotPrice { get; set; }

        /// <summary>
        /// точка на графике, куда указывает стрелка (индекс)
        /// </summary>
        [LocalizedDisplayName("TitlePivotIndex")]
        [LocalizedCategory("TitleMain")]
        [Browsable(false)]
        public double PivotIndex { get; set; }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }

        private string userSetText;
        /// <summary>
        /// текст коментария
        /// </summary>
        [LocalizedDisplayName("TitleText")]
        [LocalizedCategory("TitleMain")]        
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Text
        {
            get { return string.IsNullOrEmpty(userSetText) ? patternSetText : userSetText; }
            set { userSetText = value; }
        }

        private string patternSetText;
        /// <summary>
        /// Альтернативный способ задания текста
        /// </summary>
        [TypeConverter(typeof(CommentaryPatternsList))]
        [LocalizedDisplayName("TitleTextTemplate")]
        [LocalizedCategory("TitleMain")]
        [Browsable(true)]
        public string TextCustom
        {
            get
            {
                return patternSetText;
            }
            set
            {
                patternSetText = value;
            }
        }

        /// <summary>
        /// длина стрелки в пикселях
        /// </summary>
        [LocalizedDisplayName("TitleArrowLength")]
        [LocalizedCategory("TitleVisuals")]
        [Browsable(false)]
        public int ArrowLength { get; set; }

        private double arrowAngle;
        /// <summary>
        /// угол поворота стрелки от точки привязки (grad)
        /// </summary>
        [LocalizedDisplayName("TitleArrowAngle")]
        [LocalizedCategory("TitleVisuals")]
        [Browsable(false)]
        public double ArrowAngle
        {
            get { return arrowAngle; }
            set
            {
                if (value > 360) value -= 360;
                else                
                    if (value < 0) value += 360;
                arrowAngle = value;
            }
        }

        private Color color = Color.Black;
        [LocalizedDisplayName("TitleStrokeColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        [LocalizedDisplayName("TitleHideArrow")]
        [LocalizedCategory("TitleVisuals")]
        public bool HideArrow { get; set; }

        private bool drawFrame = true;
        [LocalizedDisplayName("TitleDrawStroke")]
        [LocalizedCategory("TitleVisuals")]
        public bool DrawFrame
        {
            get { return drawFrame; }
            set { drawFrame = value; }
        }

        [LocalizedDisplayName("TitleHideSubstrate")]
        [LocalizedCategory("TitleVisuals")]
        public bool HideBox { get; set; }

        private int fillTransparency = 32;
        [LocalizedDisplayName("TitleTransparency")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageTransparencyDescription")]
        public int FillTransparency
        {
            get { return fillTransparency; }
            set { fillTransparency = value; }
        }

        private Color colorFill = Color.White;
        [LocalizedDisplayName("TitleFillingColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorFill
        {
            get { return colorFill; }
            set { colorFill = value; }
        }

        private Color colorText = Color.Black;
        [LocalizedDisplayName("TitleTextColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorText
        {
            get { return colorText; }
            set { colorText = value; }
        }

        /// <summary>
        /// вместо строки ... выводится соотв. символ
        /// </summary>
        public const string TemplateBuy = "{BUY}";
        /// <summary>
        /// вместо строки ... выводится соотв. символ
        /// </summary>
        public const string TemplateSell = "{SELL}";

        public const int SymbolBuySellWidth = 28;
        public const int SymbolBuySellHeight = 28;

        public bool IsBeingCreated { get; set; }

        /// <summary>
        /// область, занимаемая текстом (расчитывается только при отрисовке)
        /// нужна для определения попадания
        /// </summary>
        public Rectangle TextArea { get; set; }

        /// <summary>
        /// маркеры
        /// </summary>
        private readonly ChartObjectMarker[] markers = new[]
                                                           {
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Move},
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Resize}
                                                           };

        public static void Copy(ChartComment dest, ChartComment src)
        {
            dest.ArrowAngle = src.ArrowAngle;
            dest.ArrowLength = src.ArrowLength;
            dest.Color = src.Color;
            dest.ColorFill = src.ColorFill;
            dest.ColorText = src.ColorText;
            dest.DrawFrame = src.DrawFrame;
            dest.FillTransparency = src.FillTransparency;
            dest.HideArrow = src.HideArrow;
            dest.HideBox = src.HideBox;
            dest.Magic = src.Magic;
            dest.Name = src.Name;
            dest.Owner = src.Owner;
            dest.PivotIndex = src.PivotIndex;
            dest.PivotPrice = src.PivotPrice;
            dest.Selected = src.Selected;
            dest.Text = src.Text;
            dest.TextArea = src.TextArea;            
        }

        public override string ToString()
        {
            return Magic == 0
                       ? (string.IsNullOrEmpty(Text) ? "<empty>" : Text)
                       : (string.IsNullOrEmpty(Text) ? "<empty>, magic=" + Magic : Text + ", magic=" + Magic);
        }

        public bool IsDotInTextArea(int x, int y)
        {
            return TextArea.Contains(x, y);
        }

        #region IChartInteractiveObject

        private bool selected;
        
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value && !selected) SetupMarkers();
                selected = value;
            }
        }

        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        public string Name
        {
            get;
            set;
        }

        public DateTime? DateStart
        {
            get { return null; }
            set { }
        }

        public int IndexStart
        {
            get { return (int)PivotIndex; }
        }

        #region Цветовая схема
        private static readonly Color[] fillColors = new[] { Color.BurlyWood, 
            Color.DeepSkyBlue, Color.LightSalmon, Color.YellowGreen};
        private static readonly Color[] lineColors = new[] { Color.SaddleBrown, 
            Color.MediumBlue, Color.Brown, Color.DarkOliveGreen};

        private static int nextDefaultFillColor;
        private static int nextDefaultLineColor;
        #endregion

        public ChartComment()
        {
            Name = string.Format("{0} {1}", ClassName, nextCommentNumber++);
            colorFill = fillColors[nextDefaultFillColor++];
            color = lineColors[nextDefaultLineColor++];
            if (nextDefaultFillColor == fillColors.Length) nextDefaultFillColor = 0;
            if (nextDefaultLineColor == lineColors.Length) nextDefaultLineColor = 0;
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("Comment"));
            var attrName = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Name"));
            attrName.Value = Name;
            var attrText = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Text"));
            attrText.Value = Text;
            var attrColor = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Color"));
            attrColor.Value = color.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ColorText")).Value =
                ColorText.ToArgb().ToString();

            var attrPivotPrice = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("PivotPrice"));
            attrPivotPrice.Value = PivotPrice.ToString(CultureProvider.Common);


            var attrPivotTime = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("PivotTime"));
            attrPivotTime.Value = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)Math.Round(PivotIndex)).ToString(
                "ddMMyyyy HHmmss", CultureProvider.Common);
            var attrArrowLength = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ArrowLength"));
            attrArrowLength.Value = ArrowLength.ToString();
            var attrArrowAngle = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ArrowAngle"));
            attrArrowAngle.Value = ArrowAngle.ToString(CultureProvider.Common);

            var attrHideArrow = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("HideArrow"));
            attrHideArrow.Value = HideArrow.ToString();

            var attrHideBox = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("HideBox"));
            attrHideBox.Value = HideBox.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("DrawFrame")).Value = DrawFrame.ToString();

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ColorFill")).Value = ColorFill.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("FillTransparency")).Value = FillTransparency.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("TextCustom")).Value = TextCustom;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["Name"] != null)
                Name = itemNode.Attributes["Name"].Value;
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            if (itemNode.Attributes["Text"] != null)
                Text = itemNode.Attributes["Text"].Value;
            if (itemNode.Attributes["TextCustom"] != null)
                TextCustom = itemNode.Attributes["TextCustom"].Value;
            if (itemNode.Attributes["Color"] != null)
                color = Color.FromArgb(int.Parse(itemNode.Attributes["Color"].Value));
            if (itemNode.Attributes["ColorText"] != null)
                ColorText = Color.FromArgb(int.Parse(itemNode.Attributes["ColorText"].Value));

            if (itemNode.Attributes["PivotPrice"] != null)
                PivotPrice = double.Parse(itemNode.Attributes["PivotPrice"].Value, CultureProvider.Common);
            if (itemNode.Attributes["PivotTime"] != null)
            {
                var timeStr = itemNode.Attributes["PivotTime"].Value;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                PivotIndex = index;
            }
            if (itemNode.Attributes["ArrowLength"] != null)
                ArrowLength = int.Parse(itemNode.Attributes["ArrowLength"].Value);
            if (itemNode.Attributes["ArrowAngle"] != null)
                ArrowAngle = double.Parse(itemNode.Attributes["ArrowAngle"].Value, CultureProvider.Common);

            if (itemNode.Attributes["HideArrow"] != null)
                HideArrow = bool.Parse(itemNode.Attributes["HideArrow"].Value);

            if (itemNode.Attributes["HideBox"] != null)
                HideBox = bool.Parse(itemNode.Attributes["HideBox"].Value);

            if (itemNode.Attributes["DrawFrame"] != null)
                DrawFrame = bool.Parse(itemNode.Attributes["DrawFrame"].Value);
            
            if (itemNode.Attributes["ColorFill"] != null)
                ColorFill = Color.FromArgb(int.Parse(itemNode.Attributes["ColorFill"].Value));

            if (itemNode.Attributes["FillTransparency"] != null)
                FillTransparency = int.Parse(itemNode.Attributes["FillTransparency"].Value);
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            color = ChartControl.ChartVisualSettings.AdjustColor(color, clBack);
            colorText = ChartControl.ChartVisualSettings.AdjustColor(colorText, clBack);
        }

        public Image CreateSample(Size sizeHint)
        {
            var series = Owner as SeriesComment;
            if (series == null)
                return null;
            var oldArrowLength = ArrowLength;
            var oldArrowAngle = ArrowAngle;
            ArrowLength = 20;
            ArrowAngle = 45 * Math.PI / 180;
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var fonts = new FontStorage(new Font(FontFamily.GenericSansSerif, 8));
            var size = MeasureCommentBlock(Graphics.FromImage(result), fonts, string.IsNullOrEmpty(Text) ? "" : Text);
            if (!HideArrow)
            {
                size.Width += (float) (ArrowLength * Math.Cos(ArrowAngle));
                size.Height += (float) (ArrowLength * Math.Sin(ArrowAngle));
            }
            if (size.Width < sizeHint.Width)
                size.Width = sizeHint.Width;
            if (size.Height < sizeHint.Height)
                size.Height = sizeHint.Height;
            var canvasRect = new Rectangle(new Point(0, 0), size.ToSize());
            result = new Bitmap(canvasRect.Width, canvasRect.Height);
            DrawComment(Graphics.FromImage(result), fonts, new RectangleD(PivotIndex, PivotPrice, 0, 0),
                               canvasRect,
                               new PenStorage(), new BrushesStorage(), null);
            ArrowLength = oldArrowLength;
            ArrowAngle = oldArrowAngle;
            return result;
        }

        #endregion

        #region ICustomEditDialogChartObject
        public DialogResult ShowEditDialog()
        {
            var dlg = new ChartCommentEditDialog(this, Owner.Owner.Owner.Owner);
            return dlg.ShowDialog();
        }
        #endregion

        #region Маркеры
        public ChartObjectMarker IsInMarker(int scrX, int scrY, Keys modifierKeys)
        {
            var ptClient = Owner.Owner.PointToClient(new Point(scrX, scrY));
            return markers.FirstOrDefault(marker => marker.IsIn(ptClient.X, ptClient.Y,
                Owner.Owner.WorldRect, Owner.Owner.CanvasRect));
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {
            // пересчитать координаты маркера
            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);

            if (marker.action == ChartObjectMarker.MarkerAction.Move)
            {
                // изменилась точка привязки к свечкам
                PivotPrice = marker.centerModel.Y;
                PivotIndex = marker.centerModel.X;
                CalculateTextMarkerCoords();
                return;
            }
            // изменилось расположение коментария
            markers[0].CalculateScreenCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            var vx = marker.centerScreen.X - markers[0].centerScreen.X;
            var vy = marker.centerScreen.Y - markers[0].centerScreen.Y;
            var len = Math.Sqrt(vx*vx + vy*vy);
            ArrowLength = (int) len;
            ArrowAngle = Math.Atan2(vy, vx) * 180 / Math.PI;
        }

        private void SetupMarkers()
        {
            // точка привязки к цене
            markers[0].centerModel = new PointD(PivotIndex, PivotPrice);
            
            // определить экранные ... 
            markers[0].CalculateScreenCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            CalculateTextMarkerCoords();
        }

        private void CalculateTextMarkerCoords()
        {
            var pivotScreen = markers[0].centerScreen;
            var textScreen = new PointD(
                pivotScreen.X + ArrowLength * Math.Cos(ArrowAngle * Math.PI / 180.0),
                pivotScreen.Y + ArrowLength * Math.Sin(ArrowAngle * Math.PI / 180.0));
            markers[1].centerScreen = textScreen;
            // ... и модельные координаты маркера привязки коментария
            markers[1].RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
        }

        public void DrawMarkers(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var penStorage = new PenStorage())
            using (var brushStorage = new BrushesStorage())
            foreach (var marker in markers)
            {
                marker.CalculateScreenCoords(worldRect, canvasRect);
                marker.Draw(g, penStorage, brushStorage);
            }
        }
        #endregion        
    }
}
