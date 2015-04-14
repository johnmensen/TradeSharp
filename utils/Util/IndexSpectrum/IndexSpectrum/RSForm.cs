using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using FXI.Client.MtsBase.Common;
using FXI.Client.ServiceModel.Entities;

namespace IndexSpectrum
{
    public partial class RSForm : Form
    {
        public RSForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                tbFileName.Text = openFileDialog.FileName;
        }

        private void btnCalcHurst_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbFileName.Text)) return;

            var lstQuote = new List<double>();
            DateTime? lastTime = null;
            int minDeltaMinutes = int.Parse(tbTimeframe.Text);

            using (var sr = new StreamReader(tbFileName.Text))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    //1.4128;1.4128;02.01.2009 11:05:42
                    var parts = line.Split(';');
                    if (parts.Length != 3) continue;
                    //var ask = parts[0].ToDecimalUniform();
                    var bid = parts[1].ToDecimalUniform();
                    var time = DateTime.ParseExact(parts[2], "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture);
                    if (lastTime.HasValue)
                    {
                        var deltaTime = (time - lastTime.Value).TotalMinutes;
                        if (deltaTime < minDeltaMinutes) continue;
                    }
                    lastTime = time;
                    
                    lstQuote.Add((double)bid);
                }
            }

            if (lstQuote.Count < 10) return;
            CalcHurst(lstQuote);
        }

        private void CalcHurst(List<double> lstQuote)
        {
            chartHurst.Series[0].Points.Clear();
            chartHurst.Series[1].Points.Clear();

            var arrH = new double[lstQuote.Count - 1];
            for (var i = 1; i < lstQuote.Count; i++)
            {
                arrH[i - 1] = Math.Log((lstQuote[i]/lstQuote[i - 1]));
            }

            var points = new List<Cortege2<double, double>>();
            for (var i = 2; i <= arrH.Length; i++)
            {
                var rs = CalcRS(arrH, i);
                points.Add(new Cortege2<double, double>(Math.Log(i), Math.Log(rs)));
            }
            // построить график
            double sumX = 0, sumX2 = 0;
            double sumY = 0, sumXY = 0;
            foreach (var pt in points)
            {
                chartHurst.Series[0].Points.Add(new SeriesPoint(pt.a, pt.b));
                sumX += pt.a;
                sumX2 += (pt.a*pt.a);
                sumXY += (pt.a*pt.b);
                sumY += pt.b;
            }
            var delim = (arrH.Length * sumX2 - sumX * sumX);
            var Hurst = (arrH.Length * sumXY - sumX * sumY) / delim;
            var b = (sumY*sumX2 - sumXY*sumX) / delim;
            // построить регрессию
            var y1 = points[0].a*Hurst + b;
            var y2 = points[points.Count - 1].a*Hurst + b;
            chartHurst.Series[1].Points.Add(new SeriesPoint(points[0].a, y1));
            chartHurst.Series[1].Points.Add(new SeriesPoint(points[points.Count - 1].a, y2));
            
            // построить 
            MessageBox.Show(string.Format("Хёрст: {0}", Hurst));
        }

        private static double CalcRS(double[] arrH, int n)
        {
            var mid = arrH.Average();
            double min = double.MaxValue, max = double.MinValue;
            double sum = 0;
            for (var i = 0; i < n; i++)
            {
                var delta = arrH[i] - mid;
                sum += delta * delta;
                if (delta < min) min = delta;
                if (delta > max) max = delta;
            }
            var sko = sum / n;
            var r = max - min;
            return sko == 0 ? 0 : r / sko;
        }
    }
}
