using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Candlechart;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class PriceDistributionForm : Form
    {
        private readonly string ticker;
        
        private readonly List<CandleData> candles;

        private PointF[] dens;
        
        private PointF[] densMath;

        private PointF[] densSum;

        public PriceDistributionForm()
        {
            InitializeComponent();
        }

        public PriceDistributionForm(string ticker) : this()
        {
            this.ticker = ticker;
            candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker);
        }

        private void CalculateQuoteDistribution(int intervals, int minutesDelta)
        {
            if (candles == null || candles.Count < 1000) return;
            
            // перевести свечи в массив длин OC
            float[] lengths;
            if (minutesDelta == 1)
                lengths = candles.Select(c => Math.Abs(c.close - c.open)).OrderBy(d => d).ToArray();
            else
            {
                var listLen = new List<float>();
                CandleData prev = null;
                foreach (var candle in candles)
                {
                    if (prev != null)
                    {
                        if ((candle.timeOpen - prev.timeOpen).TotalMinutes < minutesDelta) continue;
                        var delta = Math.Abs(candle.close - prev.close);
                        listLen.Add(delta);
                        prev = candle;
                    }
                    else
                        prev = candle;
                }
                lengths = listLen.OrderBy(d => d).ToArray();
            }

            // посчитать интегральную функцию распределения вероятности длина тела свечи / N свечей
            dens = MakeDistributionCurve(lengths, intervals);

            // получить оценку согласно "нормальному" закону
            densMath = new PointF[dens.Length];
            var sigma = Math.Sqrt(lengths.Sum(l => l*l)/lengths.Length);

            for (var i = 0; i < dens.Length; i++)
            {
                var x = dens[i].X;
                var y = 1 * ProbabilityCore.Erf(x / (ProbabilityCore .R2 * sigma)); // P за вычетом 0.5
                densMath[i] = new PointF(x, (float) y);
            }

            // получить параметры 2-х норм. распределений, дающих в сумме исходное
            double sg1, sg2, k;
            CalculateSumDistribution(sigma, dens, out sg1, out sg2, out k);

            DrawChart();

            // сохранить данные в Excel
            var dlg = new SaveFileDialog
                {
                    Title = "Сохранить файл",
                    DefaultExt = "csv",
                    Filter = "All Files (*.*)|*.*|CSV (*.csv)|*.csv"
                };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using (var fs = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
            {
                fs.WriteLine("Sigma;{0}", sigma.ToStringUniform());

                for (var i = 0; i < dens.Length; i++)
                {
                    fs.WriteLine("{0},{1},{2},{3}", 
                        i, dens[i].X.ToStringUniform(5), dens[i].Y.ToStringUniform(5), densMath[i].Y.ToStringUniform(5));
                }
            }
        }

        private void CalculateSumDistribution(double sigma, PointF[] dens, 
            out double sg1Opt, out double sg2Opt,
            out double kOpt)
        {
            const double mMin = 0.25, mMax = 0.75d, mStep = 0.1;
            const double kMin = 0.5, kMax = 0.8, kStep = 0.05;
            const double kSigmMin = 0.8, kSigmMax = 2.0, kSigmStep = 0.05;

            double? rss = null;
            
            sg1Opt = 0;
            sg2Opt = 0;
            kOpt = 0;

            var timeStart = DateTime.Now;
            // посчитать F, вычисляя разность квадратов
            for (var kSigm = kSigmMin; kSigm <= kSigmMax; kSigm += kSigmStep)
            for (var k = kMin; k <= kMax; k += kStep)
            {
                for (var m = mMin; m < mMax; m += mStep)
                {
                    var sg2 = sigma * kSigm / (1 + m);
                    var sg1 = sg2 * m;
                    var curRss = 0d;

                    var densResulted = new PointF[dens.Length];

                    for (var i = 0; i < dens.Length; i++)
                    {
                        var x = dens[i].X;
                        var f = k*ProbabilityCore.Erf(x/(ProbabilityCore.R2*sg1))
                                + (1 - k)*ProbabilityCore.Erf(x/(ProbabilityCore.R2*sg2));
                        var delta = f - dens[i].Y;
                        delta = delta * delta;
                        curRss += delta;

                        densResulted[i] = new PointF(x, (float) f);
                    }

                    if (!rss.HasValue || rss > curRss)
                    {
                        rss = curRss;
                        kOpt = k;
                        sg1Opt = sg1;
                        sg2Opt = sg2;
                        densSum = densResulted;
                    }
                }
            }
            //var deltaMils = (DateTime.Now - timeStart).TotalMilliseconds;
        }

        private static PointF[] MakeDistributionCurve(float[] data, int numIntervals)
        {
            var total = (float)data.Length;
            var max = data[data.Length - data.Length/100 - 1];
            var result = new PointF[numIntervals];
            var step = max / numIntervals;

            for (var i = 0; i < numIntervals; i++)
            {
                var count = 0;
                var right = (i + 1) * step;
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

        private void BtnPlotClick(object sender, EventArgs e)
        {
            CalculateQuoteDistribution(tbIntervals.Text.Replace(" ", "").ToIntSafe() ?? 100,
                                       tbMinutes.Text.Replace(" ", "").ToIntSafe() ?? 100);
        }

        private void PictureBoxResize(object sender, EventArgs e)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            if (dens == null || dens.Length == 0) return;

            const int padLeft = 30;
            const int padTop = 10;
            const int padRight = 10;
            const int padBottom = 20;

            var wdFull = pictureBox.Width;
            var htFull = pictureBox.Height;
            var wd = wdFull - padLeft - padRight;
            var ht = htFull - padTop - padBottom;
            var cy = ht / 1f;
            var cx = wd / (float) dens.Length;

            // сбацать картинку и разлиновать
            var bmp = new Bitmap(wdFull, htFull);
            using (var gr = Graphics.FromImage(bmp))
            using (var brushVoid = new SolidBrush(Color.White))
            using (var penA = new Pen(Color.Navy))
            using (var penB = new Pen(Color.ForestGreen))
            using (var penC = new Pen(Color.Red))
            {
                gr.FillRectangle(brushVoid, 0, 0, wdFull, htFull);
                var defPen = SystemPens.ControlText;
                gr.DrawRectangle(defPen, padLeft, padTop, wd, ht);

                // построить сам график
                var series = new List<PointF[]> {dens, densMath, densSum};
                var pens = new [] {penA, penB, penC};
                
                for (var i = 0; i < series.Count; i++)
                {
                    var pen = pens[i];
                    var data = series[i];

                    PointF? pointPrev = null;
                    for (var j = 0; j < data.Length; j++)
                    {
                        var x = padLeft + j * cx;
                        var y = htFull - padBottom - data[j].Y * cy;
                        if (pointPrev.HasValue)
                            gr.DrawLine(pen, x, y, pointPrev.Value.X, pointPrev.Value.Y);
                        pointPrev = new PointF(x, y);
                    }
                }
            }

            // прилепить картинку
            Image oldImage = pictureBox.Image;
            pictureBox.Image = bmp;
            if (oldImage != null)
                oldImage.Dispose();
        }
    }
}
