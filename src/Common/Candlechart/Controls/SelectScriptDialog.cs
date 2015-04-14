using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class SelectScriptDialog : Form
    {
        class ScriptItem
        {
            public string Name { get; set; }

            public object Script { get; set; }

            public ScriptItem() {}

            public ScriptItem(string name, object script)
            {
                Name = name;
                Script = script;
            }
        }

        private readonly Action showOpenScriptSettingsDialog;

        private ScriptItem selectedScript;

        public string SelectedName
        {
            get { return selectedScript == null ? string.Empty : selectedScript.Name; }
        }

        public object SelectedScript
        {
            get { return selectedScript == null ? string.Empty : selectedScript.Script; }
        }

        public DateTime? SelectedTime
        {
            get 
            { 
                var time = tbTime.Text.ToDateTimeUniformSafe();
                if (!time.HasValue) return null;
                return time.Value == initialTime ? (DateTime?)null : time.Value;
            }
        }

        public float? SelectedPrice
        {
            get
            {
                var price = tbPrice.Text.ToFloatUniformSafe();
                if (!price.HasValue) return null;
                // изменилось существенно?
                return price.Value.AreSame(initialPrice) ? (float?)null : price.Value;
            }
        }

        private readonly DateTime initialTime;

        private readonly float initialPrice;

        public SelectScriptDialog()
        {
            InitializeComponent();
        }

        public SelectScriptDialog(List<string> scriptNames, List<object> scripts,
            DateTime worldTime, float worldPrice, Action showOpenScriptSettingsDialog) : this()
        {
            this.showOpenScriptSettingsDialog = showOpenScriptSettingsDialog;
            initialTime = worldTime;
            initialPrice = worldPrice;
            tbPrice.Text = worldPrice.ToStringUniformPriceFormat(true);
            tbTime.Text = worldTime.ToStringUniform();

            var fontBold = new Font(Font, FontStyle.Bold);
            grid.Columns.Add(new FastColumn("Name", "Скрипт")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 80,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Navy
                });
            grid.CalcSetTableMinWidth();
            grid.UserHitCell += GridOnUserHitCell;

            var items = scriptNames.Select((name, i) => new ScriptItem(name, scripts[i])).ToList();
            grid.DataBind(items);
        }

        private void GridOnUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            selectedScript = (ScriptItem)grid.rows[rowIndex].ValueObject;
            DialogResult = DialogResult.OK;
        }

        private void btnSetupScripts_Click(object sender, EventArgs e)
        {
            showOpenScriptSettingsDialog();
            DialogResult = DialogResult.Cancel;
        }
    }
}
