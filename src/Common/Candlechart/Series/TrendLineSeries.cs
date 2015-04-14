using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using Entity;

namespace Candlechart.Series
{
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.TrendLine, "TitleTrendLines", ToolButtonImageIndex.TrendLine)]
    [LocalizedSeriesToolButtonParam("Type", typeof(TrendLine.TrendLineStyle), TrendLine.TrendLineStyle.Отрезок)]
    [LocalizedSeriesToolButtonParam("Stroke", typeof(Color), DefaultValueString = "-16777216")]
    [LocalizedSeriesToolButtonParam("Filling", typeof(Color), DefaultValueString = "-1")]
    [LocalizedSeriesToolButtonParam("Transparency", typeof(int), DefaultValue = 192)]
    [LocalizedSeriesToolButtonParam("Horizontal", typeof(bool), DefaultValue = false)]
    [LocalizedSeriesToolButtonParam("Horizontal_Shift", typeof(bool), DefaultValue = true)]
    [LocalizedSeriesToolButtonParam("ShowWindow", typeof(bool), DefaultValue = false)]
    [LocalizedSeriesToolButtonParam("Edit", typeof(bool), DefaultValue = true)]
    [LocalizedSeriesToolButtonParam("Subtitles", typeof(bool), DefaultValue = false)]
    [LocalizedSeriesToolButtonParam("MeasureOnly", typeof(bool), DefaultValue = false)]
    public class TrendLineSeries : InteractiveObjectSeries
    {
        /// <summary>
        /// исходные точки проекций
        /// </summary>
        public List<TrendLine> data = new List<TrendLine>();
        public override int DataCount { get { return data.Count; } }

        private bool drawText = true;

        /// <summary>
        /// готовые проекции (см MakeProjections)
        /// </summary>
        public List<FiboSpanProjection> projections = new List<FiboSpanProjection>();

        public TrendLineSeries(string name)
            : base(name, CandleChartControl.ChartTool.TrendLine)
        {
        }

        public bool DrawText
        {
            get { return drawText; }
            set { drawText = value; }
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
            using (var pens = new PenStorage())
            using (var brushes = new BrushesStorage())
            {
                foreach (TrendLine line in data)
                {
                    line.Draw(g, worldRect, canvasRect, pens, brushes);
                }
            }
        }
        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);

            var incompleted = data.Find(s => s.IsBeingCreated);
            if (incompleted != null) return;

            // создать линию, которая будет рисоваться в режиме "резинка"
            var lineType = SeriesEditParameter.TryGetParamValue(parameters, "Type", TrendLine.TrendLineStyle.Отрезок);
            var lineColor = SeriesEditParameter.TryGetParamValue(parameters, "Stroke", Color.Black);
            var fillColor = SeriesEditParameter.TryGetParamValue(parameters, "Filling", Color.White);
            var alphaColor = SeriesEditParameter.TryGetParamValue(parameters, "Transparency", 192);
            var isHorisont = SeriesEditParameter.TryGetParamValue(parameters, "Horizontal", false);
            var isHorisontShift = SeriesEditParameter.TryGetParamValue(parameters, "Horizontal_Shift", true);
            var showTags = SeriesEditParameter.TryGetParamValue(parameters, "Subtitles", true);
            
            var shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            if (!isHorisont) isHorisont = shiftPressed && isHorisontShift;
            
            var span = new TrendLine
                           {
                               Owner = this, 
                               LineStyle = isHorisont ? TrendLine.TrendLineStyle.Линия : lineType,
                               LineColor = lineColor,
                               ShapeFillColor = fillColor,
                               ShapeAlpha = alphaColor,
                               IsBeingCreated = true,
                               ShowTags = showTags
                           };
            span.AddPoint(pointD.X, pointD.Y);
            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                span.AjustColorScheme(Owner.Owner.Owner);

            // автоматом добавить вторую точку на одной высоте
            if (isHorisont)
            {
                var shouldEdit = SeriesEditParameter.TryGetParamValue(parameters, "Edit", true);
                AddSecondPointAuto(span);
                if (shouldEdit) 
                    objectToEdit = span;
            }            
            data.Add(span);            
        }

        /// <summary>
        /// если находимся в режиме создания линии - отрисуем ее до текущей точки
        /// в режиме "резинка"
        /// </summary>
        protected override bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            var incompleted = data.Find(s => s.IsBeingCreated);
            if (incompleted == null) return false;

            // продолжить редактирование
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            incompleted.AddPoint(pointD.X, pointD.Y);

            return true;
        }

        /// <summary>
        /// закончить создание линии
        /// </summary>
        protected override bool OnMouseUp(List<SeriesEditParameter> parameters, 
            MouseEventArgs e, Keys modifierKeys, 
            out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;

            var incompleted = data.Find(s => s.IsBeingCreated);
            if (incompleted == null) return false;

            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            incompleted.AddPoint(pointD.X, pointD.Y);
            
            // если линия слишком короткая - возможно, создана случайно?
            var screenPtA = Chart.Owner.WorldToChartCoords(incompleted.linePoints[0].X,
                                                           incompleted.linePoints[0].Y);
            var deltaPix = Math.Abs(e.X - screenPtA.X) + Math.Abs(e.Y - screenPtA.Y);
            if (deltaPix < 4)
            {
                data.Remove(incompleted);
                return true;
            }

            var shouldDeleteOnMouseUp = SeriesEditParameter.TryGetParamValue(parameters, "MeasureOnly", false);
            if (shouldDeleteOnMouseUp)
            {
                data.Remove(incompleted);
                return true;
            }
            
            incompleted.IsBeingCreated = false;
            incompleted.ShowTags = false;

            var showWindow = SeriesEditParameter.TryGetParamValue(parameters, "ShowWindow", false);
            var shouldEdit = SeriesEditParameter.TryGetParamValue(parameters, "Edit", true);
            if (shouldEdit) objectToEdit = incompleted;
                else 
                if (showWindow)
                {
                    Chart.toolSkipMouseDown = true;
                    var dlg = new ObjectPropertyWindow(new List<object> { incompleted });
                    dlg.ShowDialog();
                }
            return true;
        }

        //private Rectangle MakeInvalidateArea(TrendLine incompleted, MouseEventArgs e)
        //{
        //    var screenA = Chart.Owner.WorldToChartCoords(incompleted.linePoints[0].X, incompleted.linePoints[0].Y);
        //    var screenB = Owner.PointToScreen(new Point(e.X, e.Y));
        //    screenB = Owner.Owner.PointToScreen(screenB);

        //    var left = Math.Min(screenB.X, screenA.X) - 1;
        //    var top = Math.Min(screenB.Y, screenA.Y) - 1;
        //    var width = Math.Abs(screenB.X - screenA.X) + 2;
        //    var height = Math.Abs(screenB.Y - screenA.Y) + 2;
        //    return new Rectangle(left, top, width, height);
        //}

        private void AddSecondPointAuto(TrendLine span)
        {
            var x = span.linePoints[0].X;
            var x1 = x;
            var y = span.linePoints[0].Y;
            // определить границы экрана
            var left = Owner.Owner.StockPane.WorldRect.Left;
            var right = Owner.Owner.StockPane.WorldRect.Right;
            var max = Owner.Owner.StockSeries.DataCount;
            var mid = 0.5*(left + right);
            var sign = x > mid ? -1 : 1;
            var lenPix = Owner.Owner.StockPane.CanvasRect.Width/2.2;
            if (lenPix < 50) lenPix = 50;
            var sz = Conversion.ScreenToWorld(new SizeD(lenPix, 0), Owner.Owner.StockPane.WorldRect,
                                              Owner.Owner.StockPane.CanvasRect);
            var lenWorld = sz.Width;
            x += sign*lenWorld;
            if (x < left) x = left;
            else if (x > right) x = right;
            var resultLen = Math.Abs(x - x1);
            if (resultLen < 1)
            {
                span.linePoints[0] = new PointD(0, y);
                x = max > 0 ? max : 1;
            }
            span.AddPoint(x, y);
        }

        public void CopyLine(TrendLine sourceLine)
        {
            var newLine = new TrendLine(sourceLine);
            data.Add(newLine);
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            interObjects.AddRange(data);
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new TrendLine();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is TrendLine == false) return;
            data.Remove((TrendLine)interObject);
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
            return GetObjectUnderCursorPaneCoords(ptClient, tolerance);
        }

        private List<IChartInteractiveObject> GetObjectUnderCursorPaneCoords(Point ptClient, int tolerance)
        {
            var list = new List<IChartInteractiveObject>();
            foreach (var line in data)
            {
                if (!line.Completed) continue;
                var a = Conversion.WorldToScreen(new PointD(line.linePoints[0].X, line.linePoints[0].Y),
                    Owner.WorldRect, Owner.CanvasRect);
                var b = Conversion.WorldToScreen(new PointD(line.linePoints[1].X, line.linePoints[1].Y),
                    Owner.WorldRect, Owner.CanvasRect);
                var ptClientD = new PointD(ptClient.X, ptClient.Y);

                // проверить попадание в линию
                if (line.LineStyle == TrendLine.TrendLineStyle.Линия ||
                    line.LineStyle == TrendLine.TrendLineStyle.ЛинияСМаркерами)
                {
                    if (Geometry.IsDotOnLine(ptClientD, a, b, tolerance)) list.Add(line);
                    continue;
                }

                // проверить попадание в отрезок
                if (line.LineStyle == TrendLine.TrendLineStyle.Отрезок ||
                    line.LineStyle == TrendLine.TrendLineStyle.Стрелка ||
                    line.LineStyle == TrendLine.TrendLineStyle.ОтрезокСМаркерами)
                {
                    if (Geometry.IsDotOnSpan(ptClientD, a, b, tolerance)) list.Add(line);
                    continue;
                }

                // с прямоугольником все просто - попадание в область
                if (line.LineStyle == TrendLine.TrendLineStyle.Прямоугольник)
                {
                    if (Geometry.IsDotInArea(ptClientD, a, b, tolerance))
                        list.Add(line);
                    continue;
                }
                // маленький объект на свече
                if (line.LineStyle == TrendLine.TrendLineStyle.СвечнаяСтрелка)
                {
                    if (Geometry.IsDotInArea(ptClientD, 
                        new PointD(b.X - 1, b.Y), new PointD(b.X + 1, b.Y + 4), tolerance))
                            list.Add(line);
                    continue;
                }

                // проверить попадание в произвольную фигуру
                if (line.IsInObject(a, b, ptClient, tolerance))
                    list.Add(line);
                //if (Geometry.IsDotInArea(ptClientD, a, b, tolerance))
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner) {}

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

    
}