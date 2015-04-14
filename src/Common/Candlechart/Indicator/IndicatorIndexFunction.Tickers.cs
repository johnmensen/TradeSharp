using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    public partial class IndicatorIndexFunction
    {
        /// <summary>
        /// ключ - имя тикера
        /// значение - массив цен close по тикеру, где
        /// индекс совпадает с индексом свечи свечного графика
        /// </summary>
        private Dictionary<string, List<double>> tickerData;

        private string quoteCachFolder = "\\quotes";
        [LocalizedDisplayName("TitleQuoteDirectory")]
        [Description("Каталог котировок для тикеров - переменных формулы")]
        [LocalizedCategory("TitleIndex")]
        public string QuoteCacheFolder
        {
            get { return quoteCachFolder; }
            set { quoteCachFolder = value; }
        }

        private int minMinutesGapToUpdateCache = 6;
        [LocalizedDisplayName("TitleTimeToUpdateInMinutes")]
        [Description("Обновлять кэш, если гэп больше N минут")]
        [LocalizedCategory("TitleIndex")]
        public int MinMinutesGapToUpdateCache
        {
            get { return minMinutesGapToUpdateCache; }
            set { minMinutesGapToUpdateCache = value; }
        }

        public Dictionary<string, DateTime> GetRequiredTickersHistory(DateTime? timeStart)
        {
            if (!timeStart.HasValue)
            {
                if (owner.StockSeries.Data.Candles.Count == 0) return new Dictionary<string, DateTime>();
                timeStart = owner.StockSeries.Data.Candles[0].timeClose;
            }
            var varNames = new List<string>();
            foreach (var resv in resolvers)
            {
                if (resv == null) continue;
                var resvVars = resv.GetVariableNames();
                foreach (var varName in resvVars.Where(varName => !varNames.Contains(varName)))
                {
                    varNames.Add(varName);
                }
            }
            var enabledCurrencies = DalSpot.Instance.GetTickerNames();
            var tickersToLoad =
                enabledCurrencies.Where(curX => varNames.Any(c => c.Equals(curX,
                    StringComparison.OrdinalIgnoreCase))).ToList();
            return tickersToLoad.Count == 0 
                ? new Dictionary<string, DateTime>() 
                : tickersToLoad.ToDictionary(t => t, t => timeStart.Value);
        }

        public void LoadTickers(List<string> variableNames)
        {
            if (owner.StockSeries.Data == null) return;
            if (owner.StockSeries.Data.Candles.Count == 0) return;
            tickerData = new Dictionary<string, List<double>>();
            if (variableNames == null) return;
            if (variableNames.Count == 0) return;
            var enabledCurrencies = DalSpot.Instance.GetTickerNames();
            var tickersToLoad =
                enabledCurrencies.Where(curX => variableNames.Any(c => c.Equals(curX,
                    StringComparison.OrdinalIgnoreCase))).ToList();
            if (tickersToLoad.Count == 0) return;
            
            
            // прочитать котировки и составить массив - цена close-котировка
            var candles = owner.StockSeries.Data.Candles;

            foreach (var ticker in tickersToLoad)
            {
                var tickerCandles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker);
                if (tickerCandles == null || tickerCandles.Count == 0) continue;

                var pricePerCandle = new List<double>();
                var nextCandleIndex = 0;
                double lastBid = 0;

                foreach (var minuteCandle in tickerCandles)
                {
                    lastBid = minuteCandle.open;

                    // если котировка пришла в момент либо после закрытия свечи [nextIndex]...
                    if (nextCandleIndex >= candles.Count)
                    {
                        Logger.ErrorFormat("LoadTickers ({0}, index {1} >= count {2})",
                                           ticker, nextCandleIndex, candles.Count);
                        break;
                    }

                    var candle = candles[nextCandleIndex];
                    if (candle == null)
                    {
                        Logger.ErrorFormat(
                            "LoadTickers ({0}, index {1}, count {2}. owner curx {3}, tf {4}) - candle is null",
                            ticker, nextCandleIndex, candles.Count, owner.Symbol, owner.Timeframe);
                        break;
                    }

                    var timeCloseRounded = candles[nextCandleIndex].timeClose;
                    timeCloseRounded = new DateTime(
                        timeCloseRounded.Year,
                        timeCloseRounded.Month,
                        timeCloseRounded.Day,
                        timeCloseRounded.Hour,
                        timeCloseRounded.Minute, 0);


                    if (minuteCandle.timeOpen < timeCloseRounded) continue;
                    for (var curIndex = nextCandleIndex + 1; curIndex <= candles.Count; curIndex++)
                    {
                        pricePerCandle.Add(minuteCandle.open);
                        if (curIndex == candles.Count)
                        {
                            nextCandleIndex = candles.Count;
                            break;
                        }
                        if (candles[curIndex].timeClose > minuteCandle.timeOpen)
                        {
                            nextCandleIndex = curIndex;
                            break;
                        }
                    }
                    if (nextCandleIndex == candles.Count) break;
                } // foreach (var minuteCandle ...

                if (pricePerCandle.Count < candles.Count)
                    pricePerCandle.Add(lastBid);

                tickerData.Add(ticker, pricePerCandle);
            } // foreach (var ticker ...
        }                
    }
}