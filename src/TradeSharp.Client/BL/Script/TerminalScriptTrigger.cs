using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    /// <summary>
    /// описывает условия срабатывания (вызова) скрипта
    /// </summary>
    public abstract class TerminalScriptTrigger
    {
        public static Dictionary<string, Type> scriptTypeByTitle;

        public bool IsTriggered { get; set; }

        public abstract string TypeName { get; }

        public abstract string ParamsString { get; }

        static TerminalScriptTrigger()
        {
            scriptTypeByTitle = new Dictionary<string, Type>();

            var thisType = typeof (TerminalScriptTrigger);
            foreach (var type in thisType.Assembly.GetTypes())
            {
                if (!type.IsSubclassOf(thisType)) continue;
                var dispAttr = (DisplayNameAttribute) type.GetCustomAttributes(typeof (DisplayNameAttribute), true)[0];
                scriptTypeByTitle.Add(dispAttr.DisplayName, type);
            }
        }

        public abstract TerminalScriptTrigger MakeCopy();

        public virtual XmlElement SaveInXml(XmlElement parent)
        {
            var node = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement("trigger"));
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("typeName")).Value = TypeName;
            return node;
        }

        protected abstract void InitParamsFromXml(XmlElement node);        

        public static TerminalScriptTrigger LoadFromXml(XmlElement node)
        {
            if (node.Attributes["typeName"] == null) return null;
            var typeName = node.Attributes["typeName"].Value;

            Type destType;
            if (!scriptTypeByTitle.TryGetValue(typeName, out destType)) return null;
            var trigger = (TerminalScriptTrigger) destType.GetConstructor(new Type[0]).Invoke(new object[0]);
            trigger.InitParamsFromXml(node);
            return trigger;
        }

        public override string ToString()
        {
            return TypeName;
        }
    }

    /// <summary>
    /// триггер - котировка обновлена
    /// </summary>
    [DisplayName("Обновление котировки")]
    public class ScriptTriggerNewQuote : TerminalScriptTrigger
    {
        private static readonly char[] quoteListDelimiters = new[] {','};

        public List<string> quotesToCheck;

        public override string TypeName
        {
            get { return "Обновление котировки"; }
        }

        public override string ParamsString
        {
            get
            {
                return quotesToCheck == null || quotesToCheck.Count == 0
                           ? "любая"
                           : string.Join(", ", quotesToCheck);
            }
        }

        public override TerminalScriptTrigger MakeCopy()
        {
            return new ScriptTriggerNewQuote {quotesToCheck = quotesToCheck == null ? null : quotesToCheck.ToList()};
        }

        public override XmlElement SaveInXml(XmlElement parent)
        {
            var node = base.SaveInXml(parent);
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("quotes")).Value =
                quotesToCheck == null ? "" : string.Join(",", quotesToCheck);
            return node;
        }

        protected override void InitParamsFromXml(XmlElement node)
        {
            if (node.Attributes["quotes"] != null)
                quotesToCheck = node.Attributes["quotes"].Value.Split(quoteListDelimiters, 
                    StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    [Flags]
    public enum ScriptTriggerDealEventType
    {
        Нет = 0,
        НовыйОрдер = 1,
        ЗакрытиеОрдера = 2,
        РедактированиеОрдера = 4
    }

    /// <summary>
    /// триггер - событие по сделке
    /// </summary>
    [DisplayName("Ордер")]
    public class ScriptTriggerDealEvent : TerminalScriptTrigger
    {
        public ScriptTriggerDealEventType eventType;

        public override string TypeName
        {
            get { return "Ордер"; }
        }

        public ScriptTriggerDealEventType sourceEvent;

        public MarketOrder sourceOrder;

        public override string ParamsString
        {
            get
            {
                var evts = new List<string>();
                foreach (ScriptTriggerDealEventType ev in Enum.GetValues(typeof (ScriptTriggerDealEventType)))
                {
                    if ((eventType & ev) == ev)
                        evts.Add(ev.ToString());
                }
                return evts.Count == 0 ? "-" : string.Join(", ", evts);
            }
        }

        public override TerminalScriptTrigger MakeCopy()
        {
            return new ScriptTriggerDealEvent {eventType = eventType};
        }

        public override XmlElement SaveInXml(XmlElement parent)
        {
            var node = base.SaveInXml(parent);
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("eventType")).Value =
                ((int) eventType).ToString();
            return node;
        }

        protected override void InitParamsFromXml(XmlElement node)
        {
            if (node.Attributes["eventType"] != null)
            {
                try
                {
                    var eventTypeNum = node.Attributes["eventType"].Value.ToInt();
                    eventType = (ScriptTriggerDealEventType) eventTypeNum;
                }
                catch
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// триггер - результат расчета по формуле
    /// </summary>
    [DisplayName("Формула")]
    public class ScriptTriggerPriceFormula : TerminalScriptTrigger
    {
        private ExpressionResolver resolver;

        private string formula;
        public string Formula
        {
            set
            {
                FormulaError = "";
                formula = value;
                try
                {
                    resolver = new ExpressionResolver(formula);
                    CheckFormulaParams();
                }
                catch (Exception ex)
                {
                    resolver = null;
                    FormulaError = ex.Message;
                }
            }

            get { return formula; }
        }

        public string FormulaError { get; private set; }

        private List<string> formulaParams;

        private void CheckFormulaParams()
        {
            formulaParams = resolver.GetVariableNames();
            var errorPtrs = new List<string>();
            var tickers = DalSpot.Instance.GetTickerNames();

            foreach (var ptr in formulaParams)
            {
                if (ExpressionResolverLiveParams.CheckParamName(ptr)) continue;
                if (tickers.Any(t => t.Equals(ptr, StringComparison.OrdinalIgnoreCase)))
                    continue;
                errorPtrs.Add(ptr);
            }

            if (errorPtrs.Count == 0) return;
            FormulaError = "Некорректные параметры: " + string.Join(", ", errorPtrs);
            formulaParams = null;
            resolver = null;
        }
    
        public void CheckCondition()
        {
            IsTriggered = false;

            if (resolver == null) return;

            // получить значения переменных
            var varVal = new Dictionary<string, double>();
            var timeNow = DateTime.Now;
            var quotes = QuoteStorage.Instance.ReceiveAllData().ToDictionary(q => q.Key.ToLower(), q => q.Value);

            foreach (var ptrName in formulaParams)
            {
                var ptrVal = ExpressionResolverLiveParams.GetParamValue(ptrName, timeNow, false);
                if (ptrVal.HasValue)
                {
                    varVal.Add(ptrName, ptrVal.Value);
                    continue;
                }
                // получить котировку
                QuoteData quote;
                quotes.TryGetValue(ptrName.ToLower(), out quote);
                varVal.Add(ptrName, quote == null ? 0 : (quote.ask + quote.bid) * 0.5);
            }

            double result;
            try
            {
                resolver.Calculate(varVal, out result);
            }
            catch
            {
                result = 0;
            }

            // не 0 есть "истина"
            IsTriggered = result != 0;
        }

        public override string TypeName
        {
            get { return "Формула"; }
        }

        public override string ParamsString
        {
            get { return formula ?? ""; }
        }

        public override TerminalScriptTrigger MakeCopy()
        {
            return new ScriptTriggerPriceFormula
                {
                    Formula = formula
                };
        }

        public override XmlElement SaveInXml(XmlElement parent)
        {
            var node = base.SaveInXml(parent);
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("formula")).Value = formula;
            return node;
        }

        protected override void InitParamsFromXml(XmlElement node)
        {
            if (node.Attributes["formula"] != null)
                Formula = node.Attributes["formula"].Value;
        }
    }    
}
