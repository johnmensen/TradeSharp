using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class AutoUploadHistoryTickersForm : Form
    {
        public List<TickerUploadRequest> tickersToUpload;
        
        public int minMinutesOfGap;

        private readonly BackgroundWorker worker;

        private IQuoteStorage quoteStorage;

        public AutoUploadHistoryTickersForm()
        {
            InitializeComponent();
            worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += WorkerDoWork;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.ProgressChanged += WorkerProgressChanged;
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgressSafe(e.ProgressPercentage);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// подкачать котировки за указанные в параметре tickersToUpload
        /// интервалы и обновить хранилище AtomCandleStorage
        /// </summary>        
        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var totalDays = tickersToUpload.Sum(t => (t.end - t.start).TotalDays);
            UpdateTextSafe(string.Format("Загрузка {0} котировок m1: {1:f1} дней всего",
                                         tickersToUpload.Count, totalDays), true);
            const int daysInRequest = 5, minDaysInRequest = 2;
            double daysLoaded = 0;

            foreach (var ticker in tickersToUpload)
            {                
                var dateLast = DateTime.Now;
                var pointCost = DalSpot.Instance.GetPrecision10(ticker.ticker);
                var candlesFromDb = new List<CandleData>();

                for (var dateStart = ticker.start; dateStart < ticker.end; )
                {
                    var dateEnd = dateStart.AddDays(daysInRequest);
                    if ((dateLast - dateEnd).TotalDays < minDaysInRequest) dateEnd = dateLast;

                    // макс. количество сбоев загрузки, после которых попытки прекращаются
                    var numFaultsLeft = 1;
                    
                    while (numFaultsLeft > 0)
                    {
                        if ((worker.CancellationPending))
                        {
                            // прервать попытки
                            e.Result = daysLoaded;
                            return;
                        }

                        // попытка подгрузить котировки
                        try
                        {
                            var packedQuotes = quoteStorage.GetMinuteCandlesPacked(ticker.ticker, dateStart, dateEnd);
                            if (packedQuotes != null && packedQuotes.count > 0)
                            {
                                var candlesUnpacked = packedQuotes.GetCandles().Select(c => new CandleData(c, pointCost));
                                candlesFromDb.AddRange(candlesUnpacked);
                            }
                            break; // выход из (numFaultsLeft > 0)
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Ошибка закачки котировок {0} c {1:dd.MM.yyyy}: {2}",
                                                ticker.ticker, dateStart, ex.Message);
                            // попытка неуспешна - еще одна?
                            numFaultsLeft--;
                            if (numFaultsLeft > 0) continue;
                            break;
                        }                        
                    }
                    
                    daysLoaded += (dateEnd - dateStart).TotalDays;
                    dateStart = dateEnd.AddMinutes(1);
                    worker.ReportProgress((int)(daysLoaded * 100 / totalDays));
                }

                // склеить котировки из БД с котировками в хранилище
                if (candlesFromDb.Count > 0)
                {
                    var cacheQuotes = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker.ticker);
                    var newCandles = candlesFromDb;
                    if (cacheQuotes != null && cacheQuotes.Count > 0)
                    {
                        if (ticker.endingShouldBeRewritten)
                            cacheQuotes = cacheQuotes.TakeWhile(c => c.timeOpen < newCandles[0].timeOpen).ToList();
                        CandleData.MergeCandles(ref newCandles, cacheQuotes, true);
                    }

                    // переписать котировки в хранилище
                    AtomCandleStorage.Instance.RewriteCandles(ticker.ticker, newCandles);
                    AtomCandleStorage.Instance.FlushInFile(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder, ticker.ticker);
                }
                
                UpdateTextSafe(string.Format("Загрузка котировок {0} завершена", ticker.ticker), true);
            }
            e.Result = daysLoaded;
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            if (!worker.IsBusy) return;
            worker.CancelAsync();
        }

        private void AutoUploadHistoryTickersFormLoad(object sender, EventArgs e)
        {
            try
            {
                quoteStorage = QuoteStorage.Instance.proxy;
            }
            catch (Exception)
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
                tbProgress.Text = "Невозможно установить связь с сервером котировок";
                return;
            }

            tbProgress.Clear();
            // в бэкграунде стартовать поток чтения
            worker.RunWorkerAsync();
        }

        #region Обновление текста и бара

        private delegate void UpdateTextDel(string msg, bool newLine);

        private void UpdateTextUnsafe(string msg, bool newLine)
        {
            tbProgress.Text += msg;
            if (newLine) tbProgress.Text += Environment.NewLine;
        }

        private void UpdateTextSafe(string msg, bool newLine)
        {
            if (!IsHandleCreated) return;
            BeginInvoke(new UpdateTextDel(UpdateTextUnsafe), msg, newLine);
        }

        private delegate void UpdateProgressDel(int progress);

        private void UpdateProgressUnsafe(int progress)
        {
            progressBar.Value = progress < progressBar.Maximum ? progress : progressBar.Maximum;
        }

        private void UpdateProgressSafe(int progress)
        {
            if (!IsHandleCreated) return;
            BeginInvoke(new UpdateProgressDel(UpdateProgressUnsafe), progress);
        }
        #endregion        

        private void AutoUploadHistoryTickersFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!worker.IsBusy) return;
            BtnStopClick(sender, e);
            e.Cancel = true;
        }
    }

    public class TickerUploadRequest
    {
        public string ticker;

        public DateTime start, end;

        public bool endingShouldBeRewritten;
    }
}
