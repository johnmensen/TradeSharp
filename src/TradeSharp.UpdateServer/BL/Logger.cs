using System;
using log4net;
using log4net.Config;

namespace TradeSharp.UpdateServer.BL
{
    public static class Logger
    {
        #region Private
        private static readonly ILog log;
        static Logger()
        {
            //log = LogManager.GetLogger("TradeSharp");
            XmlConfigurator.Configure();
            log = LogManager.GetLogger("RollingFile");
        }
        #endregion

        #region Debug
        public static void Debug(string msg)
        {
            log.Debug(msg);
        }
        public static void Debug(string msg, Exception ex)
        {
            log.Debug(msg, ex);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }
        #endregion

        #region Error
        public static void Error(string msg)
        {
            log.Error(msg);
        }
        public static void Error(string msg, Exception ex)
        {
            log.Error(msg, ex);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }
        #endregion

        #region Fatal
        public static void Fatal(string msg)
        {
            log.Fatal(msg);
        }
        public static void Fatal(string msg, Exception ex)
        {
            log.Fatal(msg, ex);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
        #endregion

        #region Info
        public static void Info(string msg)
        {
            log.Info(msg);
        }
        public static void Info(string msg, Exception ex)
        {
            log.Info(msg, ex);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }
        #endregion

        public static void Log(LogEntryType entryType, string msg)
        {
            if (entryType == LogEntryType.Info)
                log.Info(msg);
            else if (entryType == LogEntryType.Fatal)
                log.Fatal(msg);
            else if (entryType == LogEntryType.Error)
                log.Error(msg);
            else if (entryType == LogEntryType.Debug)
                log.Debug(msg);
        }

        public static void Log(LogEntryType entryType, string msg, Exception ex)
        {
            if (entryType == LogEntryType.Info)
                log.Info(msg, ex);
            else if (entryType == LogEntryType.Fatal)
                log.Fatal(msg, ex);
            else if (entryType == LogEntryType.Error)
                log.Error(msg, ex);
            else if (entryType == LogEntryType.Debug)
                log.Debug(msg, ex);
        }

        public static void Log(LogEntryType entryType, string format, params object[] args)
        {
            if (entryType == LogEntryType.Info)
                log.InfoFormat(format, args);
            else if (entryType == LogEntryType.Fatal)
                log.FatalFormat(format, args);
            else if (entryType == LogEntryType.Error)
                log.ErrorFormat(format, args);
            else if (entryType == LogEntryType.Debug)
                log.DebugFormat(format, args);
        }
    }

    public enum LogEntryType
    {
        Debug, Error, Fatal, Info
    }
}
