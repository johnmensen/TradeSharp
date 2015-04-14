using System;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class TradeFromSLDlg : Form
    {
        public float StopLoss { get; private set; }

        public DealType Side { get; private set; }

        public TradeFromSLDlg()
        {
            InitializeComponent();
        }

        public TradeFromSLDlg(int tradeCount, DealType side, float? sl)
        {
            InitializeComponent();
            labelCountOpen.Text = tradeCount.ToString();
            cbSide.SelectedIndex = side == DealType.Buy ? 0 : 1;
            if (sl.HasValue) tbSL.Text = sl.Value.ToStringUniform(4);
        }

        private void BtnOKClick(object sender, EventArgs e)
        {
            var sl = tbSL.Text.Replace(",", ".").ToFloatUniformSafe() ?? 0;
            if (sl <= 0)
            {
                MessageBox.Show("Укажите ненулевой SL");
                return;
            }
            StopLoss = sl;
            Side = cbSide.SelectedIndex == 0 ? DealType.Buy : DealType.Sell;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
