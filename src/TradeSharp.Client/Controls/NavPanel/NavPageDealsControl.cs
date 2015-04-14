using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPageDealsControl : UserControl, INavPageContent
    {
        public event Action<int> ContentHeightChanged;

        private static readonly Color colorAltCell = Color.FromArgb(230, 230, 230);

        private int lastHeight = 100;

        public NavPageDealsControl()
        {
            InitializeComponent();
            SetupGrid();
        }

        private void SetupGrid()
        {
            grid.Columns.Add(new FastColumn("Symbol", "Инстр.")
            {
                ColumnWidth = 65,
                SortOrder = FastColumnSort.Ascending
            });
            grid.Columns.Add(new FastColumn("Side", "Тип")
            {
                ColumnWidth = 45,
                formatter = v => (int)v > 0 ? "BUY" : (int)v < 0 ? "SELL" : "-",
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                    {
                        fontColor = null;
                        color = null;
                        if ((int) value > 0) fontColor = Color.Green;
                        else
                            if ((int)value < 0) fontColor = Color.Red;
                    }
            });
            grid.Columns.Add(new FastColumn("Volume", "Объем")
                {
                    ColumnWidth = 67,
                    formatter = v => (Math.Abs((int)v)).ToStringUniformMoneyFormat()
                });
            grid.Columns.Add(new FastColumn("Profit", "Прибыль")
            {
                ColumnWidth = 80,
                SortOrder = FastColumnSort.Ascending,
                formatter = v => ((float)v).ToStringUniformMoneyFormat(false),
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    fontColor = null;
                    color = null;
                    if ((float)value > 0) fontColor = Color.Green;
                    else
                        if ((float)value < 0) fontColor = Color.Red;
                }
            });
            grid.Columns.Add(new FastColumn("AveragePrice", "Цена")
            {
                ColumnWidth = 62,
                SortOrder = FastColumnSort.Ascending,
                formatter = v => ((float)v).ToStringUniformPriceFormat()
            });

            grid.SelectEnabled = false;
            grid.CalcSetTableMinWidth();
            grid.ColorAltCellBackground = colorAltCell;
            grid.StickLast = true;

            timer.Enabled = true;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (Visible && Width > 0 && Height > 0)
                UpdateTable();
        }

        private void UpdateTable()
        {
            try
            {
                var acc = AccountStatus.Instance.AccountData;
                if (acc == null) return;

                var orders = MarketOrdersStorage.Instance.MarketOrders ?? new List<MarketOrder>();
                var sumPos = PositionSummary.GetPositionSummary(orders, acc.Currency, (float)acc.Balance) 
                    ?? new List<PositionSummary>();

                Invoke(new Action<List<PositionSummary>>(lst => grid.DataBind(lst)), sumPos);

                var height = sumPos.Count * grid.CellHeight + grid.CaptionHeight + 2;
                if (grid.Width < grid.MinimumTableWidth) // учесть высоту скролбара
                    height += SystemInformation.HorizontalScrollBarHeight;

                if (height != lastHeight)
                {
                    lastHeight = height;
                    ContentHeightChanged(height);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateAccountInfo error", ex);
            }
        }        
    }
}
