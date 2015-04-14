using System;

namespace TradeSharp.Util
{
    /// <summary>
    /// в будущем планируется брать эту информацию из БД (через DictionaryProxy)
    /// с использованием этого же класса
    /// </summary>    
    public class WorkingDay
    {
        private static WorkingDay instance;
        public static WorkingDay Instance
        {
            get { return instance ?? (instance = new WorkingDay()); }
        }

        public const int 
            StartDayOff = 5,
            StartHourOff = 23,
            EndDayOff = 1,
            EndHourOff = 5;

        [Obsolete]
        public bool IsWorkingDay(DateTime date)
        {
            DateTime startWeekDay;
            try
            {
                startWeekDay = date.AddDays(-(int)date.DayOfWeek).Date;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                startWeekDay = date;
            }
            var startOff = startWeekDay.AddDays(StartDayOff).AddHours(StartHourOff);
            var endOff = startWeekDay.AddDays(EndDayOff).AddHours(EndHourOff);

            if (StartDayOff > EndDayOff)
            {
                return date >= endOff && date <= startOff;
            }

            return date >= startOff && date <= endOff;
        }

        /// <summary>
        /// передано время, приходящееся на выходной день
        /// вернуть первый рабочий день и час, следующий за этими выходными
        /// </summary>
        [Obsolete]
        public DateTime GetWeekStart(DateTime timeOff)
        {
            var day = (int) timeOff.DayOfWeek;
            if (day == 0) day = 1;
            var deltaDays = (7 - day) + EndDayOff;
            return timeOff.Date.AddHours(24*deltaDays + EndHourOff);
        }
    }
}
