using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class ScriptPartDepoFormDialog : Form
    {
        public int Side
        {
            get { return (string)cbSide.SelectedItem == "SELL" ? -1 : 1; }
        }

        public decimal? SL
        {
            get { return tbSL.Text.Replace(',', '.').Trim().ToDecimalUniformSafe(); }
        }

        public decimal? TP
        {
            get { return tbTP.Text.Replace(',', '.').Trim().ToDecimalUniformSafe(); }
        }

        public decimal? Price
        {
            get { return tbPrice.Text.Replace(',', '.').Trim().ToDecimalUniformSafe(); }
        }
        
        public float[] Trailing
        {
            get
            {
                var price = tbTrailing.Text.Replace(",", ".").Trim().ToFloatUniformSafe() ?? 0;
                var target = tbTrailTarget.Text.Replace(",", ".").Trim().ToFloatUniformSafe() ?? 0;

                return new [] { price, target};
            }
        }

        public string TradeTicker
        {
            set { lblTradeTicker.Text = value; }
        }

        public ScriptPartDepoFormDialog()
        {
            InitializeComponent();
            cbSide.SelectedIndex = 0;
        }

        public ScriptPartDepoFormDialog(int side, float? bid, bool enablePriceField)
        {
            InitializeComponent();
            cbSide.SelectedIndex = side == 1 ? 0 : 1;
            if (bid.HasValue)
                tbPrice.Text = bid.Value.ToStringUniformPriceFormat();
            tbPrice.Enabled = enablePriceField;
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            var errors = new List<string>();
            if (tbPrice.Enabled && !Price.HasValue)
                errors.Add("Не указана цена. Пример допустимых значений: 90.81, 1.35625");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(", ", errors), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;            
        }
    }
}
