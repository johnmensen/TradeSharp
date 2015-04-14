using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Util;

namespace Candlechart.Series
{
    /// <summary>
    /// описывает параметры серии графика, указываемые при размещении/редактировании объекта
    /// например "тип: стрелка", "обводка: красная" ...
    /// </summary>
    // todo: отсутствует идентификация параметров, поэтому при смене локализации они не будут распознаны
    public class SeriesEditParameter
    {
        public string Name;

        public string title;

        public Type paramType;

        public object defaultValue;

        public static object TryGetParamValue(List<SeriesEditParameter> parameters, string ptrName)
        {
            if (parameters == null) return null;
            var ptr = parameters.FirstOrDefault(p => p.Name == ptrName);
            if (ptr == null) return null;
            return ptr.defaultValue;
        }

        public static T TryGetParamValue<T>(List<SeriesEditParameter> parameters, string ptrName, T defaultValue)
        {
            if (parameters == null) return defaultValue;
            var ptr = parameters.FirstOrDefault(p => p.Name == ptrName);
            if (ptr == null) return defaultValue;
            return (T)ptr.defaultValue;
        }

        public SeriesEditParameter()
        {
        }

        public SeriesEditParameter(string name, Type type, object defaultVal)
        {
            Name = name;
            title = Localizer.GetString("Title" + name);
            paramType = type;
            defaultValue = defaultVal;
        }
    }
}
