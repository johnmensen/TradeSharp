using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SelectAccountForm : Form
    {
        private readonly AccountSelectControl accountControl;

        public SelectAccountForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);

            accountControl = new AccountSelectControl(CurrentProtectedContext.Instance.MakeProtectedContext,
                                                      MainForm.serverProxyTrade.proxy, AccountStatus.Instance.Login);
            accountControl.OnAccountSelected += id =>
                                                    {
                                                        AccountStatus.Instance.accountID = id;
                                                        AccountStatus.Instance.isAccountSelected = true;
                                                        UserSettings.Instance.Account = id.ToString();
                                                        UserSettings.Instance.SaveSettings();
                                                        DialogResult = DialogResult.OK;
                                                        Close();
                                                    };
            Controls.Add(accountControl);
            AcceptButton = accountControl.btnAccept;
            CancelButton = accountControl.btnCancel;
        }
    }
}
