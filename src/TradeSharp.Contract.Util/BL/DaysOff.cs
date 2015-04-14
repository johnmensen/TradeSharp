using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// период времени, когда биржа не работает
    /// </summary>
    class DayOff
    {
        private readonly bool valid;
        private readonly string format;
        private readonly int? year;
        private readonly int month, day, hour, duration;

        public DayOff(string dayOffString)
        {
            valid = false;
            var formatStrings = dayOffString.Split(':');
            if (formatStrings.Length != 2)
            {
                Logger.ErrorFormat("DayOff: format error: {0}", dayOffString);
                return;
            }
            var valueStrings = formatStrings[1].Split(';');
            if (valueStrings.Length != 2)
            {
                Logger.ErrorFormat("DayOff: format error: {0}", dayOffString);
                return;
            }
            if (formatStrings[0] == "D")
            {
                format = "D";
                var dateStrings = valueStrings[0].Split('/');
                if (dateStrings.Length != 3)
                {
                    Logger.ErrorFormat("DayOff: format error: {0}", dayOffString);
                    return;
                }
                try
                {
                    day = Convert.ToInt32(dateStrings[0]);
                    month = Convert.ToInt32(dateStrings[1]);
                    if (dateStrings[2] != "-")
                        year = Convert.ToInt32(dateStrings[2]);
                }
                catch (Exception exception)
                {
                    Logger.ErrorFormat("DayOff: {0}: {1}", exception, dayOffString);
                }
            }
            else if (formatStrings[0] == "WD")
            {
                format = "WD";
                try
                {
                    day = Convert.ToInt32(valueStrings[0]);
                }
                catch (Exception exception)
                {
                    Logger.ErrorFormat("DayOff: {0}: {1}", exception, dayOffString);
                }
            }
            var timeStrings = valueStrings[1].Split('-');
            if (timeStrings.Length != 2)
            {
                Logger.ErrorFormat("DayOff: format error: {0}", dayOffString);
                return;
            }
            try
            {
                hour = Convert.ToInt32(timeStrings[0]);
                duration = Convert.ToInt32(timeStrings[1]);
            }
            catch (Exception exception)
            {
                Logger.ErrorFormat("DayOff: {0}: {1}", exception, dayOffString);
            }
            valid = true;
        }

        // создать список нерабочих интервалов, имеющих пересечение с заданным
        public List<DateSpan> GetIntersected(DateSpan interval)
        {
            var result = new List<DateSpan>();
            if (!valid)
                return result;
            DateTime beginInterval;
            if (format == "WD")
            {
                beginInterval = new DateTime(interval.start.Year, interval.start.Month, interval.start.Day, hour, 0, 0);
                // ищем первый периодический нерабочий интервал, пересекающийся с указанным,
                // соблюдая условие, что начало его раньше начала указанного
                var dayDelta = (int)interval.start.DayOfWeek - day;
                if (dayDelta < 0)
                    dayDelta += 7;
                beginInterval = beginInterval.AddDays(-dayDelta);
                // dayDelta = 0 и возможно расхождение в часах
                if (beginInterval > interval.start)
                    beginInterval = beginInterval.AddDays(-7); // convert to GetPrevDate/GetNextDate to calc periodic DateTime
                // этот нерабочий интервал не пересекается с указанным
                // подставляем следующий
                if (beginInterval.AddHours(duration) < interval.start)
                    beginInterval = beginInterval.AddDays(7);
                while (beginInterval < interval.end)
                {
                    result.Add(new DateSpan(beginInterval, beginInterval.AddHours(duration)));
                    beginInterval = beginInterval.AddDays(7);
                }
            }
            else if (format == "D")
            {
                // непериодический интервал
                if (year.HasValue)
                {
                    beginInterval = new DateTime(year.Value, month, day, hour, 0, 0);
                    // есть пересечение
                    if ((beginInterval > interval.start) || (beginInterval.AddHours(duration) < interval.end))
                        result.Add(new DateSpan(beginInterval, beginInterval.AddHours(duration)));
                }
                // периодический ежегодный интервал
                else
                {
                    beginInterval = new DateTime(interval.start.Year, month, day, hour, 0, 0);
                    if (beginInterval > interval.start)
                        beginInterval = beginInterval.AddYears(-1); // convert to GetPrevDate/GetNextDate to calc periodic DateTime
                    if (beginInterval.AddHours(duration) < interval.start)
                        beginInterval = beginInterval.AddYears(1);
                    while (beginInterval < interval.end)
                    {
                        result.Add(new DateSpan(beginInterval, beginInterval.AddHours(duration)));
                        beginInterval = beginInterval.AddYears(1);
                    }
                }
            }
            return result;
        }
    
        public bool IsIn(DateTime time)
        {
            if (!valid)
                return false;

            if (format == "WD")
            {
                DateTime startWeekDay;
                try
                {
                    startWeekDay = time.AddDays(-(int)time.DayOfWeek).Date;
                }
                catch (ArgumentOutOfRangeException)
                {
                    startWeekDay = time;
                }
                var startOff = startWeekDay.AddDays(day).AddHours(hour);
                var endOff = startWeekDay.AddDays(day).AddHours(hour + duration);

                return time >= startOff && time <= endOff;
            }

            //if (format == "D")
            var startDate = new DateTime(year ?? time.Year, month, day, hour, 0, 0);
            var endDate = startDate.AddHours(duration);
            return time >= startDate && time <= endDate;
        }
    }

    /// <summary>
    /// все нерабочие интервалы
    /// </summary>
    public class DaysOff
    {
        private static DaysOff instance;
        private static List<DayOff> daysOff;

        public static DaysOff Instance
        {
            get
            {
                if (instance == null)
                    instance = new DaysOff();
                return instance;
            }
        }

        private DaysOff()
        {
            daysOff = new List<DayOff>();
            var days = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("DayOff");
            if (days != null)
                foreach (var day in days)
                    daysOff.Add(new DayOff(day.Value.ToString()));
        }

        public bool IsDayOff(DateTime time)
        {
            return daysOff.Any(d => d.IsIn(time));
        }

        /// <summary>
        /// расчитываем все нерабочие периоды, пересекающиеся с указанным
        /// </summary>
        public List<DateSpan> GetIntersected(DateSpan interval)
        {
            var result = new List<DateSpan>();
            foreach (var dayOff in daysOff)
                result.AddRange(dayOff.GetIntersected(interval));
            return result;
        }
    }
}
