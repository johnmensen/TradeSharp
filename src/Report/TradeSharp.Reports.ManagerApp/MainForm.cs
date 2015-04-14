using System;
using System.IO;
using System.Windows.Forms;
using TradeSharp.Reports.Lib.Report;
using TradeSharp.Reports.ManagerApp.BL;
using TradeSharp.Util;

namespace TradeSharp.Reports.ManagerApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnBrowseTemplateFolderClick(object sender, EventArgs e)
        {
            var title = sender == btnBrowseTemplateFolder
                            ? "Укажите каталог шаблонов"
                            : sender == btnBrowseDestFolder
                                  ? "Укажите каталог назначения"
                                  : "Укажите временный каталог";
            var dlg = new FolderBrowserDialog {Description = title};
            if (dlg.ShowDialog() != DialogResult.OK) return;
            if (sender == btnBrowseTemplateFolder) tbFolderTemplate.Text = dlg.SelectedPath;
            else if (sender == btnBrowseDestFolder) tbFolderDest.Text = dlg.SelectedPath;
            else tbFolderTemp.Text = dlg.SelectedPath;
        }

        private void BtnAcceptSettingsClick(object sender, EventArgs e)
        {
            ReportingSettings.Instance.TemplateFolder = tbFolderTemplate.Text;
            ReportingSettings.Instance.DestFolder = tbFolderDest.Text;
            ReportingSettings.Instance.TempFolder = tbFolderTemp.Text;
            ReportingSettings.Instance.AccountId = tbAccountId.Text.ToInt();
            var indexes = tbBenchmarks.Text.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            ReportingSettings.Instance.BenchmarkA = indexes[0];
            ReportingSettings.Instance.BenchmarkB = indexes[1];
            ReportingSettings.Instance.ServerSidePath = tbServerSide.Text;
            ReportingSettings.Instance.SaveSettings();
        }

        private void BtnDeclineSettingsClick(object sender, EventArgs e)
        {
            InitSettings();
        }

        private void InitSettings()
        {
            tbFolderTemplate.Text = ReportingSettings.Instance.TemplateFolder;
            tbFolderDest.Text = ReportingSettings.Instance.DestFolder;
            tbFolderTemp.Text = ReportingSettings.Instance.TempFolder;
            tbAccountId.Text = ReportingSettings.Instance.AccountId.ToString();
            tbBenchmarks.Text = ReportingSettings.Instance.BenchmarkA + "," + ReportingSettings.Instance.BenchmarkB;
            if (!string.IsNullOrEmpty(ReportingSettings.Instance.ServerSidePath))
                tbServerSide.Text = ReportingSettings.Instance.ServerSidePath;
        }

        private void btnMakeMonthly_Click(object sender, EventArgs e)
        {
            var repo = new ReportInvestorMonthly(ReportingSettings.Instance.BenchmarkA,
                                                 ReportingSettings.Instance.BenchmarkB);
            var pathToResult = repo.MakePdf(ReportingSettings.Instance.AccountId,
                         ReportingSettings.Instance.TemplateFolder,
                         ReportingSettings.Instance.DestFolder,
                         ReportingSettings.Instance.TempFolder);
            if (string.IsNullOrEmpty(pathToResult))
                MessageBox.Show(Localizer.GetString("MessageErrorMakingReport"));
            else
                MessageBox.Show(string.Format(Localizer.GetString("MessageReportIsMadeFmt"), pathToResult));
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            InitSettings();
        }

        private void BtnSendToServerClick(object sender, EventArgs e)
        {
            // копировать отчет в папку на сервере
            var fileName = ReportInvestorMonthly.GetResultFileName(ReportingSettings.Instance.DestFolder);
            if (!File.Exists(fileName))
            {
                MessageBox.Show(string.Format(
                    Localizer.GetString("MessageReportFileNotFoundFmt"), fileName),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var fileNameDest = string.Format("{0}\\{1}", tbServerSide.Text, Path.GetFileName(fileName));

            // копировать
            try
            {
                File.Copy(fileName, fileNameDest, true);
                MessageBox.Show(Localizer.GetString("MessageReportCopiedOnServer"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageReportFileCopyErrorFmt"), fileName, ex.Message),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
