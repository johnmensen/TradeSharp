using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ManageAccountForm : Form
    {
        private class TitleValue
        {
            public string Title { get; set; }

            public string Value { get; set; }

            public TitleValue(string title, string value)
            {
                Title = title;
                Value = value;
            }
        }

        private readonly Regex regUserName = new Regex(@"[\-0-9a-zA-Z]+");

        private List<Account> userAccounts = new List<Account>();
        
        public ManageAccountForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupGrids();
        }

        public ManageAccountForm(bool showNewAccountPage) : this()
        {
            if (showNewAccountPage)
                BtnRegisterClick(this, new EventArgs());
        }

        private void ManageAccountFormLoad(object sender, EventArgs e)
        {
            UpdateAccountsList(false);
        }

        private void SetupGrids()
        {
            // выбранный счет
            gridSelectedAccount.AutoGenerateColumns = false;

            // все счета
            var blank = new TitleValue(string.Empty, string.Empty);
            gridAllAccounts.Columns.Add(new FastColumn(blank.Property(p => p.Title), "#")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnWidth = 40
                });
            gridAllAccounts.Columns.Add(new FastColumn(blank.Property(p => p.Value),
                                                       Localizer.GetString("TitleDescription")) {ColumnMinWidth = 80});
        }

        private void UpdateAccountsList(bool popupEnabled)
        {
            userAccounts.Clear();
            gridAllAccounts.rows.Clear();
            gridSelectedAccount.DataSource = new List<TitleValue>();

            var login = AccountStatus.Instance.Login;
            if (string.IsNullOrEmpty(login))
                login = UserSettings.Instance.Login;
            if (string.IsNullOrEmpty(login)) return;

            var acData = AccountStatus.Instance.AccountData;
            if (acData != null)
            {
                // заполнить таблицу - выбранный счет
                // ReSharper disable UseObjectOrCollectionInitializer
                var selAccountFields = new List<TitleValue>();
                // ReSharper restore UseObjectOrCollectionInitializer
                selAccountFields.Add(new TitleValue("#", acData.ID.ToString()));
                selAccountFields.Add(new TitleValue(Localizer.GetString("TitleGroup"), acData.Group));
                selAccountFields.Add(new TitleValue(Localizer.GetString("TitleFunds"), acData.Equity.ToStringUniformMoneyFormat(false) +
                                                                " " + acData.Currency));
                gridSelectedAccount.DataSource = selAccountFields;
            }

            AuthenticationResponse status;
            try
            {
                List<Account> accounts;
                status = MainForm.serverProxyTrade.proxy.GetUserAccountsWithDetail(
                    login, CurrentProtectedContext.Instance.MakeProtectedContext(), out accounts);
                if (accounts != null) userAccounts = accounts.ToList();
            }
            catch (Exception ex)
            {
                status = AuthenticationResponse.ServerError;
                Logger.Error("Ошибка в UpdateAccountsList (GetUserAccounts)", ex);
            }            
            if (status != AuthenticationResponse.OK)
            {
                if (popupEnabled)
                    MessageBox.Show(EnumFriendlyName<AuthenticationResponse>.GetString(status), 
                        Localizer.GetString("TitleError"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // добавить в таблицу
            var tags = new List<TitleValue>();
            foreach (var ac in userAccounts)
            {
                if (acData != null) if (ac.ID == acData.ID) continue;
                var acTag = new TitleValue(ac.ID.ToString(),
                                           string.Format("{0}, {1} {2}", ac.Group, 
                                           ac.Balance.ToStringUniformMoneyFormat(false), ac.Currency));
                tags.Add(acTag);                
            }
            gridAllAccounts.DataBind(tags);
            gridAllAccounts.Invalidate();
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnRemindPasswordClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            new RemindPasswordForm().ShowDialog();
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnChangePasswordClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            new UpdatePasswordForm().ShowDialog();
        }

        private void BtnBalanceClick(object sender, EventArgs e)
        {
            var demoAccounts = new List<Account>();
            var groups = DalAccountGroup.Instance.Groups;
            foreach (var ac in userAccounts)
            {
                var acGroup = ac.Group;
                var group = groups.FirstOrDefault(g => g.Code == acGroup);
                if (group != null && !group.IsReal)
                    demoAccounts.Add(ac);
            }
            if (demoAccounts.Count == 0) return;

            new ChangeBalanceDemoAccountForm(demoAccounts).ShowDialog();
        }
        
        private void MenuitemUpdateClick(object sender, EventArgs e)
        {
            UpdateAccountsList(true);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnEditUserDataClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            if (!AccountStatus.Instance.isAuthorized) return;
            var form = new EditAccountDataForm();
            form.SetMode(false);
            form.ShowDialog();
        }

        private void BtnRegisterClick(object sender, EventArgs e)
        {
            var form = new EditAccountDataForm();
            form.SetMode(true);
            form.ShowDialog();
        }

        private void BtnReadonlyUserClick(object sender, EventArgs e)
        {
            new ReadOnlyAccountForm().ShowDialog();
        }
    }
}
