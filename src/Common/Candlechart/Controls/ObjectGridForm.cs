using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Core;
using Candlechart.Series;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class ObjectGridForm : Form
    {
        private readonly CandleChartControl chart;

        public ObjectGridForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupGrid();
        }

        public ObjectGridForm(List<IChartInteractiveObject> objects, CandleChartControl chart, bool selectAll)
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            this.chart = chart;
            SetupGrid();
            grid.DataBind(objects, typeof(IChartInteractiveObject));
            grid.CheckSize();
            if (selectAll)            
                foreach (var row in grid.rows) row.Selected = true;
            grid.Invalidate();
        }

        private void SetupGrid()
        {
            var blank = new AsteriskTooltip();
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Name), Localizer.GetString("TitleName"))
                {
                    ColumnMinWidth = 50,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.ClassName), Localizer.GetString("TitleType"))
                {
                    ColumnMinWidth = 50
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.DateStart), Localizer.GetString("TitleTime"))
                {
                    ColumnWidth = 97
                });
            grid.CalcSetTableMinWidth();
            grid.UserHitCell += GridUserHitCell;
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button == MouseButtons.Left && e.Clicks > 1)
            {
                BtnEditClick(grid, e);
                return;
            }
        }

        private void BtnEditClick(object sender, EventArgs e)
        {
            var objs = grid.rows.Where(r => r.Selected).Select(r => (IChartInteractiveObject) r.ValueObject).ToList();
            CandleChartControl.EditSelectedChartObjects(objs);

            // перепривязать
            grid.DataBind(grid.rows.Select(r => (IChartInteractiveObject)r.ValueObject).ToList(),
                typeof(IChartInteractiveObject));
            grid.Invalidate();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            if (grid.rows.Count < 1) return;
            if (grid.rows.Count == 1)
            {
                chart.DeleteSelectedChartObjects(grid.GetRowValues<IChartInteractiveObject>(false).ToList());
                grid.DataBind(new List<IChartInteractiveObject>(), typeof(IChartInteractiveObject));
                grid.Invalidate();
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                return;
            }

            var selected = grid.GetRowValues<IChartInteractiveObject>(true).ToList();
            if (selected.Count == 0) return;
            
            var unselected =
                grid.rows.Where(r => !r.Selected).Select(r => (IChartInteractiveObject) r.ValueObject).ToList();
            
            // удалить выбранные объекты            
            chart.DeleteSelectedChartObjects(selected);

            // перепривязать
            grid.DataBind(unselected, typeof(IChartInteractiveObject));
            grid.Invalidate();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void BtnSaveClick(object sender, EventArgs e)
        {
            var doc = new XmlDocument();
            var root = (XmlElement)doc.AppendChild(doc.CreateElement("objects"));
            chart.SaveObjects(root);
            // подготовить имя файла
            saveFileDialog.FileName = string.Format("{0} {1} [{2}]", Localizer.GetString("TitleObjectsSmall"),
                                                    chart.chart.Symbol, chart.chart.Timeframe);
            // сохранить документ в файл
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                doc.Save(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения объектов графика ({0}): {1}", saveFileDialog.FileName, ex);
                MessageBox.Show(string.Format(Localizer.GetString("MessageErrorSavingChartObjectsFmt"), ex.Message),
                    Localizer.GetString("TitleError"));
            }
        }

        private void BtnLoadClick(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            if (!File.Exists(openFileDialog.FileName))
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageErrorLoadingChartObjectsFileNotExistsFmt"), 
                    openFileDialog.FileName));
                return;
            }
            try
            {
                var doc = new XmlDocument();
                doc.Load(openFileDialog.FileName);
                if (doc.DocumentElement == null) throw new Exception(Localizer.GetString("MessageDocumentEmpty"));
                if (doc.DocumentElement.Name != "objects")
                    throw new Exception(Localizer.GetString("MessageUnsupportedDocumentFormat"));
                chart.LoadObjects(doc.DocumentElement);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка загрузки объектов графика ({0}): {1}", 
                    openFileDialog.FileName, ex);
                MessageBox.Show(string.Format(
                    Localizer.GetString("MessageErrorLoadingChartObjectsFmt"), 
                    ex.Message));
            }
        }

        private void GridKeyUp(object sender, KeyEventArgs e)
        {
           if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                BtnDeleteClick(sender, EventArgs.Empty);
            }
        }

        private void CbSelectionCheckedChanged(object sender, EventArgs e)
        {
            if (grid.rows.Count == 0) return;
            var selCount = grid.rows.Count(r => r.Selected);
            var shouldSelect = selCount <= (grid.rows.Count / 2);

            foreach (var row in grid.rows)
            {
                row.Selected = shouldSelect;
            }
            btnEdit.Enabled = shouldSelect;
            btnDelete.Enabled = shouldSelect;

            grid.Refresh();
        }

        private void GridSelectionChanged(MouseEventArgs e, int rowIndex, FastColumn col)
        {
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
        }
    }
}
