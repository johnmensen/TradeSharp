using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls.QuoteTradeControls.DropDownListControls
{
    public class QuoteTradeListItem : ToolStripMenuItem, ICloneable
    {
        /// <summary>
        /// Значение объема торгов этого элемента 
        /// </summary>
        public int VolumeTrade { get; private set; }

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public int TextSize { get; set; }

        public QuoteTradeListItem(int volumeTrade)
        {
            AutoSize = false;
            Text = volumeTrade.ToString("N0");
            VolumeTrade = volumeTrade <= 0 ? -1 : volumeTrade;
        }

        public QuoteTradeListItem(string customVolume)
        {
            AutoSize = false;
            Text = customVolume;
            VolumeTrade = -1;
        }

        private void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.DarkRed)), 0, 0, Width, Height);
            Font = new Font("Times New Roman", TextSize, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
        }

        public object Clone()
        {
            return VolumeTrade <= 0
                       ? new QuoteTradeListItem(Text) {TextSize = this.TextSize}
                       : new QuoteTradeListItem(VolumeTrade) {TextSize = this.TextSize};
        }

        public bool IsHaveValue()
        {
            return !(Convert.ToInt32(VolumeTrade) <= 0);
        }
    }
}
