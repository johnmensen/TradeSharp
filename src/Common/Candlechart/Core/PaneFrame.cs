using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Candlechart.Chart;
using FrameStyle = Candlechart.Theme.FrameStyle;

namespace Candlechart.Core
{
    public class PaneFrame
    {
        private static readonly int[] bottom = new[] {1, 2};
        private static readonly int[] left = new[] {1, 2};
        private static readonly int[] right = new[] {1, 2};
        private static readonly int[] top = new[] {1, 2};
        private bool canResize = true;
        private bool showTitle = true;
        private bool? titleBoxVisible;

        internal PaneFrame(Pane owner)
        {
            Owner = owner;
        }

        public string Title { get; set; }

        public bool ShowTitle
        {
            get { return showTitle; }
            set { showTitle = value; }
        }

        internal int BottomBorderWidth
        {
            get { return bottom[(int) Chart.visualSettings.ChartFrameStyle]; }
        }

        public int BottomFrameWidth
        {
            get { return BottomBorderWidth; }
        }

        public bool CanResize
        {
            get { return canResize; }
            set { canResize = value; }
        }

        private ChartControl Chart
        {
            get { return Owner.Owner; }
        }

        public Font Font
        {
            get { return Chart.visualSettings.PaneFrameFont; }
        }

        internal int LeftBorderWidth
        {
            get { return left[(int) Chart.visualSettings.ChartFrameStyle]; }
        }

        public int LeftFrameWidth
        {
            get { return LeftBorderWidth; }
        }

        private Pane Owner { get; set; }

        internal int RightBorderWidth
        {
            get { return right[(int) Chart.visualSettings.ChartFrameStyle]; }
        }

        public int RightFrameWidth
        {
            get { return RightBorderWidth; }
        }

        internal Rectangle TitleBorderRect
        {
            get
            {
                int num = (Owner.Width - LeftBorderWidth) - RightBorderWidth;
                return new Rectangle(LeftBorderWidth, TopBorderWidth, (num > 0) ? num : 0,
                                     ShowTitle ? Font.Height + 8 : 4);
            }
        }

        internal Rectangle TitleBoxRect
        {
            get
            {
                Rectangle titleBorderRect = TitleBorderRect;
                int num = (titleBorderRect.Width - LeftBorderWidth) - RightBorderWidth;
                return new Rectangle(titleBorderRect.X + LeftBorderWidth, titleBorderRect.Y + TopBorderWidth,
                                     (num > 0) ? num : 0, (titleBorderRect.Height - TopBorderWidth) - BottomBorderWidth);
            }
        }

        public bool TitleBoxVisible
        {
            get { return titleBoxVisible ?? Chart.visualSettings.PaneFrameTitleBoxVisible; }
            set { titleBoxVisible = value; }
        }

        internal int TopBorderWidth
        {
            get { return top[(int) Chart.visualSettings.ChartFrameStyle]; }
        }

        public int TopFrameWidth
        {
            get
            {
                if (TitleBoxVisible)
                {
                    return ((TopBorderWidth + Font.Height) + 9);
                }
                return TopBorderWidth;
            }
        }

        internal void Draw(Graphics g)
        {
            DrawBorder(g);
            if (TitleBoxVisible)
            {
                DrawTitleBox(g);
                DrawTitle(g);
            }
        }

        private void DrawBorder(Graphics g)
        {
            var rect = new Rectangle(0, 0, Owner.Width, Owner.Height);
            if (Chart.visualSettings.ChartFrameStyle == FrameStyle.ThreeD)
            {
                Renderer.Draw3DBorder(g, rect, Chart.visualSettings.PaneFrameBorderColor, Border3DStyle.Sunken);
            }
            else if (Chart.visualSettings.ChartFrameStyle == FrameStyle.Flat)
            {
                var pen = new Pen(Chart.visualSettings.PaneFrameBorderColor);
                using (pen)
                {
                    if (rect.Width == 1)
                    {
                        g.DrawLine(pen, 0, 0, 0, rect.Height - 1);
                    }
                    else
                    {
                        g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                    }
                }
            }
        }

        private void DrawTitle(Graphics g)
        {
            try
            {
                GraphicsContainer container = g.BeginContainer();
                Rectangle titleBoxRect = TitleBoxRect;
                if (titleBoxRect.Width < (LeftBorderWidth + RightBorderWidth))
                {
                    titleBoxRect.Width = 0;
                }
                g.SetClip(titleBoxRect);
                RectangleF layoutRectangle = titleBoxRect;
                layoutRectangle.Inflate(-2f, -2f);
                layoutRectangle.Width = 0f;
                var stringFormat = new StringFormat {Alignment = StringAlignment.Near};
                stringFormat.FormatFlags |= StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
                stringFormat.Trimming = StringTrimming.None;
                using (var brush = new SolidBrush(Chart.visualSettings.PaneFrame2TextColor))
                {
                    foreach (Series.Series series in Owner.Series)
                    {
                        SizeF ef3 = g.MeasureString(series.Name, Font, layoutRectangle.Location, stringFormat);
                        layoutRectangle.Offset(layoutRectangle.Width, 0f);
                        layoutRectangle.Width = ef3.Width;
                        layoutRectangle.Intersect(titleBoxRect);
                        string title = series.Name;
                        if (!string.IsNullOrEmpty(Title)) title = Title;
                        g.DrawString(title, Font, brush, layoutRectangle, stringFormat);
                        if (string.IsNullOrEmpty(Title))
                        {
                            // если заголовок не задан отдельно - вывести результирующую
                            ef3 = g.MeasureString(series.CurrentPriceString + "0", Font, layoutRectangle.Location,
                                                    stringFormat);
                            layoutRectangle.Offset(layoutRectangle.Width, 0f);
                            layoutRectangle.Width = ef3.Width;
                            layoutRectangle.Intersect(titleBoxRect);
                            g.DrawString(series.CurrentPriceString, Font, brush, layoutRectangle, stringFormat);
                        }
                        break; // вывести только первую
                    }                    
                }
                g.EndContainer(container);
            }
            catch
            {
            }
        }

        private void DrawTitleBox(Graphics g)
        {
            GraphicsContainer container = g.BeginContainer();
            Rectangle titleBorderRect = TitleBorderRect;
            g.SetClip(new Rectangle(titleBorderRect.X, titleBorderRect.Y, titleBorderRect.Width,
                                    titleBorderRect.Bottom + 1));
            int upHeight = titleBorderRect.Height/2,
                downHeight = titleBorderRect.Height - upHeight;
            var upBox = new Rectangle(titleBorderRect.X, titleBorderRect.Y, titleBorderRect.Width, upHeight);
            var downBox = new Rectangle(titleBorderRect.X, titleBorderRect.Y + upHeight, titleBorderRect.Width,
                                        downHeight);
            if (Chart.visualSettings.ChartFrameStyle == FrameStyle.ThreeD)
            {
                var pen = new Pen(ControlPaint.Dark(Chart.visualSettings.PaneFrameBorderColor, 0.5f));
                var brushUp = new LinearGradientBrush(new Point(0, 0), new Point(0, upHeight + 3),
                                                      Chart.visualSettings.PaneFrameBorderMarginGradientColor,
                                                      Chart.visualSettings.PaneFrameBorderCenterGradientColor);
                var brushDn = new LinearGradientBrush(new Point(0, 0), new Point(0, downHeight),
                                                      Chart.visualSettings.PaneFrameBorderCenterGradientColor,
                                                      Chart.visualSettings.PaneFrameBorderMarginGradientColor);
                using (pen)
                {
                    using (brushUp)
                    {
                        using (brushDn)
                        {
                            Renderer.Draw3DBorder(g, titleBorderRect, Chart.visualSettings.PaneFrameBorderColor,
                                                  Border3DStyle.Raised);
                            g.DrawLine(pen, titleBorderRect.Left, titleBorderRect.Bottom, titleBorderRect.Right - 1,
                                       titleBorderRect.Bottom);
                            g.FillRectangle(brushUp, upBox);
                            g.FillRectangle(brushDn, downBox);
                        }
                    }
                }
            }
            else if (Chart.visualSettings.ChartFrameStyle == FrameStyle.Flat)
            {
                var pen2 = new Pen(Chart.visualSettings.PaneFrameBorderColor);
                var brush2 = new SolidBrush(Chart.visualSettings.PaneFrameBackColor);
                using (pen2)
                {
                    using (brush2)
                    {
                        g.FillRectangle(brush2, titleBorderRect);
                        g.DrawLine(pen2, titleBorderRect.Left, titleBorderRect.Bottom, titleBorderRect.Right - 1,
                                   titleBorderRect.Bottom);
                    }
                }
            }
            g.EndContainer(container);
        }
    }
}