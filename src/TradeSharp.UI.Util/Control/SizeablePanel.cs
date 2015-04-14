using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.UI.Util.Control
{
    public class SizeablePanel : Panel
    {
        private const int CGripSize = 20;
        
        private Point mDragPos;
        
        private bool mDragging;

        private enum DraggedSide { Top = 0, Left, Right, Bottom }

        private DraggedSide draggedSide;

        private bool sizableTop = true;
        [DisplayName("SizableTop")]
        [Category("SizeablePanel")]
        [Description("Top edge is sizable")]
        public bool SizableTop
        {
            get { return sizableTop; }
            set { sizableTop = value; }
        }

        [DisplayName("SizableBottom")]
        [Category("SizeablePanel")]
        [Description("Bottom edge is sizable")]
        public bool SizableBottom { get; set; }

        [DisplayName("SizableLeft")]
        [Category("SizeablePanel")]
        [Description("Left edge is sizable")]
        public bool SizableLeft { get; set; }

        [DisplayName("SizableRight")]
        [Category("SizeablePanel")]
        [Description("Right edge is sizable")]
        public bool SizableRight { get; set; }

        private int minDimension = 5;
        [DisplayName("MinDimension")]
        [Category("SizeablePanel")]
        [Description("Minimum width or height")]
        public int MinDimension
        {
            get { return minDimension; }
            set { minDimension = value; }
        }

        public SizeablePanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);            
        }

        private bool IsOnGrip(Point pos)
        {
            if (SizableTop)
                if (pos.Y <= CGripSize)
                {
                    draggedSide = DraggedSide.Top;                    
                    return true;
                }
            if (SizableBottom)            
                if (pos.Y >= ClientSize.Height - CGripSize)
                {
                    draggedSide = DraggedSide.Bottom;
                    return true;
                }
            if (SizableLeft)
                if (pos.X <= CGripSize)
                {
                    draggedSide = DraggedSide.Left;
                    return true;
                }
            if (SizableRight)
                if (pos.X >= ClientSize.Width - CGripSize)
                {
                    draggedSide = DraggedSide.Right;
                    return true;
                }
            
            return false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mDragging = IsOnGrip(e.Location);
            mDragPos = e.Location;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mDragging = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mDragging)
            {
                if (draggedSide == DraggedSide.Right)                
                    Width = Width + e.X - mDragPos.X;
                else if (draggedSide == DraggedSide.Bottom)
                    Height = Height + e.Y - mDragPos.Y;
                else if (draggedSide == DraggedSide.Left)
                {
                    var deltaLeft = e.X - Left;
                    var newWidth = Width + deltaLeft;
                    if (newWidth >= minDimension)
                    {
                        Width = Width + deltaLeft;
                        Left = e.X;
                    }
                }
                else if (draggedSide == DraggedSide.Top)
                {
                    var deltaTop = (CGripSize / 2) - e.Y;
                    var newHeight = Height + deltaTop;
                    if (newHeight >= minDimension)
                    {
                        Height = newHeight;
                        Top -= deltaTop;
                    }
                }
                mDragPos = e.Location;
            }
            else 
                if (IsOnGrip(e.Location))
                    Cursor = (draggedSide == DraggedSide.Right || draggedSide == DraggedSide.Left) ? Cursors.SizeWE : Cursors.SizeNS;
                else 
                    Cursor = Cursors.Default;
            
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!mDragging)
                if (Cursor != Cursors.Default)
                    Cursor = Cursors.Default;
        }
    }
}