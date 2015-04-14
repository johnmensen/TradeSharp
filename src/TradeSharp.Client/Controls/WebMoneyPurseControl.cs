using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Client.BL.PaymentSystem;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class WebMoneyPurseControl : UserControl
    {
        public Action<WebMoneyPurseControl> closeClicked;

        public WebMoneyPurseControl()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
            cbPurseCurrency.SelectedIndex = 0;
        }

        public string PurseName
        {
            get { return PurseIsValid() ? cbPurseCurrency.Text + tbPurse.Text : string.Empty; }
            set
            {
                var namePreffix = 'R';
                if (!string.IsNullOrEmpty(value))
                {
                    namePreffix = Char.ToUpper(value[0]);
                    if (Char.IsSymbol(namePreffix))
                    {
                        value = value.Substring(1, value.Length - 1);
                        var indexChar = cbPurseCurrency.Items.IndexOf(namePreffix.ToString());
                        if (indexChar >= 0)
                            cbPurseCurrency.SelectedIndex = indexChar;
                    }
                }

                tbPurse.Text = value;
            }
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(cbPurseCurrency.Text); }
        }

        private void TbPurseTextChanged(object sender, EventArgs e)
        {
            lblValid.ForeColor = PurseIsValid() ? SystemColors.ControlText : Color.Red;
        }

        private bool PurseIsValid()
        {
            return WebMoneyValidator.PusrseIdIsValid(tbPurse.Text);
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            if (closeClicked != null)
                closeClicked(this);
        }
    }
}
