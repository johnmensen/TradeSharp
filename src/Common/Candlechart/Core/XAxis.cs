using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Theme;

namespace Candlechart.Core
{
    public class XAxis : Axis
    {
        private readonly ArrayList _grid;
        
        private readonly ArrayList _majorLabels;
        
        private readonly ArrayList _minorLabels;

        public double? SelectedLabelX { get; set; }

        private const int ShiftMarkerSize = 6;

        private static readonly Point[] shiftMarkerPoints = new[] { new Point(0, -ShiftMarkerSize), new Point(-ShiftMarkerSize, 0), new Point(0, ShiftMarkerSize) };

        #region Интерактивное поведение
        /// <summary>
        /// смещение крайней правой свечки от правого края, свеч
        /// </summary>
        public int ShiftFromRight { get; set; }

        private int? shiftBarDisplacement;

        #endregion

        internal XAxis(Pane owner)
            : base(owner)
        {
            Period = Period.Other;
            _majorLabels = new ArrayList();
            _minorLabels = new ArrayList();
            _grid = new ArrayList();
        }

        internal Rectangle AxisRect
        {
            get
            {
                Rectangle clientRect = Owner.ClientRect;
                clientRect.Offset(-clientRect.Left, -clientRect.Top);
                return clientRect;
            }
        }

        internal int FixedHeight
        {
            get { return Font.Height + 4; }
        }

        internal ArrayList Grid
        {
            get { return _grid; }
        }

        private int LastIndex { get; set; }

        internal ArrayList MajorLabels
        {
            get { return _majorLabels; }
        }

        internal ArrayList MinorLabels
        {
            get { return _minorLabels; }
        }

        public Period Period { get; set; }

        private int StartIndex { get; set; }

        public override Color BackColor
        {
            get { return Chart.visualSettings.XAxisBackColor; }            
        }

        public override Color ForeColor
        {
            get { return Chart.visualSettings.XAxisForeColor; }
        }

        public override Color TextColor
        {
            get { return Chart.visualSettings.XAxisTextColor; }
        }

        public override Font Font
        {
            get { return Chart.visualSettings.XAxisFont; }
        }

        public override Color GridLineColor
        {
            get { return Chart.visualSettings.XAxisGridLineColor; }
        }

        public override GridLineStyle GridLineStyle
        {
            get { return Chart.visualSettings.XAxisGridLineStyle; }
        }

        public override bool GridLineVisible
        {
            get { return Chart.visualSettings.XAxisGridLineVisible; }
        }

        public override Color GridBandColor
        {
            get { return Chart.visualSettings.XAxisGridBandColor; }
        }

        public override bool GridBandVisible
        {
            get { return Chart.visualSettings.XAxisGridBandVisible; }
        }

        private void DetermineRange()
        {
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            PointF tf = PointD.Truncate(Conversion.ScreenToWorld(new PointD(0.0, 0.0), worldRect, canvasRect));
            PointF tf2 = PointD.Ceiling(Conversion.ScreenToWorld(new PointD(AxisRect.Right, 0.0), worldRect, canvasRect));
            StartIndex = (int)tf.X;
            LastIndex = (int)tf2.X;
        }

        internal override void Draw(Graphics g)
        {
            DetermineRange();
            DrawBackground(g);
            DrawLabels(g);
            DrawShiftBarsLabel(g);
        }

        private void DrawBackground(Graphics g)
        {
            var brush = new SolidBrush(BackColor);
            using (brush)
            {
                g.FillRectangle(brush, AxisRect);
            }
        }
        
        private bool ShouldDrawChart()
        {
            if (Chart.StockSeries != null)
                if (Chart.StockSeries.Data.Count > 0)
                    return true;            
            return false;
        }

        /// <summary>
        /// Рисовать по отметке под каждый бар графика. 
        /// Если метка налезает на предыдущую - пропустить
        /// </summary>        
        private void DrawLabels(Graphics g)
        {
            if (!ShouldDrawChart()) return;
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            int rightTextMargin = -1;
            var fmt = new StringFormat {Alignment = StringAlignment.Center};

            using (var brush = new SolidBrush(TextColor))
            {
                using (var pen = new Pen(ForeColor))
                {
                    for (int i = StartIndex; i < LastIndex; i++)
                    {
                        int x = i;
                        var labelTime = GetLabelDate(ref x, worldRect, canvasRect);
                        var dateStr = labelTime.ToString("dd/MM:HH.mm");

                        int textW = (int)g.MeasureString(dateStr, Chart.Font).Width / 2 + 1;
                        if (rightTextMargin > 0)
                        {
                            int dx = x - rightTextMargin;
                            if (dx < textW) continue;
                        }
                        rightTextMargin = x + textW;

                        g.DrawString(dateStr, Chart.Font, brush, x, 3, fmt);
                        g.DrawLine(pen, x, 0, x, 3);
                    }

                    // метка для перекрестия
                    if (SelectedLabelX.HasValue)
                    {
                        var x = (int) (SelectedLabelX.Value + 0.5);
                        var labelTime = GetLabelDate(ref x, worldRect, canvasRect);
                        var dateStr = labelTime.ToString("dd.MM.yyyy HH:mm");
                        
                        // нарисовать обводку
                        var sz = g.MeasureString(dateStr, Chart.Font);
                        var rect = new Rectangle(x - (int) (sz.Width/2),
                                                 3/* - (int) (sz.Height/2)*/, 
                                                 (int) sz.Width, (int) sz.Height);
                        rect.Inflate(4, 2);
                        using (var br = new SolidBrush(BackColor))
                            g.FillRectangle(br, rect);
                        g.DrawRectangle(pen, rect);

                        // написать текст
                        g.DrawString(dateStr, Chart.Font, brush, x, 3, fmt);
                        g.DrawLine(pen, x, 0, x, 3);
                    }
                }
            }
        }

        private DateTime GetLabelDate(ref int index, RectangleD worldRect, Rectangle canvasRect)
        {
            if (index < 0 || index >= Chart.StockSeries.Data.Count)
                return DateTime.Now;
            DateTime date = Chart.StockSeries.Data[index].timeOpen;
            index = PointD.Round(Conversion.WorldToScreen(
                new PointD(index, 0.0), worldRect, canvasRect)).X;
            return date;
        }
    
        private void DrawShiftBarsLabel(Graphics g)
        {
            var x = Chart.StockSeries.Data.Count - 1 + (shiftBarDisplacement ?? 0);
            x = (int)(Conversion.WorldToScreen(new PointD(x, 0.0), Owner.WorldRect, Owner.CanvasRect).X);
            
            // нарисовать маркер (треугольник)
            using (var brush = new SolidBrush(ForeColor))
            {
                var markPoints = shiftMarkerPoints.Select(p => new Point(x + p.X, 4 + 6 + p.Y)).ToArray();
                g.FillPolygon(brush, markPoints);
            }
        }

        #region Перетаскивание крайнего бара
        public void StartDragRightBar(MouseEventArgs e)
        {
            var deltaX = Owner.ClientRect.X;
            var deltaY = Owner.Owner.ClientRect.Bottom - AxisRect.Height;
            var mX = e.X - deltaX;
            var mY = e.Y - deltaY;
            if (!AxisRect.Contains(mX, mY)) return;

            shiftBarDisplacement = null;
            
            // определить попадание
            var x = Chart.StockSeries.Data.Count - 1 + (shiftBarDisplacement ?? 0);
            x = (int)(Conversion.WorldToScreen(new PointD(x, 0.0), Owner.WorldRect, Owner.CanvasRect).X);
            var delta = x - e.X;

            if (delta > (-ShiftMarkerSize / 2) && delta <= ShiftMarkerSize) 
                shiftBarDisplacement = 0;
        }

        public void DoDragRightBar(MouseEventArgs e)
        {
            if (!shiftBarDisplacement.HasValue) return;

            var deltaX = Owner.ClientRect.X;
            var newPos = (int)Math.Round(Conversion.ScreenToWorld(new PointD(e.X - deltaX, e.Y), Owner.WorldRect, Owner.CanvasRect).X);
            newPos = newPos - (Chart.StockSeries.Data.Count - 1);
            if (newPos == shiftBarDisplacement) return;
            if (newPos > Owner.Owner.RightBars) return;

            shiftBarDisplacement = newPos;
            
            // перерисовать
            deltaX = Owner.ClientRect.X;
            var deltaY = Owner.Owner.ClientRect.Bottom - AxisRect.Height;
            Owner.Owner.Invalidate(new Rectangle(deltaX, deltaY, AxisRect.Width, AxisRect.Height));
        }

        public void EndDragLastBar()
        {
            if (!shiftBarDisplacement.HasValue || shiftBarDisplacement.Value == 0) return;
            // для графика обновить значение - сдвиг вправо
            Owner.Owner.RightBars = Owner.Owner.RightBars - shiftBarDisplacement.Value;
            
            // обновить границы чарта
            var oldStartIndex = Owner.Owner.Window.LeftPos;
            var startIndex = oldStartIndex - shiftBarDisplacement.Value;
            shiftBarDisplacement = null;
            var endIndex = Owner.Owner.StockSeries.DataCount + Owner.Owner.RightBars - 1;
            if (startIndex >= Owner.Owner.StockSeries.DataCount)
                startIndex = Owner.Owner.StockSeries.DataCount - 1;
            if (startIndex >= endIndex) startIndex = endIndex - 1;

            Owner.Owner.Window.UpdateScrollLimits(0, Owner.Owner.StockSeries.DataCount + Owner.Owner.RightBars - 1);
            Owner.Owner.Window.SetScrollView(startIndex, endIndex);
            Owner.Owner.Invalidate();
        }
        #endregion
    }
}
