using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class DateTimeDialog : Form
    {
        public DateTimeDialog()
        {
            InitializeComponent();
        }

        private void TimePickerValueChanged(object sender, EventArgs e)
        {
            chronometerWatch.ClearTimes();
            chronometerWatch.AddTime(timePicker.Value, Color.Black);
            chronometerWatch.AddTime(
                new DateTime(timePicker.Value.Year, timePicker.Value.Month, timePicker.Value.Day,
                             timePicker.Value.Minute / 5, timePicker.Value.Second, 0), Color.DimGray);
            chronometerWatch.AddTime(
                new DateTime(timePicker.Value.Year, timePicker.Value.Month, timePicker.Value.Day,
                             timePicker.Value.Second / 5, 0, 0), Color.DarkGray);
        }

        private void MonthCalendarDateChanged(object sender, DateRangeEventArgs e)
        {

        }
    }
}
