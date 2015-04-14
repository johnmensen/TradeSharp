using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.UI.Util.Update;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
	/// <summary>
	/// Summary description for AboutCtrl.
	/// </summary>
	public partial class AboutCtrl : UserControl
    {
        private readonly BackgroundWorker workerCheckVersion = new BackgroundWorker();

		public string NameProgram
		{
			set
            {
				_name = value;
                nameProgramLabel.Text = string.IsNullOrEmpty(value) ? Application.ProductName : value;
			}
			get
            {
				return _name;
			}
		}

		public string VersionProgram
		{
			set
			{
				_version = value;
			}
			get
			{
				return _version;
			}
		}

		public AboutCtrl()
		{
			InitializeComponent();
            Localizer.LocalizeControl(this);
		    try
		    {
                if (LocalizedResourceManager.Instance != null)
                    pictureBoxLogo.Image = (Bitmap)LocalizedResourceManager.Instance.GetObject("terminal_logo_100");
		    }
		    catch(Exception ex)
		    {
                Logger.Error("Ошибка в AboutCtrl", ex);
		    }

		    workerCheckVersion.DoWork += CheckVersionDoWork;
		    workerCheckVersion.RunWorkerCompleted += CheckVersionCompleted;
		}
		
		private void AboutCtrlLoad(object sender, EventArgs e)
		{
            nameProgramLabel.Text = string.IsNullOrEmpty(NameProgram) ? Application.ProductName : NameProgram;

			versionLabel.Text = VersionProgram.Length == 0 ? Application.ProductVersion : string.Format(VersionProgram, Application.ProductVersion);

			linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://tradesharp.net");
			CheckVersion();
		}

		private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			linkLabel1.Links[linkLabel1.Links.IndexOf(e.Link)].Visited = true;
			System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
		}

        private void CheckVersionDoWork(object sender, DoWorkEventArgs e)
        {
            // проверить время файлов
            // файлы в собственной директории
            Dictionary<string, FileVersion> ownVersion;
            var vers = new VersionInfo(Logger.ErrorFormat);
            try
            {
                ownVersion = vers.GetOwnVersionInfo(ExecutablePath.ExecPath);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckVersionThreadSafe", ex);
                return;
            }

            // файлы на сервере
            var siteVersion = vers.GetVersionInfoFromUrl();
            if (siteVersion == null || siteVersion.Count == 0) return;

            // сравнить версии
            var missingFiles = new List<string>();
            var olderFiles = new List<string>();

            foreach (var pair in siteVersion)
            {
                var siteFileName = pair.Key;
                var siteFileAttrs = pair.Value;
                if (!ownVersion.ContainsKey(siteFileName))
                    missingFiles.Add(siteFileName);
                else if (!ownVersion[siteFileName].IsRelevant(siteFileAttrs))
                    olderFiles.Add(siteFileName);
            }

            e.Result = new Cortege2<List<string>, List<string>>(missingFiles, olderFiles);            
        }

        private void CheckVersionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                versionLabel.Text =
                    EnumFriendlyName<AccountConnectionStatus>.GetString(AccountConnectionStatus.ConnectionError);
                versionLabel.ForeColor = Color.Red;
                return;
            }
            var typedResult = (Cortege2<List<string>, List<string>>) e.Result;
            var missingFiles = typedResult.a;
            var olderFiles = typedResult.b;

            // показать сообщение о новой версии
            if (missingFiles.Count <= 0 && olderFiles.Count <= 0) return;

            versionLabel.Text = string.Format(Localizer.GetString("MessageVersionOutdatedNFilesFmt"),
                                              missingFiles.Count + olderFiles.Count);
            versionLabel.ForeColor = Color.Red;
            versionLabel.Cursor = Cursors.Hand;

            // собрать детальную информацию
            var msgUpdate = new StringBuilder();
            if (missingFiles.Count > 0)
                msgUpdate.Append(Localizer.GetString("TitleMissedMany") + ":" + 
                    Environment.NewLine + string.Join(Environment.NewLine, missingFiles) +
                    Environment.NewLine + Environment.NewLine);
            if (olderFiles.Count > 0)
                msgUpdate.Append(Localizer.GetString("TitleStaleMany") + ": " + Environment.NewLine + string.Join(Environment.NewLine, olderFiles) +
                                 Environment.NewLine + Environment.NewLine);
            msgUpdate.Append(Localizer.GetString("MessageTerminalRestartRecommended"));
            versionLabel.Click += (ea, s) => 
                MessageBox.Show(msgUpdate.ToString(), Localizer.GetString("TitleStatus"));
        }

        private void CheckVersion()
        {
            workerCheckVersion.RunWorkerAsync();
        }
	}
}
