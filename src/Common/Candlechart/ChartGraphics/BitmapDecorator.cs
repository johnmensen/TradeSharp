using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Candlechart.ChartGraphics
{
    public class BitmapDecorator : IDisposable
    {
        private readonly Bitmap _bitmap;
        private readonly bool _isAlpha;
        private readonly int _width;
        private readonly int _height;
        private BitmapData _bmpData;
        private IntPtr _bmpPtr;
        private byte[] _rgbValues;
        private int index;
        private int b;
        private int r;
        private int g;
        private int a;
        private int r1;
        private int g1;
        private int b1;
        private int a1;
        public BitmapDecorator(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException();
            _bitmap = bitmap;
            if (_bitmap.PixelFormat == (PixelFormat.Indexed | _bitmap.PixelFormat))
                throw new ArgumentException("Can't work with indexed pixel format");
            _isAlpha = (_bitmap.PixelFormat == (_bitmap.PixelFormat | PixelFormat.Alpha));
            _width = bitmap.Width;
            _height = bitmap.Height;
            Lock();
        }

        #region properties
        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }
        #endregion
        #region methods
        private void Lock()
        {
            var rect = new Rectangle(0, 0, _width, _height);
            _bmpData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            _bmpPtr = _bmpData.Scan0;
            int bytes = _width * _height * (_isAlpha ? 4 : 3);
            _rgbValues = new byte[bytes];
            Marshal.Copy(_bmpPtr, _rgbValues, 0, _rgbValues.Length);
        }
        private void UnLock()
        {
            // Copy the RGB values back to the bitmap			
            Marshal.Copy(_rgbValues, 0, _bmpPtr, _rgbValues.Length);
            // Unlock the bits.			
            Bitmap.UnlockBits(_bmpData);
        }
        public void SetPixel(int x, int y, int r, int g, int b)
        {
            if (_isAlpha)
            {
                index = ((y * _width + x) * 4);
                _rgbValues[index] = (byte)b;
                _rgbValues[index + 1] = (byte)g;
                _rgbValues[index + 2] = (byte)r;
                _rgbValues[index + 3] = 255;
            }
            else
            {
                index = ((y * _width + x) * 3);
                _rgbValues[index] = (byte)b;
                _rgbValues[index + 1] = (byte)g;
                _rgbValues[index + 2] = (byte)r;
            }
        }
        public void SetPixel(int x, int y, Color color)
        {
            if (_isAlpha)
            {
                index = ((y * _width + x) * 4);
                _rgbValues[index] = color.B;
                _rgbValues[index + 1] = color.G;
                _rgbValues[index + 2] = color.R;
                _rgbValues[index + 3] = color.A;
            }
            else
            {
                index = ((y * _width + x) * 3);
                _rgbValues[index] = color.B;
                _rgbValues[index + 1] = color.G;
                _rgbValues[index + 2] = color.R;
            }
        }
        public void SetPixel(Point point, Color color)
        {
            if (_isAlpha)
            {
                index = ((point.Y * _width + point.X) * 4);
                _rgbValues[index] = color.B;
                _rgbValues[index + 1] = color.G;
                _rgbValues[index + 2] = color.R;
                _rgbValues[index + 3] = color.A;
            }
            else
            {
                index = ((point.Y * _width + point.X) * 3);
                _rgbValues[index] = color.B;
                _rgbValues[index + 1] = color.G;
                _rgbValues[index + 2] = color.R;
            }
        }

        public void BlurPixel(int x, int y, float blurFactor)
        {
            var bytesPerPx = _isAlpha ? 4 : 3;
            int w = _width;
            int h = _height;
            var colorShift = (x + y * w) * bytesPerPx;

            var B = _rgbValues[colorShift + 0];
            var G = _rgbValues[colorShift + 1];
            var R = _rgbValues[colorShift + 2];

            byte sumB = 0, sumG = 0, sumR = 0;
            int neighbourCount = 0;
            if (x > 0)
            {
                neighbourCount++;
                var pindex = (x - 1 + y * w) * bytesPerPx;
                sumB += _rgbValues[pindex + 0];
                sumG += _rgbValues[pindex + 1];
                sumR += _rgbValues[pindex + 2];
            }
            if (x < (w - 1))
            {
                neighbourCount++;
                var pindex = (x + 1 + y * w) * bytesPerPx;
                sumB += _rgbValues[pindex + 0];
                sumG += _rgbValues[pindex + 1];
                sumR += _rgbValues[pindex + 2];
            }
            if (y > 0)
            {
                neighbourCount++;
                var pindex = (x + (y - 1) * w) * bytesPerPx;
                sumB += _rgbValues[pindex + 0];
                sumG += _rgbValues[pindex + 1];
                sumR += _rgbValues[pindex + 2];
            }
            if (y < (h - 1))
            {
                neighbourCount++;
                var pindex = (x + (y + 1) * w) * bytesPerPx;
                sumB += _rgbValues[pindex + 0];
                sumG += _rgbValues[pindex + 1];
                sumR += _rgbValues[pindex + 2];
            }

            B = (byte)(B * (1 - blurFactor) + blurFactor * sumB / neighbourCount);
            G = (byte)(G * (1 - blurFactor) + blurFactor * sumG / neighbourCount);
            R = (byte)(R * (1 - blurFactor) + blurFactor * sumR / neighbourCount);

            _rgbValues[colorShift + 0] = B;
            _rgbValues[colorShift + 1] = G;
            _rgbValues[colorShift + 2] = R;
            //if (bytesPerPx == 4) 
            //    _rgbValues[colorShift + 3] = 255;
        }

        public Color GetPixel(int x, int y)
        {
            if (x > _width - 1 || y > _height - 1)
                throw new ArgumentException();
            if (_isAlpha)
            {
                index = ((y * _width + x) * 4);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
                a = _rgbValues[index + 3];
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                index = ((y * _width + x) * 3);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
                return Color.FromArgb(r, g, b);
            }
        }
        public int ColorDiff(int x1, int y1, int x2, int y2)
        {
            if (_isAlpha)
            {
                index = ((y1 * _width + x1) * 4);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
                a = _rgbValues[index + 3];
            }
            else
            {
                index = ((y1 * _width + x1) * 3);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
            }
            if (_isAlpha)
            {
                index = ((y2 * _width + x2) * 4);
                b1 = _rgbValues[index];
                g1 = _rgbValues[index + 1];
                r1 = _rgbValues[index + 2];
                a1 = _rgbValues[index + 3];
            }
            else
            {
                index = ((y2 * _width + x2) * 3);
                b1 = _rgbValues[index];
                g1 = _rgbValues[index + 1];
                r1 = _rgbValues[index + 2];
            }
            r = r1 - r;
            g = g1 - g;
            b = b1 - b;
            if (r < 0)
                r = -r;
            if (g < 0)
                g = -g;
            if (b < 0)
                b = -b;
            return r + g + b;
        }
        public int ColorDiff(int i1, int i2)
        {
            if (_isAlpha)
            {
                index = ((i1) * 4);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
                a = _rgbValues[index + 3];
            }
            else
            {
                index = ((i1) * 3);
                b = _rgbValues[index];
                g = _rgbValues[index + 1];
                r = _rgbValues[index + 2];
            }
            if (_isAlpha)
            {
                index = ((i2) * 4);
                b1 = _rgbValues[index];
                g1 = _rgbValues[index + 1];
                r1 = _rgbValues[index + 2];
                a1 = _rgbValues[index + 3];
            }
            else
            {
                index = ((i2) * 3);
                b1 = _rgbValues[index];
                g1 = _rgbValues[index + 1];
                r1 = _rgbValues[index + 2];
            }
            r = r1 - r;
            g = g1 - g;
            b = b1 - b;
            if (r < 0)
                r = -r;
            if (g < 0)
                g = -g;
            if (b < 0)
                b = -b;
            return r + g + b;
        }
        public int ColorDiff(Point point1, Point point2)
        {
            return ColorDiff(point1.X, point1.Y, point2.X, point2.Y);
        }
        public Color GetPixel(Point point)
        {
            return GetPixel(point.X, point.Y);
        }
        #region IDisposable Members
        public void Dispose()
        {
            UnLock();
        }
        #endregion
        #endregion
    }
}