using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MarkupCalculator.CommonClasses;
using TradeSharp.SiteBridge.Lib.Finance;

namespace MarkupCalculator
{
    public partial class FrmMarkupCalculator : Form
    {
        #region Переменные для вычисления статистики
        private StatisticMaker statisticMaker;

        /// <summary>
        /// Поток для основных вычислений
        /// </summary>
        private BackgroundWorker CalculationWorker { get; set; }

        /// <summary>
        /// Путь к катологу с xml файлами сделок
        /// </summary>
        private string PathToDealFolder { get; set; }

        /// <summary>
        /// Имена файлов со сделками
        /// </summary>
        private string[] AllDealFileNames { get; set; }

        /// <summary>
        /// Полное имя каталога с котировками
        /// </summary>
        private string QuoteFolderName { get; set; }

        /// <summary>
        /// Результаты расчётов
        /// </summary>
        private Dictionary<int, AccountPerformanceRaw> StatisticList { get; set; }
        #endregion

        public FrmMarkupCalculator()
        {
            InitializeComponent();
            LoadDealFile();

            CalculationWorker = new BackgroundWorker();
            CalculationWorker.DoWork += CalculationThreadDoWork;
            CalculationWorker.RunWorkerCompleted += CalculationThreadRunWorkerCompleted;
            CalculationWorker.WorkerReportsProgress = true;
            CalculationWorker.WorkerSupportsCancellation = true;
            CalculationWorker.ProgressChanged += (sender, args) =>
            {
                CalculationProgress.Value = args.ProgressPercentage > 100 ? 100 : args.ProgressPercentage;
            };
            
            DepoCurrency.SelectedIndex = 0;
        }

        /// <summary>
        /// Загрузка файлов со сделками и их обработка, а так же имя каталога с котировками
        /// </summary>
        private void LoadDealFile()
        {
            var solutionFolder = Directory.GetParent(Environment.CurrentDirectory).Parent;
            PathToDealFolder = solutionFolder == null ? "" : Path.Combine(solutionFolder.FullName, "accounts");

            AllDealFileNames = Directory.GetFiles(PathToDealFolder);
            QuoteFolderName = Path.Combine(Environment.CurrentDirectory, "quotes");

            namesDealFiles.Items.AddRange(AllDealFileNames.Select(x => x.Split('\\').Last()).ToArray());
        }

        private void BtnCalcClick(object sender, EventArgs e)
        {
            btnCalc.Text = "остановить";
            var dealFileNames = namesDealFiles.CheckedItems.Cast<string>().ToArray();
            
            statisticMaker = new StatisticMaker(dealFileNames.Select(x => Path.Combine(PathToDealFolder, x)), QuoteFolderName)
            {
                DepoCurrency = (string)DepoCurrency.SelectedItem
            };

            CalculationWorker.RunWorkerAsync("MarkupCalculationThread");
        }

        void CalculationThreadDoWork(object sender, DoWorkEventArgs e)
        {
            StatisticList = statisticMaker.GetPerformanceStatistic(CalculationWorker);
        }

        void CalculationThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnCalc.Text = "расчитать";
            EquityGraph.Series.Clear();
            LeverageGraph.Series.Clear();

            foreach (var statistic in StatisticList)
            {
                var xEquityValues = statistic.Value.equity.Select(x => x.time.ToShortDateString()).ToList();
                var yEquityValues = statistic.Value.equity.Select(x => x.equity).ToList();
                var resultEquitySeries = new Series(string.Format("EquityAccount_{0}", statistic.Key))
                    {
                        ChartType = SeriesChartType.Line,
                        SmartLabelStyle = {Enabled = false},
                        IsValueShownAsLabel = false

                    };
                resultEquitySeries.Points.DataBindXY(xEquityValues, yEquityValues);
                EquityGraph.Series.Add(resultEquitySeries);


                var xLeverageValues = statistic.Value.leverage.Select(x => x.time.ToShortDateString()).ToList();
                var yLeverageValues = statistic.Value.leverage.Select(x => x.equity).ToList();
                var resultLeverageSeries = new Series(string.Format("LeverageAccount_{0}", statistic.Key))
                    {
                        ChartType = SeriesChartType.Line,
                        SmartLabelStyle = {Enabled = false},
                        IsValueShownAsLabel = false

                    };
                resultLeverageSeries.Points.DataBindXY(xLeverageValues, yLeverageValues);
                LeverageGraph.Series.Add(resultLeverageSeries);
            }
        }

        #region zooming
        private void BtnEquityZoomAddClick(object sender, EventArgs e)
        {
            GetReducers(EquityGraph.ChartAreas[0], true);
        }

        private void BtnEquityZoomReduceClick(object sender, EventArgs e)
        {
            GetReducers(EquityGraph.ChartAreas[0], false);
        }

        private void BtnLeverageZoomAddClick(object sender, EventArgs e)
        {
            GetReducers(LeverageGraph.ChartAreas[0], true);
        }

        private void BtnLeverageZoomReduceClick(object sender, EventArgs e)
        {
            GetReducers(LeverageGraph.ChartAreas[0], false);
        }

        private static void GetReducers(ChartArea zoomingArea, bool isRreduce)
        {
            var xMin = zoomingArea.AxisX.ScaleView.ViewMinimum;
            var xMax = zoomingArea.AxisX.ScaleView.ViewMaximum;
            var yMin = zoomingArea.AxisY.ScaleView.ViewMinimum;
            var yMax = zoomingArea.AxisY.ScaleView.ViewMaximum;

            var reducerX = isRreduce ? (xMax - xMin)/100 : -1*(xMax - xMin)/100;
            var reducerY = isRreduce ? (yMax - yMin)/100 : -1*(yMax - yMin)/100;

            zoomingArea.AxisX.ScaleView.Zoom(xMin + reducerX, xMax - reducerX);
            zoomingArea.AxisY.ScaleView.Zoom(yMin + reducerY, yMax - reducerY);
        }
        #endregion
    }
}