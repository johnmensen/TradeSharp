using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    /// <summary>
    /// контрол рисует "наполнение" истории по валютной паре
    /// </summary>
    public class QuoteHistoryFillLineControl : Control
    {
        struct GapIntPx
        {
            public long start, end;
            public Color color;

            public GapIntPx(long start, long end, Color color)
            {
                this.start = start;
                this.end = end;
                this.color = color;
            }
        }

        public string Ticker { get; set; }

        public Cortege2<DateTime, DateTime> Interval { get; set; }

        private List<GapInfo> gaps;

        public List<GapInfo> Gaps
        {
            set
            {
                gaps = value;
                if (gaps != null && gaps.Count > 0)
                    if (gaps[0].start < Interval.a)
                        Interval = new Cortege2<DateTime, DateTime>(gaps[0].start, Interval.b);
                UpdateGaps();
            }
            get { return gaps; }
        }

        #region Визуальные

        private Font font;

        private int padding = 5;

        private int barLeft = 70;

        private Rectangle barBounds;

        private int barHeight = 16;

        private Color clHistOk = Color.FromArgb(150, 215, 150);

        private Image imageCancel;
        
        private Color[] clGap = new []
                                    {
                                        Color.Gainsboro, Color.ForestGreen, Color.FromArgb(110, 140, 110), Color.Maroon
                                    };        

        #endregion

        private bool cancelEnabled;
        public bool CancelEnabled
        {
            get { return cancelEnabled; }
            set
            {
                if (cancelEnabled == value) return;
                cancelEnabled = value;
                Invalidate(GetImageRect());
            }
        }

        private Action onCancel;

        public QuoteHistoryFillLineControl()
        {
            font = new Font("Open Sans", 9, FontStyle.Bold);
        }

        public QuoteHistoryFillLineControl(string ticker, Cortege2<DateTime, DateTime> interval, Image imageCancel,
            Action onCancel)
            : this()
        {
            Ticker = ticker;
            Interval = interval;
            this.imageCancel = imageCancel;
            this.onCancel = onCancel;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            try
            {           
                using (var brushes = new BrushesStorage())
                using (var pens = new PenStorage())
                {
                    // подготовить фон
                    DrawBackgr(e.Graphics, brushes);

                    // нарисовать разметку
                    DrawFrame(e.Graphics, brushes, pens);

                    // кнопка отмены
                    if (CancelEnabled)
                        e.Graphics.DrawImageUnscaled(imageCancel, 
                            Width - imageCancel.Width - padding,
                            (Height - imageCancel.Height) / 2);

                    if (gaps == null) return;
                    DrawGaps(e.Graphics, brushes);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteHistoryFillLineControl error", ex);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!cancelEnabled) return;
            var isIn = GetImageRect().Contains(e.X, e.Y);
            Cursor = isIn ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left &&
                GetImageRect().Contains(e.X, e.Y))
                onCancel();
        }

        private void DrawBackgr(Graphics g, BrushesStorage brushes)
        {
            var br = brushes.GetBrush(SystemColors.Control);
            g.FillRectangle(br, 0, 0, Width, Height);
        }

        private void DrawFrame(Graphics g, BrushesStorage brushes, PenStorage pens)
        {
            var brMain = brushes.GetBrush(SystemColors.ControlText);
            var cy = Height / 2;
            
            // текст (название тикера)
            var fmt = new StringFormat {LineAlignment = StringAlignment.Center};
            g.DrawString(Ticker, font, brMain, padding, cy, fmt);
            
            // область истории
            var barRight = Width - padding - imageCancel.Width - padding;
            var barWd = barRight - barLeft;
            if (barWd < 0) barWd = 0;

            barBounds = new Rectangle(barLeft, cy - barHeight / 2, barWd, barHeight);
            if (barWd == 0) return;
            
            // рамка
            g.DrawRectangle(pens.GetPen(SystemColors.ControlText), barBounds);
        }
    
        /// <summary>
        /// нарисовать на истории полоски гэпов
        /// </summary>
        private void DrawGaps(Graphics g, BrushesStorage brushes)
        {
            if (gaps.Count == 0) return;
            var widthPx = barBounds.Width - 2;
            if (widthPx == 0) return;

            var br = brushes.GetBrush(clHistOk);
            g.FillRectangle(br, barBounds.Left + 1, barBounds.Top + 1, barBounds.Width - 1, barBounds.Height - 1);
            
            // пересчитать модельные координаты в координаты времени,
            // заодно склеив соседние
            var startTick = Interval.a.Ticks;
            var endTick = Interval.b.Ticks;
            var widthTics = endTick - startTick;
            var intervalsPx = new List<GapIntPx>();
            GapIntPx? lastPoint = null;

            var gapsCopy = gaps.ToArray();

            foreach (var gap in gapsCopy)
            {
                var gapStartPx = (gap.start.Ticks - startTick) * widthPx / widthTics;
                var gapEndPx = (gap.end.Ticks - startTick) * widthPx / widthTics;
                var color = clGap[(int) gap.status];
                
                if (lastPoint.HasValue)
                    if (lastPoint.Value.end >= gapStartPx && lastPoint.Value.color == color)
                    {
                        intervalsPx[intervalsPx.Count - 1] = new GapIntPx(lastPoint.Value.start, gapEndPx,
                            color);
                        continue;
                    }
                lastPoint = new GapIntPx(gapStartPx, gapEndPx == gapStartPx ? gapStartPx + 1 : gapEndPx, color);
                intervalsPx.Add(lastPoint.Value);
            }

            // отобразить
            foreach (var inter in intervalsPx)
            {
                var brushGap = brushes.GetBrush(inter.color);
                g.FillRectangle(brushGap, 
                    inter.start + barLeft + 1, 
                    barBounds.Top + 1,
                    (inter.end - inter.start), 
                    barBounds.Height - 1);
            }
        }

        private void UpdateGaps()
        {
            if (barBounds.Width == 0) return;
            Invalidate(barBounds);
        }           

        private Rectangle GetImageRect()
        {
            return new Rectangle(Width - padding - imageCancel.Width, (Height - imageCancel.Height)/2,
                imageCancel.Width, imageCancel.Height);
        }
    }    
}
