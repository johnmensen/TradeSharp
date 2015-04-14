using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FastChart.Chart
{
    public enum FastSeriesType
    {
        Линия
    }

    public struct FastSeriesPoint
    {
        public object x;
        public double y;
        public FastSeriesPoint(DateTime x, double y) : this()
        {
            this.x = x;
            this.y = y;
        }
        public FastSeriesPoint(double x, double y)
            : this()
        {
            this.x = x;
            this.y = y;
        }
    }
    
    public class FastSeries
    {
        public string Name { get; set; }
        private FastSeriesType seriesType = FastSeriesType.Линия;
        public FastSeriesType SeriesType
        {
            get { return seriesType; }
            set { seriesType = value; }
        }
        public FastAxis axisX, axisY;
        public bool AutoSortX { get; set; }

        public Pen PenLine { get; set; }

        public bool AntiAlias { get; set; }

        public List<FastSeriesPoint> points = new List<FastSeriesPoint>();

        public FastSeries(string name, FastSeriesType _seriesType,
            FastAxis _axisX, FastAxis _axisY, bool autoSortX)
        {
            Name = name;
            seriesType = _seriesType;
            axisX = _axisX;
            axisY = _axisY;
            AutoSortX = autoSortX;
        }

        public void AddPoint(DateTime x, double y)
        {
            if (!AutoSortX) points.Add(new FastSeriesPoint(x, y));
            var index = points.FindIndex(0, pt => (DateTime)pt.x > x);
            if (index >= 0)
                points.Insert(index, new FastSeriesPoint(x, y));
            else
                points.Add(new FastSeriesPoint(x, y));
        }

        public void AddPoint(double x, double y)
        {
            if (!AutoSortX) points.Add(new FastSeriesPoint(x, y));
            var index = points.FindIndex(0, pt => (double)pt.x > x);
            if (index >= 0)
                points.Insert(index, new FastSeriesPoint(x, y));
            else
                points.Add(new FastSeriesPoint(x, y));
        }
    
        public void Draw(PaintEventArgs e, Rectangle seriesArea)
        {
            if (seriesType == FastSeriesType.Линия)
                DrawLineSeries(e, seriesArea);
        }

        private void DrawLineSeries(PaintEventArgs e, Rectangle seriesArea)
        {
            float? prevX = null, prevY = null;

            var scaleX = axisX.IsDateTime
                             ? seriesArea.Width/(axisX.MaxValue.b - axisX.MinValue.b).TotalMilliseconds
                             : seriesArea.Width/(axisX.MaxValue.a - axisX.MinValue.a);
            var scaleY = axisY.IsDateTime
                             ? seriesArea.Height/(axisY.MaxValue.b - axisY.MinValue.b).TotalMilliseconds
                             : seriesArea.Height/(axisY.MaxValue.a - axisY.MinValue.a);

            if (double.IsNaN(scaleX) || double.IsNaN(scaleY)
                || double.IsInfinity(scaleX) || double.IsInfinity(scaleY)) return;

            var pen = PenLine ?? new Pen(Color.Black);
            var oldMode = e.Graphics.SmoothingMode;
            if (AntiAlias)
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; 
            foreach (var pt in points)
            {                
                // расчитать X-координату
                double x = axisX.IsDateTime
                               ? ((DateTime) pt.x - axisX.MinValue.b).TotalMilliseconds*scaleX
                               : ((double) pt.x - axisX.MinValue.a)*scaleX;
                x += seriesArea.Left;
                // расчитать Y-координату
                double y = (pt.y - axisY.MinValue.a) * scaleY;
                y = seriesArea.Bottom - y;

                if (prevX.HasValue)
                {
                    e.Graphics.DrawLine(pen, (float)x, (float)y, prevX.Value, prevY.Value);
                }
                prevX = (float)x;
                prevY = (float)y;
            }
            if (AntiAlias)
                e.Graphics.SmoothingMode = oldMode; 
        }        
    }
}
