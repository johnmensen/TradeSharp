using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.BL.Settings;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.Report
{
    public partial class PerformerStatFilteredViewModel
    {
        private static readonly Dictionary<string, PropertyInfo> formulaProperties;

        public static readonly Dictionary<string, string> formulaVariableTitle;

        static PerformerStatFilteredViewModel()
        {
            FilterFunction = SiteSettings.Instance.FunctionSettings.Functions.Select(i => i.MakeSelectItem()).ToList();

            if (FilterFunction.Count == 0)
                FilterFunction = new List<SelectListItem>
                    {
                        new SelectListItem
                            {
                                Text = "ML",
                                Value = Resource.FilterFunctionGreatestLeverage,
                                Selected = true
                            },
                        new SelectListItem
                            {
                                Text = "P", 
                                Value = Resource.TitleProfit + ", %"
                            }
                    };
            formulaProperties = new Dictionary<string, PropertyInfo>();
            formulaVariableTitle = new Dictionary<string, string>();
            foreach (var prop in typeof(PerformerStatRecord).GetProperties())
            {
                var attr = prop.GetCustomAttribute<ExpressionParameterNameAttribute>();
                if (attr == null) continue;
                formulaProperties.Add(attr.ParamName.ToLower(), prop);
                formulaVariableTitle.Add(attr.ParamName.ToLower(), attr.ParamTitle);
            }
        }

        public static List<SelectListItem> FilterFunction { get; set; }

        public string FunctionSelectedComment { get; set; } 

        public static void AddFunction(string function, string description)
        {
            var func = FilterFunction.FirstOrDefault(f => f.Text == function);
            if (func != null)
                func.Value = description;
            else
                FilterFunction.Add(new SelectListItem { Value = description, Text = function });
            // сохранить настройки
            SiteSettings.Instance.FunctionSettings.Functions = FilterFunction.Select(i => new TraderFilterFunction(i)).ToList();
            SiteSettings.Instance.SaveSettings();
        }

        public static void DeleteFunction(string function)
        {
            if (FilterFunction.Count <= 1) return;

            var func = FilterFunction.FirstOrDefault(f => f.Text == function);
            if (func == null) return;
            FilterFunction.Remove(func);
            
            // сохранить настройки
            SiteSettings.Instance.FunctionSettings.Functions = FilterFunction.Select(i => new TraderFilterFunction(i)).ToList();
            SiteSettings.Instance.SaveSettings();
        }
    }
}