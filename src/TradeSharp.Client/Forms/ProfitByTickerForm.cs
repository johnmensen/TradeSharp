using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Entity;
using FastMultiChart;
using TradeSharp.Client.BL;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ProfitByTickerForm : Form
    {
        public class BalanceAndEquitySeriesData
        {
            public List<TimeBalans> lstBalance = new List<TimeBalans>();
            public List<TimeBalans> lstEquity = new List<TimeBalans>();

            public void ShiftLastDate()
            {
                foreach (var list in new[] {lstBalance, lstEquity})
                {
                    if (list.Count > 1)
                        list[list.Count - 1].Time = list[list.Count - 1].Time.Date.AddDays(1);
                }
            }
        }

        private readonly string selectedTicker;

        private readonly BackgroundWorker orderCalcWorker = new BackgroundWorker();

        private volatile bool formIsLoaded;

        public ProfitByTickerForm()
        {
            InitializeComponent();

            // настроить график
            SetupChart();
            
            // заполнить список тикеров из числа торгуемых
            if (!AccountStatus.Instance.isAuthorized)
                MainForm.Instance.OpenLoginDialog();

            if (!AccountStatus.Instance.isAuthorized)
                return;

            var tickers = TradeSharpAccount.Instance.proxy.GetTickersTraded(AccountStatus.Instance.accountID);
            if (tickers.Count > 0)
            {
                tickers.Sort();
                cbTicker.DataSource = tickers;
            }

            orderCalcWorker.WorkerSupportsCancellation = true;
            orderCalcWorker.DoWork += OrderCalcWorkerOnDoWork;
            orderCalcWorker.RunWorkerCompleted += OrderCalcWorkerOnRunWorkerCompleted;
        }

        private void OrderCalcWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs ea)
        {
            standByControl.IsShown = false;
            standByControl.Visible = false;
            if (ea.Result == null) return;
            var balanceByDate = (BalanceAndEquitySeriesData)ea.Result;
            if (balanceByDate.lstBalance.Count == 0) return;

            foreach (var pt in balanceByDate.lstBalance)
                chart.Graphs[0].Series[0].Add(pt);
            foreach (var pt in balanceByDate.lstEquity)
                chart.Graphs[0].Series[1].Add(pt);
            chart.Initialize();
        }

        public static void OrderCalcWorkerOnDoWork(object senderWorker, DoWorkEventArgs argsWithTicker)
        {
            var worker = (BackgroundWorker) senderWorker;

            var ticker = (string) argsWithTicker.Argument;
            var account = AccountStatus.Instance.AccountData;
            if (account == null) return;

            var orders = new List<MarketOrder>();
            try
            {
                // получить закрытые ордера порциями
                while (true)
                {
                    if (worker.CancellationPending) return;
                    var startId = orders.Count == 0 ? 0 : orders.Max(o => o.ID);
                    var ordersPart = TradeSharpAccount.Instance.proxy.GetClosedOrders(AccountStatus.Instance.accountID,
                        ticker, startId, 200) ?? new List<MarketOrder>();
                    if (ordersPart.Count == 0) break;
                    orders.AddRange(ordersPart);
                }

                // получить открытые ордера
                if (worker.CancellationPending) return;
                List<MarketOrder> openedOrders;
                TradeSharpAccount.Instance.proxy.GetMarketOrders(AccountStatus.Instance.accountID, out openedOrders);
                if (openedOrders != null && openedOrders.Count > 0)
                {
                    openedOrders = openedOrders.Where(o => o.Symbol == ticker).ToList();
                    if (openedOrders.Count > 0)
                    {
                        CalculateOpenedOrdersResult(openedOrders, account.Currency);
                        orders.AddRange(openedOrders);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения ордеров", ex);
                MessageBox.Show(Localizer.GetString("MessageErrorGettingOrders"), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (orders.Count == 0) return;

            var balanceOnDate = new BalanceAndEquitySeriesData();
            var startDate = orders.Min(o => o.TimeEnter).Date;
            var endDate = DateTime.Now;
            orders.ForEach(o =>
                {
                    o.TimeExit = o.TimeExit ?? endDate;
                    o.State = PositionState.Opened;
                });
            orders = orders.Where(o => o.TimeExit.HasValue).OrderBy(o => o.TimeExit).ToList();

            var balance = 0f;
            // установить "курсор" котировок на начало наблюдаемого отрезка
            var cursor = new BacktestTickerCursor();
            var tickers = new List<string> {ticker};
            // нужна еще валюта пересчета?
            bool inverse, pairsEqual;
            var tickerCounterDepo = DalSpot.Instance.FindSymbol(ticker, false, account.Currency,
                out inverse, out pairsEqual);
            if (!string.IsNullOrEmpty(tickerCounterDepo) && tickerCounterDepo != ticker)
                tickers.Add(tickerCounterDepo);
            cursor.SetupCursor(TerminalEnvironment.QuoteCacheFolder, tickers, startDate);
            try
            {            
                for (var date = startDate; ; )
                {
                    if (date != endDate && DaysOff.Instance.IsDayOff(date))
                    {
                        date = date.AddDays(1);
                        continue;
                    }

                    for (var i = 0; i < orders.Count; i++)
                    {
                        if (orders[i].TimeExit > date) break;
                        balance += orders[i].ResultDepo;
                        orders.RemoveAt(i);
                        i--;
                    }

                    // получить текущий результат по открытым на момент ордерам
                    var equity = balance;
                    var curDate = date;
                    var curOrders = orders.Where(o => o.TimeEnter <= curDate).ToList();
                    if (curOrders.Count > 0)
                    {
                        cursor.MoveToTime(date);
                        var curProfit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(curOrders,
                            cursor.GetCurrentQuotes().ToDictionary(c => c.a,
                                c => new QuoteData(c.b.close, c.b.close, curDate)), account.Currency);
                        equity += curProfit;
                    }

                    balanceOnDate.lstBalance.Add(new TimeBalans(date, balance));
                    balanceOnDate.lstEquity.Add(new TimeBalans(date, equity));

                    if (date == endDate) break;
                    date = date.AddDays(1);
                    if (date > endDate) date = endDate;
                }
            }
            finally
            {
                cursor.Close();
            }
            // сместить последнюю дату кривой баланса
            balanceOnDate.ShiftLastDate();
            argsWithTicker.Result = balanceOnDate;
        }

        public ProfitByTickerForm(string selectedTicker) : this()
        {
            this.selectedTicker = selectedTicker;
        }

        private void cbTicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!formIsLoaded) return;
            if (orderCalcWorker.IsBusy)
            {
                orderCalcWorker.CancelAsync();
                for (var i = 0; i < 30; i++)
                {
                    if (!orderCalcWorker.IsBusy) break;
                    Thread.Sleep(1000);
                }
                
                if (orderCalcWorker.IsBusy)
                {
                    MessageBox.Show(Localizer.GetString("MessageOperationTimedOut"));
                    return;
                }
            }

            standByControl.IsShown = true;
            standByControl.Visible = true;
            orderCalcWorker.RunWorkerAsync(cbTicker.Text);
        }

        private void ProfitByTickerForm_Load(object sender, EventArgs e)
        {
            formIsLoaded = true;
            if (cbTicker.Items.Count > 0)
            {
                var selIndex = 0;
                if (!string.IsNullOrEmpty(selectedTicker))
                {
                    selIndex = cbTicker.Items.Cast<string>().IndexOf(selectedTicker);
                    if (selIndex < 0) selIndex = 0;
                }
                cbTicker.SelectedIndex = selIndex;
            }
        }
    
        private static void CalculateOpenedOrdersResult(List<MarketOrder> orders, string depoCurrency)
        {
            var quotes = Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData();
            DalSpot.Instance.CalculateOpenedPositionsCurrentResult(orders, quotes, depoCurrency);
        }

        private void SetupChart()
        {
            chart.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chart.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chart.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chart.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chart.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chart.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            chart.Graphs[0].Series.Add(new Series("Time", "Balans", new Pen(Color.Blue, 1f))
            {
                XMemberTitle = "дата",
                YMemberTitle = "баланс"
            });
            chart.Graphs[0].Series.Add(new Series("Time", "Balans", new Pen(Color.Green, 2f))
            {
                XMemberTitle = "дата",
                YMemberTitle = "средства"
            });
        }
    }
}
