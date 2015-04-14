using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class PublishForecastWindow : Form
    {
        public PublishForecastWindow()
        {
            InitializeComponent();
            tbSignal.Items.Clear();
            foreach (var sets in BarSettingsStorage.Instance.GetCollection())
                tbSignal.Items.Add(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(sets));            
        }

        public string Signal
        {
            get { return tbSignal.Text; }
            set { tbSignal.Text = value; }
        }

        public string Ticker
        {
            get { return cbTicker.Text; }
            set { cbTicker.Text = value; }
        }

        public string Commentary
        {
            get { return tbComment.Text; }
        }

        public bool Subscribers
        {
            get { return cbSubscribers.Checked; }
        }

        public bool Publishing
        {
            get { return cbPublishing.Checked; }
        }

        public bool IsPublishing
        {
            get { return cbTypePublishing.Text == cbTypePublishing.Items[0].ToString();  }
        }

        private void BtnPublishClick(object sender, EventArgs e)
        {
            var errorStr = new List<string>();
            if (string.IsNullOrEmpty(Ticker)) errorStr.Add(Localizer.GetString("TitleTicker"));
            if (string.IsNullOrEmpty(Signal)) errorStr.Add(Localizer.GetString("TitleSignal"));

            if (errorStr.Count > 0)
            {
                MessageBox.Show(Localizer.GetString("MessageFieldsNotFilled") + ": " + string.Join(", ", errorStr));
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void PublishForecastWindowLoad(object sender, EventArgs e)
        {
            var ticker = cbTicker.Text;
            cbTicker.DataSource = DalSpot.Instance.GetTickerNames();
            if (!string.IsNullOrEmpty(ticker)) cbTicker.Text = ticker;
            cbTypePublishing.Text = cbTypePublishing.Items[0].ToString();
        }

        private void CbTypePublishingSelectedIndexChanged(object sender, EventArgs e)
        {
            cbPublishing.Checked = cbTypePublishing.Text == cbTypePublishing.Items[0].ToString();
            cbSubscribers.Checked = cbTypePublishing.Text == cbTypePublishing.Items[0].ToString();
        }
    }
}
