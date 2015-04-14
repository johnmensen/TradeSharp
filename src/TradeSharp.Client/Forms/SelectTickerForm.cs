using System;
using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class SelectTickerForm : Form
    {
        public SelectTickerForm()
        {
            InitializeComponent();
        }

        public string SelectedTicker
        {
            get { return cbAllTickers.Text; }
        }

        private void SelectTickerFormLoad(object sender, EventArgs e)
        {
            cbAllTickers.Initialize();
            if(cbAllTickers.Items.Count > 0)
                cbAllTickers.SelectedIndex = 0;
        }
    }
}
