using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    /// <summary>
    /// Отображение интервалов времени областями
    /// </summary>
    public class RegionSeries : Series
    {
        /// <summary>
        /// если флаг взведен - прозрачность региона задается в цвете самого региона
        /// иначе - принудительно (regionAlpha)
        /// </summary>
        public bool CustomAlphaChannel { get; set; }

        private double singleBarRegionWidth = 0.5d;
        public double SingleBarRegionWidth
        {
            get { return singleBarRegionWidth; }
            set { singleBarRegionWidth = value; }
        }

        private const int regionAlpha = 35;
        
        public List<BarRegion> data = new List<BarRegion>();

        public override int DataCount { get { return data.Count; } }

        public RegionSeries(string name)
            : base(name)
        {
        }

        /// <summary>
        /// рисовать как рамку 
        /// </summary>
        public bool DrawAsFrame { get; set; }

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
            DrawRegions(g, worldRect, canvasRect);
        }
        
        private void DrawRegions(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {            
            foreach (BarRegion r in data)
            {
                DrawRegion(r, g, worldRect, canvasRect);                         
            }            
        }

        private void DrawRegion(BarRegion r, Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            double x1 = r.IndexStart.HasValue ? r.IndexStart.Value :
                    r.Start.HasValue ? Chart.CandleRange.GetXCoord(r.Start.Value)
                    : worldRect.Left;
            double x2 = r.IndexEnd.HasValue ? r.IndexEnd.Value :
                r.End.HasValue ? Chart.CandleRange.GetXCoord(r.End.Value)
                : worldRect.Right;

            if (x2 == x2)
            {
                x1 -= singleBarRegionWidth;
                x2 += singleBarRegionWidth;
            }

            double y1 = r.UpperBound.HasValue ? r.UpperBound.Value : worldRect.Bottom;
            double y2 = r.LowerBound.HasValue ? r.LowerBound.Value : worldRect.Top;
            PointD p1 = Conversion.WorldToScreen(new PointD(x1, y1), worldRect, canvasRect);
            PointD p2 = Conversion.WorldToScreen(new PointD(x2, y2), worldRect, canvasRect);

            if (!DrawAsFrame)
            {
                var color = CustomAlphaChannel ? r.Color : Color.FromArgb(regionAlpha, r.Color);
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, (float) p1.X, (float) p1.Y, (float) p2.X - (float) p1.X,
                                    (float) p2.Y - (float) p1.Y);
                }
            }
            else
            {
                using (var pen = r.MakePen())
                {                    
                    g.DrawRectangle(pen, (float)p1.X, (float)p1.Y, (float)p2.X - (float)p1.X,
                                    (float)p2.Y - (float)p1.Y);
                }
            }
        }
    }

    /// <summary>
    /// Закрашенный прямоугольник, обозначающий временной интервал
    /// </summary>
    public class BarRegion
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public int? IndexStart { get; set; }
        public int? IndexEnd { get; set; }

        public Color Color { get; set; }
        /// <summary>
        /// опциональная верхняя граница
        /// </summary>
        public float? LowerBound { get; set; }
        /// <summary>
        /// опциональная нижняя граница
        /// </summary>
        public float? UpperBound { get; set; }
        /// <summary>
        /// стиль рисования рамки
        /// </summary>
        public BarRegionLineStyle lineStyle = BarRegionLineStyle.SolidThin;

        public Pen MakePen()
        {
            int width = (
                            lineStyle == BarRegionLineStyle.SolidThin ||
                            lineStyle == BarRegionLineStyle.DashThin ||
                            lineStyle == BarRegionLineStyle.DotThin) ? 1 : 2;
            DashStyle style = (lineStyle == BarRegionLineStyle.SolidThin ||
                               lineStyle == BarRegionLineStyle.SolidThick)
                                  ? DashStyle.Solid
                                  : (lineStyle == BarRegionLineStyle.DashThin ||
                                     lineStyle == BarRegionLineStyle.DashThick)
                                        ? DashStyle.Dash
                                        : DashStyle.Dot;
            return new Pen(Color, width) { DashStyle = style };
        }
    }

    public enum BarRegionLineStyle
    {
        SolidThin = 0, SolidThick, DashThin, DashThick, DotThin, DotThick
    }
}