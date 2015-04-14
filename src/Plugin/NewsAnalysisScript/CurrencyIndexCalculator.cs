using System;
using System.Collections.Generic;
using System.Linq;
using Entity;

namespace NewsAnalysisScript
{
    class CurrencyIndexCalculator
    {
        private Dictionary<string, double> multipliers = new Dictionary<string, double>();
        private Dictionary<string, Dictionary<string, double>> currenciesAndPowers = new Dictionary<string, Dictionary<string, double>>();

        public CurrencyIndexCalculator()
        {
            multipliers.Add("USD", 50.14348112);
            var powers = new Dictionary<string, double>
                             {
                                 {"EURUSD", -0.576},
                                 {"USDJPY", 0.136},
                                 {"GBPUSD", -0.119},
                                 {"USDCAD", 0.091},
                                 //{"USDSEK", 0.042},
                                 {"USDCHF", 0.036}
                             };
            currenciesAndPowers.Add("USD", powers);

            multipliers.Add("EUR", 34.38805726);
            powers = new Dictionary<string, double>
                             {
                                 {"EURUSD", 0.3155},
                                 {"EURGBP", 0.3056},
                                 {"EURJPY", 0.1891},
                                 {"EURCHF", 0.1113},
                                 //{"EURSEK", 0.0785}
                             };
            currenciesAndPowers.Add("EUR", powers);

            multipliers.Add("JPY", 798);
            powers = new Dictionary<string, double>
                             {
                                 {"USDJPY", -0.258},
                                 {"EURJPY", -0.124},
                                 {"GBPJPY", -0.029},
                                 {"JPYCNY", 0.154},
                                 {"JPYKRW", 0.089},
                                 {"JPYTWD", 0.084},
                                 {"HKDJPY", -0.069},
                                 {"JPYTHB", 0.043}
                             };
            currenciesAndPowers.Add("JPY", powers);

            multipliers.Add("GBP", 1);
            powers = new Dictionary<string, double>
                             {
                                 {"GBPUSD", 0.167},
                                 {"EURGBP", -0.743},
                                 {"GBPJPY", 0.043},
                                 {"GBPCAD", 0.024},
                                 {"GBPCHF", 0.023}
                             };
            currenciesAndPowers.Add("GBP", powers);

            multipliers.Add("CHF", 1);
            powers = new Dictionary<string, double>
                             {
                                 {"USDCHF", -0.115},
                                 {"EURCHF", -0.69},
                                 {"GBPCHF", -0.195}
                             };
            currenciesAndPowers.Add("CHF", powers);

            multipliers.Add("CAD", 1);
            powers = new Dictionary<string, double>
                             {
                                 {"USDCAD", -0.7618},
                                 {"EURCAD", -0.0931},
                                 {"CADJPY", 0.0527},
                                 {"GBPCAD", -0.0271}
                             };
            currenciesAndPowers.Add("CAD", powers);
        }

        public List<string> GetIndexDependencies(string currency, out string error)
        {
            if (!currenciesAndPowers.ContainsKey(currency))
            {
                error = "CurrencyIndexCalculator:GetIndexDependencies: no such currency: " + currency;
                return null;
            }
            error = null;
            var result = new List<string>();
            var powers = currenciesAndPowers[currency];
            foreach (var power in powers)
                if (!result.Contains(power.Key))
                    result.Add(power.Key);
            return result;
        }

        private double GetIndex(string currency, Dictionary<string, double> tickerValues, out string error)
        {
            var result = 1.0;
            if (!currenciesAndPowers.ContainsKey(currency))
            {
                error = "CurrencyIndexCalculator:GetIndex: no such currency: " + currency;
                return result;
            }
            var powers = currenciesAndPowers[currency];
            result = multipliers[currency];
            foreach (var tickerValue in tickerValues)
            {
                if (powers.ContainsKey(tickerValue.Key))
                    result *= Math.Pow(tickerValue.Value, powers[tickerValue.Key]);
            }
            error = null;
            return result;
        }

        public double? GetIndexDelta(string currency, DateTime date, int calcRangeMinutes, Dictionary<string, List<CandleData>> cache, out List<string> errors)
        {
            errors = new List<string>();
            if (!currenciesAndPowers.ContainsKey(currency))
            {
                errors.Add("CurrencyIndexCalculator:GetIndexDelta: no such currency: " + currency);
                return null;
            }
            var date1 = date;
            var date2 = date + new TimeSpan(0, calcRangeMinutes, 0);
            var tickerValuesBefore = new Dictionary<string, double>();
            var tickerValuesAfter = new Dictionary<string, double>();
            foreach (var ticker in currenciesAndPowers[currency].Select(cp => cp.Key))
            {
                List<CandleData> candles = null;
                if (cache.Count == 0)
                    candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker, date1, date2);
                else
                    if (cache.ContainsKey(ticker))
                    {
                        var tickerCandles = cache[ticker];
                        candles = tickerCandles.Where(candle => candle.timeOpen >= date1 && candle.timeOpen <= date2).ToList();
                    }
                if ((candles == null) || (candles.Count == 0))
                {
                    errors.Add(string.Format("CurrencyIndexCalculator:GetIndexDelta: no candle data for {0} at {1} - {2}", ticker, date1, date2));
                    continue;
                }
                if ((candles[candles.Count - 1].timeClose - candles[0].timeOpen).TotalMinutes < calcRangeMinutes - 1)
                {
                    errors.Add(string.Format("CurrencyIndexCalculator:GetIndexDelta: insufficient candle data for {0} at {1} - {2}", ticker, date1, date2));
                    continue;
                }
                tickerValuesBefore.Add(ticker, candles[0].open);
                tickerValuesAfter.Add(ticker, candles[candles.Count - 1].close);
            }
            if (tickerValuesBefore.Count() == 0 || tickerValuesAfter.Count() == 0)
            {
                errors.Add(string.Format("CurrencyIndexCalculator:GetIndexDelta: insufficient candle data for {0} at {1} - {2}", currency, date1, date2));
                return null;
            }
            string error;
            var valueBefore = GetIndex(currency, tickerValuesBefore, out error);
            if (error != null)
            {
                errors.Add(error);
                return null;
            }
            var valueAfter = GetIndex(currency, tickerValuesAfter, out error);
            if (error != null)
            {
                errors.Add(error);
                return null;
            }
            return valueAfter - valueBefore;
        }
    }
}
