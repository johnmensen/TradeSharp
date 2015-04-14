using System;
using System.Windows.Forms;

namespace Candlechart.Core
{
    public class ScrollTool : InteractivityTool
    {
        // Fields
        private int _delta = 1;

        // Methods

        // Properties
        public int Delta
        {
            get { return _delta; }
            set { _delta = value; }
        }

        internal override bool HandleMouseWheel(MouseEventArgs e)
        {
            Chart.ScrollBy(Math.Sign(e.Delta) * Delta);
            Chart.Invalidate();
            return true;
        }
    }    
}