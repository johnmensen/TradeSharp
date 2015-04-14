using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class PriceProbForm : Form
    {
        private const bool StoreFunction = false;

        private readonly CandleChartControl chart;

        private float[] synthFunc;

        public PriceProbForm()
        {
            InitializeComponent();
            cbSide.SelectedIndex = 0;
        }

        public PriceProbForm(CandleChartControl chart, PointD worldCoords) : this()
        {
            this.chart = chart;
            tbPrice.Text = worldCoords.Y.ToStringUniform(5);

            var candles = chart.chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            dtEnd.Value = candles[candles.Count - 1].timeClose;
        }

        private void BtnCalculateClick(object sender, EventArgs e)
        {
            var candles = chart.chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            var curPrice = candles[candles.Count - 1].close;

            // посчитать дельта цену и дельта время
            var targetPrice = tbPrice.Text.ToDoubleUniform();
            var side = cbSide.SelectedIndex == 0 ? -1 : 1;
            var logPrice = Math.Log(targetPrice/curPrice) * side;
            var deltaTime = (int)Math.Round((dtEnd.Value - DateTime.Now).TotalMinutes);

            // получить эмпирическую функцию распределения
            const int funcIntervalsCount = 100;
            var nSteps = 1d;
            const int longestInterval = 1440;
            if (deltaTime > longestInterval)
            {
                var delta = longestInterval / 2;
                nSteps = (double)deltaTime / delta;
                deltaTime = delta;
            }
            
            var lengths = GetTargetCandlesLengths(deltaTime);
            lengths.Sort();
            var func = MakeDistributionCurve(lengths, funcIntervalsCount);

            // получить параметры 2-х СВ (нормально распределенных),
            // в сумме дающих приближение к эмпирической F
            var avgLen = lengths.Average();
            var sigma = Math.Sqrt(lengths.Sum(l => (l - avgLen)*(l - avgLen)) / lengths.Count);
            double sigm1, sigm2, k;
            CalculateSumDistribution(sigma, func, out sigm1, out sigm2, out k);

            // посчитать вреоятность
            var x = logPrice;
            var scale = nSteps == 1 ? 1 : Math.Sqrt(nSteps);
            var f = k * ProbabilityCore.Erf(x / (ProbabilityCore.R2 * sigm1 * scale))
                                + (1 - k) * ProbabilityCore.Erf(x / (ProbabilityCore.R2 * sigm2 * scale));
            f = f * 0.5 + 0.5;
            var p = 1 - f;
            tbResult.Text = (p*100).ToStringUniform(2) + "%";

            if (!StoreFunction) return;
            // вывести в Excel synthFunc
            var dlg = new SaveFileDialog
                {
                    Title = "Сохранить таблицы",
                    DefaultExt = "csv",
                    Filter = "*.csv|*.csv|*.*|*.*",
                    FilterIndex = 0
                };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            using (var sw = new StreamWriter(dlg.FileName, false, Encoding.ASCII))
            {
                sw.WriteLine("delta(X);F;F synth");
                for (var i = 0; i < func.Count(); i++)
                {
                    sw.WriteLine("{0};{1};{2}",
                        func[i].X.ToStringUniform(5), func[i].Y.ToStringUniform(4), synthFunc[i].ToStringUniform(4));
                }
            }
        }

        private List<float> GetTargetCandlesLengths(int candleLength)
        {
            var candlesM1 = AtomCandleStorage.Instance.GetAllMinuteCandles(chart.Symbol);
            if (candlesM1.Count < candleLength * 50) return null;

            var packer = new CandlePacker(new BarSettings { Intervals = new List<int> { candleLength } });
            var lengths = new List<float>();
            foreach (var cm1 in candlesM1)
            {
                var candle = packer.UpdateCandle(cm1);
                if (candle != null)
                    lengths.Add((float)Math.Log(candle.close / candle.open));
            }

            return lengths;
        }
    
        private static PointF[] MakeDistributionCurve(List<float> data, int numIntervals)
        {
            var total = (float)data.Count;
            var max = data[data.Count - data.Count / numIntervals - 1];
            var min = data[data.Count / numIntervals];

            var result = new PointF[numIntervals];
            var step = (max - min) / numIntervals;

            for (var i = 0; i < numIntervals; i++)
            {
                var count = 0;
                var right = min + (i + 0.5f) * step;
                foreach (var val in data)
                {
                    if (val <= right)
                        count++;
                    else break;
                }
                result[i] = new PointF(right, count / total);
            }

            return result;
        }

        private void CalculateSumDistribution(double sigma, PointF[] dens,
            out double sg1Opt, out double sg2Opt,
            out double kOpt)
        {
            const double mMin = 0.15, mMax = 0.85d, mStep = 0.1;
            const double kMin = 0.2, kMax = 0.8, kStep = 0.05;
            const double kSigmMin = 0.1, kSigmMax = 5.0, kSigmStep = 0.05;

            double? rss = null;

            sg1Opt = 0;
            sg2Opt = 0;
            kOpt = 0;
            // посчитать F, вычисляя разность квадратов
            for (var kSigm = kSigmMin; kSigm <= kSigmMax; kSigm += kSigmStep)
            for (var k = kMin; k <= kMax; k += kStep)
            {
                for (var m = mMin; m < mMax; m += mStep)
                {
                    var sg2 = sigma * kSigm / (1 + m);
                    var sg1 = sg2 * m;
                    var curRss = 0d;

                    float[] testSynthFunc = null;
                    if (StoreFunction) testSynthFunc = new float[dens.Length];

                    for (var i = 0; i < dens.Length; i++)
                    {
                        var x = dens[i].X;
                        var f = (k * ProbabilityCore.Erf(x / (ProbabilityCore.R2 * sg1))
                                + (1 - k) * ProbabilityCore.Erf(x / (ProbabilityCore.R2 * sg2))) * 0.5 + 0.5;
                        var delta = f - dens[i].Y;
                        delta = delta * delta;
                        curRss += delta;
                        if (StoreFunction)
                            testSynthFunc[i] = (float)f;
                    }

                    if (!rss.HasValue || rss > curRss)
                    {
                        rss = curRss;
                        kOpt = k;
                        sg1Opt = sg1;
                        sg2Opt = sg2;
                        synthFunc = testSynthFunc;
                    }
                }
            }
        }
    }
}
