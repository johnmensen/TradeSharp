using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TradeSharp.Util
{
    public static class Converter
    {
        private delegate object ConverterDel(string valString);

        private delegate string StringifyDel(object obj);

        private static readonly Dictionary<Type, string> typeName =
            new Dictionary<Type, string>
                {
                    {typeof (decimal), "decimal"},
                    {typeof (DateTime), "DateTime"},
                    {typeof (bool), "bool"},
                    {typeof (int), "int"},
                    {typeof (double), "double"},
                    {typeof (float), "float"},
                    {typeof (short), "short"},
                    {typeof (long), "long"},
                    {typeof (byte), "byte"},
                    {typeof (uint), "uint"},
                    {typeof (ulong), "ulong"},
                    {typeof (string), "string"},
                    {typeof (Size), "Size"},
                    {typeof (int?), "int?"},
                    {typeof (decimal?), "decimal?"}
                };

        private static readonly Dictionary<string, ConverterDel> typeConvertor =
            new Dictionary<string, ConverterDel>
                {
                    {"decimal", valString => valString.ToDecimalUniformSafe()},
                    {"DateTime", valString => valString.ToDateTimeUniformSafe()},
                    {"bool", valString => valString.ToBoolSafe()},
                    {"int", valString => valString.ToIntSafe()},
                    {"double", valString => valString.ToDoubleUniformSafe()},
                    {"float", valString => valString.ToFloatUniformSafe()},
                    {"short", valString => (object) (short) valString.ToIntSafe()},
                    {"long", valString => valString.ToLongSafe()},
                    {"uint", valString => (object) (uint) valString.ToIntSafe()},
                    {"ulong", valString => (object) (ulong) valString.ToLongSafe()},
                    {
                        "Size", valString =>
                            {
                                var vals = valString.Split(new[] {',', ';'});
                                if (vals.Length < 2)
                                    return new Size();
                                return new Size(vals[0].ToInt(0), vals[1].ToInt(0));
                            }
                    },
                    {"int?", valString => string.IsNullOrEmpty(valString) ? (int?) null : valString.ToIntSafe()},
                    {
                        "decimal?",
                        valString =>
                        string.IsNullOrEmpty(valString) ? (decimal?) null : valString.ToDecimalUniformSafe()
                    }
                };

        private static readonly Dictionary<Type, StringifyDel> stringConvertor =
            new Dictionary<Type, StringifyDel>
                {
                    {typeof (decimal), o => ((decimal) o).ToStringUniform()},
                    {typeof (DateTime), o => ((DateTime) o).ToStringUniform()},
                    {typeof (bool), o => ((bool) o).ToString()},
                    {typeof (double), o => ((double) o).ToStringUniform()},
                    {typeof (float), o => ((float) o).ToStringUniform()},
                    {typeof (int), o => o.ToString()},
                    {typeof (short), o => o.ToString()},
                    {typeof (long), o => o.ToString()},
                    {typeof (uint), o => o.ToString()},
                    {typeof (ulong), o => o.ToString()},
                    {typeof (Size), o => ((Size) o).Width.ToString() + ";" + ((Size) o).Height.ToString()},
                    {typeof (int?), o => ((int?) o).HasValue ? ((int?) o).Value.ToString() : ""},
                    {typeof (decimal?), o => ((decimal?) o).HasValue ? ((decimal?) o).Value.ToString() : ""}
                };

        public static bool IsConvertable(Type type)
        {
            return typeName.ContainsKey(type);
        }

        public static object GetObjectFromString(string objTypeName, string val)
        {
            try
            {
                ConverterDel conv;
                if (typeConvertor.TryGetValue(objTypeName, out conv))
                    return conv(val);
            }
            catch
            {
                return val;
            }
            return val;
        }

        public static T GetObjectFromString<T>(string val)
        {
            if (typeof(T) == typeof (string)) return (T)(object)val;

            string typeStr;
            if (!typeName.TryGetValue(typeof (T), out typeStr))
                return default(T);
            try
            {
                ConverterDel conv;
                if (typeConvertor.TryGetValue(typeStr, out conv))
                    return (T)conv(val);
            }
            catch
            {
                return default(T);
            }
            return default(T);
        }

        public static object GetObjectFromString(string val, Type targetType)
        {
            if (targetType == typeof(string)) return val;

            string typeStr;
            if (!typeName.TryGetValue(targetType, out typeStr))
                return null;
            try
            {
                ConverterDel conv;
                if (typeConvertor.TryGetValue(typeStr, out conv))
                    return conv(val);
            }
            catch
            {
                return null;
            }
            return null;
        }

        public static string GetStringFromObject(object val)
        {
            if (val == null) return "";

            StringifyDel stringifier;
            return stringConvertor.TryGetValue(val.GetType(), out stringifier) ? stringifier(val) : val.ToString();
        }

        public static string GetObjectTypeName(object val)
        {
            string typeStr;
            return typeName.TryGetValue(val.GetType(), out typeStr) ? typeStr : val.GetType().Name;
        }

        public static List<Type> GetSupportedTypes()
        {
            return typeName.Keys.ToList();
        }

        public static List<string> GetSupportedTypeNames()
        {
            return typeName.Values.ToList();
        }

        /// <summary>
        /// Проверяет объект типа Т является ли он  ref-type или value-type
        /// </summary>
        /// <typeparam name="T">Параметризуемый тип объекта</typeparam>
        /// <param name="obj">объект, который проверяем на Nullable</param>
        public static bool IsNullable<T>(T obj)
        {
            if (obj == null) return true;
            var type = typeof(T);
            if (!type.IsValueType) return true;
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Проверяет тип Т является ли он  reference-type или value-type
        /// </summary>
        /// <param name="type">Тип, который проверяем на Nullable</param>
        public static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true;
            return Nullable.GetUnderlyingType(type) != null; //Не удалять (нужно на сайте администратора)
        }

        /// <summary>
        /// получение reference-type из строки
        /// </summary>
        /// <param name="val">Значение, которым нужно инициализировать объект</param>
        /// <param name="targetType">Тип объекта</param>
        public static object GetNullableObjectFromString(string val, Type targetType)
        {
            if (targetType == typeof(string)) return val;
            if (!IsNullable(targetType)) return GetObjectFromString(val, targetType);

            object result = null;
            string typeStr;
            if (typeName.TryGetValue(Nullable.GetUnderlyingType(targetType), out typeStr))
                result = GetObjectFromString(typeStr, val); //Boxing

            return result;
        }
    }
}
