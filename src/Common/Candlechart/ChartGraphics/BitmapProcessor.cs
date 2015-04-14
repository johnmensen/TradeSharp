using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Candlechart.ChartGraphics;

namespace Candlechart.ChartGraphics
{
    public static class BitmapProcessor
    {
        public static void AddChromeEffect(Bitmap img, int alpha)
        {
            var w = img.Width;
            var h = img.Height;
            var h2 = h / 2;
            const int pad = 3;

            using (var g = Graphics.FromImage(img))
            {
                // градиентная кисть
                using (var brushGrad = new LinearGradientBrush(new Point(0, 0), new Point(0, h2),
                    Color.FromArgb(alpha, Color.White), Color.FromArgb(0, Color.White)))
                {
                    var points = new[]
                                     {
                                         new Point(pad, pad), new Point(w - pad - 1, pad),
                                         new Point(w - pad - 1, h / 3), new Point(w / 2, h / 3),
                                         new Point(pad, h / 2)
                                     };
                    g.FillPolygon(brushGrad, points);
                }
            }
        }


        /// <summary>
        /// получить копию переданного изображения, размытую
        /// </summary>
        /// <param name="img">картинка</param>
        /// <param name="rect">копируемая область</param>
        /// <param name="blurFactor">к. размытия, 0 - нет, 1 - максимальное</param>
        /// <returns>размытая копия</returns>
        public static Image GetBluredImage(Bitmap img, Rectangle rect, float blurFactor)
        {
            var blured = img.Clone(rect, img.PixelFormat);
            using (var bd = new BitmapDecorator(blured))
            {
                for (var x = 0; x < rect.Width; x++)
                {
                    for (var y = 0; y < rect.Height; y++)
                    {
                        bd.BlurPixel(x, y, blurFactor);
                    }
                }
            }
            return blured;
        }

        public static Bitmap CaptureControl(Control control)
        {
            Bitmap controlBmp;
            using (Graphics g1 = control.CreateGraphics())
            {
                controlBmp = new Bitmap(control.Width, control.Height, g1);
                using (Graphics g2 = Graphics.FromImage(controlBmp))
                {
                    IntPtr dc1 = g1.GetHdc();
                    IntPtr dc2 = g2.GetHdc();
                    BitBlt(dc2, 0, 0, control.Width, control.Height, dc1, 0, 0, 13369376);
                    g1.ReleaseHdc(dc1);
                    g2.ReleaseHdc(dc2);
                }
            }

            return controlBmp;
        }

        public static Bitmap CaptureControl(Control control, Graphics g1)
        {
            var controlBmp = new Bitmap(control.Width, control.Height, g1);
            using (Graphics g2 = Graphics.FromImage(controlBmp))
            {
                IntPtr dc1 = g1.GetHdc();
                IntPtr dc2 = g2.GetHdc();
                BitBlt(dc2, 0, 0, control.Width, control.Height, dc1, 0, 0, 13369376);
                g1.ReleaseHdc(dc1);
                g2.ReleaseHdc(dc2);
            }
            return controlBmp;
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
    }
}
