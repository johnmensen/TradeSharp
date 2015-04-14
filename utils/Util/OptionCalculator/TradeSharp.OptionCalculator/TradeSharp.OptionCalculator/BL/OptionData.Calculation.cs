using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.OptionCalculator.BL
{
    partial class OptionData
    {
        #region Расчетные данные

        private int timeframesTotal;

        private double timeframesFraction;

        private List<CandleData> candles;

        private PriceModel model;

        private int timeframe;

        #endregion

        public void Calculate()
        {
            ReadCandles();            
            if (candles.Count < 10)
            {
                LogMessage("Прочитано недостаточное ({0}) количество свечей", candles.Count);
                return;
            }
            GetTimeframesToExpire();
            if (VanillaPrice <= 0)
            {
                EnsureVanillaPrice();
                if (VanillaPrice == 0) return;
                LogMessage("Цена на момент открытия определена как {0}", VanillaPrice.ToStringUniformPriceFormat(true));
            }
            BuildModel();
            CalculatePremium();
        }

        private void CalculatePremium()
        {
            if (OptionType == OptionType.European)
            {
                CalculatePremiumEuropean();
                return;
            }

            LogMessage("Выбранный тип опционов не реализован");
        }

        private void CalculatePremiumEuropean()
        {
            double totalProfit = 0;
            int strikeReached = 0;
            var iterations = CalcSettings.Instance.IterationsCount;

            var lastDayMultiplier = timeframesFraction == 0
                ? 0.0
                : Math.Sqrt(timeframesFraction);

            for (var i = 0; i < iterations; i++)
            {
                var profit = TestTotalProfit(lastDayMultiplier);
                totalProfit += profit;
                if (profit > 0) strikeReached++;
            }

            LogMessage("Расчетная премия равна {0:f5}, {1:f1}% прохождения страйка",
                totalProfit / iterations, strikeReached * 100.0 / iterations);
        }

        private double TestTotalProfit(double lastDayMultiplier)
        {
            var price = (double)VanillaPrice;
            for (var i = 0; i < timeframesTotal; i++)
                price += model.GetRandomDelta();
            if (lastDayMultiplier > 0)
                price += lastDayMultiplier * model.GetRandomDelta();
            var delta = Side == OptionSide.Call
                ? price - (double) Strike
                : (double) Strike - price;
            return delta > 0 ? delta : 0;
        }

        private void BuildModel()
        {
            var deltas = new List<double>();
            double shift = 0;
            if (RemoveTrend)
                shift = (candles.Last().close - candles.First().open) / candles.Count;
            candles.ForEach(c => deltas.Add(c.close - c.open - shift));
            deltas.Sort();
            model = new PriceModel(deltas, (double)CalcSettings.Instance.HighPercent);
        }

        private void GetTimeframesToExpire()
        {
            timeframesTotal = 0;
            timeframesFraction = 0;
            var day = ValueDate;
            for (; day <= Expiration; day = day.AddMinutes(timeframe))
            {
                if (day.DayOfWeek == DayOfWeek.Sunday || day.DayOfWeek == DayOfWeek.Saturday)
                    continue;
                timeframesTotal++;
            }
            if (day != Expiration && day.DayOfWeek != DayOfWeek.Sunday && 
                day.DayOfWeek != DayOfWeek.Saturday)
                timeframesFraction += (day - Expiration.Date).TotalMinutes / timeframe;
            LogMessage("{0:f1} таймфреймов всего", timeframesTotal + timeframesFraction);
        }

        private void ReadCandles()
        {
            var fileExtension = Path.GetExtension(QuoteFilePath).Trim('.');
            var candlesByTicker = new CandleByTicker();
            if (fileExtension == "fxh")
            {
                var reader = new Mt4CandleReader();
                reader.ReadCandlesFromFile(candlesByTicker, QuoteFilePath, out timeframe);
            }
            if (fileExtension == "hst")
            {
                var reader = new HstFileReader();
                reader.ReadCandlesFromFile(candlesByTicker, QuoteFilePath, out timeframe);
            }
            candles = candlesByTicker.candles;
            LogMessage("Прочитано {0} свечей", candles.Count);
        }

        private void EnsureVanillaPrice()
        {
            if (VanillaPrice > 0) return;
            var prev = candles[0];
            if (candles.First().timeOpen > ValueDate)
            {
                LogMessage("Первая свеча ({0}) начинается позже даты валютирования {1}",
                    prev.timeOpen.ToStringUniform(), ValueDate.ToStringUniform());
                return;
            }
            foreach (var candle in candles)
            {
                if (candle.timeOpen > ValueDate)
                {
                    VanillaPrice = (decimal)candle.open;
                    return;
                }
                if (candle.timeClose > ValueDate)
                {
                    CalcVanillaPrice(candle.timeOpen, candle.timeClose, candle.open, candle.close);
                    return;
                }
            }
            LogMessage("Последняя свеча ({0}) заканчивается прежде даты валютирования {1}",
                    candles.Last().timeOpen.ToStringUniform(), ValueDate.ToStringUniform());
        }

        private void CalcVanillaPrice(DateTime start, DateTime end, float open, float close)
        {
            var delta = (ValueDate - start).TotalMinutes / (end - start).TotalMinutes;
            VanillaPrice = (decimal)(open + (close - open) * delta);
        }
    }
}
