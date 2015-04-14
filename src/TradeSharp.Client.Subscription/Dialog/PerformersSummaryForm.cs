using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastGrid;
using FastMultiChart;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using Entity;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class PerformersSummaryForm : Form
    {
        class StatGridItem
        {
            public static readonly StatGridItem speciman;

            public string PtrName { get; set; }

            public string PtrDescription { get; set; }

            public string PtrValue { get; set; }

            public StatGridItem()
            {
            }

            public StatGridItem(string ptrName, string ptrVal, string ptrDesc)
            {
                PtrName = ptrName;
                PtrValue = ptrVal;
                PtrDescription = ptrDesc;
            }
        }

        private const string DefaultDepoCurrency = "USD";
        private readonly BackgroundWorker workerStatLoader = new BackgroundWorker();
        private const string FormTitle = "Суммарный результат стратегий";
        private readonly List<AccountEfficiency> efcList = new List<AccountEfficiency>();

        public PerformersSummaryForm(List<PerformerStatEx> stats)
            : this()
        {
            workerStatLoader.RunWorkerAsync(stats);
        }

        public PerformersSummaryForm()
        {
            InitializeComponent();
            Text = FormTitle;
            SetupGrid();
            SetupChart(chartProfit1000);

            workerStatLoader.DoWork += WorkerStatLoaderOnDoWork;
            workerStatLoader.WorkerSupportsCancellation = true;
            workerStatLoader.RunWorkerCompleted += WorkerStatLoaderOnRunWorkerCompleted;
        }

        private void WorkerStatLoaderOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (efcList.Count == 0) return;

            // доп. вычисления
            efcList.ForEach(e => e.openedDeals.ForEach(d =>
                {
                    d.ResultPoints = DalSpot.Instance.GetPointsValue(d.Symbol,
                        d.Side * ((d.PriceExit ?? d.PriceEnter) - d.PriceEnter));                    
                }));
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            efcList.ForEach(e => DalSpot.Instance.CalculateOpenedPositionsCurrentResult(e.openedDeals, quotes, DefaultDepoCurrency));
            
            // показать результат
            DisplayResultsInGrid();
            BuildChart();
        }

        private void WorkerStatLoaderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var stats = (List<PerformerStatEx>) doWorkEventArgs.Argument;
            for (var i = 0; i < stats.Count; i++)
            {
                UpdateTitleSafe(i, stats.Count);

                // закачать статистику с данными графиков
                if (workerStatLoader.CancellationPending)
                    return;
                var efc = TradeSharpAccountStatistics.Instance.proxy.GetAccountEfficiencyShort(stats[i].Account, true, true);
                efcList.Add(efc);
            }
            UpdateTitleSafe(stats.Count, stats.Count);
        }

        private void UpdateTitleSafe(int current, int total)
        {
            if (InvokeRequired)
                Invoke(new Action<int, int>(UpdateTitleUnSafe), current, total);
            else
                UpdateTitleUnSafe(current, total);
        }

        private void UpdateTitleUnSafe(int current, int total)
        {
            Text = string.Format("{0}: {1} из {2}", FormTitle, current, total);
        }

        private void PerformersSummaryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (workerStatLoader.IsBusy)
                workerStatLoader.CancelAsync();
        }

        private void SetupGrid()
        {
            gridStat.Columns.Add(new FastColumn(StatGridItem.speciman.Property(s => s.PtrName),
                "Параметр"));
            gridStat.Columns.Add(new FastColumn(StatGridItem.speciman.Property(s => s.PtrValue),
                "Значение"));
            gridStat.colorFormatter = (object value, out Color? color, out Color? fontColor) =>
            {
                fontColor = null;
                color = null;
                var item = (StatGridItem) value;
                if (item.PtrValue.StartsWith("-"))
                    fontColor = Color.Maroon;
            };
            gridStat.CalcSetTableMinWidth(80);
        }

        private void DisplayResultsInGrid()
        {
            var totalDeals = efcList.Sum(l => l.closedDeals.Count);
            var openedDeals = efcList.Sum(l => l.openedDeals.Count);
            
            var grossProfitOpenedDepo = efcList.Sum(l => l.openedDeals.Where(d => d.ResultDepo > 0).Sum(d => d.ResultDepo));
            var grossLossOpenedDepo = efcList.Sum(l => l.openedDeals.Where(d => d.ResultDepo < 0).Sum(d => -d.ResultDepo));

            var grossProfitClosedDepo = efcList.Sum(l => l.closedDeals.Where(d => d.ResultDepo > 0).Sum(d => d.ResultDepo));
            var grossLossClosedDepo = efcList.Sum(l => l.closedDeals.Where(d => d.ResultDepo < 0).Sum(d => -d.ResultDepo));

            var sumPoints = efcList.Sum(l => l.openedDeals.Sum(d => d.ResultPoints) +
                l.closedDeals.Sum(d => d.ResultPoints));

            var ptrs = new List<StatGridItem>
            {
                new StatGridItem("Сделок всего", totalDeals.ToStringUniformMoneyFormat(), ""),
                new StatGridItem("Открытых сделок", openedDeals.ToStringUniformMoneyFormat(), ""),
                
                new StatGridItem("Общий р-т, $", (grossProfitOpenedDepo + grossProfitClosedDepo - 
                    grossLossOpenedDepo - grossLossClosedDepo).ToStringUniformMoneyFormat(), ""),

                new StatGridItem("Открытый профит, $", grossProfitOpenedDepo.ToStringUniformMoneyFormat(), ""),
                new StatGridItem("Открытый loss, $", grossLossOpenedDepo.ToStringUniformMoneyFormat(), ""),
                new StatGridItem("Открытый р-т, $", (grossProfitOpenedDepo - 
                    grossLossOpenedDepo).ToStringUniformMoneyFormat(), ""),

                new StatGridItem("Закрытый профит, $", grossProfitClosedDepo.ToStringUniformMoneyFormat(), ""),
                new StatGridItem("Закрытый loss, $", grossLossClosedDepo.ToStringUniformMoneyFormat(), ""),
                new StatGridItem("Закрытый р-т, $", (grossProfitClosedDepo - 
                    grossLossClosedDepo).ToStringUniformMoneyFormat(), ""),

                new StatGridItem("Пунктов всего", sumPoints.ToStringUniformMoneyFormat(), "")
            };
            ptrs.AddRange(CalculateProfitStratsCountByMonth());

            gridStat.DataBind(ptrs);
            gridStat.Invalidate();
        }

        private void BuildChart()
        {
            if (efcList.Count == 0)
                return;
            var profitData = CalculateProfitCurve();

            chartProfit1000.Graphs[0].Series[0].Clear();
            //chartProfit1000.Graphs[0].Series[1].Clear();
            foreach (var pt in profitData)
                chartProfit1000.Graphs[0].Series[0].Add(pt);
            chartProfit1000.Initialize();
            chartProfit1000.Invalidate();
        }

        private List<TimeBalans> CalculateProfitCurve()
        {
            var data = new List<TimeBalans>();
            var start = efcList.Where(e => e.listProfit1000.Count > 0).Min(l => l.listProfit1000[0].time);
            var curIndicies = efcList.Select(l => 0).ToArray();

            var nowDate = DateTime.Now;
            for (var day = start.Date; day < nowDate; day = day.AddDays(1))
            {
                var sumEquity = (float) 0;
                var resultsCount = 0;

                for (var i = 0; i < efcList.Count; i++)
                {
                    var index = curIndicies[i];
                    var efc = efcList[i];

                    if (index >= efc.listProfit1000.Count) continue;
                    if (efc.listProfit1000[i].time.Date > day) continue;

                    for (; index < efc.listProfit1000.Count; index++)
                    {
                        if (efc.listProfit1000[index].time.Date < day)
                            continue;
                        if (efc.listProfit1000[index].time.Date > day) break;

                        resultsCount ++;
                        sumEquity += efc.listProfit1000[index].equity;
                    }
                }

                if (resultsCount == 0) continue;
                data.Add(new TimeBalans(day, sumEquity / resultsCount));
            }

            return data;
        }

        private static void SetupChart(FastMultiChart.FastMultiChart chart)
        {
            chart.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chart.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chart.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chart.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chart.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chart.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            var blank = new TimeBalans();
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Time), blank.Property(p => p.Balans), new Pen(Color.Red, 2f))
            {
                XMemberTitle = Localizer.GetString("TitleDate"),
                YMemberTitle = Localizer.GetString("TitleBalance")
            });
            //chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Time), blank.Property(p => p.Balans), new Pen(Color.Green, 2f))
            //{
            //    XMemberTitle = Localizer.GetString("TitleDate"),
            //    YMemberTitle = "HWM"
            //});
        }

        private List<StatGridItem> CalculateProfitStratsCountByMonth()
        {
            // мин. количество счетов, с которых начинаем отсчет процента профитных стратеги за месяц
            var countInStackMin = efcList.Count/2;
            if (countInStackMin < 1) countInStackMin = 1;

            var start = efcList.Where(e => e.listProfit1000.Count > 0).Min(l => l.listProfit1000[0].time);

            var nowDate = DateTime.Now;
            var profitStratPercentList = new List<decimal>();

            for (var day = new DateTime(start.Year, start.Month, 1).AddMonths(1); day < nowDate; day = day.AddMonths(1))
            {
                var startDay = day.AddMonths(-1);
                var endDay = day;

                int countProfit = 0, count = 0;
                foreach (var efc in efcList)
                {
                    var startMark = efc.listProfit1000.FirstOrDefault(p => p.time <= startDay);
                    if (startMark.time == default(DateTime)) continue;

                    var endMark = efc.listProfit1000.FirstOrDefault(p => p.time >= endDay);
                    if (endMark.time == default(DateTime)) continue;

                    count++;
                    if (endMark.equity > startMark.equity) countProfit++;
                }

                if (count < countInStackMin) continue;
                profitStratPercentList.Add(100M * countProfit / count);
            }

            if (profitStratPercentList.Count == 0)
                return new List<StatGridItem>();

            return new List<StatGridItem>
            {
                new StatGridItem("Сред. мес. проф", profitStratPercentList.Average().ToString("f2") + "%", ""),
                new StatGridItem("Мин. мес. проф", profitStratPercentList.Min().ToString("f2") + "%", ""),
                new StatGridItem("Макс. мес. проф", profitStratPercentList.Max().ToString("f2") + "%", "")
            };
        }
    }
}
