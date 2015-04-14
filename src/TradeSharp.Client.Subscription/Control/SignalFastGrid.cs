using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class SignalFastGrid : UserControl
    {
        public event Action<TradeSignalUpdate> SignalUpdateSelected;

        public SignalFastGrid()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void BindData(List<TradeSignalUpdate> signals)
        {
            grid.DataBind(signals);
        }

        private void SetupGrid()
        {
            var blank = new TradeSignalUpdate(0, "");
            // PerformerStat
            grid.Columns.Add(new FastColumn(blank.Property(p => p.CategoryName), Localizer.GetString("TitleSignal"))
                {
                    ColumnWidth = 160,
                    SortOrder = FastColumnSort.Descending,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = new Font(Font, FontStyle.Bold)
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Ticker), Localizer.GetString("TitleInstrument"))
                {
                    ColumnWidth = 100
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.TimeframeFriendlyName), Localizer.GetString("TitleTimeframe"))
                {
                    ColumnWidth = 100
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.TimeUpdated), Localizer.GetString("TitleUpdateTime"))
                {
                    ColumnMinWidth = 80
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.ObjectCount), Localizer.GetString("TitleObjectCountShort"))
                {
                    ColumnWidth = 90
                });
            grid.ColorAltCellBackground = Color.White;
            grid.UserHitCell += GridUserHitCell;
            grid.MultiSelectEnabled = true;
            grid.CalcSetTableMinWidth();
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var sets = (TradeSignalUpdate)grid.rows[rowIndex].ValueObject;
            if (col.PropertyName == sets.Property(p => p.CategoryName) && e.Button == MouseButtons.Left)
            {
                if (SignalUpdateSelected != null)
                    SignalUpdateSelected(sets);
            }
        }
    }
}
