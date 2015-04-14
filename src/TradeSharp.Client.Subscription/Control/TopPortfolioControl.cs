using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using FastMultiChart;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class TopPortfolioControl : UserControl
    {
        public delegate void StrategyChangedDel(object sender, string strategy);

        public EventHandler PortfolioChanged;
        public EventHandler OpenedDealsLoaded;
        public EventHandler ClosedDealsLoaded;
        public EventHandler SubscriptionChanged;
        public StrategyChangedDel StrategyChanged;

        // все данные по управляющему счету (portfolio.ManagedAccount)
        // используется для пользовательского портфеля, когда данные генерируются на клиенте
        private AccountEfficiency efficiency;
        public AccountEfficiency Efficiency
        {
            get { return efficiency; }
            set { efficiency = value; }
        }

        private TopPortfolio portfolio;
        public TopPortfolio Portfolio
        {
            get { return portfolio; }
            set
            {
                if (performerEfficiencyWorker.IsBusy)
                    return;
                portfolio = value;
                if (portfolio == null)
                {
                    nameLabel.Text = "";
                    name2Label.Text = nameLabel.Text;
                    percentFinanceLabel.Amount = 0;
                    currencyFinanceLabel.Amount = 0;
                    currencyFinanceLabel.Suffix = "";
                    pointFinanceLabel.Amount = 0;
                    performersFastGrid.DataBind(new List<PerformerStatEx>(), null);
                    statisticsFastGrid.DataBind(new List<object>());
                    if (profitFastMultiChart.Graphs[0].Series.Count == 1)
                        profitFastMultiChart.Graphs[0].Series[0].Clear();
                    openedOrdersFastGrid.DataBind(new List<object>());
                    closedOrdersFastGrid.DataBind(new List<object>());
                    return;
                }

                // заголовок
                nameLabel.Text = string.Format("{0} {1}", Localizer.GetString("TitleTOP"), portfolio.ParticipantCount);
                name2Label.Text = nameLabel.Text;

                // подсказка в виде формулы и расшифровки
                var expressionToolTip = portfolio.Criteria + "\n";
                var resolver = new ExpressionResolver(portfolio.Criteria);
                foreach (var var in resolver.GetVariableNames())
                {
                    var varName = var;
                    var field = PerformerStatField.fields.FirstOrDefault(f =>
                        !string.IsNullOrEmpty(f.ExpressionParamName) && f.ExpressionParamName.Equals(
                        varName, StringComparison.OrdinalIgnoreCase));
                    if (field == null)
                        continue;
                    expressionToolTip += "\n" + field.ExpressionParamName + " - " + field.ExpressionParamTitle;
                }
                toolTip.SetToolTip(nameLabel, expressionToolTip);

                UpdateSummaryStatistics();

                // трейдеры
                var cats = SubscriptionModel.Instance.SubscribedCategories ?? new List<Contract.Entity.Subscription>();
                performersFastGrid.DataBind(portfolio.Managers.Select(performer => new PerformerStatEx(performer)
                    {
                        IsSubscribed = cats.Any(c => c.Service == performer.Service),
                    }).ToList(), null);
                var blank = new PerformerStatEx();
                var colTitle = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.TradeSignalTitle));
                var titleMinWidth = -1;
                if (colTitle != null)
                    titleMinWidth = colTitle.ColumnMinWidth;
                performersFastGrid.Grid.CheckSize(true);
                if (colTitle != null)
                    colTitle.ColumnMinWidth = titleMinWidth; // наименование сигнала может не помещаться - задаем так, чтобы поместился заголовок
                // остальные колонки сокращаться не должны
                var column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.Profit));
                if (column != null)
                    column.ColumnWidth = column.ColumnMinWidth;
                column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.SumProfitPoints));
                if (column != null)
                    column.ColumnWidth = column.ColumnMinWidth;
                column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.ProfitLastMonthsAbs));
                if (column != null)
                    column.ColumnWidth = column.ColumnMinWidth;

                // статистика
                RebindStatisticsGrid();
                
                // упрощенный график доходности
                if (portfolio.Statistics != null && portfolio.Statistics.Chart != null)
                {
                    for (var i = 0; i < portfolio.Statistics.Chart.Length; i++)
                        profitFastMultiChart.Graphs[0].Series[0].Add(new TradeSharp.Util.Cortege2<int, int>(i, portfolio.Statistics.Chart[i]));
                    profitFastMultiChart.Initialize();
                }

                UpdateSelectedStrategy();

                if (portfolio.ManagedAccount.HasValue)
                    performerEfficiencyWorker.RunWorkerAsync(portfolio.ManagedAccount.Value);
                else
                {
                    openedOrdersStandByControl.IsShown = false;
                    openedOrdersStandByControl.Hide();
                    closedOrdersStandByControl.IsShown = false;
                    closedOrdersStandByControl.Hide();
                    profitByMonthStandByControl.IsShown = false;
                    profitByMonthStandByControl.Hide();
                    CreateDetailedCharts();
                    if (PortfolioChanged != null)
                        PortfolioChanged(this, new EventArgs());
                }
            }
        }

        public bool HideStrategies
        {
            get { return !strategiesComboBox.Visible; }
            set { strategiesComboBox.Visible = !value; }
        }

        /// <summary>
        /// признак загрузки всех данных по топу (закрытые сделки, статистику по ним)
        /// </summary>
        public bool LoadAllData { get; set; }

        private bool isSubsribed;
        public bool IsSubsribed
        {
            get { return isSubsribed; }
            set
            {
                isSubsribed = value;
                subscribedLabel.Visible = isSubsribed;
                subscribeButton.Text = isSubsribed ? Localizer.GetString("TitleUnsubscribe") : Localizer.GetString("TitleSubscribe");
                subscribeToolStripMenuItem.Text = subscribeButton.Text;
                tradeSettingsButton.Visible = isSubsribed && showFullInfo;
                tradeSettingsToolStripMenuItem.Visible = isSubsribed;
            }
        }

        private bool showFullInfo;
        public bool ShowFullInfo
        {
            get { return showFullInfo; }
            set
            {
                showFullInfo = value;
                showFullInfoButton.Visible = !showFullInfo;
                //button1.Visible = showFullInfo; // Инвестировать
                subscribeButton.Visible = showFullInfo;
                button3.Visible = showFullInfo;
                actionsButton.Visible = !showFullInfo;
                logoPanel.Visible = showFullInfo;
                tradeSettingsButton.Visible = isSubsribed && showFullInfo;
                // поскольку колонки FastGrid пока невозможно растягивать при FitWidth=true
                // то при ShowFullInfo=false FitWidth=false, т.к. данные в этом режиме заведомо не уместятся
                //statisticsFastGrid.FitWidth = showFullInfo;
            }
        }

        private readonly BackgroundWorker performerEfficiencyWorker = new BackgroundWorker();

        private readonly BackgroundWorker openedOrdersWorker = new BackgroundWorker();

        private readonly BackgroundWorker closedOrdersWorker = new BackgroundWorker();

        private bool detailedChartsLoaded;

        private bool closedOrdersLoaded;

        public TopPortfolioControl()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);

            name2Label.Font = new Font(name2Label.Font, FontStyle.Bold);

            performerEfficiencyWorker.DoWork += GetPerformerEfficiency;
            performerEfficiencyWorker.RunWorkerCompleted += PerformerEfficiencyWorkerCompleted;

            openedOrdersWorker.DoWork += GetOpenedOrders;
            openedOrdersWorker.RunWorkerCompleted += OpenedOrdersWorkerCompleted;

            closedOrdersWorker.DoWork += GetClosedOrders;
            closedOrdersWorker.RunWorkerCompleted += ClosedOrdersWorkerCompleted;

            // упрощенный график доходности
            profitFastMultiChart.Graphs[0].Series.Add(new Series("a", "b", new Pen(Color.Red, 2f)));

            SetupGrids();

            pointFinanceLabel.Suffix = Localizer.GetString("TitlePointsUnits");
        }

        private void SetupGrids()
        {
            var blank = new PerformerStatEx();
            // участники
            performersFastGrid.LoadPerformersGridSelectedColumns += () => new List<string>
                {
                    blank.Property(p => p.ChartIndex),
                    blank.Property(p => p.TradeSignalTitle),
                    blank.Property(p => p.Profit),
                    blank.Property(p => p.SumProfitPoints),
                    blank.Property(p => p.ProfitLastMonthsAbs),
                };
            performersFastGrid.SetupGrid();
            // переименовываем и задаем подходящее форматирование
            var colTradeSignalTitle = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.TradeSignalTitle));
            if (colTradeSignalTitle != null)
            {
                colTradeSignalTitle.Title = Localizer.GetString("TitleTrader");
                colTradeSignalTitle.ShowClippedContent = true;
            }
            var column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.Profit));
            if (column != null)
            {
                column.Title = "%";
                column.cellFormatting = null; // отменяем дополнение цифр единицами измерения, которые выполняется в PerformersFastGrid
                column.colorColumnFormatter = DigitColorColumnFormatter;
            }
            column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.SumProfitPoints));
            if (column != null)
            {
                column.Title = Localizer.GetString("TitlePointsUnits");
                column.cellFormatting = null;
                column.colorColumnFormatter = DigitColorColumnFormatter;
            }
            column = performersFastGrid.Grid.Columns.FirstOrDefault(c => c.PropertyName == blank.Property(p => p.ProfitLastMonthsAbs));
            if (column != null)
            {
                column.Title = "$";
                column.cellFormatting = null;
                column.FormatString = "f0";
                column.colorColumnFormatter = DigitColorColumnFormatter;
            }
            performersFastGrid.Grid.CheckSize(true);
            
            // статистика
            statisticsFastGrid.SelectEnabled = false;
            statisticsFastGrid.Columns.Add(new FastColumn("a", Localizer.GetString("TitleStatisticsIndex"))
                {
                    ShowClippedContent = true,
                });
            statisticsFastGrid.Columns.Add(new FastColumn("b", " ")
                {
                    ColumnWidth = 50,
                    colorColumnFormatter = DigitColorColumnFormatter
                });
            
            // таблицы с позициями
            PerformerStatistic.SetupDealsGrid(openedOrdersFastGrid, false);
            PerformerStatistic.SetupDealsGrid(closedOrdersFastGrid, true);

            // суммарная статистика
            var blankSummaryStatItem = new SummaryStatItem(string.Empty, Color.Empty);
            gridSummaryStat.Columns.Add(new FastColumn(blankSummaryStatItem.Property(p => p.ValueString), Localizer.GetString("TitleValue")));
            gridSummaryStat.colorFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    var rowData = (SummaryStatItem) value;
                    fontColor = rowData.RowColor;
                };
            gridSummaryStat.UserDrawCellText += PerformerStatistic.GridSummaryStatUserDrawCellText;
        }

        private static void DigitColorColumnFormatter(object cellValue, out Color? backColor, out Color? fontColor)
        {
            backColor = null;
            fontColor = null;
            if (cellValue == null) return;
            if (cellValue is float || cellValue is double)
            {
                var rst = (float) cellValue;
                fontColor = rst < 0 ? Color.Red : rst < 1 ? Color.Black : Color.Blue;
                return;
            }
            var strVal = cellValue as string;
            if (string.IsNullOrEmpty(strVal)) return;
            fontColor = strVal[0] == '-' ? Color.Red : char.IsDigit(strVal[0]) ? Color.Blue : (Color?) null;
        }

        public void SetStrategies(List<string> strategies)
        {
            strategiesComboBox.Items.Clear();
            if (strategies == null || strategies.Count == 0)
                return;
            strategiesComboBox.SelectedIndexChanged -= StrategiesComboBoxSelectedIndexChanged;
            strategiesComboBox.Items.AddRange(strategies.Select(s => s as object).ToArray());
            strategiesComboBox.SelectedIndexChanged += StrategiesComboBoxSelectedIndexChanged;

            // combobox size
            var g = strategiesComboBox.CreateGraphics();
            var maxWidth =
                strategies.Max(
                    s =>
                    g.MeasureString(s, strategiesComboBox.Font).ToSize().Width +
                    SystemInformation.VerticalScrollBarWidth);
            strategiesComboBox.Width = maxWidth;
            strategiesComboBox.DropDownWidth = maxWidth;
            
            UpdateSelectedStrategy();
        }

        public void LoadDeals(bool openedDeals)
        {
            if (openedDeals)
            {
                if (!openedOrdersWorker.IsBusy)
                    openedOrdersWorker.RunWorkerAsync(portfolio.ManagedAccount);
            }
            else
            {
                if (!closedOrdersWorker.IsBusy && !closedOrdersLoaded)
                    closedOrdersWorker.RunWorkerAsync(portfolio.ManagedAccount);
            }
        }

        private void UpdateSelectedStrategy()
        {
            if (portfolio == null || strategiesComboBox.Items.Count == 0)
                return;
            var index = strategiesComboBox.Items.Cast<string>().IndexOf(portfolio.Name);
            if (index == -1)
                return;
            strategiesComboBox.SelectedIndexChanged -= StrategiesComboBoxSelectedIndexChanged;
            strategiesComboBox.SelectedIndex = index;
            strategiesComboBox.SelectedIndexChanged += StrategiesComboBoxSelectedIndexChanged;
        }

        private static void SetupChart(FastMultiChart.FastMultiChart chart)
        {
            chart.Graphs[0].Series.Clear();
            chart.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chart.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chart.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chart.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chart.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chart.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            chart.ShowScaleDivisionXLabel = true;
            chart.ShowScaleDivisionYLabel = true;
            chart.ScrollBarHeight = 30;
            chart.ShowHints = true;
            var blank = new TimeBalans();
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Time), blank.Property(p => p.Balans), new Pen(Color.Red, 2f))
                {
                    XMemberTitle = Localizer.GetString("TitleDate"),
                    YMemberTitle = Localizer.GetString("TitleBalance")
                });
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Time), blank.Property(p => p.Balans), new Pen(Color.Green, 2f))
                {
                    XMemberTitle = Localizer.GetString("TitleDate"),
                    YMemberTitle = "HWM"
                });
        }

        // создание детилизированных графиков доходности
        private void CreateDetailedCharts()
        {
            if (detailedChartsLoaded)
                return;

            SetupChart(profitFastMultiChart);
            SetupChart(profit1000FastMultiChart);

            profitFastMultiChart.Graphs[0].Series[0].Clear();
            if (efficiency.listEquity != null)
            {
                foreach (var pt in efficiency.listEquity)
                    profitFastMultiChart.Graphs[0].Series[0].Add(new TimeBalans(pt.time, pt.equity));
                var hwm = PerformerStatistic.GetHighWaterMarks(efficiency.listEquity, efficiency.listTransaction);
                foreach (var pt in hwm)
                    profitFastMultiChart.Graphs[0].Series[1].Add(new TimeBalans(pt.time, pt.equity));
            }
            profitFastMultiChart.Initialize();

            // график доходности на 1000$
            profit1000FastMultiChart.Graphs[0].Series[0].Clear();
            if (efficiency.listProfit1000 != null)
            {
                foreach (var pt in efficiency.listProfit1000)
                    profit1000FastMultiChart.Graphs[0].Series[0].Add(new TimeBalans(pt.time, pt.equity));
                var hwm = PerformerStatistic.GetHighWaterMarks(efficiency.listProfit1000);
                foreach (var pt in hwm)
                    profit1000FastMultiChart.Graphs[0].Series[1].Add(new TimeBalans(pt.time, pt.equity));
            }
            profit1000FastMultiChart.Initialize();

            detailedChartsLoaded = true;
        }

        // copied from PerformerStatistics
        private void CreateDiagram()
        {
            var profitByMonth = new Dictionary<DateTime, double>();
            foreach (var deal in efficiency.closedDeals)
            {
                if (!deal.TimeExit.HasValue)
                    continue;
                var date = deal.TimeExit.Value;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                if (profitByMonth.ContainsKey(firstDayOfMonth))
                    profitByMonth[firstDayOfMonth] += deal.ResultDepo;
                else
                    profitByMonth.Add(firstDayOfMonth, deal.ResultDepo);
            }
            var firstDayOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            foreach (var deal in efficiency.openedDeals)
            {
                if (profitByMonth.ContainsKey(firstDayOfCurrentMonth))
                    profitByMonth[firstDayOfCurrentMonth] += deal.ResultDepo;
                else
                    profitByMonth.Add(firstDayOfCurrentMonth, deal.ResultDepo);
            }
            var positiveChartData = new List<TradeSharp.Util.Cortege2<string, double>>();
            var negativeChartData = new List<TradeSharp.Util.Cortege2<string, double>>();
            foreach (var pair in profitByMonth)
            {
                if (pair.Value > 0)
                {
                    positiveChartData.Add(new TradeSharp.Util.Cortege2<string, double>(pair.Key.ToString("MMMM yyyy"),
                                                                               pair.Value));
                    negativeChartData.Add(new TradeSharp.Util.Cortege2<string, double>(pair.Key.ToString("MMMM yyyy"), 0));
                }
                else
                {
                    negativeChartData.Add(new TradeSharp.Util.Cortege2<string, double>(pair.Key.ToString("MMMM yyyy"),
                                                                               pair.Value));
                    positiveChartData.Add(new TradeSharp.Util.Cortege2<string, double>(pair.Key.ToString("MMMM yyyy"), 0));
                }
            }
            try
            {
                // ReSharper disable AssignNullToNotNullAttribute
                profitByMonthChart.Series[0].Points.DataBind(positiveChartData, "a", "b", null);
                profitByMonthChart.Series[1].Points.DataBind(negativeChartData, "a", "b", null);
                // ReSharper restore AssignNullToNotNullAttribute
            }
            catch
            {
            }
        }

        private void GetPerformerEfficiency(object sender, DoWorkEventArgs e)
        {
            var managedAccount = (int) e.Argument;

            try
            {
                var efc = TradeSharpAccountStatistics.Instance.proxy.GetAccountEfficiencyShort(managedAccount, false, false);
                e.Result = efc ?? new AccountEfficiency(new PerformerStat
                    {
                        Account = managedAccount,
                        DepoCurrency = "USD"
                    });
            }
            catch (Exception ex)
            {
                Logger.Info("TopPortfolioControl.GetPerformerEfficiency: error calling GetAccountEfficiencyShort", ex);
                return;
            }
        }

        private void PerformerEfficiencyWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            efficiency = (AccountEfficiency) e.Result;
            if (efficiency == null)
                return;

            // строим детализированные графики доходности
            CreateDetailedCharts();

            if (PortfolioChanged != null)
                PortfolioChanged(this, new EventArgs());

            if (LoadAllData && !openedOrdersWorker.IsBusy)
                openedOrdersWorker.RunWorkerAsync(portfolio.ManagedAccount);
        }

        private void GetOpenedOrders(object sender, DoWorkEventArgs e)
        {
            var managedAccount = (int) e.Argument;
            List<MarketOrder> deals;

            try
            {
                deals = TradeSharpAccountStatistics.Instance.proxy.GetAccountDeals(managedAccount, true);
            }
            catch (Exception ex)
            {
                Logger.Info("TopPortfolioControl.GetOpenedOrders: error calling GetAccountDeals", ex);
                deals = new List<MarketOrder>();
            }
            e.Result = deals;
        }

        private void OpenedOrdersWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            efficiency.openedDeals = (List<MarketOrder>) e.Result;
            var openedDeals = efficiency.openedDeals; //.ToList();
            DalSpot.Instance.CalculateOpenedPositionsCurrentResult(openedDeals, QuoteStorage.Instance.ReceiveAllData(), efficiency.Statistics.DepoCurrency);
            openedOrdersFastGrid.DataBind(openedDeals);

            openedOrdersStandByControl.IsShown = false;
            openedOrdersStandByControl.Hide();
            openedOrdersFastGrid.Show();

            if (OpenedDealsLoaded != null)
                OpenedDealsLoaded(this, new EventArgs());

            UpdateSummaryStatistics();

            PerformerStatistic.BindSummaryPositions(efficiency, tickersAndVolumesBarControl);
            summaryPositionsStandByControl.IsShown = false;
            summaryPositionsStandByControl.Hide();
            summaryPositionsPanel.Show();

            if (LoadAllData && !closedOrdersWorker.IsBusy && !closedOrdersLoaded)
                closedOrdersWorker.RunWorkerAsync(portfolio.ManagedAccount);
        }

        private void UpdateSummaryStatistics()
        {
            percentFinanceLabel.Amount = portfolio.Statistics.Profit;
            currencyFinanceLabel.Amount = portfolio.Statistics.ProfitLastMonthsAbs;
            currencyFinanceLabel.Suffix = " " + portfolio.Statistics.DepoCurrency;
            pointFinanceLabel.Amount = portfolio.Statistics.SumProfitPoints;
            PerformerStatistic.BindSummaryStatistics(gridSummaryStat, portfolio.Statistics, efficiency, showFullInfo);
            gridSummaryStat.CheckSize(true);
            gridSummaryStat.CalcSetTableMinWidth();
            if (gridSummaryStat.MinimumTableWidth.HasValue)
                gridSummaryStat.MinimumSize = new Size(gridSummaryStat.MinimumTableWidth.Value, 0);
            var gridHeight = gridSummaryStat.rows.Count * gridSummaryStat.CellHeight + 1;
            summaryTableLayoutPanel.Height = gridHeight;
        }

        private void GetClosedOrders(object sender, DoWorkEventArgs e)
        {
            var managedAccount = (int)e.Argument;
            List<MarketOrder> deals;

            try
            {
                deals = TradeSharpAccountStatistics.Instance.proxy.GetAccountDeals(managedAccount, false);
            }
            catch (Exception ex)
            {
                Logger.Info("TopPortfolioControl.GetClosedOrders: error calling GetAccountDeals", ex);
                deals = new List<MarketOrder>();
            }
            e.Result = deals;
        }

        private void ClosedOrdersWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            efficiency.closedDeals = (List<MarketOrder>) e.Result;
            if (efficiency.closedDeals == null)
                return;
            var closedDeals = efficiency.closedDeals.ToList();
            // суммарная закрытая сделка
            MarketOrder sumClosed = null;
            if (closedDeals.Count > 0)
            {
                var sumVolume = closedDeals.Sum(d => d.Volume * d.Side);
                sumClosed = new MarketOrder
                {
                    Symbol = "Сумм",
                    Volume = Math.Abs(sumVolume),
                    Side = Math.Sign(sumVolume),
                    ResultDepo = closedDeals.Sum(d => d.ResultDepo),
                    TimeEnter = closedDeals.Min(d => d.TimeEnter)
                };
                closedDeals.Add(sumClosed);
            }
            closedOrdersFastGrid.DataBind(closedDeals);
            if (sumClosed != null)
            {
                var lastRow = closedOrdersFastGrid.rows.First(r => r.ValueObject == sumClosed);
                lastRow.anchor = FastRow.AnchorPosition.AnchorBottom;
            }

            closedOrdersLoaded = true;
            closedOrdersStandByControl.IsShown = false;
            closedOrdersStandByControl.Hide();
            closedOrdersFastGrid.Show();

            // обновляем статистику
            RebindStatisticsGrid();

            // статистика по валютам
            try
            {
                // ReSharper disable AssignNullToNotNullAttribute
                var blank = new DealCountBySymbol();
                countByTickerChart.Series[0].Points.DataBind(PerformerStatistic.GetDealCountBySymbol(efficiency),
                                                             blank.Property(p => p.Title),
                                                             blank.Property(p => p.DealCount), null);
                // ReSharper restore AssignNullToNotNullAttribute
            }
            catch
            {
            }
            countByTickerStandByControl.IsShown = false;
            countByTickerStandByControl.Hide();
            countByTickerChart.Show();

            // строим диаграмму
            CreateDiagram();

            profitByMonthStandByControl.IsShown = false;
            profitByMonthStandByControl.Hide();
            profitByMonthChart.Show();

            if (ClosedDealsLoaded != null)
                ClosedDealsLoaded(this, new EventArgs());
        }

        private void ActionsButtonClick(object sender, EventArgs e)
        {
            actionsContextMenuStrip.Show(actionsButton, 0, actionsButton.Height);
        }

        private void StrategiesComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (StrategyChanged != null)
                StrategyChanged(this, (string) strategiesComboBox.SelectedItem);
        }

        private void ShowFullInfoButtonClick(object sender, EventArgs e)
        {
            var form = new TopPortfolioForm(portfolio) {IsSubscribed = IsSubsribed};
            form.Show(this);
        }

        private void OrdersTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            // все загружается автоматически
            /*if (ordersTabControl.SelectedTab == closedOrdersTabPage)
            {
                if (!closedOrdersWorker.IsBusy && !closedOrdersLoaded)
                    closedOrdersWorker.RunWorkerAsync(portfolio.ManagedAccount);
            }*/
        }

        private void DetailChartsToolStripMenuItemClick(object sender, EventArgs e)
        {
            CreateDetailedCharts();
        }

        private void ChartTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (chartTabControl.SelectedTab == profit1000TabPage)
            {
                CreateDetailedCharts();
            }
        }

        private void Subscribe(object sender, EventArgs e)
        {
            RequestStatus status;
            try
            {
                if (isSubsribed)
                {
                    status =
                        AccountModel.Instance.ServerProxy.UnsubscribePortfolio(
                            CurrentProtectedContext.Instance.MakeProtectedContext(),
                            AccountModel.Instance.GetUserLogin(), true, true);
                }
                else
                {
                    var complete = new CompleteSubscribeOnPortfolioDlg(portfolio).ShowDialog() == DialogResult.OK;
                    if (!complete) return;

                    var tradeSettings = new AutoTradeSettings();
                    // открыть диалог настройки авто-торговли
                    var dlg = new AutoTradeSettingsForm();
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        tradeSettings = dlg.sets;
                    status =
                        AccountModel.Instance.ServerProxy.SubscribeOnPortfolio(
                            CurrentProtectedContext.Instance.MakeProtectedContext(),
                            AccountModel.Instance.GetUserLogin(), portfolio, null, tradeSettings);
                }
            }
            catch (Exception ex)
            {
                //4 debug
                MessageBox.Show(this, "Операция выполнена с ошибкой:" + Environment.NewLine + ex.Message, Localizer.GetString("TitleWarning"),
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logger.Info("TopPortfolioControl.Subscribe: error calling SubscribeOnPortfolio/UnsubscribePortfolio", ex);
                return;
            }
            if (status == RequestStatus.OK)
                MessageBox.Show(this, "Операция выполнена успешно", Localizer.GetString("TitleInformation"), MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            else
                MessageBox.Show(this,
                                "Операция выполнена с ошибкой:" + Environment.NewLine +
                                EnumFriendlyName<RequestStatus>.GetString(status), Localizer.GetString("TitleWarning"), MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
            IsSubsribed = !isSubsribed;
            if (SubscriptionChanged != null)
                SubscriptionChanged(this, new EventArgs());
        }

        private void EditTradeSettings(object sender, EventArgs e)
        {
            try
            {
                var subscribedPortfolio = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolio(
                    AccountModel.Instance.GetUserLogin());
                var form = new AutoTradeSettingsForm(subscribedPortfolio.TradeSettings);
                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;
                var status = AccountModel.Instance.ServerProxy.ApplyPortfolioTradeSettings(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountModel.Instance.GetUserLogin(), form.sets);
                if (status == RequestStatus.OK)
                    MessageBox.Show(this, "Операция выполнена успешно", Localizer.GetString("TitleInformation"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                else
                    MessageBox.Show(this,
                                    "Операция выполнена с ошибкой:" + Environment.NewLine +
                                    EnumFriendlyName<RequestStatus>.GetString(status), Localizer.GetString("TitleWarning"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                //4 debug
                MessageBox.Show(this, "Операция выполнена с ошибкой:" + Environment.NewLine + ex.Message, Localizer.GetString("TitleWarning"),
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logger.Info("TopPortfolioControl.EditTradeSettings: error calling ApplyPortfolioTradeSettings", ex);
                return;
            }
        }

        private void RebindStatisticsGrid()
        {
            PerformerStatistic.RebindStatisticsFastGrid(statisticsFastGrid, portfolio.Statistics, efficiency);
        }
    }
}
