using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    // ReSharper disable LocalizableElement
    [LocalizedDisplayName("TitleEllipses")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorEllipse : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleEllipses"); } }

        public override BaseChartIndicator Copy()
        {
            var ell = new IndicatorEllipse();
            Copy(ell);
            return ell;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ell = (IndicatorEllipse) indi;
            CopyBaseSettings(ell);
            ell.LineColor = LineColor;
            ell.LineWidth = LineWidth;
            ell.LineStyle = LineStyle;
            ell.worldRect = worldRect;
            ell.canvasRect = canvasRect;
            ell.minQuoteEllipse = minQuoteEllipse;
            ell.countQuotes = countQuotes;
            ell.ellipseBuffer = ellipseBuffer;
            ell.visualEllipseBuffer = visualEllipseBuffer;
            ell.indexstart = indexstart;
            ell.indexend = indexend;
            ell.dir = dir;
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        #region Визуальные

        private Color lineColor = Color.Red;
        [LocalizedDisplayName("TitleLineColor")]
        [LocalizedDescription("MessageLineColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private decimal lineWidth = 1;
        [LocalizedDisplayName("TitleThickness")]
        [LocalizedDescription("MessageThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        #endregion

        private RectangleD worldRect;
        private RectangleD canvasRect;
        /// <summary>
        /// Минимальное количество свечек на графике
        /// </summary>
        private const int minQuotes = 10;
        /// <summary>
        /// Минимальное количество свечек эллипса
        /// </summary>
        private int minQuoteEllipse = 2;
        /// <summary>
        /// Количество свечек на графике
        /// </summary>
        private int countQuotes = 0;
        /// <summary>
        /// Список эллипсов
        /// </summary>
        private SeriesEllipse ellipseBuffer;
        /// <summary>
        /// Список эллипсов
        /// </summary>
        private SeriesEllipse visualEllipseBuffer;

        private int indexstart = 0;
        private int indexend = 0;
        int dir = 0;

        public IndicatorEllipse()
        {
            CreateOwnPanel = false;
        }

        #region расчет описывающего эллипса
        /// <summary>
        /// Получить описывающий визуальный
        /// </summary>
        /// <param name="candles"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private ChartEllipse BuildVisualInclusiveEllipse(StockSeriesData candles, int start, int end)
        {
            if (start == end) return null;
            float startvalue, endvalue;
            if (candles[start].high > candles[end].high)
            {
                startvalue = candles[start].high;
                endvalue = candles[end].low;
            }
            else
            {
                startvalue = candles[start].low;
                endvalue = candles[end].high;
            }
            var A = Conversion.WorldToScreen(new PointD(start, Convert.ToDouble(startvalue)), worldRect, canvasRect).ToPointF();
            var B = Conversion.WorldToScreen(new PointD(end, Convert.ToDouble(endvalue)), worldRect, canvasRect).ToPointF();
            var C = new PointF();

            double angle;
            float cx, cy, a, b;
            var S = float.NaN;
            var correctEllipse = false;
            // поиск охватывающего эллипса
            for (var i = start; i <= end; i++)
            {
                var d = Conversion.WorldToScreen(new PointD(i, Convert.ToDouble(candles[i].high)), worldRect, canvasRect).ToPointF();
                correctEllipse = Geometry.GetEllipseParams(A, B, d, out angle, out a, out b, out cx, out cy);
                if (float.IsNaN(S) || (correctEllipse && S < b))
                {
                    S = b;
                    C = d;
                }

                d = Conversion.WorldToScreen(new PointD(i, Convert.ToDouble(candles[i].low)), worldRect, canvasRect).ToPointF();
                correctEllipse = Geometry.GetEllipseParams(A, B, d, out angle, out a, out b, out cx, out cy);
                if (float.IsNaN(S) || (correctEllipse && S < b))
                {
                    S = b;
                    C = d;
                }
            }

            correctEllipse = // пересчитываем параметры эллипса
                Geometry.GetEllipseParams(A, B, C, out angle, out a, out b, out cx, out cy);
            if (correctEllipse)
            {   // можно построить эллипс - рисуем его
                var newEllipse = new ChartEllipse { BuildTangent = true, angle = angle, a = a, b = b, cx = cx, cy = cy };

                newEllipse.points.Add(Conversion.ScreenToWorld(new PointD(A.X, A.Y), worldRect, canvasRect).ToPointF());
                newEllipse.points.Add(Conversion.ScreenToWorld(new PointD(B.X, B.Y), worldRect, canvasRect).ToPointF());
                newEllipse.points.Add(Conversion.ScreenToWorld(new PointD(C.X, C.Y), worldRect, canvasRect).ToPointF());

                var minIndex = newEllipse.points.Min(p => p.X);
                newEllipse.DateStart = owner.StockSeries.GetCandleOpenTimeByIndex((int)minIndex);
                return newEllipse;
            }
            return null;
        }

        /// <summary>
        /// Получить описывающий 
        /// </summary>
        /// <param name="candles"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private ChartEllipse BuildInclusiveEllipse(StockSeriesData candles, int start, int end)
        {
            if (start == end) return null;
            float startvalue, endvalue;
            if (candles[start].high > candles[end].high)
            {
                startvalue = candles[start].high;
                endvalue = candles[end].low;
            }
            else
            {
                startvalue = candles[start].low;
                endvalue = candles[end].high;
            }
            var A = new PointD(start, Convert.ToDouble(startvalue));
            var B = new PointD(end, Convert.ToDouble(endvalue));
            var C = new PointD();

            double angle;
            float cx, cy, a, b;
            var S = float.NaN;
            bool correctEllipse = false;
            // поиск охватывающего эллипса
            for (var i = start; i <= end; i++)
            {
                var d = new PointD(i, Convert.ToDouble(candles[i].high));
                correctEllipse = Geometry.GetEllipseParams(A.ToPointF(), B.ToPointF(), d.ToPointF(), out angle, out a, out b, out cx, out cy);
                if (float.IsNaN(S) || (correctEllipse && S < b))
                {
                    S = b;
                    C = d;
                }

                d = new PointD(i, Convert.ToDouble(candles[i].low));
                correctEllipse = Geometry.GetEllipseParams(A.ToPointF(), B.ToPointF(), d.ToPointF(), out angle, out a, out b, out cx, out cy);
                if (float.IsNaN(S) || (correctEllipse && S < b))
                {
                    S = b;
                    C = d;
                }
            }

            correctEllipse = // пересчитываем параметры эллипса
                Geometry.GetEllipseParams(A.ToPointF(), B.ToPointF(), C.ToPointF(), out angle, out a, out b, out cx, out cy);
            if (correctEllipse)
            {   // можно построить эллипс - рисуем его
                var newEllipse = new ChartEllipse { BuildTangent = true, angle = angle, a = a, b = b, cx = cx, cy = cy };

                newEllipse.AddPoint((float)A.X, (float)A.Y);
                newEllipse.AddPoint((float)B.X, (float)B.Y);
                newEllipse.AddPoint((float)C.X, (float)C.Y);

                var minIndex = newEllipse.points.Min(p => p.X);
                newEllipse.DateStart = owner.StockSeries.GetCandleOpenTimeByIndex((int)minIndex);
                return newEllipse;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Пересчет эллипсов
        /// </summary>
        private void Recalc()
        {
            // проверка на начало расчетов
            if (ellipseBuffer.DataCount == 0)
            {
                indexstart = Low(minQuotes, minQuotes);
                indexend = High(minQuotes, minQuotes);
                dir = 1;
                if (indexend < indexstart)
                {
                    var tmp = indexend;
                    indexend = indexstart;
                    indexstart = tmp;
                    dir = -1;
                }

                var newEllipse = BuildInclusiveEllipse(owner.StockSeries.Data, indexstart, indexend);
                if (newEllipse != null)
                {
                    ellipseBuffer.data.Add(newEllipse);

                    newEllipse = BuildVisualInclusiveEllipse(owner.StockSeries.Data, indexstart, indexend);
                    if (newEllipse != null)
                        visualEllipseBuffer.data.Add(newEllipse);
                }
                countQuotes = indexend;
            }
            // теперь шагаем по одной свечке и все перерассчитываем
            while (countQuotes < owner.StockSeries.Data.Count)
            {
                if (ellipseBuffer.DataCount == 10)
                {
                    int a = 0;
                }
                if (ellipseBuffer.DataCount == 3)
                {
                    int a = 0;
                }

                #region 1 вариант - обновление эллипса
                var newend = 0;
                if (dir == 1)
                    newend = High(countQuotes - indexstart, countQuotes);
                else if (dir == -1)
                    newend = Low(countQuotes - indexstart, countQuotes);
                if (indexend != newend)
                {
                    // обновляем эллипс
                    var newEllipse = BuildInclusiveEllipse(owner.StockSeries.Data, indexstart, newend);
                    if (newEllipse != null)
                    {
                        indexend = newend;
                        // удаляем последний
                        ellipseBuffer.data.Remove(ellipseBuffer.data.Last());
                        ellipseBuffer.data.Add(newEllipse);

                        visualEllipseBuffer.data.Remove(visualEllipseBuffer.data.Last());
                        newEllipse = BuildVisualInclusiveEllipse(owner.StockSeries.Data, indexstart, newend);
                        if (newEllipse != null)
                            visualEllipseBuffer.data.Add(newEllipse);
                    }
                }
                #endregion
                #region 2 вариант - пересечение тенью свечи касательной - новый эллипс
                var ellipse = ellipseBuffer.data[ellipseBuffer.data.Count - 1];
                var ptLeft = ellipse.points[0];
                var ptRight = ellipse.points[1];
                // проверка на минимальное количество свечей
                if (ptRight.X - ptLeft.X >= minQuoteEllipse)
                {
                    var direct = ptLeft.Y > ptRight.Y
                                ? 1 // нижняя касательная для растущего эллипса
                                : -1; // верхняя для падающего
                    var m1 = new PointD(ellipse.cx + ellipse.b * Math.Sin(ellipse.angle),
                                        ellipse.cy + ellipse.b * Math.Cos(ellipse.angle));
                    var m2 = new PointD(ellipse.cx + ellipse.b * Math.Sin(ellipse.angle + Math.PI),
                                        ellipse.cy + ellipse.b * Math.Cos(ellipse.angle + Math.PI));
                    var m = ptLeft.Y < ptRight.Y
                                ? m1.Y < m2.Y ? m1 : m2 // нижняя касательная для растущего эллипса
                                : m1.Y < m2.Y ? m2 : m1; // верхняя для падающего
                    var r = new PointD(m.X - ellipse.cx, m.Y - ellipse.cy);
                    
                    var a = new PointD(ellipse.points[0].X + r.X, ellipse.points[0].Y + r.Y);
                    var b = new PointD(ellipse.points[1].X + r.X, ellipse.points[1].Y + r.Y);
                    // получить точку касательной на текущей свечке
                    var y = a.Y + ((countQuotes - a.X) * (b.Y - a.Y)) / (b.X - a.X);
                    if (double.IsInfinity(y)) y = double.MinValue;
                    // если тень выше/ниже касательной заводим новый эллипс
                    if (direct == 1)
                        if ((float)y < owner.StockSeries.Data[countQuotes].close)
                        {
                            // добавляем новый эллипс
                            var newEllipse = BuildInclusiveEllipse(owner.StockSeries.Data,
                                                                   Convert.ToInt32(ellipse.points[1].X), countQuotes);
                            if (newEllipse != null)
                            {
                                dir *= -1;
                                indexstart = Convert.ToInt32(ellipse.points[1].X);
                                indexend = countQuotes;
                                ellipseBuffer.data.Add(newEllipse);

                                newEllipse = BuildVisualInclusiveEllipse(owner.StockSeries.Data,
                                                                   Convert.ToInt32(ellipse.points[1].X), countQuotes);
                                if (newEllipse != null)
                                    visualEllipseBuffer.data.Add(newEllipse);
                            }
                        }
                    if (direct == -1)
                        if ((float)y > owner.StockSeries.Data[countQuotes].close)
                        {
                            // добавляем новый эллипс
                            var newEllipse = BuildInclusiveEllipse(owner.StockSeries.Data,
                                                                   Convert.ToInt32(ellipse.points[1].X), countQuotes);
                            if (newEllipse != null)
                            {
                                dir *= -1;
                                indexstart = Convert.ToInt32(ellipse.points[1].X);
                                indexend = countQuotes;
                                ellipseBuffer.data.Add(newEllipse);

                                newEllipse = BuildVisualInclusiveEllipse(owner.StockSeries.Data,
                                                                         Convert.ToInt32(ellipse.points[1].X),
                                                                         countQuotes);
                                if (newEllipse != null)
                                    visualEllipseBuffer.data.Add(newEllipse);
                            }
                        }
                }
                #endregion
                countQuotes++;
            }
        }

        private void RecalcVisualSeries()
        {
            worldRect = owner.StockPane.WorldRect;
            canvasRect = owner.StockPane.CanvasRect;
            visualEllipseBuffer.data.Clear();
            foreach (var ellipse in ellipseBuffer.data)
            {
                var newEllipse = BuildVisualInclusiveEllipse(owner.StockSeries.Data,
                                                                   Convert.ToInt32(ellipse.points[0].X), Convert.ToInt32(ellipse.points[1].X));
                if (newEllipse != null)
                    visualEllipseBuffer.data.Add(newEllipse);
            }
        }

        public void BuildSeries(ChartControl chart)
        {
            if (SeriesSources[0].GetType() == typeof(StockSeries))
            {
                var candles = ((StockSeries)SeriesSources[0]).Data.Candles;
                if (candles.Count < minQuotes) return;
                if (!(worldRect == DrawPane.WorldRect) || !(canvasRect == DrawPane.CanvasRect))
                {
                    if (ellipseBuffer.DataCount == 0) Recalc();
                    RecalcVisualSeries();
                } 
                /*if (!(WorldRect == chart.StockPane.WorldRect) || !(CanvasRect == chart.StockPane.CanvasRect))
                {
                    if (EllipseBuffer.Count == 0)
                        Recalc();
                    RecalcVisualSeries();
                } */   
            }
            
            Recalc();
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            
            countQuotes = chart.StockSeries.Data.Count;
            ellipseBuffer = new SeriesEllipse("Ellipses");
            visualEllipseBuffer = new SeriesEllipse("Ellipses");
            SeriesResult = new List<Series.Series>{visualEllipseBuffer};
            EntitleIndicator();
        }

        public void Remove()
        {
            visualEllipseBuffer.data.Clear();
            ellipseBuffer.data.Clear();
        }

        public void AcceptSettings()
        {
            if (DrawPane == null) return;
            worldRect = DrawPane.WorldRect;
            canvasRect = DrawPane.CanvasRect;
        }

        /// <summary>
        /// пересчитать индикатор для последней добавленной свечки
        /// </summary>        
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && newCandles.Count == 0) return;
            // объединить добавленную и новые свечи
            var candles = new List<CandleData>();
            if (updatedCandle != null) candles.Add(updatedCandle);
            candles.AddRange(newCandles);
            BuildSeries(owner);
        }

        /// <summary>
        /// Возвращает максимальное значение count свечек начиная с индекса shift
        /// в сторону уменьшения
        /// </summary>
        /// <param name="count"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        private int High(int count, int shift)
        {
            var index = shift;
            if (shift - count > owner.StockSeries.Data.Count) return 0;
            var high = owner.StockSeries.Data[shift].high;
            for (var i = shift - count; i <= shift; i++)
            {
                if (high >= owner.StockSeries.Data[i].high) continue;
                high = owner.StockSeries.Data[i].high;
                index = i;
            }
            return index;
        }

        /// <summary>
        /// Возвращает минимальное значение count свечек начиная с индекса shift
        /// в сторону уменьшения
        /// </summary>
        /// <param name="count"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        private int Low(int count, int shift)
        {
            var index = shift;
            if (shift - count > owner.StockSeries.Data.Count) return 0;
            var low = owner.StockSeries.Data[shift].low;
            for (var i = shift - count; i <= shift; i++)
            {
                if (low <= owner.StockSeries.Data[i].low) continue;
                low = owner.StockSeries.Data[i].low;
                index = i;
            }
            return index;
        }



        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }        
    }
    // ReSharper restore LocalizableElement
}
