using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    public class UiFieldAttribute : Attribute
    {
        public class PropertyValue
        {
            public string PropName { get; set; }

            public string DisplayName { get; set; }

            public object Value { get; set; }

            public Type PropType { get; set; }
        }

        public string DisplayName { get; set; }

        public int? Category { get; set; }

        public UiFieldAttribute()
        {            
        }

        public UiFieldAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public UiFieldAttribute(object category)
        {
            Category = Convert.ToInt32(category);
        }

        public UiFieldAttribute(object category, string displayName)
        {
            Category = Convert.ToInt32(category);
            DisplayName = displayName;
        }

        public static List<PropertyValue> GetAttributeNameValue(object src)
        {
            var atrValues = new List<PropertyValue>();
            foreach (var pro in src.GetType().GetProperties())
            {
                var attr = (UiFieldAttribute)pro.GetCustomAttributes(typeof(UiFieldAttribute), true).FirstOrDefault();
                if (attr == null) continue;
                var val = pro.GetValue(src, null);
                atrValues.Add(new PropertyValue
                {
                    DisplayName = attr.DisplayName, 
                    PropType = pro.PropertyType, 
                    Value = val,
                    PropName = pro.Name
                });
            }
            return atrValues;
        }

        public static void SetFieldsFromPropNameValue(Dictionary<string, string> propVal, object dest, bool setNullValues)
        {
            var destProps = dest.GetType().GetProperties().Where(p => p.GetCustomAttributes(
                typeof (UiFieldAttribute), true).Any()).ToDictionary(p => p.Name, p => p);
            
            foreach (var proVal in propVal)
            {
                try
                {
                    var prop = destProps[proVal.Key];
                    var propType = prop.PropertyType;
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof (Nullable<>))
                        propType = Nullable.GetUnderlyingType(propType);
                    var valObj = Converter.GetObjectFromString(proVal.Value, propType);
                    if (valObj == null && !setNullValues) continue;
                    prop.SetValue(dest, valObj, null);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("SetFieldsFromPropNameValue({0}) failed on prop {1}: {2}",
                        dest.GetType().Name, proVal.Key, ex);
                }
            }
        }
    }
}
