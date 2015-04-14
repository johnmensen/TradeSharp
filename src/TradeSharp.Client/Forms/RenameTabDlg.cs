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
    public partial class RenameTabDlg : Form
    {
        public string TabName { get; set; }
        public RenameTabDlg()
        {
            InitializeComponent();
        }

        private void RenameTabDlg_Load(object sender, EventArgs e)
        {
            tbTabName.Text = TabName;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            TabName = tbTabName.Text;
            DialogResult = DialogResult.OK;
        }

        private void tbTabName_TextChanged(object sender, EventArgs e)
        {
            tbTabName.Text = tbTabName.Text.Replace("#", "");
        }
    }
}
