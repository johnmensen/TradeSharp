using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls
{
    /// <summary>
    /// полоска, которую пользователь может растянуть за левую границу\
    /// </summary>
    class TimeIntervalControl : Control
    {
        private Action<int> valueChangedByDragging;

        public event Action<int> ValueChangedByDragging { add { valueChangedByDragging += value; } remove { valueChangedByDragging -= value; } }

        public string Title { get; set; }

        public int MinValue { get; set; }

        public int DefaultValue { get; set; }

        public int MaxValue { get; set; }

        public int Value { get; set; }

        private bool isDragged;

        private bool isHovered;

        private bool MarkerHighlighted
        {
            get { return isDragged || isHovered; }
        }

        private int barLeft;

        private Rectangle markerArea;

        #region Визуальные

        public int padding = 5;

        public int paddingText = 4;

        public int textWidth = 30;

        public int titleWidth = 40;

        public int barHeight = 16;

        public int markerRad = 4;

        public Color colorLine = Color.Black;

        public Color colorFillFilled = Color.FromArgb(150, 255, 180);

        public Color colorFillEmpty = Color.FromArgb(220, 255, 240);

        public Color colorText = Color.Black;

        public Font fontTitle;

        private Font FontTitle
        {
            get { return fontTitle ?? Font; }
        }

        #endregion

        public TimeIntervalControl(string title, int minValue, int maxValue, int defaultValue)
        {
            Title = title;
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var barWd = Width - titleWidth - padding - padding - textWidth - paddingText;
            var rightEdge = Width - padding;

            var cy = Height / 2;
            var top = cy - barHeight / 2;
            var scaleX = barWd / (double)Math.Max(MaxValue, DefaultValue);

            var wdFilled = (int)(Math.Min(Value, DefaultValue) * scaleX);
            var wdTotal = (int) (Value*scaleX);
            var wdLeft = wdTotal - wdFilled;
            barLeft = rightEdge - wdTotal;

            using (var brushEmpty = new SolidBrush(colorFillEmpty))
            using (var brushFilled = new SolidBrush(colorFillFilled))
            using (var brushText = new SolidBrush(colorText))
            using (var pen = new Pen(colorLine))
            {
                // залить
                if (wdLeft > 0)
                    e.Graphics.FillRectangle(brushEmpty, barLeft, top, wdLeft, barHeight);
                e.Graphics.FillRectangle(brushFilled, rightEdge - wdFilled, top, wdFilled, barHeight);
                // обводка
                e.Graphics.DrawRectangle(pen, barLeft, top, wdTotal, barHeight);
                // маркер для перетаскивания
                markerArea = new Rectangle(barLeft - markerRad, cy - markerRad, markerRad * 2, markerRad * 2);
                e.Graphics.FillEllipse(MarkerHighlighted ? brushFilled : brushEmpty, markerArea);
                e.Graphics.DrawEllipse(pen, markerArea);
                // текст слева
                e.Graphics.DrawString(Value.ToString(), Font, brushText, barLeft - paddingText, cy,
                    new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                // название (тикер)
                e.Graphics.DrawString(Title, FontTitle, brushText, padding, cy,
                    new StringFormat { LineAlignment = StringAlignment.Center });
            }          
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// подсветить маркер
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // перетаскивание
            if (isDragged)
            {
                var barWd = Width - titleWidth - padding - padding - textWidth - paddingText;
                var rightEdge = Width - padding;
                var scaleX = barWd / (double)Math.Max(MaxValue, DefaultValue);
                var newValue = (int)Math.Round((rightEdge - e.X) / scaleX);
                if (newValue > MaxValue) newValue = MaxValue;
                if (newValue < MinValue) newValue = MinValue;

                if (Value != newValue)
                    if (valueChangedByDragging != null)
                        valueChangedByDragging(newValue);
                Value = newValue;
                Invalidate();
                return;
            }

            // подсветка
            var hovered = markerArea.Contains(e.X, e.Y);
            if (hovered == isHovered) return;
            if (MarkerHighlighted == hovered)
            {
                isHovered = hovered;
                return;
            }
            isHovered = hovered;

            // перерисовать
            Invalidate(markerArea);
        }

        /// <summary>
        ///  включить перетаскивание?
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (isDragged || e.Button != MouseButtons.Left) return;
            isDragged = markerArea.Contains(e.X, e.Y);
            if (isDragged)
                Invalidate(markerArea);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!isDragged) return;
            isDragged = false;
            Invalidate(markerArea);
        }
    }
}
