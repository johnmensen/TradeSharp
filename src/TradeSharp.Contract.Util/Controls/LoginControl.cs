using System;
using System.Windows.Forms;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Controls
{
    public partial class LoginControl : UserControl
    {
        private readonly AuthenticateDel onAuthenticate;

        public enum ControlMode
        {
            ModeDialog = 0, ModeControl
        }
        private readonly Form owner;
        private readonly ILoginUserSettings settingsStorage;
        private readonly EventHandler onClose;
        private readonly EventHandler onRegLinkClicked;
        private readonly ControlMode controlMode;

        public bool HidePassword
        {
            get { return cbHidePassword.Checked; }
            set { cbHidePassword.Checked = value; }
        }

        public string Login
        {
            get { return cbLogin.Text; }
            set { cbLogin.Text = value; }
        }

        public string Password
        {
            get { return tbPassword.Text; }
            set { tbPassword.Text = value; }
        }

        public LoginControl()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public LoginControl(ControlMode controlMode,
            Form owner, 
            ILoginUserSettings settingsStorage,
            EventHandler onClose,
            EventHandler onRegLinkClicked,
            AuthenticateDel onAuthenticate) : this()
        {
            this.owner = owner;
            this.settingsStorage = settingsStorage;
            this.onClose = onClose;
            this.onAuthenticate = onAuthenticate;
            this.controlMode = controlMode;
            this.onRegLinkClicked = onRegLinkClicked;

            if (controlMode == ControlMode.ModeControl)
            {
                btnLogin.Text = Localizer.GetString("TitleSave");
            }
        }

        private void LoginControlLoad(object sender, EventArgs e)
        {
            // заполнить список логинов
            var logins = settingsStorage.LastLogins;
            foreach (var login in logins)
                cbLogin.Items.Add(login);
            var lastLogin = settingsStorage.Login;
            
            // последний залогиненный логин
            if (!string.IsNullOrEmpty(lastLogin))
            {
                cbLogin.Text = lastLogin;
                var pwrd = settingsStorage.GetPasswordForLogin(lastLogin);
                tbPassword.Text = pwrd;
                if (!string.IsNullOrEmpty(pwrd))
                    cbSavePwrd.Checked = true;
            }
        }

        private void BtnLoginClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbLogin.Text))
            {
                MessageBox.Show(
                    Localizer.GetString("MessageLoginNotProvided"),
                    Localizer.GetString("TitleLoginError"), 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show(
                    Localizer.GetString("MessagePasswordNotProvided"),
                    Localizer.GetString("TitleLoginError"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }
            
            AuthenticationResponse response;
            string authResultString;
            var success = onAuthenticate(cbLogin.Text, tbPassword.Text,
                out response, out authResultString);
            if (!success)
            {
                MessageBox.Show(
                    authResultString, 
                    Localizer.GetString("TitleError"),
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Asterisk);
                return;
            }

            // сохранить успешные настройки
            settingsStorage.StoreLogin(cbLogin.Text, cbSavePwrd.Checked ? tbPassword.Text : String.Empty);
            settingsStorage.SaveSettings();
            
            if (controlMode != ControlMode.ModeControl)
            {
                owner.DialogResult = DialogResult.Yes;
                onClose(this, EventArgs.Empty);
            }            
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            if (controlMode == ControlMode.ModeDialog)
            {
                owner.DialogResult = DialogResult.No;
                onClose(this, EventArgs.Empty);
            }
        }

        private void CbHidePasswordCheckedChanged(object sender, EventArgs e)
        {
            tbPassword.UseSystemPasswordChar = cbHidePassword.Checked;            
        }

        private void CbLoginSelectedIndexChanged(object sender, EventArgs e)
        {
            var pwrd = settingsStorage.GetPasswordForLogin(cbLogin.Text);
            if (!string.IsNullOrEmpty(pwrd))
                tbPassword.Text = pwrd;
        }

        private void LinkRegistrationLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            onRegLinkClicked(sender, e);
        }        
    }
}
