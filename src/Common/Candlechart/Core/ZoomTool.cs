using System.Drawing;
using System.Windows.Forms;
using Candlechart.ChartGraphics;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    public class ZoomTool : DragTool
    {
        private MemGraphics backBuffer;
        private SolidBrush brush;
        private MemGraphics frontBuffer;
        private Pen pen;
        private Point _previousLoc;
        private Graphics screenGraphics;
        private Point _startLoc;
        private Cursor zoomCursor = Cursors.Default;
        public Color BackColor { get; set; }

        internal Pane CurrentPane
        {
            get { return Owner.CurrentPane; }
        }

        public Color ForeColor { get; set; }

        public Cursor ZoomCursor
        {
            get { return zoomCursor; }
            set { zoomCursor = value; }
        }

        internal override bool CanDrag(MouseEventArgs e)
        {
            // запрещаем драг при взведенном соотв. флаге
            if (Chart.skipZoomming)
            {
                Chart.skipZoomming = false;
                return false;
            }

            // запрещаем зум при нажатом шифте
            if (CurrentPane != null && ((Control.ModifierKeys & Keys.Shift) != Keys.Shift))
            {
                Point pt = Conversion.ChildToParent(new Point(e.X, e.Y), CurrentPane.ClientRect);
                if (CurrentPane.CanvasRect.Contains(pt))
                {
                    DragCursor = ZoomCursor;
                    return true;
                }
            }
            
            return false;
        }

        private void DrawSelection(Point previousLoc, Point currentLoc)
        {
            Point point = CurrentPane.ClientToChart(_startLoc);
            previousLoc = CurrentPane.ClientToChart(previousLoc);
            currentLoc = CurrentPane.ClientToChart(currentLoc);
            Rectangle rectangle = CurrentPane.ClientToChart(CurrentPane.CanvasRect);
            Rectangle rectangle2 = previousLoc.X > point.X 
                                       ? new Rectangle(point.X, rectangle.Y, (previousLoc.X - point.X) + 1, rectangle.Height) 
                                       : new Rectangle(previousLoc.X, rectangle.Y, (point.X - previousLoc.X) + 1, rectangle.Height);
            Rectangle rectangle3 = currentLoc.X > point.X 
                                       ? new Rectangle(point.X, rectangle.Y, (currentLoc.X - point.X) + 1, rectangle.Height) 
                                       : new Rectangle(currentLoc.X, rectangle.Y, (point.X - currentLoc.X) + 1, rectangle.Height);
            backBuffer.Render(frontBuffer, rectangle2);
            frontBuffer.Graphics.FillRectangle(brush, rectangle3);
            frontBuffer.Graphics.DrawLine(pen, rectangle3.Left, rectangle3.Top, rectangle3.Left, rectangle3.Bottom);
            if (rectangle3.Width > 0)
            {
                frontBuffer.Graphics.DrawLine(pen, rectangle3.Right - 1, rectangle3.Top, rectangle3.Right - 1,
                                               rectangle3.Bottom);
            }
            frontBuffer.Render(screenGraphics, Rectangle.Union(rectangle2, rectangle3));
        }

        internal override void FinishDrag(MouseEventArgs e)
        {
            screenGraphics.Dispose();
            backBuffer.Dispose();
            frontBuffer.Dispose();
            pen.Dispose();
            brush.Dispose();

            var endLoc = new Point(e.X, e.Y);
            if (endLoc.X < CurrentPane.CanvasRect.Left)
            {
                endLoc.X = CurrentPane.CanvasRect.Left;
            }
            if (endLoc.X > (CurrentPane.CanvasRect.Right - 1))
            {
                endLoc.X = CurrentPane.CanvasRect.Right - 1;
            }
            SetWorldExtent(_startLoc, endLoc);
            Chart.Invalidate();
        }

        internal override void ProcessDrag(MouseEventArgs e)
        {
            var currentLoc = new Point(e.X, e.Y);
            if (currentLoc.X < CurrentPane.CanvasRect.Left)
            {
                currentLoc.X = CurrentPane.CanvasRect.Left;
            }
            if (currentLoc.X > (CurrentPane.CanvasRect.Right - 1))
            {
                currentLoc.X = CurrentPane.CanvasRect.Right - 1;
            }
            DrawSelection(_previousLoc, currentLoc);
            _previousLoc = currentLoc;
        }

        private void SetWorldExtent(Point startLoc, Point endLoc)
        {
            if (endLoc.X >= startLoc.X)
            {
                startLoc = Conversion.ParentToChild(startLoc, CurrentPane.CanvasRect);
                endLoc = Conversion.ParentToChild(endLoc, CurrentPane.CanvasRect);
                var screenRect = new RectangleF(0f, 0f, CurrentPane.CanvasRect.Width, CurrentPane.CanvasRect.Height);
                PointD td = Conversion.ScreenToWorld(startLoc, CurrentPane.WorldRect, screenRect);
                PointD td2 = Conversion.ScreenToWorld(endLoc, CurrentPane.WorldRect, screenRect);
                int leftPos = Chart.StockSeries.FindNearestRightBar(td.X);
                int rightPos = Chart.StockSeries.FindNearestLeftBar(td2.X);
                if ((rightPos - leftPos) >= 0)
                {
                    Chart.SetScrollView(leftPos, rightPos);
                }
            }
            else
            {
                // при движении мышкой справа налево, просто помещаем весь график в окне
                Chart.FitChart();
            }
        }

        internal override void StartDrag(MouseEventArgs e)
        {
            _startLoc = new Point(e.X, e.Y);
            _previousLoc = new Point(e.X, e.Y);
            screenGraphics = Chart.CreateGraphics();
            backBuffer = new MemGraphics(Chart.Width, Chart.Height);
            frontBuffer = new MemGraphics(Chart.Width, Chart.Height);
            Chart.Draw(backBuffer.Graphics);
            var rect = CurrentPane.ClientToChart(CurrentPane.CanvasRect);
            frontBuffer.Graphics.SetClip(rect);
            pen = new Pen(ForeColor);
            brush = new SolidBrush(BackColor);
            DrawSelection(_startLoc, _startLoc);
        }        
    }
}