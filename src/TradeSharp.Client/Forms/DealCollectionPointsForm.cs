using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Entity;
using FastMultiChart;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class DealCollectionPointsForm : Form
    {
        private readonly List<MarketOrder> deals;

        private readonly List<DealResult> dealResults = new List<DealResult>();

        public enum DealChartType
        {
            ТриЛинии = 0, СредняяСделка = 1, ВсеСделки = 2
        }

        private const string BtnMoreTitleMore = "Еще...",
                             BtnMoreTitleCollapse = "Свернуть";

        private const int SizeCollapsed = 30, SizeExpanded = 60;

        private int intervalsCount;

        private int intervalLength;

        struct ChartDealPoint
        {
            [DisplayName("Inter")]
            public int X { get; set; }

            [DisplayName("Points")]
            public float Y { get; set; }

            public ChartDealPoint(int t, float y)
                : this()
            {
                X = t;
                Y = y;
            }
        }

        public DealCollectionPointsForm()
        {
            InitializeComponent();

            //cbDealType.Items.Add(DealChartType.ТриЛинии);
            //cbDealType.Items.Add(DealChartType.СредняяСделка);
            //cbDealType.Items.Add(DealChartType.ВсеСделки);
            //cbDealType.SelectedIndex = 0;
            btnMoreOptions.Text = BtnMoreTitleMore;

            SetupChart();
        }

        public DealCollectionPointsForm(List<MarketOrder> deals) : this()
        {
            this.deals = deals;
        }

        private void SetupChart()
        {            
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            // средняя линия
            chart.Graphs[0].Series.Add(new Series("X", "Y", new Pen(Color.FromArgb(80, 5, 5), 2f)));
            // ср. минусовая и ср. плюсовая
            chart.Graphs[0].Series.Add(new Series("X", "Y", new Pen(Color.FromArgb(0, 255, 0), 2f)));
            chart.Graphs[0].Series.Add(new Series("X", "Y", new Pen(Color.FromArgb(255, 0, 0), 2f))); 
        }

        private void BtnApplyClick(object sender, EventArgs e)
        {
            BuildChart();
        }

        private void BuildChart()
        {
            dealResults.Clear();
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
                {
                    CalculateIntervals();
                    // пересчитать сделки
                    foreach (var deal in deals)
                        dealResults.Add(CalcDealResult(deal));                    
                    BuildChartSynch();
                };
            worker.RunWorkerCompleted += (sender, args) =>
                {
                    standByControl.IsShown = false;
                    standByControl.Visible = false;
                };
            standByControl.IsShown = true;
            standByControl.Visible = true;
            worker.RunWorkerAsync();
        }

        private void BuildChartSynch()
        {
            if (dealResults.Count == 0) return;
            if (dealResults.Count(r => r.isProfit.HasValue) == 0) return;
            
            var dealPoints = BuildChartData();
            for (var i = 0; i < chart.Graphs[0].Series.Count; i++)
            {
                chart.Graphs[0].Series[i].Clear();
                foreach (var pt in dealPoints[i])
                {
                    chart.Graphs[0].Series[i].Add(new ChartDealPoint((int)pt.X, pt.Y));                    
                }
            }
            chart.Initialize();
            chart.Invalidate();
        }

        private List<PointF>[] BuildChartData()
        {
            var dealIndex = dealResults.Where(r => r.isProfit.HasValue).ToDictionary(r => r, r => 0);
            var results = new []
                {
                    new List<PointF>(), // средняя
                    new List<PointF>(), // профитная
                    new List<PointF>()  // убыточная
                };

            for (var i = 0; ; i++)
            {
                double sumAvg = 0, sumProfit = 0, sumLoss = 0;
                int countProfit = 0, countLoss = 0;
                foreach (var dealResult in dealResults)
                {
                    var nextIndex = dealIndex[dealResult];
                    if (nextIndex < 0) continue;
                    var points = dealResult.pointsSinceEnter[nextIndex];
                    nextIndex++;
                    if (nextIndex >= dealResult.pointsSinceEnter.Length)
                        nextIndex = -1;
                    dealIndex[dealResult] = nextIndex;

                    // ReSharper disable PossibleInvalidOperationException
                    if (dealResult.isProfit.Value)
                    // ReSharper restore PossibleInvalidOperationException
                    {
                        sumProfit += points;
                        countProfit++;
                    }
                    else
                    {
                        sumLoss += points;
                        countLoss++;
                    }
                    sumAvg += points;
                }

                var countAvg = countProfit + countLoss;
                if (countAvg == 0) break;

                results[0].Add(new PointF(i, (float)(sumAvg / countAvg)));
                results[1].Add(new PointF(i, countProfit == 0 ? 0 : (float)(sumProfit / countProfit)));
                results[2].Add(new PointF(i, countLoss == 0 ? 0 : (float)(sumLoss / countLoss)));
            }
            return results;
        }

        private void DealCollectionPointsFormLoad(object sender, EventArgs e)
        {
            if (DesignMode) return;
            standByControl.IsShown = true;
            standByControl.Visible = true;
            
            // стартовать в бэкграунде расчет позиций
            BuildChart();
        }

        private void CalculateIntervals()
        {
            intervalsCount = ((string)Invoke(new Func<string>(() => tbIntervalsCount.Text))).ToIntSafe() ?? 100;
            intervalLength = (int)Math.Round(((string)Invoke(new Func<string>(() => cbIntervalLength.Text))).ToExpressionResult(60));
            if (intervalLength <= 0) intervalLength = 1;
        }

        private DealResult CalcDealResult(MarketOrder deal)
        {
            var rst = new DealResult
                {
                    dealId = deal.ID
                };
            
            // получить котировки по инструменту
            var quotes = AtomCandleStorage.Instance.GetAllMinuteCandles(deal.Symbol);
            if (quotes.Count == 0) 
                return rst;

            var start = quotes.FindIndex(q => q.timeClose >= deal.TimeEnter);
            if (start < 0) return rst;

            var points = new List<float> { 0 };
            var timeEnter = deal.TimeEnter;
            var pointCost = DalSpot.Instance.GetPointsValue(deal.Symbol, 1f);
            
            for (; start < quotes.Count; start++)
            {
                var deltaTime = (quotes[start].timeClose - timeEnter).TotalMinutes;
                var intervals = (int)deltaTime / intervalLength;
                if (intervals >= points.Count)
                {
                    var abs = deal.Side * (quotes[start].close - deal.PriceEnter);
                    points.Add(abs * pointCost);
                }

                if (points.Count >= intervalsCount) break;
            }

            if (points.Count == 1) return rst;

            rst.pointsSinceEnter = points.ToArray();
            rst.isProfit = rst.pointsSinceEnter[rst.pointsSinceEnter.Length - 1] > 0;
            
            return rst;
        }

        private void BtnMoreOptionsClick(object sender, EventArgs e)
        {
            var isCollapsed = btnMoreOptions.Text == BtnMoreTitleMore;
            panelTop.Size = new Size(panelTop.Size.Width, isCollapsed ? SizeExpanded : SizeCollapsed);
            btnMoreOptions.Text = isCollapsed ? BtnMoreTitleCollapse : BtnMoreTitleMore;
        }
    }

    class DealResult
    {
        public int dealId;

        public bool? isProfit;

        public float[] pointsSinceEnter;
    }
}
