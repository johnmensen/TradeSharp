using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace FastGrid
{
    static class Formatter
    {
        #region Classes, members, ctor
        class StringWriterUtf8 : StringWriter
        {
            private readonly Encoding encoding;

            public StringWriterUtf8(StringBuilder sb, Encoding encoding)
                : base(sb)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }

        private static readonly NumberFormatInfo formatNumberCurrency;

        static Formatter()
        {
            formatNumberCurrency = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
            formatNumberCurrency.NumberGroupSeparator = " ";
        }

        #endregion


        #region ToTarget

        public static int ToInt(this string numStr)
        {
            return int.Parse(numStr);
        }

        public static bool ToBool(this string boolStr)
        {
            return bool.Parse(boolStr);
        }

        public static bool? ToBoolSafe(this string boolStr)
        {
            bool result;
            if (Boolean.TryParse(boolStr, out result)) return result;
            return null;
        }

        public static int? ToIntSafe(this string numStr)
        {
            int val;
            if (!int.TryParse(numStr, out val)) return null;
            return val;
        }

        public static int? ToShortSafe(this string numStr)
        {
            short val;
            if (!short.TryParse(numStr, out val)) return null;
            return val;
        }

        public static long? ToLongSafe(this string numStr)
        {
            long val;
            if (!long.TryParse(numStr, out val)) return null;
            return val;
        }

        public static int ToInt(this string numStr, int defaultValue)
        {
            if (string.IsNullOrEmpty(numStr)) return defaultValue;
            var digitStr = new StringBuilder();
            foreach (var c in numStr)
                if ((c >= '0' && c <= '9') || c == '-') digitStr.Append(c);

            int result = defaultValue;
            if (!int.TryParse(digitStr.ToString(), out defaultValue))
                result = defaultValue;

            return result;
        }

        public static decimal ToDecimalUniform(this string numStr)
        {
            return decimal.Parse(numStr, CultureProvider.Common);
        }

        public static decimal? ToDecimalUniformSafe(this string numStr)
        {
            decimal result;
            if (decimal.TryParse(numStr.Replace(',', '.'), NumberStyles.Any, CultureProvider.Common, out result))
                return result;
            return null;
        }

        public static double? ToDoubleUniformSafe(this string numStr)
        {
            double result;
            if (double.TryParse(numStr.Replace(',', '.'), NumberStyles.Any, CultureProvider.Common, out result))
                return result;
            return null;
        }

        public static float ToFloatUniform(this string numStr)
        {
            return float.Parse(numStr, CultureProvider.Common);
        }

        public static float? ToFloatUniformSafe(this string numStr)
        {
            float result;
            if (float.TryParse(numStr.Replace(',', '.'), NumberStyles.Any, CultureProvider.Common, out result))
                return result;
            return null;
        }

        /// <summary>
        /// выбрать все числа, содержащиеся в строке
        /// </summary>        
        public static decimal[] ToDecimalArrayUniform(this string numStr)
        {
            if (string.IsNullOrEmpty(numStr)) return new decimal[0];
            var numbers = new List<decimal>();
            var numPart = "";
            decimal num;
            for (var i = 0; i < numStr.Length; i++)
            {
                if (numStr[i] == '.' || numStr[i] == '-' ||
                    (numStr[i] >= '0' && numStr[i] <= '9'))
                {
                    numPart = numPart + numStr[i];
                    continue;
                }

                if (decimal.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                    numbers.Add(num);
                numPart = "";
            }
            if (decimal.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                numbers.Add(num);
            return numbers.ToArray();
        }

        /// <summary>
        /// выбрать все числа, содержащиеся в строке
        /// </summary>        
        public static double[] ToDoubleArrayUniform(this string numStr)
        {
            if (string.IsNullOrEmpty(numStr)) return new double[0];
            var numbers = new List<double>();
            var numPart = "";
            double num;
            for (var i = 0; i < numStr.Length; i++)
            {
                if (numStr[i] == '.' || numStr[i] == '-' ||
                    (numStr[i] >= '0' && numStr[i] <= '9'))
                {
                    numPart = numPart + numStr[i];
                    continue;
                }

                if (double.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                    numbers.Add(num);
                numPart = "";
            }
            if (double.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                numbers.Add(num);
            return numbers.ToArray();
        }

        /// <summary>
        /// выбрать все числа, содержащиеся в строке
        /// </summary>        
        public static float[] ToFloatArrayUniform(this string numStr)
        {
            if (string.IsNullOrEmpty(numStr)) return new float[0];
            var numbers = new List<float>();
            var numPart = "";
            float num;
            for (var i = 0; i < numStr.Length; i++)
            {
                if (numStr[i] == '.' || numStr[i] == '-' ||
                    (numStr[i] >= '0' && numStr[i] <= '9'))
                {
                    numPart = numPart + numStr[i];
                    continue;
                }

                if (float.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                    numbers.Add(num);
                numPart = "";
            }
            if (float.TryParse(numPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out num))
                numbers.Add(num);
            return numbers.ToArray();
        }

        public static int[] ToIntArrayUniform(this string numStr)
        {
            if (string.IsNullOrEmpty(numStr)) return new int[0];
            var numbers = new List<int>();
            var numPart = "";
            int num;
            for (var i = 0; i < numStr.Length; i++)
            {
                if (numStr[i] == '-' || (numStr[i] >= '0' && numStr[i] <= '9'))
                {
                    numPart = numPart + numStr[i];
                    continue;
                }

                if (int.TryParse(numPart, out num)) numbers.Add(num);
                numPart = "";
            }
            if (int.TryParse(numPart, out num)) numbers.Add(num);
            return numbers.ToArray();
        }

        public static double ToDoubleUniform(this string numStr)
        {
            return double.Parse(numStr, CultureInfo.InvariantCulture);
        }

        public static string[] CastToStringArrayUniform<T>(this IEnumerable<T> coll)
            where T : IFormattable
        {
            var outLst = new List<string>();
            foreach (IFormattable item in coll)
                outLst.Add(item.ToString(null, CultureInfo.InvariantCulture));
            return outLst.ToArray();
        }

        public static List<string> CastToStringListUniform<T>(this IEnumerable<T> coll)
            where T : IFormattable
        {
            var outLst = new List<string>();
            foreach (IFormattable item in coll)
                outLst.Add(item.ToString(null, CultureInfo.InvariantCulture));
            return outLst;
        }


        public static DateTime ToDateTimeUniform(this string str)
        {
            return DateTime.ParseExact(str, "dd.MM.yyyy HH:mm:ss", CultureProvider.Common);
        }

        public static DateTime? ToDateTimeUniformSafe(this string str)
        {
            DateTime result;
            return DateTime.TryParseExact(str, "dd.MM.yyyy HH:mm:ss", CultureProvider.Common, DateTimeStyles.None,
                out result) ? (DateTime?)result : null;
        }

        public static DateTime ToDateTimeDefault(this string str, DateTime defaultDate)
        {
            DateTime result;
            return DateTime.TryParseExact(str, "dd.MM.yyyy HH:mm:ss", CultureProvider.Common, DateTimeStyles.None,
                out result) ? result : defaultDate;
        }

        public static string ToStringUniform(this DateTime time)
        {
            return time.ToString("dd.MM.yyyy HH:mm:ss", CultureProvider.Common);
        }

        public static string ToStringUniform(this XmlDocument doc, Encoding encoding, bool indentation)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriterUtf8(sb, encoding))
            {
                if (!indentation) doc.Save(sw);
                else
                    using (var xtw = new XmlTextWriter(sw) { Indentation = 4 })
                    {
                        doc.Save(xtw);
                    }
            }
            return sb.ToString();
        }

        public static string ToStringUniform(this XmlDocument doc, Encoding encoding)
        {
            return ToStringUniform(doc, encoding, false);
        }

        public static string ToStringUniform(this XmlDocument doc)
        {
            return ToStringUniform(doc, Encoding.UTF8, false);
        }

        #endregion
    }
    
    static class CultureProvider
    {
        public static CultureInfo Common = CultureInfo.InvariantCulture;
    }
}
