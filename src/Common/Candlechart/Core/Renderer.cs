using System.Drawing;
using System.Windows.Forms;

namespace Candlechart.Core
{
    internal static class Renderer
    {        
        public static void Draw3DBorder(Graphics g, Rectangle rect, Color color, Border3DStyle style)
        {
            var pen = new Pen(ControlPaint.Dark(color, 0.1f));
            var pen2 = new Pen(ControlPaint.Dark(color, 0.5f));
            var pen3 = new Pen(color);
            var pen4 = new Pen(ControlPaint.Light(color, 0.9f));
            using (pen)
            {
                using (pen2)
                {
                    using (pen3)
                    {
                        using (pen4)
                        {
                            var points = new[]
                                             {
                                                 new Point(rect.Left, rect.Bottom - 2), new Point(rect.Left, rect.Top),
                                                 new Point(rect.Right - 2, rect.Top)
                                             };
                            var pointArray2 = new[]
                                                  {
                                                      new Point(rect.Left + 1, rect.Bottom - 3),
                                                      new Point(rect.Left + 1, rect.Top + 1),
                                                      new Point(rect.Right - 3, rect.Top + 1)
                                                  };
                            var pointArray3 = new[]
                                                  {
                                                      new Point(rect.Left + 1, rect.Bottom - 2),
                                                      new Point(rect.Right - 2, rect.Bottom - 2),
                                                      new Point(rect.Right - 2, rect.Top + 1)
                                                  };
                            var pointArray4 = new[]
                                                  {
                                                      new Point(rect.Left, rect.Bottom - 1),
                                                      new Point(rect.Right - 1, rect.Bottom - 1),
                                                      new Point(rect.Right - 1, rect.Top)
                                                  };
                            if (style == Border3DStyle.Sunken)
                            {
                                if (rect.Width >= 1)
                                {
                                    g.DrawLines(pen, points);
                                }
                                if (rect.Width >= 4)
                                {
                                    g.DrawLines(pen2, pointArray2);
                                }
                                if (rect.Width >= 3)
                                {
                                    g.DrawLines(pen3, pointArray3);
                                }
                                if (rect.Width >= 2)
                                {
                                    g.DrawLines(pen4, pointArray4);
                                }
                            }
                            else if (style == Border3DStyle.Raised)
                            {
                                if (rect.Width >= 1)
                                {
                                    g.DrawLines(pen3, points);
                                }
                                if (rect.Width >= 4)
                                {
                                    g.DrawLines(pen4, pointArray2);
                                }
                                if (rect.Width >= 3)
                                {
                                    g.DrawLines(pen, pointArray3);
                                }
                                if (rect.Width >= 2)
                                {
                                    g.DrawLines(pen2, pointArray4);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}