using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using TradeSharp.Util;


namespace TradeSharp.RobotManager
{
    public partial class MainForm
    {
        #region
        private void Form1Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                HideToTray();
                return;
            }

            if (FormWindowState.Normal == WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        public void HideToTray()
        {
            notifyIcon.BalloonTipText = "управление роботами";
            notifyIcon.Text = notifyIcon.BalloonTipText;
            notifyIcon.BalloonTipTitle = "TRADE# Ферма роботов";
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(500);
            Hide();
        }

        private void NotifyIconBalloonTipClicked(object sender, EventArgs e)
        {
            contextMenuTray.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        private void NotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            //Отобразить контекстное меню
            if (e.Button == MouseButtons.Left) contextMenuTray.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        private void NotifyIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromTray();
        }
        #endregion

        #region
        private void MenuTrayQuitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuTrayRestoreClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void MenuTrayCancelClick(object sender, EventArgs e)
        {
            contextMenuTray.Close();
        }

        private void MenuRedirectToSiteClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://localhost:8091");
            }
            catch (Win32Exception ex)
            {
                Logger.Error("Ошибка перехода на веб сайт", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка перехода на веб сайт", ex);
            }

        }
        #endregion

        public void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }
    }
}