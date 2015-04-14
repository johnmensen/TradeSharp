using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    /// <summary>
    /// надо бы разместить свои контролы во всплывающем окошке
    /// 
    /// http://www.codeproject.com/Articles/37912/Embedding-NET-Controls-to-NotifyIcon-Balloon-Toolt
    /// </summary>
    public partial class MainForm
    {
        /// <summary>
        /// свернуть в трей, если выбрана соотв. опция
        /// </summary>        
        private void MainFormResize(object sender, EventArgs e)
        {
            if (UserSettings.Instance.HideToTray && FormWindowState.Minimized == WindowState)
            {
                HideToTray();
                return;
            }

            if (FormWindowState.Normal == WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        private string MakeTrayTitle()
        {
            var account = AccountStatus.Instance.isAuthorized ? AccountStatus.Instance.AccountData : null;
            if (account == null) return "Терминал TRADE#";
            return string.Format("Счет №{0}, баланс {1} {2}",
                                 account.ID, 
                                 account.Balance.ToStringUniformMoneyFormat(), 
                                 account.Currency);
        }

        private void NotifyIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromTray();
        }    

        private void MenuMinimizeInTrayClick(object sender, EventArgs e)
        {
            HideToTray();
        } 

        public void HideToTray()
        {
            notifyIcon.BalloonTipText = MakeTrayTitle();
            notifyIcon.Text = notifyIcon.BalloonTipText;
            notifyIcon.BalloonTipTitle = "TRADE#";
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(500);
            Hide();
        }

        public void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void MenuTrayQuitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuTrayRestoreClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void NotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            contextMenuTray.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        private void MenuTrayCancelClick(object sender, EventArgs e)
        {
            contextMenuTray.Close();
        }

        private void NotifyIconBalloonTipClicked(object sender, EventArgs e)
        {
            contextMenuTray.Show(Cursor.Position.X, Cursor.Position.Y);
        }
    }
}
