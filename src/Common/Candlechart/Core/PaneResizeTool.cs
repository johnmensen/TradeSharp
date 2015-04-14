using System.Drawing;
using System.Windows.Forms;
using Candlechart.ChartGraphics;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    internal class PaneResizeTool : DragTool
    {
        // Fields
        private MemGraphics _backBuffer;
        private MemGraphics _frontBuffer;
        private PaneResizeEventArgs _paneResizeEventArgs;
        private Pen _pen;
        private Graphics _screenGraphics;

        // Methods
        internal PaneResizeTool(PaneFrameTool paneFrameTool)
        {
            PaneFrameTool = paneFrameTool;
        }

        internal Pane CurrentPane
        {
            get { return Owner.CurrentPane; }
        }

        internal PaneFrameTool PaneFrameTool { get; set; }

        internal override bool CanDrag(MouseEventArgs e)
        {
            bool flag = HitTestPaneBottomBorder(e.Y);
            bool flag2 = HitTestPaneTopBorder(e.Y);
            if (!flag && !flag2)
            {
                return false;
            }
            DragCursor = PaneFrameTool.PaneResizeCursor;
            return true;
        }

        private void DrawOutline(Rectangle prevRect, Rectangle curRect)
        {
            prevRect = Conversion.ChildToParent(prevRect, Chart.ClientRect);
            curRect = Conversion.ChildToParent(curRect, Chart.ClientRect);
            var rect = new Rectangle(prevRect.Left, prevRect.Bottom - 2, prevRect.Width, 4);
            var rectangle2 = new Rectangle(curRect.Left, curRect.Bottom - 2, curRect.Width, 4);
            _backBuffer.Render(_frontBuffer, rect);
            _frontBuffer.Graphics.DrawLine(_pen, curRect.Left, curRect.Bottom, curRect.Right, curRect.Bottom);
            _frontBuffer.Render(_screenGraphics, rect);
            _frontBuffer.Render(_screenGraphics, rectangle2);
        }

        internal override void FinishDrag(MouseEventArgs e)
        {
            _paneResizeEventArgs = null;
            Chart.Panes.RecalculatePercentHeight();
            Chart.Invalidate();
            if (PaneFrameTool.PaneResizeStyle == PaneResizeStyle.Outline)
            {
                _pen.Dispose();
                _screenGraphics.Dispose();
                _backBuffer.Dispose();
                _frontBuffer.Dispose();
            }
        }

        private bool HitTestPaneBottomBorder(int y)
        {
            if ((CurrentPane == null) || (CurrentPane == Chart.Panes.BottomPane))
            {
                return false;
            }
            int index = Chart.Panes.PositionList.IndexOf(CurrentPane);
            var pane = (Pane)Chart.Panes.PositionList[index + 1];
            if (!CurrentPane.PaneFrame.CanResize || !pane.PaneFrame.CanResize)
            {
                return false;
            }
            var rect = new Rectangle(new Point(0, 0), CurrentPane.Bounds.Size);
            rect = Conversion.ParentToChild(rect, CurrentPane.ClientRect);
            return ((y >= (rect.Bottom - 3)) && (y <= (rect.Bottom - 1)));
        }

        private bool HitTestPaneTopBorder(int y)
        {
            if ((CurrentPane == null) || (CurrentPane == Chart.Panes.TopPane))
            {
                return false;
            }
            int index = Chart.Panes.PositionList.IndexOf(CurrentPane);
            var pane = (Pane)Chart.Panes.PositionList[index - 1];
            if (!CurrentPane.PaneFrame.CanResize || !pane.PaneFrame.CanResize)
            {
                return false;
            }
            var rect = new Rectangle(new Point(0, 0), CurrentPane.Bounds.Size);
            rect = Conversion.ParentToChild(rect, CurrentPane.ClientRect);
            return ((y >= rect.Top) && (y <= (rect.Top + 3)));
        }

        internal override void ProcessDrag(MouseEventArgs e)
        {
            Rectangle bounds = _paneResizeEventArgs.AffectedPane.Bounds;
            _paneResizeEventArgs.Current = CurrentPane.PointToScreen(new Point(e.X, e.Y));
            int num = _paneResizeEventArgs.Current.Y - _paneResizeEventArgs.Start.Y;
            int minimumPaneHeight = _paneResizeEventArgs.AffectedPaneInitialHeight + num;
            int num3 = _paneResizeEventArgs.AdjacentPaneInitialHeight - num;
            if (minimumPaneHeight < _paneResizeEventArgs.AffectedPane.MinimumPaneHeight)
            {
                minimumPaneHeight = _paneResizeEventArgs.AffectedPane.MinimumPaneHeight;
                num3 = (_paneResizeEventArgs.AdjacentPaneInitialHeight + _paneResizeEventArgs.AffectedPaneInitialHeight) -
                       _paneResizeEventArgs.AffectedPane.MinimumPaneHeight;
            }
            if (num3 < _paneResizeEventArgs.AdjacentPane.MinimumPaneHeight)
            {
                num3 = _paneResizeEventArgs.AdjacentPane.MinimumPaneHeight;
                minimumPaneHeight = (_paneResizeEventArgs.AffectedPaneInitialHeight +
                                     _paneResizeEventArgs.AdjacentPaneInitialHeight) -
                                    _paneResizeEventArgs.AffectedPane.MinimumPaneHeight;
            }
            _paneResizeEventArgs.AffectedPane.Height = minimumPaneHeight;
            _paneResizeEventArgs.AdjacentPane.Height = num3;
            _paneResizeEventArgs.AdjacentPane.Top = (_paneResizeEventArgs.AffectedPane.Top +
                                                     _paneResizeEventArgs.AffectedPane.Height) + Chart.InterPaneGap;
            Rectangle curRect = _paneResizeEventArgs.AffectedPane.Bounds;
            Rectangle rect = _paneResizeEventArgs.AffectedPane.Bounds;
            rect.Height += _paneResizeEventArgs.AdjacentPane.Height;
            rect = Conversion.ChildToParent(rect, Chart.ClientRect);
            if (PaneFrameTool.PaneResizeStyle == PaneResizeStyle.Instantaneous)
            {
                Chart.Invalidate(rect);
            }
            else
            {
                DrawOutline(bounds, curRect);
            }
            PaneFrameTool.FirePaneResize(_paneResizeEventArgs);
        }

        internal override void StartDrag(MouseEventArgs e)
        {
            Pane currentPane;
            Pane pane2;
            bool flag = HitTestPaneBottomBorder(e.Y);
            HitTestPaneTopBorder(e.Y);
            int index = Chart.Panes.PositionList.IndexOf(CurrentPane);
            if (flag)
            {
                currentPane = CurrentPane;
                pane2 = (Pane)Chart.Panes.PositionList[index + 1];
            }
            else
            {
                currentPane = (Pane)Chart.Panes.PositionList[index - 1];
                pane2 = CurrentPane;
            }
            _paneResizeEventArgs = new PaneResizeEventArgs(currentPane, pane2)
                                       {
                                           Start = CurrentPane.PointToScreen(new Point(e.X, e.Y)),
                                           Current = CurrentPane.PointToScreen(new Point(e.X, e.Y)),
                                           AffectedPaneInitialHeight = currentPane.Height,
                                           AdjacentPaneInitialHeight = pane2.Height
                                       };
            if (PaneFrameTool.PaneResizeStyle == PaneResizeStyle.Outline)
            {
                _screenGraphics = Chart.CreateGraphics();
                _backBuffer = new MemGraphics(Chart.Width, Chart.Height);
                _frontBuffer = new MemGraphics(Chart.Width, Chart.Height);
                _pen = new Pen(Chart.visualSettings.PaneFrameToolOutlineColor, 2f);
                Chart.Draw(_backBuffer.Graphics);
                Rectangle clientRect = Chart.ClientRect;
                clientRect.Offset(CurrentPane.PaneFrame.LeftBorderWidth, CurrentPane.PaneFrame.TopBorderWidth);
                clientRect.Width -= CurrentPane.PaneFrame.LeftBorderWidth + CurrentPane.PaneFrame.RightBorderWidth;
                clientRect.Height -= CurrentPane.PaneFrame.TopBorderWidth + CurrentPane.PaneFrame.BottomBorderWidth;
                _frontBuffer.Graphics.SetClip(clientRect);
                DrawOutline(_paneResizeEventArgs.AffectedPane.Bounds, _paneResizeEventArgs.AffectedPane.Bounds);
            }
        }

        // Properties
    }
}
