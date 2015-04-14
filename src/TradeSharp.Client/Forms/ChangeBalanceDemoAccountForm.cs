using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ChangeBalanceDemoAccountForm : Form
    {
        private readonly List<Account> accounts;
        
        public ChangeBalanceDemoAccountForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public ChangeBalanceDemoAccountForm(List<Account> accounts) : this()
        {
            this.accounts = accounts;
            foreach (var acc in accounts)
            {
                cbAccount.Items.Add(acc.ID);
            }
            cbAccount.MaxDropDownItems = cbAccount.Items.Count;
            cbAccount.SelectedIndex = 0;
        }
        
        private void CbAccountSelectedIndexChanged(object sender, EventArgs e)
        {
            var item = accounts.First(a => a.ID == (int)cbAccount.SelectedItem);
            lbAccount.Text = string.Format("{0} {1} {2}", Localizer.GetString("TitleBalanceSmall"), item.Balance, item.Currency);
        }
        
        private void BtnAddSummClick(object sender, EventArgs e)
        {
            ChangeBalance(true);
        }

        private void BtnNegSummClick(object sender, EventArgs e)
        {
            ChangeBalance(false);
        }

        private void ChangeBalance(bool isAdd)
        {
            if (string.IsNullOrEmpty(tbSumm.Text))
            {
                MessageBox.Show(Localizer.GetString("MessageAmountNotProvided"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK);
                return;
            }
            var summ = tbSumm.Text.ToDecimalUniformSafe();
            if (summ == null)
            {
                MessageBox.Show(Localizer.GetString("MessageSumFormatWrong") + ": 102.50)", 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK);
                return;
            }
            if (summ.Value <= 0)
            {
                MessageBox.Show(Localizer.GetString("MessageAmountLessThanZero"),
                    Localizer.GetString("TitleError"),
                    MessageBoxButtons.OK);
                Close();
                return;
            }
            if (summ.Value > 1000000)
            {
                MessageBox.Show(Localizer.GetString("MessageAmountGreaterThan") + " 1 000 000",
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK);
                Close();
                return;
            }
            
            summ *= isAdd ? 1 : -1;

            // проводим манипуляции с пополнением счета            
            var accId = (int) cbAccount.SelectedItem;
            var account = accounts.First(a => a.ID == accId);

            if (account.Balance + summ.Value <= 0)
            {
                MessageBox.Show(Localizer.GetString("MessageTargetAmountLessThanZero"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK);
                return;
            }

            if (TradeSharpAccount.Instance.proxy.ChangeBalance(accId, (decimal)summ, 
                "demo transfer") == RequestStatus.OK)
                MessageBox.Show(
                    Localizer.GetString("MessageOperationCompleted"),
                    Localizer.GetString("TitleConfirmation"), 
                    MessageBoxButtons.OK);
            else
                MessageBox.Show(
                    Localizer.GetString("MessageOperationFailed"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK);
            Close();
        }
    }
}
