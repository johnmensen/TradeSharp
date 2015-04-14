using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ClosePositionsForm : Form
    {
        private readonly List<int> dealIds;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public ClosePositionsForm()
        {
            InitializeComponent();
        }

        public ClosePositionsForm(List<int> dealIds) : this()
        {
            this.dealIds = dealIds.OrderBy(d => d).ToList();
            tbDeals.Text = string.Join(Environment.NewLine, this.dealIds.Select(d => "сделка #" + d));
            
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Close();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var accountId = AccountStatus.Instance.accountID;
            if (accountId == 0) return;

            var index = 0;
            foreach (var dealId in dealIds)
            {
                if (worker.CancellationPending) break;
                var status = MainForm.Instance.SendCloseRequestSafe(accountId, dealId, PositionExitReason.ClosedFromUI);
                if (worker.CancellationPending) break;
                // обновить строку
                Invoke(new Action<int, RequestStatus>(UpdateDealLineStatusUnsafe), index++, status);
                // обновить прогресс
                var percent = 100 * index / dealIds.Count;
                Invoke(new Action<int>(i => progressBar.Value = i), percent);
                if (worker.CancellationPending) break;
                // пауза (чтобы не зафладить сервер)
                Thread.Sleep(200);
                if (worker.CancellationPending) break;
            }
        }

        private void UpdateDealLineStatusUnsafe(int index, RequestStatus status)
        {
            var lines = tbDeals.Lines;
            if (lines.Length <= index) return;
            lines[index] = lines[index] + ": " + EnumFriendlyName<RequestStatus>.GetString(status);
            tbDeals.Text = string.Join(Environment.NewLine, lines);
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            worker.CancelAsync();            
        }
    }
}
