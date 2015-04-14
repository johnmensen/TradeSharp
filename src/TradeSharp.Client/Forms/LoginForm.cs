using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Util.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class LoginForm : Form
    {
        private readonly LoginControl loginControl;

        public bool HidePassword { get { return loginControl.HidePassword; } }

        public string Login
        {
            get { return loginControl.Login; }
            set { loginControl.Login = value; }
        }

        public string Password
        {
            get { return loginControl.Password; }
            set { loginControl.Password = value; }
        }

        public LoginForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public LoginForm(MainForm owner) : this()
        {
            loginControl = new LoginControl(LoginControl.ControlMode.ModeDialog,
                this, UserSettings.Instance, CloseForm, owner.OpenNewAccountForm,
                owner.Authenticate) { Dock = DockStyle.Fill, HidePassword = UserSettings.Instance.HidePasswordChars };
            Controls.Add(loginControl);
            AcceptButton = loginControl.btnLogin;
            CancelButton = loginControl.btnCancel;
        }
        
        private void CloseForm(object sender, EventArgs e)
        {
            Close();
        }
    }
}