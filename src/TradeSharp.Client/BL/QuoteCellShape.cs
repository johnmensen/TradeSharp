using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Entity;

namespace TradeSharp.Client.BL
{
    abstract class QuoteCellShape
    {
        protected readonly Color colorNeutral = Color.Black;
        protected Color colorCurrent = Color.Black;
        protected int signCurrent;
        protected readonly RestrictedQueue<float> prevBids = new RestrictedQueue<float>(3);
        protected Color[] colorsGrowth;
        protected Color[] colorsFall;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public void DetermineSentimentColorIndex(float bid)
        {
            colorCurrent = colorNeutral;
            if (prevBids.Length > 0)
            {
                signCurrent = Math.Sign(bid - prevBids.Last);
                if (signCurrent != 0)
                {
                    var sum = prevBids.Sum(pBid => Math.Sign(bid - pBid));
                    var index = signCurrent == Math.Sign(sum) ? Math.Abs(sum) - 1 : 0;
                    colorCurrent = signCurrent > 0 ? colorsGrowth[index] : colorsFall[index];
                }
            }
            prevBids.Add(bid);
        }

        public abstract void Draw(PaintEventArgs e, string ticker,
                                  int precision, float bid, float ask);
    }

    class QuoteCellShapeBar : QuoteCellShape
    {
        private static readonly Font font = new Font(FontFamily.GenericSansSerif, 8);

        public QuoteCellShapeBar()
        {
            colorsGrowth = new[] { Color.FromArgb(0, 0, 35), Color.DarkBlue, Color.Blue };
            colorsFall = new[] { Color.FromArgb(35, 0, 0), Color.DarkRed, Color.Red };

            Width = 180;
            Height = 32;
        }

        public override void Draw(PaintEventArgs e, string ticker,
                                  int precision, float bid, float ask)
        {
            DrawFrame(e.Graphics);

            var centerY = Height / 2;
            var szQuote = e.Graphics.MeasureString("GBPUSDX", font);
            var szPrice = e.Graphics.MeasureString("399.785", font);
            const int padding = 6;
            var sf = new StringFormat { LineAlignment = StringAlignment.Center };

            var strFormat = "{0:f" + precision + "}";
            var strBid = string.Format(strFormat, bid);
            var strAsk = string.Format(strFormat, ask);

            using (var br = new SolidBrush(colorCurrent))
            {
                e.Graphics.DrawString(ticker, font, br, padding, centerY, sf);
                e.Graphics.DrawString(strBid, font, br, padding * 2 + szQuote.Width,
                    centerY, sf);
                e.Graphics.DrawString(strAsk, font, br, padding * 3 + szQuote.Width + szPrice.Width,
                    centerY, sf);
            }
        }

        private void DrawFrame(Graphics g)
        {
            using (var b1 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height - 1),
                                                    Color.FromArgb(0xF9, 0xF9, 0xF9), Color.FromArgb(0xE0, 0xE0, 0xE0)))
            {
                g.FillRectangle(b1, 0, 0, Width - 1, Height - 1);
            }
            using (var p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    class QuoteCellShapeThumb : QuoteCellShape
    {
        private const int SmallWidth = 120;
        private const int SmallHeight = 60;
        private const int LargeWidth = 140;
        private const int LargeHeight = 70;

        private static readonly Size arrowSize = new Size(15, 8);
        private static readonly Point[] arrowPoints = 
            new []
                {
                    new Point(-5, -2), new Point(-5, 0), new Point(-7, 0), 
                    new Point(0, 7), new Point(7, 0), new Point(5, 0), new Point(5, -2)
                };


        private bool largeSize;
        public bool LargeSize
        {
            get { return largeSize; }
            set
            {
                if (largeSize == value) return;
                largeSize = value;
                Width = largeSize ? LargeWidth : SmallWidth;
                Height = largeSize ? LargeHeight : SmallHeight;
            }
        }

        // ReSharper disable InconsistentNaming
        private static readonly Font fontTicker = new Font(FontFamily.GenericSansSerif, 14);
        private static readonly Font fontPrice = new Font(FontFamily.GenericSansSerif, 12);
        private static readonly Font fontPriceTight = new Font(FontFamily.GenericSansSerif, 10);

        private static readonly Font largeFontTicker = new Font(FontFamily.GenericSansSerif, 16);
        private static readonly Font largeFontPrice = new Font(FontFamily.GenericSansSerif, 14);
        private static readonly Font largeFontPriceTight = new Font(FontFamily.GenericSansSerif, 12);
        // ReSharper restore InconsistentNaming

        public QuoteCellShapeThumb()
        {
            colorsGrowth = new[] { Color.FromArgb(0, 0, 35), Color.DarkBlue, Color.Blue };
            colorsFall = new[] { Color.FromArgb(35, 0, 0), Color.DarkRed, Color.Red };

            Width = SmallWidth;
            Height = SmallHeight;
        }

        public override void Draw(PaintEventArgs e, string ticker,
                                  int precision, float bid, float ask)
        {
            DrawFrame(e.Graphics);
            if (largeSize)
                DrawLargeSize(e, ticker, precision, bid, ask);
            else
                DrawSmallSize(e, ticker, precision, bid, ask);
        }

        private void DrawLargeSize(PaintEventArgs e, string ticker, int precision, float bid, float ask)
        {
            var strFormat = "{0:f" + precision + "}";
            var strBid = string.Format(strFormat, bid);
            var strAsk = string.Format(strFormat, ask);
            
            // собрать строку вида 1.4715 / 17
            var strPriceCommon = strBid;
            var strPriceDif = "";
            if (strBid != strAsk)
            {
                var difBpart = GetDifPart(strBid, strAsk, 1);
                var difApart = strBid.Substring(strBid.Length - difBpart.Length, difBpart.Length);
                strPriceDif = difApart + " / " + difBpart;
            }
            
            // тикер.. 
            const int tickTop = 6;
            const int tickPad = 4;
            var tickerRect = e.Graphics.MeasureString(ticker, largeFontTicker);
            var tickerWdTotal = arrowSize.Width + tickerRect.Width + tickPad;
            var tickerLeft = (Width - tickerWdTotal) / 2;
            var arrowLeft = tickerLeft + tickerRect.Width + tickPad;
            var arrowMidY = tickTop + tickerRect.Height / 2;
            var arrowPointsScaled =
                arrowPoints.Select(p => new Point(p.X + (int) arrowLeft, (int) (arrowMidY - p.Y*signCurrent))).ToArray();
            
            // и цена (левая часть)
            const int priceTop = 38;
            var r1 = e.Graphics.MeasureString(strPriceCommon, largeFontPrice);
            var r2 = e.Graphics.MeasureString(strPriceDif, largeFontPriceTight);
            var w12 = r1.Width + r2.Width;
            var left1 = (Width - w12) / 2;
            var left2 = left1 + r1.Width;
            var top2 = priceTop + r1.Height - r2.Height;

            using (var br = new SolidBrush(colorNeutral))
            {
                e.Graphics.DrawString(ticker, largeFontTicker, br, tickerLeft, tickTop);
                e.Graphics.DrawString(strPriceCommon, largeFontPrice, br, left1, priceTop);
            }

            using (var br = new SolidBrush(colorCurrent))
            {
                e.Graphics.DrawString(strPriceDif, largeFontPriceTight, br, left2, top2);
                e.Graphics.FillPolygon(br, arrowPointsScaled);
            }            
        }

        private void DrawSmallSize(PaintEventArgs e, string ticker, int precision, float bid, float ask)
        {
            var strFormat = "{0:f" + precision + "}";
            var strBid = string.Format(strFormat, bid);
            var strAsk = string.Format(strFormat, ask);
            
            // собрать строку вида 1.4715 / 17
            var strPrice = strBid == strAsk ? strBid : string.Format("{0} / {1}", strBid, GetDifPart(strBid, strAsk, 1));
            var priceFont = strPrice.Length < 14 ? fontPrice : fontPriceTight;
            var alignCenter = new StringFormat { Alignment = StringAlignment.Center };

            using (var br = new SolidBrush(colorNeutral))
            {
                e.Graphics.DrawString(ticker, fontTicker, br, Width / 2, 6, alignCenter);
            }
            using (var br = new SolidBrush(colorCurrent))
            {
                e.Graphics.DrawString(strPrice, priceFont, br, Width / 2, 30, alignCenter);
            }

        }

        private void DrawFrame(Graphics g)
        {
            var ht1 = (int)(Height * 0.8);
            var ht2 = Height - ht1;

            using (var b1 = new LinearGradientBrush(new Point(0, 0), new Point(0, ht1 - 1),
                                                    Color.FromArgb(0xF9, 0xF9, 0xF9), Color.FromArgb(0xE0, 0xE0, 0xE0)))
            using (var b2 = new LinearGradientBrush(new Point(0, ht1 - 1), new Point(0, Height - 1),
                                                    Color.FromArgb(0xE0, 0xE0, 0xE0), Color.FromArgb(0xF9, 0xF9, 0xF9)))
            {
                g.FillRectangle(b1, 0, 0, Width - 1, ht1);
                g.FillRectangle(b2, 0, ht1, Width - 1, ht2);
            }
            using (var pen = new Pen(Color.LightGray))
            {
                g.DrawLine(pen, 0, 0, Width - 1, 0);
                g.DrawLine(pen, 0, 0, 0, Height - 1);
            }
            using (var pen = new Pen(Color.DimGray))
            {
                g.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1);
                g.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1);
            }
        }

        private static string GetDifPart(string a, string b, int minLength)
        {
            int lastDif = -1;
            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                if (a[i] != b[i])
                {
                    lastDif = i;
                    break;
                }
            }
            if (lastDif < 0) lastDif = Math.Min(a.Length, b.Length);
            int partLength = b.Length - lastDif;
            if (partLength < minLength) partLength = minLength;
            if (partLength > b.Length) partLength = b.Length;
            return b.Substring(b.Length - partLength);
        }
    }
}
