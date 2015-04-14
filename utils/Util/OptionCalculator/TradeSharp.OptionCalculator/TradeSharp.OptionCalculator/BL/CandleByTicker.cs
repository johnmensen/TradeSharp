using System;
using System.Collections.Generic;
using System.Linq;
using Entity;

namespace TradeSharp.OptionCalculator.BL
{
    public class CandleByTicker
    {
        public string Ticker { get; set; }

        public int Timeframe { get; set; }

        public List<CandleData> candles = new List<CandleData>();

        public DateTime StartTime { get; set; }

        public CandleByTicker()
        {
        }

        public CandleByTicker(string ticker, int timeframe)
        {
            Ticker = ticker;
            Timeframe = timeframe;
        }

        public CandleByTicker(CandleByTicker history)
        {            
            Ticker = history.Ticker;
            Timeframe = history.Timeframe;
            candles.AddRange(history.candles.Select(c => new CandleData(c)));
            StartTime = history.StartTime;
        }
    }
}
