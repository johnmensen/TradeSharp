using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    /// <summary>
    /// Отображение интервалов времени областями
    /// </summary>
    public class LevelSeries : Series
    {
        public List<LevelSeriesItem> levels = new List<LevelSeriesItem>();

        public override int DataCount { get { return levels.Count; } }

        public LevelSeries(string name)
            : base(name)
        {
        }
        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            return false;
        }
        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            DrawRegions(g, worldRect, canvasRect);
        }

        private void DrawRegions(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            foreach (var l in levels)
            {
                DrawLevel(l, g, worldRect, canvasRect);
            }
        }

        private void DrawLevel(LevelSeriesItem l, Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var brushFont = new SolidBrush(Color.Black))
            {
                using (var brushSign = new SolidBrush(l.Color))
                {
                    using (var pen = new Pen(l.Color))
                    {
                        int x1 = canvasRect.Left;
                        int x2 = canvasRect.Right;
                        PointD p1 = Conversion.WorldToScreen(new PointD(0, (double) l.Price),
                                                             worldRect, canvasRect);
                        var y = (int) p1.Y;
                        var textSz = g.MeasureString(l.Title, Chart.Font);
                        var textW2 = (int) textSz.Width/2;
                        var textH2 = (int) textSz.Height/2;
                        var centerX = (x1 + x2)/2;

                        if (l.TextDirection == LevelSeriesItem.Direction.Left)
                        {
                            DrawRectangle(pen, g, x1, y - textH2 - 2, (int) textSz.Width + 4,
                                          (int) textSz.Height + 4);
                            g.DrawString(l.Title, Chart.Font, brushFont, x1 + 2, y - textH2);
                            g.DrawLine(pen, textSz.Width + 4, y, x2, y);
                            DrawSign(pen, brushSign, g, x2 - 6, y, l.Side);
                            return;
                        }

                        if (l.TextDirection == LevelSeriesItem.Direction.Right)
                        {
                            DrawRectangle(pen, g, x2 - (int) textSz.Width - 4, y - textH2 - 2,
                                          (int) textSz.Width + 4, (int) textSz.Height + 4);
                            g.DrawString(l.Title, Chart.Font, brushFont, x2 - textSz.Width - 2, y - textH2);
                            g.DrawLine(pen, x1, y, x2 - textSz.Width - 4, y);
                            DrawSign(pen, brushSign, g, x1 + 6, y, l.Side);
                            return;
                        }

                        if (l.TextDirection == LevelSeriesItem.Direction.Middle)
                        {
                            DrawRectangle(pen, g, centerX - textW2 - 2, y - textH2 - 2,
                                          (int) textSz.Width + 4, (int) textSz.Height + 4);
                            g.DrawString(l.Title, Chart.Font, brushFont, centerX - textW2, y - textH2);
                            g.DrawLine(pen, x1, y, centerX - textW2 - 2, y);
                            g.DrawLine(pen, centerX + textW2 + 2, y, x2, y);
                            DrawSign(pen, brushSign, g, x1 + 6, y, l.Side);
                            DrawSign(pen, brushSign, g, x2 - 6, y, l.Side);
                            return;
                        }
                    }
                }
            }
        }
        private static void DrawRectangle(Pen pen, Graphics g, int x, int y, int w, int h)
        {
            var colorFrom = Color.FromArgb(128, pen.Color);
            var colorTo = Color.FromArgb(90, FadeColor(pen.Color));
            using (var fillBrush = new LinearGradientBrush(new Rectangle(x, y, w, h), colorFrom, colorTo, 90))
            {
                g.FillRectangle(fillBrush, x, y, w, h);
                g.DrawRectangle(pen, x, y, w, h);    
            }            
        }
        private static Color FadeColor(Color c)
        {
            return Color.FromArgb(255, 
                (c.G*1.5) < 255 ? (int) (c.G*1.5) : 255,
                (c.B*1.5) < 255 ? (int) (c.B*1.5) : 255, 
                (c.R*1.5) < 255 ? (int) (c.R*1.5) : 255);                                   
        }
        private static void DrawSign(Pen pen, Brush brush, Graphics g, int x, int y, int side)
        {
            const int dx = 3, dy = 4;
            var points = side < 0 ?
                new[]
                     {
                        new Point(x - dx, y - dx),
                        new Point(x + dx, y - dx),
                        new Point(x, y + dy)
                     } :
                 new[]
                     {
                        new Point(x, y - dy),
                        new Point(x - dx, y + dx),
                        new Point(x + dx, y + dx)
                     };
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }
    }

    public class LevelSeriesItem
    {
        public decimal Price { get; set; }
        
        public string Title { get; set; }

        public int Side { get; set; }
        
        private Color color = Color.Blue;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public enum Direction
        {
            Left,
            Middle,
            Right
        }
        public Direction TextDirection { get; set; }
        public LevelSeriesItem() {}
        public LevelSeriesItem(decimal price, string title, Color color, Direction textDir)
        {
            Price = price;
            Title = title;
            Color = color;
            TextDirection = textDir;
        }
    }
}