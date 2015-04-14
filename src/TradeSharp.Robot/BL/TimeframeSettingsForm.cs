using System;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.BL
{
    public partial class TimeframeSettingsForm : Form
    {
        public BarSettings Timeframe { get; set; }

        public TimeframeSettingsForm()
        {
            InitializeComponent();
        }

        public TimeframeSettingsForm(BarSettings timeframe)
        {
            InitializeComponent();
            Timeframe = timeframe;
        }

        private void TimeframeSettingsFormLoad(object sender, EventArgs e)
        {
            foreach (var sets in BarSettingsStorage.Instance.GetCollection())
            {
                cbTimeframe.Items.Add(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(sets));
            }
            cbTimeframe.Text = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(Timeframe ?? 
                BarSettingsStorage.Instance.GetCollection()[0]);
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            var sets = BarSettingsStorage.Instance.GetBarSettingsByName(cbTimeframe.Text);
            if (sets == null)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageUnableParseTimeframeStringFmt"),
                                              cbTimeframe.Text), Localizer.GetString("TitleError"), 
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Timeframe = sets;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
