using System;
using NUnit.Framework;
using TradeSharp.RobotFarm.BL;

namespace TradeSharp.Test.RobotFarm
{
    [TestFixture]
    public class FarmDaysOffTest
    {
        private int oldStartDay, oldStartHour, oldDuration;

        [TestFixtureSetUp]
        public void SetupDays()
        {
            oldStartDay = FarmDaysOff.startDayOff;
            oldStartHour = FarmDaysOff.startHourOff;
            oldDuration = FarmDaysOff.durationHours;
            FarmDaysOff.startDayOff = (int)DayOfWeek.Saturday;
            FarmDaysOff.startHourOff = 0;
            FarmDaysOff.durationHours = 48;
        }

        [TestFixtureTearDown]
        public void RestoreDays()
        {
            FarmDaysOff.startDayOff = oldStartDay;
            FarmDaysOff.startHourOff = oldStartHour;
            FarmDaysOff.durationHours = oldDuration;
        }

        [Test]
        public void TestPositive()
        {
            var date = new DateTime(2014, 11, 22, 10, 44, 51);
            Assert.IsTrue(FarmDaysOff.IsDayOff(date), "Выходной не распознан");

            date = new DateTime(2013, 6, 15, 0, 0, 51);
            Assert.IsTrue(FarmDaysOff.IsDayOff(date), "Выходной не распознан");
        }

        [Test]
        public void TestNegative()
        {
            var date = new DateTime(2014, 11, 21, 10, 44, 51);
            Assert.IsFalse(FarmDaysOff.IsDayOff(date), "Рабочий день не распознан");
        }
    }
}
