
using System;
using System.Globalization;

namespace FastMultiChart
{
    public class FastMultiChartUtils
    {
        public static int GetDateTimeScaleValue(object value, FastMultiChart chart)
        {
            return (int) ((DateTime) value).Subtract(new DateTime()).TotalDays;
        }

        public static object GetDateTimeValue(int scaleValue, FastMultiChart chart)
        {
            return (new DateTime()).AddDays(scaleValue);
        }

        public static object GetDateTimeDivisionValue(object value, FastMultiChart chart)
        {
            if (chart.ScaleDivisionX == 31)
            {
                var date = (DateTime) value;
                return new DateTime(date.Year, date.Month, 1);
            }
            if (chart.ScaleDivisionX == 366)
            {
                var date = (DateTime) value;
                return new DateTime(date.Year, 1, 1);
            }
            return value;
        }

        public static object GetDateTimeMinScaleDivision(int expectedValue, FastMultiChart chart)
        {
            if (expectedValue > 7 && expectedValue < 28)
                expectedValue = 7;
            else if (expectedValue >= 28 && expectedValue < 365)
                expectedValue = 31;
            else if (expectedValue >= 365)
                expectedValue = 366;
            return (new DateTime()).AddDays(expectedValue);
        }

        public static object GetDoubleMinScaleDivision(int expectedValue, FastMultiChart chart)
        {
            var minDivision = Convert.ToDouble(
                chart.GetYValue((int)Math.Ceiling(chart.ScaleDivisionYMinPixel / chart.Graphs[0].ScaleY), chart));
            var maxDivision = Convert.ToDouble(
                chart.GetYValue((int)Math.Floor((chart.ScaleDivisionYMaxPixel == -1
                                                      ? chart.ClientSize.Height - chart.MarginYBottom - chart.MarginYTop -
                                                        chart.LabelMarginY - chart.ScrollBarHeight
                                                      : chart.ScaleDivisionYMaxPixel) / chart.Graphs[0].ScaleY), chart));
            var divisionRange = maxDivision - minDivision;
            if (divisionRange <= 0)
                return (double) 0;
            var valuableDigitCount = (int) Math.Ceiling(Math.Log10(divisionRange));
            var divisionIncrement = Math.Pow(10, valuableDigitCount);
            var calculatedDivision = divisionIncrement;
            while (true)
            {
                if (calculatedDivision >= minDivision && calculatedDivision <= maxDivision)
                    return calculatedDivision;
                calculatedDivision += divisionIncrement;
                if (calculatedDivision > maxDivision)
                {
                    divisionIncrement /= 10.0;
                    calculatedDivision = divisionIncrement;
                }
            }
        }

        public static string GetDateTimeStringValue(object value, FastMultiChart chart)
        {
            return ((DateTime) value).ToString("dd.MM.yyyy");
        }

        public static string GetDateTimeStringScaleValue(object value, FastMultiChart chart)
        {
            if (chart.ScaleDivisionX == 31)
                return ((DateTime) value).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("ru"));
            if (chart.ScaleDivisionX == 366)
                return ((DateTime) value).ToString("yyyy");
            return ((DateTime) value).ToString("dd.MM.yyyy");
        }
    }

    /// <summary>
    /// Структура из двух произвольных полей
    /// </summary>    
    [Serializable]
    public struct Cortege2<A, B>
    {
        public A a { get; set; }
        public B b { get; set; }
        public Cortege2(A _a, B _b)
            : this()
        {
            a = _a;
            b = _b;
        }
        public bool IsDefault()
        {
            return Equals(default(Cortege2<A, B>));
        }
        public override string ToString()
        {
            return string.Format("{0};{1}", a, b);
        }
    }

    /// <summary>
    /// Структура из трех произвольных полей
    /// </summary>    
    [Serializable]
    public struct Cortege3<A, B, C>
    {
        public A a { get; set; }
        public B b { get; set; }
        public C c { get; set; }
        public Cortege3(A _a, B _b, C _c)
            : this()
        {
            a = _a;
            b = _b;
            c = _c;
        }
        public override string ToString()
        {
            return string.Format("{0};{1};{2}", a, b, c);
        }
    }
}
