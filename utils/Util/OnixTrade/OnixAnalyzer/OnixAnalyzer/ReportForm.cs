using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OnixAnalyzer.BL;

namespace OnixAnalyzer
{
    public partial class ReportForm : Form
    {
        public string PathToSave
        {
            get; set;
        }
        public ReportForm()
        {
            InitializeComponent();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            var resultStr = Statistics.BuildStatistics(int.Parse(tbDealsMin.Text), int.Parse(tbHistoryPercent.Text), PathToSave);
            tbResults.Text = resultStr;
        }
    }
}
