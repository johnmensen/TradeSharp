using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.FakeUser
{
    public partial class NewAccountForm : Form
    {
        public NewAccountForm()
        {
            InitializeComponent();
        }

        public NewAccountForm(string[] groups) : this()
        {
            cbAccountGroup.Items.AddRange(groups);
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            int accountId;
            var msg = CreateAccount(out accountId);
            if (string.IsNullOrEmpty(msg))
                msg = "Счет открыт: " + accountId;
            lblAccountNumber.Text = msg;
            lblAccountNumber.ForeColor = Color.Red;
            BeginInvoke(new Action(() =>
            {
                Thread.Sleep(2000);
                lblAccountNumber.ForeColor = SystemColors.ControlText;
            }));
        }

        private string CreateAccount(out int accountId)
        {
            accountId = -1;
            if (string.IsNullOrEmpty(tbLogin.Text) ||
                string.IsNullOrEmpty(tbEmail.Text) ||
                string.IsNullOrEmpty(tbPassword.Text) ||
                cbAccountGroup.SelectedIndex < 0) return "Поля не заполнены";

            var login = tbLogin.Text;
            var password = tbPassword.Text;
            var email = tbEmail.Text;
            var group = cbAccountGroup.SelectedItem.ToString();
            var depo = tbStartDepo.Text.ToDecimalUniformSafe() ?? 0;
            var createTime = dpCreateTime.Value;
            
            using (var conn = DatabaseContext.Instance.Make())
            {
                if (conn.PLATFORM_USER.Any(u => u.Login == login))
                    return "Логин занят";
                if (conn.PLATFORM_USER.Any(u => u.Email == email))
                    return "Email занят";
                
                try
                {
                    var usr = new PLATFORM_USER
                    {
                        Login = login,
                        Password = password,
                        Email = email,
                        Name = tbName.Text,
                        Surname = tbSurname.Text,
                        RegistrationDate = createTime,
                        RoleMask = 0,
                        Patronym = "Н",
                        Title = "-"
                    };
                    conn.PLATFORM_USER.Add(usr);
                    var account = new ACCOUNT
                    {
                        Currency = "USD",
                        Balance = depo,
                        MaxLeverage = tbMaxLeverage.Text.ToDecimalUniformSafe() ?? 100,
                        AccountGroup = group,
                        TimeCreated = createTime,
                        Status = (int) Account.AccountStatus.Created                        
                    };
                    conn.ACCOUNT.Add(account);
                    conn.SaveChanges();
                    var pa = new PLATFORM_USER_ACCOUNT
                    {
                        PlatformUser = usr.ID,
                        Account = account.ID,
                        RightsMask = 0
                    };
                    conn.PLATFORM_USER_ACCOUNT.Add(pa);

                    var bc = new BALANCE_CHANGE
                    {
                        AccountID = account.ID,
                        Amount = depo,
                        ChangeType = (int) BalanceChangeType.Deposit,
                        Description = "Initial deposit",
                        ValueDate = createTime
                    };
                    conn.BALANCE_CHANGE.Add(bc);
                    conn.SaveChanges();
                    accountId = account.ID;
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.GetType().Name + ": " + ex.Message;
                }
            }
        }
    }
}
