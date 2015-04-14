using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class LineSeries : Series, IPriceQuerySeries
    {
        public double ShiftX { get; set; }

        private readonly SeriesData _data;

        public override int DataCount { get { return _data.Count; } }
        
        public LineSeries(string name)
            : base(name)
        {
            _data = new SeriesData();
        }

        internal override string CurrentPriceString
        {
            get
            {
                if (Data.BarCount != 0)
                {
                    var provider = new NumberFormatInfo {NumberDecimalDigits = NumberDecimalDigits};
                    return string.Format(provider, "({0:N})", new object[] { Data[Data.LastIndex] });
                }
                return string.Empty;
            }
        }

        public bool Hidden { get; set; }

        public SeriesData Data
        {
            get { return _data; }
        }

        #region Визуальные - линия, тень
        public bool Transparent { get; set; }

        public Color? LineColor { get; set; }

        private DashStyle? lineDashStyle = DashStyle.Solid;
        public DashStyle? LineDashStyle
        {
            get { return lineDashStyle; }
            set { lineDashStyle = value; }
        }

        private float[] dashPattern = new float[] { 4, 4 };
        public float[] DashPattern
        {
            get { return dashPattern; }
            set { dashPattern = value; }
        }

        public bool DrawShadow { get; set; }

        private Color shadowColor = Color.LightSlateGray;
        public Color ShadowColor { get { return shadowColor; } set { shadowColor = value; } }

        private int shadowWidth = 3;
        public int ShadowWidth
        {
            get { return shadowWidth; }
            set { shadowWidth = value; }
        }

        private int shadowAlpha = 70;
        public int ShadowAlpha
        {
            get { return shadowAlpha; }
            set { shadowAlpha = value; }
        }

        #endregion

        /// <summary>
        /// расстояние (в точках), через которое рисуются засечки
        /// 0 - засечки не ставятся
        /// </summary>
        public int MarkerSpanPoints
        {
            get; set;
        }

        private int markerSpanPointsMultiplier = 5;
        public int MarkerSpanPointsMultiplier
        {
            get { return markerSpanPointsMultiplier; }
            set { markerSpanPointsMultiplier = value; }
        }
        /// <summary>
        /// если расст. между маркерами оказывается меньше указанного,
        /// расстояние между маркерами умножается на markerSpanPointsMultiplier (5)
        /// </summary>
        public int MinPixelsBetweenMarkers
        {
            get; set;
        }

        private Color colorMarker = Color.DimGray;
        public Color ColorMarker
        {
            get { return colorMarker; }
            set { colorMarker = value; }
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            if (Hidden) return;
            base.Draw(g, worldRect, canvasRect);
            var pen = new Pen(LineColor ?? ForeColor, LineWidth);            
            var brush = new SolidBrush(BackColor);
            var path = new GraphicsPath();
            var path2 = new GraphicsPath();
            pen.Alignment = PenAlignment.Center;
            if (LineDashStyle.HasValue)
                pen.DashStyle = LineDashStyle.Value;
            else
                pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), LineStyle.ToString());
            if (pen.DashStyle == DashStyle.Custom)
                pen.DashPattern = dashPattern;
            
            var markerPen = new Pen(ColorMarker);

            // измерить расстояние между маркерами в пикселях
            var markerSpanPointsView = MarkerSpanPoints;
            if (MinPixelsBetweenMarkers > 0)
            {
                for (var i = 0; i < 10; i++)
                {
                    var sizeUnit = Conversion.WorldToScreen(new SizeD(markerSpanPointsView, 0), worldRect, canvasRect);
                    if (sizeUnit.Width >= MinPixelsBetweenMarkers) break;
                    markerSpanPointsView *= markerSpanPointsMultiplier;
                }                
            }
            
            using (pen)
            {
                using (brush)
                {
                    using (path)
                    {
                        using (path2)
                        {
                            using (markerPen)
                            {
                                PointF tf;
                                PointF tf2;
                                if (Data.BarCount > 0)
                                {
                                    tf =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD(Data.StartIndex,
                                                       Data[Data.StartIndex]), worldRect, canvasRect);

                                    path2.AddLine(tf.X, canvasRect.Bottom, tf.X, tf.Y);
                                }
                                // разлиновать
                                if (Data.StartIndex < Data.LastIndex && markerSpanPointsView > 0)
                                {                                    
                                    for (var i = Data.StartIndex; i <= Data.LastIndex; i += markerSpanPointsView)
                                    {
                                        var pointTop = (PointF) Conversion.WorldToScreen(
                                                                    new PointD(
                                                                        i + ShiftX,
                                                                        0),
                                                                    worldRect, canvasRect);
                                        pointTop.Y = canvasRect.Top;
                                        var pointBottom = new PointF(pointTop.X, canvasRect.Bottom);
                                        g.DrawLine(markerPen, pointTop, pointBottom);
                                    }                                    
                                }

                                // построить график
                                var startIndex = Math.Max(Data.StartIndex, (int)Chart.StockPane.WorldRect.Left);
                                var endIndex = Math.Min(Data.LastIndex, (int) Chart.StockPane.WorldRect.Right);

                                for (var i = startIndex + 1; i <= endIndex; i++)
                                {
                                    if (double.IsNaN(Data[i - 1]) || double.IsNaN(Data[i]))
                                    {
                                        g.DrawPath(pen, path);
                                        path.Reset();
                                        continue;
                                    }
                                    tf =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD((i - 1 - 1) + ShiftX,
                                                       Data[i - 1]),
                                            worldRect,
                                            canvasRect);
                                    tf2 =
                                        (PointF)
                                        Conversion.WorldToScreen(new PointD(i - 1 + ShiftX,
                                                                            Data[i]), worldRect, canvasRect);                                                                        

                                    path2.AddLine(tf, tf2);
                                    path.AddLine(tf, tf2);
                                }
                                if (Data.BarCount > 0)
                                {
                                    if (Data[Data.LastIndex] != double.NaN)
                                    {
                                       tf2 =
                                            (PointF)
                                            Conversion.WorldToScreen(
                                                new PointD(Data.LastIndex,
                                                           Data[Data.LastIndex]), worldRect, canvasRect);
                                        path2.AddLine(tf2.X, tf2.Y, tf2.X, canvasRect.Bottom);
                                    }
                                }
                                try
                                {
                                    if (!Transparent)
                                        g.FillPath(brush, path2);
                                    if (DrawShadow)
                                        DrawShadowPath(path, g);
                                    g.DrawPath(pen, path);
                                }
                                catch
                                {
                                }                                
                            }
                        }
                    }
                }
            }
        }

        private void DrawShadowPath(GraphicsPath path, Graphics g)
        {
            if (path.PointCount == 0) return;
            var points = new PointF[path.PointCount];
            var types = new byte[path.PointCount];

            var offset = shadowWidth / 2;
            offset = offset < 1 ? 1 : offset;
            for (var i = 0; i < path.PointCount; i++)
            {
                var pt = path.PathPoints[i];
                points[i] = new PointF(pt.X + offset, pt.Y + offset);
                types[i] = path.PathTypes[i];
            }
            var shadowPath = new GraphicsPath(points, types);
            using (var pen = new Pen(new SolidBrush(Color.FromArgb(shadowAlpha, shadowColor)), shadowWidth))
            {
                g.DrawPath(pen, shadowPath);
            }            
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            if (Data.BarCount <= 0)
            {
                return false;
            }
            if (Chart.StockSeries.GetBarLeftEdge(Data.StartIndex) < left)
            {
                left = Chart.StockSeries.GetBarLeftEdge(Data.StartIndex);
            }
            if (Chart.StockSeries.GetBarRightEdge(Data.LastIndex) > right)
            {
                right = Chart.StockSeries.GetBarRightEdge(Data.LastIndex);
            }
            return true;
        }

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            if (Data.BarCount <= 0)
            {
                return false;
            }
            bool flag = false;
            int startIndex = Data.StartIndex;
            double xValue = startIndex;
            if ((xValue >= left) && (xValue <= right))
            {
                flag = true;
                if (Data[startIndex] > top)
                {
                    top = Data[startIndex];
                }
                if (Data[startIndex] < bottom)
                {
                    bottom = Data[startIndex];
                }
            }
            for (startIndex = Data.StartIndex + 1; startIndex <= Data.LastIndex; startIndex++)
            {
                double num = (startIndex - 1);
                //xValue = Chart.StockSeries.GetXValue(startIndex);
                if (((startIndex + 1) >= left) && (num <= right))
                {
                    flag = true;
                    if (Data[startIndex] > top)
                    {
                        top = Data[startIndex];
                    }
                    if (Data[startIndex] < bottom)
                    {
                        bottom = Data[startIndex];
                    }
                }
            }
            startIndex = Data.LastIndex;
            xValue = startIndex;
            if ((xValue >= left) && (xValue <= right))
            {
                flag = true;
                if (Data[startIndex] > top)
                {
                    top = Data[startIndex];
                }
                if (Data[startIndex] < bottom)
                {
                    bottom = Data[startIndex];
                }
            }
            return flag;
        }

        public float? GetPrice(int index)
        {
            if (index < 0) return null;
            if (index >= Data.Count) return null;
            return (float)Data[index];
        }
    }
}
