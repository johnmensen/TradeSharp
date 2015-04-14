using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FastGrid
{
    public partial class GridDropDialog : Form
    {
        private FastGrid grid;

        public FastGridCombo.GridObjectSelectedDel gridObjectSelected;

        public GridDropDialog()
        {
            InitializeComponent();
        }

        public GridDropDialog(int left, int top, int width, int height,
            int minTableWidth, int fixedTableWidth,
            Color cellBackColor, Color altBackColor, Color fontColor,
            IEnumerable<FastColumn> columns, IList boundObjects, object selectedObj)
        {
            InitializeComponent();
            grid = new FastGrid {Dock = DockStyle.Fill, Parent = this};
            Controls.Add(grid);

            Text = string.Empty;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ControlBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            Capture = true;

            grid.Columns.AddRange(columns);
            if (minTableWidth > 0) grid.MinimumTableWidth = minTableWidth;
            if (fixedTableWidth > 0) width = fixedTableWidth;
            grid.ColorAltCellBackground = altBackColor;
            grid.ColorCellBackground = cellBackColor;
            grid.ColorCellFont = fontColor;
            grid.DataBind(boundObjects);
            if (selectedObj != null)
            {
                foreach (var row in grid.rows)
                {
                    if (row.ValueObject == selectedObj)
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }
            grid.UserHitCell += GridUserHitCell;
            
            SetBounds(left, top, width, height);            
        }

        private void GridUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            if (rowIndex >= 0 && rowIndex < grid.rows.Count)
                gridObjectSelected(grid.rows[rowIndex].ValueObject);
        }

        private void GridDropDialogDeactivate(object sender, System.EventArgs e)
        {
            Close();
        }       
    }
}
