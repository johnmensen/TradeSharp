using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Client.Controls.QuoteTradeControls;

namespace TradeSharp.Client.Controls.Bookmark
{
    public class BookmarkControl : Control
    {
        #region Визуальные настройки 
        private const int TilePad = 3;
        private const int ImageSize = 16;
        private const int TileWmin = 35;
        private const int MaxSymbolsInTitle = 14;
        private const int CloseButtonSize = 12;
        public const int TileHeightNormal = 21;
        public const int TileHeightEdit = 35;

        private int TileH
        {
            get { return modeEdit ? TileHeightEdit : TileHeightNormal; }
        }

        private readonly Color clOutline = SystemColors.ControlText;
        private readonly Color[] clOutlineSel = new Color[3];

        private readonly Dictionary<DrawState, Color> brushColor = new
            Dictionary<DrawState, Color>
            {
                { DrawState.Normal, SystemColors.ButtonFace },
                { DrawState.Hovered, SystemColors.ActiveCaption },
                { DrawState.Overlapped, SystemColors.ControlDark },
                { DrawState.Pressed, SystemColors.ControlLightLight }
            };

        private readonly Font fontSelected;

        private int blinkCounter;

        #endregion

        private TerminalBookmark bookmark;
        public TerminalBookmark Bookmark
        {
            get { return bookmark; }
            set
            {
                bookmark = value;
                if (bookmark != null)
                    toolTip.SetToolTip(this, bookmark.Title);
            }
        }

        private bool modeEdit = true;

        public bool ModeEdit
        {
            get { return modeEdit; }
            set { modeEdit = value; }
        }

        public enum DrawState { Normal = 0, Pressed, Hovered, Overlapped }

        public DrawState BookmarkDrawState { get; set; }

        public ImageList ImageList { get; set; }

        private bool closeBtnVisible;

        private PointF[] pointsCross;

        private Rectangle areaCross;

        public DragDropState DragState { get; set; }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set 
            { 
                selected = value;
                if (selected)
                    IsBlinking = false;
            }
        }

        public bool IsBlinking { get; set; }

        #region События

        private EventHandler clicked;
        private ToolTip toolTip;
        private System.ComponentModel.IContainer components;
    
        public event EventHandler Clicked
        {
            add { clicked += value; }
            remove { clicked -= value; }
        }

        private EventHandler closeClicked;
        public event EventHandler CloseClicked
        {
            add { closeClicked += value; }
            remove { closeClicked -= value; }
        }

        #endregion

        public BookmarkControl()
        {
            MakeCrossPoints();
            fontSelected = new Font(Font, FontStyle.Bold);
            clOutlineSel[0] = clOutline;
            clOutlineSel[1] = MakeContrastColor(clOutline, -40);
            clOutlineSel[2] = MakeContrastColor(clOutline, -80);
            DoubleBuffered = true;
            
            toolTip = new ToolTip();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (BookmarkDrawState == DrawState.Normal && !closeBtnVisible) return;
            closeBtnVisible = false;
            BookmarkDrawState = DrawState.Normal;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            using (var pens = new PenStorage())
            using (var brushes = new BrushesStorage())
            {
                var str = MakeDisplayedString();

                // залить прямоугольник и отрисовать границу
                var outerRectColor = Selected ? clOutlineSel[blinkCounter] : clOutline;
                var pen = pens.GetPen(outerRectColor, BookmarkDrawState == DrawState.Pressed ||
                    DragState != DragDropState.Normal ? 2 : 1);
                
                var brColor = brushColor[BookmarkDrawState];
                if (IsBlinking)
                    brColor = brushColor[(DrawState)blinkCounter];
                var brush = brushes.GetBrush(brColor);
                var area = new Rectangle(new Point(0, 0), new Size(Width - 1, Height - 1));
                e.Graphics.FillRectangle(brush, area);
                e.Graphics.DrawRectangle(pen, area);

                // нарисовать "тень"
                if (Selected || DragState == DragDropState.InFrame)
                {
                    var penSelected = pens.GetPen(clOutlineSel[blinkCounter]);
                    e.Graphics.DrawLine(penSelected, 1, 1, Width - 1, 1);
                    e.Graphics.DrawLine(penSelected, 1, 1, 1, Bottom - 1);
                }

                // нарисовать крестик контрастным
                if (modeEdit)
                {
                    var brushCross = brushes.GetBrush(closeBtnVisible ? clOutline : MakeContrastColor(brColor, 26));
                    var points =
                        pointsCross.Select(p => new PointF(p.X + areaCross.Left, p.Y + areaCross.Top)).ToArray();
                    e.Graphics.FillPolygon(brushCross, points);
                }

                // нарисовать картинку
                var left = TilePad;
                if (Bookmark.ShouldDrawImage && ImageList.Images.Count > Bookmark.ImageIndex && Bookmark.ImageIndex >= 0)
                {
                    e.Graphics.DrawImage(ImageList.Images[Bookmark.ImageIndex], left, (TileH - ImageSize) / 2);
                    left += (ImageSize + TilePad);
                }

                // нарисовать текст
                if (Bookmark.ShouldDrawText && str.Length > 0)
                    e.Graphics.DrawString(str, 
                        Selected ? fontSelected : Font,
                        brushes.GetBrush(clOutline), 
                        left, TileH/2, new StringFormat
                        {
                            LineAlignment = StringAlignment.Center
                        });
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (BookmarkDrawState == DrawState.Normal)
            {
                BookmarkDrawState = DrawState.Hovered;
                Invalidate();
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            if (BookmarkDrawState != DrawState.Pressed)
            {
                BookmarkDrawState = DrawState.Pressed;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        public void FireMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (BookmarkDrawState == DrawState.Pressed)
            {
                BookmarkDrawState = DrawState.Normal;
                Invalidate();
            }

            if (areaCross.Contains(e.X, e.Y))
            {
                if (closeClicked != null)
                    closeClicked(this, e);
            }
            else if (ClientRectangle.Contains(e.X, e.Y))
            {
                if (clicked != null)
                    clicked(this, e);
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var inCross = areaCross.Contains(e.X, e.Y);
            if (closeBtnVisible != inCross)
            {
                closeBtnVisible = inCross;
                Invalidate();
            }
            
            base.OnMouseMove(e);
        }

        public void CalculateSize()
        {
            var str = MakeDisplayedString();

            using (var g = CreateGraphics())
            {
                var w = TilePad * 2;
                if (Bookmark.BookmarkDisplayMode == TerminalBookmark.DisplayMode.TeкстПлюсКартинка)
                    w += TilePad;
                if (Bookmark.ShouldDrawImage)
                    w += ImageSize;
                if (Bookmark.ShouldDrawText)
                    w += (int)Math.Round(g.MeasureString(str, fontSelected).Width);
                if (w < TileWmin) w = TileWmin;
                Size = new Size(w, TileH);
            }

            areaCross = new Rectangle(Width - CloseButtonSize - 2, 2, CloseButtonSize, CloseButtonSize);
        }

        private static Color MakeContrastColor(Color cl, int contrast)
        {
            var r = cl.R - contrast;
            if (r < 0) r = 255 + r;
            else if (r > 255) r -= 255;
            var g = cl.G - contrast;
            if (g < 0) g = 255 + g;
            else if (g > 255) g -= 255;
            var b = cl.B - contrast;
            if (b < 0) b = 255 + b;
            else if (b > 255) b -= 255;

            return Color.FromArgb(255, r, g, b);
        }

        private string MakeDisplayedString()
        {
            var str = Bookmark.Title ?? "";
            if (str.Length > MaxSymbolsInTitle)
                str = str.Substring(0, MaxSymbolsInTitle - 1) + "..";
            return str;
        }

        private void MakeCrossPoints()
        {
            var sz = CloseButtonSize;
            var sz2 = CloseButtonSize / 2f;
            var sz4 = CloseButtonSize / 4f;

            pointsCross = new[]
                {
                    new PointF(sz4, 0),
                    new PointF(sz2, sz4),
                    new PointF(sz2 + sz4, 0),
                    new PointF(sz, sz4),
                    new PointF(sz2 + sz4, sz2),
                    new PointF(sz, sz2 + sz4),
                    new PointF(sz2 + sz4, sz),
                    new PointF(sz2, sz2 + sz4),
                    new PointF(sz4, sz),
                    new PointF(0, sz2 + sz4),
                    new PointF(sz4, sz2),
                    new PointF(0, sz4)
                };
        }
    
        /// <summary>
        /// перевести кнопку в другое состояние, если ей положено моргать или переливаться, перерисовать
        /// </summary>
        public void Blink()
        {
            if (!Selected && !IsBlinking) return;
            blinkCounter++;
            if (blinkCounter > 2) blinkCounter = 0;
            Invalidate();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }
}
