using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.QuoteTradeControls;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class QuoteTableControl : UserControl
    {
        private readonly List<QuoteTradeControl> cells =
            new List<QuoteTradeControl>();

        private int cellHeight = 18;
        public int CellHeight
        {
            get { return cellHeight; }
            set { cellHeight = value; }
        }

        private int columnsCount;
        private readonly object cellsLocker = new object();

        private EventHandler onClose;
        private EventHandler onPinDown;
        //private EventHandler onPinUp;
        private readonly QuotePoller quotePoller = new QuotePoller(1000);

        public event EventHandler OnClose
        {
            add { onClose += value; }
            remove { onClose -= value; }
        }

        //public event EventHandler OnPinUp
        //{
        //    add { onPinUp += value; }
        //    remove { onPinDown -= value; }
        //}

        public event EventHandler OnPinDown
        {
            add { onPinDown += value; }
            remove { onPinDown -= value; }
        }

        public QuoteTableControl()
        {
            InitializeComponent();
            QuoteTradeControl.imageCloseNomal = imageListCloseButton.Images[0];
            QuoteTradeControl.imageCloseLight = imageListCloseButton.Images[1];
            quotePoller.OnQuoteHashUpdated += quotePoller_OnQuoteHashUpdated;
            QuoteTableSettings.Instance.SettingsAreUpdated += InstanceOnSettingsAreUpdated;
        }

        private QuoteTradeControl CreateCell(string ticker, int precision, int cellSize)
        {
            return CreateCell(new QuoteTableCellSettings { Precision = precision, Ticker = ticker },
                              cellSize); 
        }

        private QuoteTradeControl CreateCell(QuoteTableCellSettings sets, int cellSize)
        {
            var cell = new QuoteTradeControl(sets, cellSize, selVolume =>
                {
                    var dic = UserSettings.Instance.FastDealSelectedVolumeDict;
                    if (!dic.ContainsKey(sets.Ticker)) dic.Add(sets.Ticker, selVolume);
                    else dic[sets.Ticker] = selVolume;
                    UserSettings.Instance.FastDealSelectedVolumeDict = dic;
                    UserSettings.Instance.SaveSettings();
                }, VolumeByTickerChangedDelegate)
                {
                    Parent = this, AllowDrop = true
                };
            cell.DragEnter += CellDragEnter;
            cell.DragDrop += CellDragDrop;
            cell.DragOver += CellDragOver;
            cell.MouseDown += CellMouseDown;
            cell.DragLeave += CellDragLeave;
            
            cell.OnQuoteTradeClick += (command, f, symbol) => MainForm.Instance.MakeTrade(command, symbol, (int) f,
                UserSettings.Instance.PromptTradeFromQuoteWindow);
            return cell;
        }

        private void VolumeByTickerChangedDelegate()
        {
            foreach (var cell in cells)
            {
                cell.VolumeByTickerUpdate();
            }
        }

        /// <summary>
        /// обновить котировки в таблице
        /// </summary>        
        private void quotePoller_OnQuoteHashUpdated(List<string> names, List<QuoteData> quotes)
        {
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                lock (cellsLocker)
                {
                    var cell = cells.FirstOrDefault(c => c.Text == name);
                    if (cell == null) continue;
                    UpdateCellSafe(cell, quotes[i]);
                }
            }
        }

        private delegate void UpdateCellDel(QuoteTradeControl cell, QuoteData quote);
        private static void UpdateCellUnsafe(QuoteTradeControl cell, QuoteData quote)
        {
            cell.SetPrice(quote.bid, quote.ask);
        }
        private static void UpdateCellSafe(QuoteTradeControl cell, QuoteData quote)
        {
            try
            {
                cell.BeginInvoke(new UpdateCellDel(UpdateCellUnsafe), cell, quote);
            }
            catch(Exception ex)
            {
                Logger.Error("QuoteTableControl.UpdateSellSafe: " + ex.Message);
            }
        }

        public void StartPolling()
        {
            quotePoller.StartPolling();
        }

        public void StopPolling()
        {
            quotePoller.OnQuoteHashUpdated -= quotePoller_OnQuoteHashUpdated;
            quotePoller.StopPolling();
        }

        public void LoadCells()
        {
            var cellStyle = UserSettings.Instance.QuoteCellSize;
            var trackValue = -1;
            foreach (var cell in QuoteTableSettings.Instance.GetSettings())
            {
                var name = cell.Ticker;
                // дубль?
                if (cells.Any(c => c.Text == name)) continue;
                var ctrl = CreateCell(cell, cellStyle);
                panelTable.Controls.Add(ctrl);
                ctrl.OnClosing += ctrl_OnClosing;
                trackValue = cellStyle;
                cells.Add(ctrl);
            }
            if (trackValue > 0)
                trackSize.Value = trackValue;
            ArrangeTiles();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            if (onClose != null) onClose(this, e);
        }

        private void BtnPinUpClick(object sender, EventArgs e)
        {
            if (onPinDown != null) onPinDown(this, e);
        }

        private void BtnChooseTickersClick(object sender, EventArgs e)
        {
            var tickerPickDlg = new TickersSelectForm();
            if (tickerPickDlg.ShowDialog() == DialogResult.Cancel) return;

            QuoteTableSettings.Instance.UpdateSettings(tickerPickDlg.SelectedTickers.Select(t =>
                new QuoteTableCellSettings { Ticker = t.Name, Precision = t.Precision }).ToList(), this);
        }

        private void InstanceOnSettingsAreUpdated(List<QuoteTableCellSettings> quoteTableCellSettings, object o)
        {
            try
            {
                Invoke(new Action<List<QuoteTableCellSettings>>(UpdateTickersUnsafe), quoteTableCellSettings);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void UpdateTickersUnsafe(List<QuoteTableCellSettings> quoteTableCellSettings)
        {
            lock (cellsLocker)
            {
                cells.Clear();
                panelTable.Controls.Clear();
                var cellSize = UserSettings.Instance.QuoteCellSize;
                foreach (var cell in quoteTableCellSettings)
                {
                    var ctrl = CreateCell(cell.Ticker, cell.Precision, cellSize);
                    ctrl.OnClosing += ctrl_OnClosing;
                    panelTable.Controls.Add(ctrl);
                    cells.Add(ctrl);
                }
            }

            // выстроить
            ArrangeTiles();
        }

        /// <summary>
        /// Обработчик события закрытия контрола для быстрой торговли
        /// </summary>
        /// <param name="sender">контрол, который нужно закрыть</param>
        private void ctrl_OnClosing(Control sender)
        {
            var ticker = ((QuoteTradeControl) sender).Ticker;
            QuoteTableSettings.Instance.ExcludeTicker(ticker, this);
            cells.RemoveAll(x => x.Ticker == ticker);
            panelTable.Controls.Remove(sender);
            // выстроить
            ArrangeTiles();
        }

        private void SaveCellsInfo()
        {
            var sets = new List<QuoteTableCellSettings>();
            lock (cellsLocker)
            {
                foreach (var cell in cells)
                {
                    sets.Add(new QuoteTableCellSettings
                                    {
                                        Ticker = cell.Text,
                                        Precision = cell.Precision
                                    });
                }
            }
            QuoteTableSettings.Instance.UpdateSettings(sets, this);
        }

        private void ArrangeTiles()
        {
            if (panelTable.Controls.Count == 0) return;
            var spec = panelTable.Controls[0];
            columnsCount = panelTable.Width / spec.Width;
            if (columnsCount < 1) columnsCount = 1;

            int column = 0, row = 0;
            foreach (var cellControl in cells)
            {
                var left = column*spec.Width;
                var top = row*spec.Height;
                column++;
                if (column >= columnsCount)
                {
                    column = 0;
                    row++;
                }
                lock (cellsLocker)
                {
                    cellControl.SetBounds(left, top, spec.Width, spec.Height);
                }
            }
        }

        private void QuoteTableControlLoad(object sender, EventArgs e)
        {
            LoadCells();
        }

        private void PanelTableResize(object sender, EventArgs e)
        {
            if (panelTable.Controls.Count == 0) return;
            var spec = panelTable.Controls[0];
            var colsCount = panelTable.Width / spec.Width;
            if (colsCount < 1) colsCount = 1;

            if (colsCount != columnsCount)
                ArrangeTiles();
        }

        void CellDragOver(object sender, DragEventArgs e)
        {
            var cell = sender as QuoteTradeControl;
            if (cell.CurrentDragDropState != DragDropState.Washy)
                cell.CurrentDragDropState = DragDropState.InFrame;
        }

        private void CellDragLeave(object sender, EventArgs e)
        {
            var cell = sender as QuoteTradeControl;
            if (cell.CurrentDragDropState == DragDropState.InFrame)
            {
                cell.CurrentDragDropState = DragDropState.Normal;
            }
        }

        private void CellDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }

        private void CellDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(QuoteTradeControl)))
            {
                var ctrlSrc = (QuoteTradeControl)e.Data.GetData(typeof(QuoteTradeControl));
                var ctrlTarget = sender as QuoteTradeControl;
                if (ctrlTarget == null) return;
                ctrlTarget.CurrentDragDropState = DragDropState.Normal;
                if (ctrlTarget == ctrlSrc) return;
                // поменять местами ячейки
                lock (cellsLocker)
                {
                    var indexSrc = cells.IndexOf(ctrlSrc);
                    var indexTarget = cells.IndexOf(ctrlTarget);
                    
                    cells[indexSrc] = ctrlTarget;
                    cells[indexTarget] = ctrlSrc;

                    var posTarget = new Point(ctrlTarget.Left, ctrlTarget.Top);
                    ctrlTarget.SetBounds(ctrlSrc.Left, ctrlSrc.Top, ctrlSrc.Width, ctrlSrc.Height);
                    ctrlSrc.SetBounds(posTarget.X, posTarget.Y, ctrlSrc.Width, ctrlSrc.Height);
                    ctrlSrc.CurrentDragDropState = DragDropState.Normal;
                   
                }
                SaveCellsInfo();
            }
        }
        
        private void CellMouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var cell = sender as QuoteTradeControl;

            //Начинаем перетаскивение только в том случае если был произведён клик левой кнопкой мыши и только по шапке контрола
            if (cell != null && e.Y < cell.CalculatedHaderHeight && e.Button == MouseButtons.Left)
            {
                cell.CurrentDragDropState = DragDropState.Washy;
                panelTable.DoDragDrop(sender, DragDropEffects.Link);
            }
            
        }

        private void TrackSizeScroll(object sender, EventArgs e)
        {
            // изменить формат клеток
            var wereUpdated = false;
            foreach (var cellControl in cells)
            {
                if (cellControl.CellSize != trackSize.Value)
                    wereUpdated = true;
                UserSettings.Instance.QuoteCellSize = cellControl.CellSize = trackSize.Value;    
            }
            if (!wereUpdated) return;
            
            // перекроить
            ArrangeTiles();
            Refresh();
        }
    }
}