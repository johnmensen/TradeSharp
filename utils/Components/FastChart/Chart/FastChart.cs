using System.Collections.Generic;
using System.Windows.Forms;

namespace FastChart.Chart
{
    public class FastChart : Control
    {
        #region Members
        public readonly List<FastAxis> axes = new List<FastAxis>();
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode && firstDesignTimeUse)
            {
                ConstructSample();
                firstDesignTimeUse = false;
            }
        }

        #region design time
        private bool firstDesignTimeUse = true;
        private void ConstructSample()
        {
            var ax = new FastAxis(FastAxisDirection.X, true) { AutoScale100 = true };
            axes.Add(ax);
            ax = new FastAxis(FastAxisDirection.Y, false) { AutoScale100 = true };
            axes.Add(ax);
        }
        #endregion
    }
}
