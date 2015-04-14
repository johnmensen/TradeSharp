using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using TradeSharp.NewsGrabber.Grabber;
using TradeSharp.NewsGrabber.Grabber.CME;
using TradeSharp.NewsGrabber.Grabber.TradeSharp;
using TradeSharp.NewsGrabber.Grabber.Yahoo;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.BL
{
    /// <summary>
    /// просматривает "расписание" с указанной в настройках частотой
    /// запускает процедуру получения данных: с CHF, ...
    /// </summary>
    class Scheduler
    {
        private readonly Dictionary<string, BaseNewsGrabber> grabbers =
            new Dictionary<string, BaseNewsGrabber>();

        /// <summary>
        /// интервал следящего таймера, секунд
        /// </summary>
        private readonly int timerInterval;

        private readonly List<ScheduleRoutine> routines = new List<ScheduleRoutine>();

        private Thread watchdogThread;

        private volatile bool isStopping;

        private static Scheduler instance;
        public static Scheduler Instance
        {
            get { return instance ?? (instance = new Scheduler()); }
        }

        private Scheduler()
        {
            var setsFile = string.Format("{0}\\schedule.txt", ExecutablePath.ExecPath);
            if (!File.Exists(setsFile)) return;

            using (var sr = new StreamReader(setsFile))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (timerInterval == 0)
                    {
                        var lineAsNumber = line.ToIntSafe();
                        if (lineAsNumber.HasValue)
                            timerInterval = lineAsNumber.Value;
                        continue;
                    }
                    var routine = ScheduleRoutine.TryParse(line);
                    if (routine != null) routines.Add(routine);
                }
            }
            
            // инициализация сборщиков новостей
            grabbers.Add("CME", new CMEGrabber());
            grabbers.Add("Yahoo", new YahooGrabber());
            grabbers.Add("TradeSharp", new TradeSharpGrabber());
        }
    
        public void Start()
        {
            if (routines.Count == 0) return;
            isStopping = false;
            watchdogThread = new Thread(ThreadIteration);
            watchdogThread.Start();
        }

        public void Stop()
        {
            if (routines.Count == 0) return;
            isStopping = true;
            watchdogThread.Join();
        }

        private void ThreadIteration()
        {
            while (!isStopping)
            {
                // проверить для каждой рутины - не наступило ли время срабатывания
                // стартовать процедуру...
                foreach (var routine in routines)
                {
                    if (isStopping) break;
                    if (!routine.IsTimeToCall()) continue;
                    // вызов процедуры
                    var grabber = grabbers[routine.GrabberCode];
                    grabber.GetNews();
                }
                Thread.Sleep(timerInterval * 1000);
            }
        }
    }

    class ScheduleRoutine
    {
        public DateTime? LastTimeCalled { get; set; }

        /// <summary>
        /// CME или Bloomberg или Yahoo!Finance ...
        /// </summary>
        public string GrabberCode { get; set; }

        /// <summary>
        /// пример: заданы дни 6,7
        /// суббота и вс. пропускаются
        /// </summary>
        public int[] InclusiveWeekdays { get; set; }
        
        /// <summary>
        /// пример: заданы дни 1, 2, 3, 4, 5
        /// инициируется все дни с Пн по Птн
        /// </summary>
        public int[] ExclusiveWeekdays { get; set; }

        /// <summary>
        /// пример: "1020" - запуск раз в сутки, в 17 часов (00:00 + 17 * 60 = 1020)
        /// пример: "0,60+" - запуск 24 раза в сутки, в 0 часов, в 1 час, в 2 часа и т.д.
        /// </summary>
        public int[] MinuteIntervals { get; set; }

        /// <summary>
        /// на входе строка вида
        /// CME;-W:6,7;1020
        /// Yahoo;W:1,2,3,4,5;0,15+
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ScheduleRoutine TryParse(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            str = str.Trim(' ', (char) 9);
            if (str.StartsWith("//") || str.StartsWith("--") || str.StartsWith("/*")) return null;
            var parts = str.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return null;

            var title = parts[0];
            var isExclusive = parts[1].StartsWith("-");
            var weekDays = isExclusive ? parts[1].Substring(3) : parts[1].Substring(2);
            var separateDays = weekDays.ToIntArrayUniform();

            var shouldExtendMinutes = parts[2].EndsWith("+");
            var strMinutes = shouldExtendMinutes
                                 ? parts[2].Substring(0, parts[2].Length - 1)
                                 : parts[2];

            var minutesArray = strMinutes.ToIntArrayUniform();
            var totalMinutesList = new List<int>(minutesArray);
            if (shouldExtendMinutes)
            {
                var lastInterval = minutesArray[minutesArray.Length - 1] - minutesArray[minutesArray.Length - 2];
                for (var inter = minutesArray[minutesArray.Length - 1] + lastInterval;
                    inter < 1440; inter += lastInterval)
                    totalMinutesList.Add(inter);
            }

            var routine = new ScheduleRoutine {GrabberCode = title, MinuteIntervals = totalMinutesList.ToArray()};
            if (isExclusive) routine.ExclusiveWeekdays = separateDays;
            else routine.InclusiveWeekdays = separateDays;

            return routine;
        }

        public bool IsTimeToCall()
        {
            var nowDate = DateTime.Now;
            var dayOfWeek = (int)nowDate.DayOfWeek;
            dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
            
            // проверить маску дней
            if (ExclusiveWeekdays != null)
            if (ExclusiveWeekdays.Length > 0)
                if (ExclusiveWeekdays.Any(d => d == dayOfWeek)) return false;

            if (InclusiveWeekdays != null)
            if (InclusiveWeekdays.Length > 0)
                if (!InclusiveWeekdays.Any(d => d == dayOfWeek)) return false;

            // проверить интервал в минутах
            var minPassed = (int) (nowDate - nowDate.Date).TotalMinutes;
            if (!MinuteIntervals.Any(i => i <= minPassed)) return false;
            if (!LastTimeCalled.HasValue)
            {
                LastTimeCalled = nowDate;
                return true;
            }

            var lastInter = MinuteIntervals.Last(i => i <= minPassed);
            var lastTimestamp = nowDate.Date.AddMinutes(lastInter);
            if (LastTimeCalled.Value >= lastTimestamp) return false;

            LastTimeCalled = nowDate;
            return true;
        }
    }
}