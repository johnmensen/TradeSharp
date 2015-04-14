using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Candlechart.Core
{
    public class PaneIconStrip
    {
        private readonly Bitmap image;
        private readonly int imagesCount;        
        private readonly Point leftTop;
        private readonly bool[] iconsVisibility;
        private const int ImageSpacing = 3;
        private readonly int imageWidth;

        /// <param name="resxName">картинка, которая "нарезается" горизонтально на imagesCount частей</param>
        /// <param name="totalImages">кол-во пиктограмм</param>
        /// <param name="alpha">прозрачность (0...255)</param>
        /// <param name="left">верхний угол серии пиктограмм</param>
        /// <param name="top">верхний угол серии пиктограмм</param>
        /// <param name="stateMask">индексы изначально видимых узлов в виде маски 01101B</param>
        public PaneIconStrip(string resxName, int totalImages, int left, int top,
            int stateMask)
        {
            if (string.IsNullOrEmpty(resxName)) return;
            var asm = Assembly.GetExecutingAssembly();
            var imageStream = asm.GetManifestResourceStream(resxName);
            if (imageStream != null) image = new Bitmap(imageStream);
            else
                throw new ArgumentException("Не удалось загрузить ресурс", "resxName");

            imagesCount = totalImages;            
            leftTop = new Point(left, top);
            imageWidth = image.Width / imagesCount;

            iconsVisibility = new bool[imagesCount];
            for (var i = 0; i < imagesCount; i++)
                iconsVisibility[i] = (stateMask & (1 << i)) != 0;
        }

        public void Draw(Graphics g)
        {
            if (!iconsVisibility.Any(v => v)) return;

            var left = leftTop.X;
            for (var i = 0; i < imagesCount; i++)
            {
                if (iconsVisibility[i] == false) continue;

                g.DrawImage(image, left, leftTop.Y,
                    new Rectangle(0, i * imageWidth, imageWidth, image.Height), GraphicsUnit.Pixel);

                left += (imageWidth + ImageSpacing);
            }
        }

        public void SetState(int mask)
        {
            for (var i = 0; i < imagesCount; i++)
                iconsVisibility[i] = (mask & (1 << i)) != 0;
        }

        public int GetStateMask()
        {
            int mask = 0;
            for (var i = 0; i < imagesCount; i++)
            {
                if (iconsVisibility[i]) mask = mask | (1 << i);
            }
            return mask;
        }

        public bool CheckMouseHit(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return false;
            if (!iconsVisibility.Any(v => v)) return false;
            if (e.Y < leftTop.Y || e.Y > (leftTop.Y + image.Height)) return false;

            var left = leftTop.X;
            for (var i = 0; i < imagesCount; i++)
            {
                if (iconsVisibility[i] == false) continue;
                if (e.X >= left && e.X <= (left + imageWidth))
                {
                    iconsVisibility[i] = false;
                    return true;
                }
                left += (imageWidth + ImageSpacing);
            }
            return false;
        }
    }
}
