using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class PublishPreSignalWindow : Form
    {
        public PublishPreSignalWindow()
        {
            InitializeComponent();
            cbType.Text = "Limit";
            foreach (var sets in BarSettingsStorage.Instance.GetCollection())
                tbSignal.Items.Add(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(sets));
            cbTicker.Items.AddRange(DalSpot.Instance.GetTickerNames());
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

        public float Price
        {
            get { return tbPrice.Text.ToFloatUniform(); }
            set { tbPrice.Text = value.ToStringUniform(); }
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

        public string Type
        {
            get { return cbType.Text; }
        }

        public bool IsPriceUsing
        {
            get { return cbPrice.Checked; }
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            var errorStr = new List<string>();
            if (string.IsNullOrEmpty(Ticker)) errorStr.Add(Localizer.GetString("TitleTicker"));
            if (string.IsNullOrEmpty(Signal)) errorStr.Add(Localizer.GetString("TitleSignal"));
            if (string.IsNullOrEmpty(TypeOrder)) errorStr.Add(Localizer.GetString("TitleOrder"));
            if (string.IsNullOrEmpty(Type)) errorStr.Add(Localizer.GetString("TitleType"));
            if (cbPrice.Checked && string.IsNullOrEmpty(tbPrice.Text)) errorStr.Add(Localizer.GetString("TitlePriceLevel"));
            
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
