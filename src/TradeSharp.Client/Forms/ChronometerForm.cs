using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class ChronometerForm : Form
    {
        public BarSettings Data
        {
            get
            {
                var result = new BarSettings();
                result.Title = titleTextBox.Text;
                result.StartMinute = startDateTimePicker.Value.Hour * 60 + startDateTimePicker.Value.Minute;
                for (var i = 0; i < durationsListBox.Items.Count; i++)
                    result.Intervals.Add(GetDuration(i));
                return result;
            }
            set
            {
                titleTextBox.Text = value.Title;
                startDateTimePicker.Value = DateTime.Parse("01/01/2012").AddMinutes(value.StartMinute);
                durationsListBox.Items.Clear();
                foreach(var interval in value.Intervals)
                    durationsListBox.Items.Add(String.Format("{0}:{1:D2}", interval / 60, interval % 60));
                UpdateDurations();
            }
        }

        private readonly Color[] colors = {
                                              Color.Red, Color.Orange, Color.Yellow, Color.Green,
                                              Color.LightBlue, Color.Blue, Color.Violet
                                          };

        public ChronometerForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public void SetReadOnly(bool readOnly)
        {
            // okButton.Enabled сохраняет признак "только-чтение", что используется в дальнейшем
            okButton.Enabled = !readOnly;
            titleTextBox.ReadOnly = readOnly;
            startDateTimePicker.Enabled = !readOnly;
            addButton.Enabled = !readOnly;
        }

        private Color GetColor(int index)
        {
            return colors[index % colors.Length];
        }

        private int GetDuration(int index)
        {
            var words = durationsListBox.Items[index].ToString().Split(new[] { ':' });
            var hours = Convert.ToInt32(words[0]);
            var minutes = Convert.ToInt32(words[1]);
            return hours * 60 + minutes;
        }

        private int GetTotalDuration()
        {
            var result = 0;
            for (var i = 0; i < durationsListBox.Items.Count; i++)
                result += GetDuration(i);
            return result;
        }

        private void UpdateDurations()
        {
            chronometerWatch.ClearTimes();
            chronometerWatch.ClearIntervals();
            var startDate = DateTime.Parse("01/01/2012");
            var startTimeSpan = new TimeSpan(startDateTimePicker.Value.Hour, startDateTimePicker.Value.Minute, 0);
            chronometerWatch.AddTime(startDate + startTimeSpan, Color.Black);
            var periodDuration = GetTotalDuration();
            if (periodDuration == 0)
                return;
            var totalDuration = periodDuration;
            while (totalDuration <= 24 * 60)
            {
                for (var index = 0; index < durationsListBox.Items.Count; index++)
                {
                    var duration = GetDuration(index);
                    chronometerWatch.AddInterval(startDate + startTimeSpan, duration, GetColor(index));
                    startTimeSpan += new TimeSpan(duration / 60, duration % 60, 0);
                    chronometerWatch.AddTime(startDate + startTimeSpan, Color.Black);
                }
                totalDuration += periodDuration;
            }
        }

        private void AddButtonClick(object sender, EventArgs e)
        {
            var form = new DurationForm(24 * 60 - GetTotalDuration());
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            durationsListBox.Items.Add(String.Format("{0}:{1:D2}", form.Duaration / 60, form.Duaration % 60));
            UpdateDurations();
        }

        private void RemoveButtonClick(object sender, EventArgs e)
        {
            var index = durationsListBox.SelectedIndex;
            if (index < 0)
                return;
            durationsListBox.Items.RemoveAt(index);
            UpdateDurations();
        }

        private void DurationsListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            changeButton.Enabled = false;
            removeButton.Enabled = false;
            if (!okButton.Enabled)
                return;
            var index = durationsListBox.SelectedIndex;
            if (index < 0)
                return;
            changeButton.Enabled = true;
            removeButton.Enabled = true;
        }

        private void StartDateTimePickerValueChanged(object sender, EventArgs e)
        {
            UpdateDurations();
        }

        private void ChangeButtonClick(object sender, EventArgs e)
        {
            var index = durationsListBox.SelectedIndex;
            if (index < 0)
                return;
            var duration = GetDuration(index);
            var form = new DurationForm(24 * 60 - GetTotalDuration() + duration, duration);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            durationsListBox.Items[index] = String.Format("{0}:{1:D2}", form.Duaration / 60, form.Duaration % 60);
            UpdateDurations();
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            errorProvider.SetError(titleTextBox, "");
            errorProvider.SetError(durationsListBox, "");
            if (titleTextBox.Text == "")
            {
                errorProvider.SetError(titleTextBox, "Введите наименование");
                return;
            }
            var sum = GetTotalDuration();
            if(sum == 0)
            {
                errorProvider.SetError(durationsListBox, "Введите длительности");
                return;
            }
            if(24 * 60 % sum != 0)
            {
                var durations = new List<string>();
                var n = 1;
                while(true)
                {
                    var x = 24 * 60 / n - sum;
                    if (x > 0 && x < 60 && durations.Count == 0)
                    {
                        durations.Add(String.Format("0:{0:D2}", x));
                        break;
                    }
                    if(x < 60)
                        break;
                    durations.Add(String.Format("{0}:{1:D2}", x / 60, x % 60));
                    n++;
                }
                var text = Localizer.GetString("MessageIntervalLackedInChronometer");
                if(durations.Count == 0)
                    MessageBox.Show(this,
                        text, 
                        Localizer.GetString("TitleWarning"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                else
                {
                    var form = new ListSelectDialog();
                    form.Initialize(durations.Select(o => o as object), text + 
                        ".\n\n" + Localizer.GetString("MessageChronometerChoseLength") + ":");
                    if (form.ShowDialog(this) == DialogResult.Cancel)
                        return;
                    durationsListBox.Items.Add(form.SelectedItem);
                    UpdateDurations();
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void DurationsListBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (!okButton.Enabled)
                return;
            if (e.KeyCode == Keys.Delete)
                RemoveButtonClick(sender, null);
        }
    }
}
