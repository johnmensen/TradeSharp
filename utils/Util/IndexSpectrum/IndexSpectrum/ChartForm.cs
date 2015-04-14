using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using FXI.Client.MtsBase.Common;
using FXI.Client.MtsBase.Entities;
using IndexSpectrum.Index;
using System.Linq;

namespace IndexSpectrum
{
    public partial class ChartForm : Form
    {
        public ChartForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                tbFolder.Text = dlg.SelectedPath;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            chartIndex.Series[0].Points.Clear();
            chartIndexWoTrend.Series[0].Points.Clear();
            chartSpectrum.Series[0].Points.Clear();
            tbHarmonics.Text = "";
            tbHarmAmpls.Text = "";
            
            var usePeriodAxis = cbSpectrX.SelectedIndex == 1;
            var dataCount = int.Parse(tbDataCount.Text);
            
            // индекс
            var trendType = cbTrendType.SelectedIndex == 1
                                ? IndexFFT.TrendReducion.MovingAverage
                                : IndexFFT.TrendReducion.Linear;
            var data = IndexFFT.GetIndexData(cbIndexFunction.Text,
                                  tbFolder.Text,
                                  int.Parse(tbInterval.Text),
                                  dataCount,
                                  trendType,
                                  int.Parse(tbMAPeriod.Text),
                                  cbUseStartDate.Checked ? (DateTime?)dpStartDate.Value : null,
                                  cbIndexType.SelectedIndex == 1 
                                    ? IndexFFT.CurrencyIndexType.AdditiveMomentumAccumulated
                                    : IndexFFT.CurrencyIndexType.AdditiveWeightedToOne);

            if (data.Length == 0) return;
            var i = 0;
            foreach (var ind in data)
            {
                chartIndex.Series[0].Points.Add(new SeriesPoint((double)(i++), ind));
            }

            // индекс без тренда
            var dataWT = IndexFFT.RemoveTrend(data, trendType, int.Parse(tbMAPeriod.Text));
            i = 0;
            foreach (var ind in dataWT)
            {
                chartIndexWoTrend.Series[0].Points.Add(new SeriesPoint((double)(i++), ind));
            }

            // спектр
            var spectrum = IndexFFT.CalcFFT(dataWT, dataCount);
            int nBands = dataCount / 10;
            for (i = 10; i < nBands; i++)
            {
                var x = usePeriodAxis ? (double)dataCount / i : (double)i;
                chartSpectrum.Series[0].Points.Add(new SeriesPoint(x, spectrum[i]));
            }

            // преобладающие гармоники
            var sb = new StringBuilder();
            var peakPeriod = int.Parse(tbHarmPeak.Text);

            // T - amplitude
            var peaks = new List<Cortege2<double, double>>();            
            for (i = 10; i < nBands; i++)
            {
                var isPeak = true;
                for (var j = i - peakPeriod; j < i + peakPeriod; j++)
                {
                    if (j < 0 || j == i) continue;
                    if (spectrum[i] < spectrum[j])
                    {
                        isPeak = false;
                        break;
                    }
                }
                if (!isPeak) continue;

                var T = dataCount / i;
                var ampl = spectrum[i];
                peaks.Add(new Cortege2<double, double> { a = T, b = ampl });                
            }

            peaks = peaks.OrderByDescending(p => p.b).ToList();
            foreach (var peak in peaks)
            {
                sb.AppendLine(string.Format("Период: {0:f2}. Амплитуда {1:f4}", peak.a, peak.b));
            }
            tbHarmonics.Text = sb.ToString();
            sb = new StringBuilder();

            peaks = peaks.OrderBy(p => p.a).ToList();
            foreach (var peak in peaks)
            {
                sb.AppendLine(string.Format("Период: {0:f2}. Амплитуда {1:f4}", peak.a, peak.b));
            }
            tbHarmAmpls.Text = sb.ToString();            
        }

        private void chartSpectrum_MouseClick(object sender, MouseEventArgs e)
        {
            var diag = (XYDiagram) chartSpectrum.Diagram;
            var coords = diag.PointToDiagram(e.Location);

            if (coords.IsEmpty) return;
            var numArg = Math.Round(coords.NumericalArgument);
            if (numArg == 0) return;
            var period = int.Parse(tbDataCount.Text)/numArg;
            MessageBox.Show(string.Format("Частота: {0}, период: {1:f1} баров", numArg, period));            
        }

        private void btnSynthTest_Click(object sender, EventArgs e)
        {
            var dlg = new SynthCurxForm(tbFolder.Text,
                                        int.Parse(tbInterval.Text), int.Parse(tbDataCount.Text),
                                        dpStartDate.Value);
            dlg.ShowDialog();
            return;
            
            /*var index = "cad0.38769 eur0.33699# jpy0.1322 gbp0.0838# chf0.0302 aud0.0248# nzd0.0042#";
            var ci = new CurrencyIndexInfo(index);

            var startDate = new DateTime(2010, 1, 5);
            var endDate = new DateTime(2010, 10, 14);
            var priceZeroAsk = new decimal[ci.pairs.Length];
            var priceZeroBid = new decimal[ci.pairs.Length];

            var currencyStream = new Dictionary<string, CurrencyStream>();
            // открыть потоки чтения
            foreach (var curName in ci.pairs)
            {
                currencyStream.Add(curName, new CurrencyStream(curName,
                    tbFolder.Text));
            }
            var streamOut = new StreamWriter("d:\\portfolio.quote");

            try
            {
                for (var time = startDate; time <= endDate; time = time.AddMinutes(1))
                {
                    var sumAsk = 0M;
                    var sumBid = 0M;
                    for (var i = 0; i < ci.pairs.Length; i++)                        
                    {
                        var curName = ci.pairs[i];
                        var quote = currencyStream[curName].GetQuote(time);
                        if (!quote.HasValue) continue;
                        if (priceZeroAsk[i] == 0) priceZeroAsk[i] = quote.Value.ask;
                        if (priceZeroBid[i] == 0) priceZeroBid[i] = quote.Value.bid;

                        decimal deltaAsk;
                        if (ci.weights[i] < 0)
                            deltaAsk = (-ci.weights[i] / priceZeroAsk[i]) * (priceZeroAsk[i] - quote.Value.ask);
                        else
                            deltaAsk = ci.weights[i] * (quote.Value.ask - priceZeroAsk[i]);
                        sumAsk += deltaAsk;

                        decimal deltaBid;
                        if (ci.weights[i] < 0)
                            deltaBid = (-ci.weights[i] / priceZeroBid[i]) * (priceZeroBid[i] - quote.Value.bid);
                        else
                            deltaBid = ci.weights[i] * (quote.Value.bid - priceZeroBid[i]);
                        sumBid += deltaBid;
                    }
                    var line = string.Format("{0};{1};{2}",
                                             sumBid, sumAsk,
                                             time.ToString("dd.MM.yyyy HH:mm:ss"));
                    streamOut.WriteLine(line);
                }
            }
            finally
            {
                streamOut.Close();
                // закрыть потоки
                foreach (var curName in ci.pairs)
                {
                    currencyStream[curName].CloseStream();
                }
            }*/
        }

        private void btnTimeVolume_Click(object sender, EventArgs e)
        {
            var dlg = new TimeVolumeForm();
            dlg.ShowDialog();
        }

        private void btnHurst_Click(object sender, EventArgs e)
        {
            var dlg = new RSForm();
            dlg.ShowDialog();
        }
    }
}
