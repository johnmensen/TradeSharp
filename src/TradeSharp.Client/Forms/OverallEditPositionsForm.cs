using System;
using System.Windows.Forms;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class OverallEditPositionsForm : Form
    {
        private string symbol;
        private int side;
        public bool IsTakeUsed
        {
            get { return cbTake.Checked; }
        }

        public bool IsStopUsed
        {
            get { return cbStop.Checked; }
        }

        public float? PriceTake
        {
            get
            {
                return tbTake.Text.ToFloatUniformSafe();
            }
        }

        public float? PriceStop
        {
            get { return tbStop.Text.ToFloatUniformSafe(); }
        }

        public OverallEditPositionsForm()
        {
            InitializeComponent();
        }

        public OverallEditPositionsForm(string symbol, int side)
        {
            InitializeComponent();
            Text += symbol;
            this.symbol = symbol;
            this.side = side;
        }


        private void cbTake_CheckedChanged(object sender, EventArgs e)
        {
            tbTake.Enabled = cbTake.Checked;
        }

        private void cbStop_CheckedChanged(object sender, EventArgs e)
        {
            tbStop.Enabled = cbStop.Checked;
        }

        private void ApplyBtn_Click(object sender, EventArgs e)
        {
            if (cbTake.Checked && !string.IsNullOrEmpty(tbTake.Text) && PriceTake == null)
            {
                MessageBox.Show(Localizer.GetString("MessageTakeprofitIsIncorrect") + ": " + tbTake.Text, 
                    Localizer.GetString("TitleError"));
                return;
            }

            if (cbStop.Checked && !string.IsNullOrEmpty(tbStop.Text) && PriceStop == null)
            {
                MessageBox.Show(Localizer.GetString("MessageStoplossIsIncorrect") + ": " + tbStop.Text, 
                    Localizer.GetString("TitleError"));
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
