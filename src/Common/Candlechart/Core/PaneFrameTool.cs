using System.Drawing;
using System.Windows.Forms;

namespace Candlechart.Core
{
    public class PaneFrameTool : InteractivityTool
    {
        // Fields
        private bool _canMove = true;
        private bool _canResize = true;
        private Cursor _paneMoveCursor = Cursors.Hand;
        private Cursor _paneResizeCursor = Cursors.HSplit;

        // Events

        // Methods
        internal PaneFrameTool(InteractivityToolCollection owner)
        {
            Owner = owner;
            PaneResizeTool = new PaneResizeTool(this) {Owner = owner};
            PaneMoveTool = new PaneMoveTool(this) {Owner = owner};
        }

        // Properties
        public bool CanMove
        {
            get { return _canMove; }
            set { _canMove = value; }
        }

        public bool CanResize
        {
            get { return _canResize; }
            set { _canResize = value; }
        }

        //public Color OutlineColor { get; set; }

        public Cursor PaneMoveCursor
        {
            get { return _paneMoveCursor; }
            set { _paneMoveCursor = value; }
        }

        public PaneMoveStyle PaneMoveStyle { get; set; }

        internal PaneMoveTool PaneMoveTool { get; set; }

        public Cursor PaneResizeCursor
        {
            get { return _paneResizeCursor; }
            set { _paneResizeCursor = value; }
        }

        public PaneResizeStyle PaneResizeStyle { get; set; }

        internal PaneResizeTool PaneResizeTool { get; set; }
        public event PaneMoveEventHandler PaneMove;

        public event PaneResizeEventHandler PaneResize;

        internal void FirePaneMove(PaneMoveEventArgs e)
        {
            if (PaneMove != null)
            {
                PaneMove(this, e);
            }
        }

        internal void FirePaneResize(PaneResizeEventArgs e)
        {
            if (PaneResize != null)
            {
                PaneResize(this, e);
            }
        }

        internal override bool HandleMouseDown(MouseEventArgs e)
        {
            return ((CanResize && PaneResizeTool.HandleMouseDown(e)) || (CanMove && PaneMoveTool.HandleMouseDown(e)));
        }

        internal override bool HandleMouseMove(MouseEventArgs e)
        {
            return ((CanResize && PaneResizeTool.HandleMouseMove(e)) || (CanMove && PaneMoveTool.HandleMouseMove(e)));
        }

        internal override bool HandleMouseUp(MouseEventArgs e)
        {
            return ((CanResize && PaneResizeTool.HandleMouseUp(e)) || (CanMove && PaneMoveTool.HandleMouseUp(e)));
        }
    }
}
