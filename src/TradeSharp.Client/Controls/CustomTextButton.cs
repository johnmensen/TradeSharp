using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public class CustomTextButton : Button
    {
        public string buttonCaption;

        public Color captionColor;

        public Font captionFont;

        public string buttonTextLeft;

        public Color textLeftColor;

        public Font textLeftFont;

        public string buttonTextRight;

        public Color textRightColor;

        public Font textRightFont;

        public void SetPrice(float price, string ticker)
        {
            var priceStr = price.ToStringUniformPriceFormat(true);
            buttonTextRight = priceStr.Length < 2 ? priceStr : priceStr.Substring(priceStr.Length - 2, 2);
            buttonTextLeft = priceStr.Length < 2 ? "" : priceStr.Substring(0, priceStr.Length - 2);
        }
        
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (captionFont == null || textLeftFont == null || textRightFont == null) return;
            // рисовать заголовок
            using (var brush = new SolidBrush(captionColor))
            {
                pevent.Graphics.DrawString(buttonCaption, captionFont, brush, Width / 2, Height / 2, 
                    new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far});
            }

            // измерить ширину левой и правой частей строки
            var szLeft = pevent.Graphics.MeasureString(buttonTextLeft, textLeftFont);
            var szRight = pevent.Graphics.MeasureString(buttonTextRight, textRightFont);
            const int span = 2;
            var totalWidth = szLeft.Width + szRight.Width + span;
            var pads = (Width - totalWidth) / 2;
            var maxH = Math.Max(szLeft.Height, szRight.Height);
            var bottomY = Height / 2 + maxH;

            using (var brush = new SolidBrush(textLeftColor))
            {
                pevent.Graphics.DrawString(buttonTextLeft, textLeftFont, brush, pads, Height / 2,
                                           new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near});
                
            }

            using (var brush = new SolidBrush(textRightColor))
            {
                pevent.Graphics.DrawString(buttonTextRight, textRightFont, brush, pads + szLeft.Width,
                    bottomY,
                    /*Height / 2,*/
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far });
            }
        }
    }
}
