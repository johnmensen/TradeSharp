using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPageAccountControl : UserControl, INavPageContent
    {
        public event Action<int> ContentHeightChanged;
        private bool heightWasSet;

        public NavPageAccountControl()
        {
            InitializeComponent();
            SetupGrid();
        }

        private void SetupGrid()
        {
            grid.BackColor = SystemColors.ButtonFace;
            grid.ColorAltCellBackground = SystemColors.ButtonHighlight;
            grid.ColorCellBackground = SystemColors.ButtonHighlight;
            grid.ColorSelectedCellBackground = SystemColors.ButtonFace;
            grid.CaptionHeight = 0;
            grid.SelectEnabled = false;
            
            grid.Columns.Add(new FastColumn("Name", "Название")
                {
                    ColumnMinWidth = 50
                });
            grid.Columns.Add(new FastColumn("Result", "Значение")
                {
                    ColumnMinWidth = 50
                });
            grid.CalcSetTableMinWidth();

            //UpdateTable();
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
                AccountStatusSummaryTable.UpdateAccountInfo(
                    list => Invoke(new Action<List<StatItem>>(lst => grid.DataBind(list)), list));
                if (!heightWasSet)
                {
                    heightWasSet = true;
                    ContentHeightChanged(grid.rows.Count*grid.CellHeight +
                                         grid.CaptionHeight + SystemInformation.HorizontalScrollBarHeight + 2);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateAccountInfo error", ex);
            }
        }

        
    }
}
