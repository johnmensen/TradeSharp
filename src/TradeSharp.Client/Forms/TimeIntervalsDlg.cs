using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class TimeIntervalsDlg : Form
    {
        private readonly TimeIntervalControl scaleControl;

        private readonly Dictionary<string, TimeIntervalControl> timeControls = 
            new Dictionary<string, TimeIntervalControl>();

        public Dictionary<string, DateTime> TimeIntervals
        {
            get
            {
                return timeControls.ToDictionary(c => c.Key,
                                                 c => DateTime.Now.Date.AddDays(-c.Value.Value));
            }
        }

        private const int LineHeight = 22;

        private const int MinValue = 10, MaxValue = 365;

        public TimeIntervalsDlg()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public TimeIntervalsDlg(Dictionary<string, DateTime> intervals) : this()
        {
            var nowTime = DateTime.Now;
            var maxValue = Math.Max(MaxValue, intervals.Max(i => (int) Math.Round((nowTime - i.Value).TotalDays)));

            foreach (var inter in intervals)
            {
                var ctrl = new TimeIntervalControl(inter.Key, MinValue, maxValue,
                                                   (int) Math.Round((nowTime - inter.Value).TotalDays))
                               {
                                   Height = LineHeight,
                                   Dock = DockStyle.Top
                               };
                timeControls.Add(inter.Key, ctrl);
                panelContent.Controls.Add(ctrl);
            }
            
            if (intervals.Count > 1)
            {
                var defaultTotal = timeControls.Values.Mode(i => i.Value);
                // контрол, масштабирующий все остальные контролы
                scaleControl = new TimeIntervalControl(Localizer.GetString("TitleAll"), MinValue, maxValue, defaultTotal)
                    {
                        Height = LineHeight,
                        Dock = DockStyle.Top,
                        fontTitle = new Font(Font, FontStyle.Bold)
                    };
                scaleControl.ValueChangedByDragging += ScaleControlOnValueChangedByDragging;
                panelContent.Controls.Add(scaleControl);
            }
        }

        private void ScaleControlOnValueChangedByDragging(int val)
        {
            foreach (var control in timeControls)
            {
                control.Value.Value = val;
                control.Value.Invalidate();
            }
        }

        private void BtnResetToDefaultClick(object sender, EventArgs e)
        {
            foreach (var control in timeControls)
            {
                control.Value.Value = control.Value.DefaultValue;
                control.Value.Invalidate();
            }
            scaleControl.Value = scaleControl.DefaultValue;
            scaleControl.Invalidate();
        }

        private void BtnClearGapMapClick(object sender, EventArgs e)
        {
            new ClearGapMapDialog(timeControls.Keys.ToList()).ShowDialog();
        }
    }
}
