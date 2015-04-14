using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Forms
{
    public partial class RiskCalculationPopupForm : Form
    {
        private readonly List<PortfolioActive> actives;
        private readonly double[][] corrMatrix;
        private readonly PortfolioRiskCalcSettings testSettings;
        private readonly List<double> sigmas;
        public double[][] riskPercentiles;
        private readonly Thread calculationThread;
        private delegate void ReportCompletedDel(DialogResult rst);
        
        public RiskCalculationPopupForm()
        {
            InitializeComponent();
        }

        public RiskCalculationPopupForm(List<PortfolioActive> actives, double[][] corrMatrix,
            PortfolioRiskCalcSettings testSettings, List<double> sigmas)
        {
            InitializeComponent();
            this.actives = actives;
            this.corrMatrix = corrMatrix;
            this.testSettings = testSettings;
            this.sigmas = sigmas;
            calculationThread = new Thread(MakeCalculation);
        }

        private void MakeCalculation()
        {
            riskPercentiles = TickerCorrelationCalculator.CalculateRisks(actives, corrMatrix,
                                                       sigmas, testSettings.intervalMinutes,
                                                       testSettings.simIntervalsCount,
                                                       testSettings.numSimulations, testSettings.percentiles,
                                                       UpdateProgressSafe);
            ReportCompletedSafe(riskPercentiles == null ? DialogResult.No : DialogResult.OK);
        }

        private void RiskCalculationPopupFormLoad(object sender, EventArgs e)
        {
            if (actives == null) return;
            calculationThread.Start();
        }

        private void ReportCompletedSafe(DialogResult rst)
        {
            Invoke(new ReportCompletedDel(ReportCompletedUnsafe), rst);
        }

        private void ReportCompletedUnsafe(DialogResult rst)
        {
            DialogResult = rst;
            Close();
        }

        private void UpdateProgressSafe(string strProgress)
        {
            Invoke(new TickerCorrelationCalculator.ReportProgressDel(UpdateProgressUnsafe), strProgress);
        }

        private void UpdateProgressUnsafe(string strProgress)
        {
            lblStatus.Text = strProgress;
        }

        private void btnInterrupt_Click(object sender, EventArgs e)
        {
            TickerCorrelationCalculator.stoppingCalculation = true;
        }
    }
}
