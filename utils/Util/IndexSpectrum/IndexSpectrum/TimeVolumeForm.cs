using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace IndexSpectrum
{
    public partial class TimeVolumeForm : Form
    {
        public TimeVolumeForm()
        {
            InitializeComponent();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbFileName.Text)) return;
            DateTime? startTime = cbUseStart.Checked ? (DateTime?)dpStart.Value : null;

            var dateVolm = new Dictionary<DateTime, int>();
            int total = 0;
            
            using (var sr = new StreamReader(tbFileName.Text))
            {
                while (!sr.EndOfStream)
                {
                    // 1999.10.01,00:00,1.06790,1.06850,1.06360,1.06830,295
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length != 7) continue;
                    
                    var time = DateTime.ParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture);
                    var volume = int.Parse(parts[6]);
                    if (!dateVolm.ContainsKey(time))
                        dateVolm.Add(time, volume);
                    else
                        dateVolm[time] = dateVolm[time] + volume;
                    total += volume;
                }
            }
            var sb = new StringBuilder();
            foreach (var time in dateVolm.Keys.OrderBy(k => k))
            {
                var timeStr = time.ToString("HH:mm");
                var percent = dateVolm[time] * 100.0 / total;
                sb.AppendLine(string.Format("{1}{0}{2}", (char) 9, 
                    timeStr, percent.ToString().Replace('.', ',')));
            }
            Clipboard.SetText(sb.Length == 0 ? "empty" : sb.ToString());
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                tbFileName.Text = openFileDialog.FileName;
        }
    }
}
