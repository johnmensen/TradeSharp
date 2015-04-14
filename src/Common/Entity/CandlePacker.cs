using System;

namespace Entity
{
    /// <summary>
    /// преобразует поток котировок в поток свечек
    /// </summary>
    public class CandlePacker
    {
        public DateTime CandleCloseTime { get; set; }

        public BarSettings BarSettings { get; set; }

        private CandleData currentCandle;
        public CandleData CurrentCandle
        {
            get { return currentCandle; }
        }

       
        public CandlePacker(BarSettings barSettings)
        {
            BarSettings = barSettings;
        }

        /// <summary>
        /// вернуть свечу, если свечка закрылась
        /// </summary>        
        /// <returns>свеча, если свечка закрылась котировкой, или null</returns>
        public CandleData UpdateCandle(float price, DateTime time)
        {
            if (CurrentCandle == null)
                MakeNewCandle(price, time);
            else
            {
                if (time > CandleCloseTime)
                {
                    var candle = CloseCandle(price, time);
                    return candle;
                }
                // обновить уровни свечи
                CurrentCandle.timeClose = time;
                CurrentCandle.close = price;
                if (price > CurrentCandle.high) CurrentCandle.high = price;
                if (price < CurrentCandle.low) CurrentCandle.low = price;
            }
            // коррекция свечи?
            return null;
        }

        public CandleData UpdateCandle(CandleData minuteCandle)
        {
            var time = minuteCandle.timeClose;
            if (CurrentCandle == null)
                MakeNewCandle(minuteCandle);
            else
            {
                if (time > CandleCloseTime)
                {
                    var candle = CloseCandle(minuteCandle);
                    return candle;
                }
                // обновить уровни свечи
                CurrentCandle.timeClose = time;
                CurrentCandle.close = minuteCandle.close;
                if (minuteCandle.high > CurrentCandle.high) CurrentCandle.high = minuteCandle.high;
                if (minuteCandle.low < CurrentCandle.low) CurrentCandle.low = minuteCandle.low;
            }
            // коррекция свечи?
            return null;
        }

        /// <summary>
        /// Завершить формирование свечки
        /// </summary>        
        private CandleData CloseCandle(float price, DateTime time)
        {
            var completed = new CandleData
            {
                open = CurrentCandle.open,
                close = CurrentCandle.close,
                high = CurrentCandle.high,
                low = CurrentCandle.low,
                timeOpen = CurrentCandle.timeOpen,
                timeClose = CandleCloseTime,                
            };
            
            MakeNewCandle(price, time);
            return completed;
        }

        private CandleData CloseCandle(CandleData minuteCandle)
        {
            var completed = new CandleData
            {
                open = CurrentCandle.open,
                close = CurrentCandle.close,
                high = CurrentCandle.high,
                low = CurrentCandle.low,
                timeOpen = CurrentCandle.timeOpen,
                timeClose = CandleCloseTime,
            };

            MakeNewCandle(minuteCandle);
            return completed;
        }

        private void MakeNewCandle(float price, DateTime time)
        {
            DateTime startTime;
            CandleCloseTime = GetLastCandleClose(time, out startTime);
            currentCandle = new CandleData(price, price, price, price, startTime, time);
        }

        private void MakeNewCandle(CandleData candle)
        {
            var time = candle.timeClose;
            DateTime startTime;
            CandleCloseTime = GetLastCandleClose(time, out startTime);
            currentCandle = new CandleData(candle.open, candle.high, candle.low, candle.close, startTime, time);
        }

        public DateTime GetLastCandleClose(DateTime timeFrom, out DateTime candleOpenTime)
        {
            int nextCandlePeriod;
            return GetLastCandleClose(timeFrom, out candleOpenTime, out nextCandlePeriod);
        }

        public DateTime GetLastCandleClose(DateTime timeFrom, out DateTime candleOpenTime, out int nextCandlePeriod)
        {
            if (BarSettings.Intervals.Count == 1)
                if (BarSettings.Intervals[0] == 1440 * 5 && BarSettings.StartMinute == 0)
                {
                    // отдельно обработка недельного ТФ
                    candleOpenTime = timeFrom.Date;
                    if (candleOpenTime.DayOfWeek != DayOfWeek.Monday)
                    {
                        var day = (int) candleOpenTime.DayOfWeek;
                        var shiftBehind = day < 6 && day > 0 ? 1 - day : day == 0 ? -6 : -5;
                        candleOpenTime = candleOpenTime.AddDays(shiftBehind);
                    }
                    nextCandlePeriod = 0;
                    return candleOpenTime.AddDays(7);
                }
            
            nextCandlePeriod = 0;
            var startTime = timeFrom.Date.AddMinutes(BarSettings.StartMinute);
            if (startTime > timeFrom)
                startTime = startTime.AddDays(-1);
            var endTime = startTime;
            while (endTime <= timeFrom)
            {
                startTime = endTime;
                endTime = startTime.AddMinutes(BarSettings.Intervals[nextCandlePeriod]);
                nextCandlePeriod++;
                if (nextCandlePeriod == BarSettings.Intervals.Count)
                    nextCandlePeriod = 0;
            }
            candleOpenTime = startTime;
            return endTime;
        }
    }
}