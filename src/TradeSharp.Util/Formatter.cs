using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace TradeSharp.Util
{
    public static class UniFormatterExtensions
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

        static UniFormatterExtensions()
        {
            formatNumberCurrency = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
            formatNumberCurrency.NumberGroupSeparator = " ";
        }

        #endregion

        #region ToString
        public static string ToStringUniform(this decimal num)
        {
            return num.ToString(CultureProvider.Common);
        }

        public static string ToStringUniform(this double num)
        {
            return num.ToString(CultureProvider.Common);
        }

        public static string ToStringUniform(this float num)
        {
            return num.ToString(CultureProvider.Common);
        }

        public static string ToStringUniform(this decimal num, int precision)
        {
            var fmt = "f" + precision;
            return num.ToString(fmt, CultureProvider.Common);
        }

        public static string ToStringUniform(this float num, int precision)
        {
            var fmt = "f" + precision;
            return num.ToString(fmt, CultureProvider.Common);
        }

        public static string ToStringUniform(this decimal? num)
        {
            return num.HasValue ? num.Value.ToString(CultureProvider.Common) : "";
        }

        public static string ToStringUniform(this double num, int precision)
        {
            var fmt = "f" + precision;
            return num.ToString(fmt, CultureProvider.Common);
        }

        public static string ToStringUniform(this Size sz)
        {
            return sz.Width + ";" + sz.Height;
        }

        public static string ToStringUniform(this SizeF sz)
        {
            return sz.Width.ToStringUniform() + ";" + sz.Height.ToStringUniform();
        }

        public static string ToStringUniformMoneyFormat(this decimal num, bool needCents = true)
        {
            return needCents
                       ? (num.ToString("n2", formatNumberCurrency))
                       : (num.ToString("n0", formatNumberCurrency));
        }

        public static string ToStringUniformMoneyFormat(this float num, bool needCents = true)
        {
            return needCents
                       ? (num.ToString("n2", formatNumberCurrency))
                       : (num.ToString("n0", formatNumberCurrency));
        }

        public static string ToStringUniformMoneyFormat(this double num, bool needCents = true)
        {
            return needCents
                       ? (num.ToString("n2", formatNumberCurrency))
                       : (num.ToString("n0", formatNumberCurrency));
        }

        public static string ToStringUniformMoneyFormat(this int num)
        {
            return num.ToString("n0", formatNumberCurrency);
        }

        public static string ToStringUniformMoneyFormat(this long num)
        {
            return num.ToString("n0", formatNumberCurrency);
        }
        
        public static string ToStringUniform(this IEnumerable <int> numbers, string delimiter)
        {
            var res = new StringBuilder();
            var startFlag = true;
            foreach(var number in numbers)
            {
                if (!startFlag)
                {
                    res.Append(delimiter);
                    
                }
                startFlag = false;
                res.Append(number.ToString());
            }
            return res.ToString();
        }

        public static string ToStringUniform(this IEnumerable<decimal> numbers, string delimiter)
        {
            var res = new StringBuilder();
            var startFlag = true;
            foreach (var number in numbers)
            {
                if (!startFlag)
                {
                    res.Append(delimiter);

                }
                startFlag = false;
                res.Append(number.ToStringUniform());
            }
            return res.ToString();
        }

        public static string ToStringUniform(this IEnumerable<double> numbers, string delimiter)
        {
            var res = new StringBuilder();
            var startFlag = true;
            foreach (var number in numbers)
            {
                if (!startFlag)
                {
                    res.Append(delimiter);

                }
                startFlag = false;
                res.Append(number.ToStringUniform());
            }
            return res.ToString();
        }

        public static string ToStringUniform(this IEnumerable<float> numbers, string delimiter)
        {
            var res = new StringBuilder();
            var startFlag = true;
            foreach (var number in numbers)
            {
                if (!startFlag)
                {
                    res.Append(delimiter);

                }
                startFlag = false;
                res.Append(number.ToStringUniform());
            }
            return res.ToString();
        }

        public static string ToStringUniformPriceFormat(this float price, bool extraDigit = false)
        {
            return price > 35
                ? price.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price > 7 ? price.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }

        public static string ToStringUniformPriceFormat(this float? price, string nullStr, bool extraDigit = false)
        {
            return !price.HasValue ? nullStr :
                price.Value > 25
                ? price.Value.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price.Value > 7 ? price.Value.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.Value.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }

        public static string ToStringUniformPriceFormat(this double price, bool extraDigit = false)
        {
            return price > 25
                ? price.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price > 7 ? price.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }

        public static string ToStringUniformPriceFormat(this double? price, string nullStr, bool extraDigit = false)
        {
            return !price.HasValue ? nullStr :
                price.Value > 25
                ? price.Value.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price.Value > 7 ? price.Value.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.Value.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }

        public static string ToStringUniformPriceFormat(this decimal price, bool extraDigit = false)
        {
            return price > 25
                ? price.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price > 7 ? price.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }

        public static string ToStringUniformPriceFormat(this decimal? price, string nullStr, bool extraDigit = false)
        {
            return !price.HasValue ? nullStr :
                price.Value > 25
                ? price.Value.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price.Value > 7 ? price.Value.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.Value.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }
        
        #endregion

        #region ToTarget

        public static double ToExpressionResult(this string expression, double defaultValue)
        {
            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(expression);
            }
            catch
            {
                return defaultValue;
            }

            try
            {
                double rst;
                return resv.Calculate(new Dictionary<string, double>(), out rst) ? rst : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

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
            if (!int.TryParse(digitStr.ToString(), out result))
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

        public static Size? ToSizeSafe(this string str)
        {
            var numbers = str.ToIntArrayUniform();
            return numbers.Length == 2 ? new Size(numbers[0], numbers[1]) : (Size?) null;
        }

        public static SizeF? ToSizeFSafe(this string str)
        {
            var numbers = str.ToFloatArrayUniform();
            return numbers.Length == 2 ? new SizeF(numbers[0], numbers[1]) : (SizeF?)null;
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

        public static DateTime? ToDateTimeUniformSafeMils(this string str)
        {
            DateTime result;
            return DateTime.TryParseExact(str, "dd.MM.yyyy HH:mm:ss.fff", CultureProvider.Common, DateTimeStyles.None,
                out result) ? (DateTime?)result : null;
        }

        public static DateTime? ToDateTimeUniformSafe(this string str, DateTimeFormatInfo dateTimeFormatInfo)
        {
            if (string.IsNullOrEmpty(str))
            {
                Logger.Error("ConvertStrToDateTime(). Не удалось распарсить дату. Переданная строка null или пуста");
                return null;
            }
            try
            {
                DateTime result;
                if (DateTime.TryParse(str, dateTimeFormatInfo ?? DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out result)) return result;
            }
            #region catch
            catch (Exception ex)
            {
                Logger.Error("ConvertStrToDateTime(). Не удалось распарсить дату.", ex);
                return null;
            }
            #endregion
            Logger.ErrorFormat("Не удалось распарсить дату {0}. Возможно не передан параметр dateTimeFormatInfo. Если дата в региональном формате (например, '16.01.2014' или '16.01.2014 22:14:15'), то этот параметр необходим.", str);
            return null;
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

        public static string ToStringUniformMils(this DateTime time)
        {
            return time.ToString("dd.MM.yyyy HH:mm:ss.fff", CultureProvider.Common);
        }

        public static string ToStringUniform(this TimeSpan time, bool needMinutes, bool needSeconds)
        {
            var sb = new StringBuilder();
            if (time.Days > 0) sb.Append(time.Days + " д. ");
            if (time.Hours > 0) sb.Append(time.Hours + " ч. ");
            if (needMinutes)
            {
                if (time.Minutes > 0) sb.Append(time.Minutes + " м. ");
                if (needSeconds)
                    if (time.Seconds > 0) sb.Append(time.Seconds + " с. ");
            }
            return sb.Length == 0 ? "-" : sb.ToString();
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

        #region Comparison
        public static bool SameMoney(this float amount, float cmpAmount)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < 0.01f;
        }

        public static bool SameMoney(this double amount, double cmpAmount)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < 0.01d;
        }

        public static bool SameMoney(this decimal amount, decimal cmpAmount)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < 0.01m;
        }

        public static bool RoughCompares(this float amount, float cmpAmount, float maxDelta)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < maxDelta;
        }

        public static bool RoughCompares(this double amount, double cmpAmount, double maxDelta)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < maxDelta;
        }

        public static bool RoughCompares(this decimal amount, decimal cmpAmount, decimal maxDelta)
        {
            return Math.Abs(Math.Abs(amount) - Math.Abs(cmpAmount)) < maxDelta;
        }

        public static bool SameDouble(this double amount, double cmpAmount, double precision = 0.00005)
        {
            return Math.Abs(amount - cmpAmount) < precision;
        }

        public static bool SameDouble(this float amount, float cmpAmount, double precision = 0.00005)
        {
            return Math.Abs(amount - cmpAmount) < precision;
        }

        #endregion
    }  

    public static class CultureProvider
    {
        public static CultureInfo Common = CultureInfo.InvariantCulture;
    }
}
