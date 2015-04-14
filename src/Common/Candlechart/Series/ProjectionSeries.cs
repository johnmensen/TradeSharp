using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    /// <summary>
    /// Вертикальные "расширения" и "коррекции",
    /// строятся по приближенным соотношениям соседних членов 
    /// ряда Фибоначчи
    /// </summary>
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.Projection, "TitleFibonacciProjections", ToolButtonImageIndex.FiboVert)]
    [LocalizedSeriesToolButtonParam("Correction", typeof(bool), false)]
    [LocalizedSeriesToolButtonParam("CorrectionPlusProjections", typeof(bool), false)]
    [LocalizedSeriesToolButtonParam("HideMisses", typeof(bool), true)]
    [LocalizedSeriesToolButtonParam("HideSegment", typeof(bool), true)]
    [LocalizedSeriesToolButtonParam("Labels", typeof(ProjectionPair.MarkerPlacement), ProjectionPair.MarkerPlacement.Слева)]
    [LocalizedSeriesToolButtonParam("ExpansionLevels", typeof(string), "0.618 1 1.618")]
    [LocalizedSeriesToolButtonParam("CorrectionLevels", typeof(string), "0.236 0.382 0.618")]
    [LocalizedSeriesToolButtonParam("LengthInBars", typeof(int), 50)]
    public class ProjectionSeries : InteractiveObjectSeries
    {
        public List<ProjectionPair> data = new List<ProjectionPair>();

        public override int DataCount { get { return data.Count; } }

        [PropertyXMLTag("ExtendYAxis", "false")]
        [DisplayName("Растягивать ось Y")]
        [LocalizedCategory("TitleVisuals")]
        public static bool ExtendYAxis { get; set; }

        private bool drawText = true;
        public bool DrawText
        {
            get { return drawText; }
            set { drawText = value; }
        }

        private static float fontSize = 8;
        [PropertyXMLTag("FontSize", "8")]
        [DisplayName("Размер шрифта")]
        [LocalizedCategory("TitleVisuals")]
        public static float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        [PropertyXMLTag("ExtendXAxis", "false")]
        [DisplayName("Растягивать ось X")]
        [LocalizedCategory("TitleVisuals")]
        public static bool ExtendXAxis { get; set; }

        private int fiboProjectionColorsIndex;
        private readonly Color[] fiboProjectionColors = new[]
                                                   {
                                                       Color.DimGray, Color.Salmon, Color.Coral, Color.Green, Color.Blue,
                                                       Color.Red, Color.DarkOrange
                                                   };

        public ProjectionSeries(string name)
            : base(name, CandleChartControl.ChartTool.Projection)
        {
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            if (!ExtendXAxis || data.Count == 0) return false;
            var changed = false;
            foreach (var pair in data)
            {
                if (pair.points.Count < 2 || pair.HideFarParts) continue;
                var r = pair.points[1].a + pair.ProjectionLength;
                if (r > right)
                {
                    changed = true;
                    right = r;
                }
            }
            return changed;
        }

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            if (data.Count == 0 || !ExtendYAxis) return false;
            bool extends = false;
            foreach (var pair in data)
            {
                if (!pair.Completed) continue;
                if (!pair.IsExtension) continue;
                var pairParts = pair.visibleParts;
                if (pair.HideFarParts) if (pairParts == null || pairParts.Count == 0) continue;                

                if (pair.StartIndex > right || pair.EndIndex < left) continue;

                if (pair.HideFarParts)
                {
                    foreach (var part in pairParts)
                    {
                        if (part.left > right || part.right < left || part.left == part.right)
                            continue;
                        var price = (double) part.price;
                        if (price > top)
                        {
                            top = price;
                            extends = true;
                        }
                        else
                        {
                            if (price < bottom)
                            {
                                bottom = price;
                                extends = true;
                            }
                        }
                    }
                }
                else
                {
                    // просто проверить все цены
                    foreach (var price in pair.pricesProj)
                    {
                        if (price > top)
                        {
                            top = price;
                            extends = true;
                        }
                        else if (price < bottom)
                        {
                            bottom = price;
                            extends = true;
                        }
                    }                    
                }
            }

            return extends;
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            DrawPairs(g, worldRect, canvasRect);
        }

        private void DrawPairs(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var brush = new SolidBrush(Color.White))
            using (var fnt = new Font(Chart.Font.FontFamily, fontSize))
            using (var penStorage = new PenStorage())
            {
                foreach (var pair in data)
                {
                    pair.Draw(g, worldRect, canvasRect, brush, fnt, penStorage, DrawText);
                }
            }
        }

        /// <summary>
        /// добавить точку в пару, определяющую проекцию
        /// либо создать новую пару
        /// либо удалить точку (или всю пару)
        /// </summary>
        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            // получить индекс свечки, в которую было попадание            
            var point = GetCandlePoint(e);
            if (!point.HasValue) return;
            
            // перебрать все пары
            // при попадании в точку пары удалить ее
            for (var i = 0; i < data.Count; i++)
            {
                var p = data[i];
                if (p.IsInPoint(point.Value.a, point.Value.b))
                {
                    data.RemoveAt(i);
                    return;
                }
            }

            var defaultCorrectionObj = SeriesEditParameter.TryGetParamValue(parameters, "Correction");
            var defaultCorrection = defaultCorrectionObj == null ? false : (bool)defaultCorrectionObj;

            var defaultHideErrorsObj = SeriesEditParameter.TryGetParamValue(parameters, "HideMisses");
            var defaultHideErrors = defaultHideErrorsObj == null ? true : (bool)defaultHideErrorsObj;
            var defaultMarkersObj = SeriesEditParameter.TryGetParamValue(parameters, "Labels");
            var defaultMarkers = defaultMarkersObj != null ? (ProjectionPair.MarkerPlacement) defaultMarkersObj :
                ProjectionPair.MarkerPlacement.Слева;
            var levelsProjStr = SeriesEditParameter.TryGetParamValue(parameters, "ExpansionLevels", "0.618 1 1.618");
            var levelsCorrStr = SeriesEditParameter.TryGetParamValue(parameters, "CorrectionLevels", "0.236 0.382 0.618");
            var correctWithProj = SeriesEditParameter.TryGetParamValue(parameters, "CorrectionPlusProjections", false);
            var barsLength = SeriesEditParameter.TryGetParamValue(parameters, "LengthInBars", 60);
                
            // добавить новую пару
            var shiftPressed = ((modifierKeys & Keys.Shift) == Keys.Shift);

            var pair = new ProjectionPair(point.Value.a, point.Value.b)
                            {
                                IsCorrection = defaultCorrection ^ shiftPressed,
                                HideFarParts = defaultHideErrors ^ shiftPressed,
                                IsExtension = defaultCorrection ^ !shiftPressed,
                                Color = fiboProjectionColors[fiboProjectionColorsIndex++],
                                Markers = defaultMarkers,
                                Owner = this,
                                LevelsProj = levelsProjStr,
                                LevelsCorr = levelsCorrStr,
                                HideLine = false,
                                IsBeingCreated = true
                            };
            pair.points.Add(new Cortege2<int, float>(point.Value.a, point.Value.b));
            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                pair.AjustColorScheme(Owner.Owner.Owner);
            if (correctWithProj)
            {
                pair.IsCorrection = true;
                pair.IsExtension = true;
            }
            if (barsLength > 0) 
                pair.ProjectionLength = barsLength;
            data.Add(pair);            
            if (fiboProjectionColorsIndex >= fiboProjectionColors.Length) fiboProjectionColorsIndex = 0;            
        }

        private Cortege2<int, float>? GetCandlePoint(MouseEventArgs e)
        {
            Point clientPoint = Chart.PointToScreen(new Point(e.X, e.Y));
            clientPoint = Chart.StockPane.PointToClient(clientPoint);
            var point = Chart.Owner.GetCandlePointByMouseCoord(clientPoint.X, clientPoint.Y);
            return point;
        }

        protected override bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            var pair = data.FirstOrDefault(p => p.IsBeingCreated);
            if (pair == null) return false;
            // получить ценовую отметку на свече (прилипнуть к одной из цен OHLC
            var point = GetCandlePoint(e);
            if (!point.HasValue) return false;

            // установить цену
            pair.points[pair.points.Count - 1] = point.Value;
            pair.CalculateProjections();
            return true;
        }

        protected override bool OnMouseUp(List<SeriesEditParameter> parameters, MouseEventArgs e, 
            Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            var hideLine = SeriesEditParameter.TryGetParamValue(parameters, "HideSegment", true);
            objectToEdit = null;
            var pair = data.FirstOrDefault(p => p.IsBeingCreated);
            if (pair == null) return false;

            // если проекция выродилась - удалить ее
            if (Math.Abs(pair.points[0].b - pair.points[1].b) < 0.00001f)
            {
                data.Remove(pair);
                return true;
            }

            // завершить создание
            pair.IsBeingCreated = false;
            pair.HideLine = hideLine;
            objectToEdit = pair;
            return true;
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            interObjects.AddRange(data);
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new ProjectionPair();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is ProjectionPair == false) return;
            data.Remove((ProjectionPair)interObject);
        }

        public override void RemoveObjectByNum(int num)
        {
            data.RemoveAt(num);
        }

        public override IChartInteractiveObject GetObjectByNum(int num)
        {
            return data[num];
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var list = new List<IChartInteractiveObject>();
            foreach (var proj in data)
            {
                if (proj.points.Count < 2) continue;
                if (proj.IsIn(ptClient, tolerance))
                    list.Add(proj);
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner)
        {            
        }
    
        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var proj in data)
                proj.AjustColorScheme(chart);            
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
    /// <summary>
    /// Пара точек, из которых строится проекция вероятного роста цены
    /// Может (временно) состоять из одной точки
    /// </summary>    
    public class ProjectionPair : IChartInteractiveObject
    {
        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleVerticalProjectionsShort"); } }
        /// <summary>
        /// для автоматического именования объектов
        /// </summary>
        private static int nextObjectNumber = 1;

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }
        
        private Color color = Color.FromArgb(100, 22, 22, 17);                
        [LocalizedDisplayName("TitleColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color
        {
            get { return color; } set { color = value; }
        }

        private int projectionLength = 60;
        [LocalizedDisplayName("TitleForwardInBars")]
        [LocalizedCategory("TitleMain")]
        public int ProjectionLength
        {
            get { return projectionLength; }
            set { projectionLength = value; }
        }

        private bool hideFarParts = true;
        [LocalizedDisplayName("TitleHideMisses")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageHideMissesDescription")]
        public bool HideFarParts
        {
            get { return hideFarParts; }
            set { hideFarParts = value; }
        }

        private int touchErrorPoints = 20;
        [LocalizedDisplayName("TitleHitErrorsInPoints")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageHitErrorsInPointsDescription")]
        public int TouchErrorPoints
        {
            get { return touchErrorPoints; }
            set { touchErrorPoints = value; }
        }

        [LocalizedDisplayName("TitleHideLine")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageHideLineDescription")]
        public bool HideLine { get; set; }

        public enum MarkerPlacement { Справа = 0, Слева = 1, ДвеМетки = 2}

        [LocalizedDisplayName("TitleMarkers")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageMarkersDescription")]
        public MarkerPlacement Markers { get; set; }

        public bool IsBeingCreated { get; set; }

        /// <summary>
        /// список точек (индекс бара / цена закрытия)
        /// </summary>
        public readonly List<Cortege2<int, float>> points = new List<Cortege2<int, float>>();

        private readonly ChartObjectMarker[] markers = 
            new []
            {
                new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize },
                new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize }
            };

        /// <summary>
        /// горизонтальные отрезки, по краям которых выводится текст
        /// не все уровни будут видимы в общем случае
        /// 
        /// заполняется при отрисовке
        /// </summary>
        public List<ProjectionLinePart> visibleParts;
        
        public float [] pricesProj;
        public float [] pricesCorr;

        public float[] levelsProj = { 0.618f, 1, 1.618f };

        [LocalizedDisplayName("TitleExtensionLevels")]
        [LocalizedCategory("TitleMain")]
        public string LevelsProj
        {
            get { return string.Join(" ", levelsProj.Select(l => l.ToStringUniform())); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                var parts = value.ToFloatArrayUniform();
                if (parts.Length == 0) return;
                levelsProj = parts;
                CalculateProjections();
            }
        }

        public float[] levelsCorr = { 0.236f, 0.382f, 0.5f };

        [LocalizedDisplayName("TitleCorrectionLevels")]
        [LocalizedCategory("TitleMain")]
        public string LevelsCorr
        {
            get { return string.Join(" ", levelsCorr.Select(l => l.ToStringUniform())); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                var parts = value.ToFloatArrayUniform();
                if (parts.Length == 0) return;
                levelsCorr = parts;
                CalculateProjections();
            }
        }

        private bool isCorrection;
        [LocalizedDisplayName("TitleShowCorrectionSmall")]
        [LocalizedCategory("TitleMain")]
        public bool IsCorrection
        {
            get { return isCorrection; }
            set { isCorrection = value; }
        }

        private bool isExtension = true;
        [LocalizedDisplayName("TitleShowExtensionSmall")]
        [LocalizedCategory("TitleMain")]
        public bool IsExtension
        {
            get { return isExtension; }
            set { isExtension = value; }
        }

        public int StartIndex
        {
            get { return points.Count == 2 ? points[1].a : 0; }
        }
        public int EndIndex
        {
            get { return StartIndex + ProjectionLength; }
        }

        public bool Completed { get { return points.Count == 2; } }

        public ProjectionPair()
        {
            Name = string.Format("{0} {1}", ClassName, nextObjectNumber++);
        }

        public ProjectionPair(int index, float price) : this()
        {
            points.Add(new Cortege2<int, float> { a = index, b = price });
        }

        public void AddPoint(int index, float price)
        {
            if (points.Count == 2) return;
            if (points.Count == 0)
                points.Add(new Cortege2<int, float> { a = index, b = price });
            else
            {
                if (points[0].a > index)
                    points.Insert(0, new Cortege2<int, float> { a = index, b = price });
                if (points[0].a < index)
                    points.Add(new Cortege2<int, float> { a = index, b = price });
            }
            CalculateProjections();
        }

        /// <param name="index">индекс бара</param>
        /// <param name="price">цена в точке</param>
        /// <returns>попадание</returns>
        public bool IsInPoint(int index, float price)
        {
            return points.Any(t => t.a == index && t.b == price);
        }

        public bool IsIn(Point p, int tolerance)
        {
            // попадание в начальный отрезок
            if (Geometry.IsDotOnSpan(p, spanBaseScreenCoords.a, spanBaseScreenCoords.b, tolerance)) return true;

            // попадание в отрезочки проекций
            if (spansProjScreenCoords.Any(span => Geometry.IsDotOnSpan(p, span.a, span.b, tolerance)))
                return true;
            
            // попадание в текстовые метки
            return rectanglesTextScreenCoords.Any(rect => rect.Contains(p));
        }

        public void CalculateProjections()
        {
            if (points.Count < 2) return;
            var delta = points[1].b - points[0].b;
            pricesProj = levelsProj.Select(l => 
                points[1].b - delta * (1 + l)).ToArray();
            pricesCorr = levelsCorr.Select(l =>
                points[1].b - delta * l).ToArray();
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (!selected && value)
                    SetupMarkers();
                selected = value;
            }
        }
        
        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        public string Name
        {
            get; set;
        }

        public DateTime? DateStart { get; set; }

        public int IndexStart
        {
            get { return StartIndex; }
        }

        private Cortege2<Point, Point> spanBaseScreenCoords;

        private readonly List<Cortege2<Point, Point>> spansProjScreenCoords = new List<Cortege2<Point, Point>>();

        private readonly List<Rectangle> rectanglesTextScreenCoords = new List<Rectangle>();

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("ProjectionPair"));
            var attrName = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Name"));
            attrName.Value = Name;
            
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("isCorrection")).Value =
                isCorrection.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("isExtension")).Value =
                isExtension.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("hideFarParts")).Value =
                hideFarParts.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("markers")).Value =
                ((int) Markers).ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Color")).Value = color.ToArgb().ToString();
            // расширение - коррекция
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("LevelsProj")).Value = LevelsProj;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("LevelsCorr")).Value = LevelsCorr;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("HideLine")).Value = HideLine.ToString();

            foreach (var point in points)
            {
                var nodePoint = node.AppendChild(parentNode.OwnerDocument.CreateElement("Point"));

                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex(point.a);
                var attrIndex = nodePoint.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Time"));
                attrIndex.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);

                var attrPrice = nodePoint.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Price"));
                attrPrice.Value = point.b.ToString(CultureProvider.Common);
            }
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["Name"] != null)
                Name = itemNode.Attributes["Name"].Value;
            if (itemNode.Attributes["Color"] != null)
                color = Color.FromArgb(int.Parse(itemNode.Attributes["Color"].Value));

            if (itemNode.Attributes["isCorrection"] != null)
                isCorrection = bool.Parse(itemNode.Attributes["isCorrection"].Value);
            if (itemNode.Attributes["isExtension"] != null)
                isExtension = bool.Parse(itemNode.Attributes["isExtension"].Value);
            if (itemNode.Attributes["hideFarParts"] != null)
                hideFarParts = bool.Parse(itemNode.Attributes["hideFarParts"].Value);
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;

            if (itemNode.Attributes["markers"] != null)            
                Markers = (MarkerPlacement)(itemNode.Attributes["hideFarParts"].Value.ToIntSafe() ?? 0);

            // расширение - коррекция
            if (itemNode.Attributes["LevelsProj"] != null) LevelsProj = itemNode.Attributes["LevelsProj"].Value;
            if (itemNode.Attributes["LevelsCorr"] != null) LevelsCorr = itemNode.Attributes["LevelsCorr"].Value;
            if (itemNode.Attributes["HideLine"] != null) HideLine = itemNode.Attributes["HideLine"].Value.ToBoolSafe() ?? false;
            
            foreach (XmlElement pointNode in itemNode.ChildNodes)
            {
                var timeStr = pointNode.Attributes["Time"].Value;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                var price = pointNode.Attributes["Price"].Value.ToFloatUniform();
                points.Add(new Cortege2<int, float> { a = index, b = price });
            }            
            CalculateProjections();
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

        public ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys)
        {
            var ptClient = Owner.Owner.PointToClient(new Point(screenX, screenY));
            return markers.FirstOrDefault(marker => marker.IsIn(ptClient.X, ptClient.Y,
                Owner.Owner.WorldRect, Owner.Owner.CanvasRect));
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {
            var markerIndex = 0;
            for (var i = 0; i < markers.Length; i++)
            {
                if (markers[i] != marker) continue;
                markerIndex = i;
                break;
            }
            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            var pointIndex = points.Count == 2 ? markerIndex : 0;
            points[pointIndex] = new Cortege2<int, float>((int)marker.centerModel.X,
                (float)marker.centerModel.Y);
            // пересчитать отметки Фибоначчи
            CalculateProjections();
        }

        private void SetupMarkers()
        {
            markers[0].centerModel = new PointD(points[0].a, points[0].b);
            var lastPoint = points.Count == 2 ? points[1] : points[0];
            markers[1].centerModel = new PointD(lastPoint.a, lastPoint.b);
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            Color = ChartControl.ChartVisualSettings.AdjustColor(Color, clBack);
        }

        public Image CreateSample(Size sizeHint)
        {
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var minX = points.Select(p => p.a).Min();
            var minY = points.Select(p => p.b).Min();
            var maxX = points.Select(p => p.a).Max();
            var maxY = points.Select(p => p.b).Max();
            var marginX = (maxX - minX) / 10;
            var marginY = (maxY - minY);
            minX -= marginX;
            maxX += marginX;
            minY -= marginY;
            maxY += marginY;
            Draw(Graphics.FromImage(result), new RectangleD(minX, minY, maxX - minX, maxY - minY),
                 new Rectangle(new Point(0, 0), sizeHint), new SolidBrush(Color.White),
                 new Font(FontFamily.GenericSansSerif, 8), new PenStorage(), true);
            return result;
        }

        public void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            Brush brush, Font fnt, PenStorage penStorage, 
            bool doDrawText)
        {
            spansProjScreenCoords.Clear();
            rectanglesTextScreenCoords.Clear();

            var pen = penStorage.GetPen(Color, Selected ? 2f : 1f, DashStyle.Dot);
            var screenPoints = points.Select(point =>
                Conversion.WorldToScreen(new PointD(point.a, point.b), worldRect, canvasRect)).ToList();
            
            // сохранить координаты начального отрезка (для проверки попадания)
            if (screenPoints.Count > 1)
                spanBaseScreenCoords = new Cortege2<Point, Point>(screenPoints[0].ToPoint(), screenPoints[1].ToPoint());

            // соединить
            if (Completed && HideLine == false)
                g.DrawLine(pen, (float)screenPoints[0].X, (float)screenPoints[0].Y,
                            (float)screenPoints[1].X, (float)screenPoints[1].Y);
            // нарисовать кружки
            foreach (var point in screenPoints)
            {
                g.FillEllipse(brush, (float)point.X - 3, (float)point.Y - 3, 6, 6);
                g.DrawEllipse(pen, (float)point.X - 3, (float)point.Y - 3, 6, 6);
            }
            
            // нарисовать проекции
            if (points.Count < 2 || points[0].b == points[1].b || pricesProj == null) return;

            visibleParts = new List<ProjectionLinePart>();
            if (IsExtension)
            {
                foreach (var price in pricesProj)
                {
                    visibleParts.Add(new ProjectionLinePart(StartIndex,
                                                            EndIndex, price));
                }
            }
            if (IsCorrection)
            {
                foreach (var price in pricesCorr)
                {
                    visibleParts.Add(new ProjectionLinePart(StartIndex,
                                                            EndIndex, price));
                }
            }

            // скрыть "лишние" кусочки
            if (HideFarParts) DoHideFarParts(visibleParts);

            foreach (var part in visibleParts)
            {
                var ptScrLeft = Conversion.WorldToScreen(new PointD(part.left,
                                                                part.price), worldRect, canvasRect);
                var ptScrRight = Conversion.WorldToScreen(new PointD(part.right,
                                                                part.price), worldRect, canvasRect);
                part.x1 = (float)ptScrLeft.X;
                part.y1 = (float)ptScrLeft.Y;
                part.x2 = (float)ptScrRight.X;
                part.y2 = (float)ptScrRight.Y;
                g.DrawLine(pen, (float)ptScrLeft.X, (float)ptScrLeft.Y, (float)ptScrRight.X, (float)ptScrRight.Y);
                // сохранить координаты
                spansProjScreenCoords.Add(new Cortege2<Point, Point>(
                    new Point((int)ptScrLeft.X, (int)ptScrLeft.Y),
                    new Point((int)ptScrRight.X, (int)ptScrRight.Y)));
            }

            if (doDrawText)
            {
                using (var fontBrush = new SolidBrush(Color))
                {
                    var strFormat = new StringFormat { LineAlignment = StringAlignment.Center };
                    foreach (var part in visibleParts)
                    {
                        if (part.left == part.right) continue;
                        var priceStr = part.price.ToStringUniformPriceFormat(false);
                        var textSz = g.MeasureString(priceStr, fnt);
                        if (Markers == MarkerPlacement.Слева ||
                            Markers == MarkerPlacement.ДвеМетки)
                        {
                            g.DrawString(priceStr, fnt, fontBrush, part.x1 - textSz.Width - 2, part.y1, strFormat);
                            rectanglesTextScreenCoords.Add(new Rectangle((int)(part.x1 - textSz.Width - 2), (int)(part.y1 - textSz.Height / 2),
                                (int)textSz.Width, (int)textSz.Height));
                        }
                        if (Markers == MarkerPlacement.Справа ||
                            Markers == MarkerPlacement.ДвеМетки)
                        {
                            g.DrawString(priceStr, fnt, fontBrush, part.x2 + 2, part.y2, strFormat);
                            rectanglesTextScreenCoords.Add(new Rectangle((int)(part.x2 + 2), (int)(part.y2 - textSz.Height / 2),
                                (int)textSz.Width, (int)textSz.Height));
                        }
                    }
                }
            }
            if (Selected) 
                DrawMarkers(g, worldRect, canvasRect);
        }

        /// <summary>
        /// оставить только кусочки, касающиеся свечек или соседние с ними
        /// </summary>        
        private void DoHideFarParts(List<ProjectionLinePart> parts)
        {
            if (parts == null || parts.Count == 0) return;
            if (Owner == null || Owner.Owner == null) return;
            var candles = Owner.Owner.StockSeries.Data.Candles;
            if (candles == null) return;

            // - 1 - найти свечки в диапазоне left-right
            var subCandles = new List<Cortege2<CandleData, int>>();
            for (var j = Math.Max(parts[0].left, 0); j <= parts[0].right; j++)
            {
                if (j >= candles.Count) break;
                subCandles.Add(new Cortege2<CandleData, int>(candles[j], j));
            }
            if (subCandles.Count == 0)
            {
                foreach (var part in parts) part.left = part.right;
                return;
            }

            var priceSpeciman = parts[0].price;
            var deltaPriceRough = TouchErrorPoints / (float)(DalSpot.Instance.GetPrecision10(priceSpeciman));
            
            // - 2 - отсечь уровни
            foreach (var part in parts)
            {
                int touchIndex = -1;
                foreach (var candle in subCandles)
                {
                    // проверить точное попадание
                    var touchedTight = candle.a.high >= part.price && candle.a.low <= part.price;
                    if (touchedTight)
                    {
                        touchIndex = candle.b;
                        break;
                    }
                    // проверить грубое попадание
                    var up = candle.a.high + deltaPriceRough;
                    var dn = candle.a.low - deltaPriceRough;
                    var touchedRough = up >= part.price && dn <= part.price;
                    if (touchedRough && touchIndex < 0) touchIndex = candle.b;
                }
                // скорректировать начало-конец
                if (touchIndex == -1) part.left = part.right;
                else
                {
                    part.left = Math.Max(part.left, touchIndex - 1);
                    part.right = Math.Min(part.right, touchIndex + 1);
                }
            }
        }
    }
    // ReSharper restore LocalizableElement

    public class ProjectionLinePart
    {
        public int left, right;
        public float price;
        public float x1, y1, x2, y2;
        public ProjectionLinePart(int left, int right, float price)
        {
            this.left = left;
            this.right = right;
            this.price = price;
        }
    }
}