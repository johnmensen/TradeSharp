using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.Ellipse, "TitleEllipses", ToolButtonImageIndex.Ellipse)]
    [LocalizedSeriesToolButtonParam("ByTwoPoints", typeof(bool), false)]
    [LocalizedSeriesToolButtonParam("Tangent", typeof(bool), false)]
    [LocalizedSeriesToolButtonParam("TangentView", typeof(ChartEllipse.EllipseTangentType), ChartEllipse.EllipseTangentType.Отрезок)]
    [LocalizedSeriesToolButtonParam("Stroke", typeof(Color), DefaultValueString = "-16777216")]
    public class SeriesEllipse : InteractiveObjectSeries
    {
        public List<ChartEllipse> data = new List<ChartEllipse>();

        public override int DataCount { get { return data.Count; } }

        private bool drawText = true;
        public bool DrawText
        {
            get { return drawText; }
            set { drawText = value; }
        }

        private ChartEllipse ellipseBeingSized;

        public SeriesEllipse(string name)
            : base(name, CandleChartControl.ChartTool.Ellipse)
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
            DrawEllipses(g, worldRect, canvasRect);
        }
        
        private void DrawEllipses(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var brushStorage = new BrushesStorage())
            using (var penStorage = new PenStorage())
            foreach (var ellipse in data)
            {
                ellipse.DrawEllipse(g, worldRect, canvasRect, brushStorage, penStorage,Chart.Font);
            }
        }

        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            // получить время и цену
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            var editedEllipse = data.FirstOrDefault(el => el.IsBeingCreated);

            var shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            var controlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            
            if (editedEllipse == null)
            {// начинается редактирование - выбрана первая точка            
                var lineColor = SeriesEditParameter.TryGetParamValue(parameters, "Stroke", Color.Black);
                var buildAuto = SeriesEditParameter.TryGetParamValue(parameters, "ByTwoPoints", false) ^ controlPressed;
                var buildTangent = SeriesEditParameter.TryGetParamValue(parameters, "Tangent", false) ^ shiftPressed;
                var tangentType = SeriesEditParameter.TryGetParamValue(parameters, "TangentView", 
                    ChartEllipse.EllipseTangentType.Отрезок);
                data.Add(new ChartEllipse
                             {
                                 Owner = this,
                                 Color = lineColor,
                                 BuildTangent = buildTangent,
                                 TangentType = tangentType,
                                 buildAuto = buildAuto
                             });
                editedEllipse = data[data.Count - 1];
                editedEllipse.IsBeingCreated = true;
                if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                    editedEllipse.AjustColorScheme(Owner.Owner.Owner);
            }
            
            // добавить точку в эллипс            
            editedEllipse.points.Add(pointD.ToPointF());
            // ... или сразу две
            if (editedEllipse.points.Count == 1)
                editedEllipse.points.Add(pointD.ToPointF());

            ellipseBeingSized = editedEllipse;
        }

        protected override bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            if (ellipseBeingSized == null) return false;
            
            // поменять последнюю точку эллипса
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            if (ellipseBeingSized.buildAuto)
            {// меняем вторую точку, третью не трогаем
                ellipseBeingSized.points[1] = pointD.ToPointF();
            }
            else
                ellipseBeingSized.points[ellipseBeingSized.points.Count - 1] = pointD.ToPointF();

            // автоматом достроить третье измерение?
            if (ellipseBeingSized.buildAuto)
                BuildEllipseAuto(ellipseBeingSized);
            
            return true;
        }

        protected override bool OnMouseUp(List<SeriesEditParameter> parameters, MouseEventArgs e, 
            Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (ellipseBeingSized == null) return false;

            // полуось эллипса не выродилась в точку?
            var lenPx = Conversion.GetSpanLenInScreenCoords(ellipseBeingSized.points[0],
                ellipseBeingSized.points[1], Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);
            if (lenPx < 3)
            {
                data.Remove(ellipseBeingSized);
                ellipseBeingSized = null;
                return true;
            }

            ellipseBeingSized.buildAuto = false;

            // если в эллипсе все три точки - завершить редактирование
            if (ellipseBeingSized.points.Count == 3)
            {
                // проставить время - по первой точке
                var minIndex = ellipseBeingSized.points.Min(p => p.X);
                ellipseBeingSized.DateStart = Chart.StockSeries.GetCandleOpenTimeByIndex((int)minIndex);
                ellipseBeingSized.IsBeingCreated = false;
                objectToEdit = ellipseBeingSized;
            }

            // иначе - просто завершить режим "резинки" до след. нажатия
            ellipseBeingSized = null;
            return true;
        }

        /// <summary>
        /// построить эллипс по двум точкам так, чтобы он опоясывал тени свечей в своих пределах
        /// </summary>        
        private bool BuildEllipseAuto(ChartEllipse editedEllipse)
        {
            if (editedEllipse.points[0].X == editedEllipse.points[1].X) return false; // эллипс вырожден
            if (Owner.CanvasRect.Width == 0 || Owner.CanvasRect.Height == 0 ||
                Owner.WorldRect.Width == 0 || Owner.WorldRect.Height == 0) return false;

            // если точки эллипса следуют задом наперед - поменять их местами
            if (editedEllipse.points[0].X > editedEllipse.points[1].X)
            {
                var pt = editedEllipse.points[1];
                editedEllipse.points[1] = editedEllipse.points[0];
                editedEllipse.points[0] = pt;
            }
            
            // координаты в масштабе экрана
            PointF a = editedEllipse.points[0], b = editedEllipse.points[1];
            PointF aScale = Conversion.WorldToScreen(new PointD(a.X, a.Y),
                                                     Owner.WorldRect, Owner.CanvasRect).ToPointF();
            PointF bScale = Conversion.WorldToScreen(new PointD(b.X, b.Y),
                                                     Owner.WorldRect, Owner.CanvasRect).ToPointF();

            var candles = Chart.StockSeries.Data.Candles;

            var start = Math.Max((int) a.X + 1, 0);
            var end = Math.Min((int) b.X - 1, candles.Count - 1);
            if (start >= end) return false;
            
            // по каждой свече - эллипс должен охватывать оную
            Cortege2<PointF, float>? maxEllipsePtrs = null;
            for (var i = start; i <= end; i++)
            {
                // кончик свечки (хай или лоу)
                PointF candleTip = Conversion.WorldToScreen(new PointD(i, (double)candles[i].high),
                                                     Owner.WorldRect, Owner.CanvasRect).ToPointF();

                var ellipsePtrs = TryMakeChartEllipse(aScale, bScale, candleTip, true);
                if (!maxEllipsePtrs.HasValue && ellipsePtrs.HasValue) maxEllipsePtrs = ellipsePtrs;
                else
                {
                    if (ellipsePtrs.HasValue)
                        if (ellipsePtrs.Value.b > maxEllipsePtrs.Value.b)
                            maxEllipsePtrs = ellipsePtrs;
                }
                // тупой копипаст для лоу
                candleTip = Conversion.WorldToScreen(new PointD(i, (double)candles[i].low),
                                                     Owner.WorldRect, Owner.CanvasRect).ToPointF();
                ellipsePtrs = TryMakeChartEllipse(aScale, bScale, candleTip, true);
                if (!maxEllipsePtrs.HasValue && ellipsePtrs.HasValue) maxEllipsePtrs = ellipsePtrs;
                else
                {
                    if (ellipsePtrs.HasValue)
                        if (ellipsePtrs.Value.b > maxEllipsePtrs.Value.b)
                            maxEllipsePtrs = ellipsePtrs;
                }
            }
            // если описывающий эллипс существует
            if (maxEllipsePtrs.HasValue)
            {
                var pt = Conversion.ScreenToWorld(new PointD(maxEllipsePtrs.Value.a.X, maxEllipsePtrs.Value.a.Y),
                                                  Owner.WorldRect, Owner.CanvasRect).ToPointF();
                if (editedEllipse.points.Count == 2)
                    editedEllipse.points.Add(pt);
                else
                    editedEllipse.points[2] = pt;
            }
            else
            {
                if (editedEllipse.points.Count == 3)
                    editedEllipse.points.RemoveAt(editedEllipse.points.Count - 1);
            }
            return true;
        }

        /// <param name="a">первый кончик эллипса, масштабированный</param>
        /// <param name="b">второй кончик эллипса, масштабированный</param>
        /// <param name="tip">high или low свечки, масштабированный</param>
        /// <param name="isHigh">true если tip - high свечи</param>
        /// <returns>третья точка эллипса и его вторая полуось</returns>
        private static Cortege2<PointF, float>? TryMakeChartEllipse(PointF a, PointF b, PointF tip, bool isHigh)
        {
            // - 1 - проверить - вершинка свечи ниже отрезка ab неинтересна,
            // как и лоу выше отрезка ab
            if (a.X == b.X) return null;
            //var tipProjY = (b.Y - a.Y)*(tip.X - a.X)/(b.X - a.X);
            //if (isHigh && tip.Y >= tipProjY) return null; // знаки инвертированы как и ось Y
            //if (!isHigh && tip.Y <= tipProjY) return null;
            // попробовать построить эллипс
            double alpha;
            float A, B, cx, cy;
            if (Geometry.GetEllipseParams(a, b, tip, out alpha, out A, out B, out cx, out cy))
                return new Cortege2<PointF, float>(tip, B);
            return null;
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            foreach (var item in data) interObjects.Add(item);
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new ChartEllipse();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is ChartEllipse == false) return;
            data.Remove((ChartEllipse)interObject);
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
            foreach (var ellipse in data)
            {
                if (ellipse.points.Count < 2) continue;
                
                var ptA = Conversion.WorldToScreen(new PointD(ellipse.points[0].X, ellipse.points[0].Y),
                    Owner.WorldRect, Owner.CanvasRect);
                var ptB = Conversion.WorldToScreen(new PointD(ellipse.points[1].X, ellipse.points[1].Y),
                        Owner.WorldRect, Owner.CanvasRect);
                
                if (ellipse.points.Count == 2)
                {
                    if (Geometry.IsDotOnSpan(new PointD(ptClient.X, ptClient.Y), ptA, ptB, tolerance))
                        list.Add(ellipse);
                    continue;
                }

                if (!ellipse.correctEllipse)
                {
                    var ptC = Conversion.WorldToScreen(new PointD(ellipse.points[2].X, ellipse.points[2].Y),
                        Owner.WorldRect, Owner.CanvasRect);
                    if (Geometry.IsDotOnSpan(new PointD(ptClient.X, ptClient.Y), ptA, ptC, tolerance))
                        list.Add(ellipse);
                    else
                        if (Geometry.IsDotOnSpan(new PointD(ptClient.X, ptClient.Y), ptB, ptC, tolerance))
                            list.Add(ellipse);
                    continue;
                }

                var angle = ellipse.angle;
                var a = ellipse.a;
                var b = ellipse.b;
                var cx = (float)(ptA.X + ptB.X) * 0.5f;
                var cy = (float)(ptA.Y + ptB.Y) * 0.5f;
                // проверить попадание непосредственно в эллипс
                var rx = ptClient.X - cx;
                var ry = ptClient.Y - cy;
                // повернуть точку на -angle для совпадение СК
                var pt = Geometry.RotatePoint(new PointF(rx, ry), new PointF(0, 0), angle);
                rx = pt.X;
                ry = pt.Y;

                // точка должна лежать внутри БОЛЬШЕГО эллипса
                var tol2 = (tolerance & 1) > 0 ? tolerance / 2 + 1 : tolerance / 2;
                float a2 = a + tol2, b2 = b + tol2;
                var rad = rx * rx / (a2 * a2) + ry * ry / (b2 * b2);
                if (rad <= 1)
                {
                    // точка должна лежать ВНЕ меньшего эллипса (на N пикселей уже и ниже данного tolerance)
                    a2 = a - tol2; 
                    b2 = b - tol2;
                    var rad2 = rx * rx / (a2 * a2) + ry * ry / (b2 * b2);
                    if (rad2 >= 1)
                        list.Add(ellipse);
                }
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner){}

        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var obj in data)
                obj.AjustColorScheme(chart);
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

    /// <summary>
    /// стрелочка и текст коментария
    /// </summary>
    public class ChartEllipse : IChartInteractiveObject
    {
        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleEllipse"); } }

        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        private static int nextEllipseNumber = 1;

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }

        /// <summary>
        /// эллипс не вырожден, параметр определяется при отрисовке
        /// </summary>
        public bool correctEllipse;

        public double angle;

        public float cx, cy, a, b;

        /// <summary>
        /// альфа-канал (прозрачность) кисти, от 0 до 100
        /// </summary>
        [LocalizedDisplayName("TitleBrushTransparency")]
        [LocalizedCategory("TitleVisuals")]
        [Editor(typeof(TransparencyUITypeWEditor), typeof(UITypeEditor))]
        public int BrushAlpha { get; set; }
        
        private Color brushColor = Color.YellowGreen;

        /// <summary>
        /// цвет заливки (комбинируется с BrushAlpha)
        /// </summary>
        [LocalizedDisplayName("TitleBrushColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color BrushColor
        {
            get { return brushColor; }
            set { brushColor = value; }
        }

        /// <summary>
        /// строить касательную
        /// </summary>
        [LocalizedDisplayName("TitleTangent")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(CheckBoxPropertyGridEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public bool BuildTangent { get; set; }

        public readonly List<double> tangentFiboLevels = new List<double> { 0, 0.618, 1, 1.618, 2.618 };

        [LocalizedDisplayName("TitleTangentLevels")]
        [LocalizedCategory("TitleMain")]
        public string TangentFiboLevels
        {
            get
            {
                if (tangentFiboLevels.Count == 0) return "";
                var sb = new StringBuilder();
                foreach (var level in tangentFiboLevels)
                    sb.AppendFormat("{0},", level.ToString(CultureProvider.Common));
                var str = sb.ToString();
                return str.Substring(0, str.Length - 1);
            }
            set
            {
                tangentFiboLevels.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var nums = value.Split(',');
                    foreach (var num in nums)
                        tangentFiboLevels.Add(double.Parse(num, CultureProvider.Common));
                }
            }
        }

        public enum EllipseTangentType { Отрезок = 0, Прямая = 1 }

        [LocalizedDisplayName("TitleTangentView")]
        [LocalizedCategory("TitleMain")]
        public EllipseTangentType TangentType { get; set; }

        /// <summary>
        /// список точек эллипса (индекс свечи - цена)
        /// </summary>
        public readonly List<PointF> points = new List<PointF>();

        public bool Incompleted
        {
            get
            {
                return points.Count < 3;
            }
        }

        public bool IsBeingCreated { get; set; }

        /// <summary>
        /// флаг взводится при редактировании
        /// если взведен, третья точка будет построена автоматически
        /// </summary>
        public bool buildAuto;

        /// <summary>
        /// маркеры
        /// </summary>
        private readonly ChartObjectMarker[] markers = new[]
                                                           {
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Move},
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Resize},
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Resize},
                                                               new ChartObjectMarker
                                                                   {action = ChartObjectMarker.MarkerAction.Resize}
                                                           };

        public ChartEllipse()
        {
            Name = string.Format("{0} {1}", ClassName, nextEllipseNumber++);
            // автоподбор цвета линий
            lineColor = lineColors[curColorIndex++];
            if (curColorIndex >= lineColors.Length) curColorIndex = 0;
            // подбор цвета расширения - более бледный
            // снять насыщение и поднять яркость
            const double kDesaturate = 0.75;
            const int deltaBright = 110;
            var intense = (lineColor.R + lineColor.G + lineColor.B) / 3;
            var red = lineColor.R * (1 - kDesaturate) + intense * kDesaturate + deltaBright;
            var green = lineColor.G * (1 - kDesaturate) + intense * kDesaturate + deltaBright;
            var blue = lineColor.B * (1 - kDesaturate) + intense * kDesaturate + deltaBright;

            red = red > 255 ? 255 : red;
            green = green > 255 ? 255 : green;
            blue = blue > 255 ? 255 : blue;
            tangentColor = Color.FromArgb((int) red, (int) green, (int) blue);
        }

        public void AddPoint(float index, float price)
        {
            if (points.Count < 3) points.Add(new PointF(index, price));
        }

        public void DrawEllipse(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            BrushesStorage brushStorage, PenStorage penStorage, Font font)
        {
            const int markerSize = 3;
            var pen = penStorage.GetPen(Color, Selected ? 3f : 1f);
            var screenPoints = new List<PointD>();
            foreach (var pt in points)
            {
                screenPoints.Add(Conversion.WorldToScreen(new PointD(pt.X, pt.Y),
                                                            worldRect, canvasRect));
            }
            foreach (var pt in screenPoints)
            {
                g.DrawRectangle(pen, (float)pt.X - markerSize, (float)pt.Y - markerSize,
                                markerSize * 2F, markerSize * 2F);
            }
            if (screenPoints.Count == 2)
            {
                g.DrawLine(pen, screenPoints[0].ToPointF(), screenPoints[1].ToPointF());
            }
            if (screenPoints.Count == 3)
            {
                // нарисовать собственно эллипс
                double newAngle;
                float newCx, newCy, newA, newB;
                correctEllipse = Geometry.GetEllipseParams(screenPoints[0].ToPointF(),
                                                                screenPoints[1].ToPointF(),
                                                                screenPoints[2].ToPointF(),
                                                                out newAngle, out newA, out newB, out newCx, out newCy);
                if (correctEllipse) // можно построить эллипс - рисуем его
                {
                    a = newA;
                    b = newB;
                    angle = newAngle;
                    cx = newCx;
                    cy = newCy;
                    var ellipseBezierPoints = Geometry.GetEllipseBezierPoints(newAngle, newA, newB, newCx, newCy);
                    g.DrawBeziers(pen, ellipseBezierPoints);
                    if (BrushAlpha > 0)
                    {
                        var brush = brushStorage.GetBrush(Color.FromArgb(BrushAlpha, BrushColor));
                        g.FillClosedCurve(brush, ellipseBezierPoints);
                    }
                    // строить касательную
                    if (BuildTangent)
                        DrawTangent(screenPoints, canvasRect, g,
                            penStorage, brushStorage, font);
                }
                else // построить эллипс по указанным координатам невозможно
                {
                    g.DrawLine(pen, screenPoints[1].ToPointF(), screenPoints[2].ToPointF());
                    g.DrawLine(pen, screenPoints[0].ToPointF(), screenPoints[2].ToPointF());
                }
            }
            // маркеры
            if (Selected)
                DrawComments(g, worldRect, canvasRect, penStorage, brushStorage);
        }

        private void DrawTangent(List<PointD> screenPoints, Rectangle canvasRect, Graphics g, PenStorage penStorage,
                                 BrushesStorage brushStorage, Font font)
        {
            var ptLeft = points[0].X < points[1].X ? points[0] : points[1];
            var ptRight = points[0].X < points[1].X ? points[1] : points[0];
            var m1 = new PointD(cx + b * Math.Sin(angle), cy + b * Math.Cos(angle));
            var m2 = new PointD(cx + b * Math.Sin(angle + Math.PI), cy + b * Math.Cos(angle + Math.PI));
            var m = ptLeft.Y > ptRight.Y
                        ? m1.Y < m2.Y ? m1 : m2 // нижняя касательная для растущего эллипса
                        : m1.Y < m2.Y ? m2 : m1; // верхняя для падающего

            var o = new PointD(cx, cy);
            var r = new PointD(m.X - o.X, m.Y - o.Y);

            var pen = penStorage.GetPen(TangentColor);

            foreach (var level in tangentFiboLevels)
            {
                // нарисовать касательную или параллельную ей линию
                var A = new PointD(screenPoints[0].X + r.X * (level + 1), screenPoints[0].Y + r.Y * (level + 1));
                var B = new PointD(screenPoints[1].X + r.X * (level + 1), screenPoints[1].Y + r.Y * (level + 1));
                if (TangentType == EllipseTangentType.Прямая) StretchSpanToScreen(ref A, ref B, canvasRect);
                g.DrawLine(pen, A.ToPointF(), B.ToPointF());
                if (level == 0) continue;
                // нарисовать текстовую отметку
                var ptText = new PointD(o.X + r.X * (level + 1), o.Y + r.Y * (level + 1));
                var textSz = g.MeasureString(level.ToString(), font);
                var textRect = new Rectangle((int) (ptText.X - textSz.Width / 2 - 2),
                                             (int) (ptText.Y - textSz.Height / 2 - 2),
                                             (int) textSz.Width + 4, (int) textSz.Height + 4);
                var brushWhite = brushStorage.GetBrush(Color.FromArgb(60, pen.Color));
                g.FillRectangle(brushWhite, textRect);
                g.DrawRectangle(pen, textRect);
                var brushText = brushStorage.GetBrush(pen.Color);
                g.DrawString(level.ToString(), font, brushText, (float) ptText.X, (float) ptText.Y,
                             new StringFormat
                                 {
                                     Alignment = StringAlignment.Center,
                                     LineAlignment = StringAlignment.Center
                                 });
            }
        }

        /// <summary>
        /// продолжить отрезок до границ экрана
        /// </summary>        
        private static void StretchSpanToScreen(ref PointD a, ref PointD b, Rectangle canvasRect)
        {
            if (a.X == b.X)
            {
                a = new PointD(a.X, 0);
                b = new PointD(a.X, canvasRect.Height);
                return;
            }

            double k = (b.Y - a.Y) / (b.X - a.X);
            double kB = b.Y - k * b.X;
            a = new PointD(0, 0 * k + kB);
            b = new PointD(canvasRect.Width, canvasRect.Width * k + kB);
        }
        
        #region IChartInteractiveObject

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value && !selected)                
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
            get
            {
                if (points.Count == 1)
                    return (int)points[0].X;
                return Math.Min((int) points[0].X, (int) points[1].X);
            }
        }

        private static Color[] lineColors = new[]
                                                {
                                                    Color.FromArgb(90, 60, 10), Color.FromArgb(10, 60, 90), Color.FromArgb(60, 10, 90),
                                                    Color.FromArgb(60, 90, 10), Color.FromArgb(192, 0, 10), Color.FromArgb(0, 10, 192),
                                                    Color.FromArgb(0, 192, 10), Color.FromArgb(66, 66, 66), Color.FromArgb(0, 255, 0),
                                                    Color.FromArgb(255, 0, 0), Color.FromArgb(0, 0, 255)
                                                };
        private static int curColorIndex;

        private Color lineColor = Color.DarkSeaGreen;
        /// <summary>
        /// цвет обводки
        /// </summary>
        [LocalizedDisplayName("TitleStrokeColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private Color tangentColor = Color.DarkSeaGreen;
        /// <summary>
        /// цвет обводки
        /// </summary>
        [LocalizedDisplayName("TitleTangentColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color TangentColor
        {
            get { return tangentColor; }
            set { tangentColor = value; }
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("Ellipse"));
            
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name")).Value = Name;
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("color")).Value = Color.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("brushColor")).Value = BrushColor.ToArgb().ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("brushAlpha")).Value = BrushAlpha.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("BuildTangent")).Value = BuildTangent.ToString();
            
            for (int i = 0; i < points.Count; i++)
            {
                var nodePoint = node.AppendChild(parentNode.OwnerDocument.CreateElement(string.Format("point{0}", i)));
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)Math.Round(points[i].X));
                nodePoint.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("time")).Value = 
                    time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                nodePoint.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("price")).Value = 
                    points[i].Y.ToString(CultureProvider.Common);
            }
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["name"] != null)
                Name = itemNode.Attributes["name"].Value;

            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;

            if (itemNode.Attributes["color"] != null)
                Color = Color.FromArgb(int.Parse(itemNode.Attributes["color"].Value));

            if (itemNode.Attributes["brushColor"] != null)
                BrushColor = Color.FromArgb(int.Parse(itemNode.Attributes["brushColor"].Value));

            BrushAlpha = itemNode.GetAttributeString("brushAlpha").ToIntSafe() ?? 192;

            if (itemNode.Attributes["BuildTangent"] != null)
                BuildTangent = itemNode.Attributes["BuildTangent"].Value.ToBool();
            
            foreach (XmlElement child in itemNode.ChildNodes)
            {
                var timeStr = child.Attributes["time"].Value;
                if (string.IsNullOrEmpty(timeStr)) continue;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                var price = float.Parse(child.Attributes["price"].Value, CultureProvider.Common);
                // порядок не нарушится?
                points.Add(new PointF(index, price));
            }
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            Color = ChartControl.ChartVisualSettings.AdjustColor(Color, clBack);
        }

        public Image CreateSample(Size sizeHint)
        {
            var originalPoints = new List<PointF>(points);
            points.Clear();
            var centerx = sizeHint.Width / 2;
            var centery = sizeHint.Height / 2;
            var dx = sizeHint.Width / 2;
            var dy = sizeHint.Height / 2;
            var left = centerx - dx;
            var top = centery - dy;
            var width = sizeHint.Width;
            var height = sizeHint.Height;
            points.Add(new PointF(left, top));
            points.Add(new PointF(left + width, top + height));
            points.Add(new PointF(left + width / 2, top + height));
            var worldRect = new RectangleD(left - 50, top - 50, width + 100, height + 100);
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var brushStorage = new BrushesStorage();
            var penStorage = new PenStorage();
            DrawEllipse(Graphics.FromImage(result), worldRect, new Rectangle(new Point(0, 0), sizeHint), brushStorage,
                        penStorage, new Font(FontFamily.GenericSansSerif, 8));
            points.Clear();
            points.AddRange(originalPoints);
            return result;
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
            // пересчитать маркер            
            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            
            // переместить
            if (marker.action == ChartObjectMarker.MarkerAction.Move)
            {
                float vx, vy;
                if (points.Count > 1)
                {
                    vx = (float) (marker.centerModel.X - (points[0].X + points[1].X)*0.5f);
                    vy = (float) (marker.centerModel.Y - (points[0].Y + points[1].Y)*0.5f);
                }
                else
                {
                    vx = (float)(marker.centerModel.X - points[0].X);
                    vy = (float)(marker.centerModel.Y - points[0].Y);
                }
                for (var i = 0; i < points.Count; i++)
                {
                    points[i] = new PointF(points[i].X + vx, points[i].Y + vy);
                    markers[i + 1].centerModel = points[i];
                }
                return;
            }

            // растянуть
            var markerIndex = 0;
            for (var i = 0; i < markers.Length; i++)
            {
                if (markers[i] != marker) continue;
                markerIndex = i - 1;
                break;
            }
            if (markerIndex < 0 || points.Count <= markerIndex) return;
            points[markerIndex] = new PointF((float)marker.centerModel.X, (float)marker.centerModel.Y);
        }

        private void SetupMarkers()
        {
            var point = points.Count < 2
                            ? points[0]
                            : new PointF((points[0].X + points[1].X)*0.5f,
                                         (points[0].Y + points[1].Y)*0.5f);
            // маркер перемещения
            markers[0].centerModel = new PointD(point.X, point.Y);
            
            // маркеры изменения точек привязки
            markers[1].centerModel = new PointD(points[0].X, points[0].Y);
            markers[2].centerModel = points.Count > 1 ?
                new PointD(points[1].X, points[1].Y) : new PointD(points[0].X, points[0].Y);
            markers[3].centerModel = new PointD(points[points.Count - 1].X, points[points.Count - 1].Y);
        }

        public void DrawComments(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            PenStorage penStorage, BrushesStorage brushStorage)
        {
            foreach (var marker in markers)
            {
                marker.CalculateScreenCoords(worldRect, canvasRect);
                marker.Draw(g, penStorage, brushStorage);
            }
        }
        #endregion
    }
}
