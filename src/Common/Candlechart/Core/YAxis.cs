using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Candlechart.ChartMath;
using Candlechart.Theme;
using Entity;

namespace Candlechart.Core
{
    public class YAxis : Axis
    {
        private readonly ExponentLabel exponentLabel;
        
        private readonly YAxisLabelInfo labelInfo;

        public double? SelectedLabelPrice { get; set; }

        /// <summary>
        /// точка, с которой пользователь начал растягивать / сжимать масштаб графика
        /// </summary>
        private Point? startDragPoint;
        private Point? currentDragPoint;

        public bool DragModeIsOn
        {
            get { return startDragPoint.HasValue; }
            set
            {
                if (!value)
                {
                    startDragPoint = null;
                    currentDragPoint = null;
                }
            }
        }

        internal YAxis(Pane owner)
            : base(owner)
        {
            NumberDecimalDigitsAuto = true;
            MinAuto = true;
            MaxAuto = true;
            labelInfo = new YAxisLabelInfo();
            LeftAxisRect = new Rectangle(0, 0, 0, 0);
            RightAxisRect = new Rectangle(0, 0, 0, 0);
            exponentLabel = new ExponentLabel(this);
        }

        public ExponentLabel ExponentLabel
        {
            get { return exponentLabel; }
        }

        internal YAxisLabelInfo LabelInfo
        {
            get { return labelInfo; }
        }

        internal Rectangle LeftAxisRect { get; set; }

        public double Max { get; set; }

        public bool MaxAuto { get; set; }

        public double Min { get; set; }

        public bool MinAuto { get; set; }

        public bool MinorTickVisible { get; set; }

        public int NumberDecimalDigits { get; set; }

        public bool NumberDecimalDigitsAuto { get; set; }

        internal Rectangle RightAxisRect { get; set; }

        public float? CurrentPrice { get; set; }

        public override Color BackColor
        {
            get { return Chart.visualSettings.YAxisBackColor; }
        }

        public override Color ForeColor
        {
            get { return Chart.visualSettings.YAxisForeColor; }
        }

        public override Color TextColor
        {
            get { return Chart.visualSettings.YAxisTextColor; }
        }

        public override Font Font
        {
            get { return Chart.visualSettings.XAxisFont; }
        }

        public override Color GridLineColor
        {
            get { return Chart.visualSettings.YAxisGridLineColor; }
        }

        public override GridLineStyle GridLineStyle
        {
            get { return Chart.visualSettings.YAxisGridLineStyle; }
        }

        public override bool GridLineVisible
        {
            get { return Chart.visualSettings.YAxisGridLineVisible; }
        }

        public override Color GridBandColor
        {
            get { return Chart.visualSettings.YAxisGridBandColor; }
        }

        public override bool GridBandVisible
        {
            get { return Chart.visualSettings.YAxisGridBandVisible; }
        }

        private void CalculateAxisWidth(Graphics g)
        {
            double num = double.IsInfinity(LabelInfo.Exponent) ? 0 
                : Owner.WorldRect.Top / LabelInfo.Exponent;
            double num2 = double.IsInfinity(LabelInfo.Exponent) ? 0 
                : Owner.WorldRect.Bottom / LabelInfo.Exponent;
            double num3 = (Math.Abs(num) > Math.Abs(num2)) ? Math.Abs(num) : Math.Abs(num2);
            if (num3 < double.Epsilon || double.IsNaN(num3))
            {
                num3 = 1.0;
            }
            double y = Math.Sign(num3) * (Math.Ceiling(Math.Abs(Math.Log10(num3))) + 1.0);
            double num5 = 5.0 * Math.Pow(10.0, y);
            SizeF ef = g.MeasureString("00000.00", Font);
            SizeF ef2 = g.MeasureString(num5.ToString("N", PrepareNumberFormat()), Font);
            float num6 = (ef2.Width > ef.Width) ? ef2.Width : ef.Width;
            if (num6 > Chart.YAxisWidth)
            {
                Chart.YAxisWidth = (int)Math.Ceiling(num6);
            }
        }

        private void DetermineLabelInfo()
        {
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            double num = Math.Floor(Math.Log10(worldRect.Height));
            double num2 = 1.0;
            if (Math.Abs(num) >= 3.0)
            {
                double num3 = Math.Floor((Math.Abs(num) / 3.0)) - 1.0;
                num2 = Math.Pow(10.0, (Math.Sign(num) * num3) * 3.0);
            }
            LabelInfo.Exponent = num2;
            LabelInfo.MajorStep = Conversion.ScreenToWorld(new SizeD(0.0, Font.Height), worldRect, canvasRect).Height;
            double y = Math.Ceiling(Math.Log10(LabelInfo.MajorStep));
            YAxisLabelInfo labelInfo = LabelInfo;
            labelInfo.MajorStep /= Math.Pow(10.0, y);
            if (LabelInfo.MajorStep >= 0.5)
            {
                LabelInfo.MajorStep = 1.0;
                LabelInfo.MinorStep = 0.5;
            }
            else if (LabelInfo.MajorStep >= 0.2)
            {
                LabelInfo.MajorStep = 0.5;
                LabelInfo.MinorStep = 0.10000000149011612;
            }
            else if (LabelInfo.MajorStep >= 0.1)
            {
                LabelInfo.MajorStep = 0.20000000298023224;
                LabelInfo.MinorStep = 0.10000000149011612;
            }
            else
            {
                LabelInfo.MajorStep = 0.10000000149011612;
                LabelInfo.MinorStep = 0.10000000149011612;
            }
            YAxisLabelInfo info2 = LabelInfo;
            info2.MajorStep *= Math.Pow(10.0, y);
            YAxisLabelInfo info3 = LabelInfo;
            info3.MinorStep *= Math.Pow(10.0, y);
            LabelInfo.Max = Math.Ceiling(((worldRect.Y + worldRect.Height) / LabelInfo.MajorStep)) * LabelInfo.MajorStep;
            LabelInfo.Min = Math.Floor((worldRect.Y / LabelInfo.MajorStep)) * LabelInfo.MajorStep;
        }

        private void DetermineNumberDecimalDigits()
        {
            if (NumberDecimalDigitsAuto)
            {
                try
                {
                    var num = (int)Math.Floor(Math.Log10((LabelInfo.MajorStep * 1.01) / LabelInfo.Exponent));
                    if (num < 0)
                    {
                        NumberDecimalDigits = Math.Abs(num);
                    }
                    else if (num >= 0)
                    {
                        NumberDecimalDigits = 0;
                    }
                }
                catch
                {// может быть пустой интервал
                }
            }
        }

        internal override void Draw(Graphics g)
        {
            if (Owner.CanvasRect.Height == 0) return;

            using (var brushes = new BrushesStorage())
            {
                if (Chart.YAxisAlignment == YAxisAlignment.Left)
                {
                    DrawBackground(g, brushes, LeftAxisRect, YAxisAlignment.Left);
                    DrawGrid(g, brushes);
                    DrawTicks(g, LeftAxisRect, YAxisAlignment.Left);
                    DrawLabels(g, brushes, LeftAxisRect, YAxisAlignment.Left);
                }
                else if (Chart.YAxisAlignment == YAxisAlignment.Right)
                {
                    DrawBackground(g, brushes, RightAxisRect, YAxisAlignment.Right);
                    DrawGrid(g, brushes);
                    DrawTicks(g, RightAxisRect, YAxisAlignment.Right);
                    DrawLabels(g, brushes, RightAxisRect, YAxisAlignment.Right);
                }
                else
                {
                    DrawBackground(g, brushes, RightAxisRect, YAxisAlignment.Right);
                    DrawTicks(g, RightAxisRect, YAxisAlignment.Right);
                    DrawLabels(g, brushes, RightAxisRect, YAxisAlignment.Right);
                    DrawBackground(g, brushes, LeftAxisRect, YAxisAlignment.Left);
                    DrawGrid(g, brushes);
                    DrawTicks(g, LeftAxisRect, YAxisAlignment.Left);
                    DrawLabels(g, brushes, LeftAxisRect, YAxisAlignment.Left);
                }
                // нарисовать точки, откуда пользователь начал "перетягивать" курсором масштаб
                DrawDragScaleMarks(g, brushes);
            }
        }

        private void DrawBackground(Graphics g, BrushesStorage brushes,
            Rectangle axisRect, YAxisAlignment alignment)
        {
            var brush = brushes.GetBrush(BackColor);
            var pen = new Pen(ForeColor);
            g.FillRectangle(brush, axisRect);
            using (pen)
            {
                if (alignment == YAxisAlignment.Left)
                {
                    g.DrawLine(pen, axisRect.Right - 1, axisRect.Top, axisRect.Right - 1, axisRect.Bottom - 1);
                }
                else if (alignment == YAxisAlignment.Right)
                {
                    g.DrawLine(pen, axisRect.Left, axisRect.Top, axisRect.Left, axisRect.Bottom - 1);
                }
            }
        }

        private void DrawGrid(Graphics g, BrushesStorage brushes)
        {
            DrawYAxisGrid(g, brushes);
            DrawXAxisGrid(g, brushes);
        }

        private void DrawLabels(Graphics g,
            BrushesStorage brushes,
            Rectangle axisRect, YAxisAlignment alignment)
        {
            var canvasRect = Owner.CanvasRect;
            var worldRect = Owner.WorldRect;
            var format = PrepareStringFormat(alignment);
            var provider = PrepareNumberFormat();
            var brush = brushes.GetBrush(TextColor);
            
            using (format)
            {
                var rect = new Rectangle(axisRect.Left + 5, 0, ((axisRect.Width - 5) - 5) - 1, Font.Height);
                for (double i = LabelInfo.Max; i > LabelInfo.Min; i -= LabelInfo.MajorStep)
                {
                    var tf = (PointF)Conversion.WorldToScreen(new PointD(0.0, i), worldRect, canvasRect);
                    rect.Y = ((int)tf.Y) - (Font.Height / 2);
                    double num2 = i / LabelInfo.Exponent;
                    if (axisRect.Contains(rect))
                    {
                        g.DrawString(num2.ToString("N", provider), Font, brush, rect, format);
                    }
                }
                if (CurrentPrice.HasValue)
                {
                    // рисуем текущую цену в шкале
                    DrawSelectedPriceLabel(CurrentPrice.Value,
                        brush, format, axisRect, worldRect, canvasRect, g);
                }

                if (SelectedLabelPrice.HasValue)
                {
                    // отметка цены для перекрестия
                    DrawSelectedPriceLabel(SelectedLabelPrice.Value,
                        brush, format, axisRect, worldRect, canvasRect, g);
                }
                ExponentLabel.Draw(g, axisRect, alignment);
            }            
        }

        private void DrawSelectedPriceLabel(
            double labelPrice,
            Brush brush, 
            StringFormat format, Rectangle axisRect, 
            RectangleD worldRect, Rectangle canvasRect, Graphics g)
        {
            var price = labelPrice / LabelInfo.Exponent;
            var priceFont = new Font(Font.Name, Font.Size - 1,
                                     Font.Style, Font.Unit);
            var rect = new Rectangle(axisRect.Left + 5, 0, 
                axisRect.Width - (int)priceFont.SizeInPoints, priceFont.Height);
            var tf = (PointF)Conversion.WorldToScreen(new PointD(0.0, price), worldRect, canvasRect);
            rect.Y = ((int)tf.Y) - (priceFont.Height / 2);
            if (axisRect.Contains(rect))
            {
                var pen = new Pen(ForeColor);
                var fillbrush = new SolidBrush(BackColor);
                using(fillbrush)
                    g.FillRectangle(fillbrush, rect);
                using (pen)
                    g.DrawRectangle(pen, rect);
                g.DrawString(labelPrice.ToString(Chart.PriceFormat), priceFont, brush, rect, format);
            }
        }

        private void DrawDragScaleMarks(Graphics g, BrushesStorage brushes)
        {
            if (!startDragPoint.HasValue) return;
            
            // найти X-координату (точно в центре области, занимаемой отметками оси)
            var xCoord = startDragPoint.Value.X;
            if (Chart.YAxisAlignment == YAxisAlignment.Left)
                xCoord = (int)(LeftAxisRect.Left + LeftAxisRect.Width * 0.5);
            else if (Chart.YAxisAlignment == YAxisAlignment.Right)
                xCoord = (int)(RightAxisRect.Left + RightAxisRect.Width * 0.5);
            else
            {
                var left = (int)(LeftAxisRect.Left + LeftAxisRect.Width * 0.5);
                var right = (int)(RightAxisRect.Left + RightAxisRect.Width * 0.5);
                var deltaLeft = Math.Abs(xCoord - left);
                var deltaRight = Math.Abs(xCoord - right);
                xCoord = deltaLeft < deltaRight ? left : right;
            }

            // нарисовать отметку начала перетягивания
            const int pointStartR = 5, pointStartD = 10;
            var brush = brushes.GetBrush(Chart.visualSettings.SeriesForeColor);
            var ptCenter = new Point(xCoord, startDragPoint.Value.Y);
            g.FillEllipse(brush, ptCenter.X - pointStartR, ptCenter.Y - pointStartR, pointStartD, pointStartD);
            
            // нарисовать отметку текущей позиции курсора
            if (!currentDragPoint.HasValue) return;
            const int shapeWd2 = 5, shapeHt2 = 4, shapeHt = 7;
            
            var x = currentDragPoint.Value.X;
            var y = currentDragPoint.Value.Y;
            var dir = y > startDragPoint.Value.Y ? 1 : -1;

            var points = new []
                             {
                                 new Point(x - shapeWd2, y - shapeHt2 * dir), 
                                 new Point(x + shapeWd2, y - shapeHt2 * dir), 
                                 new Point(x, y + shapeHt * dir)
                             };
            g.FillPolygon(brush, points);
        }

        private void DrawTicks(Graphics g, Rectangle axisRect, YAxisAlignment alignment)
        {
            if (Chart.visualSettings.YAxisMajorTickVisible || Chart.visualSettings.YAxisMinorTickVisible)
            {
                Rectangle canvasRect = Owner.CanvasRect;
                RectangleD worldRect = Owner.WorldRect;
                var pen = new Pen(ForeColor);
                using (pen)
                {
                    float left;
                    float num2;
                    float num3;
                    float num4;
                    if (alignment == YAxisAlignment.Left)
                    {
                        left = ((axisRect.Right - 1) - 5) + 1;
                        num2 = axisRect.Right - 1;
                        num3 = ((axisRect.Right - 1) - 3) + 1;
                        num4 = axisRect.Right - 1;
                    }
                    else
                    {
                        left = axisRect.Left;
                        num2 = (axisRect.Left + 5) - 1;
                        num3 = axisRect.Left;
                        num4 = (axisRect.Left + 3) - 1;
                    }
                    for (double i = LabelInfo.Max; i > LabelInfo.Min; i -= LabelInfo.MajorStep)
                    {
                        var tf = (PointF)Conversion.WorldToScreen(new PointD(0.0, i), worldRect, canvasRect);
                        if (Chart.visualSettings.YAxisMajorTickVisible)
                        {
                            g.DrawLine(pen, left, tf.Y, num2, tf.Y);
                        }
                        if (Chart.visualSettings.YAxisMinorTickVisible)
                        {
                            double y = i;
                            while (y > (i - LabelInfo.MajorStep))
                            {
                                y -= LabelInfo.MinorStep;
                                tf = (PointF)Conversion.WorldToScreen(new PointD(0.0, y), worldRect, canvasRect);
                                g.DrawLine(pen, num3, tf.Y, num4, tf.Y);
                            }
                        }
                    }
                }
            }
        }        
        
        private void DrawXAxisGrid(Graphics g, BrushesStorage brushes)
        {
            Rectangle canvasRect = Owner.CanvasRect;
            RectangleD worldRect = Owner.WorldRect;
            GraphicsState gstate = g.Save();
            g.SetClip(canvasRect);
            if ((Chart.XAxis.GridLineVisible || Chart.XAxis.GridBandVisible) &&
                (Chart.XAxis.Grid.Count > 0))
            {
                var pen = new Pen(Chart.XAxis.GridLineColor);
                var brush = brushes.GetBrush(Chart.XAxis.GridBandColor);
                pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), Chart.XAxis.GridLineStyle.ToString());
                using (pen)
                {                    
                    bool flag = true;
                    XAxisLabelInfo info;
                    for (int i = 0; i < Chart.XAxis.Grid.Count; i++)
                    {
                        info = (XAxisLabelInfo)Chart.XAxis.Grid[i];
                        var tf = (PointF)Conversion.WorldToScreen(new PointD(info.X, 0.0), worldRect, canvasRect);
                        if (Chart.XAxis.GridBandVisible)
                        {
                            if (flag)
                            {
                                PointF tf2;
                                if (i != (Chart.XAxis.Grid.Count - 1))
                                {
                                    var info2 = (XAxisLabelInfo)Chart.XAxis.Grid[i + 1];
                                    tf2 =
                                        (PointF)
                                        Conversion.WorldToScreen(new PointD(info2.X, 0.0), worldRect, canvasRect);
                                }
                                else
                                {
                                    tf2 = new PointF(canvasRect.Right, canvasRect.Top);
                                }
                                g.FillRectangle(brush, tf.X, 0f, tf2.X - tf.X, canvasRect.Height);
                            }
                            else if ((i == 0) && (tf.X > canvasRect.Left))
                            {
                                g.FillRectangle(brush, canvasRect.Left, 0f, tf.X - canvasRect.Left,
                                                canvasRect.Height);
                            }
                            flag = !flag;
                        }
                        if (Chart.XAxis.GridLineVisible)
                        {
                            g.DrawLine(pen, tf.X, 0f, tf.X, (canvasRect.Height - 1));
                        }
                    }                    
                }
            }            
            g.Restore(gstate);
        }

        private void DrawYAxisGrid(Graphics g, BrushesStorage brushes)
        {
            if (GridLineVisible || GridBandVisible)
            {
                Rectangle canvasRect = Owner.CanvasRect;
                RectangleD worldRect = Owner.WorldRect;
                var pen = new Pen(GridLineColor);
                var brush = brushes.GetBrush(GridBandColor);
                pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), GridLineStyle.ToString());
                using (pen)
                {
                    var ef =
                        (SizeF)Conversion.WorldToScreen(new SizeD(0.0, LabelInfo.MajorStep), worldRect, canvasRect);
                    float left = canvasRect.Left;
                    float right = canvasRect.Right;
                    right = (right < left) ? left : right;
                    double max = LabelInfo.Max;
                    bool flag = true;
                    while (max > LabelInfo.Min)
                    {
                        var tf = (PointF)Conversion.WorldToScreen(new PointD(0.0, max), worldRect, canvasRect);
                        if ((GridBandVisible && (GridBandColor != Color.Empty)) && flag)
                        {
                            g.FillRectangle(brush, canvasRect.Left, tf.Y, canvasRect.Width, ef.Height);
                        }
                        if (GridLineVisible)
                        {
                            g.DrawLine(pen, left, tf.Y, right, tf.Y);
                        }
                        flag = !flag;
                        max -= LabelInfo.MajorStep;
                    }
                }                
            }
        }

        internal NumberFormatInfo PrepareNumberFormat()
        {
            var info = new NumberFormatInfo {NumberGroupSeparator = "", NumberDecimalDigits = NumberDecimalDigits};
            return info;
        }

        internal static StringFormat PrepareStringFormat(YAxisAlignment alignment)
        {
            var format = new StringFormat
                             {
                                 Alignment =
                                     (alignment == YAxisAlignment.Left ? StringAlignment.Far : StringAlignment.Near),
                                 Trimming = StringTrimming.None
                             };
            format.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
            return format;
        }

        internal void PrepareToDraw(Graphics g)
        {
            if (Owner.CanvasRect.Height != 0)
            {
                DetermineLabelInfo();
                DetermineNumberDecimalDigits();
                CalculateAxisWidth(g);
            }
        }

        internal void TrimCanvas()
        {
            Rectangle clientRect = Owner.ClientRect;
            Rectangle canvasRect = Owner.CanvasRect;
            if (Chart.YAxisAlignment == YAxisAlignment.Left)
            {
                LeftAxisRect = new Rectangle(0, 0, Chart.YAxisWidth, clientRect.Height);
                canvasRect = new Rectangle(Chart.YAxisWidth, canvasRect.Y, canvasRect.Width - Chart.YAxisWidth,
                                           canvasRect.Height);
            }
            else if (Chart.YAxisAlignment == YAxisAlignment.Right)
            {
                RightAxisRect = new Rectangle(clientRect.Width - Chart.YAxisWidth, 0, Chart.YAxisWidth,
                                              clientRect.Height);
                canvasRect = new Rectangle(canvasRect.X, canvasRect.Y, canvasRect.Width - Chart.YAxisWidth,
                                           canvasRect.Height);
            }
            else if (Chart.YAxisAlignment == YAxisAlignment.Both)
            {
                LeftAxisRect = new Rectangle(0, 0, Chart.YAxisWidth, clientRect.Height);
                RightAxisRect = new Rectangle(clientRect.Width - Chart.YAxisWidth, 0, Chart.YAxisWidth,
                                              clientRect.Height);
                canvasRect = new Rectangle(Chart.YAxisWidth, canvasRect.Y,
                                           canvasRect.Width - (Chart.YAxisWidth * 2), canvasRect.Height);
            }
            Owner.CanvasRect = canvasRect;
        }        
    
        public bool ContainsCursor(int x, int y)
        {
            var contains =
                Chart.YAxisAlignment == YAxisAlignment.Left
                    ? LeftAxisRect.Contains(x, y)
                    : Chart.YAxisAlignment == YAxisAlignment.Right
                          ? RightAxisRect.Contains(x, y)
                          : LeftAxisRect.Contains(x, y) || RightAxisRect.Contains(x, y);            
            
            if (contains) startDragPoint = new Point(x, y);
            return contains;
        }

        public bool ChangeScaleByDragCursor(int x, int y)
        {
            return ChangeScaleOrShiftYByDragCursor(x, y, true);            
        }

        public bool ChangeShiftYByDragCursor(int x, int y)
        {
            return ChangeScaleOrShiftYByDragCursor(x, y, false);
        }

        private bool ChangeScaleOrShiftYByDragCursor(int x, int y, bool scaleNotShift)
        {
            currentDragPoint = new Point(x, y);
            if (!startDragPoint.HasValue) return false;
            var deltaY = startDragPoint.Value.Y - y;
            var deltaWorldY = deltaY * 0.006D;
            
            return scaleNotShift 
                ? Owner.SetScale(1 + deltaWorldY)
                : Owner.SetShiftYrelative(deltaWorldY);
        }
    }
}
