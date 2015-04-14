using System;
using System.Collections.Generic;

namespace TradeSharp.Util
{
    public class FloodSafeLogger
    {
        private readonly int minMillsBetween = 2000;
        
        private readonly Dictionary<int, DateTime> logFloodTimes = new Dictionary<int, DateTime>();
        
        public FloodSafeLogger(int minMills)
        {
            minMillsBetween = minMills;
        }

        
        /// <summary>
        /// по одному magic сообщения отправляются не чаще чем в minMillsBetween
        /// </summary>        
        public void LogMessageFormatCheckFlood(LogEntryType entryType, int msgMagic, int minMills,
            string fmt, params object[] ptrs)
        {
            DateTime time;
            if (!logFloodTimes.TryGetValue(msgMagic, out time))
                logFloodTimes.Add(msgMagic, DateTime.Now.AddMilliseconds(minMills));
            else
            {            
                if (DateTime.Now < time) return;
                try
                {
                    logFloodTimes[msgMagic] = DateTime.Now.AddMilliseconds(minMills);
                }
                catch
                {                    
                }
                
            }
            Logger.Log(entryType, fmt, ptrs);
        }

        /// <summary>
        /// по одному magic сообщения отправляются не чаще чем в minMillsBetween
        /// </summary>        
        public void LogMessageFormatCheckFlood(LogEntryType entryType, int msgMagic, 
            string fmt, params object[] ptrs)
        {
            DateTime time;
            if (!logFloodTimes.TryGetValue(msgMagic, out time))
                logFloodTimes.Add(msgMagic, DateTime.Now.AddMilliseconds(minMillsBetween));            
            else
            {
                if (DateTime.Now < time) return;
                try
                {
                    logFloodTimes[msgMagic] = DateTime.Now.AddMilliseconds(minMillsBetween);
                }
                catch
                {
                }
                
            }
            Logger.Log(entryType, fmt, ptrs);
        }        

    }
}
