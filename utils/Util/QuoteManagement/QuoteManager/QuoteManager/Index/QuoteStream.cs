using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entity;
using MTS.Live.Contract.Entity;
using MTS.Live.Util;

namespace QuoteManager.Index
{
    public class BacktestTickerCursor
    {
        private readonly List<BacktestTickerCursorStream> streams =
            new List<BacktestTickerCursorStream>();
        /// <summary>
        /// тикер - флаг нового формата (true) или старого (false)
        /// </summary>
        private readonly Dictionary<string, bool> formats = new Dictionary<string, bool>();

        public bool SetupCursor(string quoteDirectory,
            List<string> tickers, DateTime timeStart)
        {
            quoteDirectory = quoteDirectory.TrimEnd('\\');
            foreach (var ticker in tickers)
            {
                var fileName = string.Format("{0}\\{1}.quote", quoteDirectory, ticker);
                if (!File.Exists(fileName)) continue;
                streams.Add(new BacktestTickerCursorStream(fileName, ticker));
            }
            // установить позицию в каждом потоке
            SetupStreamPosition(timeStart);
            return true;
        }

        private void SetupStreamPosition(DateTime timeStart)
        {
            QuoteData quote;
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

        public Dictionary<string, QuoteData> MoveToTime(DateTime time)
        {
            var quotes = new Dictionary<string, QuoteData>();
            foreach (var stream in streams)
            {
                QuoteData quote;
                if (!stream.MoveToTime(time, out quote)) continue;
                quotes.Add(stream.Ticker, quote);
            }
            return quotes;
        }

        public List<Cortege2<string, QuoteData>> GetCurrentQuotes()
        {
            return (from stream in streams
                    let quote = stream.GetCurQuote()
                    where quote != null
                    select new Cortege2<string, QuoteData>(stream.Ticker, quote)).ToList();
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

    class BacktestTickerCursorStream
    {
        public string Ticker { get; private set; }
        private StreamReader reader;
        private bool isNewFormat;
        private const int BufferSize = 50;
        private List<QuoteData> buffer = new List<QuoteData>();
        private int bufferPosition = -1;
        private DateTime curDate = new DateTime();
        private bool readToEnd;

        public BacktestTickerCursorStream(string fileName, string ticker)
        {
            Ticker = ticker;
            reader = new StreamReader(fileName);
        }

        public void Close()
        {
            if (reader != null) reader.Close();
        }

        public bool MoveToTime(DateTime time, out QuoteData quote)
        {
            quote = null;
            if (reader == null) return false;
            // читать котировки из файла до даты "time"
            while (true)
            {
                var nextQuote = ReadNextQuoteFromStream();
                if (nextQuote == null)
                {// дошли до конца файла
                    reader = null;
                    return false;
                }

                if (nextQuote.time < time) continue;
                quote = nextQuote;
                buffer.Add(quote);
                bufferPosition = 0;

                // зачитать N строк в буфер
                FillBuffer();

                return true;
            }
        }

        public QuoteData GetCurQuote()
        {
            if (bufferPosition < 0 || bufferPosition >= buffer.Count) return null;
            return buffer[bufferPosition];
        }

        public DateTime? GetNextTime()
        {
            if (bufferPosition < 0) return null;
            if (bufferPosition >= buffer.Count - 1) return null;
            var time = buffer[bufferPosition + 1].time;

            if (bufferPosition == buffer.Count - 2 && !readToEnd)
            {// перезаполнить буфер
                buffer = buffer.GetRange(buffer.Count - 2, 2);
                FillBuffer();
                bufferPosition = 0;
            }

            return time;
        }

        public void MoveNext()
        {
            bufferPosition++;
        }

        private void FillBuffer()
        {
            var quoteRead = false;
            while (buffer.Count < BufferSize)
            {
                var bufQuote = ReadNextQuoteFromStream();
                if (bufQuote == null)
                {
                    reader = null;
                    break;
                }
                quoteRead = true;
                buffer.Add(bufQuote);
            }
            if (!quoteRead) readToEnd = true;
        }

        private QuoteData ReadNextQuoteFromStream()
        {
            if (reader == null) return null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.Length == 8)
                {
                    var year = line.Substring(4).ToIntSafe();
                    var month = line.Substring(2, 2).ToIntSafe();
                    var day = line.Substring(0, 2).ToIntSafe();
                    if (!year.HasValue || !month.HasValue || !day.HasValue)
                        continue;
                    isNewFormat = true;
                    curDate = new DateTime(year.Value, month.Value, day.Value);
                    continue;
                }

                // распарсить строку
                var quote = isNewFormat
                                ? QuoteData.ParseQuoteStringNewFormat(line, curDate)
                                : QuoteData.ParseQuoteStringOldFormat(line);
                return quote;
            }
            return null;
        }
    }
}
