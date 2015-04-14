using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteAdmin.BL;

namespace TradeSharp.QuoteAdmin.Forms
{
    public partial class FillHistoryForm : Form
    {
        private readonly BackgroundWorker workerForexite = new BackgroundWorker();

        enum QuoteSource
        {
            Forexite = 0, MetaTrader4 = 1
        }

        public FillHistoryForm()
        {
            InitializeComponent();
        }

        public FillHistoryForm(List<TradeTicker> tickers)
        {
            InitializeComponent();

            dpStart.Value = DateTime.Now.AddDays(-10);
            
            var text = string.Join(" ", tickers.Select(t => t.Title));
            tbTickers.Text = text;

            foreach (var val in Enum.GetValues(typeof(QuoteSource)))
            {
                cbQuoteSource.Items.Add(val);
            }
            cbQuoteSource.SelectedIndex = 0;

            workerForexite.DoWork += WorkerForexiteDoWork;
            workerForexite.RunWorkerCompleted += WorkerForexiteRunWorkerCompleted;
            workerForexite.ProgressChanged += WorkerForexiteProgressChanged;
            workerForexite.WorkerReportsProgress = true;
            workerForexite.WorkerSupportsCancellation = true;
        }

        private void BtnFillClick(object sender, EventArgs e)
        {
            if (workerForexite.IsBusy)
            {
                workerForexite.CancelAsync();
                return;
            }
            
            var names = tbTickers.Text.Split(new [] {' ', (char) 9, ',', ';'}, StringSplitOptions.RemoveEmptyEntries);
            var allTickers = TickerInfo.GetTickers();
            var tickers = allTickers.Where(t => names.Contains(t.Title)).ToList();
            if (tickers.Count == 0) return;
            FillHistoryForexiteAsync(tickers);
            btnFill.Text = "Остановить";
        }

        private void FillHistoryForexiteAsync(List<TickerInfo> tickers)
        {
            var ptrs = new UploadHistoryParams(dpStart.Value, dpEnd.Value, tickers, true);
            workerForexite.RunWorkerAsync(ptrs);
        }

        private void WorkerForexiteDoWork(object sender, DoWorkEventArgs e)
        {
            var ptrs = (UploadHistoryParams) e.Argument;
            var startDay = ptrs.start.Date;
            var endDay = (ptrs.end.Hour == 0 && ptrs.end.Minute == 0) ? ptrs.end.Date : ptrs.end.Date.AddDays(1);
            var daysTotal = (endDay - startDay).TotalDays;

            AddStatusMessage(string.Format("Получение {0} котировок с Forexite, {1} дней всего", 
                ptrs.tickers.Count,
                (int)Math.Round(daysTotal)));

            var tickerNames = ptrs.tickers.Select(t => t.Title).ToList();
            
            // таки запросить историю
            // по каждому дню
            int lastProgress = 0, dayNum = 0;
            const int deltaProgressMin = 3;
            int quotesCount = 0, daysLoaded = 0;

            var storeThread = new DBStoreThread(ptrs.rewriteQuotes);
            var readTickers = new List<int>();

            for (var day = startDay; day <= endDay; day = day.AddDays(1))
            {
                if (workerForexite.CancellationPending) break;
                var minuteCandles = ForexiteDownloader.ReadDayQuotesFromForexite(day, 0, tickerNames);
                if (workerForexite.CancellationPending) break;

                if (minuteCandles.Count > 0)
                {
                    if (quotesCount < minuteCandles.Count)
                    {
                        quotesCount = minuteCandles.Count;
                        readTickers = readTickers.Union(minuteCandles.Keys).Distinct().ToList();
                    }
                    daysLoaded++;
                    // записать котировки в БД
                    storeThread.PushQuotes(minuteCandles);
                }

                // обновить прогресс
                dayNum++;
                var progress = (int)(100 * dayNum / daysTotal);
                if (progress > 100) progress = 100;
                if ((progress - lastProgress) > deltaProgressMin)
                {
                    workerForexite.ReportProgress(progress);
                    lastProgress = progress;
                }
            }

            AddStatusMessage("Завершение - запись в базу данных");
            workerForexite.ReportProgress(0);
            
            var recordsTotal = storeThread.CandlesLeftInQueue;
            long oldProgress = 0;

            while (true)
            {
                if (workerForexite.CancellationPending) break;
                Thread.Sleep(250);
                var recsLeft = storeThread.CandlesLeftInQueue;
                if (recsLeft == 0) break;
                if (recsLeft > recordsTotal) recordsTotal = recsLeft;

                var progress = (recordsTotal - recsLeft) * 100 /recordsTotal;
                if (progress > 100) progress = 100;
                else if (progress < 0) progress = 0;
                if (oldProgress != progress)
                {
                    workerForexite.ReportProgress((int)progress);
                    oldProgress = progress;
                }
            }

            storeThread.Stop();
            var msgResult = string.Format("Загружено {0} котировок за {1} дней", quotesCount, daysLoaded);

            if (readTickers.Count < ptrs.tickers.Count)
            {
                var symbolsRead = readTickers.Select(t => DalSpot.Instance.GetSymbolByFXICode(t)).ToList();
                var symbolsLacked = tickerNames.Where(n => !symbolsRead.Contains(n));

                // получить котировки, которые не были прочитаны, и сформировать из них список
                var strLack = string.Join(", ", symbolsLacked) + " не загружены";
                msgResult = msgResult + Environment.NewLine + strLack;
            }
            e.Result = msgResult;
        }

        private void WorkerForexiteRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AddStatusMessage(e.Result.ToString());
            btnFill.Text = "Заполнить";
            progressBar.Value = 0;
        }

        private void WorkerForexiteProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void AddStatusMessage(string msg)
        {
            Invoke(new Action<string>(s => tbStatus.AppendText(s + Environment.NewLine)), msg);
        }
    }

    class UploadHistoryParams
    {
        public DateTime start, end;

        public List<TickerInfo> tickers;

        public bool rewriteQuotes;

        public UploadHistoryParams(DateTime start, DateTime end, List<TickerInfo> tickers, bool rewriteQuotes)
        {
            this.start = start;
            this.end = end;
            this.tickers = tickers;
            this.rewriteQuotes = rewriteQuotes;
        }
    }
}
