using System;
using System.Text;
using System.Windows.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ReadOnlyAccountForm : Form
    {
        public ReadOnlyAccountForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private void ReadOnlyAccountFormLoad(object sender, EventArgs e)
        {
            // подставить данные существующего пользователя
            lblStatus.Text = Localizer.GetString("TitleNotAuthorized");
            btnMake.Text = Localizer.GetString("TitleCreate");

            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null || AccountStatus.Instance.accountID <= 0) return;
            PlatformUser user;
            RequestStatus status;
            try
            {
                status = MainForm.serverProxyTrade.proxy.QueryReadonlyUserForAccount(ctx,
                    AccountStatus.Instance.accountID, out user);
                lblStatus.Text = EnumFriendlyName<RequestStatus>.GetString(status);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Данные не получены";
                Logger.Error("Ошибка в QueryReadonlyUserForAccount()", ex);
                return;
            }
            
            if (status != RequestStatus.OK)
            {
                lblStatus.Text = "Данные не получены";
                return;
            }

            if (user != null)
            {
                tbLogin.Text = user.Login;
                tbPassword.Text = user.Password;
                btnMake.Text = Localizer.GetString("TitleEdit");
            }
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null || AccountStatus.Instance.accountID <= 0) return;

            PlatformUser user;
            var status = MainForm.serverProxyTrade.proxy.MakeOrDeleteReadonlyUser(ctx, AccountStatus.Instance.accountID,
                                                                                  false, "", out user);
            lblStatus.Text = EnumFriendlyName<CreateReadonlyUserRequestStatus>.GetString(status);
            if (status == CreateReadonlyUserRequestStatus.Success)
            {
                tbLogin.Text = "";
                tbPassword.Text = "";
            }
        }

        private void BtnMakeClick(object sender, EventArgs e)
        {
            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null || AccountStatus.Instance.accountID <= 0) return;

            // проверить пароль
            var errors = new StringBuilder();
            if (tbPassword.Text.Length < 8)
                errors.AppendLine(string.Format(Localizer.GetString("MessagePasswordTooShortFmt"), 8));
            else if (tbPassword.Text.Length > 25)
                errors.AppendLine(string.Format(Localizer.GetString("MessagePasswordTooLongFmt"), 25));
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PlatformUser user;
            var status = MainForm.serverProxyTrade.proxy.MakeOrDeleteReadonlyUser(ctx, AccountStatus.Instance.accountID,
                                                                                  true, tbPassword.Text, out user);
            lblStatus.Text = EnumFriendlyName<CreateReadonlyUserRequestStatus>.GetString(status);
            if (status == CreateReadonlyUserRequestStatus.Success)
            {
                tbLogin.Text = user.Login;
                tbPassword.Text = user.Password;
            }
        }
    }
}
