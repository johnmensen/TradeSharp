using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class UpdatePasswordForm : Form
    {
        public UpdatePasswordForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private void BtnUpdateClick(object sender, EventArgs e)
        {
            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null)
            {
                MessageBox.Show("Необходимо авторизоваться для смены пароля", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            // проверка ввода
            if (string.IsNullOrEmpty(tbNewPassword.Text))
            {
                MessageBox.Show("Пароль не может быть пустой строкой", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (tbNewPassword.Text.Length < PlatformUser.PasswordLenMin)
            {
                MessageBox.Show(string.Format("Длина пароля не может быть меньше {0} символов", PlatformUser.PasswordLenMin),
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (tbNewPassword.Text.Length > PlatformUser.PasswordLenMax)
            {
                MessageBox.Show(string.Format("Длина пароля не может быть больше {0} символов", PlatformUser.PasswordLenMax),
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                var status = MainForm.serverProxyTrade.proxy.ChangePassword(ctx, AccountStatus.Instance.Login,
                                                                           tbNewPassword.Text);
                if (status != AuthenticationResponse.OK)
                {
                    MessageBox.Show(EnumFriendlyName<AuthenticationResponse>.GetString(status),
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ChangePassword", ex);
                MessageBox.Show("Ошибка исполнения команды", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("Пароль изменен", "Выполнено");
            UserSettings.Instance.StoreLogin(UserSettings.Instance.Login, tbNewPassword.Text);
            Close();
        }
    }
}
