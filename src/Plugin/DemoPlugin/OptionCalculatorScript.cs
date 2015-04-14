using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Candlechart;
using Candlechart.ChartMath;
using DemoPlugin.Dialog;
using TradeSharp.Client.BL.Script;

namespace DemoPlugin
{
    [DisplayName("Опционный калькулятор")]
    public class OptionCalculatorScript : TerminalScript
    {
        public override bool CanBeTriggered
        {
            get { return false; }
        }

        public OptionCalculatorScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Опционный калькулятор";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            var dlg = new CalculatorForm(chart, worldCoords);
            dlg.ShowDialog();
            return string.Empty;
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"OptionCalculatorScript\"");
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"OptionCalculatorScript\"");
        }
    }
}
