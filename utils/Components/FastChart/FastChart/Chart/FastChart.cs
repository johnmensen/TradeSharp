using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace FastChart.Chart
{
    public partial class FastChart : UserControl
    {
        #region Данные
        private readonly List<FastAxis> axes = new List<FastAxis>();

        [DisplayName("Оси")]
        [Category("Основные")]
        [Browsable(true)]
        public List<FastAxis> Axes
        {
            get { return axes; }            
        }

        public readonly List<FastSeries> series = new List<FastSeries>();

        public Bitmap backgroundImage;
        
        private Point backgroundImageCoords = new Point(5, 5);
        [DisplayName("Координаты картинки")]
        [Category("Оформление")]
        [Browsable(true)]
        public string BackgroundImageCoords
        {
            get { return string.Format("{0};{1}", backgroundImageCoords.X, backgroundImageCoords.Y); }
            set
            {
                var matches = regexNumber.Matches(value);
                if (matches.Count != 2) return;
                backgroundImageCoords = new Point(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
            }
        }

        /// <summary>
        /// опциональная левая граница отображаемых данных
        /// </summary>
        public int? IndexStart { get; set; }

        /// <summary>
        /// опциональная правая граница отображаемых данных
        /// </summary>
        public int? IndexStop  { get; set; }

        /// <summary>
        /// опциональная левая граница отображаемых данных
        /// </summary>
        public DateTime? TimeStart { get; set; }

        /// <summary>
        /// опциональная правая граница отображаемых данных
        /// </summary>
        public DateTime? TimeStop { get; set; }

        #endregion

        #region Служебные
        private readonly Regex regexNumber = new Regex("[\\-\\d]+");
        public string lastError;
        #endregion

        public void LoadBackImageFromResource(Assembly asm, string resourcePath)
        {
            var imageStream = asm.GetManifestResourceStream(resourcePath);
            if (imageStream != null)
                backgroundImage = new Bitmap(imageStream);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode && firstDesignTimeUse)
            {
                ConstructSample();
                firstDesignTimeUse = false;
            }

            // область графика
            DrawArea(e);

            var legendSz = DrawLegend(e, this);

            // масштабировать оси
            AutoScaleAxis();

            // нарисовать оси и данные
            DrawAxesAndSeries(e, legendSz);            

            // сообщение об ошибке?
            if (!string.IsNullOrEmpty(lastError))
            {
                using (var brush = new SolidBrush(Color.Red))
                {
                    e.Graphics.DrawString(lastError, Font, brush, 1, 1);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void AutoScaleAxis()
        {
            foreach (var ax in axes)                
                if (ax.AutoScale100)
                {
                    ax.MinValue = new Cortege2<double, DateTime>(double.MaxValue, DateTime.MaxValue);
                    ax.MaxValue = new Cortege2<double, DateTime>(double.MinValue, DateTime.MinValue);
                }
            
            foreach (var sr in series)
            {
                if (sr.points.Count == 0 || sr.axisX == null || sr.axisY == null) continue;
                var srAxes = new [] {sr.axisX, sr.axisY};
                foreach (var srAx in srAxes)
                {
                    if (!srAx.AutoScale100) continue;
                    if (!srAx.IsDateTime)
                    {
                        var min = srAx.Direction == FastAxisDirection.X
                                      ? sr.points.Min(p => (double) p.x)
                                      : sr.points.Min(p => p.y);
                        var max = srAx.Direction == FastAxisDirection.X
                                      ? sr.points.Max(p => (double)p.x)
                                      : sr.points.Max(p => p.y);
                        if (srAx.MinValue.a > min)
                            srAx.MinValue = new Cortege2<double, DateTime>(min, DateTime.MinValue);
                        if (srAx.MaxValue.a < max)
                            srAx.MaxValue = new Cortege2<double, DateTime>(max, DateTime.MaxValue);
                    }
                    else
                    {
                        var min = sr.points.Min(p => (DateTime) p.x);
                        var max = sr.points.Max(p => (DateTime)p.x);
                        if (srAx.MinValue.b > min)
                            srAx.MinValue = new Cortege2<double, DateTime>(double.MinValue, min);
                        if (srAx.MaxValue.b < max)
                            srAx.MaxValue = new Cortege2<double, DateTime>(double.MaxValue, max);
                    }
                }
            }
        }

        private void DrawArea(PaintEventArgs e)
        {
            if (UseGradient)
            {
                brushArea = new LinearGradientBrush(new Rectangle(0, 0, Width, Height),
                                                    clGradStart, clGradEnd, gradAngle);                
            }
            // залить область графика
            e.Graphics.FillRectangle(brushArea, 0, 0, Width - 1, Height - 1);            
            
            // картинка
            if (backgroundImage != null)
            {
                e.Graphics.DrawImage(backgroundImage, backgroundImageCoords.X, backgroundImageCoords.Y,
                    backgroundImage.Width, backgroundImage.Height);
            }
            
            // нарисовать границу
            if (DrawMargin)
            {
                e.Graphics.DrawRectangle(penMargin, 
                    penMargin.Width / 2, penMargin.Width / 2,
                    Width - penMargin.Width, Height - penMargin.Width);
            }
        }

        private void DrawAxesAndSeries(PaintEventArgs e, Size szLegend)
        {
            int legendWd = LegendPlacement == ChartLegendPlacement.Справа
                               ? szLegend.Width : 0;
            int legendHt = LegendPlacement == ChartLegendPlacement.Вверху
                               ? szLegend.Height : 0;
            
            int YaxWd = 40;
            int XaxHt = 40;
            var horzAx = axes.FindAll(a => a.Direction == FastAxisDirection.X);
            if (horzAx.Count(ax => ax.VerticalText) > 0) XaxHt = 55;
            var vertAx = axes.FindAll(a => a.Direction == FastAxisDirection.Y);
            if (vertAx.Count(ax => ax.VerticalText == false) == 0) YaxWd = 35;
            var axYwd = vertAx.Count * YaxWd;
            var axXht = horzAx.Count * XaxHt;


            int left = Padding + axYwd,
                top = Padding + legendHt,
                right = Width - Padding - legendWd,
                bottom = Height - Padding - axXht;
            var rectArea = new Rectangle(left, top, right - left, bottom - top);
                        
            var x = left;
            foreach (var ax in vertAx)
            {
                ax.Draw(this, e, new Point(x, bottom), new Point(x, top), rectArea.Width);
                x -= YaxWd;
            }

            var y = bottom;
            foreach (var ax in horzAx)
            {
                ax.Draw(this, e, new Point(left, y), new Point(right, y), rectArea.Height);
                y += XaxHt;
            }

            // серии            
            foreach (var sr in series)
            {
                sr.Draw(e, rectArea);
            }
        }

        #region design time
        private bool firstDesignTimeUse = true;
        private void ConstructSample()
        {
            return;
            // оси
            //var axX = new FastAxis(FastAxisDirection.X, true) 
            //{ 
            //    AutoScale100 = false,
            //    MinValue = new Cortege2<double, DateTime>(0, DateTime.Now),
            //    MaxValue = new Cortege2<double, DateTime>(0, DateTime.Now.AddDays(45)),
            //    DrawMainGrid = true,
            //    VerticalText = true,
            //    MinPixelsPerPoint = 25
            //};
            //axes.Add(axX);
            //var axY = new FastAxis(FastAxisDirection.Y, false)
            //         {
            //             AutoScale100 = false,
            //             MinValue = new Cortege2<double, DateTime>(0, DateTime.Now),
            //             MaxValue = new Cortege2<double, DateTime>(1, DateTime.Now),
            //             DrawMainGrid = true
            //         };
            //axes.Add(axY);
        }
        #endregion
    }
}
