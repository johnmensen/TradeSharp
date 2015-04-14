using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Entity;
using FastMultiChart;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Contract;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.UI.Util.Forms;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class PerformerStatistic : UserControl
    {
        private ForexInvestSiteAccessor siteAccessor;

        private const string DefaultUrl = "http://forexinvest.com/forecast/get_forecast_by_id?account=";

        public event Action DataBindCompleted;

        // исп. для переключения вкладки SubscriptionControl
        public Action<SubscriptionControl.ActivePage> pageTargeted;
        
        public event ChatControlBackEnd.EnterRoomDel EnterRoomRequested;

        // поток для загрузки статистики и открытых сделок
        private readonly BackgroundWorker worker = new BackgroundWorker();

        // поток для загрузки закрытых сделок
        private readonly BackgroundWorker workerLoadOrders = new BackgroundWorker();

        public string SignalTitle
        {
            get { return Performer.TradeSignalTitle; }
        }

        public string SubscriptionButtonTitle
        {
            get { return IsSubscribed ? Localizer.GetString("TitleUnsubscribe") : Localizer.GetString("TitleSubscribe"); }
        }

        /// <summary>
        /// Содержит все данные о статистике сигнальщика - коэффициенты и данные графиков доходности
        /// </summary>
        public AccountEfficiency Efficiency { get; private set; }

        public PerformerStat Performer { get; private set; }
        
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public bool IsSubscribed
        {
            get { return SubscriptionModel.Instance.SubscribedCategories.Any(s => s.Service == Performer.Service); }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public PerformerStatistic()
        {
            InitializeComponent();

            Init();
        }

        /// <summary>
        /// Попытка асинхронного получения данных статистики с сервера с их последующей привязкой
        /// </summary>
        public void DataBindAsynch(PerformerStat performer)
        {
            Performer = performer;

            AccountModel.Instance.Chat.RoomsReceived += RoomsReceived;
            AccountModel.Instance.Chat.GetRooms();

            worker.DoWork += GetPerformerEfficiency;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.RunWorkerAsync();

            workerLoadOrders.DoWork += WorkerLoadOrdersOnDoWork;
            workerLoadOrders.RunWorkerCompleted += WorkerLoadOrdersOnRunWorkerCompleted;
        }

        private void RoomsReceived(List<Room> rooms)
        {
            try
            {
                Invoke(new Action<List<Room>>(OnRoomReceive), rooms);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void OnRoomReceive(List<Room> rooms)
        {
            var ownedRooms = rooms.Where(r => r.Owner == Performer.UserId).Select(r => r.Name).ToList();
            foreach (var room in ownedRooms)
            {
                var control = new LinkLabel {Text = room};
                control.LinkClicked += LinkLabelClicked;
                //roomsFlowLayoutPanel.Controls.Add(control);
            }
            AccountModel.Instance.Chat.RoomsReceived -= RoomsReceived;
        }

        private void LinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = sender as LinkLabel;
            if (linkLabel == null)
                return;
            if (EnterRoomRequested != null)
                EnterRoomRequested(linkLabel.Text);
        }

        /// <summary>
        /// Заполняет данными свойство Efficiency (для графиков дохдности)
        /// </summary>
        private void GetPerformerEfficiency(object sender, DoWorkEventArgs e)
        {

            try
            {
                var efc = TradeSharpAccountStatistics.Instance.proxy.GetAccountEfficiencyShort(Performer.Account, true, false);
                if (efc != null)
                {
                    // произвести доп. вычисления
                }
                Efficiency = efc ?? new AccountEfficiency(new PerformerStat
                {
                    Account = Performer.Account,
                    DepoCurrency = "USD"
                });
            }
            catch (Exception ex)
            {
                Logger.Info("PerformerStatistics.GetPerformerEfficiency: error calling GetAccountEfficiency", ex);
                throw;
            }
        }

        /// <summary>
        /// Завершение асинхронной загрузки данных статистики с сервера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Parent == null) return;

            var userInfoExSource = new UserInfoExCache(TradeSharpAccountStatistics.Instance.proxy);
            var userInfo = userInfoExSource.GetUserInfo(Performer.UserId);
            if (userInfo != null)
            {
                photoPanel.BackgroundImageLayout = ImageLayout.Center;
                photoPanel.BackgroundImage = userInfo.AvatarBig;
                ContactListUtils.UnpackContacts(userInfo.Contacts, contactsList);
                aboutRichTextBox.Text = userInfo.About;
            }

            if (Efficiency == null)
            {
                profit1000tabPage.Enabled = false;
                profitTabPage.Enabled = false;
                openedDealsTabPage.Enabled = false;
                currenciesTabPage.Enabled = false;
                if (DataBindCompleted != null)
                    DataBindCompleted();
                return;
            }

            // суммарная статистика
            BindSummaryStatistics(gridSummaryStat, Performer, Efficiency, true);
            gridSummaryStat.CheckSize(true);
            var height = gridSummaryStat.CellHeight * gridSummaryStat.rows.Count + 1;
            gridSummaryStat.Height = height;
            summaryStatPanel.MinimumSize = new Size(0, (IsSubscribed ? 30 : 60) + height);

            // обычная статистика
            RebindStatisticsFastGrid(singleParametersFastGrid, Performer, null);
            singleParametersFastGrid.CheckSize(true);

            // подписать стоимость сигналов
            lblFee.Text = Performer.FeeUSD <= 0
                              ? Localizer.GetString("TitleFree")
                              : Performer.FeeUSD.ToStringUniformMoneyFormat() + " / " +
                                PaidService.GetMonthFeeFromDailyFee(Performer.FeeUSD).ToStringUniformMoneyFormat() +
                                " USD";
            lblFee.ForeColor = Performer.FeeUSD <= 0 ? SystemColors.ControlText : Color.Blue;

            // график доходности
            chartProfit.Graphs[0].Series[0].Clear();
            chartProfit.Graphs[0].Series[1].Clear();
            if (Efficiency.listEquity != null)
            {
                foreach (var pt in Efficiency.listEquity)
                    chartProfit.Graphs[0].Series[0].Add(new TimeBalans(pt.time, pt.equity));
                var hwm = GetHighWaterMarks(Efficiency.listEquity, Efficiency.listTransaction);
                foreach (var pt in hwm)
                    chartProfit.Graphs[0].Series[1].Add(new TimeBalans(pt.time, pt.equity));
            }
            chartProfit.Initialize();

            // график доходности на 1000
            chartProfit1000.Graphs[0].Series[0].Clear();
            chartProfit1000.Graphs[0].Series[1].Clear();
            if (Efficiency.listProfit1000 != null)
            {
                foreach (var pt in Efficiency.listProfit1000)
                    chartProfit1000.Graphs[0].Series[0].Add(new TimeBalans(pt.time, pt.equity));
                var hwm = GetHighWaterMarks(Efficiency.listProfit1000);
                foreach (var pt in hwm)
                    chartProfit1000.Graphs[0].Series[1].Add(new TimeBalans(pt.time, pt.equity));
            }
            chartProfit1000.Initialize();

            // таблицы сделок
            if (Efficiency != null && Efficiency.openedDeals != null)
                BindDeals(true);

            if (Parent == null) return;
            try
            {
                var blank = new DealCountBySymbol();
                chartCountByTicker.Series[0].Points.DataBind(GetDealCountBySymbol(Efficiency),
                                                             blank.Property(p => p.Title),
                                                             blank.Property(p => p.DealCount), null);
            }
            catch (Exception ex)
            {
                Logger.Info("PerformerStatistics.WorkerRunWorkerCompleted: error binding GetDealCountBySymbol to chart", ex);
                return;
            }

            // "Совокупные позиции"
            if (Efficiency != null)
                BindSummaryPositions(Efficiency, tickersAndVolumesBarControl);

            // прочие визуальные параметры
            txtSignalTitle.Text = SignalTitle;
            subscribeButton.Text = SubscriptionButtonTitle;
            btnInvest.Visible = !IsSubscribed;

            if (DataBindCompleted != null)
                DataBindCompleted();
        }

        private void BindDeals(bool bindOpenedDeals)
        {
            // открытые сделки
            if (bindOpenedDeals)
            {
                var openedDeals = Efficiency.openedDeals.ToList();
                openedDeals.ForEach(d => d.ResultPoints = DalSpot.Instance.GetPointsValue(d.Symbol,
                                                                                          d.Side*
                                                                                          ((d.PriceExit ?? d.PriceEnter) -
                                                                                           d.PriceEnter)));
                openedDealsFastGrid.DataBind(openedDeals);
                return;
            }

            // закрытые сделки
            var closedDeals = Efficiency.closedDeals.ToList();
            // суммарная закрытая сделка
            MarketOrder sumClosed = null;
            if (closedDeals.Count > 0)
            {
                var sumVolume = closedDeals.Sum(d => d.Volume*d.Side);
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
            closedDealsFastGrid.DataBind(closedDeals);
            if (sumClosed != null)
            {
                var lastRow = closedDealsFastGrid.rows.First(r => r.ValueObject == sumClosed);
                lastRow.anchor = FastRow.AnchorPosition.AnchorBottom;
            }
        }

        public static void RebindStatisticsFastGrid(FastGrid.FastGrid grid, PerformerStat performer, AccountEfficiency efficiency)
        {
            if (performer == null)
                return;
            var singleValueData = new List<TradeSharp.Util.Cortege2<string, string>>();
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleProfitInPercents"),
                    b = performer.Profit.ToString("f2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleDealsTotal"),
                    b = performer.DealsCount.ToStringUniformMoneyFormat()
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleMaximumRelativeDrawdownInPercents"),
                    b = performer.MaxRelDrawDown.ToString("N2")
                });

            if (efficiency != null) // при получении всех сделок
            {
                singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleProfitableDealCount"),
                    b = (efficiency.closedDeals.Count(d => d.ResultDepo > 0) +
                         efficiency.openedDeals.Count(d => d.ResultDepo > 0)).ToStringUniformMoneyFormat()
                });
                singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleLosingDealCount"),
                    b = (efficiency.closedDeals.Count(d => d.ResultDepo < 0) +
                         efficiency.openedDeals.Count(d => d.ResultDepo < 0)).ToStringUniformMoneyFormat()
                });
                singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                    {
                        a = Localizer.GetString("TitleDealsOpened"),
                        b = efficiency.DealsStillOpened.ToStringUniformMoneyFormat()
                    });
            }

            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleRatioOfAverageProfitToAverageLossShort"),
                    b = performer.GreedyRatio.ToString("N2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleMaximumLeverage"),
                    b = performer.MaxLeverage.ToString("N2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleProfitForNMonthsInPercents"),
                    b = performer.ProfitLastMonths.ToString("f2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleGAAnnualProfitInPercentsShort"),
                    b = performer.AvgYearProfit.ToString("N3")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleSharpeRatioShort"),
                    b = performer.Sharp.ToString("N2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleSubscriberCount"),
                    b = performer.SubscriberCount.ToString("d")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleFundsUnderManagementShort") + ", " + performer.DepoCurrency,
                    b = performer.Equity.ToStringUniformMoneyFormat()
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleRatingFS"),
                    b = performer.Score.ToString("f2")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleTradeTimeInDays"),
                    b = performer.DaysTraded.ToString("d")
                });
            singleValueData.Add(new TradeSharp.Util.Cortege2<string, string>
                {
                    a = Localizer.GetString("TitleProfitInPoints"),
                    b = performer.SumProfitPoints.ToStringUniformMoneyFormat(false)
                });
            grid.DataBind(singleValueData);

            // для удобочитаемости первая колонка сохраняет первоначальную минимальную ширину
            var skippedColumns = grid.Columns.Where(c => c.PropertyName == "a").ToList();
            var minWidths = skippedColumns.ToDictionary(c => c, c => c.ColumnMinWidth);
            grid.CheckSize(true);
            skippedColumns.ForEach(c => c.ColumnMinWidth = minWidths[c]);
            grid.Invalidate();
        }

        /// <summary>
        /// Предустановка свойств для контролов графиков доходности
        /// </summary>
        /// <param name="chart">контрол</param>
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
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Time), blank.Property(p => p.Balans), new Pen(Color.Green, 2f))
            {
                XMemberTitle = Localizer.GetString("TitleDate"),
                YMemberTitle = "HWM"
            });
        }

        /// <summary>
        /// Предустановка столбцов таблицы по сделкам
        /// </summary>
        public static void SetupDealsGrid(FastGrid.FastGrid grid, bool isClosedDealsTable)
        {
            var blankMarketOrder = new MarketOrder();
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.ID), "#") {ColumnWidth = 50});
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.Symbol), Localizer.GetString("TitleInstrument")) {ColumnWidth = 70});
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.Side), Localizer.GetString("TitleType"))
                {
                    ColumnWidth = 60,
                    formatter = s => (int) s < 0 ? "SELL" : "BUY"
                });
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.Volume), Localizer.GetString("TitleVolume"))
                {
                    ColumnWidth = 82,
                    formatter = value => ((int) value).ToStringUniformMoneyFormat()
                });
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.PriceEnter), Localizer.GetString("TitleEnter"))
                {
                    ColumnWidth = 64,
                    formatter = p => ((float) p).ToStringUniformPriceFormat()
                });
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.TimeEnter), Localizer.GetString("TitleEnterTime"))
                {
                    ColumnWidth = 82
                });
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.PriceExit), Localizer.GetString("TitleExit"))
                {
                    ColumnWidth = 64,
                    formatter = p => p == null ? "" : ((float) p).ToStringUniformPriceFormat()
                });
            grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.ResultBase), Localizer.GetString("TitleResultInUSDShort"))
                {
                    ColumnWidth = 82,
                    formatter = p => p == null ? "" : ((float) p).ToStringUniformPriceFormat()
                });
            if (isClosedDealsTable)
            {
                grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.TimeExit), Localizer.GetString("TitleExitTime"))
                    {
                        ColumnMinWidth = 160
                    });
                grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.ResultDepo), Localizer.GetString("TitleResult"))
                    {
                        ColumnMinWidth = 65,
                        formatter = value => ((float) value).ToStringUniformMoneyFormat(),
                        colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                            {
                                color = null;
                                fontColor = null;
                                if (value == null) return;
                                var rst = (float) value;
                                fontColor = rst < -0.9f ? Color.Red : rst < 1 ? Color.Black : Color.Blue;
                            }
                    });
            }
            else
            {
                grid.Columns.Add(new FastColumn(blankMarketOrder.Property(p => p.ResultPoints), Localizer.GetString("TitlePoints"))
                    {
                        ColumnMinWidth = 65,
                        formatter = value => ((float) value).ToStringUniformMoneyFormat(true),
                        colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                            {
                                color = null;
                                fontColor = null;
                                if (value == null) return;
                                var rst = (float) value;
                                fontColor = rst < -0.9f ? Color.Red : rst < 1 ? Color.Black : Color.Blue;
                            }
                    });
            }

            grid.FontAnchoredRow = new Font(grid.Font, FontStyle.Bold);
            grid.CheckSize(true);
            grid.CalcSetTableMinWidth();
            grid.ContextMenuRequested += OrdersFastGridContextMenuRequested;
        }

        static private void OrdersFastGridContextMenuRequested(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            var grid = sender as FastGrid.FastGrid;
            if (grid == null)
                return;
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem(Localizer.GetString("TitleExportMenu"));
            item.Click += (o, eventArgs) =>
            {
                var orders = grid.GetRowValues<MarketOrder>(false).ToList();
                new ExportPositionsForm(orders).ShowDialog();
            };
            menu.Items.Add(item);
            menu.Show(grid, mouseEventArgs.Location.X, mouseEventArgs.Location.Y);
        }

        /// <summary>
        /// Cоздание диаграммы доходности по месяцам
        /// </summary>
        private void CreateDiagram()
        {
            var profitByMonth = new Dictionary<DateTime, double>();
            foreach (var deal in Efficiency.closedDeals)
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
            foreach (var deal in Efficiency.openedDeals)
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

        public static List<EquityOnTime> GetHighWaterMarks(List<EquityOnTime> equity, List<BalanceChange> transactions = null)
        {
            var result = new List<EquityOnTime>();
            if (equity == null)
                return result;
            var dict = equity.ToDictionary(e => e.time, e => e.equity);
            if (transactions != null)
            {
                foreach (var t in transactions)
                {
                    if (dict.ContainsKey(t.ValueDate))
                        dict[t.ValueDate] += (float) t.SignedAmountDepo;
                    else
                        dict.Add(t.ValueDate, (float) t.SignedAmountDepo);
                }
            }
            if (dict.Count == 0)
                return result;
            var lastEquity = dict.First().Value;
            foreach (var equityOnTime in dict)
            {
                if (equityOnTime.Value > lastEquity)
                    lastEquity = equityOnTime.Value;
                result.Add(new EquityOnTime(lastEquity, equityOnTime.Key));
            }
            return result;
        }

        /// <summary>
        /// Получить статистику распределения сделок по инструментам
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DealCountBySymbol> GetDealCountBySymbol(AccountEfficiency efficiency)
        {
            if (efficiency == null) return new List<DealCountBySymbol>();
            if (efficiency.closedDeals == null) return new List<DealCountBySymbol>();

            var dic = new Dictionary<string, DealCountBySymbol>();

            var dealLists = new[] { efficiency.closedDeals, efficiency.openedDeals };
            foreach (var dealList in dealLists)
                foreach (var deal in dealList)
            {
                if (dic.ContainsKey(deal.Symbol))
                    dic[deal.Symbol].DealCount++;
                else
                    dic.Add(deal.Symbol, new DealCountBySymbol { DealCount = 1, Title = deal.Symbol });
            }

            var result = new List<DealCountBySymbol>();

            // calc percentage
            var dealsTotal = efficiency.closedDeals.Count + efficiency.openedDeals.Count;
            var zeroPercentageSymbols = new List<string>();
            var zeroPercentageDealCount = 0;
            foreach (var symbol in dic)
            {
                var percent = symbol.Value.DealCount * 100 / dealsTotal;
                if (percent == 0)
                {
                    zeroPercentageSymbols.Add(symbol.Key);
                    zeroPercentageDealCount += symbol.Value.DealCount;
                    continue;
                }
                result.Add(new DealCountBySymbol
                    {
                        DealCount = symbol.Value.DealCount,
                        Title = string.Format("{0}\t({1}%)", symbol.Value.Title, percent)
                    });
            }

            if (zeroPercentageSymbols.Count != 0)
                result.Add(new DealCountBySymbol
                    {
                        DealCount = zeroPercentageDealCount,
                        Title = string.Format("{0}\t({1}%)", Localizer.GetString("TitleOthers"), zeroPercentageDealCount * 100 / dealsTotal)
                    });

            return result;
        }

        // отобразить данные суммарной статистики
        public static void BindSummaryStatistics(FastGrid.FastGrid gridSummaryStat, PerformerStat performerStat, AccountEfficiency efficiency, bool showFullInfo)
        {
            if (performerStat == null)
                return;

            // суммарная статистика
            var rowColors = new[] { Color.Red, Color.Blue, Color.Black };
            var color = performerStat.Profit >= 0 ? rowColors[1] : rowColors[0];
            var statItems = new List<SummaryStatItem>();
            if (showFullInfo)
            {
                statItems.Add(
                    new SummaryStatItem(
                        string.Format("{0}\t{1}", Localizer.GetString("TitleSubscriberCount"),
                                      performerStat.SubscriberCount),
                        rowColors[2]));
                statItems.Add(
                    new SummaryStatItem(string.Format("{0}\t{1}", Localizer.GetString("TitleInvestorCount"), 0),
                                        rowColors[2]));
                statItems.Add(new SummaryStatItem(
                                  string.Format("{0}, {1}\t{2}", Localizer.GetString("TitleFundsUnderManagementShort"),
                                                performerStat.DepoCurrency,
                                                performerStat.Equity.ToStringUniformMoneyFormat()), rowColors[2]));
            }
            statItems.Add(new SummaryStatItem(Localizer.GetString("TitleTotalProfit"), rowColors[2]));
            statItems.Add(new SummaryStatItem(string.Format("{0} {1} / {2:f2}% / {3} {4}",
                                                            performerStat.ProfitLastMonthsAbs.ToStringUniformMoneyFormat(false),
                                                            performerStat.DepoCurrency,
                                                            performerStat.Profit,
                                                            performerStat.SumProfitPoints.ToStringUniformMoneyFormat(false),
                                                            Localizer.GetString("TitlePointsUnits")), color));

            if (efficiency != null && efficiency.openedDeals != null && efficiency.openedDeals.Count > 0)
            {
                var openResultDepo = efficiency.openedDeals.Sum(o => o.ResultDepo);
                var openResultPoints = efficiency.openedDeals.Sum(o => o.ResultPoints);
                var openResultPercent = performerStat.Equity == 0 ? 0
                                            : 100 * openResultDepo / performerStat.Equity;
                var colorOpen = openResultDepo >= 0 ? rowColors[1] : rowColors[0];
                statItems.Add(new SummaryStatItem(Localizer.GetString("TitleCurrentProfit"), rowColors[2]));
                statItems.Add(new SummaryStatItem(string.Format("{0} {1} / {2:f2}% / {3} {4}",
                                                      openResultDepo.ToStringUniformMoneyFormat(),
                                                      performerStat.DepoCurrency,
                                                      openResultPercent,
                                                      openResultPoints.ToStringUniformMoneyFormat(),
                                                      Localizer.GetString("TitlePointsUnits")),
                                        colorOpen));
            }

            gridSummaryStat.DataBind(statItems);
        }

        // отобразить данные "Совокупные позиции"
        public static void BindSummaryPositions(AccountEfficiency efficiency, TickersAndVolumesBarControl tickersAndVolumesBarControl)
        {
            if (efficiency.openedDeals == null || efficiency.openedDeals.Count == 0)
                return;
            // исходные объемы валютных инструментов
            // ticker, <buy_volume, sell_volume>
            var summaryPositions = new Dictionary<string, TradeSharp.Util.Cortege2<int, int>>();
            var symbols = efficiency.openedDeals.Select(d => d.Symbol).Distinct().ToList();
            foreach (var symbol in symbols)
            {
                var ticker = symbol;
                var deals = efficiency.openedDeals.Where(d => d.Symbol == ticker).ToList();
                var buyVolume = deals.Sum(d => d.Side > 0 ? d.Volume : 0);
                var sellVolume = deals.Sum(d => d.Side < 0 ? d.Volume : 0);
                summaryPositions.Add(ticker, new TradeSharp.Util.Cortege2<int, int>(buyVolume, sellVolume));
            }

            // оценка исходных объемов в валюте депозита
            // ticker, <buy_volume, sell_volume>
            var summaryPositionsDepo = new Dictionary<string, TradeSharp.Util.Cortege2<decimal?, decimal?>>();
            decimal maxDepoValue = 0; // максимальное значение; исп. для масштабирования
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            foreach (var sumPos in summaryPositions)
            {
                string errorString;
                var buyDepoVolume = DalSpot.Instance.ConvertToTargetCurrency(sumPos.Key, true,
                                                                             efficiency.Statistics.DepoCurrency,
                                                                             sumPos.Value.a, quotes, out errorString);
                if (buyDepoVolume.HasValue && buyDepoVolume.Value > maxDepoValue)
                    maxDepoValue = buyDepoVolume.Value;
                var sellDepoVolume = DalSpot.Instance.ConvertToTargetCurrency(sumPos.Key, true,
                                                                             efficiency.Statistics.DepoCurrency,
                                                                             sumPos.Value.b, quotes, out errorString);
                if (sellDepoVolume.HasValue && sellDepoVolume.Value > maxDepoValue)
                    maxDepoValue = sellDepoVolume.Value;
                summaryPositionsDepo.Add(sumPos.Key, new TradeSharp.Util.Cortege2<decimal?, decimal?>(buyDepoVolume, sellDepoVolume));
            }

            // визуализация
            try
            {

                var graphics = tickersAndVolumesBarControl.CreateGraphics();
                tickersAndVolumesBarControl.Height = summaryPositions.Count * tickersAndVolumesBarControl.RowHeight;
                // под центральную надпись выделяем чуть больше расчетного места
                tickersAndVolumesBarControl.CenterLabelWidth =
                    (int) symbols.Max(s => graphics.MeasureString(s, tickersAndVolumesBarControl.Font).Width) + 10;
                for (var i = 0; i < summaryPositions.Count; i++)
                {
                    var sumPos = summaryPositions.ElementAt(i);
                    var sumPosDepo = summaryPositionsDepo[sumPos.Key];
                    var buyVolumeString = sumPos.Value.a > 1000
                                              ? (sumPos.Value.a / 1000).ToStringUniformMoneyFormat() + " K"
                                              : sumPos.Value.a.ToStringUniformMoneyFormat();
                    var sellVolumeString = sumPos.Value.b > 1000
                                               ? (sumPos.Value.b / 1000).ToStringUniformMoneyFormat() + " K"
                                               : sumPos.Value.b.ToStringUniformMoneyFormat();
                    tickersAndVolumesBarControl.Lines.Add(new TickerVolumes
                        {
                            CenterLabel = sumPos.Key,
                            LeftValue = sumPosDepo.b.HasValue ? (double) (sumPosDepo.b.Value / maxDepoValue) : 0,
                            RightValue = sumPosDepo.a.HasValue ? (double) (sumPosDepo.a.Value / maxDepoValue) : 0,
                            LeftLabel = sellVolumeString,
                            RightLabel = buyVolumeString
                        });
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("PerformerStatistic.BindSummaryPositions", ex);
            }
        }

        /// <summary>
        /// Подписаться / отписаться от сигнальщика
        /// </summary>
        private void SubscribeButtonClick(object sender, EventArgs e)
        {
            if (!SubscribeOrUnsubscribe(Performer, true)) return;
            subscribeButton.Text = SubscriptionButtonTitle;
            if (pageTargeted != null)
                pageTargeted(SubscriptionControl.ActivePage.Subscription);
        }

        private void Init()
        {
            try
            {
                // суммарная статистика
                var blank = new SummaryStatItem(string.Empty, Color.Empty);
                gridSummaryStat.Columns.Add(new FastColumn(blank.Property(p => p.ValueString),
                                                           Localizer.GetString("TitleValue")));
                gridSummaryStat.colorFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    var rowData = (SummaryStatItem) value;
                    fontColor = rowData.RowColor;
                };
                gridSummaryStat.UserDrawCellText += GridSummaryStatUserDrawCellText;

                // обычная статистика
                singleParametersFastGrid.Columns.Add(new FastColumn("a", Localizer.GetString("TitleStatisticsIndex")));
                singleParametersFastGrid.Columns.Add(new FastColumn("b", " ")
                    {
                        colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                            {
                                color = null;
                                fontColor = null;
                                var strVal = value as string;
                                if (string.IsNullOrEmpty(strVal)) return;
                                fontColor = strVal[0] == '-'
                                                ? Color.Red
                                                : char.IsDigit(strVal[0]) ? Color.Blue : (Color?) null;
                            }
                    });

                SetupChart(chartProfit);
                SetupChart(chartProfit1000);

                SetupDealsGrid(openedDealsFastGrid, false);
                SetupDealsGrid(closedDealsFastGrid, true);

                var metadataSettings = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("UserInfoEx");
                if (metadataSettings != null)
                    photoPanel.Size = new Size((int) metadataSettings["BigAvatarMaxSize"],
                                               (int) metadataSettings["BigAvatarMaxSize"]);

                siteAccessor = new ForexInvestSiteAccessor(forecastsWebBrowser);
            }
            catch (Exception ex)
            {
                Logger.Info("PerformerStatistic.Init", ex);
            }
        }

        /// <summary>
        /// подписаться или отписаться от получения торговых сигналов
        /// </summary>
        /// <param name="performer">сигнальщик</param>
        /// <param name="tradeAuto">оказать диалог автоторговли</param>
        public static bool SubscribeOrUnsubscribe(PerformerStat performer, bool? tradeAuto)
        {
            var isSubscribed = SubscriptionModel.Instance.SubscribedCategories.Any(s => s.Service == performer.Service);
            var isOk = isSubscribed ? Unsubscribe(performer) : Subscribe(performer, tradeAuto ?? false);

            if (!isOk) return false;

            if (!isSubscribed && (tradeAuto ?? false))
            {
                // показать диалог настройки авто-торговли
                var cat = SubscriptionModel.Instance.SubscribedCategories.FirstOrDefault(
                    c => c.Service == performer.Service);
                if (cat == null)
                    return false;
                var dlg = new AutoTradeSettingsForm(cat.AutoTradeSettings ?? new AutoTradeSettings());
                if (dlg.ShowDialog() != DialogResult.OK)
                    return false;
                cat.AutoTradeSettings = dlg.sets;                
            }
            else
            {
                // предупредить пользователя - изменения вступят в силу не мгновенно
                var msg = "Вы " + (isSubscribed
                                   ? "отказались от получения торговых сигналов \""
                                   : "подписались на получение торговых сигналов\n \"") + performer.TradeSignalTitle + "\"";
                MessageBox.Show(msg);
            }

            return true;
        }

        private static bool Subscribe(PerformerStat performer, bool tradeAuto)
        {
            if (performer == null) return false;

            // проверить, а хватит ли бабла в кошельке?
            // а готов ли заплатить?
            if (performer.FeeUSD > 0)
                if (!CheckUserHasEnoughMoneyOrRefusesToPay(performer))
                    return false;

            SubscriptionModel.Instance.AddSubscription(new Contract.Entity.Subscription
                {
                    RenewAuto = true,
                    Service = performer.Service,
                    TimeStarted = DateTime.Now.Date,
                    TimeEnd = DateTime.Now.Date.AddDays(1),
                    PaidService = new PaidService
                        {
                            Comment = performer.TradeSignalTitle
                        },
                    AutoTradeSettings = new AutoTradeSettings
                        {
                            TradeAuto = tradeAuto
                        }
                });
            return true;
        }

        private static bool CheckUserHasEnoughMoneyOrRefusesToPay(PerformerStat performer)
        {
            try
            {
                var wallet = TradeSharpWalletManager.Instance.proxy.GetUserWallet(
                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                        AccountModel.Instance.GetUserLogin());
                if (wallet == null) throw new Exception("Wallet is null");

                if (wallet.Balance == 0)
                {
                    MessageBox.Show("В вашем кошельке отсутствуют средства.\nНевозможно подписаться на платные сигналы (" +
                        performer.FeeUSD.ToStringUniformMoneyFormat() + " " + PerformerStat.FeeCurrency + ")",
                        "Недостаточно средств");
                    return false;
                }

                // перевести сумму
                var walletAmount = performer.FeeUSD;
                if (wallet.Currency != PerformerStat.FeeCurrency)
                {
                    string errorString;
                    var amount = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(wallet.Currency,
                        PerformerStat.FeeCurrency, (double) walletAmount, QuoteStorage.Instance.ReceiveAllData(), out errorString);
                    if (!amount.HasValue)
                        throw new Exception(errorString);
                    walletAmount = amount.Value;                                        
                }

                // достаточно ли средств
                var msgText = string.Format("Стоимость подписки: {0} {1}, баланс кошелька: {2} {1}",
                                            walletAmount.ToStringUniformMoneyFormat(), wallet.Currency, 
                                            wallet.Balance.ToStringUniformMoneyFormat());
                if (walletAmount > wallet.Balance)
                {
                    MessageBox.Show("В вашем кошельке недостаточно средств.\n" + msgText, "Недостаточно средств");
                    return false;
                }
                return MessageBox.Show(msgText + "\nПродолжить?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }
            catch (Exception ex)
            {
                Logger.Error("Subscribe - ошибка получения инф. по кошельку", ex);
                return MessageBox.Show("Ошибка получения информации о кошельке. Продолжить?", "Ошибка",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes;
            }
        }

        private static bool Unsubscribe(PerformerStat performer)
        {
            if (performer == null)
                return false;
            SubscriptionModel.Instance.RemoveSubscription(new Contract.Entity.Subscription
                {
                    Service = performer.Service
                });
            return true;
        }

        private void OrdersTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!closedDealsStandByControl.IsShown || workerLoadOrders.IsBusy)
                return;
            workerLoadOrders.RunWorkerAsync();
        }

        private void WorkerLoadOrdersOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            List<MarketOrder> deals;

            try
            {
                deals = TradeSharpAccountStatistics.Instance.proxy.GetAccountDeals(Performer.Account, false);                
            }
            catch (Exception ex)
            {
                Logger.Info("PerformerStatistics.WorkerLoadOrdersOnDoWork: error calling", ex);
                deals = new List<MarketOrder>();
            }
            doWorkEventArgs.Result = deals;
        }

        private void WorkerLoadOrdersOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // запомнить сделки
            Efficiency.closedDeals = (List<MarketOrder>) e.Result;
            // отобразить сделки в таблице
            BindDeals(false);
            // добавить недостающую статистику
            RebindStatisticsFastGrid(singleParametersFastGrid, Performer, Efficiency);
            singleParametersFastGrid.CheckSize(true);
            // создать диграмму
            CreateDiagram();
            
            // восстановиь нормальный вид
            closedDealsStandByControl.IsShown = false;
            closedDealsStandByControl.Visible = false;
            closedDealsFastGrid.Visible = true;

            profitByMonthStandByControl.IsShown = false;
            profitByMonthStandByControl.Visible = false;
            profitByMonthChart.Visible = true;
        }

        private void BtnInvestClick(object sender, EventArgs e)
        {
            AccountModel.Instance.InvestInPAMM(Performer);
            //subscribeButton.Text = SubscriptionButtonTitle;
            if (pageTargeted != null)
                pageTargeted(SubscriptionControl.ActivePage.Subscription);
        }

        public static void GridSummaryStatUserDrawCellText(int columnIndex, FastColumn column, FastCell cell,
                                                     FastGrid.BrushesStorage brushes, Graphics g, Point leftTop,
                                                     int cellWidth, int cellHeight, Font font, Brush brushFont,
                                                     Color? fontColor, int rowIndex, int cellPadding)
        {
            var cellString = cell.CellString;
            var cellFont = column.ColumnFont ?? font;
            var brush = brushFont;
            if (fontColor.HasValue)
                brush = brushes.GetBrush(fontColor.Value);
            var leftString = cellString;
            var rightString = "";
            if (cellString.IndexOf('\t') != -1)
            {
                leftString = cellString.Substring(0, cellString.IndexOf('\t'));
                rightString = cellString.Substring(cellString.IndexOf('\t') + 1);
            }
            g.DrawString(leftString, cellFont, brush,
                         leftTop.X + cellPadding, leftTop.Y + cellHeight / 2,
                         new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center});
            g.DrawString(rightString, cellFont, brush,
                         leftTop.X + cellWidth - cellPadding, leftTop.Y + cellHeight / 2,
                         new StringFormat {Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center});
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == forecasts && !siteAccessor.IsLogIn)
            {
                
                /*
                var login = AccountModel.Instance.GetUserLogin();
                if (string.IsNullOrEmpty(login)) return;
                var context = CurrentProtectedContext.Instance.MakeProtectedContext();
                if (context == null) return;
                var token = context.hash;
                var lockTime = context.clientLocalTime;

               
                siteAccessor.LoginOnSite(login, token, lockTime);*/

                try
                {
                    var url = DefaultUrl;
                    var sysMeta = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("TerminalUrl");
                    if (sysMeta != null)
                    {
                        url = (string)sysMeta["ForecastUrl"];
                    }
                    forecastsWebBrowser.Navigate(url + Performer.Account.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error("TabControlSelectedIndexChanged()", ex);
                }
                
            }
        }
    }

    /// <summary>
    /// Вспомогвтельный класс для привязки данных ко графикам доходности (chartProfit и chartProfit1000)
    /// </summary>
    public class TimeBalans
    {
        public float Balans { get; set; }
        public DateTime Time { get; set; }

        public TimeBalans()
        {
        }

        public TimeBalans(DateTime time, float balans)
        {
            Time = time;
            Balans = balans;
        }
    }

    public class SummaryStatItem
    {
        public string ValueString { get; set; }

        public Color RowColor { get; set; }

        public SummaryStatItem(string valStr, Color rowCol)
        {
            ValueString = valStr;
            RowColor = rowCol;
        }
    }

    public class DealCountBySymbol
    {
        public int DealCount { get; set; }

        public string Title { get; set; }
    }
}
