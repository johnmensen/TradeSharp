using System;

namespace TradeSharp.Util
{
    public class TimeLogger : IDisposable
    {
        private readonly string message;
        private readonly DateTime time;

        public TimeLogger(string message)
        {
            this.message = message;
            time = DateTime.Now;
        }

        public void Dispose()
        {
            var delta = DateTime.Now - time;
            Logger.InfoFormat("{0}: [{1}m {2}s {3}ms]",
                message,
                (int)delta.TotalMinutes, delta.Seconds, delta.Milliseconds);
        }
    }
}
