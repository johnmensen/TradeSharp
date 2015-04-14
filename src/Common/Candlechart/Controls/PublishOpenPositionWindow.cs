using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class PublishOpenPositionWindow : Form
    {
        public PublishOpenPositionWindow()
        {
            InitializeComponent();
            cbTicker.Items.AddRange(DalSpot.Instance.GetTickerNames());
        }

        public string Ticker
        {
            get { return cbTicker.Text; }
            set { cbTicker.Text = value; }
        }

        public decimal Price
        {
            get { return tbPrice.Text.ToDecimalUniform(); }
            set { tbPrice.Text = value.ToStringUniform(); }
        }

        public decimal? StopLoss
        {
            get 
            { 
                if (!cbStopLoss.Checked) return null;
                return tbStopLoss.Text.ToDecimalUniform(); 
            }
            set
            {
                cbStopLoss.Checked = true;
                tbStopLoss.Text = value.ToStringUniform();
            }
        }

        public int Side
        {
            get
            {
                if (string.IsNullOrEmpty(cbTypeOrder.Text)) return 0;
                return cbTypeOrder.Text == "Buy" ? 1 : -1;
            }
        }

        public decimal? TakeProfit
        {
            get
            {
                if (!cbTakeProfit.Checked) return null;
                return tbTakeProfit.Text.ToDecimalUniform();
            }
            set
            {
                cbTakeProfit.Checked = true;
                tbTakeProfit.Text = value.ToStringUniform();
            }
        }

        public DateTime Time
        {
            get { return dtTime.Value; }
            set { dtTime.Value = value; }
        }

        public string Commentary
        {
            get { return tbComment.Text; }
        }

        public string TypeOrder
        {
            get { return cbTypeOrder.Text; }
        }


        private void btnPublish_Click(object sender, EventArgs e)
        {
            var errorStr = new List<string>();
            if (string.IsNullOrEmpty(Ticker)) errorStr.Add(Localizer.GetString("TitleTicker"));
            if (string.IsNullOrEmpty(TypeOrder)) errorStr.Add(Localizer.GetString("TitleOrder"));
            if (string.IsNullOrEmpty(tbPrice.Text)) errorStr.Add(Localizer.GetString("TitlePriceEntry"));
            if (cbStopLoss.Checked && string.IsNullOrEmpty(tbStopLoss.Text)) errorStr.Add("StopLoss");
            if (cbTakeProfit.Checked && string.IsNullOrEmpty(tbTakeProfit.Text)) errorStr.Add("TakeProfit");

            if (errorStr.Count > 0)
            {
                MessageBox.Show(Localizer.GetString("MessageFieldsNotFilled") + ": " + string.Join(", ", errorStr));
                return;
            }   
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
