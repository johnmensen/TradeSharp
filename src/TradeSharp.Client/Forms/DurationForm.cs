using System;
using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class DurationForm : Form
    {
        private int maxTimeMinutes;

        private int duration;
        public int Duaration
        {
            get { return duration; }
        }

        public DurationForm(int maxTimeMinutes, int valueMinutes = 0)
        {
            InitializeComponent();

            this.maxTimeMinutes = maxTimeMinutes;
            hoursNumericUpDown.Value = valueMinutes / 60;
            minutesNumericUpDown.Value = valueMinutes % 60;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            var hours = (int) hoursNumericUpDown.Value;
            var minutes = (int) minutesNumericUpDown.Value;
            errorProvider.SetError(hoursNumericUpDown, "");
            errorProvider.SetError(minutesNumericUpDown, "");
            duration = hours * 60 + minutes;
            if (duration == 0 || duration > maxTimeMinutes)
            {
                errorProvider.SetError(hoursNumericUpDown, "Вне допустимого диапазона");
                errorProvider.SetError(minutesNumericUpDown, "Вне допустимого диапазона");
            }
            else
                DialogResult = DialogResult.OK;
        }
    }
}
