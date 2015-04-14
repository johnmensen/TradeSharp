using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;

namespace Candlechart.Series
{
    public class CandlestickSeries : StockSeries
    {
        public override int DataCount { get { return Data.Count; } }

        private int barWidthPercent = 80;
        [Browsable(true)]
        [Category("Общие настройки")]
        [DisplayName("Ширина бара")]
        public int BarWidthPercent
        {
            get { return barWidthPercent; }
            set { barWidthPercent = value; }
        }

        public CandlestickSeries(string name)
            : base(name)
        {
        }
        
        /// <summary>
        /// Отрисовать линию крайней котировки
        /// </summary>        
        protected void DrawLastQuote(Graphics g, RectangleD worldRect, Rectangle canvasRect, Pen pen)
        {
            if (Data.Candles.Count == 0) return;
            if (!ShowLastQuote) return;            

            var lastQuote = (double) Data.Candles[Data.Candles.Count - 1].close;
            PointD p = Conversion.WorldToScreen(new PointD(0.0, lastQuote), worldRect, canvasRect);
            var a = new Point(canvasRect.Left, (int)p.Y);
            var b = new Point(canvasRect.Right, (int)p.Y);            
            g.DrawLine(pen, a, b);
        }        

        public int GetCandleCoordX(int index, RectangleD worldRect, Rectangle canvasRect)
        {
            return PointD.Round(Conversion.WorldToScreen(
                new PointD(index, 0), worldRect, canvasRect)).X;
        }        

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);

            var left = worldRect.Left <= 0 ? 0 : (int) worldRect.Left;
            var right = worldRect.Right >= Data.Count ? Data.Count - 1 : (int)worldRect.Right;
            if (right <= left) return;

            using (var quotePen = new Pen(Color.Gray, 1))
                DrawLastQuote(g, worldRect, canvasRect, quotePen);

            if (BarDrawMode == CandleDrawMode.Candle)
                DrawCandles(g, worldRect, canvasRect, left, right);
            else
                if (BarDrawMode == CandleDrawMode.Bar)
                    DrawBars(g, worldRect, canvasRect, left, right);
                else
                    DrawLines(g, worldRect, canvasRect, left, right);
        }

        private void DrawCandles(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            int left, int right)
        {
            var barWidth = (int)(Conversion.WorldToScreen(new SizeD(BarWidthPercent / 100.0, 0.0),
                                                 worldRect, canvasRect).Width);
            if (barWidth < 3) barWidth = 1;

            using (var brushStorage = new BrushesStorage())
            using (var brushUp = new SolidBrush(UpFillColor))
            using (var brushDown = new SolidBrush(DownFillColor))
            using (var penBarUp = new Pen(UpLineColor, BarLineWidth)
            {
                Alignment = PenAlignment.Center,
                DashCap = DashCap.Flat,
                StartCap = LineCap.Flat,
                EndCap = LineCap.Flat
            })
            using (var penBarDown = new Pen(DownLineColor, BarLineWidth)
            {
                Alignment = PenAlignment.Center,
                DashCap = DashCap.Flat,
                StartCap = LineCap.Flat,
                EndCap = LineCap.Flat
            })
            {                
                for (var i = left; i <= right; i++)
                {
                    double close;
                    double open;
                    Brush brush;
                    var candle = Data[i];
                    var isGrowing = candle.close >= candle.open;

                    if (isGrowing)
                    {
                        close = candle.close;
                        open = candle.open;
                        brush = brushUp;
                    }
                    else
                    {
                        close = candle.open;
                        open = candle.close;
                        brush = brushDown;
                    }
                    double xValue = i;
                    if (candle.customColor.HasValue)
                        brush = brushStorage.GetBrush(candle.customColor.Value);

                    // рисовать отрезочек
                    if (barWidth == 1)
                    {
                        var pointA = PointD.Round(Conversion.WorldToScreen(
                                         new PointD(xValue, close), worldRect, canvasRect));

                        var pointB = PointD.Round(Conversion.WorldToScreen(
                                    new PointD(xValue, open), worldRect, canvasRect));

                        DrawLine(g, isGrowing ? penBarUp : penBarDown, pointA, pointB);
                        continue;
                    }

                    var point1 = PointD.Round(Conversion.WorldToScreen(
                                         new PointD(xValue - 0.45, close), worldRect, canvasRect));

                    var point2 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue + 0.45, open), worldRect, canvasRect));

                    var point3 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue, Data[i].low), worldRect, canvasRect));

                    var point4 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue, Data[i].high), worldRect, canvasRect));

                    // свеча...
                    DrawCandle(g, barWidth, close, open, isGrowing ? penBarUp : penBarDown,
                                       point3, point1, point4, point2, brush);
                }
            }
        }

        private void DrawBars(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            int left, int right)
        {
            var barWidth = (int)(Conversion.WorldToScreen(new SizeD(BarWidthPercent / 100.0, 0.0),
                                                 worldRect, canvasRect).Width);
            if (barWidth < 3) barWidth = 1;

            using (var penNeutral = new Pen(BarNeutralColor, BarLineWidth)
                                        {
                                            Alignment = PenAlignment.Center,
                                            DashCap = DashCap.Flat,
                                            StartCap = LineCap.Flat,
                                            EndCap = LineCap.Flat
                                        })
            {
                using (var quotePen = new Pen(Color.Gray, 1))
                    DrawLastQuote(g, worldRect, canvasRect, quotePen);
                for (var i = left; i <= right; i++)
                {
                    double close;
                    double open;                    
                    var candle = Data[i];
                    var isGrowing = candle.close >= candle.open;

                    if (isGrowing)
                    {
                        close = candle.close;
                        open = candle.open;                        
                    }
                    else
                    {
                        close = candle.open;
                        open = candle.close;                        
                    }
                    double xValue = i;                    

                    var point1 = PointD.Round(Conversion.WorldToScreen(
                                         new PointD(xValue - 0.45, close), worldRect, canvasRect));

                    var point2 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue + 0.45, open), worldRect, canvasRect));

                    var point3 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue, Data[i].low), worldRect, canvasRect));

                    var point4 = PointD.Round(Conversion.WorldToScreen(
                                new PointD(xValue, Data[i].high), worldRect, canvasRect));

                    // бар...
                    DrawBar(g, barWidth, penNeutral, point1, point2, point3,
                                    point4, Data[i].close > Data[i].open);
                }
            }
        }

        private void DrawLines(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            int left, int right)
        {

            var prev = Data[left].close;
            using (var penNeutral = new Pen(BarNeutralColor, BarLineWidth)
                                        {
                                            Alignment = PenAlignment.Center,
                                            DashCap = DashCap.Flat,
                                            StartCap = LineCap.Flat,
                                            EndCap = LineCap.Flat
                                        })            
            {
                using (var quotePen = new Pen(Color.Gray, 1))
                    DrawLastQuote(g, worldRect, canvasRect, quotePen);
                for (var i = left + 1; i <= right; i++)
                {
                    var close = Data[i].close;
                    var ptB = PointD.Round(Conversion.WorldToScreen(new PointD(
                                   i, close), worldRect, canvasRect));
                    var ptA = PointD.Round(Conversion.WorldToScreen(new PointD(
                        i - 1, prev), worldRect, canvasRect));
                    prev = close;

                    DrawLine(g, penNeutral, ptA, ptB);                                                                
                }
            }
        }

        protected static void DrawCandle(Graphics g, int barWidth, 
            double close, double open, Pen pen, 
            Point point3, Point point1, Point point4, Point point2, Brush brush)
        {
            g.DrawLine(pen, point3.X, point1.Y, point3.X, point4.Y);
            g.DrawLine(pen, point3.X, point2.Y, point3.X, point3.Y);
            if (close == open)
            {
                g.DrawLine(pen, point1.X, point1.Y, point2.X, point1.Y);
            }
            else
            {
                g.FillRectangle(brush, point3.X - (barWidth / 2), point1.Y,
                                barWidth,
                                point2.Y - point1.Y);
                g.DrawRectangle(pen, point3.X - (barWidth / 2), point1.Y,
                                barWidth - 1,
                                point2.Y - point1.Y);
            }
        }

        protected static void DrawBar(Graphics g, int barWidth, Pen pen,
            Point point1, Point point2, Point point3, Point point4, bool upBar)
        {
            // вертикальная
            g.DrawLine(pen, point3.X, point3.Y, point3.X, point4.Y);
            // открытие
            g.DrawLine(pen, 
                point3.X - barWidth / 2, upBar ? point2.Y : point1.Y,
                point3.X, upBar ? point2.Y : point1.Y);
            // закрытие
            g.DrawLine(pen,
                point3.X + barWidth / 2, upBar ? point1.Y : point2.Y,
                point3.X, upBar ? point1.Y : point2.Y);
        }

        protected static void DrawLine(Graphics g, Pen pen, Point point1, Point point2)
        {
            g.DrawLine(pen, point1.X, point1.Y, point2.X, point2.Y);
        }                   
    }
}