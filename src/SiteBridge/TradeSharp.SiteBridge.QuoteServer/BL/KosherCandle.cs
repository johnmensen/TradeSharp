using System;
using Entity;

namespace TradeSharp.SiteBridge.QuoteServer.BL
{
    [Serializable]
    public class KosherCandle
    {
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }

        private DateTime timeOpen;
        public DateTime TimeOpen
        {
            get { return timeOpen; }
            set { timeOpen = value; }
        }
        
        public KosherCandle()
        {            
        }

        public KosherCandle(CandleData candle)
        {
            TimeOpen = candle.timeOpen;
            Open = candle.open;
            Close = candle.close;
            High = candle.high;
            Low = candle.low;
        }
    }
}
