using System;
using System.Collections.Generic;
using System.Text;
using TradeSharp.Util;

namespace Entity
{
    public class TerminalLog
    {
        #region Singletone

        private static readonly Lazy<TerminalLog> instance = new Lazy<TerminalLog>(() => new TerminalLog());

        public static TerminalLog Instance
        {
            get { return instance.Value; }
        }

        #endregion

        private List<Cortege2<DateTime, string>> logTerminal = new List<Cortege2<DateTime, string>>();
        private List<Cortege2<DateTime, string>> logRobots = new List<Cortege2<DateTime, string>>();
        
        //private const int RecordsInCache = 50;
        public delegate void TerminalLogEventHandler(string log);
        public delegate void RobotsLogEventHandler(string log);

        public event TerminalLogEventHandler TerminalLogEvent;
        public event RobotsLogEventHandler RobotsLogEvent;

        public void StartSession()
        {
            logRobots.Clear();
        }

        public void EndSession()
        {
            
        }

        public void GetRobotsLog()
        {
            var eventLog = new StringBuilder();
            foreach (var item in logRobots)
            {
                var currTime = DateTime.Now;
                eventLog.AppendLine(string.Format("{0}: {1}", item.a, item.b));
            }

            // семафорим событие
            if (eventLog.Length == 0 || RobotsLogEvent == null) return;
            RobotsLogEvent(eventLog.ToString());
        }

        public void SaveTerminalLog(List<string> logs)
        {
            var eventLog = new StringBuilder();
            foreach (var log in logs)
            {
                var currTime = DateTime.Now;
                logTerminal.Add(new Cortege2<DateTime, string>(currTime, log));
                eventLog.AppendLine(string.Format("{0}: {1}", currTime.ToShortTimeString(), log));
            }
            // семафорим событие
            if (eventLog.Length == 0 || TerminalLogEvent == null) return;
            TerminalLogEvent(eventLog.ToString());
        }

        public void SaveRobotLog(List<string> logs)
        {
            var eventLog = new StringBuilder();
            foreach (var log in logs)
            {
                var currTime = DateTime.Now;
                logRobots.Add(new Cortege2<DateTime, string>(currTime, log));
                eventLog.AppendLine(string.Format("{0}: {1}", currTime.ToShortTimeString(), log));
            }

            // семафорим событие
            if (eventLog.Length == 0 || RobotsLogEvent == null) return;
            RobotsLogEvent(eventLog.ToString());
        }
        
        public void SaveRobotLog(string log)
        {
            var eventLog = new StringBuilder();
            var currTime = DateTime.Now;
            logRobots.Add(new Cortege2<DateTime, string>(currTime, log));
            eventLog.AppendLine(string.Format("{0}: {1}", currTime.ToShortTimeString(), log));
            
            // семафорим событие
            if (eventLog.Length == 0 || RobotsLogEvent == null) return;
            RobotsLogEvent(eventLog.ToString());
        }

        //private string GenerateRobotsLogFileName()
        //{
        //    var dirName = ExecutablePath.ExecPath + RobotLogsCacheFolder;
        //    if (!Directory.Exists(dirName))
        //    {
        //        try
        //        {
        //            Directory.CreateDirectory(dirName);
        //        }
        //        catch
        //        {
        //            //MessageBox.Show(
        //            //    string.Format("Ошибка: невозможно создать каталог роботов ({0})",
        //            //                  dirName), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return null;
        //        }
        //    }

        //    return string.Empty;
        //}
    }
}
