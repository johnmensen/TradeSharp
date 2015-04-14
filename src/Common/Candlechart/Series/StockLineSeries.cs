using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class StockLineSeries : StockSeries
    {
        // Fields

        // Methods
        public StockLineSeries(string name)
            : base(name)
        {
            PriceField = PriceField.Close;
        }

        // Properties
        public PriceField PriceField { get; set; }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            var pen = new Pen(UpLineColor, LineWidth);
            var path = new GraphicsPath();
            pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), LineStyle.ToString());
            using (pen)
            {
                using (path)
                {                    
                    for (int i = 1; i < Data.Count; i++)
                    {
                        GetPriceByField(i);
                        var tf =
                            (PointF)
                            Conversion.WorldToScreen(
                                new PointD(i - 1, GetPriceByField(i - 1)), worldRect,
                                canvasRect);
                        var tf2 =
                            (PointF)
                            Conversion.WorldToScreen(new PointD(i, GetPriceByField(i)), worldRect,
                                                     canvasRect);
                        path.AddLine(tf, tf2);
                    }
                    g.DrawPath(pen, path);
                }
            }
        }

        private double GetPriceByField(int i)
        {
            var dataValue = Data[i].open;
            switch (PriceField)
            {
                case PriceField.Close:
                    dataValue = Data[i].close;
                    break;
                case PriceField.High:
                    dataValue = Data[i].high;
                    break;
                case PriceField.Low:
                    dataValue = Data[i].low;
                    break;                            
            }
            return (double) dataValue;
        }
    }    
}