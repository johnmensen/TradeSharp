using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class ChannelSeries : Series
    {
        // Fields
        private readonly ChannelSeriesData _data;

        public override int DataCount { get { return _data.Count; } }

        // Methods
        public ChannelSeries(string name)
            : base(name)
        {
            _data = new ChannelSeriesData();
        }

        internal override string CurrentPriceString
        {
            get
            {
                if (Data.BarCount != 0)
                {
                    var provider = new NumberFormatInfo {NumberDecimalDigits = NumberDecimalDigits};
                    return string.Format(provider, "({0:N}, {1:N})",
                                         new object[] { Data.Top[Data.LastIndex], Data.Bottom[Data.LastIndex] });
                }
                return string.Empty;
            }
        }

        public ChannelSeriesData Data
        {
            get { return _data; }
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            var pen = new Pen(ForeColor, LineWidth);
            var brush = new SolidBrush(BackColor);
            var path = new GraphicsPath();
            var path2 = new GraphicsPath();
            var path3 = new GraphicsPath();
            pen.Alignment = PenAlignment.Center;
            pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), LineStyle.ToString());
            using (pen)
            {
                using (brush)
                {
                    using (path)
                    {
                        using (path2)
                        {
                            using (path3)
                            {
                                PointF tf;
                                PointF tf2;
                                for (int i = Data.StartIndex + 1; i <= Data.LastIndex; i++)
                                {
                                    tf =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD(i - 1, Data.Top[i - 1]),
                                            worldRect, canvasRect);
                                    tf2 =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD(i, Data.Top[i]), worldRect,
                                            canvasRect);
                                    path3.AddLine(tf, tf2);
                                    path.AddLine(tf, tf2);
                                }
                                for (int j = Data.LastIndex - 1; j >= Data.StartIndex; j--)
                                {
                                    tf =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD(j + 1, Data.Bottom[j + 1]),
                                            worldRect, canvasRect);
                                    tf2 =
                                        (PointF)
                                        Conversion.WorldToScreen(
                                            new PointD(j, Data.Bottom[j]), worldRect,
                                            canvasRect);
                                    path3.AddLine(tf, tf2);
                                    path2.AddLine(tf, tf2);
                                }
                                g.FillPath(brush, path3);
                                g.DrawPath(pen, path);
                                g.DrawPath(pen, path2);
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
            int startIndex = Data.StartIndex;
            double xValue = startIndex;
            if ((xValue >= left) && (xValue <= right))
            {
                flag = true;
                if (Data.Top[startIndex] > top)
                {
                    top = Data.Top[startIndex];
                }
                if (Data.Bottom[startIndex] < bottom)
                {
                    bottom = Data.Bottom[startIndex];
                }
            }
            for (startIndex = Data.StartIndex + 1; startIndex <= Data.LastIndex; startIndex++)
            {
                double num = startIndex - 1;
                //xValue = Chart.StockSeries.GetXValue(startIndex);
                if (((startIndex + 1) >= left) && (num <= right))
                {
                    flag = true;
                    if (Data.Top[startIndex] > top)
                    {
                        top = Data.Top[startIndex];
                    }
                    if (Data.Bottom[startIndex] < bottom)
                    {
                        bottom = Data.Bottom[startIndex];
                    }
                }
            }
            startIndex = Data.LastIndex;
            xValue = startIndex;
            if ((xValue >= left) && (xValue <= right))
            {
                flag = true;
                if (Data.Top[startIndex] > top)
                {
                    top = Data.Top[startIndex];
                }
                if (Data.Bottom[startIndex] < bottom)
                {
                    bottom = Data.Bottom[startIndex];
                }
            }
            return flag;
        }

        // Properties
    }
}
