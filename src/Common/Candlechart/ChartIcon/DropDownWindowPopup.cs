using System.Windows.Forms;
using Candlechart.Chart;

namespace Candlechart.ChartIcon
{
    public class DropDownWindowPopup : ToolStripDropDown
    {
        public DropDownWindowPopup(ChartControl ownerChart)
        {
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            AutoSize = false;

            var window = new SummaryPositionDropWindow(ownerChart.Owner)
            {
                Parent = ownerChart
            };

            Width = window.Width;
            Height = window.Height;
            window.closeControl += Close;
            Closing += (sender, args) =>
            {
                if (window.isPinup) args.Cancel = true;
            };

            var host = new ToolStripControlHost(window)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };
            Items.Add(host);
        }
    }
}
