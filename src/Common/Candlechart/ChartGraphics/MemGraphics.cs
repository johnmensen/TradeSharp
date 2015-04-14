using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Candlechart.ChartGraphics
{
    internal class MemGraphics : IDisposable
    {
        // Fields
        private readonly Graphics _graphics;
        private readonly int _height;
        private readonly Bitmap _memBitmap;
        private readonly int _width;

        // Methods
        public MemGraphics(int width, int height)
        {
            _width = width;
            _height = height;
            _memBitmap = new Bitmap(width, height);
            _graphics = Graphics.FromImage(_memBitmap);
        }

        public Graphics Graphics
        {
            get { return _graphics; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _graphics.Dispose();
            _memBitmap.Dispose();
        }

        #endregion

        public void Render(MemGraphics buffer)
        {
            Render(buffer.Graphics);
        }

        public void Render(Graphics g)
        {
            Render(g, new Rectangle(0, 0, _width, _height));
        }

        public void Render(MemGraphics buffer, Rectangle rect)
        {
            Render(buffer.Graphics, rect);
        }

        public void Render(Graphics g, Rectangle rect)
        {
            Render(g, rect, rect);
        }

        public void Render(MemGraphics buffer, Rectangle srcRect, Rectangle dstRect)
        {
            Render(buffer.Graphics, srcRect, dstRect, null);
        }

        public void Render(Graphics g, Rectangle srcRect, Rectangle dstRect)
        {
            Render(g, srcRect, dstRect, null);
        }

        public void Render(MemGraphics buffer, Rectangle srcRect, Rectangle dstRect, ImageAttributes imageAttr)
        {
            Render(buffer.Graphics, srcRect, dstRect, imageAttr);
        }

        public void Render(Graphics g, Rectangle srcRect, Rectangle dstRect, ImageAttributes imageAttr)
        {
            new Rectangle(0, 0, _width, _height);
            if (imageAttr == null)
            {
                g.DrawImage(_memBitmap, dstRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.DrawImage(_memBitmap, dstRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel,
                            imageAttr);
            }
        }

        public void Xor(Color color)
        {
            for (int i = 0; i < _memBitmap.Height; i++)
            {
                for (int j = 0; j < _memBitmap.Width; j++)
                {
                    Color pixel = _memBitmap.GetPixel(j, i);
                    _memBitmap.SetPixel(j, i, Color.FromArgb(pixel.R ^ color.R, pixel.G ^ color.G, pixel.B ^ color.B));
                }
            }
        }
    }
}