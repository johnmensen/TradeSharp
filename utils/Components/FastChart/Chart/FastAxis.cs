using System;
using System.Drawing;
using System.Windows.Forms;

namespace FastChart.Chart
{
    public enum FastAxisDirection
    {
        X, Y
    }
    public class FastAxis
    {
        public bool AutoScale100 { get; set; }
        public FastAxisDirection Direction { get; set; }
        public bool IsDateTime { get; set; }
        public Cortege2<double, DateTime> MinValue { get; set; }
        public Cortege2<double, DateTime> MaxValue { get; set; }
        public bool AlwaysShowNil { get; set; }
        public string LabelFormat { get; set; }
        public Cortege2<double, TimeSpan>? Step { get; set; }

        private int minPixelsPerPoint = 40;
        public int MinPixelsPerPoint
        {
            get { return minPixelsPerPoint; }
            set { minPixelsPerPoint = value; }
        }

        private Color lineColor = Color.Black;
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        public bool VerticalText { get; set; }

        public FastAxis(FastAxisDirection _direction, bool isDateTime)
        {
            Direction = _direction;
            IsDateTime = isDateTime;
        }

        public void Draw(PaintEventArgs e, Point o, Point tip)
        {
            var start = !AlwaysShowNil ? MinValue : new Cortege2<double, DateTime>(0, DateTime.MinValue);
            var step = CalculateStep(start, o, tip);

        }

        private Cortege2<double, TimeSpan> CalculateStep(Cortege2<double, DateTime> start, Point o, Point tip)
        {
            if (Step.HasValue) return Step.Value;
            var len = Direction == FastAxisDirection.X ? Math.Abs(tip.X - o.X) : Math.Abs(tip.Y - o.Y);
            var numMinSteps = len / minPixelsPerPoint;

            if (!IsDateTime)
            {
                var fractStep = (MaxValue.a - start.a) / numMinSteps;
                Math.


            }

            var fractSpan = (MaxValue.b - start.b).TotalSeconds / numMinSteps;
            if (fractSpan < 1) return new Cortege2<double, TimeSpan>(0, new TimeSpan(0, 0, 0, 0, (int)(fractSpan * 1000)));
            if (fractSpan < 60)
                return new Cortege2<double, TimeSpan>(0, new TimeSpan(0, 0, 5 * ((int)fractSpan / 5)));

        }

        private static double ToFPParts()
        {
            
        }
    }
}
