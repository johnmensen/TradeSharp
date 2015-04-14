using System;
using System.Collections.Generic;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Robot.BacktestServerProxy
{
    // Функции работы с логом роботов
    public partial class RobotContextBacktest
    {
        public readonly List<RobotLogEntry> robotLogEntries = new List<RobotLogEntry>();

        private void LogRobotMessages(BaseRobot robot, DateTime time, List<string> messages)
        {
            robotLogEntries.Add(new RobotLogEntry(robot, time, messages));
        }
    }

    public class RobotLogEntry
    {
        public BaseRobot Robot { get; set; }
        public DateTime Time { get; set; }
        public List<string> Messages { get; set; }

        public RobotLogEntry(BaseRobot robot, DateTime time, List<string> messages)
        {
            Robot = robot;
            Time = time;
            Messages = messages;
        }
    }
}