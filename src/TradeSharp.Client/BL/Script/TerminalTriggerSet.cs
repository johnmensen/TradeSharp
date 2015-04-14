using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    /// <summary>
    /// набор триггеров, объединенных логикой И-ИЛИ-НЕ,
    /// приводящих к вызову скрипта
    /// </summary>
    public class TerminalTriggerSet
    {
        public List<TerminalScriptTrigger> triggers = new List<TerminalScriptTrigger>();

        private ExpressionResolver resolver;

        private static readonly Regex regexParam = new Regex(@"T\d+", RegexOptions.IgnoreCase);

        public string FormulaError { get; private set; }

        private string formula;
        public string Formula
        {
            get { return formula; }
            set
            {
                FormulaError = "";
                formula = value;
                try
                {
                    resolver = new ExpressionResolver(value);
                }
                catch (Exception ex)
                {
                    FormulaError = ex.Message;
                    return;
                }

                // проверить значения параметров
                var resolverParams = resolver.GetVariableNames();
                if (resolverParams.Count == 0) return;
                FormulaError = string.Join(", ", resolverParams.Where(t => !regexParam.IsMatch(t)));
                
                if (!string.IsNullOrEmpty(FormulaError))
                {
                    FormulaError = "Некорректные параметры: " + FormulaError;
                    resolver = null;
                }
            }
        }
    
        public TerminalTriggerSet MakeCopy()
        {
            return new TerminalTriggerSet
                {
                    triggers = triggers.Select(t => t.MakeCopy()).ToList(),
                    Formula = formula
                };
        }

        public void SaveInXml(XmlElement node)
        {
            var scriptNode = (XmlElement)node.AppendChild(node.OwnerDocument.CreateElement("triggerSet"));
            scriptNode.Attributes.Append(node.OwnerDocument.CreateAttribute("formula")).Value = formula;
            // сохранить "детишек"
            foreach (var trigger in triggers)
            {
                trigger.SaveInXml(scriptNode);
            }
        }

        public static TerminalTriggerSet LoadFromXml(XmlElement parent)
        {
            var sets = new TerminalTriggerSet();
            var nodeList = parent.GetElementsByTagName("triggerSet");
            if (nodeList == null || nodeList.Count == 0) return sets;
            var node = (XmlElement)nodeList[0];
            if (node.Attributes["formula"] != null)
                sets.Formula = node.Attributes["formula"].Value;

            foreach (XmlElement child in node.ChildNodes)
            {
                var trigger = TerminalScriptTrigger.LoadFromXml(child);
                if (trigger != null)
                    sets.triggers.Add(trigger);
            }

            return sets;
        }

        public override string ToString()
        {
            return triggers.Count == 0 ? "-" : string.Join(", ", triggers);
        }
    }
}
