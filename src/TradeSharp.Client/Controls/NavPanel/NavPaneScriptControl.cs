using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Client.BL.Script;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPaneScriptControl : UserControl, INavPageContent
    {
        class ScriptContainer
        {
            public TerminalScript script;

            public int ImageIndex { get; private set; }

            public string Title { get { return script.Title; } }

            public string ScriptName { get { return script.ScriptName; } }

            public ScriptContainer() {}

            public ScriptContainer(TerminalScript script)
            {
                this.script = script;
                ImageIndex = script.ScriptTarget == TerminalScript.TerminalScriptTarget.График
                    ? 0 : script.ScriptTarget == TerminalScript.TerminalScriptTarget.Терминал ? 1
                    : script.ScriptTarget == TerminalScript.TerminalScriptTarget.Тикер ? 2 : 0;
            }
        }

        public event Action<int> ContentHeightChanged;

        public NavPaneScriptControl()
        {
            InitializeComponent();
            SetupUi();            
        }

        private void SetupUi()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            grid.Columns.Add(new FastColumn("ImageIndex", "*")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnWidth = 28,
                    ImageList = imageList
                });
            grid.Columns.Add(new FastColumn("Title", "Название")
                {
                    SortOrder = FastColumnSort.Ascending,
                    HyperlinkFontActive = fontBold,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Navy,
                    ColumnMinWidth = 70
                });
            grid.Columns.Add(new FastColumn("ScriptName", "Тип")
            {
                ColumnMinWidth = 55
            });
            grid.CalcSetTableMinWidth();
            grid.MouseDown += GridOnMouseDown;
            grid.UserHitCell += GridOnUserHitCell;
        }

        private void GridOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var cell = grid.GetCellUnderCursor(e.X, e.Y);
            if (!cell.HasValue) return;

            // дать пользователю перетащить скрипт на график
            var scriptContainer = (ScriptContainer)grid.rows[cell.Value.Y].ValueObject;
            grid.DoDragDrop(new CandleChartDroppingObject(scriptContainer.script,
                    CandleChartDroppingObject.ValueType.Script), DragDropEffects.All);            
        }

        private void BindScripts()
        {
            var scripts = ScriptManager.Instance.GetScripts().Select(s => new ScriptContainer(s)).ToList();
            grid.DataBind(scripts);

            // установить контролу высоту
            if (ContentHeightChanged != null)
            {
                var height = grid.rows.Count * grid.CellHeight + grid.CaptionHeight + 2;
                if (grid.Width < grid.MinimumTableWidth) // учесть высоту скролбара
                    height += SystemInformation.HorizontalScrollBarHeight;
                ContentHeightChanged(height);
            }
        }

        private void GridOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (col.PropertyName == "Title" && e.Button == MouseButtons.Left)
            {
                // вызвать скрипт сразу или дать перетащить его на график
                var scriptContainer = (ScriptContainer) grid.rows[rowIndex].ValueObject;
                var script = scriptContainer.script;
                
                // просто выполнить скрипт
                if (script.ScriptTarget == TerminalScript.TerminalScriptTarget.Терминал)
                {
                    script.ActivateScript(false);
                    //return;
                }                                
            }
        }

        private void ScriptListIsUpdated()
        {
            try
            {
                BindScripts();
            }
            catch
            {
            }
        }

        private void NavPaneScriptControlLoad(object sender, EventArgs e)
        {
            BindScripts();

            ScriptManager.Instance.scriptListIsUpdated += ScriptListIsUpdated;
            MainForm.Instance.Closing += (s, args) =>
            {
                ScriptManager.Instance.scriptListIsUpdated -= ScriptListIsUpdated;
            };
        }
    }
}
