using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.BacktestServerProxy
{
    public class BacktestTickerCursor
    {
        private readonly List<BacktestTickerCursorStream> streams =
            new List<BacktestTickerCursorStream>();
        
        public bool SetupCursor(string quoteDirectory,
            List<string> tickers, DateTime timeStart)
        {
            var hasStream = false;
            foreach (var ticker in tickers)
            {
                streams.Add(new BacktestTickerCursorStream(quoteDirectory, ticker));
                hasStream = true;
            }
            // установить позицию в каждом потоке
            SetupStreamPosition(timeStart);
            return hasStream;
        }

        private void SetupStreamPosition(DateTime timeStart)
        {
            CandleData quote;
            foreach (var stream in streams)
                stream.MoveToTime(timeStart, out quote);
        }

        public bool MoveNext()
        {
            var timeList = new List<DateTime>();
            var streamsToMove = new List<int>();
            DateTime? nearestTime = null;
            var streamIndex = -1;
            foreach (var stream in streams)
            {
                streamIndex++;
                var nextTime = stream.GetNextTime();
                if (!nextTime.HasValue) continue;

                timeList.Add(nextTime.Value);
                if (!nearestTime.HasValue)
                {
                    nearestTime = nextTime.Value;
                    streamsToMove.Add(streamIndex);
                    continue;
                }

                if (nearestTime.Value < nextTime) continue;
                if (nextTime < nearestTime.Value) streamsToMove.Clear();
                streamsToMove.Add(streamIndex);
            }

            if (streamsToMove.Count == 0) 
                return false;
            foreach (var ind in streamsToMove)
            {
                streams[ind].MoveNext();
            }
            return true;
        }

        public Dictionary<string, CandleData> MoveToTime(DateTime time)
        {
            var quotes = new Dictionary<string, CandleData>();
            foreach (var stream in streams)
            {
                CandleData quote;
                if (!stream.MoveToTime(time, out quote)) continue;
                quotes.Add(stream.Ticker, quote);
            }
            return quotes;
        }

        public List<Cortege2<string, CandleData>> GetCurrentQuotes()
        {
            return (from stream in streams
                    let quote = stream.GetCurQuote()
                    where quote != null
                    select new Cortege2<string, CandleData>(stream.Ticker, quote)).ToList();
        }

        public void Close()
        {
            foreach (var stream in streams)
            {
                try
                {
                    stream.Close();
                }
                catch
                {                    
                }
            }
        }
    }    
}
