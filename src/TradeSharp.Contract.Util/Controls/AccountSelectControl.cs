using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Controls
{
    public partial class AccountSelectControl : UserControl
    {
        public delegate void AccountSelectedDel(int accountId);
        public delegate ProtectedOperationContext GetProtectedOperationContextDel();
        
        private readonly GetProtectedOperationContextDel getProtectedOperationContext;
        private readonly ITradeSharpServerTrade proxyAccount;
        private readonly string login;

        private AccountSelectedDel onAccountSelected;
        public event AccountSelectedDel OnAccountSelected
        {
            add { onAccountSelected += value; }
            remove { onAccountSelected -= value; }
        }

        public int AccountId
        {
            get { return string.IsNullOrEmpty(cbAccount.Text) ? 0 : cbAccount.Text.ToIntSafe() ?? 0; }
            set { cbAccount.Text = value.ToString(); }
        }

        public AccountSelectControl()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public AccountSelectControl(GetProtectedOperationContextDel getProtectedOperationContext,
            ITradeSharpServerTrade proxyAccount, string login) : this()
        {
            this.getProtectedOperationContext = getProtectedOperationContext;
            this.proxyAccount = proxyAccount;
            this.login = login;
        }        

        private void BtnRefreshClick(object sender, EventArgs e)
        {
            UpdateAccountList(false);
        }

        private void UpdateAccountList(bool silentMode)
        {
            if (proxyAccount == null)
            {
                if (!silentMode) 
                    MessageBox.Show(Localizer.GetString("MessageNoConnectionServer"), 
                        Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            try
            {
                List<Account> accounts;
                var context = getProtectedOperationContext();
                if (context == null) return;
                var response = proxyAccount.GetUserAccountsWithDetail(login, context, out accounts);
                
                if (response == AuthenticationResponse.OK)
                {
                    cbAccount.Items.Clear();
                    foreach (var t in accounts)
                        cbAccount.Items.Add(new AccountForSelectList(t));
                    if (cbAccount.Items.Count == 1)
                        cbAccount.SelectedIndex = 0;
                }
                if (accounts.Count == 0)
                    MessageBox.Show(
                        Localizer.GetString("MessageUserHasNoAccounts"), 
                        Localizer.GetString("TitleWarning"),
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(
                    Localizer.GetString("MessageErrorGettingAccountsFmt"), ex.Message),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            if (cbAccount.SelectedIndex < 0) return;
            var account = ((AccountForSelectList)cbAccount.SelectedItem).account;

            var ctx = getProtectedOperationContext();
            if (ctx == null) return;
            try
            {
                proxyAccount.SelectAccount(ctx, account.ID);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SelectAccount", ex);
            }
            if (onAccountSelected != null) onAccountSelected(account.ID);
        }

        private void AccountSelectControlLoad(object sender, EventArgs e)
        {
            UpdateAccountList(true);
        }
    }

    class AccountForSelectList
    {
        public Account account;
        
        public AccountForSelectList(Account account)
        {
            this.account = account;            
        }

        public override string ToString()
        {
            return account.ID + " / " + account.Group;
        }
    }
}
