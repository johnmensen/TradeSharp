using System.Windows.Forms;

namespace Candlechart.Core
{
    public abstract class DragTool : InteractivityTool
    {        
        private bool _dragActive;
        private Cursor _dragCursor = Cursors.Default;
        private MouseButtons _mouseDragButton = MouseButtons.Left;
        private bool _prevCanDrag;

        internal bool DragActive
        {
            get { return _dragActive; }
            set
            {
                if (value && !_dragActive)
                {
                    Capture = true;
                }
                if (!value && _dragActive)
                {
                    Capture = false;
                }
                _dragActive = value;
            }
        }

        public Cursor DragCursor
        {
            get { return _dragCursor; }
            set { _dragCursor = value; }
        }

        public MouseButtons MouseDragButton
        {
            get { return _mouseDragButton; }
            set { _mouseDragButton = value; }
        }
        
        public event MouseEventHandler BeginDrag;

        public event MouseEventHandler DoDrag;

        public event MouseEventHandler EndDrag;

        // Methods

        internal virtual bool CanDrag(MouseEventArgs e)
        {
            return true;
        }

        internal abstract void FinishDrag(MouseEventArgs e);

        internal override bool HandleMouseDown(MouseEventArgs e)
        {
            if (((e.Button & MouseDragButton) == MouseButtons.None) || !CanDrag(e))
            {
                return false;
            }
            DragActive = true;
            StartDrag(e);
            if (BeginDrag != null)
            {
                BeginDrag(this, e);
            }
            return true;
        }

        internal override bool HandleMouseMove(MouseEventArgs e)
        {
            if (DragActive)
            {
                ProcessDrag(e);
                if (DoDrag != null)
                {
                    DoDrag(this, e);
                }
                return true;
            }
            if (CanDrag(e))
            {
                SetCursor();
                return true;
            }
            ResetCursor();
            return false;
        }

        internal override bool HandleMouseUp(MouseEventArgs e)
        {
            if (!DragActive)
            {
                return false;
            }
            DragActive = false;
            FinishDrag(e);
            if (EndDrag != null)
            {
                EndDrag(this, e);
            }
            return true;
        }

        internal abstract void ProcessDrag(MouseEventArgs e);

        private void ResetCursor()
        {
            if (_prevCanDrag)
            {
                _prevCanDrag = false;
                Chart.ResetCursor();
            }
        }

        private void SetCursor()
        {
            if (!_prevCanDrag)
            {
                _prevCanDrag = true;
            }
            Chart.Cursor = DragCursor;
        }

        internal abstract void StartDrag(MouseEventArgs e);
    }
}
