using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastGrid;
using FastMultiChart;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class PortfolioRiskForm : Form
    {
        public List<PortfolioActive> portfolio;
        public decimal deposit;

        private readonly PortfolioRiskCalcSettings testSettings = new PortfolioRiskCalcSettings();
        
        public double[][] calculatedPercentiles;
        private List<double> tickerSigmas;

        private static readonly Color[] seriesColors =
            new[] { Color.DarkBlue, Color.Red, Color.DarkSeaGreen, Color.Khaki, Color.DarkMagenta, Color.BurlyWood };

        struct ChartDealPoint
        {
            [DisplayName("Minute")]
            public int X { get; set; }

            [DisplayName("Loss")]
            public double Y { get; set; }

            public ChartDealPoint(int t, double y)
                : this()
            {
                X = t;
                Y = y;
            }
        }

        public PortfolioRiskForm()
        {
            InitializeComponent();
            cbPercentiles.RowColors = seriesColors;
        }

        private void BtnCalculateClick(object sender, EventArgs e)
        {
            // посчитать и вывести корреляцию
            var tickers = portfolio.Select(p => p.Ticker).ToList();
            List<string> errors;
            double[][] corMatrix;
            var oldBtnCaption = btnCalculate.Text;
            btnCalculate.Text = "Производится расчет";
            btnCalculate.Enabled = false;
            try
            {
                TickerCorrelationCalculator.CalculateCorrelation(tickers, testSettings.intervalMinutes,
                    testSettings.corrIntervalsCount,
                    testSettings.minCorrIntervalsCount,
                    testSettings.uploadQuotesFromDB,
                    out errors, out corMatrix, out tickerSigmas);
            }
            catch (Exception ex)
            {
                Logger.Error("TickerCorrelationCalculator.CalculateCorrelation()", ex);
                throw;
            }
            
            
            btnCalculate.Text = oldBtnCaption;
            btnCalculate.Enabled = true;
            if (errors != null && errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors),
                    Localizer.GetString("MessageErrorCalculatingCorrelations"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // показать матрицу корреляций
            rtbCorr.Clear();
            var sb = new StringBuilder();
            sb.Append("        ");
            for (var i = 0; i < tickers.Count; i++)
            {
                sb.AppendFormat(" {0} ", tickers[i]);
            }
            for (var i = 0; i < tickers.Count; i++)
            {
                sb.AppendLine();
                sb.AppendFormat("{0}  ", tickers[i]);
                for (var j = 0; j < tickers.Count; j++)
                {
                    var rStr = corMatrix[i][j] < 0 
                        ? string.Format(CultureProvider.Common, " {0:f2}  ", corMatrix[i][j])
                        : string.Format(CultureProvider.Common, "  {0:f2}  ", corMatrix[i][j]);
                    sb.Append(rStr);
                }
            }
            rtbCorr.Text = sb.ToString();

            // перейти на страницу расчета риска
            tabControl.SelectedTab = tabPageCorrelation;
            btnCalcRisk.Focus();
        }

        private void PortfolioRiskFormLoad(object sender, EventArgs e)
        {
            SetupChartsGrids();
        }

        private void SetupChartsGrids()
        {
            // таблица - портфель
            gridPortfolio.ColorAltCellBackground = Color.FromArgb(220, 220, 220);
            gridPortfolio.Columns.Add(new FastColumn("Ticker", "Актив") { ColumnWidth = 90, SortOrder = FastColumnSort.Ascending });
            gridPortfolio.Columns.Add(new FastColumn("Side", "Тип") { formatter = s => ((int)s > 0 ? "BUY" : "SELL"), ColumnWidth = 60 });
            gridPortfolio.Columns.Add(new FastColumn("Leverage", "Плечо"));
            gridPortfolio.CalcSetTableMinWidth();
            gridPortfolio.DataBind(portfolio);

            // график
            chartPercent.GetYValue = (value, chart) => (double)(value/1000.0);
            chartPercent.GetYScaleValue = (value, chart) => (int)((double)value * 1000.0);
            //chartPercent.GetYScaleValue = (value, chart) =>
            //    (int)(value is float ? (float)value * 1000f
            //    : (float)(double)value * 1000f);
            chartPercent.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
        }

        private void BtnCalcRiskClick(object sender, EventArgs e)
        {
            if (tickerSigmas == null || tickerSigmas.Count == 0) return;
            
            // считать таблицу корреляций
            var lines = rtbCorr.Text.Split(
                new[] {Environment.NewLine, "\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < (portfolio.Count + 1)) return;
            var tickers = lines[0].Split(
                new[] { (char)9, ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tickers.Length != portfolio.Count) return;
            var curM = new double[tickers.Length][];
            for (var i = 1; i < lines.Length; i++)
            {
                var values = lines[i].ToDoubleArrayUniform();
                if (values.Length != tickers.Length) return;
                curM[i - 1] = values;
            }
            
            // моделировать
            var dlg = new RiskCalculationPopupForm(portfolio, curM, testSettings, tickerSigmas);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            calculatedPercentiles = dlg.riskPercentiles;
            if (calculatedPercentiles == null || calculatedPercentiles.Length == 0) return;

            // заполнить список перцентилей
            cbPercentiles.Items.Clear();
            foreach (var p in testSettings.percentiles)
            {
                cbPercentiles.Items.Add(p.ToString("f1", CultureProvider.Common));
            }
            for (var i = 0; i < testSettings.percentiles.Count; i++)
                cbPercentiles[i] = true;

            // перейти на страницу графика
            tabControl.SelectedTab = tabPageChart;
            btnDrawChart.Focus();
        }

        private void BtnDrawChartClick(object sender, EventArgs e)
        {
            BuildPercentilesChart();
        }

        private void BuildPercentilesChart()
        {
            if (calculatedPercentiles == null || calculatedPercentiles.Length == 0 ||
                calculatedPercentiles[0].Length == 0) return;

            // строить выбранные графики
            // по одной серии для графика перцентилей
            chartPercent.Graphs[0].Series.Clear();
            for (var i = 0; i < testSettings.percentiles.Count; i++)
            {
                if (!cbPercentiles[i]) continue;
                //var title = string.Format("{0:f1}%", testSettings.percentiles[i]);
                var colorIndex = i < seriesColors.Length ? i : i%seriesColors.Length;
                var color = seriesColors[colorIndex];
                var series = new Series("X", "Y", new Pen(color, 2f));
                chartPercent.Graphs[0].Series.Add(series);

                var interval = 0;
                series.Add(new ChartDealPoint(0, 0));
                for (var j = 0; j < calculatedPercentiles[i].Length; j++)
                {
                    interval += testSettings.intervalMinutes;
                    series.Add(new ChartDealPoint(interval, calculatedPercentiles[i][j]));
                }
            }
            if (chartPercent.Graphs[0].Series.Count == 0) return;
            chartPercent.Initialize();
            chartPercent.Invalidate();
        }

        private void BtnSetupClick(object sender, EventArgs e)
        {
            var dlg = new PortfolioRiskCalcSettingsForm(testSettings);
            dlg.ShowDialog();
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageChart)
                BuildPercentilesChart();
        }
    }    
}
