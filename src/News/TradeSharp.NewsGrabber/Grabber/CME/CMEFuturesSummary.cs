using System;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMEFuturesSummary
    {
        public int VolumeRTH { get; set; }
        public int VolumeGlobex { get; set; }
        public int OI { get; set; }
        public CMETickerInfo Ticker { get; set; }
        public DateTime DocumentDate { get; set; }

        public bool IsValid
        {
            get { return Ticker != null && DocumentDate != default(DateTime) && OI != 0; }
        }
    }
}
