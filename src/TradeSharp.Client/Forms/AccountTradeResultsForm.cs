using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using FastMultiChart;
using TradeSharp.Client.BL;
using TradeSharp.Client.BL.Report;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class AccountTradeResultsForm : Form, IMdiNonChartWindow
    {
        /// <summary>
        /// Account ID - statistics
        /// </summary>
        private static readonly Dictionary<int, AccountStatistics> statByAccount = new Dictionary<int, AccountStatistics>();

        private AccountStatistics statistics;
        
        private const int DefaultColumnWidth = 75;
        
        public bool InstantCalculation { get; set; }
        
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public AccountTradeResultsForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupStatGrid();
            SetupChart(chartProfit, string.Empty);
            SetupChart(chartProfit1000, string.Empty);

            worker.DoWork += MakeCalculation;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
        }

        #region IMdiNonChartWindow
        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.Profit; }            
        }
        
        public int WindowInnerTabPageIndex
        {
            get
            {
                return (int)Invoke(new Func<object>(() => tabControl.SelectedIndex));
            }
            set
            {
                if (value < tabControl.TabCount)
                    tabControl.SelectedIndex = value;
            }
        }

        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }
        #endregion

        private static void SetupChart(FastMultiChart.FastMultiChart chart, string extraSeriesName)
        {
            chart.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chart.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chart.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chart.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chart.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chart.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            var blank = new BalanceByDate(DateTime.Now, 0);
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.X), blank.Property(p => p.Y),
                                                  new Pen(Color.FromArgb(80, 5, 5), 2f)));
            if (!string.IsNullOrEmpty(extraSeriesName))
            {
                chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.X), blank.Property(p => p.Y),
                                                      new Pen(Color.FromArgb(5, 105, 5), 2f)));
            }
        }

        private void SetupStatGrid()
        {
            dgStat.SelectEnabled = false;
            dgStat.CaptionHeight = 0;
            var blank = new StatItem();
            dgStat.Columns.Add(new FastColumn(blank.Property(p => p.Name), Localizer.GetString("TitleName")));
            dgStat.Columns.Add(new FastColumn(blank.Property(p => p.Result), Localizer.GetString("TitleValue")) { ColumnWidth = 140 });
            dgStat.ColorAltCellBackground = Color.FromArgb(230, 230, 230);
            dgStat.MinimumTableWidth = dgStat.Columns.Count * DefaultColumnWidth;
        }

        private int Timeframe
        {
            get { return tbTimeframe.Text.ToInt(); }
        }

        private void BtnUpdateClick(object sender, EventArgs e)
        {
            // запуск процесса-планировщика
            if (worker.IsBusy)
            {
                worker.CancelAsync();
                // прервать                
                return;
            }

            // сохранить настройки
            UserSettings.Instance.AccountResultsShowEquityCurve = cbShowBalanceCurve.Checked;
            UserSettings.Instance.AccountResultsUpdateQuotes = cbDefaultUploadQuotes.Checked;
            UserSettings.Instance.SaveSettings();

            btnUpdate.Text = Localizer.GetString("TitleBreak");
            worker.RunWorkerAsync();
        }
        
        private void MakeCalculation(object obj, DoWorkEventArgs args)
        {
            if (AccountStatus.Instance.AccountData == null ||
                MainForm.serverProxyTrade.proxy == null)
            {
                MessageBox.Show(Localizer.GetString("MessageNoConnectionServer"));
                return;
            }
            var accountId = AccountStatus.Instance.AccountData.ID;

            // получить сделки за указанный период и эквити на начало периода 
            // и все пополнения - снятия
            SetProgressValueSafe(10, Localizer.GetString("MessageGettingOrderHistory") + "...");
            if (worker.CancellationPending) return;
            List<MarketOrder> orders;
            var status = TradeSharpAccount.Instance.GetHistoryOrdersUncompressed(accountId,
                cbStartFrom.Checked ? dpStart.Value : (DateTime?)null, out orders);
            if (status != RequestStatus.OK || orders == null)
            {
                MessageBox.Show(string.Format(
                    Localizer.GetString("MessageUnableToGetOrderHistory") + ": {0}",
                                              EnumFriendlyName<RequestStatus>.GetString(status)));
                return;
            }
            // получить изменения баланса
            if (worker.CancellationPending) return;
            SetProgressValueSafe(20, Localizer.GetString("MessageGettingTransfersHistory") + "...");
            List<BalanceChange> balanceChanges;
            status = TradeSharpAccount.Instance.proxy.GetBalanceChanges(accountId, null, out balanceChanges);
            if (status != RequestStatus.OK || balanceChanges == null)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageUnableToGetTransfersHistory") + ": {0}",
                                              EnumFriendlyName<RequestStatus>.GetString(status)));
                return;
            }
            // получить открытые ордера
            if (worker.CancellationPending) return;
            SetProgressValueSafe(30, Localizer.GetString("MessageGettingOpenPosHistory") + "...");
            List<MarketOrder> openOrders;
            status = TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out openOrders);
            if (status != RequestStatus.OK || openOrders == null)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageUnableToGetOpenPosHistory") + ": {0}",
                                              EnumFriendlyName<RequestStatus>.GetString(status)));
                return;
            }
            // построить кривую equity и посчитать характеристики торговли по счету
            var stat = BuildEquityCurve(orders, openOrders, balanceChanges);
            statistics = stat;

            // сохранить доходность в словаре
            if (statByAccount.ContainsKey(accountId))
                statByAccount[accountId] = stat;
            else
                statByAccount.Add(accountId, stat);

            if (worker.CancellationPending) return;
            // построить график и вывести показатели доходности
            SetProgressValueSafe(90, Localizer.GetString("MessageMakingReport") + "...");

            SetProgressValueSafe(100, Localizer.GetString("MessageCalculationCompleted") + "...");

            Invoke(new Action(() =>
                {
                    UpdateTablesAndCharts(stat);
                    MessageBox.Show(this,
                        Localizer.GetString("MessageCalculationCompleted"), 
                        Localizer.GetString("TitleNotice"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }));

            SetProgressValueSafe(0, string.Empty);
        }

        private void UpdateTablesAndCharts(AccountStatistics stat)
        {
            ShowStatistics(stat);
            BuildEquityChartSafe(stat);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Invoke(new Action(UpdateCalculationStatus));
        }

        private void UpdateCalculationStatus()
        {
            btnUpdate.Text = Localizer.GetString("TitleCalculate");
            progressBar.Value = 0;

            btnMakeReportHTML.Enabled = statistics != null && statistics.listEquity != null;
        }

        private void ShowStatistics(AccountStatistics stat)
        {
            var depoCurx = AccountStatus.Instance.AccountData.Currency;
            var statList = new List<StatItem>
                {
                    new StatItem(Localizer.GetString("TitleAccountNumber"),
                                 AccountStatus.Instance.AccountData.ID.ToString()),
                    new StatItem(Localizer.GetString("TitleDate"), string.Format("{0:dd.MM.yyyy}", DateTime.Now)),
                    new StatItem(Localizer.GetString("TitleInitialDate"), string.Format("{0:dd.MM.yyyy}",
                                                                                        stat.listEquity.Count > 0
                                                                                            ? stat.listEquity[0].time
                                                                                            : cbStartFrom.Checked
                                                                                                  ? dpStart.Value
                                                                                                  : DateTime.Now)),
                    new StatItem(Localizer.GetString("TitleInitialDeposit"),
                                 stat.InitialBalance.ToStringUniformMoneyFormat(false) + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleCurrentDeposit"),
                                 stat.Statistics.Equity.ToStringUniformMoneyFormat(false) + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleDealsTotal"), stat.Statistics.DealsCount.ToString()),
                    new StatItem(Localizer.GetString("TitleOpenedDeals"), stat.DealsStillOpened.ToString()),
                    new StatItem(Localizer.GetString("TitleSummaryResult"),
                                 (stat.sumClosedResult + stat.sumOpenResult).ToStringUniformMoneyFormat() + " " +
                                 depoCurx),
                    new StatItem(Localizer.GetString("TitleSummaryResultInClosedDeals"),
                                 stat.sumClosedResult.ToStringUniformMoneyFormat() + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleSummaryResultInOpenedDeals"),
                                 stat.sumOpenResult.ToStringUniformMoneyFormat() + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleSummaryDepositsAndWithdraws"),
                                 stat.sumDeltaBalance.ToStringUniformMoneyFormat() + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleMaximumRelativeDrawdown"),
                                 stat.Statistics.MaxRelDrawDown.ToString("f1") + "%"),
                    new StatItem(Localizer.GetString("TitleGADailyProfit"),
                                 (100 * stat.ProfitGeomDay).ToStringUniformMoneyFormat() + "%"),
                    new StatItem(Localizer.GetString("TitleGAMonthlyProfit"),
                                 (100 * stat.ProfitGeomMonth).ToStringUniformMoneyFormat() + "%"),
                    new StatItem(Localizer.GetString("TitleGAAnnualProfit"),
                                 (100 * stat.ProfitGeomYear).ToStringUniformMoneyFormat() + "%")
                };
            dgStat.DataBind(statList, typeof(StatItem));
        }

        private delegate void SetProgressValueDel(int val, string text);
        
        private void SetProgressValueSafe(int val, string text)
        {
            Invoke(new SetProgressValueDel(SetProgressValue), val, text);
        }

        private void SetProgressValue(int val, string text)
        {
            progressBar.Value = val;
            lbProgress.Text = text;
            panelProgress.Height = !string.IsNullOrEmpty(text) ? 46 : 23;
            if (string.IsNullOrEmpty(text))
                btnUpdate.Enabled = true;
        }

        private void BuildEquityChartSafe(AccountStatistics stat)
        {
            Invoke(new Action<AccountStatistics>(BuildEquityChartUnsafe),
                   stat);
        }

        private void BuildEquityChartUnsafe(AccountStatistics stat)
        {
            chartProfit.Graphs[0].Series[0].Clear();
            //chartProfit.Graphs[0].Series[1].Clear();
            chartProfit1000.Graphs[0].Series[0].Clear();
            if (stat.listEquity == null || stat.listEquity.Count == 0) return;

            if (stat.listEquity != null)
                foreach (var pt in stat.listEquity)
                {
                    chartProfit.Graphs[0].Series[0].Add(new BalanceByDate(pt.time, pt.equity));
                }

            chartProfit.Initialize();
            
            // доход на 1000
            foreach (var pt in stat.listProfit1000)
            {
                chartProfit1000.Graphs[0].Series[0].Add(new BalanceByDate(pt.time, pt.equity));
            }
            chartProfit1000.Initialize();
        }

        private AccountStatistics BuildEquityCurve(
            List<MarketOrder> orders, 
            List<MarketOrder> openOrders,
            List<BalanceChange> balanceChanges)
        {
            var stat = new AccountStatistics
                           {
                               Statistics = new PerformerStat
                                   {
                                       DealsCount = orders.Count + openOrders.Count,
                                   },
                               DealsStillOpened = openOrders.Count,
                               listEquity = new List<EquityOnTime>()
                           };
            if (worker.CancellationPending) return stat;
            SetProgressValueSafe(40, Localizer.GetString("MessageActualizingQuoteHistory") + "...");

            if (balanceChanges.Count == 0)
            {
                MessageBox.Show(Localizer.GetString("MessageNoTransfersData"));
                return stat;
            }
            var curxDepo = AccountStatus.Instance.AccountData.Currency;
            // запросить котировки, если стоит соотв. флаг
            var quotesDic = UpdateQuotesForStats(orders, openOrders, curxDepo);

            // построить кривую эквити
            // отсчет от первого движения на счете либо от стартовой даты
            balanceChanges = balanceChanges.OrderBy(bc => bc.ValueDate).ToList();
            var startDate = cbStartFrom.Checked ? dpStart.Value : balanceChanges[0].ValueDate;
            
            // начальный баланс
            var initialBalance = !cbStartFrom.Checked ? balanceChanges[0].SignedAmountDepo
                : balanceChanges.Where(bc => bc.ValueDate <= startDate).Sum(bc => bc.SignedAmountDepo);

            // движения от начального баланса с заданным интервалом дискретизации
            var dueBalances = balanceChanges.Where(bc => bc.ValueDate > startDate).ToList();
            var dueDeals = orders.Union(openOrders).OrderBy(o => o.TimeEnter).ToList();

            var endDate = DateTime.Now;
            var balance = initialBalance;

            var cursor = new BacktestTickerCursor();
            var path = ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder;
            if (!cursor.SetupCursor(path, quotesDic.Keys.ToList(), quotesDic.Min(t => t.Value)))
            {
                MessageBox.Show(Localizer.GetString("MessageErrorGettingQuotesFromFiles"));
                return stat;
            }
            var currentQuotes = new Dictionary<string, QuoteData>();
            var timeframeMinutes = Timeframe;
            SetProgressValueSafe(60, Localizer.GetString("MessageCalculationInProcess") + "...");
            try
            {
                for (var time = startDate; time < endDate; time = time.AddMinutes(timeframeMinutes))
                {
                    if (worker.CancellationPending) return stat;
                    // получить баланс на дату
                    for (var i = 0; i < dueBalances.Count; i++)
                    {
                        if (dueBalances[i].ValueDate > time) break;
                        balance += dueBalances[i].SignedAmountDepo;
                        dueBalances.RemoveAt(i);
                        i--;
                    }
                    var equity = balance;

                    // получить текущие котировки
                    var candles = cursor.MoveToTime(time);
                    foreach (var candle in candles)
                    {
                        var quote = new QuoteData(candle.Value.open,
                                                  DalSpot.Instance.GetAskPriceWithDefaultSpread(candle.Key,
                                                                                                candle.Value.open),
                                                  candle.Value.timeOpen);

                        if (currentQuotes.ContainsKey(candle.Key))
                            currentQuotes[candle.Key] = quote;
                        else
                            currentQuotes.Add(candle.Key, quote);
                    }

                    // уточнить результат по открытым позициям
                    for (var i = 0; i < dueDeals.Count; i++)
                    {
                        if (worker.CancellationPending) return stat;
                        if (dueDeals[i].TimeExit.HasValue)
                            if (dueDeals[i].TimeExit.Value <= time)
                            {
                                dueDeals.RemoveAt(i);
                                i--;
                                continue;
                            }
                        var deal = dueDeals[i];
                        if (deal.TimeEnter <= time)
                        {
                            // посчитать текущий результат сделки
                            // в контрвалюте
                            if (!currentQuotes.ContainsKey(deal.Symbol)) continue;
                            var dealTickerQuote = currentQuotes[deal.Symbol];
                            var dealTickerPrice = deal.Side > 0 ? dealTickerQuote.bid : dealTickerQuote.ask;
                            var dealResult = deal.Volume * deal.Side * (dealTickerPrice - deal.PriceEnter);

                            // перевод прибыли в валюту депо
                            bool inverse, areSame;
                            var dealTransferSymbol = DalSpot.Instance.FindSymbol(deal.Symbol,
                                                                                 false, curxDepo, out inverse,
                                                                                 out areSame);
                            float? baseDepoRate = null;
                            if (areSame) baseDepoRate = 1f;
                            else
                            {
                                if (!string.IsNullOrEmpty(dealTransferSymbol))
                                    if (currentQuotes.ContainsKey(dealTransferSymbol))
                                    {
                                        var quote = currentQuotes[dealTransferSymbol];
                                        if (quote.time >= time)
                                            baseDepoRate = !inverse
                                                               ? (deal.Side > 0 ? quote.bid : quote.ask)
                                                               : (deal.Side > 0 ? 1 / quote.ask : 1 / quote.bid);
                                    }
                            }
                            if (!baseDepoRate.HasValue) continue;
                            dealResult *= baseDepoRate.Value;
                            equity += (decimal)dealResult;
                        }
                    } // for (deal ...
                    // сохранить отметку - время/доходность
                    stat.listEquity.Add(new EquityOnTime((float)equity, time));
                } // for (time ... += Timeframe
            }
            finally
            {
                cursor.Close();
            }
            // если история начинается с пустого депо - изъять эти строки из истории
            var firstNotEmpty = stat.listEquity.FindIndex(e => e.equity > 0);
            if (firstNotEmpty == stat.listEquity.Count - 1) stat.listEquity.Clear();
            else
                if (firstNotEmpty > 0) stat.listEquity.RemoveRange(0, firstNotEmpty);

            SetProgressValueSafe(70, 
                Localizer.GetString("MessageBuildingEquityChart") + "...");
            stat.Calculate(balanceChanges, openOrders, startDate);
            return stat;
        }

        private Dictionary<string, DateTime> UpdateQuotesForStats(List<MarketOrder> orders,
            List<MarketOrder> openOrders, string curxDepo)
        {
            var quoteTime = new Dictionary<string, DateTime>();
            var allOrders = orders.Union(openOrders);
            foreach (var order in allOrders)
            {
                // сама валюта сделки
                if (quoteTime.ContainsKey(order.Symbol))
                {
                    if (quoteTime[order.Symbol] > order.TimeEnter)
                        quoteTime[order.Symbol] = order.TimeEnter;
                }
                else quoteTime.Add(order.Symbol, order.TimeEnter);
                // валюта пересчета
                var orderSymbol = order.Symbol;
                bool inverse, areSame;
                var counterSymbol = DalSpot.Instance.FindSymbol(orderSymbol, false, curxDepo, out inverse,
                                                                out areSame);
                if (string.IsNullOrEmpty(counterSymbol)) continue;
                if (quoteTime.ContainsKey(counterSymbol))
                {
                    if (quoteTime[counterSymbol] > order.TimeEnter)
                        quoteTime[counterSymbol] = order.TimeEnter;
                }
                else quoteTime.Add(counterSymbol, order.TimeEnter);
            }

            var minutesOfGap = Timeframe / 10;
            if (minutesOfGap < 5) minutesOfGap = 5;

            if (cbDefaultUploadQuotes.Checked ||
                MessageBox.Show(Localizer.GetString("TitleLoadQuotes") + "?", 
                Localizer.GetString("TitleQuestion"), 
                MessageBoxButtons.YesNo) == DialogResult.Yes)
                MainForm.Instance.UpdateTickersCacheForRobots(quoteTime,  minutesOfGap);
            return quoteTime;
        }

        private void CbStartFromCheckedChanged(object sender, EventArgs e)
        {
            dpStart.Enabled = cbStartFrom.Checked;
        }

        private void CbFilterByMagicCheckedChanged(object sender, EventArgs e)
        {
            tbMagic.Enabled = cbFilterByMagic.Checked;
        }

        private void AccountTradeResultsFormLoad(object sender, EventArgs e)
        {
            // загрузить сохраненные настройки
            cbShowBalanceCurve.Checked = UserSettings.Instance.AccountResultsShowEquityCurve;
            cbDefaultUploadQuotes.Checked = UserSettings.Instance.AccountResultsUpdateQuotes;

            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });

            if (InstantCalculation)
                BtnUpdateClick(sender, e);
            else
            {
                // показать ту стату, что имеется
                var accId = AccountStatus.Instance.accountID;
                if (statByAccount.TryGetValue(accId, out statistics))
                    UpdateTablesAndCharts(statistics);
            }
        }
    
        private void BtnShowOptionsClick(object sender, EventArgs e)
        {
            panelOptions.Visible = !panelOptions.Visible;
            btnShowOptions.Text = panelOptions.Visible ? Localizer.GetString("TitleHideOptions") : Localizer.GetString("TitleShowOptions");
        }

        private void BtnMakeReportHtmlClick(object sender, EventArgs e)
        {
            if (statistics == null) return;
            var account = AccountStatus.Instance.AccountData;
            if (account == null) return;
            
            List<MarketOrder> histOrders;
            var status = TradeSharpAccount.Instance.proxy.GetHistoryOrders(AccountStatus.Instance.AccountData.ID,
                                                                            cbStartFrom.Checked ? dpStart.Value : (DateTime?)null, out histOrders);
            if (status != RequestStatus.OK || histOrders == null)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageUnableToGetOrderHistory") + ": {0}",
                                              EnumFriendlyName<RequestStatus>.GetString(status)));
                return;
            }

            List<MarketOrder> openOrders;
            status = TradeSharpAccount.Instance.proxy.GetMarketOrders(AccountStatus.Instance.AccountData.ID,
                                                                       out openOrders);
            if (status != RequestStatus.OK || openOrders == null)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageUnableToGetOpenPosHistory") + ": {0}",
                                              EnumFriendlyName<RequestStatus>.GetString(status)));
                return;
            }

            var render = new ReportRenderClassic();
            var html = render.RenderReport(account, statistics, openOrders, histOrders, true);
            var fileName = string.Format("счет_{0}_на_{1:dd_mm_yyyy_HH_mm}.html",
                account.ID, DateTime.Now);

            var dlg = new SaveFileDialog
                          {
                              Title = Localizer.GetString("TitleSaveReport"),
                              DefaultExt = "html",
                              Filter = Localizer.GetString("FilterHtml"),
                              FilterIndex = 0,
                              FileName = fileName
                          };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using (var sw = new StreamWriter(dlg.FileName))
            {
                try
                {
                    sw.Write(html);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localizer.GetString("MessageErrorSavingFile"), 
                        Localizer.GetString("TitleError"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Logger.ErrorFormat("Ошибка сохранения файла \"{0}\": {1}", dlg.FileName, ex);
                }
            }

            // запустить браузер
            try
            {
                System.Diagnostics.Process.Start(dlg.FileName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// точка графика
        /// </summary>
        class BalanceByDate
        {
            [DisplayName("дата")]
            public DateTime X { get; set; }

            [DisplayName("баланс")]
            public double Y { get; set; }

            public BalanceByDate(DateTime x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private void AccountTradeResultsFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        private void TabPageSetupMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void AccountTradeResultsFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }        
    }    
}
