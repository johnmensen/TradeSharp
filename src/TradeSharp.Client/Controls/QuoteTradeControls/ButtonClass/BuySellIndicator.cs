using System.Drawing;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass
{
    /// <summary>
    /// Класс инкапсулирует поля и методы необходимые для прорисовки на фигурной кнопке типа операции (BUY / SELL) и соответствующего значения Bid или Ask
    /// </summary>
    public class BuySellIndicator
    {
        private const float MarginText = 5.7f;
        private const float MarginLeft = 1.5f;
        private const float TextSizeOriginal = 4;     

        public string HaderText { get; set; } 
        public float? Volume
        {
            get { return currentVolume; }
            set
            {
                currentVolume = value.HasValue ? value.Value : 0;
                var cvString = currentVolume.ToStringUniformPriceFormat(true);
                var cvLength = cvString.Length;

                volumeBotton = value.HasValue ? cvString.Substring(cvLength - 1, 1) : "-";
                volumeCentr = value.HasValue ? cvString.Substring(cvLength - 3, 2) : "-";
                volumeTop = value.HasValue ? cvString.Substring(0, cvLength - 3) : "-";
                       
            }
        }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        /// <summary>
        /// Положение на кнопке 
        /// </summary>
        public Point OriginalLocation { get; set; }

        private string volumeTop;
        private string volumeCentr;
        private string volumeBotton;
        private float currentVolume;

        /// <summary>
        /// прорисовка на кнопке
        /// </summary>
        public void Draw(Graphics graphic, BrushesStorage brushes)
        {
            var textSize = ScaleY < ScaleX ? TextSizeOriginal * ScaleY : TextSizeOriginal * ScaleX;
            
            graphic.DrawString(HaderText, new Font("Open Sans", textSize), brushes.GetBrush(Color.Black),
                new PointF(OriginalLocation.X * ScaleX, OriginalLocation.Y * ScaleY));

            graphic.DrawString(volumeTop, new Font("Open Sans", textSize * 0.7f, FontStyle.Bold), brushes.GetBrush(Color.DarkBlue),
                new PointF((OriginalLocation.X + MarginLeft) * ScaleX, (OriginalLocation.Y + MarginText) * ScaleY));
            
            //Прорисовка центральной части
            var sizeTop = graphic.MeasureString(volumeTop,
                                              new Font("Open Sans", textSize * 0.7f, FontStyle.Bold));
            graphic.DrawString(volumeCentr, new Font("Open Sans", textSize * 1.25f, FontStyle.Bold), brushes.GetBrush(Color.DarkBlue),
                new PointF((OriginalLocation.X + 4 * MarginLeft) * ScaleX, (OriginalLocation.Y + MarginText) * ScaleY + 4 * sizeTop.Height / 5));

            //Прорисовка нижней части
            var sizeMiddle = graphic.MeasureString(volumeCentr, new Font("Open Sans", textSize * 1.25f, FontStyle.Bold));
            graphic.DrawString(volumeBotton, new Font("Open Sans", textSize * 0.7f, FontStyle.Bold), brushes.GetBrush(Color.DarkBlue),
                new PointF((OriginalLocation.X +  4 * MarginLeft) * ScaleX + sizeMiddle.Height, 
                    (OriginalLocation.Y + MarginText) * ScaleY + 2 * sizeTop.Height / 3 + 2 * sizeMiddle.Height / 3));
        }
    }
}