using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Indicator;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class SetupChartSeriesDlg : Form
    {
        private readonly List<CandleChartControl> charts;
        public MultyTimeframeIndexSettings sets;

        private List<IndicatorDiver.DivergenceType> divTypes = new 
            List<IndicatorDiver.DivergenceType>();

        public SetupChartSeriesDlg()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public SetupChartSeriesDlg(List<CandleChartControl> charts) : this()
        {
            this.charts = charts;
        }

        public SetupChartSeriesDlg(List<CandleChartControl> charts, MultyTimeframeIndexSettings sets) : this()
        {
            this.charts = charts;
            this.sets = sets;
        }

        private void SetupChartSeriesDlgLoad(object sender, EventArgs e)
        {
            // разновидности дивергенций
            foreach (IndicatorDiver.DivergenceType divType in Enum.GetValues(typeof (IndicatorDiver.DivergenceType)))            
                divTypes.Add(divType);
            foreach (var divType in divTypes) cbDiverType.Items.Add(divType);
            cbDiverType.SelectedIndex = 0;

            // заполнить выпадающий список
            var tags = charts.Select(c => new CandleChartControlTag(c));
            foreach (var tag in tags) cbChart.Items.Add(tag);
            // показать выбранные значения
            if (sets != null)
            {
                for (var i = 0; i < cbChart.Items.Count; i++)
                {
                    if (((CandleChartControlTag)cbChart.Items[i]).chart.UniqueId ==
                        sets.chartId)
                    {
                        cbChart.SelectedIndex = i;
                        cbInverse.Checked = sets.InverseDivergency;
                        cbSeriesSrc.Text = sets.FullyQualifiedSeriesSrcName;
                        cbSeriesDest.Text = sets.FullyQualifiedSeriesDestName;
                        cbDiverType.SelectedIndex = divTypes.IndexOf(sets.DiverType);
                        tbPeriodExtremum.Text = sets.periodExtremum.ToString();
                        tbMaxPastExtremum.Text = sets.maxPastExtremum.ToString();
                        tbIntervalMargins.Text = string.Format("{0} {1}",
                                                               sets.marginLower.ToStringUniform(),
                                                               sets.marginUpper.ToStringUniform());
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// заполнить выпадающие списки серий
        /// </summary>
        private void CbChartSelectedIndexChanged(object sender, EventArgs e)
        {
            cbSeriesSrc.Items.Clear();
            cbSeriesDest.Items.Clear();
            cbSeriesSrc.Items.Add(Localizer.GetString("TitleCourse"));
            cbSeriesDest.Items.Add(Localizer.GetString("TitleCourse"));

            if (cbChart.SelectedIndex < 0) return;
            var chart = (CandleChartControlTag) cbChart.Items[cbChart.SelectedIndex];
            foreach (var i in chart.chart.indicators)
            {                
                foreach (var ser in i.SeriesResult)
                {
                    var name = i.UniqueName + Separators.IndiNameDelimiter[0] + ser.Name;
                    cbSeriesSrc.Items.Add(name);
                    cbSeriesDest.Items.Add(name);
                }
            }
        }

        /// <summary>
        /// Принять изменения
        /// </summary>
        private void BtnOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            if (cbChart.SelectedIndex < 0) return;
            var chart = (CandleChartControlTag)cbChart.Items[cbChart.SelectedIndex];
            
            if (sets == null) sets = new MultyTimeframeIndexSettings();
            sets.chartId = chart.chart.UniqueId;
            sets.InverseDivergency = cbInverse.Checked;
            sets.FullyQualifiedSeriesSrcName = cbSeriesSrc.Text;
            sets.FullyQualifiedSeriesDestName = cbSeriesDest.Text;
            sets.TimeframeAndSymbol = cbChart.Text;
            sets.DiverType = (IndicatorDiver.DivergenceType)cbDiverType.SelectedItem;
            sets.periodExtremum = tbPeriodExtremum.Text.ToInt();
            sets.maxPastExtremum = tbMaxPastExtremum.Text.ToInt();
            // интервал
            var parts = tbIntervalMargins.Text.Split(new[] {' ', ',', ';', (char) 9},
                                                     StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                sets.marginLower = (double)(parts[0].ToDecimalUniformSafe() ?? 0);
                sets.marginUpper = (double)(parts[1].ToDecimalUniformSafe() ?? 0);
            }

            DialogResult = DialogResult.OK;
        }
    }

    public class CandleChartControlTag
    {
        public CandleChartControl chart;

        public CandleChartControlTag(CandleChartControl chart)
        {
            this.chart = chart;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", chart.Symbol,
                                 BarSettingsStorage.Instance.GetBarSettingsFriendlyName(chart.Timeframe));
        }
    }
}
