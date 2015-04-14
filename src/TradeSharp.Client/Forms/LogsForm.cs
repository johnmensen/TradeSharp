using System;
using System.Windows.Forms;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class LogsForm : Form
    {
        public LogsForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private void PerformEventRobotsLog(string log)
        {
            BeginInvoke(new AddLogDel(UpdateLog), log);
        }

        private delegate void AddLogDel(string txt);

        private void UpdateLog(string log)
        {
            tbRobotsLog.Text = log + tbRobotsLog.Text;
        }

        private void LogsForm_Load(object sender, EventArgs e)
        {
            TerminalLog.Instance.RobotsLogEvent += PerformEventRobotsLog;
            TerminalLog.Instance.GetRobotsLog();
        }

        private void LogsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            TerminalLog.Instance.RobotsLogEvent -= PerformEventRobotsLog;
        }
    }
}
