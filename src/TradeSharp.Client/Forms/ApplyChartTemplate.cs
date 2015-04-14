using System;
using System.Windows.Forms;
using Candlechart;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ApplyChartTemplate : Form
    {
        /// <summary>
        /// Название текущего выбранного инструмента (валютной пары)
        /// </summary>
        private readonly string symbol;

        private CandleChartControl candleChart;

        public ApplyChartTemplate(CandleChartControl candleChartControl)
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            candleChart = candleChartControl;
            symbol = candleChartControl.Symbol;
            LoadTemplateNames();
        }

        private void ChbOnlyCurrentTickerTemplateCheckedChanged(object sender, EventArgs e)
        {
            LoadTemplateNames();
        }   
 
        private void BtnOkClick(object sender, EventArgs e)
        {
            candleChart.CurrentTemplateName = cbTemplateName.Text;
        }       

        /// <summary>
        /// Вспомогательный метод загрузки имён шаблонов
        /// </summary>
        private void LoadTemplateNames()
        {
            cbTemplateName.Items.Clear();
            cbTemplateName.Items.AddRange(ChartTemplate.GetChartTemplateNames(symbol, chbOnlyCurrentTickerTemplate.Checked));

            if (cbTemplateName.Items.Count > 0) cbTemplateName.SelectedIndex = 0;
        }

    }
}
