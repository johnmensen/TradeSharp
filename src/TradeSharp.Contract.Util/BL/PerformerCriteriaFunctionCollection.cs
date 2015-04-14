using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    public class PerformerCriteriaFunctionCollection
    {
        private static PerformerCriteriaFunctionCollection instance;

        public static PerformerCriteriaFunctionCollection Instance
        {
            get { return instance ?? (instance = new PerformerCriteriaFunctionCollection()); }
        }

        private int selectedFunctionIndex;

        public PerformerCriteriaFunction SelectedFunction
        {
            get
            {
                return selectedFunctionIndex < 0 || selectedFunctionIndex >= criterias.Count
                           ? obligatoryFunctions[0]
                           : criterias[selectedFunctionIndex];
            }
            set
            {
                var index = criterias.FindIndex(c => c.AreEqual(value));
                if (index >= 0)
                    selectedFunctionIndex = index;
            }
        }

        public List<PerformerCriteriaFunction> criterias;

        public readonly List<string> enabledParametersNames;

        private readonly string fileName;

        public readonly Dictionary<string, PropertyInfo> propertyByVariable;

        #region Obligatory functions
        private static readonly PerformerCriteriaFunction[] obligatoryFunctions = new[]
            {
                new PerformerCriteriaFunction
                    {
                        Function = "P",
                        Description = "Все",
                        PreferredSortOrder = SortOrder.Descending,
                        MarginValue = null
                    },
                new PerformerCriteriaFunction
                    {
                        Function = "AYP/DD*(ML<10)",
                        Description =
                            "С. геом год. доходность, деленная на макс. отн. проседание. Только для счетов с макс. торговым плечом < 10 (умеренный риск).",
                        PreferredSortOrder = SortOrder.Descending,
                        MarginValue = 0
                    },
                new PerformerCriteriaFunction
                    {
                        Function = "P",
                        Description = "Прибыль, %",
                        PreferredSortOrder = SortOrder.Descending,
                        MarginValue = 0
                    },
                new PerformerCriteriaFunction
                    {
                        Function = "AYP",
                        Description = "Среднегеометрическая годовая доходность, %",
                        PreferredSortOrder = SortOrder.Descending,
                        MarginValue = 0
                    },
                new PerformerCriteriaFunction
                    {
                        Function = "AYP*(DD<30)",
                        Description = "Среднегеометрическая годовая доходность + просадка < 30%",
                        PreferredSortOrder = SortOrder.Descending,
                        MarginValue = 0
                    },
                new PerformerCriteriaFunction
                    {
                        Function = "((ML<33)??-1) + ((DD<33)??-1) + Sign(PnA) + ((WPtL>1)??-1) + (WN/(PnA??1)>0.49)",
                        Description =
                            "Критерий Winner / Looser (учитывает прибыль за 3 месяца, плечо, проседание, вывод средств и распределение профитов / убытков).",
                        PreferredSortOrder = SortOrder.Ascending,
                        MarginValue = null
                    }
            };
        #endregion

        private PerformerCriteriaFunctionCollection()
        {
            enabledParametersNames = PerformerStatField.fields.Where(f =>
                !String.IsNullOrEmpty(f.ExpressionParamName)).Select(f => f.ExpressionParamName).ToList();
            
            // словарь имя переменной - свойство
            propertyByVariable = new Dictionary<string, PropertyInfo>();
            foreach (var propertyInfo in typeof (PerformerStat).GetProperties())
            {
                var attrs = propertyInfo.GetCustomAttributes(typeof (ExpressionParameterNameAttribute), true);
                if (attrs.Length == 1)
                    propertyByVariable.Add(((ExpressionParameterNameAttribute)attrs[0]).ParamName.ToLower(), propertyInfo);
            }

            fileName = ExecutablePath.ExecPath + "\\filters.txt";
            ReadFromFile();
        }

        /// <summary>
        /// прочитать формулы - описания из строки
        /// </summary>
        public void ReadFromFile()
        {
            criterias = new List<PerformerCriteriaFunction>();
            
            if (File.Exists(fileName))
                try
                {
                    var doc = new XmlDocument();
                    using (var sr = new StreamReader(fileName, Encoding.UTF8))
                        doc.Load(sr);

                    if (doc.DocumentElement != null)
                    {
                        selectedFunctionIndex = doc.DocumentElement.GetAttributeInt("selected") ?? 0;

                        foreach (XmlElement child in doc.DocumentElement.ChildNodes)
                        {
                            var crit = PerformerCriteriaFunction.ReadFromXml(child);
                            if (crit == null) continue;
                            // проверка на дубль и валидность и добавление
                            if (criterias.Any(c => c.AreEqual(crit))) continue;

                            string error;
                            if (!ExpressionResolver.CheckFunction(crit.Function, out error, enabledParametersNames))
                                continue;
                            criterias.Add(crit);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в PerformerCriteriaFunction - ReadFromFile", ex);
                }

            // добавить обязательные функции
            foreach (var fun in obligatoryFunctions)
            {
                var formula = fun.Function;
                if (criterias.Any(c => c.Function.Equals(formula, StringComparison.OrdinalIgnoreCase))) continue;
                criterias.Add(fun);
            }
        }
    
        public void WriteToFile()
        {
            try
            {
                var doc = new XmlDocument();
                var root = (XmlElement)doc.AppendChild(doc.CreateElement("functions"));
                root.Attributes.Append(doc.CreateAttribute("selected")).Value = selectedFunctionIndex.ToString();

                foreach (var crit in criterias)
                {
                    crit.WriteToXml(root);
                }

                using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                    {
                        doc.Save(xw);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в PerformerCriteriaFunction - WriteToFile", ex);                
            }
        }
    
        /// <summary>
        /// оставить только обязательный (краткий) список критериев
        /// </summary>
        public void ResetCriterias()
        {
            criterias = obligatoryFunctions.ToList();
        }
    
        public bool RemoveCriteria(string criteriaTitle)
        {
            if (criterias.Count < 1) return false;
            var crit = criterias.FirstOrDefault(c => c.Description == criteriaTitle);
            if (crit == null) return false;
            criterias.Remove(crit);
            return true;
        }
    }
}
