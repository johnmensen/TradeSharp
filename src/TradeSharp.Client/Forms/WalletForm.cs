using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using FastGrid;
using TradeSharp.Client.BL.PaymentSystem;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using TradeSharp.Util.NotificationControl;

namespace TradeSharp.Client.Forms
{
    // ReSharper disable LocalizableElement
    // ReSharper disable RedundantArgumentDefaultValue
    public partial class WalletForm : Form, IMdiNonChartWindow
    {
        class SynchCompletedTask
        {
            public WaitCallback callback;

            public object callbackParam;
        }

        /// <summary>
        /// основная информация по кошельку - собственно кошелек, подписки,
        /// TOP(N) последних вводов / выводов
        /// </summary>
        class WalletExplicitResponse
        {
            public Wallet wallet;

            public List<Contract.Entity.Subscription> subscriptions;

            public List<Transfer> transfers;

            public List<AccountShared> realAccounts;

            public int transfersTotal;

            public TransfersByAccountSummary transfersSummary;

            public bool IsIncomplete
            {
                get { return wallet == null || realAccounts == null || realAccounts.Count == 0; }
            }
        }

        private volatile bool wasSynchrnoizedAtLeastOneTime;

        private readonly ThreadSafeList<SynchCompletedTask> tasksPerformedOnSynchIsCompleted = new ThreadSafeList<SynchCompletedTask>();

        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.WalletForm; }
        }

        public int WindowInnerTabPageIndex
        {
            get { return (int) Invoke(new Func<object>(() => tabControl.SelectedIndex)); }
            set
            {
                if (value < 0 || value >= tabControl.TabPages.Count)
                    return;
                tabControl.SelectedIndex = value;
            }
        }

        private WalletExplicitResponse walletExplicitDetail;

        private string walletCurrency = "";

        private readonly BackgroundWorker workerWallet = new BackgroundWorker();

        private readonly BackgroundWorker transfersWorker = new BackgroundWorker();

        public event Action<Form> FormMoved;

        public event Action<Form> ResizeEnded;

        private Account lastAccountDataProcessed;

        private volatile bool stopTransferDownload;

        private const string ColumnTagChart = "chart",
            ColumnTagDeposit = "deposit",
            ColumnTagWithdraw = "withdraw";

        public WalletForm()
        {
            InitializeComponent();

            SetupGrids();
            SetupPaySysButtons();
            Localizer.LocalizeControl(this);
            workerWallet.DoWork += LoadWalletAsynch;
            workerWallet.RunWorkerCompleted += WorkerWalletOnRunWorkerCompleted;
            transfersWorker.DoWork += TransferWorkerOnDoWork;
            transfersWorker.RunWorkerCompleted += TransferWorkerOnRunWorkerCompleted;
            SubscriptionModel.Instance.ModelIsLoaded += cats => InitiateAsynchLoad();
        }

        private void SetupPaymentGrid(FastGrid.FastGrid grid)
        {
            var blank = new Transfer();
            // платежи
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Id), "#") { SortOrder = FastColumnSort.Ascending, ColumnWidth = 60 });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.ValueDate), Localizer.GetString("TitleTime"))
            {
                ColumnMinWidth = 75,
                rowFormatter = valueObject =>
                {
                    var transfer = (Transfer)valueObject;
                    return transfer.Id == 0 ? "" : transfer.ValueDate.ToStringUniform();
                }
            });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.TargetAmount), Localizer.GetString("TitleSum"))
            {
                ColumnMinWidth = 75,
                rowFormatter = valueObject =>
                {
                    var transfer = (Transfer) valueObject;
                    return transfer.Id == 0 ? "" :
                        transfer.TargetAmount.ToStringUniformMoneyFormat() + " " + walletCurrency;
                }
            });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Comment), Localizer.GetString("TitleComment"))
            {
                ColumnMinWidth = 75,
                rowFormatter = valueObject =>
                {
                    var transfer = (Transfer) valueObject;
                    if (transfer.Subscription.HasValue)
                    {
                        return Localizer.GetString("TitleServiceSubscription");
                    }
                    if (transfer.RefWallet.HasValue)
                    {
                        return Localizer.GetString("TitleWalletPayment") + " #" + transfer.RefWallet.Value;
                    }
                    if (transfer.BalanceChange.HasValue)
                    {
                        return Localizer.GetString("TitleTransferToTradeAccount");
                    }
                    return transfer.Comment;
                },
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.Blue,
                HyperlinkFontActive = new Font(Font, FontStyle.Bold)
            });
            grid.colorFormatter = (object value, out Color? color, out Color? fontColor) =>
            {
                color = null;
                fontColor = null;
                var trans = value as Transfer;
                if (trans == null)
                    return;
                if (trans.Id == 0)
                    color = Color.Lime;
            };
            grid.UserHitCell += GridPaymentOnUserHitCell;
            grid.CalcSetTableMinWidth();
        }

        private void SetupGrids()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            var dataSpeciman = new AccountShared(new Account(), true);

            // счета
            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.AccountId), "#")
            {
                SortOrder = FastColumnSort.Ascending,
                ColumnWidth = 60
            });
            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.IsOwnAccount), Localizer.GetString("TitleOwner"))
            {
                ColumnMinWidth = 60,
                formatter = value => (bool) value ? Localizer.GetString("TitleYes") : ""
            });
            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.SharePercent), Localizer.GetString("TitlePercent"))
            {
                ColumnMinWidth = 75,
                formatter = value =>
                    {
                        var percent = ((decimal) value);
                        return percent == 100 ? "100 %" : percent.ToString("f5") + "%";
                    }
            });
            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.ShareMoneyWallet), Localizer.GetString("TitleSum"))
            {
                ColumnMinWidth = 75,
                formatter = value => ((decimal) value).ToStringUniformMoneyFormat()
            });

            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.IsOwnAccount), Localizer.GetString("TitleChart"))
            {
                Tag = ColumnTagChart,
                ColumnMinWidth = 60,
                ImageList = imageListGridChart,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
            });

            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.IsOwnAccount), Localizer.GetString("TitleDeposit"))
            {
                Tag = ColumnTagDeposit,
                SortOrder = FastColumnSort.Ascending,
                ColumnWidth = 70,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.Blue,
                HyperlinkFontActive = fontBold,
                formatter = valueObject => Localizer.GetString("TitleDeposit")
            });
            gridAccount.Columns.Add(new FastColumn(dataSpeciman.Property(s => s.IsOwnAccount), Localizer.GetString("TitleWithdraw"))
            {
                Tag = ColumnTagWithdraw,
                SortOrder = FastColumnSort.Ascending,
                ColumnWidth = 63,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.Blue,
                HyperlinkFontActive = fontBold,
                formatter = valueObject => Localizer.GetString("TitleWithdraw")
            });
            gridAccount.UserHitCell += GridAccountOnUserHitCell;
            gridAccount.CheckSize();

            // подписки
            var blank = new Contract.Entity.Subscription();
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.PaidService), Localizer.GetString("TitleService"))
            {
                Tag = PaidServiceType.Signals,
                SortOrder = FastColumnSort.Ascending,
                ColumnMinWidth = 65,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.Blue,
                HyperlinkFontActive = fontBold,
                formatter = valueObject =>
                {
                    var srv = (PaidService) valueObject;
                    return EnumFriendlyName<PaidServiceType>.GetString(srv.ServiceType) +
                           (string.IsNullOrEmpty(srv.Comment) ? "" : " (" + srv.Comment + ")");
                }
            });
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.TimeStarted), Localizer.GetString("TitleBeginning")) { ColumnMinWidth = 75 });
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.TimeEnd), Localizer.GetString("TitleEnd")) { ColumnMinWidth = 75 });
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.PaidService), Localizer.GetString("TitleCost"))
            {
                ColumnMinWidth = 70,
                formatter = valueObject =>
                {
                    var srv = (PaidService) valueObject;
                    if (srv.ServiceType == PaidServiceType.Signals)
                        return srv.FixedPrice == 0
                                   ? "-"
                                   : srv.FixedPrice.ToStringUniformMoneyFormat(true) + " / " +
                                     PaidService.GetMonthFeeFromDailyFee(srv.FixedPrice) + " " +
                                     srv.Currency;
                    if (srv.ServiceType == PaidServiceType.PAMM)
                    {
                        if (srv.serviceRates.Count == 0) return "-";
                        if (srv.serviceRates.Count == 1)
                            return srv.serviceRates[0].Amount.ToStringUniformMoneyFormat(true) + "%";
                        return
                            srv.serviceRates[srv.serviceRates.IndexOfMin(r =>
                                r.Amount)].Amount.ToStringUniformMoneyFormat(true) + "% ... " +
                            srv.serviceRates[srv.serviceRates.IndexOfMin(r =>
                                -r.Amount)].Amount.ToStringUniformMoneyFormat(true) + "%";
                    }
                    return "";
                }
            });
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.RenewAuto), Localizer.GetString("TitleProlong"))
            {
                ColumnWidth = 61,
                formatter = v => (bool)v ? Localizer.GetString("TitleProlong") : Localizer.GetString("TitleNo"),
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                HyperlinkFontActive = fontBold,
                Tag = true
            });
            gridSubscription.Columns.Add(new FastColumn(blank.Property(p => p.RenewAuto), Localizer.GetString("TitleTurnOff"))
            {
                ColumnWidth = 57,
                ImageList = imageListGrid,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand
            });
            gridSubscription.UserHitCell += GridSubscriptionOnUserHitCell;
            gridSubscription.CheckSize();

            summaryTransfersFastGrid.Columns.Add(new FastColumn("a", " "));
            summaryTransfersFastGrid.Columns.Add(new FastColumn("b", Localizer.GetString("TitleTransactions")));
            summaryTransfersFastGrid.Columns.Add(new FastColumn("c", Localizer.GetString("TitleSum")));
        }

        private void GridPaymentOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var gridPayment = sender as FastGrid.FastGrid;
            if (gridPayment == null)
                return;
            if (e.Button != MouseButtons.Left)
                return;
            var transfer = (Transfer)gridPayment.rows[rowIndex].ValueObject;
            // показать подсказку по платежу - перевод на счет, платеж за услугу...
            // if (col.PropertyName == "Comment")
            if (col.PropertyName == transfer.Property(t => t.Comment))
            {
                // подписка?
                if (transfer.Subscription.HasValue)
                {
                    new ServiceDetailForm(transfer.Subscription.Value).ShowDialog();
                    return;
                }
                // перевод на торг. счет?
                // платеж в пользу кошелька?
                if (transfer.BalanceChange.HasValue || transfer.RefWallet.HasValue)
                {
                    BalanceChange bc = null;
                    PlatformUser us = null;
                    try
                    {
                        TradeSharpWalletManager.Instance.proxy.GetTransferExtendedInfo(
                            CurrentProtectedContext.Instance.MakeProtectedContext(), transfer.Id, out bc, out us);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("GetTransferExtendedInfo()", ex);
                    }
                    if (bc == null && us == null) return;
                    if (UserSettings.Instance.GetAccountEventAction(AccountEventCode.WalletModified) == AccountEventAction.DoNothing) return;

                    var text = bc != null
                        ? string.Format("{0} счета {1}, {2} {3}",
                            BalanceChange.GetSign(bc.ChangeType) > 0 ? "Пополнение" : "Списание со",
                            bc.AccountID,
                            bc.AmountDepo.ToStringUniformMoneyFormat(),
                            walletCurrency/*bc.Currency*/)
                        : string.Format("Платеж на кошелек пользователя, {0}", us.MakeFullName());

                    bool repeatNotification;
                    NotificationBox.Show(text, "Операция выполнена", out repeatNotification);

                    if (!repeatNotification)
                    {
                        UserSettings.Instance.SwitchAccountEventAction(AccountEventCode.WalletModified);
                        UserSettings.Instance.SaveSettings();
                    }
                }
            }
        }

        private void GridAccountOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (walletExplicitDetail == null || walletExplicitDetail.IsIncomplete) return;
            if (e.Button != MouseButtons.Left) return;
            
            // окно пополнения - снятия
            var accountShared = (AccountShared)gridAccount.rows[rowIndex].ValueObject;
            if (col.Tag != null && col.Tag is string)
            if (col.Tag == ColumnTagDeposit || col.Tag == ColumnTagWithdraw)
            {
                var deposit = col.Tag == ColumnTagDeposit;
                var dlg = new WalletWithdrawForm(walletExplicitDetail.wallet,
                    gridAccount.GetRowValues<AccountShared>(false).Select(a => a.Account).ToList(),
                    accountShared.Account, deposit);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (dlg.TargetWithdrawal > 0)
                        WithdrawOrDepositFromAccount(accountShared.Account, dlg.TargetWithdrawal, true);
                    if (dlg.TargetDespoit > 0)
                        WithdrawOrDepositFromAccount(accountShared.Account, dlg.TargetDespoit, false);
                }
            }

            if (col.Tag != null && col.Tag is string)
            if (col.Tag == ColumnTagChart)
            {
                new AccountShareHistoryForm(accountShared).ShowDialog();
            }
        }

        private void GridSubscriptionOnUserHitCell(object sender, MouseEventArgs ea, int rowIndex, FastColumn col)
        {
            var data = (Contract.Entity.Subscription) gridSubscription.rows[rowIndex].ValueObject;

            // показать детализацию услуги
            if (ea.Button == MouseButtons.Left && col.PropertyName == data.Property(p => p.PaidService) && col.Tag != null)
            {
                new ServiceDetailForm(data.Service).ShowDialog();
                return;
            }

            // продлять / не продлять
            if (ea.Button == MouseButtons.Left && col.PropertyName == data.Property(p => p.RenewAuto) && col.Tag != null)
            {
                // показать предупреждение
                var confirm = data.RenewAuto
                                  ? "Отключить автоматическое продление подписки?\n" +
                                    DateTime.Now.Date.AddDays(1).ToString("dd MMM") +
                                    " в 00:00 подписка будет отключена. Продолжить?"
                                  : "Включить автоматическое продление подписки?\n" +
                                    DateTime.Now.Date.AddDays(1).ToString("dd MMM") +
                                    " в 00:00 подписка будет возобновлена. Продолжить?";
                if (MessageBox.Show(confirm, "Подтвердите действие", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No)
                    return;

                // таки поменять флаг
                data.RenewAuto = !data.RenewAuto;
                UpdateOrRemoveSubscription(data, false);
                return;
            }

            // отписаться
            if (ea.Button == MouseButtons.Left && col.PropertyName == data.Property(p => p.PaidService))
            {
                UpdateOrRemoveSubscription(data, true);
                return;
            }
        }

        private void UpdateOrRemoveSubscription(Contract.Entity.Subscription sub, bool remove)
        {
            var isOk = false;
            var error = WalletError.CommonError;
            try
            {
                isOk = MainForm.serverProxyTrade.proxy.SubscribeOnService(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountStatus.Instance.Login,
                    sub.Service, sub.RenewAuto, remove, sub.AutoTradeSettings, out error);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GridSubscriptionOnUserHitCell - ошибка в GridSubscriptionOnUserHitCell({0}): {1}",
                    sub.Service, ex);
            }
            if (!isOk)
                MessageBox.Show((remove ? "Ошибка при отключении подписки: " : "Ошибка обновления подписки: ")
                    + EnumFriendlyName<WalletError>.GetString(error), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                // обновить состояние подписки и кошелька
                SubscriptionModel.Instance.LoadSubscribedCategories();
                InitiateAsynchLoad();
            }
        }

        private void WalletFormLoad(object sender, EventArgs e)
        {
            try
            {
                if (!AccountStatus.Instance.isAuthorized ||
                string.IsNullOrEmpty(AccountStatus.Instance.Login))
                    return;

                InitiateAsynchLoad();

                // запомнить окошко
                MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
                {
                    Window = WindowCode,
                    WindowPos = Location,
                    WindowSize = Size,
                    WindowState = WindowState.ToString()
                });
            }
            finally
            {
                timerUpdate.Enabled = true;
            }
        }

        private void InitiateAsynchLoad()
        {
            if (workerWallet.IsBusy)
                return;
            ShowHideProgress(true);
            if (!workerWallet.IsBusy)
                workerWallet.RunWorkerAsync();
        }

        private void LoadWalletAsynch(object sender, DoWorkEventArgs ea)
        {
            var details = new WalletExplicitResponse();

            // запросить средства в кошельке, средства на счете
            var error = WalletError.CommonError;

            try
            {
                details.wallet = TradeSharpWalletManager.Instance.proxy.GetUserWalletSubscriptionAndLastPayments(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountStatus.Instance.Login,
                    0,
                    out details.transfersTotal,
                    out details.subscriptions,
                    out details.transfers,
                    out error);
                /*details.wallet = TradeSharpWalletManager.Instance.proxy.GetUserWalletSubscriptionAndLastPayments(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountStatus.Instance.Login,
                    details.transfersTotal,
                    out details.transfersTotal,
                    out details.subscriptions,
                    out details.transfers,
                    out error);*/
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetUserWalletSubscriptionAndLastPayments({0}) error: {1}",
                    AccountStatus.Instance.Login, ex);
            }
            if (error != WalletError.OK)
            {
                Logger.ErrorFormat("GetUserWalletSubscriptionAndLastPayments({0}) error: {1}",
                    AccountStatus.Instance.Login, EnumFriendlyName<WalletError>.GetString(error));
            }
            if (details.wallet == null)
                return;

            lastAccountDataProcessed = AccountStatus.Instance.AccountData;

            // запросить актуальные данные по реальным счетам пользователя
            try
            {
                TradeSharpAccount.Instance.proxy.GetUserOwnAndSharedAccounts(
                    AccountStatus.Instance.Login, CurrentProtectedContext.Instance.MakeProtectedContext(),
                    out details.realAccounts);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetUserOwnAndSharedAccounts({0}) error: {1}", AccountStatus.Instance.Login, ex);
            }

            try
            {
                details.transfersSummary = TradeSharpAccount.Instance.proxy.GetTransfersSummary(
                    CurrentProtectedContext.Instance.MakeProtectedContext(), AccountStatus.Instance.Login);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetTransfersSummary({0}) error: {1}", AccountStatus.Instance.Login, ex);
            }

            ea.Result = details;
        }

        private void WorkerWalletOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs ea)
        {
            ShowHideProgress(false);

            if (ea.Result == null)
                return;

            walletExplicitDetail = (WalletExplicitResponse) ea.Result;

            // байндинг
            walletCurrency = walletExplicitDetail.wallet.Currency;
            lblWallet.Text = walletExplicitDetail.wallet.Balance.ToStringUniformMoneyFormat() + " " + walletExplicitDetail.wallet.Currency;

            var summaryData = new List<Cortege3<string, int, decimal>>();
            foreach (var transfersByType in walletExplicitDetail.transfersSummary.TransfersByType)
            {
                var row = new Cortege3<string, int, decimal>
                    {
                        a = EnumFriendlyName<TransfersByAccountSummary.AccountTransferType>.GetString(transfersByType.Key),
                        b = transfersByType.Value.a,
                        c = transfersByType.Value.b
                    };
                summaryData.Add(row);
            }
            summaryTransfersFastGrid.DataBind(summaryData);

            gridSubscription.DataBind(walletExplicitDetail.subscriptions ?? new List<Contract.Entity.Subscription>());

            gridAccount.DataBind(walletExplicitDetail.realAccounts ?? new List<AccountShared>());

            if (!transfersWorker.IsBusy)
                transfersWorker.RunWorkerAsync();
        }

        private void TransferWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var result = new List<Transfer>();
            stopTransferDownload = false;
            try
            {
                var lastTransferId = 0;
                while (!stopTransferDownload)
                {
                    var transfers = TradeSharpAccount.Instance.proxy.GetAccountTransfersPartByPart(
                        CurrentProtectedContext.Instance.MakeProtectedContext(), AccountStatus.Instance.Login, lastTransferId, 500);
                    if (transfers == null || transfers.Count == 0)
                        break;
                    result.AddRange(transfers);
                    lastTransferId = transfers.Last().Id;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetAccountTransfersPartByPart({0}) error: {1}",
                    AccountStatus.Instance.Login, ex);
            }
            e.Result = result;
        }

        private void TransferWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            walletExplicitDetail.transfers = (List<Transfer>) e.Result;
            var transfers = walletExplicitDetail.transfers ?? new List<Transfer>();
            SetupPaymentGrid(transferFastGrid);
            transferFastGrid.GroupingFunctions = new List<FastGrid.FastGrid.GroupingFunctionDel> { GroupTransfer };
            transferFastGrid.GroupingComparisons = new List<Comparison<object>>
                {
                    (a, b) => (int) a - (int) b
                };
            transferFastGrid.DataBind(transfers);
            transferFastGrid.CheckSize(true);

            standByControl.IsShown = false;
            standByControl.Hide();
            transferTableLayoutPanel.Hide();
            transferFastGrid.Show();
            summaryTransfersCollapsiblePanel.Show();

            // запустить задачи, ожидающие своего часа
            var callbacks = tasksPerformedOnSynchIsCompleted.ExtractAll(2000);
            callbacks.ForEach(c => ThreadPool.QueueUserWorkItem(c.callback, c.callbackParam));

            wasSynchrnoizedAtLeastOneTime = true;
        }

        private void ShowHideProgress(bool show)
        {
            if (InvokeRequired)
                Invoke(new Action<bool>(b =>
                    {
                        timerProgress.Enabled = b;
                        progressBar.Visible = b;
                    }), show);
            else
            {
                timerProgress.Enabled = show;
                progressBar.Visible = show;
            }
        }

        private void TimerProgressTick(object sender, EventArgs e)
        {
            var val = progressBar.Value + progressBar.Step;
            if (val > progressBar.Maximum) val = 0;
            progressBar.Value = val;
        }

        private void WithdrawOrDepositFromAccount(Account account, decimal sum, bool withdraw)
        {
            WalletError error;
            try
            {
                var walletResulted = withdraw
                                         ? TradeSharpWalletManager.Instance.proxy.TransferToWallet(
                                             CurrentProtectedContext.Instance.MakeProtectedContext(),
                                             AccountStatus.Instance.Login,
                                             account.ID, sum, out error)
                                         : TradeSharpWalletManager.Instance.proxy.TransferToTradingAccount(
                                             CurrentProtectedContext.Instance.MakeProtectedContext(),
                                             AccountStatus.Instance.Login,
                                             account.ID, sum, out error);
                if (error == WalletError.OK)
                {
                    if (UserSettings.Instance.GetAccountEventAction(AccountEventCode.WalletModified) !=
                        AccountEventAction.DoNothing)
                    {
                        var deltaMoney = walletResulted.Balance - walletExplicitDetail.wallet.Balance;
                        var msg = string.Format("Баланс кошелька изменен: {0} {1} {2}",
                                                deltaMoney >= 0 ? "внесено" : "выведено",
                                                deltaMoney.ToStringUniformMoneyFormat(true),
                                                walletResulted.Currency);

                        bool repeatNotification;
                        NotificationBox.Show(msg, "Операция выполнена", out repeatNotification);

                        if (!repeatNotification)
                        {
                            UserSettings.Instance.SwitchAccountEventAction(AccountEventCode.WalletModified);
                            UserSettings.Instance.SaveSettings();
                        }
                    }

                    // обновить данные кошелька и счетов
                    InitiateAsynchLoad();
                    return;
                }
            }
            catch
            {
                error = WalletError.CommonError;
            }

            if (error != WalletError.OK)
            {
                var errorString = EnumFriendlyName<WalletError>.GetString(error);
                Logger.ErrorFormat("WithdrawFromAccount() error: {0}", errorString);
                MessageBox.Show("Ошибка выполнения операции:\n" + errorString,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
        }

        private static void GroupTransfer(object data, out object groupData, out string groupLabel)
        {
            groupData = 0;
            groupLabel = "";
            var transfer = data as Transfer;
            if (transfer == null)
                return;
            var key = transfer.ValueDate.Year * 100 + transfer.ValueDate.Month;
            groupData = key;
            groupLabel = new DateTime(key / 100, key % 100, 1).ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
        }

        #region PaymentSystem

        private void SetupPaySysButtons()
        {
            btnWebMoney.Click += (sender, args) => PaySysButtonClick(PaymentSystem.WebMoney);
            // ! commented due to localization fault
            //btnWebMoney.Tag = PaymentSystem.WebMoney;
        }

        private void GetUserWalletsWebMoney()
        {
            var wallets = GetUserWallets() ?? new List<UserPaymentSystem>();
            var walletsWm = wallets.Where(w => w.SystemPayment == PaymentSystem.WebMoney).ToList();

            if (walletsWm.Count == 0)
                cbWMID.Text = "";
            else
                cbWMID.Text = walletsWm[0].RootId;

            webMoneyPursePanelControl.SetupPurses(walletsWm.Select(w => w.PurseId).ToList());
        }

        private List<UserPaymentSystem> GetUserWallets()
        {
            WalletError error;
            List<UserPaymentSystem> paySystWallets = null;
            try
            {
                paySystWallets = TradeSharpWalletManager.Instance.proxy.GetUserRegistredPaymentSystemWallets(
                    CurrentProtectedContext.Instance.MakeProtectedContext(), AccountStatus.Instance.Login,
                    "", // !! pwrd
                    out error);
            }
            catch (Exception ex)
            {
                Logger.Error("GetWalletsWebMoney() error", ex);
                error = WalletError.CommonError;
            }

            if (error != WalletError.OK)
            {
                MessageBox.Show("Невозможно получить данные кошельков:\n" +
                                EnumFriendlyName<WalletError>.GetString(error), "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // показать данные кошелька
            return paySystWallets;
        }

        private void PaySysButtonClick(PaymentSystem paymentSystem)
        {
            var panel = panelWebMoney;
            if (paymentSystem == PaymentSystem.WebMoney)
            {
                panel.Visible = true;
                panelPaymentSystems.Visible = false;
                GetUserWalletsWebMoney();
            }
        }

        private void BtnShowPaymentSystemsClick(object sender, EventArgs e)
        {
            panelWebMoney.Visible = false;
            panelPaymentSystems.Visible = true;
        }

        private void CbWmidTextChanged(object sender, EventArgs e)
        {
            // 154129735662
            var isValid = WebMoneyValidator.WmidIsValid(cbWMID.Text);
            lblWmidValid.ForeColor = isValid ? SystemColors.ControlText : Color.Red;
        }

        /// <summary>
        /// сохранить данные кошельков WM
        /// </summary>
        private void BtnSaveWebMoneyClick(object sender, EventArgs e)
        {
            var wmId = cbWMID.Text;
            var isValid = WebMoneyValidator.WmidIsValid(wmId);
            if (!isValid)
            {
                MessageBox.Show("WMID введен некорректно.\n" + WebMoneyValidator.GetCorrectWMIDSampleStringWithSpecs(),
                                "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // взять все purseId
            bool errorExists;
            var purses = webMoneyPursePanelControl.GetPurseIds(out errorExists);
            if (errorExists)
            {
                MessageBox.Show("ID кошелька введен некорректно.\n" +
                    WebMoneyValidator.GetCorrectPurseIdSampleStringWithSpecs(),
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // забить на сервер кошельки / WMID
            WalletError error;
            try
            {
                error = TradeSharpWalletManager.Instance.proxy.SetPaymentWalletsBySystem(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    PaymentSystem.WebMoney,
                    purses.Select(p => new UserPaymentSystem
                    {
                        RootId = wmId,
                        PurseId = p,
                        SystemPayment = PaymentSystem.WebMoney
                    }).ToList(),
                    AccountStatus.Instance.Login,
                    ""); // wallet pwrd
            }
            catch (Exception ex)
            {
                Logger.Error("BtnSaveWebMoneyClick() error", ex);
                error = WalletError.CommonError;
            }

            if (error == WalletError.OK)
            {
                MessageBox.Show("Данные платежной системы обновлены");
                GetUserWalletsWebMoney();
                return;
            }
            MessageBox.Show("Ошибка обновления данных:\n" + EnumFriendlyName<WalletError>.GetString(error),
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        private void BtnCancelWebMoneyClick(object sender, EventArgs e)
        {
            GetUserWalletsWebMoney();
        }

        private void WalletFormFormClosing(object sender, FormClosingEventArgs e)
        {
            workerWallet.RunWorkerCompleted -= WorkerWalletOnRunWorkerCompleted;
            transfersWorker.RunWorkerCompleted -= TransferWorkerOnRunWorkerCompleted; 

            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        private void WalletFormResizeEnd(object sender, EventArgs e)
        {
            if (ResizeEnded != null)
                ResizeEnded(this);
        }

        private void WalletFormMove(object sender, EventArgs e)
        {
            if (FormMoved != null)
                FormMoved(this);
        }

        private void TimerUpdateTick(object sender, EventArgs e)
        {
            //lastAccountDataProcessed = AccountStatus.Instance.AccountData
            Invoke(new Action(() =>
            {
                var accountData = AccountStatus.Instance.AccountData;
                if (accountData == null) return;
                if (lastAccountDataProcessed == null || lastAccountDataProcessed.ID != accountData.ID)
                    InitiateAsynchLoad();
            }));
        }

        private void StopTransfersDownloadButtonClick(object sender, EventArgs e)
        {
            stopTransferDownload = true;
        }
    
        public void OpenInvestInPAMMDialog(PerformerStat performer, bool invest)
        {
            ScheduleTaskOnSynchronizationCompleted(state =>
                {
                    if (walletExplicitDetail == null || performer == null) return;

                    var performerAccount = new Account
                        {
                            ID = performer.Account,
                            Currency = performer.DepoCurrency,
                            Balance = (decimal) performer.Equity,
                            Equity = (decimal) performer.Equity
                        };

                    // сколько можно вывести средств?
                    var userShare = walletExplicitDetail.realAccounts.FirstOrDefault(a => a.AccountId == performer.Account);
                    performerAccount.Balance = userShare == null ? 0 : userShare.ShareMoneyWallet;
                    
                    // окно пополнения - снятия
                    var dlg = new WalletWithdrawForm(walletExplicitDetail.wallet,
                                                     gridAccount.GetRowValues<AccountShared>(false)
                                                                .Select(a => a.Account)
                                                                .ToList(),
                                                     performerAccount, true);
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    
                    if (dlg.TargetWithdrawal > 0)
                        InvestInPAMM(performer, dlg.TargetWithdrawal, false, dlg.WithdrawAll);
                    if (dlg.TargetDespoit > 0)
                        InvestInPAMM(performer, dlg.TargetDespoit, true, dlg.WithdrawAll);
                });
        }

        public void InvestInPAMM(PerformerStat performer, decimal amount, bool investNotWithdraw, bool withdrawAll)
        {
            var accountData = AccountStatus.Instance.AccountData;
            if (accountData == null) return;

            var status = 
                investNotWithdraw 
                    ? TradeSharpWalletManager.Instance.proxy.InvestInPAMM(
                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                        AccountStatus.Instance.Login, performer.Account, amount)
                    : TradeSharpWalletManager.Instance.proxy.WithdrawFromPAMM(
                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                        AccountStatus.Instance.Login, performer.Account, amount, withdrawAll);

            if (status == RequestStatus.OK)
            {
                var msg = investNotWithdraw
                              ? "Сумма {0} {1} зачислена на счет №{2}"
                              : "Сумма {0} {1} списана со счета №{2}";

                MessageBox.Show(string.Format(msg, amount.ToStringUniformMoneyFormat(), accountData.Currency, performer.Account), 
                    investNotWithdraw ? "Зачисление проведено" : "Списание проведено",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var opStatusName = EnumFriendlyName<RequestStatus>.GetString(status);
            var msgFail = investNotWithdraw
                              ? "Сумма {0} {1} не зачислена на счет: {2}"
                              : "Сумма {0} {1} не списана со счета: {2}";

            MessageBox.Show(string.Format(msgFail,
                    amount.ToStringUniformMoneyFormat(), accountData.Currency, opStatusName),
                    investNotWithdraw ? "Зачисление не проведено" : "Списание не проведено",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ScheduleTaskOnSynchronizationCompleted(WaitCallback callback, object ptr = null)
        {
            if (wasSynchrnoizedAtLeastOneTime)
                ThreadPool.QueueUserWorkItem(callback, ptr);
            else
                tasksPerformedOnSynchIsCompleted.Add(new SynchCompletedTask
                    {
                        callback = callback,
                        callbackParam = ptr
                    }, 2000);
        }

        private void btnRefreshAll_Click(object sender, EventArgs e)
        {
            InitiateAsynchLoad();
        }
    }
    // ReSharper restore LocalizableElement
    // ReSharper restore RedundantArgumentDefaultValue
}

