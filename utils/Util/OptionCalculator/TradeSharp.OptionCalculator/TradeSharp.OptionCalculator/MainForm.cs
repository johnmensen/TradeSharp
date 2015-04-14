using System;
using System.IO;
using System.Windows.Forms;
using TradeSharp.OptionCalculator.BL;
using TradeSharp.Util;

namespace TradeSharp.OptionCalculator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            OptionData.logMessage += LogMessageSafe;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            ShowLastOption();
        }

        private void btnSetQuoteFolder_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog
            {
                Description = "Укажите каталог котировок (*.hst)"
            };
            if (Directory.Exists(tbQuoteFolder.Text))
                dlg.SelectedPath = tbQuoteFolder.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
                tbQuoteFolder.Text = dlg.SelectedPath;
        }

        private void LoadSettings()
        {
            tbQuoteTimezoneShift.Text = CalcSettings.Instance.QuoteTimeOffset.ToString();
            tbQuoteFolder.Text = CalcSettings.Instance.QuoteFolder;
            tbHighStepPercent.Text = CalcSettings.Instance.HighPercent.ToStringUniform();
            tbIterations.Text = CalcSettings.Instance.IterationsCount.ToString();
        }

        private void InitSettings()
        {
            CalcSettings.Instance.QuoteTimeOffset = tbQuoteTimezoneShift.Text.ToInt();
            CalcSettings.Instance.QuoteFolder = tbQuoteFolder.Text;
            CalcSettings.Instance.HighPercent = tbHighStepPercent.Text.ToDecimalUniformSafe() ?? 1.5M;
            CalcSettings.Instance.IterationsCount = tbIterations.Text.ToIntSafe() ?? 50000;
        }

        private OptionData InitOptionData()
        {
            var option = new OptionData
            {
                Expiration = dpExpireTime.Value,
                QuoteFilePath = GetQuoteFilePath(),
                Strike = tbStrikePrice.Text.ToDecimalUniformSafe() ?? 0,
                ValueDate = dpTradeTime.Value,
                VanillaPrice = tbEnterPrice.Text.ToDecimalUniformSafe() ?? 0,
                RemoveTrend = cbRemoveTrend.Checked
            };
            option.OptionType = cbOptionType.SelectedIndex < 2 ? OptionType.European : OptionType.American;
            option.Side = cbOptionType.SelectedIndex == 0 || cbOptionType.SelectedIndex == 1
                ? OptionSide.Call
                : OptionSide.Put;
            return option;
        }

        private void ShowLastOption()
        {
            var opt = OptionData.LoadLastSettings();
            dpExpireTime.Value = opt.Expiration;
            SetQuoteFilePath(opt.QuoteFilePath);
            tbStrikePrice.Text = opt.Strike.ToStringUniformPriceFormat(true);
            dpTradeTime.Value = opt.ValueDate;
            tbEnterPrice.Text = opt.VanillaPrice.ToStringUniformPriceFormat(true);
            cbRemoveTrend.Checked = opt.RemoveTrend;
            cbOptionType.SelectedIndex = 
                (opt.OptionType == OptionType.European && opt.Side == OptionSide.Call) ? 0 :
                (opt.OptionType == OptionType.European && opt.Side == OptionSide.Put) ? 1 :
                (opt.OptionType == OptionType.American && opt.Side == OptionSide.Call) ? 2 :
                3;
        }

        private void btnDiscardSettings_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }        

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            InitSettings();
            CalcSettings.Instance.SaveSettings();
        }

        private void btnPickQuoteFile_Click(object sender, EventArgs e)
        {
            var path = Path.Combine(CalcSettings.Instance.QuoteFolder,
                tbQuoteFileName.Text);
            if (File.Exists(path))
                openFileDialog.FileName = path;
            else
            {
                if (Directory.Exists(CalcSettings.Instance.QuoteFolder))
                    openFileDialog.InitialDirectory = CalcSettings.Instance.QuoteFolder;
            }            

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            SetQuoteFilePath(openFileDialog.FileName);
        }

        private string GetQuoteFilePath()
        {
            if (string.IsNullOrEmpty(tbQuoteFileName.Text)) return "";
            if (File.Exists(tbQuoteFileName.Text)) return tbQuoteFileName.Text;
            var path = Path.Combine(CalcSettings.Instance.QuoteFolder,
                tbQuoteFileName.Text);
            if (File.Exists(path)) return path;
            return "";
        }

        private void SetQuoteFilePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var dir = Path.GetDirectoryName(path);
            if (dir.Equals(CalcSettings.Instance.QuoteFolder, StringComparison.OrdinalIgnoreCase))
                tbQuoteFileName.Text = Path.GetFileName(path);
            else
                tbQuoteFileName.Text = path;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            var option = InitOptionData();
            var errors = option.GetErrorsInSettings();
            if (errors.Count > 0)
            {
                LogMessageSafe("Ошибки заполнения данных:");
                errors.ForEach(LogMessageSafe);
                return;
            }
            // сохранить последние настройки
            option.SaveSettings();
            // считать
            option.Calculate();
        }

        private void LogMessageSafe(string msg)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(LogMessageUnsafe), msg);
            else
                LogMessageUnsafe(msg);
        }

        private void LogMessageUnsafe(string msg)
        {
            tbLog.AppendText(msg + Environment.NewLine);
        }
    }
}