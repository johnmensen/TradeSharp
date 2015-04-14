using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Entity
{
    public static class BitmapConversion
    {
        /// <summary>
        /// на входе - картинка 8 бит в оттенках серого
        /// на выходе - картинка 32 бита переданного оттенка с указанной непрозрачностью
        /// </summary>
        public static Image GetOpaqueColoredImage(Bitmap src, byte alpha, Color tint)
        {
            const int bppSrc = 1;
            const int bppDst = 4;

            var w = src.Width;
            var h = src.Height;
            var srcRect = new Rectangle(0, 0, w, h);
            var tintR = tint.R;
            var tintG = tint.G;
            var tintB = tint.B;

            var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            var srcData = src.LockBits(srcRect, ImageLockMode.ReadOnly, src.PixelFormat);
            var dstData = dst.LockBits(srcRect, ImageLockMode.WriteOnly, dst.PixelFormat);
            try
            {
                IntPtr ptr = srcData.Scan0;
                var bytesSrc = Math.Abs(srcData.Stride) * h;
                var bytesDst = Math.Abs(dstData.Stride) * h;
                var rgbValuesSrc = new byte[bytesSrc];
                var rgbValuesDst = new byte[bytesDst];
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValuesSrc, 0, bytesSrc);

                // читать массив и заменять цвет
                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++)
                    {
                        // получить интенс. цвета исходного и посчитать цвета пикселя результата
                        var ind = x * bppSrc + y * srcData.Stride;
                        var intense = rgbValuesSrc[ind] / 255.0;
                        //var green = rgbValues[ind + 1];
                        //var blue = rgbValues[ind + 2];
                        var red = (byte)(intense * tintR);
                        var green = (byte)(intense * tintG);
                        var blue = (byte)(intense * tintB);

                        // записать в массив результата
                        var indDst = x * bppDst + y * dstData.Stride;
                        rgbValuesDst[indDst + 0] = blue;
                        rgbValuesDst[indDst + 1] = green;
                        rgbValuesDst[indDst + 2] = red;
                        rgbValuesDst[indDst + 3] = alpha;
                    }
                }
                System.Runtime.InteropServices.Marshal.Copy(rgbValuesDst,
                            0, dstData.Scan0, bytesDst);
            }
            finally
            {
                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }
            return dst;
        }
    }
}