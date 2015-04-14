using System;
using System.Linq;

namespace TradeSharp.Util
{
    /// <summary>
    /// стандартный набор параметров для ExpressionResolver, включает
    /// дату-время, random
    /// </summary>
    public static class ExpressionResolverLiveParams
    {
        private static readonly Random random = new Random();

        public static readonly string[] paramNames = new
            []{
                "random",
                "year",
                "month",
                "day",
                "weekday",
                "hour",
                "minute",
                "second"
            };
        
        public static bool CheckParamName(string varName, bool compareCase = false)
        {
            return paramNames.Any(p => p.Equals(varName,
                                                compareCase
                                                    ? StringComparison.Ordinal
                                                    : StringComparison.OrdinalIgnoreCase));
        }

        public static double? GetParamValue(string varName, DateTime modelTime, bool compareCase = false)
        {
            var compType = compareCase ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            if (varName.Equals("random", compType))
                return random.NextDouble();

            if (varName.Equals("year", compType))
                return modelTime.Year;
            if (varName.Equals("month", compType))
                return modelTime.Month;
            if (varName.Equals("day", compType))
                return modelTime.Day;
            if (varName.Equals("weekday", compType))
                return (int)modelTime.DayOfWeek;
            
            if (varName.Equals("hour", compType))                
                return modelTime.Hour;
            if (varName.Equals("minute", compType))
                return modelTime.Minute;
            if (varName.Equals("second", compType))
                return modelTime.Second;

            return null;
        }
    }
}
