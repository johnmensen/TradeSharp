using System;
using System.Globalization;
using System.IO;
using Entity;

namespace QuoteManager.BL
{
    class QuoteSaver
    {
        private readonly string fmtString;
        private readonly CultureInfo ci = CultureInfo.InvariantCulture;
        private DateTime? lastTime;

        public QuoteSaver(string tickerName)
        {
            var precision = DalSpot.Instance.GetPrecision(tickerName);
            fmtString = "{0:HHmm} {1:f" + precision + "} {2:f" + precision + "}";
        }

        public QuoteSaver(int precision)
        {
            fmtString = "{0:HHmm} {1:f" + precision + "} {2:f" + precision + "}";
        }
        
        public void SaveQuote(
            StreamWriter sw, float bid, float ask, DateTime time)
        {
            var needWriteTime = true;
            if (!lastTime.HasValue)
                lastTime = time.Date;
            else
            {
                if (lastTime.Value.Date != time.Date) lastTime = time.Date;
                else needWriteTime = false;
            }

            if (needWriteTime)
                sw.WriteLine(string.Format("{0:ddMMyyyy}", time));                                    
            sw.WriteLine(string.Format(ci, fmtString, time, bid, ask));            
        }
    }
}
