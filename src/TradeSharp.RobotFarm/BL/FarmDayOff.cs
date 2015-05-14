using System;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL
{
    public static class FarmDaysOff
    {
        public static int startDayOff = AppConfig.GetIntParam("DaysOff.StartDay", (int)DayOfWeek.Saturday);
        public static int startHourOff = AppConfig.GetIntParam("DaysOff.StartHour", 0);
        public static int durationHours = AppConfig.GetIntParam("DaysOff.DurationHours", 48);

        public static bool IsDayOff(DateTime time)
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
            var startOff = startWeekDay.AddDays(startDayOff).AddHours(startHourOff);
            var endOff = startWeekDay.AddDays(startDayOff).AddHours(startHourOff + durationHours);

            return time >= startOff && time <= endOff;
        }
    }
}
