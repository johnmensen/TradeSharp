using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.UI.Util.Control
{
    public class FinanceLabel : Label
    {
        // объем
        private double amount;
        public double Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                UpdateText();
            }
        }

        private bool useMoneyFormat;
        public bool UseMoneyFormat
        {
            get { return useMoneyFormat; }
            set
            {
                useMoneyFormat = value;
                UpdateText();
            }
        }

        private bool needCents;
        public bool NeedCents
        {
            get { return needCents; }
            set
            {
                needCents = value;
                UpdateText();
            }
        }

        private string format = "g";
        public string Format
        {
            get { return format; }
            set
            {
                format = value;
                UpdateText();
            }
        }

        private string suffix;
        public string Suffix
        {
            get { return suffix; }
            set
            {
                suffix = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            Text = (useMoneyFormat ? amount.ToStringUniformMoneyFormat(needCents) : amount.ToString(format)) + suffix;
            ForeColor = Math.Abs(amount - 0) < double.Epsilon ? Color.Black : (amount < 0 ? Color.Red : Color.Blue);
        }
    }
}
