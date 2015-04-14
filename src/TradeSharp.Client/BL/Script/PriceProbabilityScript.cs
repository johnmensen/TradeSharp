using System;
using System.ComponentModel;
using Candlechart;
using Candlechart.ChartMath;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Вероятность цены")]
    public class PriceProbabilityScript : TerminalScript
    {
        public PriceProbabilityScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Вероятность цены";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            new PriceProbForm(chart, worldCoords).ShowDialog();
            return string.Empty;
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверные параметры активации");
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверные параметры активации");
        }
    }
}
