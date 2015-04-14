using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class GoToForm : Form
    {
        #region Начальные настройки
        private int start, stop;
        private DateTime startTime, stopTime;
        private DateTime minTime, maxTime;
        private int totalData;
        #endregion

        #region Результат выбора
        public Point? CandleBounds
        {
            get
            {
                if (tabControl.SelectedTab == pageCenter)
                {
                    if (!tbCenterCandle.Enabled) return null;
                    var center = tbCenterCandle.Text.ToIntSafe();
                    if (!center.HasValue) return null;
                    var range = stop - start;
                    var a = center.Value - range / 2;
                    if (a < 0) a = 0;
                    var b = a + range;
                    if (b >= totalData) b = totalData - 1;
                    return new Point(a, b);
                }
                return null;
            }
        }

        public Cortege2<DateTime, DateTime>? TimeBounds
        {
            get
            {
                if (tabControl.SelectedTab == pageCenter)
                {
                    if (tbCenterCandle.Enabled) return null;
                    var center = dpCenterTime.Value;
                    var range = (int)(stopTime - startTime).TotalMinutes;
                    var a = center.AddMinutes(-range / 2);
                    if (a < minTime) a = minTime;
                    var b = center.AddMinutes(range / 2);
                    if (b >= maxTime) b = maxTime;
                    return new Cortege2<DateTime, DateTime>(a, b);
                }
                // указан диапазон
                if (dpFrom.Value == dpTo.Value) return null;
                return dpFrom.Value > dpTo.Value 
                    ? new Cortege2<DateTime, DateTime>(dpTo.Value, dpFrom.Value) 
                    : new Cortege2<DateTime, DateTime>(dpFrom.Value, dpTo.Value);
            }
        }
        #endregion

        public GoToForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public GoToForm(int start, int stop, DateTime startTime, DateTime stopTime,
            int totalData, DateTime minTime, DateTime maxTime) : this()
        {
            this.minTime = minTime;
            this.maxTime = maxTime;
            this.totalData = totalData;
            this.start = start;
            this.stop = stop;
            this.startTime = startTime;
            this.stopTime = stopTime;
            dpFrom.MinDate = minTime;
            dpFrom.MaxDate = maxTime;
            dpTo.MinDate = minTime;
            dpTo.MaxDate = maxTime;

            if (startTime > dpFrom.MaxDate)
                dpFrom.Value = dpFrom.MaxDate;
            else if (startTime < dpFrom.MinDate)
                dpFrom.Value = dpFrom.MinDate;
            else dpFrom.Value = startTime;            

            if (stopTime > dpTo.MaxDate)
                dpTo.Value = dpTo.MaxDate;
            else if (stopTime < dpTo.MinDate)
                dpTo.Value = dpTo.MinDate;
            else dpTo.Value = stopTime;
            
            
            tbCenterCandle.Text = ((start + stop) / 2).ToString();
            dpCenterTime.Value = startTime.AddMinutes((int)((stopTime - startTime).TotalMinutes / 2));

            lblRangeCandle.Text = string.Format("[{0} - {1}]", 0, totalData - 1);
            lblRangeTime.Text = string.Format("[{0:dd.MM.yyyy HH:mm} - {1:dd.MM.yyyy HH:mm}]", minTime, maxTime);
        }

        #region Чекбоксы
        private void CbCenterDateCheckedChanged(object sender, EventArgs e)
        {
            cbCenterCandle.Checked = !cbCenterDate.Checked;
            dpCenterTime.Enabled = cbCenterDate.Checked;
            tbCenterCandle.Enabled = cbCenterCandle.Checked;
        }

        private void CbCenterCandleCheckedChanged(object sender, EventArgs e)
        {
            cbCenterDate.Checked = !cbCenterCandle.Checked;
            dpCenterTime.Enabled = cbCenterDate.Checked;
            tbCenterCandle.Enabled = cbCenterCandle.Checked;
        }
        #endregion
    }
}
