using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class VolumeSeries : Series
    {
        // Fields
        private readonly SeriesData _data;
        public override int DataCount { get { return _data.Count; } }
        private float _percentBarWidth;

        // Methods
        public VolumeSeries(string name)
            : base(name)
        {
            _data = new SeriesData();
            UseUpDownColor = true;
        }

        // Properties
        internal override string CurrentPriceString
        {
            get
            {
                if (Data.BarCount != 0)
                {
                    var provider = new NumberFormatInfo
                                       {
                                           NumberDecimalDigits = NumberDecimalDigits
                                       };
                    return string.Format(provider, "({0:N})", new object[] {Data[Data.LastIndex]});
                }
                return string.Empty;
            }
        }

        public SeriesData Data
        {
            get { return _data; }
        }

        public float PercentBarWidth
        {
            get { return _percentBarWidth; }
            set
            {
                if ((value < 0f) || (value > 100f))
                {
                    throw new ArgumentOutOfRangeException("PercentBarWidth", "Percentage must be between 0 and 100.");
                }
                _percentBarWidth = value;
            }
        }

        public bool UseUpDownColor { get; set; }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            var pen = new Pen(ForeColor, LineWidth);
            var brush = new SolidBrush(BackColor);
            pen.Alignment = PenAlignment.Center;
            var pen2 = new Pen(Chart.StockSeries.UpLineColor, LineWidth);
            var pen3 = new Pen(Chart.StockSeries.DownLineColor, LineWidth);
            var brush2 = new SolidBrush(Chart.StockSeries.UpFillColor);
            var brush3 = new SolidBrush(Chart.StockSeries.DownFillColor);
            pen2.Alignment = PenAlignment.Center;
            pen3.Alignment = PenAlignment.Center;
            using (pen)
            {
                using (brush)
                {
                    using (pen2)
                    {
                        using (pen3)
                        {
                            using (brush2)
                            {
                                using (brush3)
                                {
                                    Pen pen4;
                                    double xValue;
                                    PointF tf;
                                    PointF tf2;
                                    if (PercentBarWidth < float.Epsilon)
                                    {
                                        for (int i = Data.StartIndex; i <= Data.LastIndex; i++)
                                        {
                                            xValue = i;
                                            tf =
                                                (PointF)
                                                Conversion.WorldToScreen(new PointD(xValue, 0.0), worldRect, canvasRect);
                                            tf2 =
                                                (PointF)
                                                Conversion.WorldToScreen(new PointD(xValue, Data[i]), worldRect,
                                                                         canvasRect);
                                            if (UseUpDownColor)
                                            {
                                                if (((i >= Chart.StockSeries.Data.StartIndex) &&
                                                     (i <= Chart.StockSeries.Data.LastIndex)) &&
                                                    (Chart.StockSeries.Data[i].open <=
                                                     Chart.StockSeries.Data[i].close))
                                                {
                                                    pen4 = pen2;
                                                }
                                                else
                                                {
                                                    pen4 = pen3;
                                                }
                                            }
                                            else
                                            {
                                                pen4 = pen;
                                            }
                                            g.DrawLine(pen4, tf, tf2);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = Data.StartIndex; j <= Data.LastIndex; j++)
                                        {
                                            SolidBrush brush4;
                                            xValue = j;
                                            double barLeftEdge = Chart.StockSeries.GetBarLeftEdge(j);
                                            double num5 = ((Chart.StockSeries.GetBarRightEdge(j) - barLeftEdge)*
                                                           PercentBarWidth)/200.0;
                                            tf =
                                                (PointF)
                                                Conversion.WorldToScreen(new PointD(xValue - num5, Data[j]), worldRect,
                                                                         canvasRect);
                                            tf2 =
                                                (PointF)
                                                Conversion.WorldToScreen(new PointD(xValue + num5, 0.0), worldRect,
                                                                         canvasRect);
                                            if (UseUpDownColor)
                                            {
                                                if (((j >= Chart.StockSeries.Data.StartIndex) &&
                                                     (j <= Chart.StockSeries.Data.LastIndex)) &&
                                                    (Chart.StockSeries.Data[j].open <=
                                                     Chart.StockSeries.Data[j].close))
                                                {
                                                    pen4 = pen2;
                                                    brush4 = brush2;
                                                }
                                                else
                                                {
                                                    pen4 = pen3;
                                                    brush4 = brush3;
                                                }
                                            }
                                            else
                                            {
                                                pen4 = pen;
                                                brush4 = brush;
                                            }
                                            g.FillRectangle(brush4, tf.X, tf.Y, tf2.X - tf.X, tf2.Y - tf.Y);
                                            g.DrawRectangle(pen4, tf.X, tf.Y, tf2.X - tf.X, tf2.Y - tf.Y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
            for (int i = Data.StartIndex; i <= Data.LastIndex; i++)
            {
                double xValue = i;                
                if ((xValue >= left) && (xValue <= right))
                {
                    flag = true;
                    if (Data[i] > top)
                    {
                        top = Data[i];
                    }
                }
            }
            if (0.0 < bottom)
            {
                bottom = 0.0;
            }
            return flag;
        }
    }
}