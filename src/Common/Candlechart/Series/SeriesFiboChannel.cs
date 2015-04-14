using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.FiboChannel, "TitleFibonacciChannels", ToolButtonImageIndex.FiboChannel)]
    [LocalizedSeriesToolButtonParam("LineType", typeof(TrendLine.TrendLineStyle), TrendLine.TrendLineStyle.Отрезок)]
    [LocalizedSeriesToolButtonParam("DrawLabels", typeof(bool), false)]
    [LocalizedSeriesToolButtonParam("Color", typeof(Color), DefaultValueString = "-16777216")]
    public class SeriesFiboChannel : InteractiveObjectSeries
    {
        /// <summary>
        /// каналы Фибоначчи
        /// </summary>
        public List<FiboChannel> data = new List<FiboChannel>();
        
        public override int DataCount { get { return data.Count; } }

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60 * 5);
        
        private const int LogMsgErrorDrawChannel = 1;

        public SeriesFiboChannel(string name)
            : base(name, CandleChartControl.ChartTool.FiboChannel)
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
            using (var penDic = new PenStorage())
            using (var brushStorage = new BrushesStorage())
            foreach (var channel in data)
            {
                try
                {
                    channel.Draw(g, worldRect, canvasRect, Chart.Font, penDic, brushStorage);
                }
                catch (Exception ex)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        LogMsgErrorDrawChannel, "SeriesFiboChannel darawing error: {0}", ex);
                }                
            }
        }

        private FiboChannel channelBeingCreated;
        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);

            var incompleted = data.Find(s => s.IsBeingCreated);
            if (incompleted != null)
            {
                // добавить третью точку
                incompleted.Point3 = new PointD(
                    (incompleted.linePoints[0].X + incompleted.linePoints[1].X)/2,
                    (incompleted.linePoints[0].Y + incompleted.linePoints[1].Y)/2);
                channelBeingCreated = incompleted;
                return;
            }

            // ReSharper disable UseObjectOrCollectionInitializer
            var channel = new FiboChannel { Owner = this, IsBeingCreated = true };
            // ReSharper restore UseObjectOrCollectionInitializer
            channel.DrawText = SeriesEditParameter.TryGetParamValue(parameters, "DrawLabels", false);
            channel.LineColor = SeriesEditParameter.TryGetParamValue(parameters, "Color", Color.DarkBlue);
            channel.LineStyle = SeriesEditParameter.TryGetParamValue(parameters, "LineType", TrendLine.TrendLineStyle.Отрезок);
            channel.AddPoint(pointD.X, pointD.Y);
            channel.AddPoint(pointD.X, pointD.Y);
            channelBeingCreated = channel;

            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                channel.AjustColorScheme(Owner.Owner.Owner);
            data.Add(channel);            
        }

        protected override bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            if (channelBeingCreated == null) return false;
            
            // изменить вторую точку линии или точку проекции
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            if (channelBeingCreated.Point3.HasValue)
                channelBeingCreated.Point3 = pointD;
            else
                channelBeingCreated.linePoints[channelBeingCreated.linePoints.Count - 1] = pointD;

            return true;
        }

        protected override bool OnMouseUp(List<SeriesEditParameter> parameters, MouseEventArgs e, 
            Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (channelBeingCreated == null) return false;

            // если канал незавершен - нет точки проекции
            if (!channelBeingCreated.Point3.HasValue)
            {
                // если канал сжался в точку сингулярности - удалить его
                var lenPx = Conversion.GetSpanLenInScreenCoords(channelBeingCreated.linePoints[0],
                    channelBeingCreated.linePoints[1], Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);
                if (lenPx < 3)
                {
                    data.Remove(channelBeingCreated);
                    channelBeingCreated = null;
                    return true;
                }
                
                // добавить третью точку
                channelBeingCreated.Point3 = new PointD(
                    (channelBeingCreated.linePoints[0].X + channelBeingCreated.linePoints[1].X) / 2,
                    (channelBeingCreated.linePoints[0].Y + channelBeingCreated.linePoints[1].Y) / 2);

                channelBeingCreated = null;
                return true;
            }
            
            // канал завершен
            channelBeingCreated.IsBeingCreated = false;
            objectToEdit = channelBeingCreated;

            channelBeingCreated = null;
            return true;
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            foreach (var item in data) interObjects.Add(item);
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is FiboChannel == false) return;
            data.Remove((FiboChannel) interObject);
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
            var obj = new FiboChannel();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var list = new List<IChartInteractiveObject>();
            foreach (var channel in data)
            {
                if (channel.IsIn(ptClient.X, ptClient.Y, tolerance))
                    list.Add(channel);

                //if (!channel.Completed) continue;
                //if (channel.LineStyle == TrendLine.TrendLineStyle.Линия)
                //{
                //    list.Add(channel);
                //    continue;
                //}
                //var a = Conversion.WorldToScreen(new PointD(channel.linePoints[0].X, channel.linePoints[0].Y),
                //    Owner.WorldRect, Owner.CanvasRect);
                //var b = Conversion.WorldToScreen(new PointD(channel.linePoints[1].X, channel.linePoints[1].Y),
                //    Owner.WorldRect, Owner.CanvasRect);
                //if (Geometry.IsDotInArea(new PointD(ptClient.X, ptClient.Y), a, b, tolerance))
                //    list.Add(channel);
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner)
        {
        }

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
    /// линия (TrendLine) и точка, точка задает первую линию, параллельную отрезку
    /// остальные точки задают серии Фибоначчи (0.618, 1, 1.618 ...)
    /// </summary>
    public class FiboChannel : TrendLine
    {
        private static int nextFiboChannelNum = 1;

        [Browsable(false)]
        public override string ClassName { get { return Localizer.GetString("TitleFibonacciChannelShort"); } }

        private readonly List<Cortege2<Point, Point>> spansInScreenCoords = new List<Cortege2<Point, Point>>();

        private Cortege2<Point, Point> spanNormalScreenCoords;

        /// <summary>
        /// третья точка - задает первую линию канала
        /// </summary>
        public PointD? Point3
        {
            get; set;
        }

        /// <summary>
        /// проекции
        /// </summary>
        public readonly List<double> series = new List<double> { 0.618, 1, 1.618, 2.618 };

        /// <summary>
        /// определить уровни в виде строки (для окна свойств)
        /// </summary>
        [LocalizedDisplayName("TitleFibonacciSeriesShort")]
        [LocalizedCategory("TitleMain")]
        public string Series
        {
            get
            {
                if (series.Count == 0) return "";
                var sb = new StringBuilder();
                foreach (var level in series)
                    sb.AppendFormat("{0},", level.ToString(CultureProvider.Common));
                var str = sb.ToString();
                return str.Substring(0, str.Length - 1);
            }
            set
            {
                series.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var nums = value.Split(',');
                    foreach (var num in nums)
                        series.Add(double.Parse(num, CultureProvider.Common));
                }
            }
        }

        [LocalizedDisplayName("TitleDrawText")]
        [LocalizedCategory("TitleVisuals")]
        public bool DrawText { get; set; }

        public FiboChannel()
        {
            Name = string.Format("{0} {1}", ClassName, nextFiboChannelNum++);
            LineStyle = TrendLineStyle.Отрезок;
            PenDashStyle = DashStyle.Dash;
            markers = new []
                          {
                              new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Move },
                              new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize },
                              new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize },
                              new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize }
                          };
        }

        public override void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            base.SaveInXML(parentNode, owner);
            var node = parentNode.ChildNodes[parentNode.ChildNodes.Count - 1];
            
            var seriesAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("series"));
            seriesAttr.Value = Series;

            if (Point3.HasValue)
            {
                var pointXAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("time"));
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)Point3.Value.X);
                pointXAttr.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                
                var pointYAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("y"));
                pointYAttr.Value = Point3.Value.Y.ToString(CultureProvider.Common);
            }
        }

        public override void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            base.LoadFromXML(itemNode, owner);
            if (itemNode.Attributes["series"] != null) Series = itemNode.Attributes["series"].Value;
            if (itemNode.Attributes["time"] != null)
            {
                var timeStr = itemNode.Attributes["time"].Value;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);

                Point3 = new PointD(index, double.Parse(itemNode.Attributes["y"].Value, 
                    CultureProvider.Common));
            }
        }

        public void AddPoint(PointD point)
        {
            if (linePoints.Count < 2) linePoints.Add(point);
            else
                if (!Point3.HasValue) Point3 = point;
        }

        public new void AddPoint(double x, double y)
        {
            AddPoint(new PointD(x, y));
        }

        public new bool Completed
        {
            get
            {
                return linePoints.Count == 2 && Point3.HasValue;
            }
        }

        public void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect, Font font,
            PenStorage penDic, BrushesStorage brushStorage)
        {
            spansInScreenCoords.Clear();
            Draw(g, worldRect, canvasRect, penDic, brushStorage);
            
            if (linePoints.Count == 2)
            {// сохранить экранные координаты задающего отрезка
                spansInScreenCoords.Add(new Cortege2<Point, Point>(
                                            new Point((int) screenPointA.X, (int) screenPointA.Y),
                                            new Point((int) screenPointB.X, (int) screenPointB.Y)));
            }
            if (!Point3.HasValue) return;

            PointD oldPoint0 = linePoints[0], oldPoint1 = linePoints[1];            
            // параллельно перенести прямую по уровням
            var pointC = ProjectDotOnSpan(worldRect, canvasRect);
            var vector = new PointD(Point3.Value.X - pointC.X, Point3.Value.Y - pointC.Y);
            
            // начальная прямая
            linePoints[0] = oldPoint0 + vector;
            linePoints[1] = oldPoint1 + vector;
            Draw(g, worldRect, canvasRect, penDic, brushStorage);
            spansInScreenCoords.Add(new Cortege2<Point, Point>(
                                            new Point((int)screenPointA.X, (int)screenPointA.Y),
                                            new Point((int)screenPointB.X, (int)screenPointB.Y)));

            // нормаль
            var vectorScale = series.Count == 0 ? 1.0 : series[series.Count - 1] + 1;
            var pointC2 = pointC + vector * vectorScale;
            var pointArrow = Conversion.WorldToScreen(pointC + vector, worldRect, canvasRect);
            var screenC = Conversion.WorldToScreen(pointC, worldRect, canvasRect);
            var screenC2 = Conversion.WorldToScreen(pointC2, worldRect, canvasRect);
            using (var pen = new Pen(LineColor) { DashStyle = DashStyle.Dot })
            {
                g.DrawLine(pen, screenC.ToPointF(), screenC2.ToPointF());
                g.DrawPolygon(pen, GetArrowPoints(screenC, pointArrow));
                // сохранить экранные координаты нормали
                spanNormalScreenCoords = new Cortege2<Point, Point>(screenC.ToPoint(), screenC2.ToPoint());
            }

            // ряд прямых
            var rectPen = penDic.GetPen(LineColor);
            foreach (var level in series)
            {
                var vectorScaled = vector * (1 + level);
                linePoints[0] = oldPoint0 + vectorScaled;
                linePoints[1] = oldPoint1 + vectorScaled;
                Draw(g, worldRect, canvasRect, penDic, brushStorage);
                // сохранить координаты (для проверки попадания)
                spansInScreenCoords.Add(new Cortege2<Point, Point>(
                                            new Point((int)screenPointA.X, (int)screenPointA.Y),
                                            new Point((int)screenPointB.X, (int)screenPointB.Y)));
                // вывести подпись в рамке
                if (DrawText)
                {
                    var whiteBrush = brushStorage.GetBrush(Color.White);
                    var brush = brushStorage.GetBrush(LineColor);

                    var ptText = pointC + vectorScaled;
                    ptText = Conversion.WorldToScreen(ptText, worldRect, canvasRect);
                    var textSz = g.MeasureString(level.ToString(), font);
                    var textRect = new Rectangle((int) (ptText.X - textSz.Width/2 - 2),
                                                    (int) (ptText.Y - textSz.Height/2 - 2),
                                                    (int) textSz.Width + 4, (int) textSz.Height + 4);
                    g.FillRectangle(whiteBrush, textRect);
                    g.DrawRectangle(rectPen, textRect);
                    g.DrawString(level.ToString(), font, brush, (float) ptText.X, (float) ptText.Y,
                                    new StringFormat
                                        {
                                            Alignment = StringAlignment.Center,
                                            LineAlignment = StringAlignment.Center
                                        });
                }
            }

            // восстановить начальные точки
            linePoints[0] = oldPoint0;
            linePoints[1] = oldPoint1;
        }

        public bool IsIn(int x, int y, int tolerance)
        {
            var ptCurs = new Point(x, y);
            // попадание в один из отрезков (в одну из линий)
            foreach (var span in spansInScreenCoords)
            {
                if (LineStyle == TrendLineStyle.Линия)
                {
                    if (Geometry.IsDotOnLine(ptCurs, span.a, span.b, tolerance)) return true;
                }
                else 
                    if (Geometry.IsDotOnSpan(ptCurs, span.a, span.b, tolerance)) return true;            
            }
            // попадание в нормаль
            if (Geometry.IsDotOnSpan(ptCurs, spanNormalScreenCoords.a, spanNormalScreenCoords.b, tolerance))
                return true;
            return false;
        }

        private PointD ProjectDotOnSpan(RectangleD worldRect, Rectangle canvasRect)
        {
            if (linePoints[0].X == linePoints[1].X)
                return new PointD(linePoints[0].X, Point3.Value.Y);
            if (linePoints[0].Y == linePoints[1].Y)
                return new PointD(Point3.Value.X, linePoints[0].Y);
            
            var pA = Conversion.WorldToScreen(linePoints[0], worldRect, canvasRect);
            var pB = Conversion.WorldToScreen(linePoints[1], worldRect, canvasRect);
            var pC = Conversion.WorldToScreen(Point3.Value, worldRect, canvasRect);
            
            var b = (pA.X - pB.X) / (pB.Y - pA.Y);
            var c = pA.X + b * pA.Y;

            var x = (c - b*pC.Y + b*b*pC.X)/(b*b + 1.0);
            var y = (c - x)/b;
            return Conversion.ScreenToWorld(new PointD(x, y), worldRect, canvasRect);
        }

        /// <summary>
        /// вернуть массив точек стрелочки в точку e
        /// </summary>        
        private PointF[] GetArrowPoints(PointD b, PointD e)
        {
            const double arrowA = Math.PI/8;
            const int arrowL = 8;

            //double spanAngle = Math.Atan2(e.Y - b.Y, e.X - b.X);
            var spanLen = Geometry.GetSpanLength(b, e);
            if (spanLen == 0) return new PointF[0];

            var c = e + (b - e)*(arrowL/spanLen);
            var c1 = Geometry.RotatePoint(c, e, -arrowA);
            var c2 = Geometry.RotatePoint(c, e, arrowA);
            return new[] { e.ToPointF(), c1.ToPointF(), c2.ToPointF() };
        }

        protected override void SetupMarkers()
        {
            var firstPoint = linePoints[0];
            var lastPoint = linePoints.Count > 1 ? linePoints[1] : linePoints[0];
            markers[0].centerModel = new PointD((firstPoint.X + lastPoint.X) * 0.5f,
                                                (firstPoint.Y + lastPoint.Y) * 0.5f);
            markers[1].centerModel = firstPoint;
            markers[2].centerModel = lastPoint;
            markers[3].centerModel = Point3.HasValue ? Point3.Value : lastPoint;
        }

        public override void OnMarkerMoved(ChartObjectMarker marker)
        {
            // если речь не идет о точке, задающей ширину канала...
            if (marker.action == ChartObjectMarker.MarkerAction.Move)
            {
                base.OnMarkerMoved(marker);
                return;
            }
            var markerIndex = 0;
            for (var i = 0; i < markers.Length; i++)
            {
                if (markers[i] != marker) continue;
                markerIndex = i - 1;
                break;
            }
            if (markerIndex < 2)
            {
                base.OnMarkerMoved(marker);
                return;
            }

            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);
            // определить ширину канала
            Point3 = marker.centerModel;
        }
    }
}