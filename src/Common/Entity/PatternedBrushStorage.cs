using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using TradeSharp.Util;

namespace Entity
{
    /// <summary>
    /// класс хранит картинки, предназначенные для рисования PictureBrush
    /// раскрашиваемые в нужный цвет с заданной прозрачностью
    /// 
    /// хранит раскрашенные картинки в кеше
    /// </summary>
    public class PatternedBrushStorage
    {
        private readonly Bitmap bmpSrc;

        private readonly Dictionary<Cortege2<int, Color>, Bitmap> images =
            new Dictionary<Cortege2<int, Color>, Bitmap>();

        /// <summary>
        /// на входе - картинка-источник, 8 бит, шкала серого
        /// </summary>
        public PatternedBrushStorage(Bitmap bmp)
        {
            bmpSrc = bmp;
        }

        /// <summary>
        /// имя ресурса - картинки-источника, 8 бит, шкала серого
        /// вида Candlechart.images.terminal_logo_main.png
        /// </summary>
        public PatternedBrushStorage(Assembly asm,
            string resourceName)
        {
            try
            {                
                var imageStream = asm.GetManifestResourceStream(resourceName);
                if (imageStream != null)
                    bmpSrc = new Bitmap(imageStream);
            }
            catch (Exception ex)
            {
                Logger.InfoFormat("PatternedBrushStorage.ctor() - ошибка загрузки ресурса \"{0}\": {1}",
                    resourceName, ex);
                throw;
            }
        }

        /// <summary>
        /// вернуть изображение из кеша либо
        /// создать изображение нужной окраски, поместить в кеш и вернуть
        /// из кеша
        /// </summary>
        public Bitmap GetImage(byte alpha, Color tint)
        {
            var key = new Cortege2<int, Color>(alpha, tint);
            if (images.ContainsKey(key)) return images[key];
            var img = (Bitmap)BitmapConversion.GetOpaqueColoredImage(bmpSrc, alpha, tint);
            images.Add(key, img);
            return img;
        }
    }
}
