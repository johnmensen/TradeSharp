using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class OhlcBarSeries : StockSeries
    {
        // Fields
        private const double _tickWidth = 0.4;

        // Methods
        public OhlcBarSeries(string name)
            : base(name)
        {
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            var pen = new Pen(UpLineColor, LineWidth);
            var pen2 = new Pen(DownLineColor, LineWidth);
            pen.Alignment = PenAlignment.Center;
            pen2.Alignment = PenAlignment.Center;
            using (pen)
            {
                using (pen2)
                {
                    for (int i = 0; i < Data.Count; i++)
                    {
                        Pen pen3 = Data[i].open <= Data[i].close ? pen : pen2;
                        double xValue = i;
                        Point point3 =
                            Conversion.WorldToScreen(new PointD(xValue, (double)Data[i].low), worldRect,
                                                                  canvasRect).Round();
                        Point point4 =
                            Conversion.WorldToScreen(new PointD(xValue, (double)Data[i].high), worldRect,
                                                                  canvasRect).Round();
                        Point point =
                            Conversion.WorldToScreen(new PointD(xValue - _tickWidth, (double)Data[i].open), worldRect,
                                                                  canvasRect).Round();
                        Point point2 =
                            Conversion.WorldToScreen(new PointD(xValue + _tickWidth, (double)Data[i].close),
                                                                  worldRect, canvasRect).Round();
                        g.DrawLine(pen3, point3.X, point4.Y, point3.X, point3.Y);
                        g.DrawLine(pen3, point.X, point.Y, point3.X, point.Y);
                        g.DrawLine(pen3, point3.X, point2.Y, point2.X, point2.Y);
                    }
                }
            }
        }
    }
}
