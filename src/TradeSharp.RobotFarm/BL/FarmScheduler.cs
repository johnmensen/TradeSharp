using System;
using System.Collections.Generic;
using TradeSharp.RobotFarm.Request;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL
{
    /// <summary>
    /// в промежутке с 00:00:00 до 00:30:00 обновляет состав портфеля,
    /// если, после старта на этом промежутке времени портфлеь уже не был обновлен
    /// </summary>
    class FarmScheduler : Scheduler
    {
        private const int SecondsBetweenCheckTossTime = 1;

        private const int SecondsBetweenReviveChannel = 15;

        private static FarmScheduler instance;

        public static FarmScheduler Instance
        {
            get { return instance ?? (instance = new FarmScheduler()); }
        }

        private readonly ThreadSafeTimeStamp lastTimeUpdated = new ThreadSafeTimeStamp();

        private static readonly string iniFilePath = ExecutablePath.ExecPath + "\\settings.ini";

        private FarmScheduler()
        {
            threadIntervalMils = 100;
            var sectPortfolio = new IniFile(iniFilePath).ReadSection("portfolio");
            
            // прочитать время последней перетасовки портфеля
            string lastTimeUpdatedStr;
            if (sectPortfolio.TryGetValue("uptime", out lastTimeUpdatedStr))
            {
                var lastTimeUpdatedVal = lastTimeUpdatedStr.ToDateTimeUniformSafe();
                if (lastTimeUpdatedVal.HasValue)
                    lastTimeUpdated.SetTime(lastTimeUpdatedVal.Value);
            }
        }

        public void ScheduleTopPortfolioUpdates(IEnumerable<FarmAccount> accountsList)
        {
            schedules = new[]
                {
                    new Schedule(
                        () =>
                            {
                                var nowTime = DateTime.Now;
                                var lastUpdated = lastTimeUpdated.GetLastHitIfHitted();
                                // если портфель уже тасовался - обновить его минимум сутки спустя
                                if (lastUpdated.HasValue)
                                {
                                    var daysPassed = (nowTime - lastUpdated.Value).TotalDays;
                                    if (daysPassed < 1) return;
                                }
                                // иначе - обновить портфель на промежутке 0:00 - 0:30
                                else                                
                                    if (nowTime.Hour > 0 || nowTime.Minute > 30) return;

                                // таки обновить портфели по всем счетам
                                Logger.Info("Обновить портфели по счетам");
                                var accounts = RobotFarm.Instance.Accounts;
                                foreach (var ac in accounts)
                                {
                                    if (RobotFarm.Instance.State == FarmState.Stopping)
                                        break;
                                    ac.UpdatePortfolio();
                                }

                                // сохранить время последнего обновления
                                lastTimeUpdated.Touch();
                                new IniFile(iniFilePath).WriteValue("portfolio", "uptime",
                                                                    DateTime.Now.ToStringUniform());
                            }, SecondsBetweenCheckTossTime * 1000),
                    new Schedule(
                        () =>
                            {
                                var accounts = RobotFarm.Instance.Accounts;
                                foreach (var ac in accounts)
                                {
                                    if (RobotFarm.Instance.State == FarmState.Stopping)
                                        break;
                                    ac.ReviveChannel();
                                }
                            }, SecondsBetweenReviveChannel * 1000)
                };
        }
    }
}
