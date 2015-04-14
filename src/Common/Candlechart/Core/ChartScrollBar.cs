using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Candlechart.Chart;

namespace Candlechart.Core
{
    [ToolboxBitmap(typeof(ChartScrollBar), "ChartScrollBar.ChartScrollBar.bmp"),
     Description("Provides a scroll bar for ChartControl"),
     DefaultProperty("Chart")]
    public class ChartScrollBar : HScrollBar
    {// DevExpress.XtraEditors.HScrollBar        
        private ChartControl _chart;

        [Description("Specifies the chart that this scrollbar is attached to."),
         ImmutableObject(true),
         DefaultValue((string)null)]
        public ChartControl Chart
        {
            get { return _chart; }
            set
            {
                if (Chart != null)
                {
                    Chart.ViewChanged -= OnChartViewChanged;
                }
                _chart = value;
                if (value != null)
                {
                    if (Chart != null) Chart.ViewChanged += OnChartViewChanged;
                    SetScrollInfo();
                }
            }
        }

        // Methods
        protected void OnChartViewChanged(object sender, EventArgs e)
        {
            if (Chart != null)
            {
                SetScrollInfo();
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            if (Chart == null) return;
            //Chart.ScrollBy(se.NewValue - se.OldValue);
            if (se.NewValue == Chart.LeftPos) return;
            Chart.ScrollTo(se.NewValue);
            Chart.Invalidate();
        }

        private void SetScrollInfo()
        {
            if (Chart.RightPos < Chart.LeftPos) return;
            
            SmallChange = 1;
            LargeChange = Chart.RightPos - Chart.LeftPos;// + 1;
            Minimum = ChartControl.LeftMargin;
            Maximum = Chart.RightMargin;// -LargeChange;
            if (Maximum < Minimum) Maximum = Minimum;
            Value = Chart.LeftPos >= Minimum && Chart.LeftPos <= Maximum
                ? Chart.LeftPos : Minimum;
            
            Invalidate();
        }
    }
}
