using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class RemindPasswordForm : Form
    {
        public RemindPasswordForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private void RemindPasswordFormLoad(object sender, EventArgs e)
        {
            var login = !string.IsNullOrEmpty(AccountStatus.Instance.Login)
                            ? AccountStatus.Instance.Login
                            : UserSettings.Instance.Login;
            tbLogin.Text = login;
        }

        private void BtnRemindClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbLogin.Text) && 
                string.IsNullOrEmpty(tbEmail.Text))
            {
                MessageBox.Show(Localizer.GetString("MessageAtLeastOneOfTheFieldsShouldBeCompleted"),
                                Localizer.GetString("TitleError"), 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                if (!TradeSharpAccount.Instance.proxy.RemindPassword(tbEmail.Text, tbLogin.Text))
                {
                    MessageBox.Show(Localizer.GetString("MessageSuchUserNotfound"), 
                        Localizer.GetString("TitleError"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в RemindPassword", ex);
                MessageBox.Show(Localizer.GetString("MessageOperationFailed"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(Localizer.GetString("MessageEmailWithPasswordWasSent"),
                Localizer.GetString("TitleConfirmation"));
            Close();
        }
    }
}
