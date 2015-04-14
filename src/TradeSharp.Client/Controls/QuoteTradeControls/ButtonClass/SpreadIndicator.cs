using System;
using System.Drawing;
using Entity;

namespace TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass
{
    /// <summary>
    /// Класс инкапсулиует свойства и методы, необходимые для отрисовки индикатора спреда, на канве контрола QuoteTradeControl 
    /// </summary>
    public class SpreadIndicator
    {
        /// <summary>
        /// Размер спреда
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Квадрат индикатора с оригинальными размерами и положением
        /// </summary>
        private Rectangle originalRectangle;

        /// <summary>
        /// Квадрат отрисовки индикатора отмасштабированный и расположенный в нужном месте на канве контрола
        /// </summary>       
        public Rectangle SpreadPointRectangle
        {
            get { return spreadPointRectangle; }
        }
        
        private Rectangle spreadPointRectangle;
        private int precisionPower = 4;

        public SpreadIndicator(Rectangle rectangle, string ticker, float value)
        {
            originalRectangle = rectangle;
            Value = value;
            precisionPower = DalSpot.Instance.GetPrecision10(ticker);
        }

        /// <summary>
        /// Установка размеров и положения контрола при его масштабировании
        /// </summary>
        /// <param name="scaleX">Новый масшаб по оси X</param>
        /// <param name="scaleY">Новый масшаб по оси Y</param>
        /// <param name="paddingLeftRight">Отступ родительского контрола (типа QuoteTradeControl) от левого края формы</param>
        /// <param name="paddingTop">Отступ родительского контрола (типа QuoteTradeControl) от верхнего края формы</param>
        public void SetDimensions(float scaleX, float scaleY, int paddingLeftRight, int paddingTop)
        {
            spreadPointRectangle.Location = new Point(Convert.ToInt32(((originalRectangle.X + paddingLeftRight / 2) * scaleX)),
                                                      Convert.ToInt32((originalRectangle.Y + paddingTop) * scaleY));
            spreadPointRectangle.Width = Convert.ToInt32(originalRectangle.Width * scaleX) + 1;  // "+ 1" это поправка на округление
            spreadPointRectangle.Height = Convert.ToInt32(originalRectangle.Height * scaleY) + 1;
        }

        /// <summary>
        /// Прорисовать индикатор - пририсовка квадритика и надписи в соответствии с установлеными для них размеров и положения
        /// </summary>
        /// <param name="graphic">Объект типа "Graphics" контрола типа QuoteTradeControl на котором будем рисовать индикатор</param>
        /// <param name="brushes">Кисть</param>
        public void Draw(Graphics graphic, BrushesStorage brushes)
        {
            graphic.FillRectangle(brushes.GetBrush(Color.FromArgb(255, 192, 255, 192)), spreadPointRectangle);
            graphic.DrawRectangle(new Pen(Color.FromArgb(255, 90, 188, 90), 1), spreadPointRectangle);
            var pipsStr = (Value*precisionPower).ToString("N1");
            

            // Устанавливаем стандартный размер шрифта
            var textSize = spreadPointRectangle.Height / 1.5f > spreadPointRectangle.Width / 3f ? spreadPointRectangle.Width / 3f : spreadPointRectangle.Height / 1.5f;
            var rectSize = graphic.MeasureString(pipsStr, new Font("Open Sans", textSize, FontStyle.Bold));

            // Если при стандарном размере шрифта текст полкостью не влезает, то смотрим насколько не влезло и высчитаваем подходящий размер шрифта
            if (rectSize.Height > spreadPointRectangle.Height || rectSize.Width > spreadPointRectangle.Width)
            {
                textSize = rectSize.Height > rectSize.Width
                               ? textSize * spreadPointRectangle.Height / rectSize.Height - 0.5f
                               : textSize * spreadPointRectangle.Width / rectSize.Width - 0.5f;
                rectSize = graphic.MeasureString(pipsStr, new Font("Open Sans", textSize, FontStyle.Bold));
            }

            var px = (spreadPointRectangle.Width - rectSize.Width) / 2;
            var py = (spreadPointRectangle.Height - rectSize.Height) / 2;
            graphic.DrawString(pipsStr, new Font("Open Sans", textSize, FontStyle.Bold),
                      brushes.GetBrush(Color.Black), spreadPointRectangle.X + px, spreadPointRectangle.Y + py);
        }
    }
}
