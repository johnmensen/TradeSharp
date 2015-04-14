using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace TradeSharp.Util
{
    public class ColumnFilter
    {
        public string Title { get; set; }
        [Browsable(false)]
        public string Category { get; set; }
        [Browsable(false)]
        public PropertyInfo PropInfo { get; set; }
        public ColumnFilterCriteria Criterias { get; set; }
        public ColumnFilterCriteria EnabledCriterias { get; set; }
        public Object Value { get; set; } // значение фильтра заполняется в форме настройки фильтра
        public Object[] EnabledValues { get; set; } // разрешенные значения
        [Browsable(false)]
        public bool AutoCheck { get; set; }
        public ColumnFilter(string title, string category, PropertyInfo propInfo, ColumnFilterCriteria enabledCriterias,
            Object[] enabledValues, bool autoCheck)
        {
            Title = title;
            Category = category;
            PropInfo = propInfo;
            EnabledCriterias = enabledCriterias;
            EnabledValues = enabledValues;
            AutoCheck = autoCheck;
        }

        public string[] GetStringCriterias()
        {
            return (from ColumnFilterCriteria c in Enum.GetValues(typeof (ColumnFilterCriteria))
                       where EnabledCriterias.HasFlag(c)
                       select c.ToString()).ToArray();
            
        }
    }

    public class EntityFilter
    {
        public Dictionary<string, ColumnFilter> ColumnFilters { get; set; }
        public EntityFilter(Type specType)
        {
            ColumnFilters = new Dictionary<string, ColumnFilter>();
            // заполняем словарь - имя свойства - натравленный фильтр на него ColumnFilter   
            foreach (var property in specType.GetProperties())
            {
                foreach (var attr in property.GetCustomAttributes(true).Where(customAttribute => 
                    customAttribute is EntityFilterAttribute != false).Cast<EntityFilterAttribute>())
                {
                    var crit = attr.EnabledCriterias;
                    if (attr.EnabledCriterias == ColumnFilterCriteria.Нет)
                        crit = EntityFilter.DeriveCriteriasFromType(property.PropertyType);
                    
                    ColumnFilters.Add(property.Name, new ColumnFilter(attr.Title, attr.Category ?? String.Empty,
                                      property, crit, 
                    attr.EnabledValues.ToArray(), attr.CheckAuto));
                }
            } 
         
        }

        private static ColumnFilterCriteria DeriveCriteriasFromType(Type propType)
        {
            if (propType == typeof(string))
                return ColumnFilterCriteria.Нет | ColumnFilterCriteria.КончаетсяНа | ColumnFilterCriteria.НачинаетсяС | 
                    ColumnFilterCriteria.НеРавно | ColumnFilterCriteria.Равно | ColumnFilterCriteria.Включает;
            if (propType == typeof(int?) || propType == typeof(int) || propType == typeof(float?) || propType == typeof(float) ||
                propType == typeof(double?) || propType == typeof(double) || propType == typeof(decimal?) ||
                propType == typeof(decimal))
                return ColumnFilterCriteria.Нет | ColumnFilterCriteria.Равно | ColumnFilterCriteria.НеРавно |
                       ColumnFilterCriteria.Меньше | ColumnFilterCriteria.Больше;
            if (propType == typeof(DateTime?) || propType == typeof(DateTime))
                return ColumnFilterCriteria.Нет | ColumnFilterCriteria.Равно | ColumnFilterCriteria.НеРавно |
                       ColumnFilterCriteria.Больше | ColumnFilterCriteria.Меньше;
            return ColumnFilterCriteria.Нет;
        }

        public bool PredicateFunc(object obj)
        {
            foreach (var filter in ColumnFilters.Values.Where(filter => filter.AutoCheck))
            {
                if (filter.Criterias == ColumnFilterCriteria.Нет)
                    continue;
                var objValue = filter.PropInfo.GetValue(obj, null);
                if (objValue is string)
                    if (!CheckStringCriteria(filter.Criterias, (string)objValue, (string)filter.Value))
                        return false;
                if (objValue is int?)
                {
                    if (filter.Value is string)
                        return CheckStringCriteria(filter.Criterias, (int?)objValue, (string)filter.Value);
                    if (!CheckStringCriteria(filter.Criterias, (int?)objValue, (int?)filter.Value))
                        return false;
                }

                if (objValue is double?)
                    if (!CheckStringCriteria(filter.Criterias, (double?)objValue, (double?)filter.Value))
                        return false;
                if (objValue is float?)
                    if (!CheckStringCriteria(filter.Criterias, (float?)objValue, (float?)filter.Value))
                        return false;
                if (objValue is decimal?)
                    if (!CheckStringCriteria(filter.Criterias, (decimal?)objValue, (decimal?)filter.Value))
                        return false;
                if (objValue is DateTime?)
                    if (!CheckStringCriteria(filter.Criterias, (DateTime?)objValue, (DateTime?)filter.Value))
                        return false;
            }
            return true; // включить в выборку
        }

        #region CheckStringCriteria
        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, string value, string specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Включает && value.Contains(specimen) == false) return false;
            if (crit == ColumnFilterCriteria.НачинаетсяС && value.StartsWith(specimen) == false) return false;
            if (crit == ColumnFilterCriteria.КончаетсяНа && value.EndsWith(specimen) == false) return false;
            return true;
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, int? value, int? specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Больше && value <= specimen) return false;
            if (crit == ColumnFilterCriteria.Меньше && value >= specimen) return false;
            return true;
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, int? value, string specimen)
        {
            var values = specimen.ToIntArrayUniform();
            if (crit == ColumnFilterCriteria.Включает)
            {
                return values.Any(i => crit == ColumnFilterCriteria.Включает && value == i);
            }
            return crit == ColumnFilterCriteria.НеВключает && values.All(i => i != value);
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, double? value, double? specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Больше && value <= specimen) return false;
            if (crit == ColumnFilterCriteria.Меньше && value >= specimen) return false;
            return true;
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, float? value, float? specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Больше && value <= specimen) return false;
            if (crit == ColumnFilterCriteria.Меньше && value >= specimen) return false;
            return true;
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, decimal? value, decimal? specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Больше && value <= specimen) return false;
            if (crit == ColumnFilterCriteria.Меньше && value >= specimen) return false;
            return true;
        }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, DateTime? value, DateTime? specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Больше && value <= specimen) return false;
            if (crit == ColumnFilterCriteria.Меньше && value >= specimen) return false;
            return true;
        }
        #endregion
    }

    /*
    class A
    {
        // [EntityFilter(Category="", Title="№", EnabledCriterias = Равно | НеРавно)] - у атрибута добавить GetEnabledFilterCriterias()
        public int Id { get; set; }

        // [EntityFilter(EnabledValues = new object[] {"Sir.", "Mr.", "..."}, MatchCase = false)
        public string Salutation { get; set; }

        // [EntityFilter(CheckAuto = false)]
        public int AccountId { get; set; }
    } */
}
