using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FastGrid;
using MTS.Live.Util;
using QuoteManager.BL;
using System.Linq;

namespace QuoteManager
{
    public partial class MainForm : Form
    {
        private readonly string iniFilePath = ExecutablePath.ExecPath + "\\quotemgr.txt";
        private const string IniSectPath = "path";
        private const string IniKeySrcFolder = "quoteFolder";

        public MainForm()
        {
            InitializeComponent();
            SetupGrid();
        }

        private void SetupGrid()
        {
            gridInfo.MultiSelectEnabled = true;
            gridInfo.ColorAltCellBackground = Color.FromArgb(220, 220, 212);
            gridInfo.columns.Add(new FastColumn("TickerName", "Тикер") 
                { ColumnWidth = 68, SortOrder = FastColumnSort.Ascending});
            gridInfo.columns.Add(new FastColumn("Size", "KB") { ColumnWidth = 68 });
            gridInfo.columns.Add(new FastColumn("StartDate", "Начало") 
                { ColumnWidth = 71, FormatString = "dd.MM.yyyy" });
            gridInfo.columns.Add(new FastColumn("EndDate", "Конец") 
                { ColumnWidth = 71, FormatString = "dd.MM.yyyy" });
            gridInfo.MinimumTableWidth = gridInfo.columns.Sum(c => c.ColumnWidth);
        }

        private void BtnBrowseQuoteFolderClick(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog
                          {
                              Description = "Каталог котировок", 
                              ShowNewFolderButton = true
                          };
            if (Directory.Exists(tbQuoteFolder.Text))
                dlg.SelectedPath = tbQuoteFolder.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbQuoteFolder.Text = dlg.SelectedPath;
                OnFolderSelected(tbQuoteFolder.Text);
            }
        }

        private void OnFolderSelected(string path)
        {
            new IniFile(iniFilePath).WriteValue(IniSectPath, IniKeySrcFolder, path);
            // прочитать статистику из каталога
            ShowQuoteFolderStat(path);
        }

        private void ShowQuoteFolderStat(string path)
        {
            if (!Directory.Exists(path)) return;
            var infos = new List<QuoteFileInfo>();
            foreach (var fileName in Directory.GetFiles(path, "*.quote"))
            {
                var inf = QuoteFileInfo.ReadFile(fileName);
                if (inf != null) infos.Add(inf);
            }
            var totalFiles = infos.Count;
            var totalSizeKb = infos.Sum(i => i.Size) / 1024;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} файлов, {1} KB котировок", totalFiles, totalSizeKb));
            boxInfo.Text += sb.ToString();
            
            gridInfo.DataBind(infos);
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            var quoteFolder = new IniFile(iniFilePath).ReadValue(IniSectPath, IniKeySrcFolder, string.Empty);
            if (!string.IsNullOrEmpty(quoteFolder))
            {
                tbQuoteFolder.Text = quoteFolder;
                ShowQuoteFolderStat(quoteFolder);
            }
        }

        private void MenuitemTrimHistoryClick(object sender, EventArgs e)
        {
            // для всех либо для выбранных строк
            var selectedRows = gridInfo.rows.Where(r => r.Selected).ToList();
            if (selectedRows.Count == 0)
                selectedRows.AddRange(gridInfo.rows.ToList());
            if (selectedRows.Count == 0) return;
            
            // получить список записей QuoteFileInfo
            var fileInfos = selectedRows.Select(r => (QuoteFileInfo) r.ValueObject).ToList();

            // открыть диалог отсечения истории
            var dlg = new TruncateHistoryForm(fileInfos);
            dlg.ShowDialog();
        }

        private void MenuitemMakeIndexesClick(object sender, EventArgs e)
        {
            var quoteInfo = gridInfo.rows.Select(r => (QuoteFileInfo) r.ValueObject).ToList();
            new IndexMakerForm(tbQuoteFolder.Text, quoteInfo).ShowDialog();
        }
    }
}
