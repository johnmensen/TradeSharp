using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using TradeSharp.Util.Properties;

namespace TradeSharp.Util.Controls
{
    // базовый класс изменен на Panel для правильного размещения компонентов внутри себя,
    // и поэтому дизайнер отказывается визуализировать компоненты
    [Designer(typeof(CollapsiblePanelDesigner))]
    [ToolboxBitmap(typeof(CollapsiblePanel), "TradeSharp.Util.Controls.CollapsiblePanel.bmp")]
    [DefaultProperty("HeaderText")]
    public partial class CollapsiblePanel : Panel
    {
        #region "Private members"

        private bool collapse;
        private int originalHight;
        private bool useAnimation;
        private bool showHeaderSeparator = true;
        private bool roundedCorners;
        private int headerCornersRadius = 10;
        private bool headerTextAutoEllipsis;
        private string headerText;
        private Color headerTextColor;
        private Image headerImage;
        private Font headerFont;
        private RectangleF toolTipRectangle;
        private bool useToolTip;

        #endregion 

        #region "Public Properties"

        [Browsable(false)]
        public new Color BackColor
        {
            get
            {
                return Color.Transparent;
            }
            set
            {
                base.BackColor = Color.Transparent;
            }
        }

        [DefaultValue(false)]
        [Description("Collapses the control when set to true")]
        [Category("CollapsiblePanel")]
        public bool Collapse
        {
            get { return collapse; }
            set 
            {
                // If using animation make sure to ignore requests for collapse or expand while a previous
                // operation is in progress.
                if (useAnimation)
                {
                    // An operation is already in progress.
                    if (timerAnimation.Enabled)
                    {
                        return;
                    }
                }
                collapse = value;
                CollapseOrExpand();
                Refresh();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("Height after expand")]
        public int OriginalHeight
        {
            get { return originalHight; }
            set
            {
                originalHight = value;
                if (!collapse)
                    Height = originalHight;
            }
        }


        [DefaultValue(50)]
        [Category("CollapsiblePanel")]
        [Description("Specifies the speed (in ms) of Expand/Collapse operation when using animation. UseAnimation property must be set to true.")]
        public int AnimationInterval
        {
            get 
            {
                return timerAnimation.Interval ;
            }
            set
            {
                // Update animation interval only during idle times.
                if(!timerAnimation.Enabled )
                    timerAnimation.Interval = value;
            }
        }

        [DefaultValue(false)]
        [Category("CollapsiblePanel")]
        [Description("Indicate if the panel uses amination during Expand/Collapse operation")]
        public bool UseAnimation
        {
            get { return useAnimation; }
            set { useAnimation = value; }
        }

        [DefaultValue(true)]
        [Category("CollapsiblePanel")]
        [Description("When set to true draws panel borders, and shows a line separating the panel's header from the rest of the control")]
        public bool ShowHeaderSeparator
        {
            get { return showHeaderSeparator; }
            set
            {
                showHeaderSeparator = value;
                Refresh();
            }
        }

        [DefaultValue(false)]
        [Category("CollapsiblePanel")]
        [Description("When set to true, draws a panel with rounded top corners, the radius can bet set through HeaderCornersRadius property")]
        public bool RoundedCorners
        {
            get
            {
                return roundedCorners;
            }
            set
            {
                roundedCorners = value;
                Refresh();
            }
        }

        [DefaultValue(10)]
        [Category("CollapsiblePanel")]
        [Description("Top corners radius, it should be in [1, 15] range")]
        public int HeaderCornersRadius
        {
            get
            {
                return headerCornersRadius;
            }
            set
            {
                if (value < 1 || value > 15)
                    throw new ArgumentOutOfRangeException("HeaderCornersRadius", value, "Value should be in range [1, 90]");
                else
                {
                    headerCornersRadius = value;
                    Refresh();
                }
            }
        }

        [DefaultValue(false)]
        [Category("CollapsiblePanel")]
        [Description("Enables the automatic handling of text that extends beyond the width of the label control.")]
        public bool HeaderTextAutoEllipsis
        {
            get { return headerTextAutoEllipsis; }
            set
            {
                headerTextAutoEllipsis = value;
                Refresh();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("Text to show in panel's header")]
        public string HeaderText
        {
            get { return headerText; }
            set
            {
                headerText = value;
                Refresh();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("Color of text header, and panel's borders when ShowHeaderSeparator is set to true")]
        public Color HeaderTextColor
        {
            get { return headerTextColor; }
            set
            {
                headerTextColor = value;
                Refresh();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("Image that will be displayed in the top left corner of the panel")]
        public Image HeaderImage
        {
            get { return headerImage; }
            set
            {
                headerImage = value;
                Refresh();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("The font used to display text in the panel's header.")]
        public Font HeaderFont
        {
            get { return headerFont; }
            set
            {
                headerFont = value;
                Refresh();
            }
        }

        [Category("CollapsiblePanel")]
        [Description("Show expand/collapse icon box in header.")]
        public bool ShowExpandCollapseBox
        {
            get { return pictureBoxExpandCollapse.Visible; }
            set { pictureBoxExpandCollapse.Visible = value; }
        }

        #endregion 

        public CollapsiblePanel()
        {
            InitializeComponent();
            pnlHeader.Width = Width -1;
            headerFont = new Font(Font, FontStyle.Bold);
            headerTextColor = Color.Black;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawHeaderPanel(e);
        }

        public void DrawHeaderCorners(Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            var gp = new GraphicsPath();
            gp.AddLine(x + radius, y, x + width - (radius * 2), y); // Line
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90); // Corner
            gp.AddLine(x + width, y + radius, x + width, y + height ); // Line
            gp.AddLine(x + width , y + height, x , y + height); // Line
            gp.AddLine(x, y + height , x, y + radius); // Line
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Corner
            gp.CloseFigure();
            g.FillPath(brush, gp);
            if (showHeaderSeparator)
            {
                g.DrawPath(new Pen(headerTextColor), gp);
            }
            gp.Dispose();
        }
       
        private void DrawHeaderPanel(PaintEventArgs e)
        {
            var headerRect = pnlHeader.ClientRectangle;
            if (headerRect.Width > 0 && headerRect.Height > 0)
            {
                var headerBrush = new LinearGradientBrush(
                    headerRect, Color.Snow, Color.LightBlue, LinearGradientMode.Horizontal);

                if (!roundedCorners)
                {
                    e.Graphics.FillRectangle(headerBrush, headerRect);
                    if (showHeaderSeparator)
                    {
                        e.Graphics.DrawRectangle(new Pen(headerTextColor), headerRect);
                    }
                }
                else
                    DrawHeaderCorners(e.Graphics, headerBrush, headerRect.X, headerRect.Y,
                                        headerRect.Width, headerRect.Height, headerCornersRadius);
            }

            // Draw header separator
            if (showHeaderSeparator)
            {
                var start = new Point(pnlHeader.Location.X, pnlHeader.Location.Y + pnlHeader.Height);
                var end = new Point(pnlHeader.Location.X + pnlHeader.Width, pnlHeader.Location.Y + pnlHeader.Height);
                e.Graphics.DrawLine(new Pen(headerTextColor, 2), start, end);
                // Draw rectangle lines for the rest of the control.
                var bodyRect = ClientRectangle;
                bodyRect.Y += pnlHeader.Height;
                bodyRect.Height -= (pnlHeader.Height + 1);
                bodyRect.Width -= 1;
                if (bodyRect.Width > 0 && bodyRect.Height > 0)
                    e.Graphics.DrawRectangle(new Pen(headerTextColor), bodyRect);
            }

            var headerRectHeight = pnlHeader.Height;
            // Draw header image.
            if (headerImage != null)
            {
                pictureBoxImage.Image = headerImage;
                pictureBoxImage.Visible = true;
            }
            else
            {
                pictureBoxImage.Image = null;
                pictureBoxImage.Visible = false;
            }

            // Calculate header string position.
            if (!string.IsNullOrEmpty(headerText))
            {
                useToolTip = false;
                var delta = pictureBoxExpandCollapse.Width + 5;
                var offset = 0;
                if (headerImage != null)
                {
                    offset = headerRectHeight;
                }
                var headerTextSize = TextRenderer.MeasureText(headerText, headerFont);
                if (headerTextAutoEllipsis && headerTextSize.Width >= headerRect.Width - (delta + offset))
                {
                    var rectLayout =
                        new RectangleF((float) headerRect.X + offset,
                                       (float) (headerRect.Height - headerTextSize.Height) / 2,
                                       (float) headerRect.Width - delta,
                                       (float) headerTextSize.Height);
                    var format = new StringFormat {Trimming = StringTrimming.EllipsisWord};
                    e.Graphics.DrawString(headerText, headerFont, new SolidBrush(headerTextColor),
                                          rectLayout, format);

                    toolTipRectangle = rectLayout;
                    useToolTip = true;
                }
                else
                {
                    var headerTextPosition = new PointF
                        {
                            X = (offset + headerRect.Width - headerTextSize.Width) / 2,
                            Y = (headerRect.Height - headerTextSize.Height) / 2
                        };
                    e.Graphics.DrawString(headerText, headerFont, new SolidBrush(headerTextColor),
                                          headerTextPosition);
                }
            }
        }

        private void PictureBoxExpandCollapseClick(object sender, EventArgs e)
        {
            Collapse = !Collapse;
        }

        private void CollapseOrExpand()
        {
            if (!useAnimation)
            {
                if (collapse)
                {
                    originalHight = Height;
                    Height = pnlHeader.Height/* + 3*/;
                    pictureBoxExpandCollapse.Image = Resources.expand;
                }
                else
                {
                    Height = originalHight;
                    pictureBoxExpandCollapse.Image = Resources.collapse;
                }
            }
            else
            {
                // Keep original height only in case of a collapse operation.
                if (collapse)
                    originalHight = Height;
                timerAnimation.Enabled = true;
                timerAnimation.Start();
            }
        }

        private void PictureBoxExpandCollapseMouseMove(object sender, MouseEventArgs e)
        {
            if (timerAnimation.Enabled)
                return;
            pictureBoxExpandCollapse.Image = !collapse ? Resources.collapse_hightlight : Resources.expand_highlight;
        }

        private void PictureBoxExpandCollapseMouseLeave(object sender, EventArgs e)
        {
            if (timerAnimation.Enabled)
                return;
            pictureBoxExpandCollapse.Image = !collapse ? Resources.collapse : Resources.expand;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            pnlHeader.Width = Width -1;
            Refresh();
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                return new Rectangle(0, pnlHeader.Height, Width, Height - pnlHeader.Height);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            pnlHeader.Width = Width -1;
            Refresh();
        }

        private void TimerAnimationTick(object sender, EventArgs e)
        {
            if (collapse)
            {
                var destHeight = pnlHeader.Height/* + 3*/;
                if (Height <= destHeight)
                {
                    timerAnimation.Stop();
                    timerAnimation.Enabled = false;
                    pictureBoxExpandCollapse.Image = Resources.expand;
                }
                else
                {
                    var newHight = Height - 20;
                    if (newHight <= destHeight)
                        newHight = destHeight;
                    Height = newHight;
                }
            }
            else
            {
                if (Height >= originalHight)
                {
                    timerAnimation.Stop();
                    timerAnimation.Enabled = false;
                    pictureBoxExpandCollapse.Image = Resources.collapse;
                }
                else
                {
                    var newHeight = Height + 20;
                    if (newHeight >= originalHight)
                        newHeight = originalHight;
                    Height = newHeight;
                }
            }
        }

        private void PnlHeaderMouseHover(object sender, EventArgs e)
        {
            if (!useToolTip) return;
            var p = PointToClient(MousePosition);
            if (toolTipRectangle.Contains(p))
            {
                toolTip.Show(headerText, pnlHeader, p);
            }
        }

        private void PnlHeaderMouseLeave(object sender, EventArgs e)
        {
            if (!useToolTip) return;
            var p = PointToClient(MousePosition);
            if (!toolTipRectangle.Contains(p))
            {
                toolTip.Hide(pnlHeader);
            }
        }

        private void PnlHeaderClick(object sender, EventArgs e)
        {
            if (!pictureBoxExpandCollapse.Visible)
                Collapse = !Collapse;
        }
    }
}
