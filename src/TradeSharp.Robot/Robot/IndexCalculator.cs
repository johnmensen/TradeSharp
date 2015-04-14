using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    public class IndexCalculator
    {
        public List<string> formulaVariableNames;

        public Dictionary<string, double> varValues;

        public double? lastIndexValue;

        public ExpressionResolver resolver;

        public double? NanReplacement;

        public IndexCalculator(string indexFormula)
        {
            try
            {
                resolver = new ExpressionResolver(indexFormula);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка парсинга формулы \"{0}\" в IndexCalculator: {1}",
                    indexFormula, ex);
                resolver = null;
                return;
            }
            
            formulaVariableNames = resolver.GetVariableNames();
        }

        public Dictionary<string, double> GetVariableValues(string[] tickerNames,
            List<CandleData> candles,
            Dictionary<string, List<double>> lastBidLists,
            DateTime curTime, Random randomGener)
        {
            if (resolver == null) return null;
            varValues = new Dictionary<string, double>();
            if (formulaVariableNames == null) return new Dictionary<string, double>();
            // специальные имена переменных:
            // open, low, high, close - текущие OHLC-уровни для свечной серии
            // для серий типа LineSeries доступен только close
            // close#2 - цена два бара назад
            // i - индекс текущей свечи
            // count - количество данных в основном ряду
            // year, month, day, weekday, hour, minute, second
            // eurusd, audjpy#100 - close соотв. валютной пары
            // random - равномерно распред. СЧ на интервале 0..1

            // переменные с неизвестными именами инициализируются нулями
            foreach (var name in formulaVariableNames)
            {
                var variableName = name;
                if (name == "i")
                {
                    varValues.Add(name, candles.Count);
                    continue;
                }
                if (name == "count")
                {
                    varValues.Add(name, candles.Count);
                    continue;
                }

                var tickerName = tickerNames.FirstOrDefault(tn =>
                    variableName.StartsWith(tn, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(tickerName))
                {// котировка
                    var price = 0.0;
                    if (lastBidLists.ContainsKey(tickerName))
                    {
                        var tickerQueue = lastBidLists[tickerName];
                        var datIndex = GetDeltaIndexFromSharpSeparatedString(variableName);
                        price = datIndex >= tickerQueue.Count ? 0 : tickerQueue[tickerQueue.Count - datIndex - 1];
                    }
                    varValues.Add(name, price);
                    continue;
                }
                if (name == "random")
                {
                    varValues.Add(name, randomGener.NextDouble());
                    continue;
                }
                if (name == "year" || name == "month" || name == "day" || name == "weekday" || name == "hour"
                    || name == "minute" || name == "second")
                {
                    // получить дату текущей точки графика                    
                    if (name == "year") varValues.Add(name, curTime.Year);
                    else if (name == "month") varValues.Add(name, curTime.Month);
                    else if (name == "day") varValues.Add(name, curTime.Day);
                    else if (name == "weekday") varValues.Add(name, (int)curTime.DayOfWeek);
                    else if (name == "hour") varValues.Add(name, curTime.Hour);
                    else if (name == "minute") varValues.Add(name, curTime.Minute);
                    else if (name == "second") varValues.Add(name, curTime.Second);

                    continue;
                }
                if (name.StartsWith("close") || name.StartsWith("open") || name.StartsWith("low") || name.StartsWith("high"))
                {// "close", "close#17" ...
                    var datIndex = GetDeltaIndexFromSharpSeparatedString(name);
                    if (datIndex >= candles.Count)
                    {
                        varValues.Add(name, 0);
                        continue;
                    }
                    var candle = candles[candles.Count - datIndex - 1];
                    if (name.StartsWith("open")) varValues.Add(name, candle.open);
                    else if (name.StartsWith("high")) varValues.Add(name, candle.high);
                    else if (name.StartsWith("low")) varValues.Add(name, candle.low);
                    else if (name.StartsWith("close")) varValues.Add(name, candle.close);
                    continue;
                }
                // переменная не определена вообще - инициализируем 0-м
                varValues.Add(name, 0);
            }
            return varValues;
        }

        public void CalculateValue(string[] tickerNames, List<CandleData> candles,
            Dictionary<string, List<double>> lastBidLists, 
            DateTime curTime, Random randomGener)
        {
            if (resolver == null) return;
            // посчитать индекс
            double index;
            varValues = GetVariableValues(tickerNames, candles, lastBidLists, curTime, randomGener);
            if (resolver.Calculate(varValues, out index))
            {
                if (double.IsNaN(index))
                    lastIndexValue = NanReplacement;
                else
                    lastIndexValue = index;
            }
        }

        /// <summary>
        /// пример: для EURUSD#15 вернуть 15 (без знака!)
        /// </summary>        
        private static int GetDeltaIndexFromSharpSeparatedString(string varName)
        {
            var indexSharp = varName.IndexOf('#');
            if (indexSharp < 0) return 0;
            var partIndex = varName.Substring(indexSharp + 1);
            var deltaIndex = partIndex.ToIntSafe();
            return deltaIndex ?? 0;
        }
    }
}
