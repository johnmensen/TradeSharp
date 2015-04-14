using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Candlechart.ChartGraphics;
using TradeSharp.Util;

namespace Candlechart
{
    public partial class CandleChartControl
    {
        public Image MakeImage()
        {
            var width = chart.Width;
            var height = chart.Height;
            var bmp = new Bitmap(width, height);
            chart.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
            return bmp;
        }

        public void SaveAsImage()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PNG|*.PNG|JPEG|*.JPG|BMP|*.BMP|GIF|*.GIF|EMF (vector)|*.EMF",
                Title = Localizer.GetString("TitleSaveChartMenu").Replace("...", ""),
                DefaultExt = "PNG"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var extension = new FileInfo(dlg.FileName).Extension;
                if (extension.ToLower() == ".emf")
                {
                    SaveAsImageWMF(dlg.FileName);
                    return;
                }
                var fileFmt = ImageFormat.Jpeg;
                if (extension.ToLower() == ".bmp") fileFmt = ImageFormat.Bmp;
                if (extension.ToLower() == ".gif") fileFmt = ImageFormat.Gif;
                if (extension.ToLower() == ".png") fileFmt = ImageFormat.Png;
                var width = chart.Width;
                var height = chart.Height;
                using (var bmp = new Bitmap(width, height))
                {
                    chart.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
                    bmp.Save(dlg.FileName, fileFmt);
                }
            }
        }

        public void SaveAsImageWMF(string fileName)
        {
            var width = chart.Width;
            var height = chart.Height;
            var bmp = new Bitmap(width, height);
            var g = Graphics.FromImage(bmp);

            var path = fileName;

            var useSimple = true;
            var img = useSimple ? new Metafile(path, g.GetHdc())
                          : new Metafile(path, g.GetHdc(), EmfType.EmfPlusDual);
            var ig = Graphics.FromImage(img);

            chart.Draw(ig);

            ig.Dispose();
            img.Dispose();
            g.ReleaseHdc();
            g.Dispose();
        }

        public void SaveAsImage(string path, ImageFormat imgFormat)
        {
            var width = chart.Width;
            var height = chart.Height;
            using (var bmp = new Bitmap(width, height))
            {
                chart.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
                bmp.Save(path, imgFormat);
            }
        }

        public void SaveAsImage(string path, ImageFormat imgFormat, int w, int h)
        {
            SaveAsImage(path, imgFormat, w, h, false);
        }

        public void SaveAsImage(string path, ImageFormat imgFormat, int w, int h, bool addChrome)
        {
            var width = chart.Width;
            var height = chart.Height;
            using (var bmp = new Bitmap(width, height))
            {
                chart.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
                using (var bmpScaled = Scale(bmp, (float)w / width, (float)h / height))
                {
                    if (addChrome)
                        BitmapProcessor.AddChromeEffect(bmpScaled, 180);
                    bmpScaled.Save(path, imgFormat);    
                }
            }
        }

        public static Bitmap Scale(Bitmap bitmap, float ScaleFactorX, float ScaleFactorY)
        {
            var scaleWidth = (int)Math.Max(bitmap.Width * ScaleFactorX, 1.0f);
            var scaleHeight = (int)Math.Max(bitmap.Height * ScaleFactorY, 1.0f);

            var scaledBitmap = new Bitmap(scaleWidth, scaleHeight);

            // Scale the bitmap in high quality mode.
            using (Graphics gr = Graphics.FromImage(scaledBitmap))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                gr.DrawImage(bitmap, new Rectangle(0, 0, scaleWidth, scaleHeight), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }

            // Copy original Bitmap's EXIF tags to new bitmap.
            foreach (PropertyItem propertyItem in bitmap.PropertyItems)
            {
                scaledBitmap.SetPropertyItem(propertyItem);
            }

            return scaledBitmap;
        }

        /// <summary>
        /// сохранить как картинку в текущий каталог с именем файла вида
        /// screen00012.png
        /// </summary>
        public void SaveAsImageCurrentFolder()
        {
            var curFolder = Path.GetDirectoryName(Application.ExecutablePath);
            var files = Directory.GetFiles(curFolder, "screen*.png");
            int curNumber = 0;
            foreach (var fileName in files)
            {
                var nameStr = Path.GetFileNameWithoutExtension(fileName);
                nameStr = nameStr.Substring(6);
                int fileNum;
                int.TryParse(nameStr, out fileNum);
                if (fileNum > curNumber) curNumber = fileNum;
            }
            var imgFileName = string.Format("{0}\\screen{1}.png",
                                            curFolder, (++curNumber).ToString("00000"));
            SaveAsImage(imgFileName, ImageFormat.Png);
        }
    }
}