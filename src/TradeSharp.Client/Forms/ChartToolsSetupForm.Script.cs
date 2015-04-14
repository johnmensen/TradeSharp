using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.BL.Script;
using System.Linq;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Forms
{
    /// <summary>
    /// настройки скриптов
    /// </summary>
    public partial class ChartToolsSetupForm
    {
        private void SetupScriptsGrid()
        {
            var blank = new DummyScript();
            var fontBold = new Font(Font, FontStyle.Bold);
            gridScripts.Columns.Add(new FastColumn(blank.Property(p => p.Title), Localizer.GetString("TitleName"))
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 70,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            gridScripts.Columns.Add(new FastColumn(blank.Property(p => p.ScriptName), Localizer.GetString("TitleType")) { ColumnMinWidth = 70 });
            gridScripts.Columns.Add(new FastColumn(blank.Property(p => p.ScriptTarget), Localizer.GetString("TitleCategory")) { ColumnMinWidth = 70 });
            gridScripts.Columns.Add(new FastColumn(blank.Property(p => p.Trigger), Localizer.GetString("TitleTrigger"))
                {
                    ColumnMinWidth = 60,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            gridScripts.Columns.Add(new FastColumn(blank.Property(p => p.ScriptTarget), "-")
                {
                    ColumnWidth = 60,
                    formatter = d => Localizer.GetString("TitleDelete"),
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            
            gridScripts.MinimumTableWidth = gridScripts.Columns.Sum(c => c.ColumnWidth != 0 
                ? c.ColumnWidth : c.ColumnMinWidth > 0 ? c.ColumnMinWidth : 70);
        }

        private void LoadScripts()
        {
            var scripts = ScriptManager.Instance.GetScripts();
            var dummy = new DummyScript();
            scripts.Add(dummy);
            gridScripts.DataBind(scripts);            
        }

        private void GridScriptsUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            // удалить скрипт или создать новый или редактировать существующий
            if (rowIndex < 0 || rowIndex >= gridScripts.rows.Count) return;
            var script = (TerminalScript) gridScripts.rows[rowIndex].ValueObject;

            // удалить
            if (col.Title == "-")
            {
                if (script is DummyScript) return;
                var allScripts = GetScriptsFromTable();
                allScripts.Remove(script);
                ScriptManager.Instance.UpdateScripts(allScripts);
                LoadScripts();
                gridScripts.Invalidate();
                return;
            }

            // настроить триггер
            if (col.PropertyName == script.Property(p => p.Trigger))
            {
                if (script is DummyScript) return;
                var dlg = new TriggerSetupDialog(script.Trigger);
                if (dlg.ShowDialog() != DialogResult.OK) return;
                script.Trigger = dlg.Trigger;
                gridScripts.UpdateRow(rowIndex, script);
                gridScripts.InvalidateRow(rowIndex);
                return;
            }

            // добавить новый или отредактировать существующий
            if (col.PropertyName != script.Property(p => p.Title))
                return;
            
            // добавить скрипт
            if (script is DummyScript)
            {
                AddNewScript();
                return;
            }

            // редактировать скрипт
            new PropertiesDlg(script, Localizer.GetString("TitleChangeScript")).ShowDialog();
            gridScripts.UpdateRow(rowIndex, script);
            gridScripts.InvalidateRow(rowIndex);
            
            // сохранить изменения
            var updatedScripts = GetScriptsFromTable();
            ScriptManager.Instance.UpdateScripts(updatedScripts);
        }

        private List<TerminalScript> GetScriptsFromTable()
        {
            return gridScripts.rows.Where(r => ((TerminalScript)r.ValueObject).GetType()
                    != typeof(DummyScript)).Select(r => (TerminalScript)r.ValueObject).ToList();
        }
    
        private void AddNewScript()
        {
            // выбрать тип скрипта
            var scriptTypes = TerminalScript.GetAllTerminalScripts();
            var dialog = new ListSelectDialog();
            dialog.Initialize(scriptTypes.Keys.Cast<object>().ToList(), "", Localizer.GetString("TitleSelectScript"));
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            var selectedScript = (string) dialog.SelectedItem;

            // создать скрипт с параметрами по-умолчанию
            var script = TerminalScript.MakeScriptByScriptName(selectedScript);
            
            // добавить в таблицу
            var scripts = gridScripts.rows.Select(r => (TerminalScript) r.ValueObject).ToList();
            scripts.Add(script);
            gridScripts.DataBind(scripts);
            gridScripts.Invalidate();
            
            // сохранить настройки
            ScriptManager.Instance.UpdateScripts(GetScriptsFromTable());
        }
    }
}