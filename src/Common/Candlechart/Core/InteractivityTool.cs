using System.Windows.Forms;
using Candlechart.Chart;

namespace Candlechart.Core
{
    public abstract class InteractivityTool
    {
        private bool _enabled = true;
     
        internal bool Capture
        {
            get { return (Owner.CaptureTool == this); }
            set
            {
                if (value)
                {
                    Owner.CaptureTool = this;
                    Chart.Capture = true;
                }
                else
                {
                    Owner.CaptureTool = null;
                    Chart.Capture = false;
                }
            }
        }

        protected ChartControl Chart
        {
            get { return Owner.Owner; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string Name { get; set; }

        internal InteractivityToolCollection Owner { get; set; }

        internal virtual bool HandleMouseDown(MouseEventArgs e)
        {
            return false;
        }

        internal virtual bool HandleMouseMove(MouseEventArgs e)
        {
            return false;
        }

        internal virtual bool HandleMouseUp(MouseEventArgs e)
        {
            return false;
        }

        internal virtual bool HandleMouseWheel(MouseEventArgs e)
        {
            return false;
        }
    }
}
