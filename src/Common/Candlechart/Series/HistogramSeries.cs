using System;
using System.Collections.Generic;
using System.Drawing;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class HistogramSeries : Series, IPriceQuerySeries
    {
        public List<HistogramBar> data = new List<HistogramBar>();

        public override int DataCount { get { return data.Count; } }

        private int barWidth = 75;
        /// <summary>
        /// ширина бара, в %
        /// </summary>
        public int BarWidth
        {
            get { return barWidth; }
            set { barWidth = value; }
        }

        public HistogramSeries(string name) : base(name)
        {
        }

        #region Overrides of Series

        public override bool GetXExtent(ref double left, ref double right)
        {            
            return false;
        }

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            var start = Math.Max(0, (int) left);
            var end = Math.Min(data.Count, (int)right);
            if (start >= end) 
                return false;
            var result = false;
            for (var i = start; i < end; i++)
            {
                if (data[i].y > top)
                {
                    result = true;
                    top = data[i].y;
                }

                if (data[i].y < bottom)
                {
                    result = true;
                    bottom = data[i].y;
                }
            }
            return result;
        }

        #endregion

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            var size = SizeD.Round(Conversion.WorldToScreen(
                new SizeD(barWidth / 100.0, 0.0), worldRect, canvasRect));
            var barWd = Math.Max(size.Width, 2);

            var left = worldRect.Left <= 0 ? 0 : (int)worldRect.Left;
            var right = worldRect.Right >= data.Count ? data.Count - 1 : (int)worldRect.Right;
            if (right <= left) return;

            for (var i = left; i <= right; i++)
            //foreach (var bar in data)
            {
                var bar = data[i];
                Point pointTop =
                    Conversion.WorldToScreen(
                        new PointD(bar.index - 0.45, bar.y),
                        worldRect,
                        canvasRect).Round();
                Point pointBottom =
                    Conversion.WorldToScreen(
                                        new PointD(bar.index - 0.45, 0),
                                        worldRect,
                                        canvasRect).Round();
                using (var brush = new SolidBrush(bar.color))
                {
                    var low = Math.Min(pointTop.Y, pointBottom.Y);
                    var height = Math.Abs(pointTop.Y - pointBottom.Y);

                    g.FillRectangle(brush, pointTop.X - (barWd / 2), low,
                                barWd, height);
                }
            }
        }

        public float? GetPrice(int index)
        {
            if (index < 0) return null;
            if (index >= data.Count) return null;
            return (float)data[index].y;
        }
    }

    public struct HistogramBar
    {
        public int index;
        public double y;
        public Color color;
    }
}