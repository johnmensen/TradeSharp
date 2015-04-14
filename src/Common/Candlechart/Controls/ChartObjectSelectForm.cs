using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Core;
using FastGrid;

namespace Candlechart.Controls
{
    public partial class ChartObjectSelectForm : Form
    {
        public IChartInteractiveObject SelectedObject
        {
            get 
            { 
                var row = grid.rows.FirstOrDefault(r => r.Selected);
                if (row == null) return null;
                return (IChartInteractiveObject) row.ValueObject;
            }
        }

        public ChartObjectSelectForm()
        {
            InitializeComponent();
        }

        public ChartObjectSelectForm(List<IChartInteractiveObject> objects)
        {
            InitializeComponent();
            SetupGrid();
            grid.DataBind(objects, typeof(IChartInteractiveObject));
        }

        private void SetupGrid()
        {
            grid.ColorAltCellBackground = Color.FromArgb(240, 240, 240);
            grid.ColorCellBackground = Color.White;
            grid.Columns.Add(new FastColumn("ClassName", "Тип")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 50
                });
            grid.Columns.Add(new FastColumn("Name", "Название") {ColumnMinWidth = 50});
            grid.Columns.Add(new FastColumn("IndexStart", "Индекс")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 50
                });
            grid.MinimumTableWidth = 50 * grid.Columns.Count;
            grid.UserHitCell += GridUserHitCell;
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (rowIndex < 0 || rowIndex >= grid.rows.Count) return;
            if (e.Button == MouseButtons.Left && e.Clicks > 1)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            //if (grid.rows[rowIndex].Selected)
            //    grid.rows[rowIndex].Selected = false;             
        }
    }
}
