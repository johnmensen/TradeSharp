using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;

namespace TradeSharp.Client
{
    partial class MainForm
    {
        private volatile bool loginDialogIsShown;

        public void OpenLoginDialog(string login = null, string pwrd = null)
        {
            if (loginDialogIsShown) return;
            try
            {
                loginDialogIsShown = true;
                var dlg = new LoginForm(this);
                if (!string.IsNullOrEmpty(login))
                    dlg.Login = login;
                if (!string.IsNullOrEmpty(pwrd))
                    dlg.Password = pwrd;
                var dlgResult = dlg.ShowDialog();
                UserSettings.Instance.HidePasswordChars = dlg.HidePassword;

                if (dlgResult != DialogResult.Yes) return;
                if (!AccountStatus.Instance.isAuthorized) return;
                // показать диалог выбора счета
                var dlgAc = new SelectAccountForm();
                dlgAc.ShowDialog();
                HistoryOrderStorage.Instance.HurryUpUpdate();
            }
            finally
            {
                loginDialogIsShown = false;
            }
        }
    }
}
