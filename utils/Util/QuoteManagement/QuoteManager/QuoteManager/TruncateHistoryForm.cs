using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MTS.Live.Contract.Entity;
using QuoteManager.BL;

namespace QuoteManager
{
    public partial class TruncateHistoryForm : Form
    {
        private string folderSrc;
        //private DateTime timeStart;
        private List<QuoteFileInfo> fileInfos;

        public TruncateHistoryForm()
        {
            InitializeComponent();
        }

        public TruncateHistoryForm(List<QuoteFileInfo> fileInfos)
        {
            InitializeComponent();
            folderSrc = Path.GetDirectoryName(fileInfos[0].FullPath);
            this.fileInfos = fileInfos;
            var minDate = fileInfos.Min(i => i.StartDate);
            //timeStart = minDate;
            dpStart.Value = minDate;
            dpEnd.Value = DateTime.Now;
        }

        private void BtnTruncateClick(object sender, EventArgs e)
        {
            var dest = tbDestFolder.Text;
            if (string.IsNullOrEmpty(dest)) return;
            if (dest == folderSrc)
            {
                MessageBox.Show("Каталог назначения совпадает с исходным каталогом");
                return;
            }

            var countProc = 0;
            foreach (var fileInf in fileInfos)
            {
                if (fileInf.StartDate >= dpStart.Value && fileInf.EndDate <= dpEnd.Value)
                {
                    // просто скопировать файл в целевой каталог
                    File.Copy(fileInf.FullPath,
                        tbDestFolder.Text.TrimEnd('\\') + '\\' + Path.GetFileName(fileInf.FullPath));
                }
                ProcessFile(fileInf.FullPath, fileInf.TickerName);
                countProc++;
            }

            var msg = string.Format("{0} файлов из {1} усечено", countProc, fileInfos.Count);
            MessageBox.Show(msg);
        }

        private void ProcessFile(string path, string ticker)
        {
            var dateStart = dpStart.Value;
            var dateEnd = dpEnd.Value;
            var destFileName = tbDestFolder.Text.TrimEnd('\\') + "\\" + ticker + ".quote";

            using (var sr = new StreamReader(path))
            using (var sw = new StreamWriter(destFileName))
            {
                DateTime? curTime = null;
                DateTime? storeDate = null;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    DateTime? quoteDate;
                    QuoteData.IsNewFormatDateRecord(line, out quoteDate);
                    if (quoteDate.HasValue)
                        curTime = quoteDate;
                    if (!curTime.HasValue) continue;
                    var quote = QuoteData.ParseQuoteStringNewFormat(line, curTime.Value);
                    if (quote == null) continue;

                    if (quote.time < dateStart) continue;
                    if (quote.time > dateEnd) break;

                    // записать котировку
                    var writeDate = false;
                    if (!storeDate.HasValue) writeDate = true;
                    else
                    {
                        if (storeDate.Value != quote.time.Date)
                            writeDate = true;
                    }
                    storeDate = quote.time.Date;
                    if (writeDate)
                        sw.WriteLine(string.Format("{0:ddMMyyyy}", quote.time));
                    sw.WriteLine(
                        string.Format(CultureInfo.InvariantCulture, "{0:HHmm} {1} {2}", 
                        quote.time, quote.bid, quote.ask));
                }
            }
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog {Description = "Каталог назначения", ShowNewFolderButton = true};
            if (dlg.ShowDialog() == DialogResult.OK)
                tbDestFolder.Text = dlg.SelectedPath;
        }
    }
}
