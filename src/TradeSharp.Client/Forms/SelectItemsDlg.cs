using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class SelectItemsDlg : Form
    {
        public List<string> selColumns = new List<string>();
        public SelectItemsDlg(List<string> columns, List<string> selColumns)
        {
            InitializeComponent();
            lbColumns.Items.AddRange(columns.ToArray());
            lbSelected.Items.AddRange(selColumns.ToArray());
            this.selColumns = selColumns;
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            lbColumns_DoubleClick(sender, e);
        }

        private void BtnUnselect_Click(object sender, EventArgs e)
        {
            var items = lbSelected.SelectedItems;
            
            while (items.Count > 0)
            {
                selColumns.Remove((string)items[0]);
                lbSelected.Items.Remove(items[0]);
            }
        }

        private void lbColumns_DoubleClick(object sender, EventArgs e)
        {
            if (lbColumns.SelectedIndex == -1) return;
            var items = lbColumns.SelectedItems;
            foreach (var item in items.Cast<object>().Where(item => !selColumns.Contains(item)))
            {
                selColumns.Add((string)item);
                lbSelected.Items.Add(item);
            }
        }

        private void lbSelected_DoubleClick(object sender, EventArgs e)
        {
            BtnUnselect_Click(sender, e);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
