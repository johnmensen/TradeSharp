using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Indicator;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class CrossChartDivergenciesSettingsWindow : Form
    {
        private IndicatorCrossChartDivergencies indi;
        public List<MultyTimeframeIndexSettings> sets;
        public delegate void GetChartsListDel(out List<CandleChartControl> charts);

        private static GetChartsListDel getChartsList;
        public static event GetChartsListDel GetChartsList
        {
            add { getChartsList += value; }
            remove { getChartsList -= value; }
        }

        public CrossChartDivergenciesSettingsWindow()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            grid.AutoGenerateColumns = false;
            grid.Columns[0].HeaderText = Localizer.GetString("TitleChart");
            grid.Columns[1].HeaderText = Localizer.GetString("TitleIndex");
            grid.Columns[2].HeaderText = Localizer.GetString("TitleChart");
        }

        public CrossChartDivergenciesSettingsWindow(IndicatorCrossChartDivergencies indi) : this()
        {
            this.indi = indi;
            // копировать настройки и отобразить в графике
            sets = indi.Sets.Select(s => s.MakeCopy()).ToList();
            grid.DataSource = sets;
        }

        /// <summary>
        /// добавить график и серии     
        /// </summary>        
        private void BtnAddClick(object sender, System.EventArgs e)
        {
            // получить графики
            List<CandleChartControl> charts;
            getChartsList(out charts);
            var dlg = new SetupChartSeriesDlg(charts);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            sets.Add(dlg.sets);
            // показать новый дивер
            grid.DataSource = null;
            grid.DataSource = sets;
            grid.Refresh();
        }

        private void GridCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var set = (MultyTimeframeIndexSettings) grid.Rows[e.RowIndex].DataBoundItem;

            List<CandleChartControl> charts;
            getChartsList(out charts);
            var dlg = new SetupChartSeriesDlg(charts, set);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            
            // показать обновленный дивер
            grid.DataSource = null;
            grid.DataSource = sets;
            grid.Refresh();
        }

        private void BtnRemoveClick(object sender, System.EventArgs e)
        {
            var needRefresh = false;
            for (var i = 0; i < grid.Rows.Count; i++)
            {
                if (grid.Rows[i].Selected)
                {
                    sets.Remove(((MultyTimeframeIndexSettings) grid.Rows[i].DataBoundItem));
                    needRefresh = true;
                }
            }
            if (!needRefresh) return;
            grid.DataSource = null;
            grid.DataSource = sets;
            grid.Refresh();
        }
    }
}
