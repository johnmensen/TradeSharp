using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Candlechart.ChartGraphics;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    internal class PaneMoveTool : DragTool
    {
        // Fields
        private MemGraphics _backBuffer;
        private ColorMatrix _colorMatrix;
        private MemGraphics _frontBuffer;
        private ImageAttributes _imageAttr;
        private float _opacity = 0.5f;
        private Pen _pen;
        private Point _previousLoc;
        private Graphics _screenGraphics;
        private Point _startLoc;

        // Methods
        internal PaneMoveTool(PaneFrameTool paneFrameTool)
        {
            PaneFrameTool = paneFrameTool;
        }

        internal Pane CurrentPane
        {
            get { return Owner.CurrentPane; }
        }

        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        internal PaneFrameTool PaneFrameTool { get; set; }

        internal override bool CanDrag(MouseEventArgs e)
        {
            if (CurrentPane != null)
            {
                if (!CurrentPane.PaneFrame.TitleBoxVisible)
                {
                    return false;
                }
                Point pt = Conversion.ChildToParent(new Point(e.X, e.Y), CurrentPane.ClientRect);
                if (CurrentPane.PaneFrame.TitleBoxRect.Contains(pt))
                {
                    DragCursor = PaneFrameTool.PaneMoveCursor;
                    return true;
                }
            }
            return false;
        }

        private void DrawImage(Point previousLoc, Point currentLoc)
        {
            Rectangle srcRect = Conversion.ChildToParent(CurrentPane.Bounds, Chart.ClientRect);
            int num = currentLoc.Y - _startLoc.Y;
            int x = currentLoc.X - _startLoc.X;
            var rect = new Rectangle(x, CurrentPane.Top + num, CurrentPane.Width, CurrentPane.Height);
            rect = Conversion.ChildToParent(rect, Chart.ClientRect);
            num = previousLoc.Y - _startLoc.Y;
            x = previousLoc.X - _startLoc.X;
            var rectangle3 = new Rectangle(x, CurrentPane.Top + num, CurrentPane.Width, CurrentPane.Height);
            rectangle3 = Conversion.ChildToParent(rectangle3, Chart.ClientRect);
            _colorMatrix.Matrix33 = Opacity;
            _imageAttr.SetColorMatrix(_colorMatrix);
            _backBuffer.Render(_frontBuffer, rectangle3);
            _backBuffer.Render(_frontBuffer, srcRect, rect, _imageAttr);
            _frontBuffer.Render(_screenGraphics, Rectangle.Union(rect, rectangle3));
        }

        private void DrawOutline(Point previousLoc, Point currentLoc)
        {
            int num = currentLoc.Y - _startLoc.Y;
            int x = currentLoc.X - _startLoc.X;
            var rect = new Rectangle(x, CurrentPane.Top + num, CurrentPane.Width, CurrentPane.Height);
            rect = Conversion.ChildToParent(rect, Chart.ClientRect);
            rect.Offset(CurrentPane.PaneFrame.LeftBorderWidth, CurrentPane.PaneFrame.TopBorderWidth);
            rect.Width -= CurrentPane.PaneFrame.LeftBorderWidth + CurrentPane.PaneFrame.RightBorderWidth;
            rect.Height -= CurrentPane.PaneFrame.TopBorderWidth + CurrentPane.PaneFrame.BottomBorderWidth;
            Rectangle rectangle2 = Rectangle.Inflate(rect, -1, -1);
            num = previousLoc.Y - _startLoc.Y;
            x = previousLoc.X - _startLoc.X;
            var rectangle3 = new Rectangle(x, CurrentPane.Top + num, CurrentPane.Width, CurrentPane.Height);
            rectangle3 = Conversion.ChildToParent(rectangle3, Chart.ClientRect);
            _backBuffer.Render(_frontBuffer, rectangle3);
            _frontBuffer.Graphics.DrawRectangle(_pen, rectangle2);
            _frontBuffer.Render(_screenGraphics, Rectangle.Union(rect, rectangle3));
        }

        internal override void FinishDrag(MouseEventArgs e)
        {
            if (PaneFrameTool.PaneMoveStyle == PaneMoveStyle.Blend)
            {
                _imageAttr.Dispose();
                _imageAttr = null;
                _colorMatrix = null;
            }
            else
            {
                _pen.Dispose();
            }
            _screenGraphics.Dispose();
            _backBuffer.Dispose();
            _frontBuffer.Dispose();
            Point point = CurrentPane.ClientToChart(new Point(e.X, e.Y));
            Pane topPane;
            if (point.Y < Chart.ClientRect.Top)
            {
                topPane = Chart.Panes.TopPane;
            }
            else if (point.Y > ((Chart.ClientRect.Bottom - 1) - Chart.Panes.XAxisPane.FixedHeight))
            {
                topPane = Chart.Panes.BottomPane;
            }
            else
            {
                topPane = Owner.FindPane(Chart.ClientRect.Left, point.Y);
            }
            if ((topPane != CurrentPane) && (topPane != null))
            {
                int index = Chart.Panes.PositionList.IndexOf(CurrentPane);
                if (Chart.Panes.PositionList.IndexOf(topPane) > index)
                {
                    Chart.Panes.PositionList.Remove(CurrentPane);
                    int num3 = Chart.Panes.PositionList.IndexOf(topPane);
                    Chart.Panes.PositionList.Insert(num3 + 1, CurrentPane);
                }
                else
                {
                    Chart.Panes.PositionList.Remove(CurrentPane);
                    int num4 = Chart.Panes.PositionList.IndexOf(topPane);
                    Chart.Panes.PositionList.Insert(num4, CurrentPane);
                }
            }
            Chart.PerformLayout();
        }

        internal override void ProcessDrag(MouseEventArgs e)
        {
            if (PaneFrameTool.PaneMoveStyle == PaneMoveStyle.Outline)
            {
                DrawOutline(_previousLoc, new Point(e.X, e.Y));
            }
            else
            {
                DrawImage(_previousLoc, new Point(e.X, e.Y));
            }
            _previousLoc = new Point(e.X, e.Y);
            PaneFrameTool.FirePaneMove(new PaneMoveEventArgs(CurrentPane));
        }

        internal override void StartDrag(MouseEventArgs e)
        {
            _startLoc = new Point(e.X, e.Y);
            _previousLoc = new Point(e.X, e.Y);
            _screenGraphics = Chart.CreateGraphics();
            _backBuffer = new MemGraphics(Chart.Width, Chart.Height);
            _frontBuffer = new MemGraphics(Chart.Width, Chart.Height);
            Chart.Draw(_backBuffer.Graphics);
            Rectangle clientRect = Chart.ClientRect;
            clientRect.Offset(CurrentPane.PaneFrame.LeftBorderWidth, CurrentPane.PaneFrame.TopBorderWidth);
            clientRect.Width -= CurrentPane.PaneFrame.LeftBorderWidth + CurrentPane.PaneFrame.RightBorderWidth;
            clientRect.Height -= CurrentPane.PaneFrame.TopBorderWidth + CurrentPane.PaneFrame.BottomBorderWidth;
            clientRect.Height -= Chart.Panes.XAxisPane.FixedHeight;
            _frontBuffer.Graphics.SetClip(clientRect);
            if (PaneFrameTool.PaneMoveStyle == PaneMoveStyle.Outline)
            {
                _pen = new Pen(Chart.visualSettings.PaneFrameToolOutlineColor, 2f);
                DrawOutline(_previousLoc, _startLoc);
            }
            else
            {
                _imageAttr = new ImageAttributes();
                _colorMatrix = new ColorMatrix();
                DrawImage(_previousLoc, _startLoc);
            }
        }

        // Properties
    }
}
