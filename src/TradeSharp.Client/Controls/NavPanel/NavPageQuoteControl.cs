using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPageQuoteControl : UserControl, INavPageContent
    {
        class QuoteDataRecord
        {
            /// <summary>
            /// 0 - вбок, 1 - вверх, 2 - вниз
            /// </summary>
            public int Direction { get; set; }

            public string Title { get; set; }

            private float bid;
            public float Bid
            {
                // ReSharper disable UnusedMember.Local
                get { return bid; }
                // ReSharper restore UnusedMember.Local
                set
                {
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (bid != 0)
                        Direction = value > bid ? 1 : value < bid ? 2 : 0;
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                    bid = value;
                }
            }

            public float Ask { get; set; }

            public QuoteDataRecord(string title)
            {
                Title = title;
            }            
        }

        private readonly QuotePoller quotePoller = new QuotePoller(1000);

        private int selectedRow = -1;

        public event Action<int> ContentHeightChanged;

        public NavPageQuoteControl()
        {
            InitializeComponent();
        }

        private void NavPageQuoteControlLoad(object snd, EventArgs e)
        {
            SetupGrid();
            FillGrid(QuoteTableSettings.Instance.GetSettings());

            MainForm.Instance.FormClosing += (sender, args) => quotePoller.StopPolling();
            QuoteTableSettings.Instance.SettingsAreUpdated += InstanceOnSettingsAreUpdated;
            quotePoller.StartPolling();
            quotePoller.OnQuoteHashUpdated += QuotePollerOnOnQuoteHashUpdated;
        }  

        private void InstanceOnSettingsAreUpdated(List<QuoteTableCellSettings> sets, object o)
        {
            if (o == this) return;
            Invoke(new Action<List<QuoteTableCellSettings>>(FillGrid), sets);
        }

        private void SetupGrid()
        {
            grid.Columns.Add(new FastColumn("Title", "Инстр.")
                {
                    ColumnWidth = 58,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn("Bid", "Bid")
            {
                ColumnWidth = 56,
                formatter = v => ((float)v).ToStringUniformPriceFormat(true)
            });
            grid.Columns.Add(new FastColumn("Ask", "Ask")
            {
                ColumnWidth = 56,
                formatter = v => ((float)v).ToStringUniformPriceFormat(true)
            });
            grid.Columns.Add(new FastColumn("Direction", "*")
            {
                ColumnWidth = 30,
                ImageList = imageListArrows
            });
            grid.CalcSetTableMinWidth();
            grid.SelectEnabled = false;
            grid.MouseDown += GridOnMouseDown;
            grid.MouseUp += GridOnMouseUp;
            grid.UserHitCell += GridOnUserHitCell;
        }

        private void GridOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Clicks > 1 && e.Button == MouseButtons.Left)
            {
                // вызвать диалог торговли
                var tickerInfo = ((QuoteDataRecord) grid.rows[rowIndex].ValueObject).Title;
                new OrderDlg(tickerInfo).ShowDialog();
            }
        }

        private void GridOnMouseUp(object sender, MouseEventArgs e)
        {
            if (selectedRow < 0) return;
            grid.Cursor = Cursors.Default;
            var selectedStart = selectedRow;
            selectedRow = -1;

            var cell = grid.GetCellUnderCursor(e.X, e.Y);
            if (cell == null) return;
            if (cell.Value.Y == selectedStart) return;

            // поменять строки местами
            var recStart = grid.rows[selectedStart].ValueObject;
            var recEnd = grid.rows[cell.Value.Y].ValueObject;
            grid.rows[selectedStart].Selected = false;
            grid.rows[cell.Value.Y].Selected = false;
            grid.UpdateRow(selectedStart, recEnd);
            grid.UpdateRow(cell.Value.Y, recStart);

            // перерисовать
            grid.InvalidateRow(selectedStart);
            grid.InvalidateRow(cell.Value.Y);
        }

        /// <summary>
        /// начать перетаскивание строки таблицы
        /// </summary>
        private void GridOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenu.Show(this, e.X, e.Y);
                return;
            }

            if (e.Button != MouseButtons.Left) return;
            var cell = grid.GetCellUnderCursor(e.X, e.Y);
            if (cell == null) return;

            grid.rows[cell.Value.Y].Selected = true;
            selectedRow = cell.Value.Y;
            grid.InvalidateRow(cell.Value.Y);
            grid.Cursor = Cursors.UpArrow;
        }

        public void FillGrid(List<QuoteTableCellSettings> sets)
        {
            grid.rows.Clear();

            // подстроить высоту
            var ht = sets.Count*grid.CellHeight + grid.CaptionHeight + 3;
            if (ht > 650) ht = 650;
            ContentHeightChanged(ht);
            
            var records = new List<QuoteDataRecord>();
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            foreach (var tickerSets in sets)
            {
                var rec = new QuoteDataRecord(tickerSets.Ticker);
                // отыскать значение для котировки
                QuoteData q;
                if (quotes.TryGetValue(rec.Title, out q))
                {
                    rec.Ask = q.ask;
                    rec.Bid = q.bid;
                }
                records.Add(rec);
            }
            grid.DataBind(records);
            
        }

        private void QuotePollerOnOnQuoteHashUpdated(List<string> names, List<QuoteData> quotes)
        {
            if (Visible && Parent.Visible && Height > 0)
                BeginInvoke(new Action<List<string>, List<QuoteData>>(UpdateTablePricesUnsafe),
                   names, quotes);
        }

        private void UpdateTablePricesUnsafe(List<string> names, List<QuoteData> quotes)
        {
            var records = grid.rows.Select(r => (QuoteDataRecord) r.ValueObject).ToList();
            
            // обновить соотв. строки таблицы
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var quote = quotes[i];
                
                // найти строку в таблице
                var recIndex = records.FindIndex(r => r.Title == name);
                if (recIndex < 0) continue;
                
                // обновить значение и саму строку
                records[recIndex].Bid = quote.bid;
                records[recIndex].Ask = quote.ask;
                grid.UpdateRow(recIndex, records[recIndex]);
                grid.InvalidateRow(recIndex);
            }
        }

        private void MenuitemSetsClick(object sender, EventArgs e)
        {
            var tickerPickDlg = new TickersSelectForm();
            if (tickerPickDlg.ShowDialog() == DialogResult.Cancel) return;

            QuoteTableSettings.Instance.UpdateSettings(tickerPickDlg.SelectedTickers.Select(t => 
                new QuoteTableCellSettings { Ticker = t.Name, Precision = t.Precision }).ToList(), null);
        }
    }
}
