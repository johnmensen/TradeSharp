using System;
using System.Collections.Generic;
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
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.Asteriks, "TitleMarks", ToolButtonImageIndex.Asterisk)]
    [LocalizedSeriesToolButtonParam("Text", typeof(string), "e")]
    [LocalizedSeriesToolButtonParam("Stroke", typeof(Color), DefaultValueString = "-16777216")]
    [LocalizedSeriesToolButtonParam("Filling", typeof(Color), DefaultValueString = "-1")]
    [LocalizedSeriesToolButtonParam("Text_Shift", typeof(string), "e")]
    [LocalizedSeriesToolButtonParam("Stroke_Shift", typeof(Color), DefaultValueString = "-16777216")]
    [LocalizedSeriesToolButtonParam("Filling_Shift", typeof(Color), DefaultValueString = "-65536")]
    [LocalizedSeriesToolButtonParam("Transparency", typeof(int), DefaultValue = 192)]
    [LocalizedSeriesToolButtonParam("Type", typeof(AsteriskTooltip.ShapeType), DefaultValue = AsteriskTooltip.ShapeType.СтрелкаВверх)]
    [LocalizedSeriesToolButtonParam("Type_Shift", typeof(AsteriskTooltip.ShapeType), DefaultValue = AsteriskTooltip.ShapeType.СтрелкаВниз)]
    public class SeriesAsteriks : InteractiveObjectSeries
    {
        public readonly List<AsteriskTooltip> data = new List<AsteriskTooltip>();

        public override int DataCount { get { return data.Count; } }

        public const int DefaultMouseTolerance = 5;

        private static float fontSize = 8;
        [PropertyXMLTag("FontSize", "8")]
        [LocalizedDisplayName("TitleFontSize")]
        public static float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        public SeriesAsteriks(string name)
            : base(name, CandleChartControl.ChartTool.Asteriks)
        {
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            return false;
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            var indexMin = Chart.StockPane.WorldRect.Left - 1;
            var indexMax = Chart.StockPane.WorldRect.Right + 1;

            using (var font = new Font(Chart.Font.FontFamily, fontSize))
            using (var penStorage = new PenStorage())
            using (var brushStorage = new BrushesStorage())
            {
                foreach (var tooltip in data)
                {
                    var index = tooltip.IndexStart;
                    if (index < indexMin || index > indexMax) continue;
                    tooltip.Draw(font, g, worldRect, canvasRect, penStorage, brushStorage);
                }
            }
        }

        public override void OnTimeframeChanged()
        {
            foreach (var marker in data)
            {
                if (marker.DateStart.HasValue)
                    marker.CandleIndex = Chart.StockSeries.GetIndexByCandleOpen(marker.DateStart.Value);
            }
        }

        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            // по одному клику добавить маркер, по двум - открыть
            // редактор
            
            // получить время и цену
            var clientPoint = Chart.PointToScreen(new Point(e.X, e.Y));            
            clientPoint = Chart.StockPane.PointToClient(clientPoint);
            var x = clientPoint.X;
            var y = clientPoint.Y;

            var markers = data.Where(aster => aster.IsIn(
                x - Chart.StockPane.CanvasRect.Left, y)).ToList();
            if (markers.Count > 0)
            {
                var selected = markers[markers.Count - 1];
                // показать текст маркера во всплывающем окне
                var markerDlg = new MarkerWindow {Text = selected.Text};
                var result = markerDlg.ShowDialog();

                if (markerDlg.IsDeleteBtnPressed)
                    data.Remove(selected);
                else
                    if (result == DialogResult.OK) 
                        selected.Text = markerDlg.Text;                
                return;
            }

            var pointD = Conversion.ScreenToWorld(new PointD(x, y),
               Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);

            // поставить маркер и открыть диалог редактирования свойств
            var isShiftPressed = (modifierKeys & Keys.Shift) == Keys.Shift;

            var text = SeriesEditParameter.TryGetParamValue(parameters, "Text", "e");
            var lineColor = SeriesEditParameter.TryGetParamValue(parameters, "Stroke", Color.Black);
            var fillColor = SeriesEditParameter.TryGetParamValue(parameters, "Filling", Color.White);
            var textShift = SeriesEditParameter.TryGetParamValue(parameters, "Text_Shift", "e");
            var lineColorShift = SeriesEditParameter.TryGetParamValue(parameters, "Stroke_Shift", Color.Black);
            var fillColorShift = SeriesEditParameter.TryGetParamValue(parameters, "Filling_Shift", Color.White);
            var shape = SeriesEditParameter.TryGetParamValue(parameters, "Type", AsteriskTooltip.ShapeType.СтрелкаВверх);
            var shapeShift = SeriesEditParameter.TryGetParamValue(parameters, "Type_Shift", AsteriskTooltip.ShapeType.СтрелкаВниз);
            var alphaColor = SeriesEditParameter.TryGetParamValue(parameters, "Transparency", 192);

            var marker = new AsteriskTooltip
                {
                    Price = (float) pointD.Y,
                    CandleIndex = (int) (Math.Round(pointD.X)),
                    Shape = isShiftPressed ? shapeShift : shape,
                    Transparency = alphaColor,
                    ColorFill = isShiftPressed ? fillColorShift : fillColor,
                    ColorLine = isShiftPressed ? lineColorShift : lineColor,
                    Sign = isShiftPressed ? textShift : text,
                    Owner = this
                };
            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                marker.AjustColorScheme(Owner.Owner.Owner);

            marker.DateStart = Chart.StockSeries.GetCandleOpenTimeByIndex(marker.CandleIndex);
            data.Add(marker);
            //var dlg = new ObjectPropertyWindow(new object[] { marker });
            Chart.toolSkipMouseDown = true;
            //dlg.ShowDialog();   
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            interObjects.AddRange(data);
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject is AsteriskTooltip == false) return;
            try
            {
                data.Remove((AsteriskTooltip)interObject);
            }
            catch (Exception ex)
            {
                Logger.Error("RemoveObjectFromList error:", ex);
            }
        }

        public override void RemoveObjectByNum(int num)
        {
            data.RemoveAt(num);
        }

        public override IChartInteractiveObject GetObjectByNum(int num)
        {
            return data[num];
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new AsteriskTooltip();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            // получить время и цену
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var list = new List<IChartInteractiveObject>();
            foreach (var aster in data)
            {
                if (aster.IsIn(ptClient.X, ptClient.Y)) list.Add(aster);
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner){}

        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var ast in data)
                ast.AjustColorScheme(chart);            
        }

        public override IChartInteractiveObject FindObject(Func<IChartInteractiveObject, bool> predicate, out int objIndex)
        {
            objIndex = -1;
            for (var i = 0; i < data.Count; i++)
            {
                if (predicate(data[i]))
                {
                    objIndex = i;
                    return data[i];
                }
            }
            return null;
        }
    }

    // ReSharper disable LocalizableElement
    public class AsteriskTooltip : IChartInteractiveObject
    {
        private static int nextAsterksNumber = 1;

        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        private int candleIndex;
        public int CandleIndex
        {
            get { return candleIndex; }
            set
            {
                candleIndex = value;
                markers[0].centerModel.X = value;
            }
        }

        private float price;
        public float Price
        {
            get { return price; }
            set
            {
                price = value;
                markers[0].centerModel.Y = value;
            }
        }

        private string sign = "!";

        [LocalizedDisplayName("TitleIcon")]
        [LocalizedDescription("MessageIconDescription")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public string Sign
        {
            get { return sign; }
            set { sign = value; }
        }

        [LocalizedDisplayName("TitleText")]
        [LocalizedDescription("MessageTextDescription")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Text { get; set; }

        private Color colorLine = Color.DarkGray;
        [LocalizedDisplayName("TitleStrokeColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorLine
        {
            get { return colorLine; }
            set { colorLine = value; }
        }

        private Color colorFill = Color.White;
        [LocalizedDisplayName("TitleFillingColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorFill
        {
            get { return colorFill; }
            set { colorFill = value; }
        }

        private int transparency = 192;
        [LocalizedDisplayName("TitleTransparencyAndOpaque")]
        [LocalizedDescription("MessageTransparencyAndOpaqueDescription")]
        [Editor(typeof(TransparencyUITypeWEditor), typeof(UITypeEditor))]
        [LocalizedCategory("TitleVisuals")]
        public int Transparency
        {
            get { return transparency; }
            set { transparency = value; }
        }

        private int transparencyText = 192;
        [LocalizedDisplayName("TitleTextTransparencyAndOpaque")]
        [LocalizedDescription("MessageTextTransparencyAndOpaqueDescription")]
        [Editor(typeof(TransparencyUITypeWEditor), typeof(UITypeEditor))]
        [LocalizedCategory("TitleVisuals")]
        public int TransparencyText
        {
            get { return transparencyText; }
            set { transparencyText = value; }
        }

        private Color colorText = Color.Black;
        [LocalizedDisplayName("TitleTextColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorText
        {
            get { return colorText; }
            set { colorText = value; }
        }

        public enum ShapeType
        {
            Круг = 0, Гайка, Квадрат, Звезда,
            СтрелкаВверх, СтрелкаВниз, ГалкаВверх, ГалкаВниз,
            КрестВверх, КрестВниз
        }

        #region Shapes

        private static readonly PointF[] shapePointsNut =
            new []
                {
                    new PointF(49, -22), new PointF(22, -49), new PointF(-22, -49), new PointF(-49, -22),
                    new PointF(-49, 22), new PointF(-22, 49), new PointF(22, 49), new PointF(49, 22)
                };

        private const int ShapePointsNutRad = 55;

        private static readonly PointF[] shapePointsStar =
            new[]
                {
                    new PointF(49, 0), new PointF(37, -12), new PointF(49, -49), new PointF(12, -37), new PointF(0, -49),
                    new PointF(-12, -37), new PointF(-49, -49), new PointF(-37, -12), new PointF(-49, 0),
                    new PointF(-37, 12), new PointF(-49, 49), new PointF(-12, 37), new PointF(0, 49),
                    new PointF(12, 37), new PointF(49, 49), new PointF(37, 12)
                };

        private const int ShapePointsStarRad = 55;

        private static readonly PointF[] shapePointsArrow =
            new[]
                {
                    new PointF(-24, 0), new PointF(0, -24), new PointF(24, 0), new PointF(10, 0), new PointF(10, 30),
                    new PointF(-10, 30), new PointF(-10, 0)
                };

        private const int ShapePointsArrowRad = 28;

        private static readonly PointF[] shapePointsTip =
            new[]
                {
                    new PointF(-10, 4), new PointF(0, -6), new PointF(10, 4), new PointF(0, 0)
                };

        private const int ShapePointsCrossArrow = 6;

        private static readonly PointF[] shapePointsCrossArrow =
            new[]
                {
                    new PointF(-4, -4), new PointF(4, -4), 
                    new PointF(4, 4), new PointF(0, 7), new PointF(-4, 4)
                };

        private const int ShapePointsTipRad = 10;

        private static readonly Dictionary<ShapeType, int> figureSize =
            new Dictionary<ShapeType, int>
                {
                    { ShapeType.Круг, ShapePointsNutRad },
                    { ShapeType.Гайка, ShapePointsNutRad },
                    { ShapeType.Квадрат, ShapePointsNutRad },
                    { ShapeType.Звезда, ShapePointsStarRad },
                    { ShapeType.СтрелкаВверх, ShapePointsArrowRad },
                    { ShapeType.СтрелкаВниз, ShapePointsArrowRad },
                    { ShapeType.ГалкаВверх, ShapePointsTipRad },
                    { ShapeType.ГалкаВниз, ShapePointsTipRad },
                    { ShapeType.КрестВниз, ShapePointsCrossArrow },
                    { ShapeType.КрестВверх, ShapePointsCrossArrow }
                };

        private static readonly Dictionary<ShapeType, PointF[]> figureShape =
            new Dictionary<ShapeType, PointF[]>
                {
                    { ShapeType.Круг, null },
                    { ShapeType.Гайка, shapePointsNut },
                    { ShapeType.Квадрат, null },
                    { ShapeType.Звезда, shapePointsStar },
                    { ShapeType.СтрелкаВверх, shapePointsArrow },
                    { ShapeType.СтрелкаВниз, shapePointsArrow },
                    { ShapeType.ГалкаВверх, shapePointsTip },
                    { ShapeType.ГалкаВниз, shapePointsTip },
                    { ShapeType.КрестВниз, shapePointsCrossArrow },
                    { ShapeType.КрестВверх, shapePointsCrossArrow }
                };
        #endregion

        [LocalizedDisplayName("TitleForm")]
        [LocalizedCategory("TitleVisuals")]
        public ShapeType Shape { get; set; }

        private int radius = 8;
        [LocalizedDisplayName("TitleRadiusOrSize")]
        [LocalizedCategory("TitleVisuals")]
        public int Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        public int Magic { get; set; }

        /// <summary>
        /// экранные координаты центра, считаются при отрисовке,
        /// нужны для проверки попадания
        /// </summary>
        private double screenX, screenY;

        /// <summary>
        /// маркеры
        /// </summary>
        private readonly ChartObjectMarker[] markers = new[]
                                                           {
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Move}
                                                           };

        public AsteriskTooltip()
        {
            Name = string.Format("{0} {1}", ClassName, nextAsterksNumber++);
        }

        public AsteriskTooltip(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public void Draw(Font font, Graphics g, RectangleD worldRect, Rectangle canvasRect,
            PenStorage penStorage, BrushesStorage brushStorage)
        {
            var strFormat = new StringFormat { LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center };

            var pen = penStorage.GetPen(Color.FromArgb(Transparency, ColorLine), Selected ? 3f : 1f);
            var brush = brushStorage.GetBrush(Color.FromArgb(Transparency, ColorFill));
            var brushText = brushStorage.GetBrush(Color.FromArgb(Transparency, ColorText));
            
            var ptCenter = Conversion.WorldToScreen(new PointD(CandleIndex, Price),
                                                            worldRect, canvasRect);
            screenX = ptCenter.X;
            screenY = ptCenter.Y;

            DrawShape(g, brush, ptCenter, pen);
            
            // значок
            g.DrawString(Sign, font, brushText, (float)ptCenter.X, (float)ptCenter.Y, strFormat);

            // маркеры
            if (Selected)
                foreach (var marker in markers)
                {
                    marker.CalculateScreenCoords(worldRect, canvasRect);
                    marker.Draw(g, penStorage, brushStorage);
                }                        
        }

        public bool IsIn(int x, int y)
        {
            var dX = Math.Abs(x - screenX);
            var dY = Math.Abs(y - screenY);
            return dX <= radius && dY <= radius;
        }

        private void DrawShape(Graphics g, Brush brush, PointD ptCenter, Pen pen)
        {            
            if (Shape == ShapeType.Квадрат)
            {
                g.FillRectangle(brush, (float)ptCenter.X - radius, (float)ptCenter.Y - radius,
                    radius * 2, radius * 2);
                g.DrawRectangle(pen, (float)ptCenter.X - radius, (float)ptCenter.Y - radius,
                    radius * 2, radius * 2);                
                return;
            }
            if (Shape == ShapeType.Круг)
            {
                g.FillEllipse(brush, (float)ptCenter.X - radius, (float)ptCenter.Y - radius,
                            radius * 2, radius * 2);
                g.DrawEllipse(pen, (float)ptCenter.X - radius, (float)ptCenter.Y - radius,
                                radius * 2, radius * 2);
                return;
            }

            // рисовать фигуру по точкам
            var pointsSrc = figureShape[Shape];
            var srcSize = figureSize[Shape];
            var scaleY = (Shape == ShapeType.СтрелкаВниз || Shape == ShapeType.ГалкаВниз || Shape == ShapeType.КрестВверх) ? -1 : 1;
                
            var points = new PointF[pointsSrc.Length];                                
            for (var i = 0; i < pointsSrc.Length; i++)
            {
                var x = ptCenter.X + pointsSrc[i].X * radius / srcSize;
                var y = ptCenter.Y + pointsSrc[i].Y * scaleY * radius / srcSize;
                points[i] = new PointF((float)x, (float)y);
            }
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }

        #region IChartInteractiveObject

        [Browsable(false)]
        public string ClassName
        {
            get { return Localizer.GetString("TitleHint"); }
        }

        public bool Selected { get; set; }

        [LocalizedDisplayName("TitleName")]
        [LocalizedDescription("MessageNameDescription")]
        [LocalizedCategory("TitleMain")]
        public string Name { get; set; }

        public DateTime? DateStart { get; set; }

        public int IndexStart
        {
            get { return CandleIndex; }
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("Asterisk"));
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Name")).Value = Name;
            
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Text")).Value = Text;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Sign")).Value = Sign;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();
            
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ColorLine")).Value = ColorLine.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ColorFill")).Value = ColorFill.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("ColorText")).Value = ColorText.ToArgb().ToString();

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Price")).Value = Price.ToString(CultureProvider.Common);
            if (DateStart.HasValue)
                node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("PivotTime")).Value = 
                    DateStart.Value.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Shape")).Value = Shape.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Transparency")).Value =
                Transparency.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("TransparencyText")).Value =
                TransparencyText.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Radius")).Value =
                Radius.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["Name"] != null)
                Name = itemNode.Attributes["Name"].Value;
            if (itemNode.Attributes["Sign"] != null)
                Sign = itemNode.Attributes["Sign"].Value;
            if (itemNode.Attributes["Text"] != null)
                Text = itemNode.Attributes["Text"].Value;

            if (itemNode.Attributes["ColorLine"] != null)
                ColorLine = Color.FromArgb(int.Parse(itemNode.Attributes["ColorLine"].Value));
            if (itemNode.Attributes["ColorFill"] != null)
                ColorFill = Color.FromArgb(int.Parse(itemNode.Attributes["ColorFill"].Value));
            if (itemNode.Attributes["ColorText"] != null)
                ColorText = Color.FromArgb(int.Parse(itemNode.Attributes["ColorText"].Value));

            if (itemNode.Attributes["Price"] != null)
                Price = itemNode.Attributes["Price"].Value.ToFloatUniform();
            
            if (itemNode.Attributes["magic"] != null) 
                Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;

            if (itemNode.Attributes["PivotTime"] != null)
            {
                var timeStr = itemNode.Attributes["PivotTime"].Value;
                DateStart = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(DateStart.Value);
                CandleIndex = index;
            }
            if (itemNode.Attributes["Transparency"] != null)
                Transparency = int.Parse(itemNode.Attributes["Transparency"].Value);
            if (itemNode.Attributes["TransparencyText"] != null)
                TransparencyText = int.Parse(itemNode.Attributes["TransparencyText"].Value);
            if (itemNode.Attributes["Radius"] != null)
                Radius = int.Parse(itemNode.Attributes["Radius"].Value);
            if (itemNode.Attributes["Shape"] != null)
            {
                try
                {
                    Shape = (ShapeType) Enum.Parse(typeof (ShapeType), itemNode.Attributes["Shape"].Value);
                }
                catch
                {
                    Shape = ShapeType.Круг;
                }                
            }
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            colorLine = ChartControl.ChartVisualSettings.AdjustColor(colorLine, clBack);
            colorText = ChartControl.ChartVisualSettings.AdjustColor(colorText, clBack);
            colorFill = ChartControl.ChartVisualSettings.AdjustColor(colorFill,
                                                                     chart.chart.visualSettings.SeriesForeColor);
        }

        public Image CreateSample(Size sizeHint)
        {
            if (sizeHint.IsEmpty)
                return null;
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var g = Graphics.FromImage(result);
            var brushStorage = new BrushesStorage();
            var penStorage = new PenStorage();
            var pen = penStorage.GetPen(Color.FromArgb(Transparency, ColorLine));
            var brush = brushStorage.GetBrush(Color.FromArgb(Transparency, ColorFill));
            var brushText = brushStorage.GetBrush(Color.FromArgb(Transparency, ColorText));
            var ptCenter = new PointD(sizeHint.Width / 2.0, sizeHint.Height / 2.0);
            DrawShape(g, brush, ptCenter, pen);
            // значок
            var strFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
            g.DrawString(Sign, new Font(FontFamily.GenericSansSerif, 8), brushText, (float) ptCenter.X,
                         (float) ptCenter.Y, strFormat);
            return result;
        }

        #endregion

        #region Маркеры

        public ChartObjectMarker IsInMarker(int scrX, int scrY, Keys modifierKeys)
        {
            if (markers == null || markers.Length == 0 || Owner == null || Owner.Owner == null) return null;
            var ptClient = Owner.Owner.PointToClient(new Point(scrX, scrY));
            return markers[0].IsIn(ptClient.X, ptClient.Y, Owner.Owner.WorldRect,
                Owner.Owner.CanvasRect) ? markers[0] : null;
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {
            // переместить сам объект
            screenX = marker.centerScreen.X;
            screenY = marker.centerScreen.Y;
            // пересчитать центр
            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            candleIndex = (int) marker.centerModel.X;
            DateStart = Owner.Owner.Owner.StockSeries.GetCandleOpenTimeByIndex(candleIndex);
            Price = (float)marker.centerModel.Y;            
        }

        #endregion
    }
    // ReSharper restore LocalizableElement
}
