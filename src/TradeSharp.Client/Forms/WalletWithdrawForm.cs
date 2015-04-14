using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class WalletWithdrawForm : Form
    {
        private Wallet wallet;
        private List<Account> realAccounts;
        private Account account;
        /// <summary>
        /// сумма, которую пользователь хочет положить на счет
        /// </summary>
        private decimal targetDeposit;
        private decimal targetWithdrawal;

        public decimal TargetDespoit
        {
            get
            {
                return tabControl.SelectedTab == tabPageDeposit ? targetDeposit : 0;
            }
        }
        public decimal TargetWithdrawal
        {
            get
            {
                return tabControl.SelectedTab == tabPageWithdraw ? targetWithdrawal : 0;
            }
        }
        public bool WithdrawAll
        {
            get { return cbWithdrawAll.Checked; }
        }

        public WalletWithdrawForm()
        {
            InitializeComponent();
        }

        public WalletWithdrawForm(Wallet wallet, 
            List<Account> realAccounts,
            Account account, 
            bool deposit) : this()
        {
            this.wallet = wallet;
            this.account = account;
            this.realAccounts = realAccounts;

            Text = "Счет №" + account.ID;
            tabControl.SelectedTab = deposit ? tabPageDeposit : tabPageWithdraw;

            // вкладка Пополнить
            lblDepositWalletCurrency.Text = wallet.Currency;
            tbDepositAmount.Text = wallet.Balance.ToStringUniformMoneyFormat(true);
            lblAccountAmount.Text = account.Equity.ToStringUniformMoneyFormat(true) + 
                " " + account.Currency;

            // вкладка Вывести
            tbWithdraw.Text = "0";
            lblWithdrawCurrency.Text = account.Currency;
            lblMargin.Text = account.UsedMargin.ToStringUniformMoneyFormat(true) + " " + account.Currency;
            lblAccountRemains.Text = account.Equity.ToStringUniformMoneyFormat(true) + " " + account.Currency;
        }

        /// <summary>
        /// посчитать, сколько будет на счете после пополнения из кошелька
        /// </summary>
        private void UpdateDepositTargetLabel()
        {
            targetDeposit = 0;
            if (wallet.Balance > 0)
            {
                var sum = tbDepositAmount.Text.Replace(" ", "").ToDecimalUniformSafe();
                if (sum.HasValue && sum.Value > 0)
                {
                    if (sum.Value > wallet.Balance)
                        sum = wallet.Balance;

                    // привести сумму пополнения к нужной валюте
                    if (wallet.Currency == account.Currency)
                        targetDeposit = sum.Value;
                    else
                    {
                        string strError;
                        var destVal = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(account.Currency,
                            wallet.Currency, (double) sum.Value, QuoteStorage.Instance.ReceiveAllData(), out strError);
                        targetDeposit = destVal ?? 0;
                        if (!string.IsNullOrEmpty(strError))
                        {
                            lblDepositTarget.Text = strError;
                            return;
                        }
                    }
                }
            }
            lblDepositTarget.Text = (account.Equity + targetDeposit).ToStringUniformMoneyFormat(true) + " " + account.Currency;
        }

        private void TbDepositAmountTextChanged(object sender, System.EventArgs e)
        {
            UpdateDepositTargetLabel();
        }

        private void BtnDepositAllClick(object sender, System.EventArgs e)
        {
            tbDepositAmount.Text = wallet.Balance.ToStringUniformMoneyFormat(true);
        }

        private void BtnWithdrawAllClick(object sender, System.EventArgs e)
        {
            
        }

        /// <summary>
        /// посчитать, сколько останется в кошельке
        /// после снятия определенной суммы
        /// </summary>
        private void TbWithdrawTextChanged(object sender, System.EventArgs e)
        {
            lblMargin.ForeColor = SystemColors.ControlText;
            targetWithdrawal = 0;
            var volume = tbWithdraw.Text.Replace(" ", "").ToDecimalUniformSafe();
            if (volume == null) return;

            // перевести в валюту кошелька
            if (wallet.Currency != account.Currency)
            {
                string strError;
                var destVal = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(wallet.Currency,
                    account.Currency, (double)volume, QuoteStorage.Instance.ReceiveAllData(), out strError);
                targetWithdrawal = destVal ?? 0;
                if (!string.IsNullOrEmpty(strError))
                {
                    lblAccountRemains.Text = strError;
                    return;
                }
            }
            else            
                targetWithdrawal = volume.Value;

            var remains = account.Equity - targetWithdrawal;
            lblAccountRemains.Text = remains.ToStringUniformMoneyFormat(true) + " " + account.Currency;

            // если осталось мало - предупредить
            if (remains < account.UsedMargin*2)
            {
                lblMargin.ForeColor = Color.Red;
            }
        }

        private void cbWithdrawAll_CheckedChanged(object sender, System.EventArgs e)
        {
            if (cbWithdrawAll.Checked)
                tbWithdraw.Text = account.Balance.ToStringUniformMoneyFormat(true);
        }
    }
}
