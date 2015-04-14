using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class QuoteBaseForm : Form
    {
        public enum Action { Actualize = 0, ClearAndReload, ReadFromFile }

        private BackgroundWorker workerUpdateGrid = new BackgroundWorker();
        private string selectedSymbol;

        public QuoteBaseForm()
        {            
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupGrid();

            workerUpdateGrid.DoWork += UpdateQuoteArchiveDoWork;
            workerUpdateGrid.RunWorkerCompleted += UpdateQuoteArchiveCompleted;

            cbAction.Items.AddRange(EnumItem<Action>.items.Cast<object>().ToArray());
            cbAction.SelectedIndex = 0;
        }

        public QuoteBaseForm(string symbol) : this()
        {
            selectedSymbol = symbol;
        }

        private void SetupGrid()
        {
            var blank = new SymbolArchiveInfo(string.Empty, null);
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Selected), Localizer.GetString("TitleChoice"))
                {
                    ColumnWidth = 49,
                    ImageList = imageListSelected,
                    SortOrder = FastColumnSort.Descending,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });

            grid.Columns.Add(new FastColumn(blank.Property(p => p.IsFavorite), "*")
                {
                    ColumnWidth = 25,
                    ImageList = imageListFavorite,
                    SortOrder = FastColumnSort.Descending,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Loaded), Localizer.GetString("TitleLoaded"))
                {
                    ColumnWidth = 64,
                    formatter = v => (bool) v ? Localizer.GetString("TitleYes") : Localizer.GetString("TitleNo"),
                    SortOrder = FastColumnSort.Descending
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Symbol), Localizer.GetString("TitleInstrument"))
                {
                    ColumnWidth = 75,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.DateRange), Localizer.GetString("TitleInterval"))
                {
                    ColumnMinWidth = 190
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.GapsTotalString),
                                            Localizer.GetString("TitleGapsCountShort")) {ColumnWidth = 85});
            grid.UserHitCell += GridUserHitCell;
            grid.CheckSize(true);
            grid.CalcSetTableMinWidth();
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var smb = (SymbolArchiveInfo) grid.rows[rowIndex].ValueObject;
            if (col.PropertyName == smb.Property(p => p.Selected) && e.Button == MouseButtons.Left)
            {
                smb.Selected = !smb.Selected;
                grid.UpdateRow(rowIndex, smb);
                grid.InvalidateCell(col, rowIndex);
                return;
            }

            if (col.PropertyName == smb.Property(p => p.IsFavorite) && e.Button == MouseButtons.Left)
            {
                // добавить иконку в список выбранных либо исключить из этого списка
                smb.IsFavorite = !smb.IsFavorite;
                grid.UpdateRow(rowIndex, smb);
                grid.InvalidateCell(col, rowIndex);

                var gridTickers = grid.rows.Select(r => ((SymbolArchiveInfo) r.ValueObject));
                var favGridTickers = gridTickers.Where(t => t.IsFavorite).Select(t => t.Symbol).ToArray();
                // сохранить изменения в UserSettings и в DalSpot
                DalSpot.Instance.SetFavoritesList(favGridTickers);
                UserSettings.Instance.FavoriteTickers = DalSpot.Instance.GetTickerNames(true).ToList();
                UserSettings.Instance.SaveSettings();
            }
        }

        private void QuoteBaseFormLoad(object sender, EventArgs e)
        {
            UpdateQuoteArchiveGrid();
        }

        private void UpdateQuoteArchiveGrid()
        {
            // запомнить, какие котировки были отмечены favorites
            var gridTickers = grid.rows.Select(r => ((SymbolArchiveInfo)r.ValueObject));
            var favGridTickers = gridTickers.Where(t => t.IsFavorite).Select(t => t.Symbol).ToArray();

            grid.rows.Clear();
            standByControl.IsShown = true;
            standByControl.Visible = true;
            var dirName = ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder;
            if (!Directory.Exists(dirName))
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                }
                catch
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageErrorCreateQuoteDirectory") + " ({0})",
                                      dirName), 
                                      Localizer.GetString("TitleError"), 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            workerUpdateGrid.RunWorkerAsync(favGridTickers);
        }

        private void UpdateQuoteArchiveDoWork(object sender, DoWorkEventArgs e)
        {
            // получить все котиры
            var symbols = DalSpot.Instance.GetTickerNames().OrderBy(s => s);
            // определить, какие выбраны пользователем
            var favGridTickers = (string[])e.Argument;
            var favTickers = favGridTickers.Length > 0 ? favGridTickers 
                : DalSpot.Instance.GetTickerNames(true);

            var smbInfos = new List<SymbolArchiveInfo>();
            var dateRangeBySymbol = AtomCandleStorage.GetFileDateRange(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder);
            
            foreach (var symbol in symbols)
            {
                Cortege2<DateTime, DateTime> dateRange;
                var smbInf = dateRangeBySymbol.TryGetValue(symbol, out dateRange) 
                    ? new SymbolArchiveInfo(symbol, dateRange) 
                    : new SymbolArchiveInfo(symbol, null);
                // поставить звездочку
                smbInf.IsFavorite = favTickers.Contains(smbInf.Symbol);
                smbInfos.Add(smbInf);
            }
            
            // запихать список в результат
            e.Result = smbInfos;
        }

        private void UpdateQuoteArchiveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var symbols = (List<SymbolArchiveInfo>) e.Result;
            if (symbols.Count > 0)
            {
                grid.DataBind(symbols);
                grid.CheckSize(true);
                // подсветить выбранный символ
                if (!string.IsNullOrEmpty(selectedSymbol))
                {
                    var row = grid.rows.FirstOrDefault(r => ((SymbolArchiveInfo) r.ValueObject).Symbol == selectedSymbol);
                    if (row != null)
                    {
                        row.Selected = true;
                    }
                    selectedSymbol = string.Empty;
                }
            }
            standByControl.IsShown = false;
            standByControl.Visible = false;
        }

        /// <summary>
        /// склеить свечи m1, прочитанные из БД либо из файла с теми,
        /// что хранятся в AtomCandleStorage
        /// </summary>        
        private static void MergeQuotes(string smb, List<CandleData> quotes)
        {
            // прочитать котировки из файла (локальный кеш)
            var filePath = string.Format("{0}{1}\\{2}.quote", 
                ExecutablePath.ExecPath, TerminalEnvironment.QuoteCacheFolder, smb);
            var srcQuotes = AtomCandleStorage.Instance.GetAllMinuteCandles(smb);
            
            // склеить их с новыми котировками
            var newQuotes = quotes;
            if (srcQuotes != null && srcQuotes.Count > 0)
            {
                if (!CandleData.MergeCandles(ref srcQuotes, quotes, true)) 
                    return; // обновление не требуется
                newQuotes = srcQuotes;
            }

            // обновить хранилище
            AtomCandleStorage.Instance.RewriteCandles(smb, newQuotes);
            
            // сохранить склеенные котировки в файл
            try
            {
                CandleData.SaveInFile(filePath, smb, newQuotes);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(Localizer.GetString("MessageErrorSavingQuotesInFileFmt"), filePath, ex);
                MessageBox.Show(string.Format(Localizer.GetString("MessageErrorSavingQuotesInFileFmt"),
                                              filePath, ex.Message),
                                              Localizer.GetString("TitleError"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadQuotesFromFile(SymbolArchiveInfo smbInf, bool promptAndUpdateCharts)
        {
            // подкачать котировки из файла
            // ReSharper disable LocalizableElement
            var dlg = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Title = "Выберите файл котировок " + smbInf.Symbol,
                    DefaultExt = "quote",
                    Filter = "Все файлы|*.*|Котировки (*.quote)|*.quote"
                };
            // ReSharper restore LocalizableElement

            if (dlg.ShowDialog() != DialogResult.OK) return;
            List<CandleData> quotes;
            try
            {
                quotes = CandleData.LoadFromFile(dlg.FileName, smbInf.Symbol);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения котировок из файла \"{0}\": {1}",
                                   dlg.FileName, ex);
                
                MessageBox.Show(string.Format(Localizer.GetString("MessageErrorReadingQuotes") + ": \"{0}\"", ex.Message),
                                Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MergeQuotes(smbInf.Symbol, quotes);

            if (promptAndUpdateCharts)
            {
                if (MessageBox.Show(this, Localizer.GetString("MessageUpdateChartsBy") + " " + smbInf.Symbol + "?", 
                    Localizer.GetString("TitleConfirmation"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    MainForm.Instance.ReopenChartsSafe(smbInf.Symbol);
            }
        }

        /// <summary>
        /// над выбранными тикерами произвести определенные действия
        /// </summary>        
        private void BtnExecuteClick(object sender, EventArgs e)
        {
            var symbols = grid.GetRowValues<SymbolArchiveInfo>(false).Where(s => s.Selected).ToList();
            if (symbols.Count == 0) return;

            var defaultTime = DateTime.Now.Date.AddDays(-356);
            switch (((EnumItem<Action>) cbAction.SelectedItem).Value)
            {
                case Action.Actualize:
                    MainForm.Instance.UpdateTickersCacheForRobotsChooseIntervals(
                        symbols.ToDictionary(s => s.Symbol, s => s.DateStart ?? defaultTime), true, true);
                    break;
                case Action.ClearAndReload:
                    // очистить кэш...
                    foreach (var ticker in symbols)
                    {
                        AtomCandleStorage.Instance.ClearHistory(ticker.Symbol, 
                            ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder);
                    }
                    // актуализировать
                    MainForm.Instance.UpdateTickersCacheForRobotsChooseIntervals(
                        symbols.ToDictionary(s => s.Symbol, s => s.DateStart ?? defaultTime), true, true);
                    break;
                case Action.ReadFromFile:
                    foreach (var ticker in symbols)
                        ReloadQuotesFromFile(ticker, false);
                    break;
            }
            // обновить таблицу
            UpdateQuoteArchiveGrid();
            // предложить обновить графики...
        }
    }    
}
