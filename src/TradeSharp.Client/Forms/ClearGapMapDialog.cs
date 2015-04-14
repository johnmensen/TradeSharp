using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ClearGapMapDialog : Form
    {
        #region Класс описывает записи гэпов по инструменту
        class GapsByTicker
        {
            public string Ticker { get; set; }

            public string GapsSummaryStr { get; set; }

            public string GapLongestStr { get; set; }

            public int GapsCount { get; set; }

            public bool Selected { get; set; }

            public GapsByTicker()
            {                
            }

            public GapsByTicker(string ticker, GapMapRecord rec)
            {
                Ticker = ticker;
                
                if (rec.serverGaps != null && rec.serverGaps.Count > 0)
                {
                    GapsCount = rec.serverGaps.Count;
                    var sumMinutes = rec.serverGaps.Sum(g => g.TotalMinutes);
                    GapsSummaryStr = new TimeSpan(0, (int)sumMinutes, 0).ToStringUniform(false, false);

                    var longestMinutes = rec.serverGaps.Max(g => g.TotalMinutes);
                    GapLongestStr = new TimeSpan(0, (int)longestMinutes, 0).ToStringUniform(false, false);
                }
            }
        }
        #endregion

        private readonly List<string> initiallySelectedTickers;

        public ClearGapMapDialog()
        {
            InitializeComponent();
            SetupGrid();
        }

        public ClearGapMapDialog(List<string> initiallySelectedTickers) : this()
        {
            this.initiallySelectedTickers = initiallySelectedTickers;
        }

        private void SetupGrid()
        {
            grid.Columns.Add(new FastColumn("Selected", "*")
                {
                    SortOrder = FastColumnSort.Descending,
                    ImageList = imageList,
                    ColumnWidth = 42,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            grid.Columns.Add(new FastColumn("Ticker", "Инструмент")
                {
                    ColumnWidth = 100,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn("GapsCount", "Гэпов")
            {
                ColumnWidth = 70
            });
            grid.Columns.Add(new FastColumn("GapsSummaryStr", "Гэпов (часов)")
            {
                ColumnWidth = 100
            });
            grid.Columns.Add(new FastColumn("GapLongestStr", "Макс. гэп")
            {
                ColumnWidth = 100
            });
            
            grid.CalcSetTableMinWidth();
            grid.UserHitCell += GridOnUserHitCell;
        }

        private void GridOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (col.PropertyName == "Selected" && e.Button == MouseButtons.Left)
            {
                var obj = (GapsByTicker) (grid.rows[rowIndex].ValueObject);
                obj.Selected = !obj.Selected;
                grid.UpdateRow(rowIndex, obj);
                grid.InvalidateCell(col, rowIndex);
                return;
            }
        }

        private void ClearGapMapDialogLoad(object sender, EventArgs e)
        {
            var boundList = new List<GapsByTicker>();

            // загрузить карту гэпов
            foreach (var ticker in DalSpot.Instance.GetTickerNames())
            {
                var record = GapMap.Instance.GetServerGaps(ticker);
                if (record == null) continue;
                var row = new GapsByTicker(ticker, record);
                if (initiallySelectedTickers != null)
                    row.Selected = initiallySelectedTickers.Contains(row.Ticker);
                boundList.Add(row);
            }

            grid.DataBind(boundList);
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            // очистить выбранные записи
            var selected = grid.GetRowValues<GapsByTicker>(false).Where(r => r.Selected).ToList();
            if (selected.Count == 0) return;

            foreach (var ticker in selected)
            {
                GapMap.Instance.ClearGaps(ticker.Ticker);
            }

            GapMap.Instance.SaveToFile();
            DialogResult = DialogResult.OK;
        }
    }
}
