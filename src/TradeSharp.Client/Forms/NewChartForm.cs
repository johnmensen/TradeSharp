using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class NewChartForm : Form
    {
        public NewChartForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public string TemplateName { get; set; }

        public BarSettings Timeframe { get; private set; }
        
        public string Ticker { get; private set; }

        public int? DaysInRequest
        {
            get
            {
                if (!intervalModeIsOn || daysInHistoryDefault == null) return null;
                var chosenValue = (int) tbDaysInRequest.Value;
                return chosenValue == daysInHistoryDefault.Value ? (int?)null : chosenValue;
            }
        }

        private int? daysInHistoryDefault;

        private bool intervalModeIsOn;

        private void NewChartFormLoad(object sender, EventArgs e)
        {
            var timeframes =
                BarSettingsStorage.Instance.GetCollection().Select(
                    n => BarSettingsStorage.Instance.GetBarSettingsFriendlyName(n)).ToList();
            cbTimeframe.DataSource = timeframes;
            if (timeframes.Count > 4) cbTimeframe.SelectedIndex = 4;

            cbTicker.Initialize();
            if(cbTicker.Items.Count != 0)
                cbTicker.SelectedIndex = 0;

            cbTemplates.Items.Clear();
            cbTemplates.Items.AddRange(ChartTemplate.GetChartTemplateNames(cbTicker.Text));
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbTicker.Text))
            {
                MessageBox.Show(Localizer.GetString("MessageTickerNotChosen"));
                return;
            }

            if (!DalSpot.Instance.GetTickerNames().Contains(cbTicker.Text))
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageTickerNotFoundFmt"), 
                    cbTicker.Text));
                return;
            }

            Timeframe = BarSettingsStorage.Instance.GetBarSettingsByName(cbTimeframe.Text);
            Ticker = cbTicker.Text;
            intervalModeIsOn = panelTimeBounds.Visible;

            //Если выбран какой нибудь шаблон
            if (cbTemplates.SelectedIndex > -1) TemplateName = cbTemplates.Text;

            DialogResult = DialogResult.OK;            
        }

        private void BtnIntervalClick(object sender, EventArgs e)
        {
            var showPanel = !panelTimeBounds.Visible;
            if (showPanel)
                EnsureStartDateSet();
            Height = showPanel ? 210 : 170;
            panelTimeBounds.Visible = showPanel;
        }

        private void EnsureStartDateSet()
        {
            var ticker = (string) cbTicker.SelectedItem;
            
            // узнать, сколько дней хранится в AtomCandleStorage - либо в файле
            DateTime? timeStart = null;
            var range = AtomCandleStorage.Instance.GetDataRange(ticker);
            if (range.HasValue)
                timeStart = range.Value.a;
            if (!timeStart.HasValue)
            {
                var path = ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder +
                           "\\" + ticker + ".quote";
                timeStart = QuoteCacheManager.GetFirstDate(path);
            }

            // подставить дату
            var daysInRequest = ChartForm.DaysToQuotesRequest;
            if (timeStart.HasValue)
                daysInRequest = (int) (DateTime.Now - timeStart.Value).TotalDays;
            if (daysInRequest < ChartForm.MinDaysToQuotesRequest)
                daysInRequest = ChartForm.MinDaysToQuotesRequest;

            // инициализировать поле-счетчик дней
            tbDaysInRequest.Minimum = ChartForm.MinDaysToQuotesRequest;
            tbDaysInRequest.Maximum = Math.Max(daysInRequest, ChartForm.MaxDaysInQuotesRequest);
            tbDaysInRequest.Value = daysInRequest;
            tbDaysInRequest.ForeColor = Color.Black;

            daysInHistoryDefault = daysInRequest;
        }

        private void TbDaysInRequestValueChanged(object sender, EventArgs e)
        {
            if (daysInHistoryDefault.HasValue)
            {
                tbDaysInRequest.ForeColor = tbDaysInRequest.Value != daysInHistoryDefault.Value ? Color.Blue : Color.Black;
            }
        }

        private void CbTickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (panelTimeBounds.Visible)
                EnsureStartDateSet();

            cbTemplates.Items.Clear();
            cbTemplates.Items.AddRange(ChartTemplate.GetChartTemplateNames(cbTicker.Text));
        }
    }
}
