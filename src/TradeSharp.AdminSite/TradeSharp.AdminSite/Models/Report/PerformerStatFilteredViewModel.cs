using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.Report
{
    /// <summary>
    /// Описывает текущие значения фильтров
    /// </summary>
    public partial class PerformerStatFilteredViewModel
    {
        public bool IsInitialized { get; set; }

        public enum SelectOrder { First = 0, Last = 1 }

        public SelectOrder Order { get; set; }

        public int CountAccount { get; set; }

        public List<PerformerStatRecord> Records { get; set; }

        public void FilterRecords(List<PerformerStatRecord> allRecords, out string errorStr)
        {
            var func = FilterFunction.FirstOrDefault(i => i.Value == FunctionSelectedComment) ?? FilterFunction[0];
            var formula = func.Text;

            var resolver = CheckFunction(formula, out errorStr);
            if (resolver == null) return;
            var resolverVars = resolver.GetVariableNames();

            var records = allRecords.ToList();
            var calcError = "";
            records.ForEach(r =>
                {
                    // заполнить переменные resolver-а
                    var varValue = new Dictionary<string, double>();
                    foreach (var name in resolverVars)
                    {
                        var val = formulaProperties[name].GetValue(r);
                        double dVal = val is float
                                          ? (float) val
                                          : val is double
                                                ? (double) val
                                                : val is int ? (int) val : val is decimal ? (float) (decimal) val : 0;
                        varValue.Add(name, dVal);
                    }
                    // посчитать
                    try
                    {
                        double rst;
                        resolver.Calculate(varValue, out rst);
                        r.FunctionValue = rst;
                    }
                    catch (Exception ex)
                    {
                        calcError = Resource.ErrorMessageCalculationFunctions + " '" + formula + "': " + ex.Message;
                        r.FunctionValue = 0;
                    }
                });
            if (!string.IsNullOrEmpty(calcError))
                errorStr = calcError;

            // вернуть первые N
            Records = (Order == SelectOrder.First
                                    ? records.OrderByDescending(r => r.FunctionValue)
                                    : records.OrderBy(r => r.FunctionValue)).Take(CountAccount).ToList();
        }

        public static ExpressionResolver CheckFunction(string formula, out string error)
        {
            error = string.Empty;
            
            ExpressionResolver resolver;
            try
            {
                resolver = new ExpressionResolver(formula);
            }
            catch (Exception ex)
            {
                error = Resource.ErrorMessageInFunction + " '" + formula + "': " + ex.Message;
                return null;
            }

            var resolverVars = resolver.GetVariableNames();
            if (resolverVars.Any(v => !formulaProperties.ContainsKey(v)))
            {
                error = Resource.ErrorMessageUnknownVariables + ": " +
                           string.Join(", ", resolverVars.Where(v => !formulaProperties.ContainsKey(v)));
                return null;
            }
            
            return resolver;
        }
    }
}