using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class PortfolioRiskCalcSettingsForm : Form
    {
        public PortfolioRiskCalcSettings settings;

        public PortfolioRiskCalcSettingsForm(PortfolioRiskCalcSettings sets)
        {
            InitializeComponent();
            settings = sets;
        }
        
        public PortfolioRiskCalcSettingsForm()
        {
            InitializeComponent();
        }

        private void PortfolioRiskCalcSettingsFormLoad(object sender, EventArgs e)
        {
            if (settings == null) return;
            tbTimeframe.Text = settings.intervalMinutes.ToString();
            tbTestsCount.Text = settings.numSimulations.ToString();
            tbTestTimeframeCount.Text = settings.simIntervalsCount.ToString();
            tbCorrTimeframeCount.Text = settings.corrIntervalsCount.ToString();
            cbUploadQuotes.Checked = settings.uploadQuotesFromDB;
            tbPercentiles.Text = string.Join(" ", settings.percentiles.Select(p => p.ToString(
                "f1", CultureProvider.Common)));
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            if (settings == null) settings = new PortfolioRiskCalcSettings();
            settings.intervalMinutes = tbTimeframe.Text.ToInt();
            settings.numSimulations = tbTestsCount.Text.ToInt();
            settings.simIntervalsCount = tbTestTimeframeCount.Text.ToInt();
            settings.corrIntervalsCount = tbCorrTimeframeCount.Text.ToInt();
            settings.percentiles = tbPercentiles.Text.ToDoubleArrayUniform().ToList();
            settings.uploadQuotesFromDB = cbUploadQuotes.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class PortfolioRiskCalcSettings
    {
        public int intervalMinutes = 240;
        public int corrIntervalsCount = 60 * 6;
        public int simIntervalsCount = 50;
        public int minCorrIntervalsCount = 30 * 6;
        public int numSimulations = 20000;
        public bool uploadQuotesFromDB;
        public List<double> percentiles = new List<double> { 60, 90, 95 };
    }
}
