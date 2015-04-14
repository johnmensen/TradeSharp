using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Entity;
using FastChart.Chart;
using TradeSharp.Contract.Entity;

namespace Candlechart.Controls
{
    public partial class CandleMenuWindow : Form
    {
        private readonly CandleChartControl ownerChart;
        private readonly int candleIndex;

        public CandleMenuWindow()
        {
            InitializeComponent();
        }

        public CandleMenuWindow(CandleChartControl ownerChart, int candleIndex)
        {
            InitializeComponent();

            this.ownerChart = ownerChart;
            this.candleIndex = candleIndex;
            SetupChart();
            ShowCandleData();
        }

        private void ShowCandleData()
        {
            var candle = ownerChart.chart.StockSeries.Data.Candles[candleIndex];
            Text = string.Format("Свеча: индекс {0}", candleIndex);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("с {0:dd/MM/yyyy HH:mm} по {1:dd/MM/yyyy HH:mm}",
                                        candle.timeOpen, candle.timeClose));
            var precision = string.IsNullOrEmpty(ownerChart.Symbol)
                                ? 4 : DalSpot.Instance.GetPrecision(ownerChart.Symbol);
            string strFmt = string.Format("f{0}", precision);
            string strO = candle.open.ToString(strFmt), strH = candle.high.ToString(strFmt);
            string strL = candle.low.ToString(strFmt), strC = candle.close.ToString(strFmt);

            sb.AppendLine(string.Format("O: {0}, H: {1}, L: {2}, C: {3}", strO, strH, strL, strC));
            tbInfo.Text = sb.ToString();

            // отобразить кривую курса в теле свечки
            BuildCandleCurve();
        }

        private void BuildCandleCurve()
        {
            chart.series[0].points.Clear();
            chart.series[1].points.Clear();

            var candle = ownerChart.chart.StockSeries.Data.Candles[candleIndex];
            DateTime start = candle.timeOpen, end = candle.timeClose;

            // получить минутные свечки из атомарного (m1) хранилища
            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ownerChart.Symbol, start, end);
            
            int indexLow = -1, indexHigh = -1;
            float min = float.MaxValue, max = float.MinValue;
            
            for (var i = 0; i < candles.Count; i++)
            {
                if (candles[i].high > max)
                {
                    max = candles[i].high;
                    indexHigh = i;
                }
                if (candles[i].low < min)
                {
                    min = candles[i].low;
                    indexLow = i;
                }                
            }
            // проредить массив при необходимости
            
            // построить график
            foreach (var quote in candles)
            {
                chart.series[0].AddPoint(quote.timeOpen, quote.close);
                if (cbShowHL.Checked)
                {
                    chart.series[1].AddPoint(quote.timeOpen, quote.high);
                    chart.series[2].AddPoint(quote.timeOpen, quote.low);
                }
            }
            // отобразить H - L
        }

        private void SetupChart()
        {
            // настройки графика
            chart.UseGradient = true;
            chart.GradAngle = 80;
            chart.ClGradStart = Color.FromArgb(255, 190, 190, 190);
            chart.ClGradEnd = Color.FromArgb(255, 210, 210, 210);            
            
            var axisX = new FastAxis(FastAxisDirection.X, true)
                            {
                                DrawMainGrid = true,
                                DrawSubGrid = true,
                                ColorMainGrid = Color.FromArgb(255, 130, 130, 130),
                                ColorSubGrid = Color.FromArgb(255, 170, 170, 170)
                            };
            chart.Axes.Add(axisX);
            var axisY = new FastAxis(FastAxisDirection.Y, false)
                            {
                                DrawMainGrid = true,
                                ColorMainGrid = Color.FromArgb(255, 130, 130, 130)
                            };
            chart.Axes.Add(axisY);
            chart.series.Add(new FastSeries("цена (close)", FastSeriesType.Линия, axisX, axisY, false) 
                { PenLine = new Pen(Color.Blue) });
            chart.series.Add(new FastSeries("цена (H)", FastSeriesType.Линия, axisX, axisY, false) 
                { PenLine = new Pen(Color.Green) });
            chart.series.Add(new FastSeries("цена (L)", FastSeriesType.Линия, axisX, axisY, false) 
                { PenLine = new Pen(Color.Red) });
            chart.ShowLegend = false;
        }

        private void BtnRefreshChartClick(object sender, EventArgs e)
        {
            BuildCandleCurve();
            chart.Invalidate();
        }
    }
}
