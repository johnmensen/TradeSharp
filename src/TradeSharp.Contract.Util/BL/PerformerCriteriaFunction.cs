using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// используется для фильтрации таблицы сигнальщиков
    /// пример заполнения:
    /// "AYP - DD * 1.5"
    /// "среднегодовой доход за вычетом макс. относительного проседания с коэффициентом 1.5"
    /// </summary>
    public class PerformerCriteriaFunction
    {
        private string function;
        public string Function
        {
            get
            {
                if (!IsExpressionParsed)
                    return function;
                var expression = "";
                foreach (var filter in filters)
                    expression += (!string.IsNullOrEmpty(expression) ? "&" : "") + "(" + filter.a.ExpressionParamName +
                                  GetExpressionOperatorString(filter.b) + filter.c + ")";
                if (SortField != null && !string.IsNullOrEmpty(SortField.ExpressionParamName))
                    expression = (!string.IsNullOrEmpty(expression) ? "(" + expression + ")*" : "") + SortField.ExpressionParamName;
                return expression;
            }
            set
            {
                function = value;
                IsExpressionParsed = PerformerStatField.ParseSimpleFormula(function, out filters, out SortField);
            }
        }

        public string Description { get; set; }

        // рекомендуемый порядок сортировки
        public SortOrder PreferredSortOrder { get; set; }

        // граничное значение для выборки
        public float? MarginValue { get; set; }

        // выражения в формуле;
        // формируются при установке Function
        private List<Cortege3<PerformerStatField, ExpressionOperator, double>> filters =
            new List<Cortege3<PerformerStatField, ExpressionOperator, double>>();
        public List<Cortege3<PerformerStatField, ExpressionOperator, double>> Filters
        {
            get { return filters; }
        }

        // поле, по которому сортируется результат;
        // формируется при установке Function
        public PerformerStatField SortField;

        public bool IsExpressionParsed;

        public static string GetExpressionOperatorString(ExpressionOperator expressionOperator)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Greater:
                    return ">";
                case ExpressionOperator.Lower:
                    return "<";
                case ExpressionOperator.Equal:
                    return "=";
            }
            return "?";
        }

        public static ExpressionOperator GetExpressionOperatorByString(string expressionOperator)
        {
            var result = ExpressionOperator.Equal;
            if (expressionOperator == ">")
                result = ExpressionOperator.Greater;
            if (expressionOperator == "<")
                result = ExpressionOperator.Lower;
            if (expressionOperator == "=")
                result = ExpressionOperator.Equal;
            return result;
        }

        public PerformerCriteriaFunction()
        {
        }

        public PerformerCriteriaFunction(PerformerCriteriaFunction funct)
        {
            function = funct.function;
            Description = funct.Description;
            PreferredSortOrder = funct.PreferredSortOrder;
            MarginValue = funct.MarginValue;
            filters = new List<Cortege3<PerformerStatField, ExpressionOperator, double>>(funct.filters);
            SortField = funct.SortField;
            IsExpressionParsed = funct.IsExpressionParsed;
        }

        public void WriteToXml(XmlElement nodeParent)
        {
            var node = nodeParent.AppendChild(nodeParent.OwnerDocument.CreateElement("function"));
            node.Attributes.Append(nodeParent.OwnerDocument.CreateAttribute("Description")).Value = Description;
            node.Attributes.Append(nodeParent.OwnerDocument.CreateAttribute("PreferredSortOrder")).Value = ((int)PreferredSortOrder).ToString();
            node.Attributes.Append(nodeParent.OwnerDocument.CreateAttribute("MarginValue")).Value = 
                MarginValue.HasValue ? MarginValue.Value.ToString() : "";
            node.InnerText = Function;
        }

        public static PerformerCriteriaFunction ReadFromXml(XmlElement node)
        {
            var func = node.InnerText;
            if (string.IsNullOrEmpty(func)) return null;
            var desc = node.GetAttributeString("Description");
            if (string.IsNullOrEmpty(desc)) return null;
            var function = new PerformerCriteriaFunction
                {
                    Function = func,
                    Description = desc
                };

            var order = node.GetAttributeInt("PreferredSortOrder");
            try
            {
                function.PreferredSortOrder = order.HasValue ? (SortOrder) order.Value : SortOrder.Descending;
            }
            catch
            {
            }
            function.MarginValue = node.GetAttributeFloat("MarginValue");
            return function;
        }

        public bool AreEqual(PerformerCriteriaFunction crit)
        {
            if (!Function.Equals(crit.Function, StringComparison.OrdinalIgnoreCase)) return false;

            var descA = Description ?? "";
            var descB = crit.Description ?? "";
            return descA.Trim().Equals(descB.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public bool FormulasEqual(string formula)
        {
            if (string.IsNullOrEmpty(formula)) return false;
            if (Function == formula) return true;
            var a = Function.Trim().ToLower();
            var b = formula.Trim().ToLower();
            return a == b;            
        }

        public static double Calculate(ExpressionResolver resolver, PerformerStat stat)
        {
            var ptrVal = new Dictionary<string, double>();
            try
            {
                foreach (var varName in resolver.GetVariableNames())
                {
                    var val = PerformerCriteriaFunctionCollection.Instance.propertyByVariable[varName].GetValue(stat, null);
                    double dVal = val is float
                                              ? (float)val
                                              : val is double
                                                    ? (double)val
                                                    : val is int ? (int)val : val is decimal ? (float)(decimal)val : 0;
                    ptrVal.Add(varName, dVal);
                }
                double rst;
                resolver.Calculate(ptrVal, out rst);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в Calculate(" + stat.Account + ")", ex);
                return 0;
            }
        }

        public override string ToString()
        {
            return Function;
        }
    }
}
