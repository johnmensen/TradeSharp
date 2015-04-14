using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class DownloadQuotesForm : Form
    {
        public EventHandler LoadCompleted;

        public List<string> TickersToUpload
        {
            get { return tickersToUpload.ToList().Select(t => t.Key).ToList(); }
        }

        private readonly Dictionary<string, QuoteHistoryFillLineControl> quoteControls = 
            new Dictionary<string, QuoteHistoryFillLineControl>();

        private readonly Dictionary<string, DateTime> tickersToUpload;

        private const int QuoteLineHeight = 22;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public DownloadQuotesForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += WorkerOnDoWork;
            worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
        }

        public DownloadQuotesForm(Dictionary<string, DateTime> tickersToUpload, int minMinutesToUpdateCache) : this()
        {
            this.tickersToUpload = tickersToUpload;
        }

        private void DownloadQuotesFormLoad(object sender, EventArgs e)
        {
            // создать контролы для каждого тикера
            foreach (var ticker in tickersToUpload)
            {
                var ctrl = new QuoteHistoryFillLineControl(ticker.Key,
                    new Cortege2<DateTime, DateTime>(ticker.Value, DateTime.Now), imageList.Images[0],
                    (() =>
                    {
                        FileGapActualizator.currentTickerCancelled = true;
                    }))
                {
                    Height = QuoteLineHeight,
                    Dock = DockStyle.Top
                };

                quoteControls.Add(ticker.Key, ctrl);
            }

            foreach (var ctrl in quoteControls.Values.Reverse())
                panelTicker.Controls.Add(ctrl);

            // запустить background-процесс подкачки по каждому тикеру
            worker.RunWorkerAsync();
        }
        
        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if(LoadCompleted != null)
                LoadCompleted(this, new EventArgs());
            Close();
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            using (new TimeLogger("Обновление котировок"))
            {
                foreach (var ticker in tickersToUpload)
                {
                    // получить историю по каждому тикеру (набор гэпов)
                    VerifyTickerHistory(ticker.Key, ticker.Value);
                }

                // закачать историю по тикеру
                FileGapActualizator.currentTickerCancelled = false;
                foreach (var ticker in tickersToUpload)
                {
                    FillGapsByTicker(ticker.Key, ticker.Value);
                }
            }
        }

        private void VerifyTickerHistory(string ticker, DateTime timeStart)
        {
            var gaps = FileGapActualizator.VerifyTickerHistory(ticker, timeStart, 
                ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder, worker);
            if (worker.CancellationPending) 
                return;
            // обновить контрол
            UpdateControlSafe(ticker, gaps);
        }

        public void FillGapsByTicker(string ticker, DateTime startTime)
        {
            if (worker.CancellationPending) return;
            var ctrl = quoteControls[ticker];
            if (ctrl.Gaps == null) return;
            var gaps = ctrl.Gaps.ToList();
            if (gaps.Count == 0) return;

            UpdateCurrentTickerLabelSafe(ticker);

            FileGapActualizator.FillGapsByTickerFast(ticker, startTime, DateTime.Now, gaps,
                                                     ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder,
                                                     worker, UpdateControlSafe);
        }

        private void BtnBreakClick(object sender, EventArgs e)
        {
            worker.CancelAsync();
            Close();
        }

        private void UpdateControlSafe(string ticker, List<GapInfo> gaps)
        {
            try
            {
                Invoke(new Action<string, List<GapInfo>>(UpdateControlUnsafe), ticker, gaps);
            }
            catch (InvalidOperationException)
            {                
            }
        }

        private void UpdateControlUnsafe(string ticker, List<GapInfo> gaps)
        {
            var ctrl = quoteControls[ticker];
            ctrl.Gaps = gaps;
        }

        /// <summary>
        /// обновить метку - название загружаемого тикера
        /// заодно сделать активной кнопку отмены для того контрола, в котором производится загрузка
        /// </summary>
        private void UpdateCurrentTickerLabelSafe(string ticker)
        {
            try
            {
                Invoke(new Action<string>(s =>
                {
                    lblCurrentTicker.Text = Localizer.GetString("TitleQuoteSmall") + ": " + ticker;
                    foreach (var ctrl in quoteControls)
                    {
                        if (ctrl.Key != ticker)
                        {
                            ctrl.Value.CancelEnabled = false;
                            continue;
                        }
                        ctrl.Value.CancelEnabled = true;
                        //break;
                    }
                }), ticker);
            }
            catch
            { // окно может быть вообще закрыто
            }
        }
    }
}
