using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    // часы с 12/24 делениями; могут отображать радиусы и сектора
    public partial class ChronometerWatch : UserControl
    {
        private readonly List<Cortege2<DateTime, Color>> times = new List<Cortege2<DateTime, Color>>();
        private readonly List<Cortege3<DateTime, int, Color>> intervals = new List<Cortege3<DateTime, int, Color>>();

        private int hourCount = 24;
        public int HourCount
        {
            get { return hourCount; }
            set
            {
                hourCount = value == 12 ? 12 : 24;
                Invalidate();
            }
        }

        public ChronometerWatch()
        {
            InitializeComponent();
        }

        // добавление отметки-радиуса
        public void AddTime(DateTime time, Color color)
        {
            times.Add(new Cortege2<DateTime, Color>(time, color));
            Invalidate();
        }

        public void ClearTimes()
        {
            times.Clear();
            Invalidate();
        }

        // добавление отметки-сектора
        public void AddInterval(DateTime time, int intervalMinutes, Color color)
        {
            intervals.Add(new Cortege3<DateTime, int, Color>(time, intervalMinutes, color));
            Invalidate();
        }

        public void ClearIntervals()
        {
            intervals.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var width = Width;
            var height = Height;
            if (Width < Height)
                height = Width;
            else
                width = Height;
            var borderPen = new Pen(Color.Gray);
            //var borderPen = new Pen(Color.DarkGray);
            var scalePen = new Pen(Color.Black);
            var borderRect = new Rectangle(0, 0, width - 1, height - 1);
            var outerBorderRect = borderRect;
            const double borderThickness = 0.02;
            var margin = borderThickness;
            var innerBorderRect = new Rectangle((int)(width * margin), (int)(height * margin), (int)(width * (1 - margin * 2)), (int)(height * (1 - margin * 2)));
            const double scaleMargin = 0.05;
            margin += scaleMargin;
            var outerScaleRect = new Rectangle((int)(width * margin), (int)(height * margin), (int)(width * (1 - margin * 2)), (int)(height * (1 - margin * 2)));
            const double scaleThickness = 0.02;
            margin += scaleThickness;
            var innerScaleRect = new Rectangle((int)(width * margin), (int)(height * margin), (int)(width * (1 - margin * 2)), (int)(height * (1 - margin * 2)));
            const double textMargin = 0.05;

            // borders
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new LinearGradientBrush(outerBorderRect, Color.LightGray, Color.Gray, LinearGradientMode.ForwardDiagonal), outerBorderRect);
            e.Graphics.DrawEllipse(borderPen, outerBorderRect);
            e.Graphics.FillEllipse(new LinearGradientBrush(innerBorderRect, Color.Gray, Color.LightGray, LinearGradientMode.ForwardDiagonal), innerBorderRect);
            e.Graphics.DrawEllipse(borderPen, innerBorderRect);
            e.Graphics.DrawEllipse(scalePen, outerScaleRect);
            e.Graphics.DrawEllipse(scalePen, innerScaleRect);
            e.Graphics.DrawEllipse(scalePen, width / 2 - 1, height / 2 - 1, 2, 2);

            var srcMatrix = e.Graphics.Transform;
// ReSharper disable PossibleLossOfFraction
            e.Graphics.TranslateTransform(width / 2, height / 2);
// ReSharper restore PossibleLossOfFraction
            e.Graphics.RotateTransform(-90);

            // intervals & times
            var degreesPerHour = 360 / hourCount;
            innerScaleRect.X = -innerScaleRect.Width / 2;
            innerScaleRect.Y = -innerScaleRect.Height / 2;
            foreach (var interval in intervals)
                e.Graphics.FillPie(new SolidBrush(interval.c), innerScaleRect,
                                   (float) (interval.a.Hour + interval.a.Minute / 60.0) * degreesPerHour,
                                   (float) (interval.b / 60.0) * degreesPerHour);
            var scaleX = (int)(width * (0.5 - borderThickness - scaleMargin - scaleThickness));
            foreach (var time in times)
            {
                var state = e.Graphics.Save();
                e.Graphics.RotateTransform((float)(time.a.Hour + time.a.Minute / 60.0) * degreesPerHour);
                e.Graphics.DrawLine(new Pen(time.b), 0, 0, scaleX, 0);
                e.Graphics.Restore(state);
            }

            // scale & scale text
            var textX = (int) (width * (0.5 - borderThickness - scaleMargin - scaleThickness - textMargin));
            for (var i = 1; i <= hourCount; i++)
            {
                e.Graphics.RotateTransform(degreesPerHour);
                e.Graphics.DrawLine(scalePen, scaleX, 0, scaleX + (int)(width * scaleThickness), 0);
                if (hourCount == 24 && i % 2 != 0)
                    continue;
                var textSize = e.Graphics.MeasureString(i.ToString(), Font);
                var matrix = e.Graphics.Transform;
                Point[] points = {new Point(textX, 0)};
                e.Graphics.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, points);
                e.Graphics.Transform = srcMatrix;
                var x = points[0].X;
                var y = points[0].Y;
                e.Graphics.DrawString(i.ToString(), Font, new SolidBrush(Color.Black), x - textSize.Width / 2,
                                      y - textSize.Height / 2);
                e.Graphics.Transform = matrix;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
