using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteAdmin.BL;
using TradeSharp.QuoteAdmin.Forms;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin
{
    public partial class MainForm : Form
    {
        private TickerInfo selectedTicker;

        public MainForm()
        {
            InitializeComponent();
            SetupGUI();
        }

        private void SetupGUI()
        {
            gridQuote.Columns.Add(new FastColumn("Title", "Котировка")
                                      {
                                          ColumnWidth = 66                                          
                                      });
            gridQuote.Columns.Add(new FastColumn("CodeFXI", "Код")
            {
                ColumnMinWidth = 38,
                SortOrder = FastColumnSort.Ascending
            });
            gridQuote.MultiSelectEnabled = true;
            gridQuote.CalcSetTableMinWidth();

            gridCandles.Columns.Add(new FastColumn("Time", "время")
                                        {
                                            FormatString = "dd.MM.yyyy HH:mm",
                                            SortOrder = FastColumnSort.Ascending,
                                            ColumnWidth = 94
                                        });
            gridCandles.Columns.Add(new FastColumn("Open", "open")
                                        {
                                            formatter = v => ((float) v).ToStringUniformPriceFormat(),
                                            ColumnMinWidth = 50
                                        });
            gridCandles.Columns.Add(new FastColumn("High", "high")
            {
                formatter = v => ((float)v).ToStringUniformPriceFormat(),
                ColumnMinWidth = 50
            });
            gridCandles.Columns.Add(new FastColumn("Low", "low")
            {
                formatter = v => ((float)v).ToStringUniformPriceFormat(),
                ColumnMinWidth = 50
            });
            gridCandles.Columns.Add(new FastColumn("Close", "close")
            {
                formatter = v => ((float)v).ToStringUniformPriceFormat(),
                ColumnMinWidth = 50
            });
        }

        private void RefreshTickers()
        {
            var tickers = TickerInfo.GetTickers();
            gridQuote.DataBind(tickers);
        }

        private void MainFormLoad(object sender, System.EventArgs e)
        {
            RefreshTickers();
        }

        private void GridQuoteUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selected = gridQuote.rows.Where(r => r.Selected).Select(r => (TickerInfo) r.ValueObject).ToList();
                menuQuote.Tag = selected.Count > 1
                                    ? selected
                                    : new List<TickerInfo> {(TickerInfo) gridQuote.rows[rowIndex].ValueObject};
                menuQuote.Show(gridQuote, e.X, e.Y);
                return;
            }

            selectedTicker = (TickerInfo)gridQuote.rows[rowIndex].ValueObject;
            tbActiveBase.Text = selectedTicker.ActiveBase;
            tbActiveCounter.Text = selectedTicker.ActiveCounter;
            tbFormat.Text = string.Format("{0:f" + selectedTicker.Precision + "}", 1.2345678);
        }

        private void BtnGetHistoryRangeClick(object sender, System.EventArgs e)
        {
            if (selectedTicker != null)
            if (selectedTicker.CodeFXI.HasValue)
            {
                var times = QuoteDataBase.GetFirstAndLastDateByTicker(selectedTicker.CodeFXI.Value);
                if (times != null)
                {
                    dpStartHist.Value = times.Value.a;
                    dpEndHist.Value = times.Value.b;
                }                    
                return;
            }
            dpStartHist.Value = DateTime.Now;
            dpEndHist.Value = DateTime.Now;
        }

        private void MenuitemLoadInDbClick(object sender, EventArgs e)
        {
            if (menuQuote.Tag == null) return;
            var items = (List<TickerInfo>) menuQuote.Tag;
            if (items.Count == 0) return;

            var dlg = new FillHistoryForm(items.Cast<TradeTicker>().ToList());
            dlg.ShowDialog();
        }

        private void BtnGetCandlesClick(object sender, EventArgs e)
        {
            if (selectedTicker == null) return;
            if (dpStartHist.Value == dpEndHist.Value) return;

            var count = tbCountCandles.Text.ToIntSafe() ?? 20;
            var symbol = selectedTicker.Title;
            var candles = QuoteDataBase.ReadTopNumCandles(symbol, count, dpStartHist.Value, dpEndHist.Value);
            gridCandles.DataBind(candles.Select(c => new CandleForTable(c)).ToList());
        }

        private void menuLoadFromCsv_Click(object sender, EventArgs e)
        {
            if (menuQuote.Tag == null) return;
            var items = (List<TickerInfo>)menuQuote.Tag;
            if (items.Count == 0) return;
            if (items.Count > 1)
            {
                MessageBox.Show("Выбрано более одного инструмента. Снимите выделение и повторите запрос");
                return;
            }
            var selectedTicker = items[0];
            if (!selectedTicker.CodeFXI.HasValue)
            {
                MessageBox.Show("Для инструмента " + selectedTicker.Title + " не задан код");
                return;
            }

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            var fileName = openFileDialog.FileName;
            if (!File.Exists(fileName))
                return;
            var linesCount = new CsvLoader().LoadFromCsv(fileName, selectedTicker.CodeFXI.Value);
            MessageBox.Show("Сохранено " + linesCount.ToStringUniformMoneyFormat() + " котировок");
        }
    }

    class CandleForTable
    {
        public DateTime Time { get; set; }

        public float Open { get; set; }

        public float High { get; set; }

        public float Low { get; set; }

        public float Close { get; set; }

        public CandleForTable() {}

        public CandleForTable(CandleData c)
        {
            Time = c.timeOpen;
            Open = c.open;
            High = c.high;
            Low = c.low;
            Close = c.close;
        }
    }
}
