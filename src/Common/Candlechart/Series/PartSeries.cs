using System.Collections.Generic;
using System.Drawing;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    /// <summary>
    /// Серия ломаных
    /// </summary>
    public class PartSeries : Series
    {       
        /// <summary>
        /// отрезки (индекс данных - цена)
        /// </summary>
        public List<List<PartSeriesPoint>> parts = new List<List<PartSeriesPoint>>();

        public override int DataCount { get { return parts.Count; } }

        private Color lineColor = Color.Black;
        /// <summary>
        /// цвет линии
        /// </summary>
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private float lineThickness = 1;
        /// <summary>
        /// толщина линии
        /// </summary>
        public float LineThickness
        {
            get { return lineThickness; }
            set { lineThickness = value; }
        }

        private float markerRadius;
        /// <summary>
        /// радиус окружности-маркера (0 - не рисуется)
        /// </summary>
        public float MarkerRadius
        {
            get { return markerRadius; }
            set { markerRadius = value; }
        }

        public PartSeries(string name)
            : base(name)
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
            DrawParts(g, worldRect, canvasRect);
        }

        private void DrawParts(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var linePen = new Pen(LineColor, LineThickness))
            {
                using (var markerPen = new Pen(LineColor, 1))
                {
                    using (var markerBrush = new SolidBrush(Color.White))
                    {
                        foreach (var chain in parts)
                        {
                            DrawRegion(chain, g, worldRect, canvasRect, linePen, markerPen, markerBrush);
                        }
                    }
                }
            }
        }

        private void DrawRegion(List<PartSeriesPoint> chain, Graphics g, 
            RectangleD worldRect, Rectangle canvasRect, 
            Pen linePen, Pen markerPen, Brush markerBrush)
        {
            var linePoints = new List<PointF>();
            for (var i = 0; i < chain.Count; i++)
            {
                var p = Conversion.WorldToScreen(
                    new PointD(chain[i].index - 0.5, (double) chain[i].quote), worldRect, canvasRect);
                linePoints.Add(new PointF((float)p.X, (float)p.Y));
            }
            g.DrawLines(linePen, linePoints.ToArray());
            if (MarkerRadius > 0)
            {
                foreach (var point in linePoints)
                {
                    var ellipseRect = new RectangleF(
                        point.X - MarkerRadius, point.Y - MarkerRadius,
                        MarkerRadius*2, MarkerRadius*2);
                    g.FillEllipse(markerBrush, ellipseRect);
                    g.DrawEllipse(markerPen, ellipseRect);
                }
            }
        }
    }

    public struct PartSeriesPoint
    {
        public int index;
        public decimal quote;
        public PartSeriesPoint(int index, decimal quote)
        {
            this.index = index;
            this.quote = quote;
        }
    }
}