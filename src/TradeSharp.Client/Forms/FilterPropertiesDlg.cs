using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class FilterPropertiesDlg : Form
    {
        public EntityFilter filter;
        public FilterPropertiesDlg()
        {
            filter = new EntityFilter(null);
            InitializeComponent();
        }
        public FilterPropertiesDlg(EntityFilter filter)
        {
            this.filter = filter;
            InitializeComponent();
        }

        private void FilterPropertiesDlg_Load(object sender, EventArgs e)
        {
            // заполняем таблицу
            foreach (var item in filter.ColumnFilters.Values.ToList())
            {
                var row = new DataGridViewRow();
                
                row.Cells.Add(new DataGridViewTextBoxCell { Value = item.PropInfo.Name });
                row.Cells.Add(new DataGridViewTextBoxCell { Value = item.Title });

                if (item.EnabledValues.Length > 0)
                {
                    var valueCol = new DataGridViewComboBoxCell();
                    valueCol.Items.AddRange(item.EnabledValues.ToArray());
                    row.Cells.Add(valueCol);
                }
                else
                    row.Cells.Add(new DataGridViewTextBoxCell {Value = item.Value ?? string.Empty});

                var criteriasCol = new DataGridViewComboBoxCell();
                criteriasCol.Items.AddRange(item.GetStringCriterias());
                if (item.Criterias != ColumnFilterCriteria.Нет)
                    criteriasCol.Value = item.Criterias.ToString();
                else
                    criteriasCol.Value = criteriasCol.Items[0];
                row.Cells.Add(criteriasCol);
                grid.Rows.Add(row);
            }
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // просматриваю есть ли изменения пользователем
            foreach (DataGridViewRow row in grid.Rows)
            {
                var item = filter.ColumnFilters[row.Cells["Property"].Value.ToString()];
                if (row.Cells["Value"].Value == null)
                    item.Value = row.Cells["Value"].Value;
                else
                    item.Value = row.Cells["Value"].Value.ToString() == string.Empty ? null : row.Cells["Value"].Value;
                item.Criterias = (ColumnFilterCriteria)Enum.Parse(typeof(ColumnFilterCriteria),
                                                             row.Cells["Criterias"].Value.ToString());
                filter.ColumnFilters[item.PropInfo.Name] = item;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
