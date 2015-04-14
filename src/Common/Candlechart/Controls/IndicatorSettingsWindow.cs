using System;
using System.Windows.Forms;
using Candlechart.Indicator;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class IndicatorSettingsWindow : Form
    {
        public IndicatorSettingsWindow()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public IChartIndicator Indi { get; set;}

        private void InitGrid()
        {
            grid.SelectedObject = Indi;
        }

        private void IndicatorSettingsWindowLoad(object sender, EventArgs e)
        {
            InitGrid();
        }
    }    
}
