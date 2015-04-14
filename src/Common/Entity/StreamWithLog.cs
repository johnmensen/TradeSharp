using System.Diagnostics;
using System.IO;
using System.Text;
using TradeSharp.Util;
using System.Linq;

namespace Entity
{
    public class StreamReaderLog : StreamReader
    {
        private static bool? showStackTrace;
        public static bool ShowStackTrace
        {
            get { return showStackTrace ?? (bool)(showStackTrace = AppConfig.GetBooleanParam("Debug.ShowStreamStack", false)); }
        }
        private static bool? loggingIsEnabled;
        public static bool LoggingIsEnabled
        {
            get { return loggingIsEnabled ?? (bool)(loggingIsEnabled = AppConfig.GetBooleanParam("Debug.ShowStreamLog", false)); }
        }
        
        private string filePath;

        private void ShowLog(string path, bool isEnter)
        {
            if (isEnter) filePath = path;
            var sign = isEnter ? ">" : "<";
            if (ShowStackTrace)
                Logger.InfoFormat("{0}StreamReaderLog(\"{1}\"). Stack: {2}",
                    sign, path, string.Join(";", new StackTrace().GetFrames().Cast<object>()));
            else
                Logger.InfoFormat("{0}StreamReaderLog(\"{1}\")", sign, path);
        }

        public StreamReaderLog(string path) : base (path)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamReaderLog(string path, Encoding encoding) : base(path, encoding)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamReaderLog(Stream stream, Encoding encoding) : base(stream, encoding)
        {
            if (LoggingIsEnabled) ShowLog("stream", true);
        }
        public StreamReaderLog(string path, bool detectEncoding) : base (path, detectEncoding)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamReaderLog(Stream stream, bool detectEncoding) : base(stream, detectEncoding)
        {
            if (LoggingIsEnabled) ShowLog("path", true);
        }

        protected override void Dispose(bool disposing)
        {
            if (LoggingIsEnabled) ShowLog(filePath, false);
            base.Dispose(disposing);
        }
    }

    public class StreamWriterLog : StreamWriter
    {
        private static bool? showStackTrace;
        public static bool ShowStackTrace
        {
            get { return showStackTrace ?? (bool)(showStackTrace = AppConfig.GetBooleanParam("Debug.ShowStreamStack", false)); }
        }
        private static bool? loggingIsEnabled;
        public static bool LoggingIsEnabled
        {
            get { return loggingIsEnabled ?? (bool)(loggingIsEnabled = AppConfig.GetBooleanParam("Debug.ShowStreamLog", false)); }
        }

        private string filePath;

        private void ShowLog(string path, bool isEnter)
        {
            if (isEnter) filePath = path;
            var sign = isEnter ? ">" : "<";
            if (ShowStackTrace)
                Logger.InfoFormat("{0}StreamWriterLog(\"{1}\"). Stack: {2}",
                    sign, path, string.Join(";", new StackTrace().GetFrames().Cast<object>()));
            else
                Logger.InfoFormat("{0}StreamWriterLog(\"{1}\")", sign, path);
        }
        
        public StreamWriterLog(string path) : base (path)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamWriterLog(string path, bool append)
            : base(path, append)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamWriterLog(string path, bool append, Encoding encoding)
            : base(path, append, encoding)
        {
            if (LoggingIsEnabled) ShowLog(path, true);
        }
        public StreamWriterLog(Stream stream)
            : base(stream)
        {
            if (LoggingIsEnabled) ShowLog("stream", true);
        }
        public StreamWriterLog(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
            if (LoggingIsEnabled) ShowLog("stream", true);
        }

        protected override void Dispose(bool disposing)
        {
            if (LoggingIsEnabled) ShowLog(filePath, false);
            base.Dispose(disposing);
        }
    }
}
