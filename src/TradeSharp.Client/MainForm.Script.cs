using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Client.BL.Script;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client
{   
    public partial class MainForm
    {
        /// <summary>
        /// всем чартам добавить в меню сценарии,
        /// сценарии, не требующие чартов, прописать в основном меню
        /// </summary>
        private void ActualizeScriptMenu()
        {
            var allScripts = ScriptManager.Instance.GetScripts();
            var chartScripts = allScripts.Where(s => s.ScriptTarget == TerminalScript.TerminalScriptTarget.График ||
                                                     s.ScriptTarget == TerminalScript.TerminalScriptTarget.Тикер);
            var mainMenuScripts = allScripts.Where(s => s.ScriptTarget == TerminalScript.TerminalScriptTarget.Терминал ||
                                                     s.ScriptTarget == TerminalScript.TerminalScriptTarget.Тикер);

            // добавить в главное меню
            menuitemScript.DropDownItems.Clear();
            foreach (var mainMenuItem in mainMenuScripts)
            {
                var item = menuitemScript.DropDownItems.Add(mainMenuItem.Title,
                    lstGlyph32.Images["gear_16.png"], MainMenuScriptActivated);
                item.Tag = mainMenuItem;
            }
            menuitemScript.Available = menuitemScript.DropDownItems.Count > 0;

            // добавить окошкам
            var menuTitles = chartScripts.Select(s => s.Title).ToList();
            var menuTags = chartScripts.Cast<object>().ToList();
            foreach (var chartForm in Charts)
            {
                chartForm.chart.SetupScriptMenu(menuTitles, menuTags);
            }
        }

        private void AddScriptsToChart(CandleChartControl chart)
        {
            var allScripts = ScriptManager.Instance.GetScripts();
            var chartScripts = allScripts.Where(s => s.ScriptTarget == TerminalScript.TerminalScriptTarget.График ||
                                                     s.ScriptTarget == TerminalScript.TerminalScriptTarget.Тикер);
            var menuTitles = chartScripts.Select(s => s.Title).ToList();
            var menuTags = chartScripts.Cast<object>().ToList();
            chart.SetupScriptMenu(menuTitles, menuTags);
        }

        private void MainMenuScriptActivated(object sender, EventArgs args)
        {
            var menuItem = (ToolStripMenuItem) sender;
            var script = (TerminalScript) menuItem.Tag;
            
            // если скрипт требует выбора тикера, предложить этот выбор            
            if (script.ScriptTarget == TerminalScript.TerminalScriptTarget.Тикер)
            {
                string inputText;
                object selectedObject;
                var tickerList = DalSpot.Instance.GetTickerNames();
                var majors = new[] {"EURUSD", "USDJPY", "USDCAD", "GBPUSD", "USDCHF"};
                var subList = tickerList.Except(majors).OrderBy(t => t);
                var totalList = majors.Concat(subList).Cast<object>().ToList();

                if (!Dialogs.ShowComboDialog("Укажите торгуемый инструмент",
                    totalList, out selectedObject, out inputText, true))
                    return;
                var scriptTicker = (string) selectedObject;
                var rstString = script.ActivateScript(scriptTicker);
                if (!string.IsNullOrEmpty(rstString))
                    AddMessageToStatusPanelSafe(DateTime.Now,
                        "Скрипт [" + script.ScriptName + "]: " + rstString);
                return;
            }

            // активировать скрипт без параметров
            var rst = script.ActivateScript(false);
            if (!string.IsNullOrEmpty(rst))
                AddMessageToStatusPanelSafe(DateTime.Now, "Скрипт [" + script.ScriptName + "]: " + rst);
        }

        private void ChartScriptMenuItemActivated(CandleChartControl sender, 
            PointD worldCoords, object script)
        {
            var scriptTyped = (TerminalScript) script;
            if (scriptTyped.ScriptTarget == TerminalScript.TerminalScriptTarget.График)
            {
                var rstString = scriptTyped.ActivateScript(sender, worldCoords);
                if (!string.IsNullOrEmpty(rstString))
                    AddMessageToStatusPanelSafe(DateTime.Now, "Скрипт [" + scriptTyped.ScriptName + "]: " + rstString);
                return;
            }
            var rst = scriptTyped.ActivateScript(sender.Symbol);
            if (!string.IsNullOrEmpty(rst))
                AddMessageToStatusPanelSafe(DateTime.Now, "Скрипт [" + scriptTyped.ScriptName + "]: " + rst);
        }
    
        private void CheckScriptTriggerOrder(ScriptTriggerDealEventType evt, MarketOrder order)
        {
            // сформировать список скриптов для срабатывания
            var scriptsToFire = new List<TerminalScript>();
            var scripts = ScriptManager.Instance.GetScripts().ToList();
            foreach (var script in scripts.Where(s => s.Trigger != null &&
                s.Trigger is ScriptTriggerDealEvent /*&& s.ScriptTarget != TerminalScript.TerminalScriptTarget.График*/))
            {
                var orderScriptTrigger = (ScriptTriggerDealEvent)script.Trigger;
                var shouldFire = (orderScriptTrigger.eventType & evt) == evt;
                if (shouldFire)
                {
                    orderScriptTrigger.sourceOrder = order;
                    orderScriptTrigger.sourceEvent = evt;
                    scriptsToFire.Add(script);
                }
            }

            // запустить скрипты на выполнение
            if (scriptsToFire.Count > 0)
                ThreadPool.QueueUserWorkItem(ExecuteScriptsRoutine, scriptsToFire);
        }

        private void CheckScriptTriggerQuote(string[] quoteNames, QuoteData[] quoteArray)
        {
            // сформировать список скриптов (по котировке и по формуле) для срабатывания
            var scriptsToFire = new List<TerminalScript>();
            var scripts = ScriptManager.Instance.GetScripts().ToList();
            foreach (var script in scripts.Where(s => s.Trigger != null &&
                                                      (s.Trigger is ScriptTriggerNewQuote ||
                                                       s.Trigger is ScriptTriggerPriceFormula) 
                                                       /*&& s.ScriptTarget != TerminalScript.TerminalScriptTarget.График*/))
            {
                if (script.Trigger is ScriptTriggerNewQuote)
                {
                    var trigger = (ScriptTriggerNewQuote) script.Trigger;
                    if (trigger.quotesToCheck.Count == 0 ||
                        trigger.quotesToCheck.Any(q => quoteNames.Any(n => n.Equals(q, StringComparison.OrdinalIgnoreCase))))
                    {
                        scriptsToFire.Add(script);
                    }
                    continue;
                }
                if (script.Trigger is ScriptTriggerPriceFormula)
                {
                    var trigger = (ScriptTriggerPriceFormula) script.Trigger;
                    trigger.CheckCondition();
                    if (trigger.IsTriggered)
                        scriptsToFire.Add(script);
                }
            }

            // запустить скрипты на выполнение
            if (scriptsToFire.Count > 0)
                ThreadPool.QueueUserWorkItem(ExecuteScriptsRoutine, scriptsToFire);
        }

        private void ExecuteScriptsRoutine(object args)
        {
            var scriptsToFire = (List<TerminalScript>) args;
            foreach (var script in scriptsToFire)
            {
                // запустить (активировать) скрипт
                try
                {
                    script.ActivateScript(true);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка выполнения скрипта {0}: {1}", script.Title, ex);
                }
            }
        }
    }
}

